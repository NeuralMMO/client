using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

[assembly: InternalsVisibleTo("Unity.Entities.Hybrid")]
[assembly: InternalsVisibleTo("Unity.Tiny.Core")]
[assembly: InternalsVisibleTo("Unity.DOTS.Editor")]

namespace Unity.Entities
{
    // Exists to allow `EntityManager mgr = null` to compile, as it required by existing packages (Physics)
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct EntityManagerNullShim
    {
    }

    /// <summary>
    /// The EntityManager manages entities and components in a World.
    /// </summary>
    /// <remarks>
    /// The EntityManager provides an API to create, read, update, and destroy entities.
    ///
    /// A <see cref="World"/> has one EntityManager, which manages all the entities for that World.
    ///
    /// Many EntityManager operations result in *structural changes* that change the layout of entities in memory.
    /// Before it can perform such operations, the EntityManager must wait for all running Jobs to complete, an event
    /// called a *sync point*. A sync point both blocks the main thread and prevents the application from taking
    /// advantage of all available cores as the running Jobs wind down.
    ///
    /// Although you cannot prevent sync points entirely, you should avoid them as much as possible. To this end, the ECS
    /// framework provides the <see cref="EntityCommandBuffer"/>, which allows you to queue structural changes so that
    /// they all occur at one time in the frame.
    /// </remarks>
    [Preserve]
    [NativeContainer]
    [DebuggerTypeProxy(typeof(EntityManagerDebugView))]
    public unsafe partial struct EntityManager : IEquatable<EntityManager>
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        private bool m_JobMode;
#endif

        [NativeDisableUnsafePtrRestriction]
        private EntityDataAccess* m_EntityDataAccess;

        // This is extremely unfortunate but needed because of the IsCreated API
        private GCHandle m_AliveHandle;

        internal EntityDataAccess* GetCheckedEntityDataAccess()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            if (m_JobMode != m_EntityDataAccess->m_JobMode)
            {
                throw new InvalidOperationException($"EntityManager cannot be used from this context job mode {m_JobMode} != current mode {m_EntityDataAccess->m_JobMode}");
            }
#endif
            return m_EntityDataAccess;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal bool IsInsideForEach => GetCheckedEntityDataAccess()->m_InsideForEach != 0;

        internal struct InsideForEach : IDisposable
        {
            EntityManager m_Manager;
            int m_InsideForEachSafety;

            public InsideForEach(EntityManager manager)
            {
                m_Manager = manager;
                EntityDataAccess* g = manager.GetCheckedEntityDataAccess();
                m_InsideForEachSafety = g->m_InsideForEach++;
            }

            public void Dispose()
            {
                EntityDataAccess* g = m_Manager.GetCheckedEntityDataAccess();
                int newValue = --g->m_InsideForEach;
                if (m_InsideForEachSafety != newValue)
                {
                    throw new InvalidOperationException("for each unbalanced");
                }
            }
        }
#endif

        // Attribute to indicate an EntityManager method makes structural changes.
        // Do not remove form EntityManager and please apply to all appropriate methods.
        [AttributeUsage(AttributeTargets.Method)]
        private class StructuralChangeMethodAttribute : Attribute
        {
        }

        /// <summary>
        /// The <see cref="World"/> of this EntityManager.
        /// </summary>
        /// <value>A World has one EntityManager and an EntityManager manages the entities of one World.</value>
        public World World => GetCheckedEntityDataAccess()->ManagedEntityDataAccess.m_World;

        /// <summary>
        /// The latest entity generational version.
        /// </summary>
        /// <value>This is the version number that is assigned to a new entity. See <see cref="Entity.Version"/>.</value>
        public int Version => IsCreated? GetCheckedEntityDataAccess()->EntityComponentStore->EntityOrderVersion : 0;

        /// <summary>
        /// A counter that increments after every system update.
        /// </summary>
        /// <remarks>
        /// The ECS framework uses the GlobalSystemVersion to track changes in a conservative, efficient fashion.
        /// Changes are recorded per component per chunk.
        /// </remarks>
        /// <seealso cref="ArchetypeChunk.DidChange"/>
        /// <seealso cref="ChangedFilterAttribute"/>
        public uint GlobalSystemVersion => IsCreated? GetCheckedEntityDataAccess()->EntityComponentStore->GlobalSystemVersion : 0;

        /// <summary>
        /// Reports whether the EntityManager has been initialized yet.
        /// </summary>
        /// <value>True, if the EntityManager's OnCreateManager() function has finished.</value>
        public bool IsCreated => m_AliveHandle.IsAllocated && m_AliveHandle.Target != null;

        /// <summary>
        /// The capacity of the internal entities array.
        /// </summary>
        /// <value>The number of entities the array can hold before it must be resized.</value>
        /// <remarks>
        /// The entities array automatically resizes itself when the entity count approaches the capacity.
        /// You should rarely need to set this value directly.
        ///
        /// **Important:** when you set this value (or when the array automatically resizes), the EntityManager
        /// first ensures that all Jobs finish. This can prevent the Job scheduler from utilizing available CPU
        /// cores and threads, resulting in a temporary performance drop.
        /// </remarks>
        public int EntityCapacity => GetCheckedEntityDataAccess()->EntityComponentStore->EntitiesCapacity;

        /// <summary>
        /// A EntityQuery instance that matches all components.
        /// </summary>
        public EntityQuery UniversalQuery => GetCheckedEntityDataAccess()->ManagedEntityDataAccess.m_UniversalQuery;

        /// <summary>
        /// An object providing debugging information and operations.
        /// </summary>
        public EntityManagerDebug Debug
        {
            get
            {
                var guts = GetCheckedEntityDataAccess()->ManagedEntityDataAccess;
                if (guts.m_Debug == null)
                    guts.m_Debug = new EntityManagerDebug(this);
                return guts.m_Debug;
            }
        }

        internal void Initialize(World world, GCHandle boxedAliveBool)
        {
            TypeManager.Initialize();
            StructuralChange.Initialize();
            EntityCommandBuffer.Initialize();

            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = AtomicSafetyHandle.Create();
            m_JobMode = false;
#endif

            m_AliveHandle = boxedAliveBool;

            m_EntityDataAccess = (EntityDataAccess*)UnsafeUtility.Malloc(sizeof(EntityDataAccess), 16, Allocator.Persistent);
            UnsafeUtility.MemClear(m_EntityDataAccess, sizeof(EntityDataAccess));
            EntityDataAccess.Initialize(m_EntityDataAccess, world);
        }

        internal void PreDisposeCheck()
        {
            EndExclusiveEntityTransaction();
            GetCheckedEntityDataAccess()->DependencyManager->PreDisposeCheck();
        }

        internal void DestroyInstance()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif

            PreDisposeCheck();

            GetCheckedEntityDataAccess()->Dispose();
            UnsafeUtility.Free(m_EntityDataAccess, Allocator.Persistent);
            m_EntityDataAccess = null;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(m_Safety);
            m_Safety = default;
#endif
        }

        internal static EntityManager CreateEntityManagerInUninitializedState()
        {
            return new EntityManager();
        }

        public bool Equals(EntityManager other)
        {
            return m_EntityDataAccess == other.m_EntityDataAccess;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityManager other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked((int)(long)m_EntityDataAccess);
        }

        public static bool operator==(EntityManager lhs, EntityManager rhs)
        {
            return lhs.m_EntityDataAccess == rhs.m_EntityDataAccess;
        }

        public static bool operator!=(EntityManager lhs, EntityManager rhs)
        {
            return lhs.m_EntityDataAccess != rhs.m_EntityDataAccess;
        }

        // Temporarily allow conversion from null reference to allow existing packages to compile.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("EntityManager is a struct. Please use `default` instead of `null`. (RemovedAfter 2020-07-01)")]
        public static implicit operator EntityManager(EntityManagerNullShim? shim) => default(EntityManager);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This is slow. Use The EntityDataAccess directly in new code.")]
        internal EntityComponentStore* EntityComponentStore => GetCheckedEntityDataAccess()->EntityComponentStore;

        #if ENABLE_UNITY_COLLECTIONS_CHECKS
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This is slow. Use The EntityDataAccess directly in new code.")]
        internal ComponentSafetyHandles* SafetyHandles => &GetCheckedEntityDataAccess()->DependencyManager->Safety;
        #endif
    }
}

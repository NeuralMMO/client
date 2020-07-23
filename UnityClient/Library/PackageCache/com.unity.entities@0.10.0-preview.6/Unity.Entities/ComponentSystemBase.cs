using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Core;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Entities
{
    public unsafe struct SystemState
    {
        internal int m_UnmanagedMetaIndex;
        internal void* m_SystemPtr;

        private UnsafeList m_EntityQueries;
        private UnsafeList m_RequiredEntityQueries;

        internal ref UnsafeList<EntityQuery> EntityQueries
        {
            get
            {
                fixed(void* ptr = &m_EntityQueries)
                {
                    return ref UnsafeUtilityEx.AsRef<UnsafeList<EntityQuery>>(ptr);
                }
            }
        }

        internal ref UnsafeList<EntityQuery> RequiredEntityQueries
        {
            get
            {
                fixed(void* ptr = &m_RequiredEntityQueries)
                {
                    return ref UnsafeUtilityEx.AsRef<UnsafeList<EntityQuery>>(ptr);
                }
            }
        }

        internal UnsafeIntList m_JobDependencyForReadingSystems;
        internal UnsafeIntList m_JobDependencyForWritingSystems;

        internal uint m_LastSystemVersion;

        internal EntityManager m_EntityManager;
        internal EntityComponentStore* m_EntityComponentStore;
        internal ComponentDependencyManager* m_DependencyManager;
        internal GCHandle m_World;

        internal bool m_AlwaysUpdateSystem;
        internal bool m_Enabled;
        internal bool m_PreviouslyEnabled;

        // SystemBase stuff
        internal bool      m_GetDependencyFromSafetyManager;
        internal JobHandle m_JobHandle;

    #if ENABLE_PROFILER
        internal Profiling.ProfilerMarker m_ProfilerMarker;
    #endif

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal int                        m_SystemID;
#endif

        public bool Enabled { get => m_Enabled; set => m_Enabled = value; }
        public uint GlobalSystemVersion => m_EntityComponentStore->GlobalSystemVersion;
        public uint LastSystemVersion => m_LastSystemVersion;
        public EntityManager EntityManager => m_EntityManager;

        public World World => (World)m_World.Target;
        public ref readonly TimeData Time => ref World.Time;

        internal static SystemState* Allocate()
        {
            void* result = UnsafeUtility.Malloc(sizeof(SystemState), 16, Allocator.Persistent);
            UnsafeUtility.MemClear(result, sizeof(SystemState));
            return (SystemState*)result;
        }

        internal static void Deallocate(SystemState* state)
        {
            UnsafeUtility.Free(state, Allocator.Persistent);
        }

        // Apply changes on top of zero-initialized heap allocation
        internal void Init(World world, Type managedType)
        {
            m_Enabled = true;
            m_UnmanagedMetaIndex = -1;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_SystemID = World.AllocateSystemID();
#endif
            m_World = GCHandle.Alloc(world);
            m_EntityManager = world.EntityManager;
            m_EntityComponentStore = m_EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore;
            m_DependencyManager = m_EntityManager.GetCheckedEntityDataAccess()->DependencyManager;

            EntityQueries = new UnsafeList<EntityQuery>(0, Allocator.Persistent);
            RequiredEntityQueries = new UnsafeList<EntityQuery>(0, Allocator.Persistent);

            m_JobDependencyForReadingSystems = new UnsafeIntList(0, Allocator.Persistent);
            m_JobDependencyForWritingSystems = new UnsafeIntList(0, Allocator.Persistent);

            m_AlwaysUpdateSystem = false;

            if (managedType != null)
            {
 #if !NET_DOTS
                m_AlwaysUpdateSystem = Attribute.IsDefined(managedType, typeof(AlwaysUpdateSystemAttribute), true);
#else
                var attrs = TypeManager.GetSystemAttributes(managedType, typeof(AlwaysUpdateSystemAttribute));
                if (attrs.Length > 0)
                    m_AlwaysUpdateSystem = true;
#endif
            }
        }

        internal void Dispose()
        {
            DisposeQueries(ref EntityQueries);
            DisposeQueries(ref RequiredEntityQueries);

            EntityQueries.Dispose();
            EntityQueries = default;

            RequiredEntityQueries.Dispose();
            RequiredEntityQueries = default;

            if (m_World.IsAllocated)
            {
                m_World.Free();
            }

            m_JobDependencyForReadingSystems.Dispose();
            m_JobDependencyForWritingSystems.Dispose();
        }

        private void DisposeQueries(ref UnsafeList<EntityQuery> queries)
        {
            for (var i = 0; i < queries.Length; ++i)
            {
                var query = queries[i];

                if (m_EntityManager.IsQueryValid(query))
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    query._GetImpl()->_DisallowDisposing = false;
#endif
                    query.Dispose();
                }
            }
        }

        public JobHandle Dependency
        {
            get
            {
                if (m_GetDependencyFromSafetyManager)
                {
                    var depMgr = m_DependencyManager;
                    m_GetDependencyFromSafetyManager = false;
                    m_JobHandle = depMgr->GetDependency(m_JobDependencyForReadingSystems.Ptr,
                        m_JobDependencyForReadingSystems.Length, m_JobDependencyForWritingSystems.Ptr,
                        m_JobDependencyForWritingSystems.Length);
                }

                return m_JobHandle;
            }
            set
            {
                m_GetDependencyFromSafetyManager = false;
                m_JobHandle = value;
            }
        }

        public void CompleteDependency()
        {
            // Previous frame job
            m_JobHandle.Complete();

            // We need to get more job handles from other systems
            if (m_GetDependencyFromSafetyManager)
            {
                m_GetDependencyFromSafetyManager = false;
                CompleteDependencyInternal();
            }
        }

        internal void CompleteDependencyInternal()
        {
            m_DependencyManager->CompleteDependenciesNoChecks(m_JobDependencyForReadingSystems.Ptr,
                m_JobDependencyForReadingSystems.Length, m_JobDependencyForWritingSystems.Ptr,
                m_JobDependencyForWritingSystems.Length);
        }

        internal void BeforeUpdateVersioning()
        {
            m_EntityComponentStore->IncrementGlobalSystemVersion();
            ref var qs = ref EntityQueries;
            for (int i = 0; i < qs.Length; ++i)
            {
                qs[i].SetChangedFilterRequiredVersion(m_LastSystemVersion);
            }
        }

        internal void BeforeOnUpdate()
        {
            BeforeUpdateVersioning();

            // We need to wait on all previous frame dependencies, otherwise it is possible that we create infinitely long dependency chains
            // without anyone ever waiting on it
            m_JobHandle.Complete();
            m_GetDependencyFromSafetyManager = true;
        }

#pragma warning disable 649
        private unsafe struct JobHandleData
        {
            public void* jobGroup;
            public int version;
        }
#pragma warning restore 649

        internal void AfterOnUpdate()
        {
            AfterUpdateVersioning();

            var depMgr = m_DependencyManager;

            // If outputJob says no relevant jobs were scheduled,
            // then no need to batch them up or register them.
            // This is a big optimization if we only Run methods on main thread...
            var outputJob = m_JobHandle;
            if (((JobHandleData*)&outputJob)->jobGroup != null)
            {
                JobHandle.ScheduleBatchedJobs();
                m_JobHandle = depMgr->AddDependency(m_JobDependencyForReadingSystems.Ptr,
                    m_JobDependencyForReadingSystems.Length, m_JobDependencyForWritingSystems.Ptr,
                    m_JobDependencyForWritingSystems.Length, outputJob);
            }
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal void CheckSafety(ref SystemDependencySafetyUtility.SafetyErrorDetails details, ref bool errorFound)
        {
            var depMgr = m_DependencyManager;
            if (JobsUtility.JobDebuggerEnabled)
            {
                var dependencyError = SystemDependencySafetyUtility.CheckSafetyAfterUpdate(ref m_JobDependencyForReadingSystems, ref m_JobDependencyForWritingSystems, depMgr, out details);

                if (dependencyError)
                {
                    SystemDependencySafetyUtility.EmergencySyncAllJobs(ref m_JobDependencyForReadingSystems, ref m_JobDependencyForWritingSystems, depMgr);
                }

                errorFound = dependencyError;
            }
        }

#endif

        internal void AfterUpdateVersioning()
        {
            m_LastSystemVersion = m_EntityComponentStore->GlobalSystemVersion;
        }

        internal bool ShouldRunSystem()
        {
            if (m_AlwaysUpdateSystem)
                return true;

            ref var required = ref RequiredEntityQueries;

            if (required.Length > 0)
            {
                for (int i = 0; i != required.Length; i++)
                {
                    EntityQuery query = required[i];
                    if (query.IsEmptyIgnoreFilter)
                        return false;
                }

                return true;
            }
            else
            {
                // Systems without queriesDesc should always run. Specifically,
                // IJobForEach adds its queriesDesc the first time it's run.
                ref var eqs = ref EntityQueries;
                var length = eqs.Length;
                if (length == 0)
                    return true;

                // If all the queriesDesc are empty, skip it.
                // (There’s no way to know what the key value is without other markup)
                for (int i = 0; i != length; i++)
                {
                    EntityQuery query = eqs[i];
                    if (!query.IsEmptyIgnoreFilter)
                        return true;
                }

                return false;
            }
        }
    }

    /// <summary>
    /// A system provides behavior in an ECS architecture.
    /// </summary>
    /// <remarks>
    /// System implementations should inherit <see cref="SystemBase"/>, which is a subclass of ComponentSystemBase.
    /// </remarks>
    public abstract unsafe partial class ComponentSystemBase
    {
        internal SystemState* m_StatePtr;

        internal SystemState* CheckedState()
        {
            var state = m_StatePtr;
            if (state == null)
            {
                throw new InvalidOperationException("object is not initialized or has already been destroyed");
            }
            return state;
        }

        /// <summary>
        /// Controls whether this system executes when its OnUpdate function is called.
        /// </summary>
        /// <value>True, if the system is enabled.</value>
        /// <remarks>The Enabled property is intended for debugging so that you can easily turn on and off systems
        /// from the Entity Debugger window. A system with Enabled set to false will not update, even if its
        /// <see cref="ShouldRunSystem"/> function returns true.</remarks>
        public bool Enabled { get => CheckedState()->m_Enabled; set => CheckedState()->m_Enabled = value; }

        /// <summary>
        /// The query objects cached by this system.
        /// </summary>
        /// <remarks>A system caches any queries it implicitly creates through the IJob interfaces or
        /// <see cref="EntityQueryBuilder"/>, that you create explicitly by calling <see cref="GetEntityQuery"/>, or
        /// that you add to the system as a required query with <see cref="RequireForUpdate"/>.
        /// Implicit queries may be created lazily and not exist before a system has run for the first time.</remarks>
        /// <value>A read-only array of the cached <see cref="EntityQuery"/> objects.</value>
        public EntityQuery[] EntityQueries => UnsafeListToRefArray(ref CheckedState()->EntityQueries);

        internal static EntityQuery[] UnsafeListToRefArray(ref UnsafeList<EntityQuery> objs)
        {
            EntityQuery[] result = new EntityQuery[objs.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = objs.Ptr[i];
            }
            return result;
        }

        /// <summary>
        /// The current change version number in this <see cref="World"/>.
        /// </summary>
        /// <remarks>The system updates the component version numbers inside any <see cref="ArchetypeChunk"/> instances
        /// that this system accesses with write permissions to this value.</remarks>
        public uint GlobalSystemVersion => EntityManager.GlobalSystemVersion;

        /// <summary>
        /// The current version of this system.
        /// </summary>
        /// <remarks>
        /// LastSystemVersion is updated to match the <see cref="GlobalSystemVersion"/> whenever a system runs.
        ///
        /// When you use <seealso cref="EntityQuery.SetFilterChanged"/>
        /// or <seealso cref="ArchetypeChunk.DidChange"/>, LastSystemVersion provides the basis for determining
        /// whether a component could have changed since the last time the system ran.
        ///
        /// When a system accesses a component and has write permission, it updates the change version of that component
        /// type to the current value of LastSystemVersion. The system updates the component type's version whether or not
        /// it actually modifies data in any instances of the component type -- this is one reason why you should
        /// specify read-only access to components whenever possible.
        ///
        /// For efficiency, ECS tracks the change version of component types by chunks, not by individual entities. If a system
        /// updates the component of a given type for any entity in a chunk, then ECS assumes that the components of all
        /// entities in that chunk could have been changed. Change filtering allows you to save processing time by
        /// skipping all entities in an unchanged chunk, but does not support skipping individual entities in a chunk
        /// that does contain changes.
        /// </remarks>
        /// <value>The <see cref="GlobalSystemVersion"/> the last time this system ran.</value>
        public uint LastSystemVersion => CheckedState()->m_LastSystemVersion;

        /// <summary>
        /// The EntityManager object of the <see cref="World"/> in which this system exists.
        /// </summary>
        /// <value>The EntityManager for this system.</value>
        public EntityManager EntityManager => CheckedState()->m_EntityManager;

        /// <summary>
        /// The World in which this system exists.
        /// </summary>
        /// <value>The World of this system.</value>
        public World World => m_StatePtr != null ? (World)m_StatePtr->m_World.Target : null;

        /// <summary>
        /// The current Time data for this system's world.
        /// </summary>
        public ref readonly TimeData Time => ref World.Time;

        // ============


        internal void CreateInstance(World world)
        {
            m_StatePtr = SystemState.Allocate();
            m_StatePtr->Init(world, GetType());

            OnBeforeCreateInternal(world);
            try
            {
                OnCreateForCompiler();
                OnCreate();
        #if ENABLE_PROFILER
                m_StatePtr->m_ProfilerMarker = new Profiling.ProfilerMarker($"{world.Name} {TypeManager.GetSystemName(GetType())}");
        #endif
            }
            catch
            {
                OnBeforeDestroyInternal();
                OnAfterDestroyInternal();
                throw;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected internal virtual void OnCreateForCompiler()
        {
            //do not remove, dots compiler will emit methods that implement this method.
        }

        internal void DestroyInstance()
        {
            OnBeforeDestroyInternal();
            OnDestroy();
            OnAfterDestroyInternal();
        }

        /// <summary>
        /// Called when this system is created.
        /// </summary>
        /// <remarks>
        /// Implement an OnCreate() function to set up system resources when it is created.
        ///
        /// OnCreate is invoked before the the first time <see cref="OnStartRunning"/> and OnUpdate are invoked.
        /// </remarks>
        protected virtual void OnCreate()
        {
        }

        /// <summary>
        /// Called before the first call to OnUpdate and when a system resumes updating after being stopped or disabled.
        /// </summary>
        /// <remarks>If the <see cref="EntityQuery"/> objects defined for a system do not match any existing entities
        /// then the system skips updates until a successful match is found. Likewise, if you set <see cref="Enabled"/>
        /// to false, then the system stops running. In both cases, <see cref="OnStopRunning"/> is
        /// called when a running system stops updating; OnStartRunning is called when it starts updating again.
        /// </remarks>
        protected virtual void OnStartRunning()
        {
        }

        /// <summary>
        /// Called when this system stops running because no entities match the system's <see cref="EntityQuery"/>
        /// objects or because you change the system <see cref="Enabled"/> property to false.
        /// </summary>
        /// <remarks>If the <see cref="EntityQuery"/> objects defined for a system do not match any existing entities
        /// then the system skips updating until a successful match is found. Likewise, if you set <see cref="Enabled"/>
        /// to false, then the system stops running. In both cases, <see cref="OnStopRunning"/> is
        /// called when a running system stops updating; OnStartRunning is called when it starts updating again.
        /// </remarks>
        protected virtual void OnStopRunning()
        {
        }

        internal virtual void OnStopRunningInternal()
        {
            OnStopRunning();
        }

        /// <summary>
        /// Called when this system is destroyed.
        /// </summary>
        /// <remarks>Systems are destroyed when the application shuts down, the World is destroyed, or you
        /// call <see cref="World.DestroySystem"/>. In the Unity Editor, system destruction occurs when you exit
        /// Play Mode and when scripts are reloaded.</remarks>
        protected virtual void OnDestroy()
        {
        }

        internal void OnDestroy_Internal()
        {
            OnDestroy();
        }

        /// <summary>
        /// Executes the system immediately.
        /// </summary>
        /// <remarks>The exact behavior is determined by this system's specific subclass.</remarks>
        /// <seealso cref="SystemBase"/>
        /// <seealso cref="ComponentSystemGroup"/>
        /// <seealso cref="EntityCommandBufferSystem"/>
        abstract public void Update();

        // ===================

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal static ComponentSystemBase ms_ExecutingSystem;

        public static Type ExecutingSystemType => ms_ExecutingSystem?.GetType();

        internal ComponentSystemBase GetSystemFromSystemID(World world, int systemID)
        {
            foreach (var system in world.Systems)
            {
                if (system == null) continue;

                if (system.CheckedState()->m_SystemID == systemID)
                {
                    return system;
                }
            }

            return null;
        }

#endif

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckExists()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (m_StatePtr != null && World != null && World.IsCreated) return;

            throw new InvalidOperationException(
                $"System {GetType()} is invalid. This usually means it was not created with World.GetOrCreateSystem<{GetType()}>() or has already been destroyed.");
#endif
        }

        /// <summary>
        /// Reports whether any of this system's entity queries currently match any chunks. This function is used
        /// internally to determine whether the system's OnUpdate function can be skipped.
        /// </summary>
        /// <returns>True, if the queries in this system match existing entities or the system has the
        /// <see cref="AlwaysUpdateSystemAttribute"/>.</returns>
        /// <remarks>A system without any queries also returns true. Note that even if this function returns true,
        /// other factors may prevent a system from updating. For example, a system will not be updated if its
        /// <see cref="Enabled"/> property is false.</remarks>
        public bool ShouldRunSystem() => CheckedState()->ShouldRunSystem();

        internal virtual void OnBeforeCreateInternal(World world)
        {
        }

        internal void OnAfterDestroyInternal()
        {
            var state = CheckedState();

            state->Dispose();
            SystemState.Deallocate(state);
            m_StatePtr = null;
        }

        private static void DisposeQueries(ref UnsafeList<GCHandle> queries)
        {
            for (var i = 0; i < queries.Length; ++i)
            {
                var query = (EntityQuery)queries[i].Target;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                query._GetImpl()->_DisallowDisposing = false;
#endif
                query.Dispose();
                queries[i].Free();
            }
        }

        internal virtual void OnBeforeDestroyInternal()
        {
            var state = CheckedState();

            if (state->m_PreviouslyEnabled)
            {
                state->m_PreviouslyEnabled = false;
                OnStopRunning();
            }
        }

        internal void BeforeUpdateVersioning()
        {
            var state = CheckedState();
            state->m_EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            ref var qs = ref state->EntityQueries;
            for (int i = 0; i < qs.Length; ++i)
            {
                qs[i].SetChangedFilterRequiredVersion(state->m_LastSystemVersion);
            }
        }

        internal void AfterUpdateVersioning()
        {
            CheckedState()->m_LastSystemVersion = EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->GlobalSystemVersion;
        }

        internal void CompleteDependencyInternal()
        {
            CheckedState()->CompleteDependencyInternal();
        }

        /// <summary>
        /// Gets the run-time type information required to access an array of component data in a chunk.
        /// </summary>
        /// <param name="isReadOnly">Whether the component data is only read, not written. Access components as
        /// read-only whenever possible.</param>
        /// <typeparam name="T">A struct that implements <see cref="IComponentData"/>.</typeparam>
        /// <returns>An object representing the type information required to safely access component data stored in a
        /// chunk.</returns>
        /// <remarks>Pass an <see cref="ArchetypeChunkComponentType"/> instance to a job that has access to chunk data,
        /// such as an <see cref="IJobChunk"/> job, to access that type of component inside the job.</remarks>
        public ArchetypeChunkComponentType<T> GetArchetypeChunkComponentType<T>(bool isReadOnly = false) where T : struct, IComponentData
        {
            AddReaderWriter(isReadOnly ? ComponentType.ReadOnly<T>() : ComponentType.ReadWrite<T>());
            return EntityManager.GetArchetypeChunkComponentType<T>(isReadOnly);
        }

        /// <summary>
        /// Gets the run-time type information required to access an array of component data in a chunk.
        /// </summary>
        /// <param name="componentType">Type of the component</param>
        /// <returns>An object representing the type information required to safely access component data stored in a
        /// chunk.</returns>
        /// <remarks>Pass an ArchetypeChunkComponentTypeDynamic instance to a job that has access to chunk data, such as an
        /// <see cref="IJobChunk"/> job, to access that type of component inside the job.</remarks>
        public ArchetypeChunkComponentTypeDynamic GetArchetypeChunkComponentTypeDynamic(ComponentType componentType)
        {
            AddReaderWriter(componentType);
            return EntityManager.GetArchetypeChunkComponentTypeDynamic(componentType);
        }

        /// <summary>
        /// Gets the run-time type information required to access an array of buffer components in a chunk.
        /// </summary>
        /// <param name="isReadOnly">Whether the data is only read, not written. Access data as
        /// read-only whenever possible.</param>
        /// <typeparam name="T">A struct that implements <see cref="IBufferElementData"/>.</typeparam>
        /// <returns>An object representing the type information required to safely access buffer components stored in a
        /// chunk.</returns>
        /// <remarks>Pass a GetArchetypeChunkBufferType instance to a job that has access to chunk data, such as an
        /// <see cref="IJobChunk"/> job, to access that type of buffer component inside the job.</remarks>
        public ArchetypeChunkBufferType<T> GetArchetypeChunkBufferType<T>(bool isReadOnly = false)
            where T : struct, IBufferElementData
        {
            AddReaderWriter(isReadOnly ? ComponentType.ReadOnly<T>() : ComponentType.ReadWrite<T>());
            return EntityManager.GetArchetypeChunkBufferType<T>(isReadOnly);
        }

        /// <summary>
        /// Gets the run-time type information required to access a shared component data in a chunk.
        /// </summary>
        /// <typeparam name="T">A struct that implements <see cref="ISharedComponentData"/>.</typeparam>
        /// <returns>An object representing the type information required to safely access shared component data stored in a
        /// chunk.</returns>
        public ArchetypeChunkSharedComponentType<T> GetArchetypeChunkSharedComponentType<T>()
            where T : struct, ISharedComponentData
        {
            return EntityManager.GetArchetypeChunkSharedComponentType<T>();
        }

        /// <summary>
        /// Gets the run-time type information required to access the array of <see cref="Entity"/> objects in a chunk.
        /// </summary>
        /// <returns>An object representing the type information required to safely access Entity instances stored in a
        /// chunk.</returns>
        public ArchetypeChunkEntityType GetArchetypeChunkEntityType()
        {
            return EntityManager.GetArchetypeChunkEntityType();
        }

        /// <summary>
        /// Gets an dictionary-like container containing all components of type T, keyed by Entity.
        /// </summary>
        /// <param name="isReadOnly">Whether the data is only read, not written. Access data as
        /// read-only whenever possible.</param>
        /// <typeparam name="T">A struct that implements <see cref="IComponentData"/>.</typeparam>
        /// <returns>All component data of type T.</returns>
        public ComponentDataFromEntity<T> GetComponentDataFromEntity<T>(bool isReadOnly = false)
            where T : struct, IComponentData
        {
            AddReaderWriter(isReadOnly ? ComponentType.ReadOnly<T>() : ComponentType.ReadWrite<T>());
            return EntityManager.GetComponentDataFromEntity<T>(isReadOnly);
        }

        /// <summary>
        /// Gets a BufferFromEntity&lt;T&gt; object that can access a <seealso cref="DynamicBuffer{T}"/>.
        /// </summary>
        /// <remarks>Assign the returned object to a field of your Job struct so that you can access the
        /// contents of the buffer in a Job.</remarks>
        /// <param name="isReadOnly">Whether the buffer data is only read or is also written. Access data in
        /// a read-only fashion whenever possible.</param>
        /// <typeparam name="T">The type of <see cref="IBufferElementData"/> stored in the buffer.</typeparam>
        /// <returns>An array-like object that provides access to buffers, indexed by <see cref="Entity"/>.</returns>
        /// <seealso cref="ComponentDataFromEntity{T}"/>
        public BufferFromEntity<T> GetBufferFromEntity<T>(bool isReadOnly = false) where T : struct, IBufferElementData
        {
            AddReaderWriter(isReadOnly ? ComponentType.ReadOnly<T>() : ComponentType.ReadWrite<T>());
            return EntityManager.GetBufferFromEntity<T>(isReadOnly);
        }

        /// <summary>
        /// Adds a query that must return entities for the system to run. You can add multiple required queries to a
        /// system; all of them must match at least one entity for the system to run.
        /// </summary>
        /// <param name="query">A query that must match entities this frame in order for this system to run.</param>
        /// <remarks>Any queries added through RequireforUpdate override all other queries cached by this system.
        /// In other words, if any required query does not find matching entities, the update is skipped even
        /// if another query created for the system (either explicitly or implicitly) does match entities and
        /// vice versa.</remarks>
        public void RequireForUpdate(EntityQuery query)
        {
            var state = CheckedState();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (state->m_AlwaysUpdateSystem)
                throw new InvalidOperationException($"Cannot require {nameof(EntityQuery)} for update on a system with {nameof(AlwaysSynchronizeSystemAttribute)}");
#endif

            state->RequiredEntityQueries.Add(query);
        }

        /// <summary>
        /// Require that a specific singleton component exist for this system to run.
        /// </summary>
        /// <typeparam name="T">The <see cref="IComponentData"/> subtype of the singleton component.</typeparam>
        public void RequireSingletonForUpdate<T>()
        {
            var type = ComponentType.ReadOnly<T>();
            var query = GetSingletonEntityQueryInternal(type);
            RequireForUpdate(query);
        }

        /// <summary>
        /// Checks whether a singelton component of the specified type exists.
        /// </summary>
        /// <typeparam name="T">The <see cref="IComponentData"/> subtype of the singleton component.</typeparam>
        /// <returns>True, if a singleton of the specified type exists in the current <see cref="World"/>.</returns>
        public bool HasSingleton<T>()
            where T : struct, IComponentData
        {
            var type = ComponentType.ReadOnly<T>();
            var query = GetSingletonEntityQueryInternal(type);
            return query.CalculateEntityCount() == 1;
        }

        /// <summary>
        /// Gets the value of a singleton component.
        /// </summary>
        /// <typeparam name="T">The <see cref="IComponentData"/> subtype of the singleton component.</typeparam>
        /// <returns>The component.</returns>
        /// <seealso cref="EntityQuery.GetSingleton{T}"/>
        public T GetSingleton<T>()
            where T : struct, IComponentData
        {
            var type = ComponentType.ReadOnly<T>();
            var query = GetSingletonEntityQueryInternal(type);
            return query.GetSingleton<T>();
        }

        /// <summary>
        /// Sets the value of a singleton component.
        /// </summary>
        /// <param name="value">A component containing the value to assign to the singleton.</param>
        /// <typeparam name="T">The <see cref="IComponentData"/> subtype of the singleton component.</typeparam>
        /// <seealso cref="EntityQuery.SetSingleton{T}"/>
        public void SetSingleton<T>(T value)
            where T : struct, IComponentData
        {
            var type = ComponentType.ReadWrite<T>();
            var query = GetSingletonEntityQueryInternal(type);
            query.SetSingleton(value);
        }

        /// <summary>
        /// Gets the Entity instance for a singleton.
        /// </summary>
        /// <typeparam name="T">The Type of the singleton component.</typeparam>
        /// <returns>The entity associated with the specified singleton component.</returns>
        /// <seealso cref="EntityQuery.GetSingletonEntity"/>
        public Entity GetSingletonEntity<T>()
        {
            var type = ComponentType.ReadOnly<T>();
            var query = GetSingletonEntityQueryInternal(type);
            return query.GetSingletonEntity();
        }

        internal void AddReaderWriter(ComponentType componentType)
        {
            var state = CheckedState();
            if (CalculateReaderWriterDependency.Add(componentType, ref state->m_JobDependencyForReadingSystems, ref state->m_JobDependencyForWritingSystems))
            {
                CompleteDependencyInternal();
            }
        }

        internal void AddReaderWriters(EntityQuery query)
        {
            var state = CheckedState();
            if (query.AddReaderWritersToLists(ref state->m_JobDependencyForReadingSystems, ref state->m_JobDependencyForWritingSystems))
            {
                CompleteDependencyInternal();
            }
        }

        // Fast path for singletons
        internal EntityQuery GetSingletonEntityQueryInternal(ComponentType type)
        {
            var state = CheckedState();
            ref var handles = ref state->EntityQueries;

            for (var i = 0; i != handles.Length; i++)
            {
                var query = handles[i];
                var queryData = query._GetImpl()->_QueryData;

                // EntityQueries are constructed including the Entity ID
                if (2 != queryData->RequiredComponentsCount)
                    continue;

                if (queryData->RequiredComponents[1] != type)
                    continue;

                return query;
            }

            var newQuery = EntityManager.CreateEntityQuery(&type, 1);

            AddReaderWriters(newQuery);
            AfterQueryCreated(newQuery);

            return newQuery;
        }

        internal EntityQuery GetEntityQueryInternal(ComponentType* componentTypes, int count)
        {
            var state = CheckedState();
            ref var handles = ref state->EntityQueries;

            for (var i = 0; i != handles.Length; i++)
            {
                var query = handles[i];

                if (query.CompareComponents(componentTypes, count))
                    return query;
            }

            var newQuery = EntityManager.CreateEntityQuery(componentTypes, count);

            AddReaderWriters(newQuery);
            AfterQueryCreated(newQuery);

            return newQuery;
        }

        internal EntityQuery GetEntityQueryInternal(ComponentType[] componentTypes)
        {
            fixed(ComponentType* componentTypesPtr = componentTypes)
            {
                return GetEntityQueryInternal(componentTypesPtr, componentTypes.Length);
            }
        }

        internal EntityQuery GetEntityQueryInternal(EntityQueryDesc[] desc)
        {
            var state = CheckedState();
            ref var handles = ref state->EntityQueries;

            for (var i = 0; i != handles.Length; i++)
            {
                var query = handles[i];

                if (query.CompareQuery(desc))
                    return query;
            }

            var newQuery = EntityManager.CreateEntityQuery(desc);

            AddReaderWriters(newQuery);
            AfterQueryCreated(newQuery);

            return newQuery;
        }

        void AfterQueryCreated(EntityQuery query)
        {
            var state = CheckedState();

            query.SetChangedFilterRequiredVersion(state->m_LastSystemVersion);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            query._GetImpl()->_DisallowDisposing = true;
#endif

            state->EntityQueries.Add(query);
        }

        /// <summary>
        /// Gets the cached query for the specified component types, if one exists; otherwise, creates a new query
        /// instance and caches it.
        /// </summary>
        /// <param name="componentTypes">An array or comma-separated list of component types.</param>
        /// <returns>The new or cached query.</returns>
        protected internal EntityQuery GetEntityQuery(params ComponentType[] componentTypes)
        {
            return GetEntityQueryInternal(componentTypes);
        }

        /// <summary>
        /// Gets the cached query for the specified component types, if one exists; otherwise, creates a new query
        /// instance and caches it.
        /// </summary>
        /// <param name="componentTypes">An array of component types.</param>
        /// <returns>The new or cached query.</returns>
        protected EntityQuery GetEntityQuery(NativeArray<ComponentType> componentTypes)
        {
            return GetEntityQueryInternal((ComponentType*)componentTypes.GetUnsafeReadOnlyPtr(),
                componentTypes.Length);
        }

        /// <summary>
        /// Combines an array of query description objects into a single query.
        /// </summary>
        /// <remarks>This function looks for a cached query matching the combined query descriptions, and returns it
        /// if one exists; otherwise, the function creates a new query instance and caches it.</remarks>
        /// <returns>The new or cached query.</returns>
        /// <param name="queryDesc">An array of query description objects to be combined to define the query.</param>
        protected internal EntityQuery GetEntityQuery(params EntityQueryDesc[] queryDesc)
        {
            return GetEntityQueryInternal(queryDesc);
        }
    }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
    public static unsafe class ComponentSystemBaseManagedComponentExtensions
    {
        /// <summary>
        /// Checks whether a singleton component of the specified type exists.
        /// </summary>
        /// <typeparam name="T">The <see cref="IComponentData"/> subtype of the singleton component.</typeparam>
        /// <returns>True, if a singleton of the specified type exists in the current <see cref="World"/>.</returns>
        public static bool HasSingleton<T>(this ComponentSystemBase sys) where T : class, IComponentData
        {
            var type = ComponentType.ReadOnly<T>();
            var query = sys.GetSingletonEntityQueryInternal(type);
            return query.CalculateEntityCount() == 1;
        }

        /// <summary>
        /// Gets the value of a singleton component.
        /// </summary>
        /// <typeparam name="T">The <see cref="IComponentData"/> subtype of the singleton component.</typeparam>
        /// <returns>The component.</returns>
        /// <seealso cref="EntityQuery.GetSingleton{T}"/>
        public static T GetSingleton<T>(this ComponentSystemBase sys) where T : class, IComponentData
        {
            var type = ComponentType.ReadOnly<T>();
            var query = sys.GetSingletonEntityQueryInternal(type);
            return query.GetSingleton<T>();
        }

        /// <summary>
        /// Sets the value of a singleton component.
        /// </summary>
        /// <param name="value">A component containing the value to assign to the singleton.</param>
        /// <typeparam name="T">The <see cref="IComponentData"/> subtype of the singleton component.</typeparam>
        /// <seealso cref="EntityQuery.SetSingleton{T}"/>
        public static void SetSingleton<T>(this ComponentSystemBase sys, T value) where T : class, IComponentData
        {
            var type = ComponentType.ReadWrite<T>();
            var query = sys.GetSingletonEntityQueryInternal(type);
            query.SetSingleton(value);
        }
    }
#endif
}

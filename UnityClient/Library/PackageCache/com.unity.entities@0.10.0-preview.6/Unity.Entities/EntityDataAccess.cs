using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Profiling;

namespace Unity.Entities
{
    class ManagedEntityDataAccess
    {
        public World                       m_World;
        public EntityQuery                 m_UniversalQuery; // matches all components
        public EntityManager.EntityManagerDebug m_Debug;
        public ManagedComponentStore       m_ManagedComponentStore;
        public EntityQuery                 m_UniversalQueryWithChunks;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct EntityDataAccess : IDisposable
    {
        internal ManagedEntityDataAccess ManagedEntityDataAccess => (ManagedEntityDataAccess)m_ManagedAccess.Target;
        internal ManagedComponentStore ManagedComponentStore => ManagedEntityDataAccess.m_ManagedComponentStore;

        // These pointer attributes freak debuggers out because they take
        // copies of the root struct when inspecting, and it generally
        // misbehaves from there.

#if !NET_DOTS
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        internal EntityComponentStore* EntityComponentStore
        {
            // This is always safe as the EntityDataAccess is always unsafe heap allocated.
            get { fixed(EntityComponentStore* ptr = &m_EntityComponentStore) { return ptr; } }
        }

#if !NET_DOTS
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        internal EntityQueryManager* EntityQueryManager
        {
            // This is always safe as the EntityDataAccess is always unsafe heap allocated.
            get { fixed(EntityQueryManager* ptr = &m_EntityQueryManager) { return ptr; } }
        }

#if !NET_DOTS
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        internal ComponentDependencyManager* DependencyManager
        {
            // This is always safe as the EntityDataAccess is always unsafe heap allocated.
            get { fixed(ComponentDependencyManager* ptr = &m_DependencyManager) { return ptr; } }
        }

        [NativeDisableUnsafePtrRestriction]
        private EntityComponentStore m_EntityComponentStore;
        [NativeDisableUnsafePtrRestriction]
        private EntityQueryManager m_EntityQueryManager;
        [NativeDisableUnsafePtrRestriction]
        private ComponentDependencyManager m_DependencyManager;

        private GCHandle m_ManagedAccess;

        EntityArchetype m_EntityOnlyArchetype;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        public int m_InsideForEach;
#endif

        private UntypedUnsafeHashMap m_AliveEntityQueries;

        internal ref UnsafeHashMap<ulong, byte> AliveEntityQueries
        {
            get
            {
                fixed(void* ptr = &m_AliveEntityQueries)
                {
                    return ref UnsafeUtilityEx.AsRef<UnsafeHashMap<ulong, byte>>(ptr);
                }
            }
        }

        internal bool m_JobMode;

        internal bool IsMainThread => !m_JobMode;

        [BurstCompile]
        struct DestroyChunks : IJob
        {
            [NativeDisableUnsafePtrRestriction]
            public EntityComponentStore* EntityComponentStore;
            public NativeArray<ArchetypeChunk> Chunks;

            public void Execute()
            {
                EntityComponentStore->DestroyEntities(Chunks);
            }
        }

        public static void Initialize(EntityDataAccess* self, World world)
        {
            var managedGuts = new ManagedEntityDataAccess();

            self->m_EntityOnlyArchetype = default;
            self->m_ManagedAccess = GCHandle.Alloc(managedGuts);

            self->AliveEntityQueries = new UnsafeHashMap<ulong, byte>(32, Allocator.Persistent);

            managedGuts.m_World = world;

            self->m_DependencyManager.OnCreate();
            Entities.EntityComponentStore.Create(&self->m_EntityComponentStore, world.SequenceNumber << 32);
            Unity.Entities.EntityQueryManager.Create(&self->m_EntityQueryManager, &self->m_DependencyManager);

            managedGuts.m_ManagedComponentStore = new ManagedComponentStore();

            managedGuts.m_UniversalQuery = self->m_EntityQueryManager.CreateEntityQuery(
                self,
                new EntityQueryDesc[]
                {
                    new EntityQueryDesc { Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled },
                }
            );

            managedGuts.m_UniversalQueryWithChunks = self->m_EntityQueryManager.CreateEntityQuery(
                self,
                new EntityQueryDesc[]
                {
                    new EntityQueryDesc
                    {
                        All = new[] {ComponentType.ReadWrite<ChunkHeader>()},
                        Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
                    },
                    new EntityQueryDesc
                    {
                        Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
                    }
                });

            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            managedGuts.m_UniversalQuery._GetImpl()->_DisallowDisposing = true;
            managedGuts.m_UniversalQueryWithChunks._GetImpl()->_DisallowDisposing = true;
            #endif
        }

        public void Dispose()
        {
            ManagedEntityDataAccess managedGuts = ManagedEntityDataAccess;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            managedGuts.m_UniversalQuery._GetImpl()->_DisallowDisposing = false;
            managedGuts.m_UniversalQueryWithChunks._GetImpl()->_DisallowDisposing = false;
#endif
            managedGuts.m_UniversalQuery.Dispose();
            managedGuts.m_UniversalQueryWithChunks.Dispose();
            managedGuts.m_UniversalQuery = default;
            managedGuts.m_UniversalQueryWithChunks = default;

            m_DependencyManager.Dispose();
            Entities.EntityComponentStore.Destroy(EntityComponentStore);
            Entities.EntityQueryManager.Destroy(EntityQueryManager);

            managedGuts.m_ManagedComponentStore.Dispose();
            managedGuts.m_World = null;
            managedGuts.m_Debug = null;

            m_ManagedAccess.Free();
            m_ManagedAccess = default;

            AliveEntityQueries.Dispose();
            AliveEntityQueries = default;
        }

        public bool Exists(Entity entity)
        {
            return EntityComponentStore->Exists(entity);
        }

        public void DestroyEntity(Entity entity)
        {
            DestroyEntityInternal(&entity, 1);
        }

        public void BeforeStructuralChange()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (DependencyManager->IsInTransaction)
            {
                throw new InvalidOperationException(
                    "Access to EntityManager is not allowed after EntityManager.BeginExclusiveEntityTransaction(); has been called.");
            }

            if (DependencyManager->IsInForEachDisallowStructuralChange != 0)
            {
                throw new InvalidOperationException(
                    "Structural changes are not allowed during Entities.ForEach. Please use EntityCommandBuffer instead.");
            }

            // This is not an end user error. If there are any managed changes at this point, it indicates there is some
            // (previous) EntityManager change that is not properly playing back the managed changes that were buffered
            // afterward. That needs to be found and fixed.
            EntityComponentStore->AssertNoQueuedManagedDeferredCommands();
#endif

            DependencyManager->CompleteAllJobsAndInvalidateArrays();
        }

        public void DestroyEntity(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter)
        {
            if (!IsMainThread)
                throw new InvalidOperationException("Must be called from the main thread");

            Profiler.BeginSample("DestroyEntity(EntityQuery entityQueryFilter)");

            Profiler.BeginSample("GetAllMatchingChunks");
            using (var chunks = ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList, Allocator.TempJob, ref filter, DependencyManager))
            {
                Profiler.EndSample();

                if (chunks.Length != 0)
                {
                    BeforeStructuralChange();

                    Profiler.BeginSample("EditorOnlyChecks");
                    EntityComponentStore->AssertWillDestroyAllInLinkedEntityGroup(chunks, GetArchetypeChunkBufferType<LinkedEntityGroup>(false));
                    Profiler.EndSample();

                    // #todo @macton DestroyEntities should support IJobChunk. But internal writes need to be handled.
                    Profiler.BeginSample("DestroyChunks");
                    new DestroyChunks { EntityComponentStore = EntityComponentStore, Chunks = chunks }.Run();
                    Profiler.EndSample();

                    Profiler.BeginSample("Managed Playback");
                    PlaybackManagedChanges();
                    Profiler.EndSample();
                }
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="archetypeList"></param>
        /// <param name="filter"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void DestroyEntityDuringStructuralChange(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter)
        {
            if (!IsMainThread)
                throw new InvalidOperationException("Must be called from the main thread");

            Profiler.BeginSample("DestroyEntity(EntityQuery entityQueryFilter)");

            Profiler.BeginSample("GetAllMatchingChunks");
            using (var chunks = ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList, Allocator.TempJob, ref filter, DependencyManager))
            {
                Profiler.EndSample();

                if (chunks.Length != 0)
                {
                    Profiler.BeginSample("EditorOnlyChecks");
                    EntityComponentStore->AssertWillDestroyAllInLinkedEntityGroup(chunks, GetArchetypeChunkBufferType<LinkedEntityGroup>(false));
                    Profiler.EndSample();

                    // #todo @macton DestroyEntities should support IJobChunk. But internal writes need to be handled.
                    Profiler.BeginSample("DeleteChunks");
                    new DestroyChunks { EntityComponentStore = EntityComponentStore, Chunks = chunks }.Run();
                    Profiler.EndSample();
                }
            }

            Profiler.EndSample();
        }

        internal EntityArchetype CreateArchetype(ComponentType* types, int count)
        {
            ComponentTypeInArchetype* typesInArchetype = stackalloc ComponentTypeInArchetype[count + 1];

            var cachedComponentCount = FillSortedArchetypeArray(typesInArchetype, types, count);

            // Lookup existing archetype (cheap)
            EntityArchetype entityArchetype;
        #if ENABLE_UNITY_COLLECTIONS_CHECKS
            entityArchetype._DebugComponentStore = EntityComponentStore;
        #endif

            entityArchetype.Archetype = EntityComponentStore->GetExistingArchetype(typesInArchetype, cachedComponentCount);
            if (entityArchetype.Archetype != null)
                return entityArchetype;

            // Creating an archetype invalidates all iterators / jobs etc
            // because it affects the live iteration linked lists...
            EntityComponentStore.ArchetypeChanges archetypeChanges = default;

            if (IsMainThread)
                BeforeStructuralChange();
            archetypeChanges = EntityComponentStore->BeginArchetypeChangeTracking();

            entityArchetype.Archetype = EntityComponentStore->GetOrCreateArchetype(typesInArchetype, cachedComponentCount);

            EntityComponentStore->EndArchetypeChangeTracking(archetypeChanges, EntityQueryManager);

            return entityArchetype;
        }

        internal static int FillSortedArchetypeArray(ComponentTypeInArchetype* dst, ComponentType* requiredComponents, int count)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (count + 1 > 1024)
                throw new ArgumentException($"Archetypes can't hold more than 1024 components");
#endif

            dst[0] = new ComponentTypeInArchetype(ComponentType.ReadWrite<Entity>());
            for (var i = 0; i < count; ++i)
                SortingUtilities.InsertSorted(dst, i + 1, requiredComponents[i]);
            return count + 1;
        }

        public Entity CreateEntity(EntityArchetype archetype)
        {
            if (IsMainThread)
                BeforeStructuralChange();
            Entity entity = CreateEntityDuringStructuralChange(archetype);
            ManagedComponentStore.Playback(ref EntityComponentStore->ManagedChangesTracker);
            return entity;
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="archetype"></param>
        /// <returns></returns>
        public Entity CreateEntityDuringStructuralChange(EntityArchetype archetype)
        {
            Entity entity = EntityComponentStore->CreateEntityWithValidation(archetype);
            return entity;
        }

        internal void CreateEntity(EntityArchetype archetype, Entity* outEntities, int count)
        {
            if (IsMainThread)
                BeforeStructuralChange();
            StructuralChange.CreateEntity(EntityComponentStore, archetype.Archetype, outEntities, count);
            Assert.IsTrue(EntityComponentStore->ManagedChangesTracker.Empty);
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="archetype"></param>
        /// <param name="outEntities"></param>
        /// <param name="count"></param>
        internal void CreateEntityDuringStructuralChange(EntityArchetype archetype, Entity* outEntities, int count)
        {
            EntityComponentStore->CreateEntityWithValidation(archetype, outEntities, count);
        }

        public void CreateEntity(EntityArchetype archetype, NativeArray<Entity> entities)
        {
            CreateEntity(archetype, (Entity*)entities.GetUnsafePtr(), entities.Length);
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="archetype"></param>
        /// <param name="entities"></param>
        public void CreateEntityDuringStructuralChange(EntityArchetype archetype, NativeArray<Entity> entities)
        {
            CreateEntityDuringStructuralChange(archetype, (Entity*)entities.GetUnsafePtr(), entities.Length);
        }

        public bool AddComponent(Entity entity, ComponentType componentType)
        {
            if (HasComponent(entity, componentType))
                return false;

            EntityComponentStore->AssertCanAddComponent(entity, componentType);

            if (IsMainThread)
                BeforeStructuralChange();

            var archetypeChanges = EntityComponentStore->BeginArchetypeChangeTracking();

            var result = AddComponentDuringStructuralChange(entity, componentType);

            EntityComponentStore->EndArchetypeChangeTracking(archetypeChanges, EntityQueryManager);
            PlaybackManagedChanges();

            return result;
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componentType"></param>
        /// <returns></returns>
        public bool AddComponentDuringStructuralChange(Entity entity, ComponentType componentType)
        {
            if (HasComponent(entity, componentType))
                return false;

            var result = StructuralChange.AddComponentEntity(EntityComponentStore, &entity, componentType.TypeIndex);

            return result;
        }

        public void AddComponent(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter,
            ComponentType componentType)
        {
            if (!IsMainThread)
                throw new InvalidOperationException("Must be called from the main thread");

            EntityComponentStore->AssertCanAddComponent(archetypeList, componentType);

            using (var chunks =
                       ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList, Allocator.TempJob, ref filter,
                           DependencyManager))
            {
                if (chunks.Length == 0)
                    return;

                BeforeStructuralChange();
                var archetypeChanges = EntityComponentStore->BeginArchetypeChangeTracking();

                //@TODO the fast path for a chunk that contains a single entity is only possible if the chunk doesn't have a Locked Entity Order
                //but we should still be allowed to add zero sized components to chunks with a Locked Entity Order, even ones that only contain a single entity

                EntityComponentStore->AddComponentWithValidation(archetypeList, filter, componentType, DependencyManager);

                EntityComponentStore->EndArchetypeChangeTracking(archetypeChanges, EntityQueryManager);
                ManagedComponentStore.Playback(ref EntityComponentStore->ManagedChangesTracker);
            }
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="archetypeList"></param>
        /// <param name="filter"></param>
        /// <param name="componentType"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddComponentDuringStructuralChange(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter, ComponentType componentType)
        {
            if (!IsMainThread)
                throw new InvalidOperationException("Must be called from the main thread");

            using (var chunks =
                       ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList, Allocator.TempJob, ref filter,
                           DependencyManager))
            {
                if (chunks.Length == 0)
                    return;

                StructuralChange.AddComponentChunks(EntityComponentStore, (ArchetypeChunk*)NativeArrayUnsafeUtility.GetUnsafePtr(chunks), chunks.Length, componentType.TypeIndex);
            }
        }

        public bool RemoveComponent(Entity entity, ComponentType componentType)
        {
            if (IsMainThread)
                BeforeStructuralChange();

            var archetypeChanges = EntityComponentStore->BeginArchetypeChangeTracking();

            var removed = RemoveComponentDuringStructuralChange(entity, componentType);

            EntityComponentStore->EndArchetypeChangeTracking(archetypeChanges, EntityQueryManager);
            PlaybackManagedChanges();

            return removed;
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componentType"></param>
        /// <returns></returns>
        public bool RemoveComponentDuringStructuralChange(Entity entity, ComponentType componentType)
        {
            var removed = EntityComponentStore->RemoveComponent(entity, componentType);

            return removed;
        }

        public void RemoveComponent(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter, ComponentType componentType)
        {
            if (!IsMainThread)
                throw new InvalidOperationException("Must be called from the main thread");

            using (var chunks = ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList, Allocator.TempJob,
                ref filter, DependencyManager))
            {
                RemoveComponent(chunks, componentType);
            }
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="archetypeList"></param>
        /// <param name="filter"></param>
        /// <param name="componentType"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RemoveComponentDuringStructuralChange(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter, ComponentType componentType)
        {
            if (!IsMainThread)
                throw new InvalidOperationException("Must be called from the main thread");

            using (var chunks = ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList, Allocator.TempJob,
                ref filter, DependencyManager))
            {
                StructuralChange.RemoveComponentChunks(EntityComponentStore, (ArchetypeChunk*)NativeArrayUnsafeUtility.GetUnsafePtr(chunks), chunks.Length, componentType.TypeIndex);
            }
        }

        internal void RemoveComponent(NativeArray<ArchetypeChunk> chunks, ComponentType componentType)
        {
            if (chunks.Length == 0)
                return;

            if (IsMainThread)
                BeforeStructuralChange();
            var archetypeChanges = EntityComponentStore->BeginArchetypeChangeTracking();

            StructuralChange.RemoveComponentChunks(EntityComponentStore, (ArchetypeChunk*)NativeArrayUnsafeUtility.GetUnsafePtr(chunks), chunks.Length, componentType.TypeIndex);

            EntityComponentStore->EndArchetypeChangeTracking(archetypeChanges, EntityQueryManager);
            PlaybackManagedChanges();
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="chunks"></param>
        /// <param name="componentType"></param>
        internal void RemoveComponentDuringStructuralChange(NativeArray<ArchetypeChunk> chunks, ComponentType componentType)
        {
            EntityComponentStore->RemoveComponentWithValidation(chunks, componentType);
        }

        public bool HasComponent(Entity entity, ComponentType type)
        {
            return EntityComponentStore->HasComponent(entity, type);
        }

        public T GetComponentData<T>(Entity entity) where T : struct, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();

            EntityComponentStore->AssertEntityHasComponent(entity, typeIndex);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (ComponentType.FromTypeIndex(typeIndex).IsZeroSized)
                throw new System.ArgumentException(
                    $"GetComponentData<{typeof(T)}> can not be called with a zero sized component.");
#endif

            if (IsMainThread)
                DependencyManager->CompleteWriteDependency(typeIndex);

            var ptr = EntityComponentStore->GetComponentDataWithTypeRO(entity, typeIndex);

            T value;
            UnsafeUtility.CopyPtrToStructure(ptr, out value);
            return value;
        }

        public void* GetComponentDataRawRW(Entity entity, int typeIndex)
        {
            return EntityComponentStore->GetComponentDataRawRW(entity, typeIndex);
        }

        internal void* GetComponentDataRawRWEntityHasComponent(Entity entity, int typeIndex)
        {
            return EntityComponentStore->GetComponentDataRawRWEntityHasComponent(entity, typeIndex);
        }

        public void SetComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();

            EntityComponentStore->AssertEntityHasComponent(entity, typeIndex);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (ComponentType.FromTypeIndex(typeIndex).IsZeroSized)
                throw new System.ArgumentException(
                    $"SetComponentData<{typeof(T)}> can not be called with a zero sized component.");
#endif

            if (IsMainThread)
                DependencyManager->CompleteReadAndWriteDependency(typeIndex);

            var ptr = EntityComponentStore->GetComponentDataWithTypeRW(entity, typeIndex,
                EntityComponentStore->GlobalSystemVersion);
            UnsafeUtility.CopyStructureToPtr(ref componentData, ptr);
        }

        public void SetComponentDataRaw(Entity entity, int typeIndex, void* data, int size)
        {
            EntityComponentStore->SetComponentDataRawEntityHasComponent(entity, typeIndex, data, size);
        }

        public bool AddSharedComponentData<T>(Entity entity, T componentData, ManagedComponentStore managedComponentStore) where T : struct, ISharedComponentData
        {
            //TODO: optimize this (no need to move the entity to a new chunk twice)
            var added = AddComponent(entity, ComponentType.ReadWrite<T>());
            SetSharedComponentData(entity, componentData, managedComponentStore);
            return added;
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componentData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool AddSharedComponentDataDuringStructuralChange<T>(Entity entity, T componentData) where T : struct, ISharedComponentData
        {
            //TODO: optimize this (no need to move the entity to a new chunk twice)
            var added = AddComponentDuringStructuralChange(entity, ComponentType.ReadWrite<T>());
            SetSharedComponentData(entity, componentData, ManagedComponentStore);
            return added;
        }

        public void AddSharedComponentDataBoxedDefaultMustBeNull(Entity entity, int typeIndex, int hashCode, object componentData, ManagedComponentStore managedComponentStore)
        {
            //TODO: optimize this (no need to move the entity to a new chunk twice)
            AddComponent(entity, ComponentType.FromTypeIndex(typeIndex));
            SetSharedComponentDataBoxedDefaultMustBeNull(entity, typeIndex, hashCode, componentData, managedComponentStore);
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="typeIndex"></param>
        /// <param name="hashCode"></param>
        /// <param name="componentData"></param>
        public bool AddSharedComponentDataBoxedDefaultMustBeNullDuringStructuralChange(Entity entity, int typeIndex, int hashCode, object componentData, UnsafeList* managedReferenceIndexRemovalCount)
        {
            //TODO: optimize this (no need to move the entity to a new chunk twice)
            var added = AddComponentDuringStructuralChange(entity, ComponentType.FromTypeIndex(typeIndex));
            SetSharedComponentDataBoxedDefaultMustBeNullDuringStructuralChange(entity, typeIndex, hashCode, componentData, managedReferenceIndexRemovalCount);

            return added;
        }

        public void AddSharedComponentDataBoxedDefaultMustBeNull(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter, int typeIndex, int hashCode, object componentData, ManagedComponentStore managedComponentStore)
        {
            if (!IsMainThread)
                throw new InvalidOperationException("Must be called from the main thread");

            ComponentType componentType = ComponentType.FromTypeIndex(typeIndex);
            using (var chunks = ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList, Allocator.TempJob, ref filter, DependencyManager))
            {
                if (chunks.Length == 0)
                    return;
                var newSharedComponentDataIndex = 0;
                if (componentData != null) // null means default
                    newSharedComponentDataIndex = managedComponentStore.InsertSharedComponentAssumeNonDefault(typeIndex, hashCode, componentData);

                AddSharedComponentData(chunks, newSharedComponentDataIndex, componentType);
                managedComponentStore.RemoveReference(newSharedComponentDataIndex);
            }
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// ManagedComponentStore.RemoveReference() must be called after Playback for each newSharedComponentDataIndex added
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="archetypeList"></param>
        /// <param name="filter"></param>
        /// <param name="typeIndex"></param>
        /// <param name="hashCode"></param>
        /// <param name="componentData"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddSharedComponentDataBoxedDefaultMustBeNullDuringStructuralChange(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter, int typeIndex, int hashCode, object componentData, UnsafeList* managedReferenceIndexRemovalCount)
        {
            if (!IsMainThread)
                throw new InvalidOperationException("Must be called from the main thread");

            ComponentType componentType = ComponentType.FromTypeIndex(typeIndex);
            using (var chunks = ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList, Allocator.TempJob, ref filter, DependencyManager))
            {
                if (chunks.Length == 0)
                    return;
                var newSharedComponentDataIndex = 0;
                if (componentData != null) // null means default
                    newSharedComponentDataIndex = ManagedComponentStore.InsertSharedComponentAssumeNonDefault(typeIndex, hashCode, componentData);

                AddSharedComponentDataDuringStructuralChange(chunks, newSharedComponentDataIndex, componentType);
                managedReferenceIndexRemovalCount->Add(newSharedComponentDataIndex);
            }
        }

        internal void AddSharedComponentData(NativeArray<ArchetypeChunk> chunks, int sharedComponentIndex, ComponentType componentType)
        {
            Assert.IsTrue(componentType.IsSharedComponent);

            if (IsMainThread)
                BeforeStructuralChange();
            var archetypeChanges = EntityComponentStore->BeginArchetypeChangeTracking();

            StructuralChange.AddSharedComponentChunks(EntityComponentStore, (ArchetypeChunk*)NativeArrayUnsafeUtility.GetUnsafePtr(chunks), chunks.Length, componentType.TypeIndex, sharedComponentIndex);

            EntityComponentStore->EndArchetypeChangeTracking(archetypeChanges, EntityQueryManager);
            PlaybackManagedChanges();
        }

        private void PlaybackManagedChanges()
        {
            if (!EntityComponentStore->ManagedChangesTracker.Empty)
                ManagedComponentStore.Playback(ref EntityComponentStore->ManagedChangesTracker);
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="chunks"></param>
        /// <param name="sharedComponentIndex"></param>
        /// <param name="componentType"></param>
        internal void AddSharedComponentDataDuringStructuralChange(NativeArray<ArchetypeChunk> chunks, int sharedComponentIndex, ComponentType componentType)
        {
            Assert.IsTrue(componentType.IsSharedComponent);
            StructuralChange.AddSharedComponentChunks(EntityComponentStore, (ArchetypeChunk*)NativeArrayUnsafeUtility.GetUnsafePtr(chunks), chunks.Length, componentType.TypeIndex, sharedComponentIndex);
        }

        public T GetSharedComponentData<T>(Entity entity, ManagedComponentStore managedComponentStore) where T : struct, ISharedComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();
            EntityComponentStore->AssertEntityHasComponent(entity, typeIndex);

            var sharedComponentIndex = EntityComponentStore->GetSharedComponentDataIndex(entity, typeIndex);
            return managedComponentStore.GetSharedComponentData<T>(sharedComponentIndex);
        }

        public void SetSharedComponentData<T>(Entity entity, T componentData, ManagedComponentStore managedComponentStore) where T : struct, ISharedComponentData
        {
            if (IsMainThread)
                BeforeStructuralChange();

            var typeIndex = TypeManager.GetTypeIndex<T>();
            var componentType = ComponentType.FromTypeIndex(typeIndex);
            EntityComponentStore->AssertEntityHasComponent(entity, typeIndex);

            var newSharedComponentDataIndex = managedComponentStore.InsertSharedComponent(componentData);
            EntityComponentStore->SetSharedComponentDataIndex(entity, componentType, newSharedComponentDataIndex);
            managedComponentStore.Playback(ref EntityComponentStore->ManagedChangesTracker);
            managedComponentStore.RemoveReference(newSharedComponentDataIndex);
        }

        public void SetSharedComponentDataBoxedDefaultMustBeNull(Entity entity, int typeIndex, int hashCode, object componentData, ManagedComponentStore managedComponentStore)
        {
            if (IsMainThread)
                BeforeStructuralChange();

            EntityComponentStore->AssertEntityHasComponent(entity, typeIndex);

            var newSharedComponentDataIndex = 0;
            if (componentData != null) // null means default
                newSharedComponentDataIndex = managedComponentStore.InsertSharedComponentAssumeNonDefault(typeIndex, hashCode, componentData);

            var componentType = ComponentType.FromTypeIndex(typeIndex);
            EntityComponentStore->SetSharedComponentDataIndex(entity, componentType, newSharedComponentDataIndex);
            ManagedComponentStore.Playback(ref EntityComponentStore->ManagedChangesTracker);
            ManagedComponentStore.RemoveReference(newSharedComponentDataIndex);
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// ManagedComponentStore.RemoveReference() must be called after Playback for each newSharedComponentDataIndex added
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="typeIndex"></param>
        /// <param name="hashCode"></param>
        /// <param name="componentData"></param>
        public void SetSharedComponentDataBoxedDefaultMustBeNullDuringStructuralChange(Entity entity, int typeIndex, int hashCode, object componentData, UnsafeList* managedReferenceIndexRemovalCount)
        {
            EntityComponentStore->AssertEntityHasComponent(entity, typeIndex);

            var newSharedComponentDataIndex = 0;
            if (componentData != null) // null means default
                newSharedComponentDataIndex = ManagedComponentStore.InsertSharedComponentAssumeNonDefault(typeIndex,
                    hashCode, componentData);
            var componentType = ComponentType.FromTypeIndex(typeIndex);
            EntityComponentStore->SetSharedComponentDataIndex(entity, componentType, newSharedComponentDataIndex);

            managedReferenceIndexRemovalCount->Add(newSharedComponentDataIndex);
        }

        public DynamicBuffer<T> GetBuffer<T>(Entity entity
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            , AtomicSafetyHandle safety, AtomicSafetyHandle arrayInvalidationSafety
#endif
        ) where T : struct, IBufferElementData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            EntityComponentStore->AssertEntityHasComponent(entity, typeIndex);
            if (!TypeManager.IsBuffer(typeIndex))
                throw new ArgumentException(
                    $"GetBuffer<{typeof(T)}> may not be IComponentData or ISharedComponentData; currently {TypeManager.GetTypeInfo<T>().Category}");
#endif

            if (IsMainThread)
                DependencyManager->CompleteReadAndWriteDependency(typeIndex);

            BufferHeader* header =
                (BufferHeader*)EntityComponentStore->GetComponentDataWithTypeRW(entity, typeIndex,
                    EntityComponentStore->GlobalSystemVersion);

            int internalCapacity = TypeManager.GetTypeInfo(typeIndex).BufferCapacity;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var useMemoryInit = EntityComponentStore->useMemoryInitPattern != 0;
            byte memoryInitPattern = EntityComponentStore->memoryInitPattern;
            return new DynamicBuffer<T>(header, safety, arrayInvalidationSafety, false, useMemoryInit, memoryInitPattern, internalCapacity);
#else
            return new DynamicBuffer<T>(header, internalCapacity);
#endif
        }

        public void SetBufferRaw(Entity entity, int componentTypeIndex, BufferHeader* tempBuffer, int sizeInChunk)
        {
            if (IsMainThread)
                DependencyManager->CompleteReadAndWriteDependency(componentTypeIndex);

            EntityComponentStore->SetBufferRawWithValidation(entity, componentTypeIndex, tempBuffer, sizeInChunk);
        }

        public EntityArchetype GetEntityOnlyArchetype()
        {
            if (!m_EntityOnlyArchetype.Valid)
                m_EntityOnlyArchetype = CreateArchetype(null, 0);

            return m_EntityOnlyArchetype;
        }

        internal void InstantiateInternal(Entity srcEntity, Entity* outputEntities, int count)
        {
            if (IsMainThread)
                BeforeStructuralChange();
            EntityComponentStore->AssertEntitiesExist(&srcEntity, 1);
            EntityComponentStore->AssertCanInstantiateEntities(srcEntity, outputEntities, count);
            StructuralChange.InstantiateEntities(EntityComponentStore, &srcEntity, outputEntities, count);
            PlaybackManagedChanges();
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="srcEntity"></param>
        /// <param name="outputEntities"></param>
        /// <param name="count"></param>
        internal void InstantiateInternalDuringStructuralChange(Entity srcEntity, Entity* outputEntities, int count)
        {
            EntityComponentStore->InstantiateWithValidation(srcEntity, outputEntities, count);
        }

        internal void InstantiateInternal(Entity* srcEntities, Entity* outputEntities, int count, int outputCount, bool removePrefab)
        {
            if (IsMainThread)
                BeforeStructuralChange();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (count != outputCount)
                throw new System.ArgumentException($"srcEntities.Length ({count}) and outputEntities.Length (({outputCount})) must be the same.");
#endif

            EntityComponentStore->AssertEntitiesExist(srcEntities, count);
            EntityComponentStore->AssertCanInstantiateEntities(srcEntities, count, removePrefab);
            EntityComponentStore->InstantiateEntities(srcEntities, outputEntities, count, removePrefab);
            ManagedComponentStore.Playback(ref EntityComponentStore->ManagedChangesTracker);
        }

        internal void DestroyEntityInternal(Entity* entities, int count)
        {
            if (IsMainThread)
                BeforeStructuralChange();

            EntityComponentStore->AssertValidEntities(entities, count);
            EntityComponentStore->DestroyEntities(entities, count);
            PlaybackManagedChanges();
        }

        /// <summary>
        /// EntityManager.BeforeStructuralChange must be called before invoking this.
        /// ManagedComponentStore.Playback must be called after invoking this.
        /// EntityQueryManager.AddAdditionalArchetypes must be called after invoking this.
        /// Invoking this must be wrapped in ArchetypeChangeTracking.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="count"></param>
        internal void DestroyEntityInternalDuringStructuralChange(Entity* entities, int count)
        {
            EntityComponentStore->DestroyEntityWithValidation(entities, count);
        }

        public void SwapComponents(ArchetypeChunk leftChunk, int leftIndex, ArchetypeChunk rightChunk, int rightIndex)
        {
            if (IsMainThread)
                BeforeStructuralChange();

            var globalSystemVersion = EntityComponentStore->GlobalSystemVersion;

            ChunkDataUtility.SwapComponents(leftChunk.m_Chunk, leftIndex, rightChunk.m_Chunk, rightIndex, 1,
                globalSystemVersion, globalSystemVersion);
        }

        public ArchetypeChunkBufferType<T> GetArchetypeChunkBufferType<T>(bool isReadOnly)
            where T : struct, IBufferElementData
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var typeIndex = TypeManager.GetTypeIndex<T>();
            return new ArchetypeChunkBufferType<T>(
                DependencyManager->Safety.GetSafetyHandleForArchetypeChunkBufferType(typeIndex, isReadOnly),
                DependencyManager->Safety.GetBufferHandleForArchetypeChunkBufferType(typeIndex),
                isReadOnly, EntityComponentStore->GlobalSystemVersion);
#else
            return new ArchetypeChunkBufferType<T>(isReadOnly, EntityComponentStore->GlobalSystemVersion);
#endif
        }
    }

    static unsafe partial class EntityDataAccessManagedComponentExtensions
    {
        internal static int* GetManagedComponentIndex(ref this EntityDataAccess dataAccess, Entity entity, int typeIndex)
        {
            dataAccess.EntityComponentStore->AssertEntityHasComponent(entity, typeIndex);

            if (dataAccess.IsMainThread)
                dataAccess.DependencyManager->CompleteReadAndWriteDependency(typeIndex);

            return (int*)dataAccess.EntityComponentStore->GetComponentDataWithTypeRW(entity, typeIndex, dataAccess.EntityComponentStore->GlobalSystemVersion);
        }

        public static T GetComponentData<T>(ref this EntityDataAccess dataAccess, Entity entity, ManagedComponentStore managedComponentStore) where T : class, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();
            var index = *dataAccess.GetManagedComponentIndex(entity, typeIndex);
            return (T)managedComponentStore.GetManagedComponent(index);
        }

        public static T GetComponentObject<T>(ref this EntityDataAccess dataAccess, Entity entity, ComponentType componentType, ManagedComponentStore managedComponentStore)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!componentType.IsManagedComponent)
                throw new System.ArgumentException($"GetComponentObject must be called with a managed component type.");
#endif
            var index = *dataAccess.GetManagedComponentIndex(entity, componentType.TypeIndex);
            return (T)managedComponentStore.GetManagedComponent(index);
        }

        public static void SetComponentObject(ref this EntityDataAccess dataAccess, Entity entity, ComponentType componentType, object componentObject, ManagedComponentStore managedComponentStore)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!componentType.IsManagedComponent)
                throw new System.ArgumentException($"SetComponentObject must be called with a managed component type.");
            if (componentObject != null && componentObject.GetType() != TypeManager.GetType(componentType.TypeIndex))
                throw new System.ArgumentException($"SetComponentObject {componentObject.GetType()} doesn't match the specified component type: {TypeManager.GetType(componentType.TypeIndex)}");
#endif
            var ptr = dataAccess.GetManagedComponentIndex(entity, componentType.TypeIndex);
            managedComponentStore.UpdateManagedComponentValue(ptr, componentObject, ref *dataAccess.EntityComponentStore);
        }
    }
}

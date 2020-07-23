using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.CodeGeneratedJobForEach;
using Unity.Jobs;

namespace Unity.Entities
{
    public class EntityQueryDescValidationException : Exception
    {
        public EntityQueryDescValidationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Describes a query to find archetypes in terms of required, optional, and excluded
    /// components.
    /// </summary>
    /// <remarks>
    /// Define an EntityQueryDesc object to describe complex queries. Inside a system,
    /// pass an EntityQueryDesc object to <see cref="ComponentSystemBase.GetEntityQuery(EntityQueryDesc[])"/>
    /// to create the <see cref="EntityQuery"/>. Outside a system, use
    /// <see cref="EntityManager.CreateEntityQuery(EntityQueryDesc[])"/>.
    ///
    /// A query description combines the component types you specify in `All`, `Any`, and `None` sets according to the
    /// following rules:
    ///
    /// * All - Includes archetypes that have every component in this set
    /// * Any - Includes archetypes that have at least one component in this set
    /// * None - Excludes archetypes that have any component in this set
    ///
    /// For example, given entities with the following components:
    ///
    /// * Player has components: Position, Rotation, Player
    /// * Enemy1 has components: Position, Rotation, Melee
    /// * Enemy2 has components: Position, Rotation, Ranger
    ///
    /// The query description below matches all of the archetypes that:
    /// have any of [Melee or Ranger], AND have none of [Player], AND have all of [Position and Rotation]
    ///
    /// <example>
    /// <code lang="csharp" source="../../DocCodeSamples.Tests/EntityQueryExamples.cs" region="query-description" title="Query Description"/>
    /// </example>
    ///
    /// In other words, the query created from this description selects the Enemy1 and Enemy2 entities, but not the Player entity.
    /// </remarks>
    public class EntityQueryDesc
    {
        /// <summary>
        /// Include archetypes that contain at least one (but possibly more) of the
        /// component types in the Any list.
        /// </summary>
        public ComponentType[] Any = Array.Empty<ComponentType>();
        /// <summary>
        /// Exclude archetypes that contain any of the
        /// component types in the None list.
        /// </summary>
        public ComponentType[] None = Array.Empty<ComponentType>();
        /// <summary>
        /// Include archetypes that contain all of the
        /// component types in the All list.
        /// </summary>
        public ComponentType[] All = Array.Empty<ComponentType>();
        /// <summary>
        /// Specialized query options.
        /// </summary>
        /// <remarks>
        /// You should not need to set these options for most queries.
        ///
        /// Options is a bit mask; use the bitwise OR operator to combine multiple options.
        /// </remarks>
        public EntityQueryOptions Options = EntityQueryOptions.Default;

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void ValidateComponentTypes(ComponentType[] componentTypes, ref NativeArray<int> allComponentTypeIds, ref int curComponentTypeIndex)
        {
            for (int i = 0; i < componentTypes.Length; i++)
            {
                var componentType = componentTypes[i];
                allComponentTypeIds[curComponentTypeIndex++] = componentType.TypeIndex;
                if (componentType.AccessModeType == ComponentType.AccessMode.Exclude)
                    throw new ArgumentException("EntityQueryDesc cannot contain Exclude Component types");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public void Validate()
        {
            // Determine the number of ComponentTypes contained in the filters
            var itemCount = None.Length + All.Length + Any.Length;

            // Project all the ComponentType Ids of None, All, Any queryDesc filters into the same array to identify duplicated later on
            // Also, check that queryDesc doesn't contain any ExcludeComponent...

            var allComponentTypeIds = new NativeArray<int>(itemCount, Allocator.Temp);
            var curComponentTypeIndex = 0;
            ValidateComponentTypes(None, ref allComponentTypeIds, ref curComponentTypeIndex);
            ValidateComponentTypes(All, ref allComponentTypeIds, ref curComponentTypeIndex);
            ValidateComponentTypes(Any, ref allComponentTypeIds, ref curComponentTypeIndex);

            // Check for duplicate, only if necessary
            if (itemCount > 1)
            {
                // Sort the Ids to have identical value adjacent
                allComponentTypeIds.Sort();

                // Check for identical values
                var refId = allComponentTypeIds[0];
                for (int i = 1; i < allComponentTypeIds.Length; i++)
                {
                    var curId = allComponentTypeIds[i];
                    if (curId == refId)
                    {
#if NET_DOTS
                        throw new EntityQueryDescValidationException(
                            $"EntityQuery contains a filter with duplicate component type index {curId}.  Queries can only contain a single component of a given type in a filter.");
#else
                        var compType = TypeManager.GetType(curId);
                        throw new EntityQueryDescValidationException(
                            $"EntityQuery contains a filter with duplicate component type name {compType.Name}.  Queries can only contain a single component of a given type in a filter.");
#endif
                    }

                    refId = curId;
                }
            }
        }
    }

    /// <summary>
    /// The bit flags to use for the <see cref="EntityQueryDesc.Options"/> field.
    /// </summary>
    [Flags]
    public enum EntityQueryOptions
    {
        /// <summary>
        /// No options specified.
        /// </summary>
        Default = 0,
        /// <summary>
        /// The query does not exclude the special <see cref="Prefab"/> component.
        /// </summary>
        IncludePrefab = 1,
        /// <summary>
        /// The query does not exclude the special <see cref="Disabled"/> component.
        /// </summary>
        IncludeDisabled = 2,
        /// <summary>
        /// The query filters selected entities based on the
        /// <see cref="WriteGroupAttribute"/> settings of the components specified in the query description.
        /// </summary>
        FilterWriteGroup = 4,
    }

    /// <summary>
    /// Provides an efficient test of whether a specific entity would be selected by an EntityQuery.
    /// </summary>
    /// <remarks>
    /// Use a mask to quickly identify whether an entity would be selected by an EntityQuery.
    ///
    /// <example>
    /// <code lang="csharp" source="../../DocCodeSamples.Tests/EntityQueryExamples.cs" region="entity-query-mask" title="Query Mask"/>
    /// </example>
    ///
    /// You can create up to 1024 unique EntityQueryMasks in an application.
    /// Note that you cannot create an EntityQueryMasks from an EntityQuery object that has a filter.
    /// </remarks>
    /// <seealso cref="EntityManager.GetEntityQueryMask"/>
    public unsafe struct EntityQueryMask
    {
        internal byte Index;
        internal byte Mask;

        [NativeDisableUnsafePtrRestriction]
        internal readonly EntityComponentStore* EntityComponentStore;

        internal EntityQueryMask(byte index, byte mask, EntityComponentStore* entityComponentStore)
        {
            Index = index;
            Mask = mask;
            EntityComponentStore = entityComponentStore;
        }

        internal bool IsCreated()
        {
            return EntityComponentStore != null;
        }

        /// <summary>
        /// Reports whether an entity would be selected by the EntityQuery instance used to create this entity query mask.
        /// </summary>
        /// <remarks>
        /// The match does not consider any filter settings of the EntityQuery.
        /// </remarks>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the entity would be returned by the EntityQuery, false if it would not.</returns>
        public bool Matches(Entity entity)
        {
            return EntityComponentStore->GetArchetype(entity)->CompareMask(this);
        }
    };

    internal unsafe struct EntityQueryImpl
    {
        internal EntityDataAccess*              _Access;
        internal EntityQueryData* _QueryData;
        internal EntityQueryFilter          _Filter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal bool                    _DisallowDisposing;
#endif


        internal GCHandle                _CachedState;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal ComponentSafetyHandles* SafetyHandles => &_Access->DependencyManager->Safety;
#endif

        internal void Construct(EntityQueryData* queryData, EntityDataAccess* access)
        {
            _Access = access;
            _QueryData = queryData;
            _Filter = default(EntityQueryFilter);
            fixed(EntityQueryImpl* self = &this)
            {
                access->AliveEntityQueries.Add((ulong)(IntPtr)self, default);
            }
        }

        public bool IsEmptyIgnoreFilter
        {
            get
            {
                for (var m = 0; m < _QueryData->MatchingArchetypes.Length; ++m)
                {
                    var match = _QueryData->MatchingArchetypes.Ptr[m];
                    if (match->Archetype->EntityCount > 0)
                        return false;
                }

                return true;
            }
        }
#if NET_DOTS
        internal class SlowListSet<T>
        {
            internal List<T> items;

            internal SlowListSet()
            {
                items = new List<T>();
            }

            internal void Add(T item)
            {
                if (!items.Contains(item))
                    items.Add(item);
            }

            internal int Count => items.Count;

            internal T[] ToArray()
            {
                return items.ToArray();
            }
        }
#endif

        internal ComponentType[] GetQueryTypes()
        {
#if !NET_DOTS
            var types = new HashSet<ComponentType>();
#else
            var types = new SlowListSet<ComponentType>();
#endif

            for (var i = 0; i < _QueryData->ArchetypeQueryCount; ++i)
            {
                for (var j = 0; j < _QueryData->ArchetypeQuery[i].AnyCount; ++j)
                {
                    types.Add(TypeManager.GetType(_QueryData->ArchetypeQuery[i].Any[j]));
                }
                for (var j = 0; j < _QueryData->ArchetypeQuery[i].AllCount; ++j)
                {
                    types.Add(TypeManager.GetType(_QueryData->ArchetypeQuery[i].All[j]));
                }
                for (var j = 0; j < _QueryData->ArchetypeQuery[i].NoneCount; ++j)
                {
                    types.Add(ComponentType.Exclude(TypeManager.GetType(_QueryData->ArchetypeQuery[i].None[j])));
                }
            }

#if !NET_DOTS
            var array = new ComponentType[types.Count];
            var t = 0;
            foreach (var type in types)
                array[t++] = type;
            return array;
#else
            return types.ToArray();
#endif
        }

        internal ComponentType[] GetReadAndWriteTypes()
        {
            var types = new ComponentType[_QueryData->ReaderTypesCount + _QueryData->WriterTypesCount];
            var typeArrayIndex = 0;
            for (var i = 0; i < _QueryData->ReaderTypesCount; ++i)
            {
                types[typeArrayIndex++] = ComponentType.ReadOnly(TypeManager.GetType(_QueryData->ReaderTypes[i]));
            }
            for (var i = 0; i < _QueryData->WriterTypesCount; ++i)
            {
                types[typeArrayIndex++] = TypeManager.GetType(_QueryData->WriterTypes[i]);
            }

            return types;
        }

        public void Dispose()
        {
            fixed(EntityQueryImpl* self = &this)
            {
                _Access->AliveEntityQueries.Remove((ulong)(IntPtr)self);
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (_DisallowDisposing)
                throw new InvalidOperationException("EntityQuery cannot currently be disposed");
#endif

            if (_CachedState.IsAllocated)
            {
                ((IDisposable)_CachedState.Target).Dispose();
                _CachedState.Free();
                _CachedState = default;
            }

            if (_QueryData != null)
                ResetFilter();

            _Access = null;
            _QueryData = null;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        /// <summary>
        ///     Gets safety handle to a ComponentType required by this EntityQuery.
        /// </summary>
        /// <param name="indexInEntityQuery">Index of a ComponentType in this EntityQuery's RequiredComponents list./param>
        /// <returns>AtomicSafetyHandle for a ComponentType</returns>
        internal AtomicSafetyHandle GetSafetyHandle(int indexInEntityQuery)
        {
            var type = _QueryData->RequiredComponents + indexInEntityQuery;
            var isReadOnly = type->AccessModeType == ComponentType.AccessMode.ReadOnly;
            return SafetyHandles->GetSafetyHandle(type->TypeIndex, isReadOnly);
        }

        /// <summary>
        ///     Gets buffer safety handle to a ComponentType required by this EntityQuery.
        /// </summary>
        /// <param name="indexInEntityQuery">Index of a ComponentType in this EntityQuery's RequiredComponents list./param>
        /// <returns>AtomicSafetyHandle for a buffer</returns>
        internal AtomicSafetyHandle GetBufferSafetyHandle(int indexInEntityQuery)
        {
            var type = _QueryData->RequiredComponents + indexInEntityQuery;
            return SafetyHandles->GetBufferSafetyHandle(type->TypeIndex);
        }

#endif

        bool GetIsReadOnly(int indexInEntityQuery)
        {
            var type = _QueryData->RequiredComponents + indexInEntityQuery;
            var isReadOnly = type->AccessModeType == ComponentType.AccessMode.ReadOnly;
            return isReadOnly;
        }

        public int CalculateEntityCount()
        {
            SyncFilterTypes();
            return ChunkIterationUtility.CalculateEntityCount(in _QueryData->MatchingArchetypes, ref _Filter);
        }

        public int CalculateEntityCountWithoutFiltering()
        {
            var dummyFilter = default(EntityQueryFilter);
            return ChunkIterationUtility.CalculateEntityCount(in _QueryData->MatchingArchetypes, ref dummyFilter);
        }

        public int CalculateChunkCount()
        {
            SyncFilterTypes();
            return ChunkIterationUtility.CalculateChunkCount(in _QueryData->MatchingArchetypes, ref _Filter);
        }

        public int CalculateChunkCountWithoutFiltering()
        {
            var dummyFilter = default(EntityQueryFilter);
            return ChunkIterationUtility.CalculateChunkCount(_QueryData->MatchingArchetypes, ref dummyFilter);
        }

        public ArchetypeChunkIterator GetArchetypeChunkIterator()
        {
            return new ArchetypeChunkIterator(_QueryData->MatchingArchetypes, _Access->DependencyManager, _Access->EntityComponentStore->GlobalSystemVersion, ref _Filter);
        }

        internal int GetIndexInEntityQuery(int componentType)
        {
            var componentIndex = 0;
            while (componentIndex < _QueryData->RequiredComponentsCount && _QueryData->RequiredComponents[componentIndex].TypeIndex != componentType)
                ++componentIndex;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (componentIndex >= _QueryData->RequiredComponentsCount || _QueryData->RequiredComponents[componentIndex].AccessModeType == ComponentType.AccessMode.Exclude)
                throw new InvalidOperationException($"Trying to get iterator for {TypeManager.GetType(componentType)} but the required component type was not declared in the EntityQuery.");
#endif
            return componentIndex;
        }

        public NativeArray<ArchetypeChunk> CreateArchetypeChunkArrayAsync(Allocator allocator, out JobHandle jobhandle)
        {
            JobHandle dependency = default;

            var filterCount = _Filter.Changed.Count;
            if (filterCount > 0)
            {
                var readerTypes = stackalloc int[filterCount];
                for (int i = 0; i < filterCount; ++i)
                    readerTypes[i] = _QueryData->RequiredComponents[_Filter.Changed.IndexInEntityQuery[i]].TypeIndex;

                dependency = _Access->DependencyManager->GetDependency(readerTypes, filterCount, null, 0);
            }

            return ChunkIterationUtility.CreateArchetypeChunkArrayWithoutSync(_QueryData->MatchingArchetypes, allocator, out jobhandle, ref _Filter, dependency);
        }

        public NativeArray<ArchetypeChunk> CreateArchetypeChunkArray(Allocator allocator)
        {
            SyncFilterTypes();
            JobHandle job;
            var res = ChunkIterationUtility.CreateArchetypeChunkArrayWithoutSync(_QueryData->MatchingArchetypes, allocator, out job, ref _Filter);
            job.Complete();
            return res;
        }

        public NativeArray<Entity> ToEntityArrayAsync(Allocator allocator, out JobHandle jobhandle, EntityQuery outer)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var entityType = new ArchetypeChunkEntityType(SafetyHandles->GetSafetyHandleForArchetypeChunkEntityType());
#else
            var entityType = new ArchetypeChunkEntityType();
#endif

            return ChunkIterationUtility.CreateEntityArray(_QueryData->MatchingArchetypes, allocator, entityType, outer, ref _Filter, out jobhandle, GetDependency());
        }

        public NativeArray<Entity> ToEntityArray(Allocator allocator, EntityQuery outer)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var entityType = new ArchetypeChunkEntityType(SafetyHandles->GetSafetyHandleForArchetypeChunkEntityType());
#else
            var entityType = new ArchetypeChunkEntityType();
#endif
            JobHandle job;
            var res = ChunkIterationUtility.CreateEntityArray(_QueryData->MatchingArchetypes, allocator, entityType, outer, ref _Filter, out job, GetDependency());
            job.Complete();
            return res;
        }

        internal void GatherEntitiesToArray(out EntityQuery.GatherEntitiesResult result, EntityQuery outer)
        {
            ChunkIterationUtility.GatherEntitiesToArray(_QueryData, ref _Filter, out result);

            if (result.EntityBuffer == null)
            {
                var entityCount = CalculateEntityCount();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                var entityType = new ArchetypeChunkEntityType(SafetyHandles->GetSafetyHandleForArchetypeChunkEntityType());
#else
                var entityType = new ArchetypeChunkEntityType();
#endif
                var job = new GatherEntitiesJob
                {
                    EntityType = entityType,
                    Entities = new NativeArray<Entity>(entityCount, Allocator.TempJob)
                };
                job.Run(outer);
                result.EntityArray = job.Entities;
                result.EntityBuffer = (Entity*)result.EntityArray.GetUnsafeReadOnlyPtr();
                result.EntityCount = result.EntityArray.Length;
            }
        }

        internal void ReleaseGatheredEntities(ref EntityQuery.GatherEntitiesResult result)
        {
            ChunkIterationUtility.currentOffsetInResultBuffer = result.StartingOffset;
            if (result.EntityArray.IsCreated)
            {
                result.EntityArray.Dispose();
            }
        }

        public NativeArray<T> ToComponentDataArrayAsync<T>(Allocator allocator, out JobHandle jobhandle, EntityQuery outer)
            where T : struct, IComponentData
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var componentType = new ArchetypeChunkComponentType<T>(SafetyHandles->GetSafetyHandleForArchetypeChunkComponentType(TypeManager.GetTypeIndex<T>(), true), true, _Access->EntityComponentStore->GlobalSystemVersion);
#else
            var componentType = new ArchetypeChunkComponentType<T>(true, _Access->EntityComponentStore->GlobalSystemVersion);
#endif


#if ENABLE_UNITY_COLLECTIONS_CHECKS
            int typeIndex = TypeManager.GetTypeIndex<T>();
            int indexInEntityQuery = GetIndexInEntityQuery(typeIndex);
            if (indexInEntityQuery == -1)
                throw new InvalidOperationException($"Trying ToComponentDataArrayAsync of {TypeManager.GetType(typeIndex)} but the required component type was not declared in the EntityQuery.");
#endif
            return ChunkIterationUtility.CreateComponentDataArray(_QueryData->MatchingArchetypes, allocator, componentType, outer, ref _Filter, out jobhandle, GetDependency());
        }

        public NativeArray<T> ToComponentDataArray<T>(Allocator allocator, EntityQuery outer)
            where T : struct, IComponentData
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var componentType = new ArchetypeChunkComponentType<T>(SafetyHandles->GetSafetyHandleForArchetypeChunkComponentType(TypeManager.GetTypeIndex<T>(), true), true, _Access->EntityComponentStore->GlobalSystemVersion);
#else
            var componentType = new ArchetypeChunkComponentType<T>(true, _Access->EntityComponentStore->GlobalSystemVersion);
#endif

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            int typeIndex = TypeManager.GetTypeIndex<T>();
            int indexInEntityQuery = GetIndexInEntityQuery(typeIndex);
            if (indexInEntityQuery == -1)
                throw new InvalidOperationException($"Trying ToComponentDataArray of {TypeManager.GetType(typeIndex)} but the required component type was not declared in the EntityQuery.");
#endif

            JobHandle job;
            var res = ChunkIterationUtility.CreateComponentDataArray(_QueryData->MatchingArchetypes, allocator, componentType, outer, ref _Filter, out job, GetDependency());
            job.Complete();
            return res;
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        public T[] ToComponentDataArray<T>() where T : class, IComponentData
        {
            int typeIndex = TypeManager.GetTypeIndex<T>();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var componentType = new ArchetypeChunkComponentType<T>(SafetyHandles->GetSafetyHandleForArchetypeChunkComponentType(typeIndex, true), true, _Access->EntityComponentStore->GlobalSystemVersion);
#else
            var componentType = new ArchetypeChunkComponentType<T>(true, _Access->EntityComponentStore->GlobalSystemVersion);
#endif

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            int indexInEntityQuery = GetIndexInEntityQuery(typeIndex);
            if (indexInEntityQuery == -1)
                throw new InvalidOperationException($"Trying ToComponentDataArray of {TypeManager.GetType(typeIndex)} but the required component type was not declared in the EntityQuery.");
#endif

            var mcs = _Access->ManagedComponentStore;
            var matches = _QueryData->MatchingArchetypes;
            var entityCount = ChunkIterationUtility.CalculateEntityCount(matches, ref _Filter);
            T[] res = new T[entityCount];
            int i = 0;
            for (int mi = 0; mi < matches.Length; ++mi)
            {
                var match = _QueryData->MatchingArchetypes.Ptr[mi];
                var indexInTypeArray = ChunkDataUtility.GetIndexInTypeArray(match->Archetype, typeIndex);
                var chunks = match->Archetype->Chunks;

                for (int ci = 0; ci < chunks.Count; ++ci)
                {
                    var chunk = chunks.p[ci];

                    if (_Filter.RequiresMatchesFilter && !chunk->MatchesFilter(match, ref _Filter))
                        continue;

                    var managedComponentArray = (int*)ChunkDataUtility.GetComponentDataRW(chunk, 0, indexInTypeArray);
                    for (int entityIndex = 0; entityIndex < chunk->Count; ++entityIndex)
                    {
                        res[i++] = (T)mcs.GetManagedComponent(managedComponentArray[entityIndex]);
                    }
                }
            }

            return res;
        }

#endif

        public void CopyFromComponentDataArray<T>(NativeArray<T> componentDataArray, EntityQuery outer)
            where T : struct, IComponentData
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var entityCount = CalculateEntityCount();
            if (entityCount != componentDataArray.Length)
                throw new ArgumentException($"Length of input array ({componentDataArray.Length}) does not match length of EntityQuery ({entityCount})");

            var componentType = new ArchetypeChunkComponentType<T>(SafetyHandles->GetSafetyHandleForArchetypeChunkComponentType(TypeManager.GetTypeIndex<T>(), false), false, _Access->EntityComponentStore->GlobalSystemVersion);
#else
            var componentType = new ArchetypeChunkComponentType<T>(false, _Access->EntityComponentStore->GlobalSystemVersion);
#endif

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            int typeIndex = TypeManager.GetTypeIndex<T>();
            int indexInEntityQuery = GetIndexInEntityQuery(typeIndex);
            if (indexInEntityQuery == -1)
                throw new InvalidOperationException($"Trying CopyFromComponentDataArray of {TypeManager.GetType(typeIndex)} but the required component type was not declared in the EntityQuery.");
#endif


            ChunkIterationUtility.CopyFromComponentDataArray(_QueryData->MatchingArchetypes, componentDataArray, componentType, outer, ref _Filter, out var job, GetDependency());
            job.Complete();
        }

        public void CopyFromComponentDataArrayAsync<T>(NativeArray<T> componentDataArray, out JobHandle jobhandle, EntityQuery outer)
            where T : struct, IComponentData
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var entityCount = CalculateEntityCount();
            if (entityCount != componentDataArray.Length)
                throw new ArgumentException($"Length of input array ({componentDataArray.Length}) does not match length of EntityQuery ({entityCount})");
#endif

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var componentType = new ArchetypeChunkComponentType<T>(SafetyHandles->GetSafetyHandleForArchetypeChunkComponentType(TypeManager.GetTypeIndex<T>(), false), false, _Access->EntityComponentStore->GlobalSystemVersion);
#else
            var componentType = new ArchetypeChunkComponentType<T>(false, _Access->EntityComponentStore->GlobalSystemVersion);
#endif

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            int typeIndex = TypeManager.GetTypeIndex<T>();
            int indexInEntityQuery = GetIndexInEntityQuery(typeIndex);
            if (indexInEntityQuery == -1)
                throw new InvalidOperationException($"Trying CopyFromComponentDataArrayAsync of {TypeManager.GetType(typeIndex)} but the required component type was not declared in the EntityQuery.");
#endif

            ChunkIterationUtility.CopyFromComponentDataArray(_QueryData->MatchingArchetypes, componentDataArray, componentType, outer, ref _Filter, out jobhandle, GetDependency());
        }

        public Entity GetSingletonEntity()
        {
            // Fast path with no filter
            if (!_Filter.RequiresMatchesFilter)
            {
                var archetypeIndex = GetFirstArchetypeIndexWithEntity(out var archetypeEntityCount);
       #if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (archetypeEntityCount != 1)
                    throw new InvalidOperationException($"GetSingletonEntity() requires that exactly one entity exists that matches this query, but there are {archetypeEntityCount}.");
       #endif
                return UnsafeUtilityEx.AsRef<Entity>(ChunkIterationUtility.GetChunkComponentDataROPtr(_QueryData->MatchingArchetypes.Ptr[archetypeIndex]->Archetype->Chunks.p[0], 0));
            }

            // Slow path with filter, can't just use first matching archetype/chunk
       #if ENABLE_UNITY_COLLECTIONS_CHECKS
            var queryEntityCount = CalculateEntityCount();
            if (queryEntityCount != 1)
                throw new InvalidOperationException($"GetSingletonEntity() requires that exactly one entity exists that matches this query, but there are {queryEntityCount}.");
       #endif
            var iterator = GetArchetypeChunkIterator();
            iterator.MoveNext();
            var array = iterator.GetCurrentChunkComponentDataPtr(false, 0);
            UnsafeUtility.CopyPtrToStructure(array, out Entity entity);
            return entity;
        }

        internal int GetFirstArchetypeIndexWithEntity(out int entityCount)
        {
            entityCount = 0;
            int archeTypeIndex = -1;
            for (int i = 0; i < _QueryData->MatchingArchetypes.Length; i++)
            {
                var entityCountInArchetype = _QueryData->MatchingArchetypes.Ptr[i]->Archetype->EntityCount;
                if (archeTypeIndex == -1 && entityCountInArchetype > 0)
                    archeTypeIndex = i;
                entityCount += entityCountInArchetype;
            }

            return archeTypeIndex;
        }

        public T GetSingleton<T>() where T : struct, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();
            _Access->DependencyManager->CompleteWriteDependencyNoChecks(typeIndex);

            // Fast path with no filter
            if (!_Filter.RequiresMatchesFilter && _QueryData->RequiredComponentsCount <= 2 && _QueryData->RequiredComponents[1].TypeIndex == typeIndex)
            {
                var archetypeIndex = GetFirstArchetypeIndexWithEntity(out var archetypeEntityCount);
       #if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (archetypeEntityCount != 1)
                    throw new InvalidOperationException($"GetSingleton<{typeof(T)}>() requires that exactly one {typeof(T)} exist that match this query, but there are {archetypeEntityCount}.");
       #endif
                var archetype = _QueryData->MatchingArchetypes.Ptr[archetypeIndex]->Archetype;
                for (var typeIndexInArchetype = 0; typeIndexInArchetype < archetype->TypesCount; ++typeIndexInArchetype)
                {
                    if (archetype->Types[typeIndexInArchetype].TypeIndex == typeIndex)
                        return UnsafeUtilityEx.AsRef<T>(ChunkIterationUtility.GetChunkComponentDataROPtr(archetype->Chunks.p[0], typeIndexInArchetype));
                }
                return default;
            }

            // Slow path with filter, can't just use first matching archetype/chunk
       #if ENABLE_UNITY_COLLECTIONS_CHECKS
            var queryEntityCount = CalculateEntityCount();
            if (queryEntityCount != 1)
                throw new InvalidOperationException($"GetSingleton<{typeof(T)}>() requires that exactly one {typeof(T)} exist that match this query, but there are {queryEntityCount}.");
       #endif
            var iterator = GetArchetypeChunkIterator();
            iterator.MoveNext();
            return UnsafeUtilityEx.AsRef<T>(iterator.GetCurrentChunkComponentDataPtr(false, 1));
        }

        public void SetSingleton<T>(T value) where T : struct, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();
            _Access->DependencyManager->CompleteWriteDependencyNoChecks(typeIndex);

            // Fast path with no filter & assuming this is a simple query with just one singleton component
            if (!_Filter.RequiresMatchesFilter && _QueryData->RequiredComponentsCount <= 2 && _QueryData->RequiredComponents[1].TypeIndex == typeIndex)
            {
                var archetypeIndex = GetFirstArchetypeIndexWithEntity(out var entityCount);
               #if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (entityCount != 1)
                    throw new InvalidOperationException($"SetSingleton<{typeof(T)}>() requires that exactly one {typeof(T)} exist that match this query, but there are {entityCount}.");
               #endif
                var match = _QueryData->MatchingArchetypes.Ptr[archetypeIndex];
                UnsafeUtility.CopyStructureToPtr(ref value, ChunkIterationUtility.GetChunkComponentDataPtr(match->Archetype->Chunks.p[0], true,
                    match->IndexInArchetype[1], _Access->EntityComponentStore->GlobalSystemVersion));
                return;
            }


#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var queryEntityCount = CalculateEntityCount();
            if (queryEntityCount != 1)
                throw new InvalidOperationException($"SetSingleton<{typeof(T)}>() requires that exactly one {typeof(T)} exist that match this query, but there are {queryEntityCount}.");
#endif
            var iterator = GetArchetypeChunkIterator();
            iterator.MoveNext();
            UnsafeUtility.CopyStructureToPtr(ref value, iterator.GetCurrentChunkComponentDataPtr(true, GetIndexInEntityQuery(typeIndex)));
        }

        internal bool CompareComponents(ComponentType* componentTypes, int count)
        {
            return EntityQueryManager.CompareComponents(componentTypes, count, _QueryData);
        }

        public bool CompareComponents(ComponentType[] componentTypes)
        {
            fixed(ComponentType* componentTypesPtr = componentTypes)
            {
                return EntityQueryManager.CompareComponents(componentTypesPtr, componentTypes.Length, _QueryData);
            }
        }

        public bool CompareComponents(NativeArray<ComponentType> componentTypes)
        {
            return EntityQueryManager.CompareComponents((ComponentType*)componentTypes.GetUnsafeReadOnlyPtr(), componentTypes.Length, _QueryData);
        }

        public bool CompareQuery(EntityQueryDesc[] queryDesc)
        {
            return EntityQueryManager.CompareQuery(queryDesc, _QueryData);
        }

        public void ResetFilter()
        {
            var sharedCount = _Filter.Shared.Count;
            var sm = _Access->ManagedComponentStore;
            for (var i = 0; i < sharedCount; ++i)
                sm.RemoveReference(_Filter.Shared.SharedComponentIndex[i]);

            _Filter.Changed.Count = 0;
            _Filter.Shared.Count = 0;
        }

        public void SetSharedComponentFilter<SharedComponent1>(SharedComponent1 sharedComponent1)
            where SharedComponent1 : struct, ISharedComponentData
        {
            ResetFilter();
            AddSharedComponentFilter(sharedComponent1);
        }

        public void SetSharedComponentFilter<SharedComponent1, SharedComponent2>(SharedComponent1 sharedComponent1,
            SharedComponent2 sharedComponent2)
            where SharedComponent1 : struct, ISharedComponentData
            where SharedComponent2 : struct, ISharedComponentData
        {
            ResetFilter();
            AddSharedComponentFilter(sharedComponent1);
            AddSharedComponentFilter(sharedComponent2);
        }

        public void SetChangedVersionFilter(ComponentType componentType)
        {
            ResetFilter();
            AddChangedVersionFilter(componentType);
        }

        internal void SetChangedFilterRequiredVersion(uint requiredVersion)
        {
            _Filter.RequiredChangeVersion = requiredVersion;
        }

        public void SetChangedVersionFilter(ComponentType[] componentType)
        {
            if (componentType.Length > EntityQueryFilter.ChangedFilter.Capacity)
                throw new ArgumentException(
                    $"EntityQuery.SetFilterChanged accepts a maximum of {EntityQueryFilter.ChangedFilter.Capacity} component array length");
            if (componentType.Length <= 0)
                throw new ArgumentException(
                    $"EntityQuery.SetFilterChanged component array length must be larger than 0");

            ResetFilter();
            for (var i = 0; i != componentType.Length; i++)
                AddChangedVersionFilter(componentType[i]);
        }

        public void AddChangedVersionFilter(ComponentType componentType)
        {
            var newFilterIndex = _Filter.Changed.Count;
            if (newFilterIndex >= EntityQueryFilter.ChangedFilter.Capacity)
                throw new ArgumentException($"EntityQuery accepts a maximum of {EntityQueryFilter.ChangedFilter.Capacity} changed filters.");

            _Filter.Changed.Count = newFilterIndex + 1;
            _Filter.Changed.IndexInEntityQuery[newFilterIndex] = GetIndexInEntityQuery(componentType.TypeIndex);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _Filter.AssertValid();
#endif
        }

        public void AddSharedComponentFilter<SharedComponent>(SharedComponent sharedComponent)
            where SharedComponent : struct, ISharedComponentData
        {
            var sm = _Access->ManagedComponentStore;

            var newFilterIndex = _Filter.Shared.Count;
            if (newFilterIndex >= EntityQueryFilter.SharedComponentData.Capacity)
                throw new ArgumentException($"EntityQuery accepts a maximum of {EntityQueryFilter.SharedComponentData.Capacity} shared component filters.");

            _Filter.Shared.Count = newFilterIndex + 1;
            _Filter.Shared.IndexInEntityQuery[newFilterIndex] = GetIndexInEntityQuery(TypeManager.GetTypeIndex<SharedComponent>());
            _Filter.Shared.SharedComponentIndex[newFilterIndex] = sm.InsertSharedComponent(sharedComponent);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _Filter.AssertValid();
#endif
        }

        public void CompleteDependency()
        {
            _Access->DependencyManager->CompleteDependenciesNoChecks(_QueryData->ReaderTypes, _QueryData->ReaderTypesCount,
                _QueryData->WriterTypes, _QueryData->WriterTypesCount);
        }

        public JobHandle GetDependency()
        {
            return _Access->DependencyManager->GetDependency(_QueryData->ReaderTypes, _QueryData->ReaderTypesCount,
                _QueryData->WriterTypes, _QueryData->WriterTypesCount);
        }

        public JobHandle AddDependency(JobHandle job)
        {
            return _Access->DependencyManager->AddDependency(_QueryData->ReaderTypes, _QueryData->ReaderTypesCount,
                _QueryData->WriterTypes, _QueryData->WriterTypesCount, job);
        }

        public int GetCombinedComponentOrderVersion()
        {
            var version = 0;

            for (var i = 0; i < _QueryData->RequiredComponentsCount; ++i)
                version += _Access->EntityComponentStore->GetComponentTypeOrderVersion(_QueryData->RequiredComponents[i].TypeIndex);

            return version;
        }

        internal bool AddReaderWritersToLists(ref UnsafeIntList reading, ref UnsafeIntList writing)
        {
            bool anyAdded = false;
            for (int i = 0; i < _QueryData->ReaderTypesCount; ++i)
                anyAdded |= CalculateReaderWriterDependency.AddReaderTypeIndex(_QueryData->ReaderTypes[i], ref reading, ref writing);

            for (int i = 0; i < _QueryData->WriterTypesCount; ++i)
                anyAdded |= CalculateReaderWriterDependency.AddWriterTypeIndex(_QueryData->WriterTypes[i], ref reading, ref writing);
            return anyAdded;
        }

        internal void SyncFilterTypes()
        {
            for (int i = 0; i < _Filter.Changed.Count; ++i)
            {
                var type = _QueryData->RequiredComponents[_Filter.Changed.IndexInEntityQuery[i]];
                _Access->DependencyManager->CompleteWriteDependency(type.TypeIndex);
            }
        }

        internal static void SyncFilterTypes(ref UnsafeMatchingArchetypePtrList matchingArchetypes, ref EntityQueryFilter filter, ComponentDependencyManager* safetyManager)
        {
            if (matchingArchetypes.Length < 1)
                return;

            var match = *matchingArchetypes.Ptr;
            for (int i = 0; i < filter.Changed.Count; ++i)
            {
                var indexInEntityQuery = filter.Changed.IndexInEntityQuery[i];
                var componentIndexInChunk = match->IndexInArchetype[indexInEntityQuery];
                var type = match->Archetype->Types[componentIndexInChunk];
                safetyManager->CompleteWriteDependency(type.TypeIndex);
            }
        }

        public bool HasFilter()
        {
            return _Filter.RequiresMatchesFilter;
        }

        internal static EntityQueryImpl* Allocate()
        {
            void* ptr = UnsafeUtility.Malloc(sizeof(EntityQueryImpl), 8, Allocator.Persistent);
            UnsafeUtility.MemClear(ptr, sizeof(EntityQueryImpl));
            return (EntityQueryImpl*)ptr;
        }

        internal static void Free(EntityQueryImpl* impl)
        {
            UnsafeUtility.Free(impl, Allocator.Persistent);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct EntityQueryNullShim
    {
    }

    /// <summary>
    /// Use an EntityQuery object to select entities with components that meet specific requirements.
    /// </summary>
    /// <remarks>
    /// An entity query defines the set of component types that an [archetype] must contain
    /// in order for its chunks and entities to be selected and specifies whether the components accessed
    /// through the query are read-only or read-write.
    ///
    /// For simple queries, you can create an EntityQuery based on an array of
    /// component types. The following example defines a EntityQuery that finds all entities
    /// with both Rotation and RotationSpeed components.
    ///
    /// <example>
    /// <code source="../DocCodeSamples.Tests/EntityQueryExamples.cs" region="query-from-list" title="EntityQuery Example"/>
    /// </example>
    ///
    /// The query uses [ComponentType.ReadOnly] instead of the simpler `typeof` expression
    /// to designate that the system does not write to RotationSpeed. Always specify read-only
    /// when possible, since there are fewer constraints on read-only access to data, which can help
    /// the Job scheduler execute your Jobs more efficiently.
    ///
    /// For more complex queries, you can use an <see cref="EntityQueryDesc"/> object to create the entity query.
    /// A query description provides a flexible query mechanism to specify which archetypes to select
    /// based on the following sets of components:
    ///
    /// * `All` = All component types in this array must exist in the archetype
    /// * `Any` = At least one of the component types in this array must exist in the archetype
    /// * `None` = None of the component types in this array can exist in the archetype
    ///
    /// For example, the following query includes archetypes containing Rotation and
    /// RotationSpeed components, but excludes any archetypes containing a Frozen component:
    ///
    /// <example>
    /// <code source="../DocCodeSamples.Tests/EntityQueryExamples.cs" region="query-from-description" title="EntityQuery Example"/>
    /// </example>
    ///
    /// **Note:** Do not include completely optional components in the query description. To handle optional
    /// components, use <see cref="IJobChunk"/> and the [ArchetypeChunk.Has()] method to determine whether a chunk contains the
    /// optional component or not. Since all entities within the same chunk have the same components, you
    /// only need to check whether an optional component exists once per chunk -- not once per entity.
    ///
    /// Within a system class, use the [ComponentSystemBase.GetEntityQuery()] function
    /// to get a EntityQuery instance. Outside a system, use the [EntityManager.CreateEntityQuery()] function.
    ///
    /// You can filter entities based on
    /// whether they have [changed] or whether they have a specific value for a [shared component].
    /// Once you have created an EntityQuery object, you can
    /// [reset] and change the filter settings, but you cannot modify the base query.
    ///
    /// Use an EntityQuery for the following purposes:
    ///
    /// * To get a [native array] of a the values for a specific <see cref="IComponentData"/> type for all entities matching the query
    /// * To get an [native array] of the <see cref="ArchetypeChunk"/> objects matching the query
    /// * To schedule an <see cref="IJobChunk"/> job
    /// * To control whether a system updates using [ComponentSystemBase.RequireForUpdate(query)]
    ///
    /// Note that [Entities.ForEach] defines an entity query implicitly based on the methods you call. You can
    /// access this implicit EntityQuery object using [Entities.WithStoreEntityQueryInField]. However, you cannot
    /// create an [Entities.ForEach] construction based on an existing EntityQuery object.
    ///
    /// [Entities.ForEach]: xref:Unity.Entities.SystemBase.Entities
    /// [Entities.WithStoreEntityQueryInField]: xref:Unity.Entities.SystemBase.Entities
    /// [ComponentSystemBase.GetEntityQuery()]: xref:Unity.Entities.ComponentSystemBase.GetEntityQuery*
    /// [EntityManager.CreateEntityQuery()]: xref:Unity.Entities.EntityManager.CreateEntityQuery*
    /// [ComponentType.ReadOnly]: xref:Unity.Entities.ComponentType.ReadOnly``1
    /// [ComponentSystemBase.RequireForUpdate()]: xref:Unity.Entities.ComponentSystemBase.RequireForUpdate(Unity.Entities.EntityQuery)
    /// [ArchetypeChunk.Has()]: xref:Unity.Entities.ArchetypeChunk.Has``1(Unity.Entities.ArchetypeChunkComponentType{``0})
    /// [archetype]: xref:Unity.Entities.EntityArchetype
    /// [changed]: xref:Unity.Entities.EntityQuery.SetChangedVersionFilter*
    /// [shared component]: xref:Unity.Entities.EntityQuery.SetSharedComponentFilter*
    /// [reset]: xref:Unity.Entities.EntityQuery.ResetFilter*
    /// [native array]: https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html
    /// </remarks>
    unsafe public struct EntityQuery : IDisposable
    {
        public bool Equals(EntityQuery other)
        {
            return __impl == other.__impl;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityQuery other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked((int)(long)__impl);
        }

        static internal unsafe EntityQuery Construct(EntityQueryData* queryData, EntityDataAccess* access)
        {
            EntityQuery _result = default;
            var _ptr = EntityQueryImpl.Allocate();
            _ptr->Construct(queryData, access);
            _result.__seqno = World.ms_NextSequenceNumber.Data++;
            _result.__impl = _ptr;
            _CreateSafetyHandle(ref _result);
            return _result;
        }

        /// <summary>
        /// Reports whether this query would currently select zero entities.
        /// </summary>
        /// <returns>True, if this EntityQuery matches zero existing entities. False, if it matches one or more entities.</returns>
        public bool IsEmptyIgnoreFilter => _GetImpl()->IsEmptyIgnoreFilter;

        /// <summary>
        /// Gets the array of <see cref="ComponentType"/> objects included in this EntityQuery.
        /// </summary>
        /// <returns>An array of ComponentType objects</returns>
        internal ComponentType[] GetQueryTypes() => _GetImpl()->GetQueryTypes();

        /// <summary>
        ///     Packed array of this EntityQuery's ReadOnly and writable ComponentTypes.
        ///     ReadOnly ComponentTypes come before writable types in this array.
        /// </summary>
        /// <returns>Array of ComponentTypes</returns>
        internal ComponentType[] GetReadAndWriteTypes() => _GetImpl()->GetReadAndWriteTypes();

        /// <summary>
        /// Disposes this EntityQuery instance.
        /// </summary>
        /// <remarks>Do not dispose EntityQuery instances accessed using
        /// <see cref="ComponentSystemBase.GetEntityQuery(ComponentType[])"/>. Systems automatically dispose of
        /// their own entity queries.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if you attempt to dispose an EntityQuery
        /// belonging to a system.</exception>
        public void Dispose()
        {
            var self = _GetImpl();
            self->Dispose();

            EntityQueryImpl.Free(self);

            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(__safety);
            #endif

            __impl = null;
        }

        internal IDisposable _CachedState
        {
            get
            {
                var impl = _GetImpl();
                if (!impl->_CachedState.IsAllocated)
                    return null;
                return (IDisposable)impl->_CachedState.Target;
            }
            set
            {
                var impl = _GetImpl();
                if (!impl->_CachedState.IsAllocated)
                {
                    impl->_CachedState = GCHandle.Alloc(value);
                }
                else
                {
                    impl->_CachedState.Target = value;
                }
            }
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        /// <summary>
        ///     Gets safety handle to a ComponentType required by this EntityQuery.
        /// </summary>
        /// <param name="indexInEntityQuery">Index of a ComponentType in this EntityQuery's RequiredComponents list./param>
        /// <returns>AtomicSafetyHandle for a ComponentType</returns>
        internal AtomicSafetyHandle GetSafetyHandle(int indexInEntityQuery) => _GetImpl()->GetSafetyHandle(indexInEntityQuery);

        /// <summary>
        ///     Gets buffer safety handle to a ComponentType required by this EntityQuery.
        /// </summary>
        /// <param name="indexInEntityQuery">Index of a ComponentType in this EntityQuery's RequiredComponents list./param>
        /// <returns>AtomicSafetyHandle for a buffer</returns>
        internal AtomicSafetyHandle GetBufferSafetyHandle(int indexInEntityQuery) => _GetImpl()->GetBufferSafetyHandle(indexInEntityQuery);

#endif

        /// <summary>
        /// Calculates the number of entities selected by this EntityQuery.
        /// </summary>
        /// <remarks>
        /// The EntityQuery must execute and apply any filters to calculate the entity count.
        /// </remarks>
        /// <returns>The number of entities based on the current EntityQuery properties.</returns>
        public int CalculateEntityCount() => _GetImpl()->CalculateEntityCount();
        /// <summary>
        /// Calculates the number of entities selected by this EntityQuery, ignoring any set filters.
        /// </summary>
        /// <remarks>
        /// The EntityQuery must execute to calculate the entity count.
        /// </remarks>
        /// <returns>The number of entities based on the current EntityQuery properties.</returns>
        public int CalculateEntityCountWithoutFiltering() => _GetImpl()->CalculateEntityCountWithoutFiltering();
        /// <summary>
        /// Calculates the number of chunks that match this EntityQuery.
        /// </summary>
        /// <remarks>
        /// The EntityQuery must execute and apply any filters to calculate the chunk count.
        /// </remarks>
        /// <returns>The number of chunks based on the current EntityQuery properties.</returns>
        public int CalculateChunkCount() => _GetImpl()->CalculateChunkCount();
        /// <summary>
        /// Calculates the number of chunks that match this EntityQuery, ignoring any set filters.
        /// </summary>
        /// <remarks>
        /// The EntityQuery must execute to calculate the chunk count.
        /// </remarks>
        /// <returns>The number of chunks based on the current EntityQuery properties.</returns>
        public int CalculateChunkCountWithoutFiltering() => _GetImpl()->CalculateChunkCountWithoutFiltering();
        /// <summary>
        /// Gets an ArchetypeChunkIterator which can be used to iterate over every chunk returned by this EntityQuery.
        /// </summary>
        /// <returns>ArchetypeChunkIterator for this EntityQuery</returns>
        public ArchetypeChunkIterator GetArchetypeChunkIterator() => _GetImpl()->GetArchetypeChunkIterator();
        /// <summary>
        ///     Index of a ComponentType in this EntityQuery's RequiredComponents list.
        ///     For example, you have a EntityQuery that requires these ComponentTypes: Position, Velocity, and Color.
        ///
        ///     These are their type indices (according to the TypeManager):
        ///         Position.TypeIndex == 3
        ///         Velocity.TypeIndex == 5
        ///            Color.TypeIndex == 17
        ///
        ///     RequiredComponents: [Position -> Velocity -> Color] (a linked list)
        ///     Given Velocity's TypeIndex (5), the return value would be 1, since Velocity is in slot 1 of RequiredComponents.
        /// </summary>
        /// <param name="componentType">Index of a ComponentType in the TypeManager</param>
        /// <returns>An index into RequiredComponents.</returns>
        internal int GetIndexInEntityQuery(int componentType) => _GetImpl()->GetIndexInEntityQuery(componentType);
        /// <summary>
        /// Asynchronously creates an array of the chunks containing entities matching this EntityQuery.
        /// </summary>
        /// <remarks>
        /// Use <paramref name="jobhandle"/> as a dependency for jobs that use the returned chunk array.
        /// <seealso cref="CreateArchetypeChunkArray(Unity.Collections.Allocator)"/>.</remarks>
        /// <param name="allocator">Allocator to use for the array.</param>
        /// <param name="jobhandle">An `out` parameter assigned the handle to the internal job
        /// that gathers the chunks matching this EntityQuery.
        /// </param>
        /// <returns>NativeArray of all the chunks containing entities matching this query.</returns>
        public NativeArray<ArchetypeChunk> CreateArchetypeChunkArrayAsync(Allocator allocator, out JobHandle jobhandle) => _GetImpl()->CreateArchetypeChunkArrayAsync(allocator, out jobhandle);

        /// <summary>
        /// Synchronously creates an array of the chunks containing entities matching this EntityQuery.
        /// </summary>
        /// <remarks>This method blocks until the internal job that performs the query completes.
        /// <seealso cref="CreateArchetypeChunkArray(Unity.Collections.Allocator,out Unity.Jobs.JobHandle)"/>
        /// </remarks>
        /// <param name="allocator">Allocator to use for the array.</param>
        /// <returns>NativeArray of all the chunks in this ComponentChunkIterator.</returns>
        public NativeArray<ArchetypeChunk> CreateArchetypeChunkArray(Allocator allocator) => _GetImpl()->CreateArchetypeChunkArray(allocator);
        /// <summary>
        /// Creates a NativeArray containing the selected entities.
        /// </summary>
        /// <param name="allocator">The type of memory to allocate.</param>
        /// <param name="jobhandle">An `out` parameter assigned a handle that you can use as a dependency for a Job
        /// that uses the NativeArray.</param>
        /// <returns>An array containing all the entities selected by the EntityQuery.</returns>
        public NativeArray<Entity> ToEntityArrayAsync(Allocator allocator, out JobHandle jobhandle) => _GetImpl()->ToEntityArrayAsync(allocator, out jobhandle, this);
        /// <summary>
        /// Creates a NativeArray containing the selected entities.
        /// </summary>
        /// <remarks>This version of the function blocks until the Job used to fill the array is complete.</remarks>
        /// <param name="allocator">The type of memory to allocate.</param>
        /// <returns>An array containing all the entities selected by the EntityQuery.</returns>
        public NativeArray<Entity> ToEntityArray(Allocator allocator) => _GetImpl()->ToEntityArray(allocator, this);

        internal struct GatherEntitiesResult
        {
            public int StartingOffset;
            public int EntityCount;
            public Entity* EntityBuffer;
            public NativeArray<Entity> EntityArray;
        }

        internal void GatherEntitiesToArray(out GatherEntitiesResult result) => _GetImpl()->GatherEntitiesToArray(out result, this);
        internal void ReleaseGatheredEntities(ref GatherEntitiesResult result) => _GetImpl()->ReleaseGatheredEntities(ref result);
        /// <summary>
        /// Creates a NativeArray containing the components of type T for the selected entities.
        /// </summary>
        /// <param name="allocator">The type of memory to allocate.</param>
        /// <param name="jobhandle">An `out` parameter assigned a handle that you can use as a dependency for a Job
        /// that uses the NativeArray.</param>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>An array containing the specified component for all the entities selected
        /// by the EntityQuery.</returns>
        public NativeArray<T> ToComponentDataArrayAsync<T>(Allocator allocator, out JobHandle jobhandle)            where T : struct, IComponentData
            => _GetImpl()->ToComponentDataArrayAsync<T>(allocator, out jobhandle, this);
        /// <summary>
        /// Creates a NativeArray containing the components of type T for the selected entities.
        /// </summary>
        /// <param name="allocator">The type of memory to allocate.</param>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>An array containing the specified component for all the entities selected
        /// by the EntityQuery.</returns>
        /// <exception cref="InvalidOperationException">Thrown if you ask for a component that is not part of
        /// the group.</exception>
        public NativeArray<T> ToComponentDataArray<T>(Allocator allocator)            where T : struct, IComponentData
            => _GetImpl()->ToComponentDataArray<T>(allocator, this);
#if !UNITY_DISABLE_MANAGED_COMPONENTS
        public T[] ToComponentDataArray<T>() where T : class, IComponentData
            => _GetImpl()->ToComponentDataArray<T>();
#endif

        public void CopyFromComponentDataArray<T>(NativeArray<T> componentDataArray)            where T : struct, IComponentData
            => _GetImpl()->CopyFromComponentDataArray<T>(componentDataArray, this);

        public void CopyFromComponentDataArrayAsync<T>(NativeArray<T> componentDataArray, out JobHandle jobhandle)            where T : struct, IComponentData
            => _GetImpl()->CopyFromComponentDataArrayAsync<T>(componentDataArray, out jobhandle, this);

        public Entity GetSingletonEntity() => _GetImpl()->GetSingletonEntity();

        /// <summary>
        /// Gets the value of a singleton component.
        /// </summary>
        /// <remarks>A singleton component is a component of which only one instance exists that satisfies this query.</remarks>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>A copy of the singleton component.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <seealso cref="SetSingleton{T}(T)"/>
        /// <seealso cref="GetSingletonEntity"/>
        /// <seealso cref="ComponentSystemBase.GetSingleton{T}"/>
        public T GetSingleton<T>() where T : struct, IComponentData
            => _GetImpl()->GetSingleton<T>();
        /// <summary>
        /// Sets the value of a singleton component.
        /// </summary>
        /// <remarks>
        /// For a component to be a singleton, there can be only one instance of that component
        /// that satisfies this query.
        ///
        /// **Note:** singletons are otherwise normal entities. The EntityQuery and <see cref="ComponentSystemBase"/>
        /// singleton functions add checks that you have not created two instances of a
        /// type that can be accessed by this singleton query, but other APIs do not prevent such accidental creation.
        ///
        /// To create a singleton, create an entity with the singleton component.
        ///
        /// For example, if you had a component defined as:
        ///
        /// <example>
        /// <code lang="csharp" source="../../DocCodeSamples.Tests/EntityQueryExamples.cs" region="singleton-type-example" title="Singleton"/>
        /// </example>
        ///
        /// You could create a singleton as follows:
        ///
        /// <example>
        /// <code lang="csharp" source="../../DocCodeSamples.Tests/EntityQueryExamples.cs" region="create-singleton" title="Create Singleton"/>
        /// </example>
        ///
        /// To update the singleton component after creation, you can use an EntityQuery object that
        /// selects the singleton entity and call this `SetSingleton()` function:
        ///
        /// <example>
        /// <code lang="csharp" source="../../DocCodeSamples.Tests/EntityQueryExamples.cs" region="set-singleton" title="Set Singleton"/>
        /// </example>
        ///
        /// You can set and get the singleton value from a system: see <seealso cref="ComponentSystemBase.SetSingleton{T}(T)"/>
        /// and <seealso cref="ComponentSystemBase.GetSingleton{T}"/>.
        /// </remarks>
        /// <param name="value">An instance of type T containing the values to set.</param>
        /// <typeparam name="T">The component type.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown if more than one instance of this component type
        /// exists in the world or the component type appears in more than one archetype.</exception>
        /// <seealso cref="GetSingleton{T}"/>
        /// <seealso cref="GetSingletonEntity"/>
        public void SetSingleton<T>(T value) where T : struct, IComponentData
            => _GetImpl()->SetSingleton<T>(value);
        internal bool CompareComponents(ComponentType* componentTypes, int count) => _GetImpl()->CompareComponents(componentTypes, count);
        /// <summary>
        /// Compares a list of component types to the types defining this EntityQuery.
        /// </summary>
        /// <remarks>Only required types in the query are used as the basis for the comparison.
        /// If you include types that the query excludes or only includes as optional,
        /// the comparison returns false.</remarks>
        /// <param name="componentTypes">An array of ComponentType objects.</param>
        /// <returns>True, if the list of types, including any read/write access specifiers,
        /// matches the list of required component types of this EntityQuery.</returns>
        public bool CompareComponents(ComponentType[] componentTypes) => _GetImpl()->CompareComponents(componentTypes);
        /// <summary>
        /// Compares a list of component types to the types defining this EntityQuery.
        /// </summary>
        /// <remarks>Only required types in the query are used as the basis for the comparison.
        /// If you include types that the query excludes or only includes as optional,
        /// the comparison returns false. Do not include the <see cref="Entity"/> type, which
        /// is included implicitly.</remarks>
        /// <param name="componentTypes">An array of ComponentType objects.</param>
        /// <returns>True, if the list of types, including any read/write access specifiers,
        /// matches the list of required component types of this EntityQuery.</returns>
        public bool CompareComponents(NativeArray<ComponentType> componentTypes) => _GetImpl()->CompareComponents(componentTypes);
        /// <summary>
        /// Compares a query description to the description defining this EntityQuery.
        /// </summary>
        /// <remarks>The `All`, `Any`, and `None` components in the query description are
        /// compared to the corresponding list in this EntityQuery.</remarks>
        /// <param name="queryDesc">The query description to compare.</param>
        /// <returns>True, if the query description contains the same components with the same
        /// read/write access modifiers as this EntityQuery.</returns>
        public bool CompareQuery(EntityQueryDesc[] queryDesc) => _GetImpl()->CompareQuery(queryDesc);
        /// <summary>
        /// Resets this EntityQuery's filter.
        /// </summary>
        /// <remarks>
        /// Removes references to shared component data, if applicable, then resets the filter type to None.
        /// </remarks>
        public void ResetFilter() => _GetImpl()->ResetFilter();
        /// <summary>
        /// Filters this EntityQuery so that it only selects entities with shared component values
        /// matching the values specified by the `sharedComponent1` parameter.
        /// </summary>
        /// <param name="sharedComponent1">The shared component values on which to filter.</param>
        /// <typeparam name="SharedComponent1">The type of shared component. (The type must also be
        /// one of the types used to create the EntityQuery.</typeparam>
        public void SetSharedComponentFilter<SharedComponent1>(SharedComponent1 sharedComponent1)            where SharedComponent1 : struct, ISharedComponentData
            => _GetImpl()->SetSharedComponentFilter<SharedComponent1>(sharedComponent1);
        /// <summary>
        /// Filters this EntityQuery based on the values of two separate shared components.
        /// </summary>
        /// <remarks>
        /// The filter only selects entities for which both shared component values
        /// specified by the `sharedComponent1` and `sharedComponent2` parameters match.
        /// </remarks>
        /// <param name="sharedComponent1">Shared component values on which to filter.</param>
        /// <param name="sharedComponent2">Shared component values on which to filter.</param>
        /// <typeparam name="SharedComponent1">The type of shared component. (The type must also be
        /// one of the types used to create the EntityQuery.</typeparam>
        /// <typeparam name="SharedComponent2">The type of shared component. (The type must also be
        /// one of the types used to create the EntityQuery.</typeparam>
        public void SetSharedComponentFilter<SharedComponent1, SharedComponent2>(SharedComponent1 sharedComponent1,
            SharedComponent2 sharedComponent2)            where SharedComponent1 : struct, ISharedComponentData
            where SharedComponent2 : struct, ISharedComponentData
            => _GetImpl()->SetSharedComponentFilter<SharedComponent1, SharedComponent2>(sharedComponent1, sharedComponent2);
        /// <summary>
        /// Filters out entities in chunks for which the specified component has not changed.
        /// </summary>
        /// <remarks>
        ///     Saves a given ComponentType's index in RequiredComponents in this group's Changed filter.
        /// </remarks>
        /// <param name="componentType">ComponentType to mark as changed on this EntityQuery's filter.</param>
        public void SetChangedVersionFilter(ComponentType componentType) => _GetImpl()->SetChangedVersionFilter(componentType);
        internal void SetChangedFilterRequiredVersion(uint requiredVersion) => _GetImpl()->SetChangedFilterRequiredVersion(requiredVersion);
        public void SetChangedVersionFilter(ComponentType[] componentType) => _GetImpl()->SetChangedVersionFilter(componentType);
        public void AddChangedVersionFilter(ComponentType componentType) => _GetImpl()->AddChangedVersionFilter(componentType);
        public void AddSharedComponentFilter<SharedComponent>(SharedComponent sharedComponent)            where SharedComponent : struct, ISharedComponentData
            => _GetImpl()->AddSharedComponentFilter<SharedComponent>(sharedComponent);
        /// <summary>
        /// Ensures all jobs running on this EntityQuery complete.
        /// </summary>
        /// <remarks>An entity query uses jobs internally when required to create arrays of
        /// entities and chunks. This function completes those jobs and returns when they are finished.
        /// </remarks>
        public void CompleteDependency() => _GetImpl()->CompleteDependency();
        /// <summary>
        /// Combines all dependencies in this EntityQuery into a single JobHandle.
        /// </summary>
        /// <remarks>An entity query uses jobs internally when required to create arrays of
        /// entities and chunks.</remarks>
        /// <returns>JobHandle that represents the combined dependencies of this EntityQuery</returns>
        public JobHandle GetDependency() => _GetImpl()->GetDependency();
        /// <summary>
        /// Adds another job handle to this EntityQuery's dependencies.
        /// </summary>
        /// <remarks>An entity query uses jobs internally when required to create arrays of
        /// entities and chunks. This junction adds an external job as a dependency for those
        /// internal jobs.</remarks>
        public JobHandle AddDependency(JobHandle job) => _GetImpl()->AddDependency(job);
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetCombinedComponentOrderVersion() => _GetImpl()->GetCombinedComponentOrderVersion();
        internal bool AddReaderWritersToLists(ref UnsafeIntList reading, ref UnsafeIntList writing) => _GetImpl()->AddReaderWritersToLists(ref reading, ref writing);
        /// <summary>
        /// Syncs the needed types for the filter.
        /// For every type that is change filtered we need to CompleteWriteDependency to avoid race conditions on the
        /// change version of those types
        /// </summary>
        internal void SyncFilterTypes() => _GetImpl()->SyncFilterTypes();
        /// <summary>
        /// Syncs the needed types for the filter using the types in UnsafeMatchingArchetypePtrList
        /// This version is used when the EntityQuery is not known
        /// </summary>
        internal static void SyncFilterTypes(ref UnsafeMatchingArchetypePtrList matchingArchetypes, ref EntityQueryFilter filter, ComponentDependencyManager* safetyManager) => EntityQueryImpl.SyncFilterTypes(ref matchingArchetypes, ref filter, safetyManager);
        /// <summary>
        /// Reports whether this entity query has a filter applied to it.
        /// </summary>
        /// <returns>Returns true if the query has a filter, returns false if the query does not have a filter.</returns>
        public bool HasFilter() => _GetImpl()->HasFilter();
        [Obsolete("CreateArchetypeChunkArray with out JobHandle parameter renamed to CreateArchetypeChunkArrayAsync (RemovedAfter 2020-04-13). (UnityUpgradable) -> CreateArchetypeChunkArrayAsync(*)", false)]
        public NativeArray<ArchetypeChunk> CreateArchetypeChunkArray(Allocator allocator, out JobHandle jobhandle) => CreateArchetypeChunkArrayAsync(allocator, out jobhandle);
        [Obsolete("ToEntityArray with out JobHandle parameter renamed to ToEntityArrayAsync (RemovedAfter 2020-04-13). (UnityUpgradable) -> ToEntityArrayAsync(*)", false)]
        public NativeArray<Entity> ToEntityArray(Allocator allocator, out JobHandle jobhandle) => ToEntityArrayAsync(allocator, out jobhandle);
        [Obsolete("ToComponentDataArray with out JobHandle parameter renamed to ToComponentDataArrayAsync (RemovedAfter 2020-04-13). (UnityUpgradable) -> ToComponentDataArrayAsync(*)", false)]
        public NativeArray<T> ToComponentDataArray<T>(Allocator allocator, out JobHandle jobhandle) where T : struct, IComponentData  => ToComponentDataArrayAsync<T>(allocator, out jobhandle);
        [Obsolete("CopyFromComponentDataArray with out JobHandle parameter renamed to CopyFromComponentDataArrayAsync (RemovedAfter 2020-04-13). (UnityUpgradable) -> CopyFromComponentDataArrayAsync(*)", false)]
        public void CopyFromComponentDataArray<T>(NativeArray<T> componentDataArray, out JobHandle jobhandle) where T : struct, IComponentData  => CopyFromComponentDataArrayAsync<T>(componentDataArray, out jobhandle);

        /// <summary>
        ///  Internal gen impl
        /// </summary>
        /// <returns></returns>
        internal EntityQueryImpl* _GetImpl()
        {
            _CheckSafetyHandle();
            return __impl;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void _CheckSafetyHandle()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(__safety);
#endif
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void _CreateSafetyHandle(ref EntityQuery _s)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            _s.__safety = AtomicSafetyHandle.Create();
#endif
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle __safety;
#endif

        internal EntityQueryImpl* __impl;
        internal ulong __seqno;

        // Temporarily allow conversion from null reference to allow existing packages to compile.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("EntityQuery is a struct. Please use `default` instead of `null`. (RemovedAfter 2020-07-01)")]
        public static implicit operator EntityQuery(EntityQueryNullShim? shim) => default(EntityQuery);

        public static bool operator==(EntityQuery lhs, EntityQuery rhs)
        {
            return lhs.__seqno == rhs.__seqno;
        }

        public static bool operator!=(EntityQuery lhs, EntityQuery rhs)
        {
            return !(lhs == rhs);
        }
    }


#if !UNITY_DISABLE_MANAGED_COMPONENTS
    public static unsafe class EntityQueryManagedComponentExtensions
    {
        /// <summary>
        /// Gets the value of a singleton component.
        /// </summary>
        /// <remarks>A singleton component is a component of which only one instance exists that satisfies this query.</remarks>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>A copy of the singleton component.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <seealso cref="SetSingleton{T}(T)"/>
        /// <seealso cref="GetSingletonEntity"/>
        /// <seealso cref="ComponentSystemBase.GetSingleton{T}"/>
        public static T GetSingleton<T>(this EntityQuery query) where T : class, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();
            var impl = query._GetImpl();
            impl->_Access->DependencyManager->CompleteWriteDependencyNoChecks(typeIndex);
            int managedComponentIndex;

            // Fast path with no filter & assuming this is a simple query with just one singleton component
            if (!impl->_Filter.RequiresMatchesFilter && impl->_QueryData->RequiredComponentsCount <= 2 && impl->_QueryData->RequiredComponents[1].TypeIndex == typeIndex)
            {
                var archetypeIndex = impl->GetFirstArchetypeIndexWithEntity(out var archetypeEntityCount);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (archetypeEntityCount != 1)
                    throw new InvalidOperationException($"GetSingleton<{typeof(T)}>() requires that exactly one {typeof(T)} exist that match this query, but there are {archetypeEntityCount}.");
#endif
                var match = impl->_QueryData->MatchingArchetypes.Ptr[archetypeIndex];
                managedComponentIndex = *(int*)ChunkIterationUtility.GetChunkComponentDataPtr(match->Archetype->Chunks.p[0], true,
                    match->IndexInArchetype[1], impl->_Access->EntityComponentStore->GlobalSystemVersion);
                return (T)impl->_Access->ManagedComponentStore.GetManagedComponent(managedComponentIndex);
            }


#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var queryEntityCount = query.CalculateEntityCount();
            if (queryEntityCount != 1)
                throw new InvalidOperationException($"GetSingleton<{typeof(T)}>() requires that exactly one {typeof(T)} exist that match this query, but there are {queryEntityCount}.");
#endif

            var iterator = query.GetArchetypeChunkIterator();
            iterator.MoveNext();
            managedComponentIndex = *(int*)iterator.GetCurrentChunkComponentDataPtr(true, 1);
            return (T)impl->_Access->ManagedComponentStore.GetManagedComponent(managedComponentIndex);
        }

        /// <summary>
        /// Sets the value of a singleton component.
        /// </summary>
        /// <remarks>
        /// For a component to be a singleton, there can be only one instance of that component
        /// that satisfies this query.
        ///
        /// **Note:** singletons are otherwise normal entities. The EntityQuery and <see cref="ComponentSystemBase"/>
        /// singleton functions add checks that you have not created two instances of a
        /// type that can be accessed by this singleton query, but other APIs do not prevent such accidental creation.
        ///
        /// To create a singleton, create an entity with the singleton component.
        ///
        /// For example, if you had a component defined as:
        ///
        /// <example>
        /// <code lang="csharp" source="../../DocCodeSamples.Tests/EntityQueryExamples.cs" region="singleton-type-example" title="Singleton"/>
        /// </example>
        ///
        /// You could create a singleton as follows:
        ///
        /// <example>
        /// <code lang="csharp" source="../../DocCodeSamples.Tests/EntityQueryExamples.cs" region="create-singleton" title="Create Singleton"/>
        /// </example>
        ///
        /// To update the singleton component after creation, you can use an EntityQuery object that
        /// selects the singleton entity and call this `SetSingleton()` function:
        ///
        /// <example>
        /// <code lang="csharp" source="../../DocCodeSamples.Tests/EntityQueryExamples.cs" region="set-singleton" title="Set Singleton"/>
        /// </example>
        ///
        /// You can set and get the singleton value from a system: see <seealso cref="ComponentSystemBase.SetSingleton{T}(T)"/>
        /// and <seealso cref="ComponentSystemBase.GetSingleton{T}"/>.
        /// </remarks>
        /// <param name="value">An instance of type T containing the values to set.</param>
        /// <typeparam name="T">The component type.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown if more than one instance of this component type
        /// exists in the world or the component type appears in more than one archetype.</exception>
        /// <seealso cref="GetSingleton{T}"/>
        /// <seealso cref="EntityQuery.GetSingletonEntity"/>
        public static void SetSingleton<T>(this EntityQuery query, T value) where T : class, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();
            var impl = query._GetImpl();
            var access = impl->_Access;

            access->DependencyManager->CompleteWriteDependencyNoChecks(typeIndex);
            int* managedComponentIndex;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (value != null && value.GetType() != typeof(T))
                throw new ArgumentException($"Assigning component value is of type: {value.GetType()} but the expected component type is: {typeof(T)}");
#endif

            if (!impl->_Filter.RequiresMatchesFilter && impl->_QueryData->RequiredComponentsCount <= 2 && impl->_QueryData->RequiredComponents[1].TypeIndex == typeIndex)
            {
                var archetypeIndex = impl->GetFirstArchetypeIndexWithEntity(out var entityCount);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (entityCount != 1)
                    throw new InvalidOperationException($"SetSingleton<{typeof(T)}>() requires that exactly one {typeof(T)} exist that match this query, but there are {entityCount}.");
#endif
                var match = impl->_QueryData->MatchingArchetypes.Ptr[archetypeIndex];
                managedComponentIndex = (int*)ChunkIterationUtility.GetChunkComponentDataPtr(match->Archetype->Chunks.p[0], true,
                    match->IndexInArchetype[1], access->EntityComponentStore->GlobalSystemVersion);
                access->ManagedComponentStore.UpdateManagedComponentValue(managedComponentIndex, value, ref *access->EntityComponentStore);
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var queryEntityCount = query.CalculateEntityCount();
            if (queryEntityCount != 1)
                throw new InvalidOperationException($"SetSingleton<{typeof(T)}>() requires that exactly one {typeof(T)} exist that match this query, but there are {queryEntityCount}.");
#endif

            var iterator = query.GetArchetypeChunkIterator();
            iterator.MoveNext();
            managedComponentIndex = (int*)iterator.GetCurrentChunkComponentDataPtr(true, 1);
            access->ManagedComponentStore.UpdateManagedComponentValue(managedComponentIndex, value, ref *access->EntityComponentStore);
        }
    }
#endif
}

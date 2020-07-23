using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities
{
    public unsafe partial struct EntityManager
    {
        // ----------------------------------------------------------------------------------------------------------
        // PUBLIC
        // ----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a EntityQuery from an array of component types.
        /// </summary>
        /// <param name="requiredComponents">An array containing the component types.</param>
        /// <returns>The EntityQuery derived from the specified array of component types.</returns>
        /// <seealso cref="EntityQueryDesc"/>
        public EntityQuery CreateEntityQuery(params ComponentType[] requiredComponents)
        {
            var access = GetCheckedEntityDataAccess();
            fixed(ComponentType* requiredComponentsPtr = requiredComponents)
            {
                return access->EntityQueryManager->CreateEntityQuery(access, requiredComponentsPtr, requiredComponents.Length);
            }
        }

        /// <summary>
        /// Creates a EntityQuery from an EntityQueryDesc.
        /// </summary>
        /// <param name="queriesDesc">A queryDesc identifying a set of component types.</param>
        /// <returns>The EntityQuery corresponding to the queryDesc.</returns>
        public EntityQuery CreateEntityQuery(params EntityQueryDesc[] queriesDesc)
        {
            var access = GetCheckedEntityDataAccess();
            return access->EntityQueryManager->CreateEntityQuery(access, queriesDesc);
        }

        /// <summary>
        /// Gets all the chunks managed by this EntityManager.
        /// </summary>
        /// <remarks>
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before getting these chunks and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="allocator">The type of allocation for creating the NativeArray to hold the ArchetypeChunk
        /// objects.</param>
        /// <returns>An array of ArchetypeChunk objects referring to all the chunks in the <see cref="World"/>.</returns>
        public NativeArray<ArchetypeChunk> GetAllChunks(Allocator allocator = Allocator.TempJob)
        {
            var access = GetCheckedEntityDataAccess();
            access->BeforeStructuralChange();
            return access->ManagedEntityDataAccess.m_UniversalQuery.CreateArchetypeChunkArray(allocator);
        }

        /// <summary>
        /// Gets all the archetypes.
        /// </summary>
        /// <remarks>The function adds the archetype objects to the existing contents of the list.
        /// The list is not cleared.</remarks>
        /// <param name="allArchetypes">A native list to receive the EntityArchetype objects.</param>
        public void GetAllArchetypes(NativeList<EntityArchetype> allArchetypes)
        {
            var access = GetCheckedEntityDataAccess();
            for (var i = 0; i < access->EntityComponentStore->m_Archetypes.Length; ++i)
            {
                var archetype = access->EntityComponentStore->m_Archetypes.Ptr[i];
                var entityArchetype = new EntityArchetype()
                {
                    Archetype = archetype,
                    #if ENABLE_UNITY_COLLECTIONS_CHECKS
                    _DebugComponentStore = access->EntityComponentStore
                    #endif
                };
                allArchetypes.Add(entityArchetype);
            }
        }

        /// <summary>
        /// Gets an <see cref="EntityQueryMask"/> that can be used to quickly match if an entity belongs to an EntityQuery.
        /// There is a maximum limit of 1024 EntityQueryMasks that can be created.  EntityQueryMasks cannot be created
        /// from EntityQueries with filters.
        /// </summary>
        /// <param name="query">The EntityQuery that describes the EntityQueryMask.</param>
        /// <returns>The EntityQueryMask corresponding to the EntityQuery.</returns>
        public EntityQueryMask GetEntityQueryMask(EntityQuery query)
        {
            var access = GetCheckedEntityDataAccess();
            var queryImpl = query._GetImpl();

            if (query.HasFilter())
                throw new Exception("GetEntityQueryMask can only be called on an EntityQuery without a filter applied to it."
                    + "  You can call EntityQuery.ResetFilter to remove the filters from an EntityQuery.");

            if (queryImpl->_QueryData->EntityQueryMask.IsCreated())
                return queryImpl->_QueryData->EntityQueryMask;

            if (access->EntityQueryManager->m_EntityQueryMasksAllocated >= 1024)
                throw new Exception("You have reached the limit of 1024 unique EntityQueryMasks, and cannot generate any more.");

            var mask = new EntityQueryMask(
                (byte)(access->EntityQueryManager->m_EntityQueryMasksAllocated / 8),
                (byte)(1 << (access->EntityQueryManager->m_EntityQueryMasksAllocated % 8)),
                access->EntityComponentStore);

            access->EntityQueryManager->m_EntityQueryMasksAllocated++;

            for (var i = 0; i < queryImpl->_QueryData->MatchingArchetypes.Length; ++i)
            {
                queryImpl->_QueryData->MatchingArchetypes.Ptr[i]->Archetype->QueryMaskArray[mask.Index] |= mask.Mask;
            }

            queryImpl->_QueryData->EntityQueryMask = mask;

            return mask;
        }

        // ----------------------------------------------------------------------------------------------------------
        // INTERNAL
        // ----------------------------------------------------------------------------------------------------------

        internal EntityQuery CreateEntityQuery(ComponentType* requiredComponents, int count)
        {
            var access = GetCheckedEntityDataAccess();
            return access->EntityQueryManager->CreateEntityQuery(access, requiredComponents, count);
        }

        bool TestMatchingArchetypeAny(Archetype* archetype, ComponentType* anyTypes, int anyCount)
        {
            if (anyCount == 0) return true;

            var componentTypes = archetype->Types;
            var componentTypesCount = archetype->TypesCount;
            for (var i = 0; i < componentTypesCount; i++)
            {
                var componentTypeIndex = componentTypes[i].TypeIndex;
                for (var j = 0; j < anyCount; j++)
                {
                    var anyTypeIndex = anyTypes[j].TypeIndex;
                    if (componentTypeIndex == anyTypeIndex) return true;
                }
            }

            return false;
        }

        bool TestMatchingArchetypeNone(Archetype* archetype, ComponentType* noneTypes, int noneCount)
        {
            var componentTypes = archetype->Types;
            var componentTypesCount = archetype->TypesCount;
            for (var i = 0; i < componentTypesCount; i++)
            {
                var componentTypeIndex = componentTypes[i].TypeIndex;
                for (var j = 0; j < noneCount; j++)
                {
                    var noneTypeIndex = noneTypes[j].TypeIndex;
                    if (componentTypeIndex == noneTypeIndex) return false;
                }
            }

            return true;
        }

        bool TestMatchingArchetypeAll(Archetype* archetype, ComponentType* allTypes, int allCount)
        {
            var componentTypes = archetype->Types;
            var componentTypesCount = archetype->TypesCount;
            var foundCount = 0;
            var disabledTypeIndex = TypeManager.GetTypeIndex<Disabled>();
            var prefabTypeIndex = TypeManager.GetTypeIndex<Prefab>();
            var requestedDisabled = false;
            var requestedPrefab = false;
            for (var i = 0; i < componentTypesCount; i++)
            {
                var componentTypeIndex = componentTypes[i].TypeIndex;
                for (var j = 0; j < allCount; j++)
                {
                    var allTypeIndex = allTypes[j].TypeIndex;
                    if (allTypeIndex == disabledTypeIndex)
                        requestedDisabled = true;
                    if (allTypeIndex == prefabTypeIndex)
                        requestedPrefab = true;
                    if (componentTypeIndex == allTypeIndex) foundCount++;
                }
            }

            if (archetype->Disabled && (!requestedDisabled))
                return false;
            if (archetype->Prefab && (!requestedPrefab))
                return false;

            return foundCount == allCount;
        }

        /// <summary>
        /// Check if an entity query is still valid
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Return true if the specified query handle is still valid (and can be disposed)</returns>
        public bool IsQueryValid(EntityQuery query)
        {
            return GetCheckedEntityDataAccess()->AliveEntityQueries.ContainsKey((ulong)(IntPtr)query.__impl);
        }
    }
}

using System;
using Unity.Assertions;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Entities
{
    internal unsafe partial struct EntityComponentStore
    {
        // ----------------------------------------------------------------------------------------------------------
        // PUBLIC
        // ----------------------------------------------------------------------------------------------------------

        public bool AddComponent(Entity entity, ComponentType type)
        {
            var dstChunk = GetChunkWithEmptySlotsWithAddedComponent(entity, type);
            if (dstChunk == null)
                return false;

            Move(entity, dstChunk);
            return true;
        }

        public bool RemoveComponent(Entity entity, ComponentType type)
        {
            var dstChunk = GetChunkWithEmptySlotsWithRemovedComponent(entity, type);
            if (dstChunk == null)
                return false;

            Move(entity, dstChunk);
            return true;
        }

        bool AddComponent(EntityBatchInChunk entityBatchInChunk, ComponentType componentType, int sharedComponentIndex = 0)
        {
            var srcChunk = entityBatchInChunk.Chunk;
            var archetypeChunkFilter = GetArchetypeChunkFilterWithAddedComponent(srcChunk, componentType, sharedComponentIndex);
            if (archetypeChunkFilter.Archetype == null)
                return false;

            Move(entityBatchInChunk, ref archetypeChunkFilter);
            return true;
        }

        bool RemoveComponent(EntityBatchInChunk entityBatchInChunk, ComponentType componentType)
        {
            var srcChunk = entityBatchInChunk.Chunk;
            var archetypeChunkFilter = GetArchetypeChunkFilterWithRemovedComponent(srcChunk, componentType);
            if (archetypeChunkFilter.Archetype == null)
                return false;

            Move(entityBatchInChunk, ref archetypeChunkFilter);
            return true;
        }

        public void AddComponent(ArchetypeChunk* chunks, int chunkCount, ComponentType componentType, int sharedComponentIndex = 0)
        {
            Archetype* prevArchetype = null;
            Archetype* dstArchetype = null;
            int indexInTypeArray = 0;

            for (int i = 0; i < chunkCount; i++)
            {
                var chunk = chunks[i].m_Chunk;
                var srcArchetype = chunk->Archetype;
                if (prevArchetype != chunk->Archetype)
                {
                    dstArchetype = GetArchetypeWithAddedComponent(srcArchetype, componentType, &indexInTypeArray);
                    prevArchetype = chunk->Archetype;
                }

                if (dstArchetype == null)
                    continue;

                var archetypeChunkFilter = GetArchetypeChunkFilterWithAddedComponent(chunk, dstArchetype, indexInTypeArray, componentType, sharedComponentIndex);

                Move(chunk, ref archetypeChunkFilter);
            }
        }

        public void RemoveComponent(ArchetypeChunk* chunks, int chunkCount, ComponentType componentType)
        {
            Archetype* prevArchetype = null;
            Archetype* dstArchetype = null;
            int indexInTypeArray = 0;

            for (int i = 0; i < chunkCount; i++)
            {
                var chunk = chunks[i].m_Chunk;
                var srcArchetype = chunk->Archetype;

                if (prevArchetype != chunk->Archetype)
                {
                    dstArchetype = GetArchetypeWithRemovedComponent(srcArchetype, componentType, &indexInTypeArray);
                    prevArchetype = chunk->Archetype;
                }

                if (dstArchetype == srcArchetype)
                    continue;

                var archetypeChunkFilter = GetArchetypeChunkFilterWithRemovedComponent(chunk, dstArchetype, indexInTypeArray, componentType);

                Move(chunk, ref archetypeChunkFilter);
            }
        }

        public void AddComponent(UnsafeList* sortedEntityBatchList, ComponentType type, int existingSharedComponentIndex)
        {
            Assert.IsFalse(type.IsChunkComponent);

            // Reverse order so that batch indices do not change while iterating.
            for (int i = sortedEntityBatchList->Length - 1; i >= 0; i--)
                AddComponent(((EntityBatchInChunk*)sortedEntityBatchList->Ptr)[i], type, existingSharedComponentIndex);
        }

        public void RemoveComponent(UnsafeList* sortedEntityBatchList, ComponentType type)
        {
            Assert.IsFalse(type.IsChunkComponent);

            // Reverse order so that batch indices do not change while iterating.
            for (int i = sortedEntityBatchList->Length - 1; i >= 0; i--)
                RemoveComponent(((EntityBatchInChunk*)sortedEntityBatchList->Ptr)[i], type);
        }

        public void AddComponents(Entity entity, ComponentTypes types)
        {
            var archetypeChunkFilter = GetArchetypeChunkFilterWithAddedComponents(GetChunk(entity), types);
            if (archetypeChunkFilter.Archetype == null)
                return;

            Move(entity, ref archetypeChunkFilter);
        }

        public bool AddComponentWithValidation(Entity entity, ComponentType componentType)
        {
            if (HasComponent(entity, componentType))
                return false;

            AssertCanAddComponent(entity, componentType);
            AddComponent(entity, componentType);

            return true;
        }

        public void AddComponentWithValidation(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter,
            ComponentType componentType, ComponentDependencyManager* dependencyManager)
        {
            AssertCanAddComponent(archetypeList, componentType);

            using (var chunks = ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList,
                Collections.Allocator.TempJob,
                ref filter, dependencyManager))
            {
                if (chunks.Length == 0)
                    return;

                //@TODO the fast path for a chunk that contains a single entity is only possible if the chunk doesn't have a Locked Entity Order
                //but we should still be allowed to add zero sized components to chunks with a Locked Entity Order, even ones that only contain a single entity

                /*
                if ((chunks.Length == 1) && (chunks[0].Count == 1))
                {
                    var entityPtr = (Entity*) chunks[0].m_Chunk->Buffer;
                    StructuralChange.AddComponentEntity(EntityComponentStore, entityPtr, componentType.TypeIndex);
                }
                else
                {
                */
                AddComponent((ArchetypeChunk*)NativeArrayUnsafeUtility.GetUnsafePtr(chunks),
                    chunks.Length, componentType);
                /*
                }
                */
            }
        }

        public bool RemoveComponentWithValidation(Entity entity, ComponentType componentType)
        {
            ValidateEntity(entity);
            AssertCanRemoveComponent(entity, componentType);
            var removed = RemoveComponent(entity, componentType);

            return removed;
        }

        public void RemoveComponentWithValidation(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter,
            ComponentType componentType, ComponentDependencyManager* dependencyManager)
        {
            using (var chunks = ChunkIterationUtility.CreateArchetypeChunkArray(archetypeList, Collections.Allocator.TempJob, ref filter, dependencyManager))
            {
                RemoveComponentWithValidation(chunks, componentType);
            }
        }

        public void RemoveComponentWithValidation(Collections.NativeArray<ArchetypeChunk> chunks, ComponentType componentType)
        {
            if (chunks.Length == 0)
                return;

            RemoveComponent((ArchetypeChunk*)NativeArrayUnsafeUtility.GetUnsafePtr(chunks), chunks.Length, componentType);
        }

        public void SetSharedComponentDataIndex(Entity entity, ComponentType componentType, int dstSharedComponentDataIndex)
        {
            var archetypeChunkFilter = GetArchetypeChunkFilterWithChangedSharedComponent(GetChunk(entity), componentType, dstSharedComponentDataIndex);
            if (archetypeChunkFilter.Archetype == null)
                return;

            ChunkDataUtility.SetSharedComponentDataIndex(entity, archetypeChunkFilter.Archetype, archetypeChunkFilter.SharedComponentValues, componentType.TypeIndex);
        }

        // Note previously called SetArchetype: SetArchetype is used internally to refer to the function which only creates the cross-reference between the
        // entity id and the archetype (m_ArchetypeByEntity). This is not "Setting" the archetype, it is moving the components to a different archetype.
        public void Move(Entity entity, Archetype* dstArchetype)
        {
            var archetypeChunkFilter = GetArchetypeChunkFilterWithChangedArchetype(GetChunk(entity), dstArchetype);
            if (archetypeChunkFilter.Archetype == null)
                return;

            Move(entity, ref archetypeChunkFilter);
        }

        public void Move(Entity entity, Archetype* archetype, SharedComponentValues sharedComponentValues)
        {
            var archetypeChunkFilter = new ArchetypeChunkFilter(archetype, sharedComponentValues);
            Move(entity, ref archetypeChunkFilter);
        }

        // ----------------------------------------------------------------------------------------------------------
        // INTERNAL
        // ----------------------------------------------------------------------------------------------------------

        void Move(Entity entity, Chunk* dstChunk)
        {
            var srcEntityInChunk = GetEntityInChunk(entity);
            var srcChunk = srcEntityInChunk.Chunk;
            var srcChunkIndex = srcEntityInChunk.IndexInChunk;
            var entityBatch = new EntityBatchInChunk { Chunk = srcChunk, Count = 1, StartIndex = srcChunkIndex };

            Move(entityBatch, dstChunk);
        }

        void Move(Entity entity, ref ArchetypeChunkFilter archetypeChunkFilter)
        {
            var srcEntityInChunk = GetEntityInChunk(entity);
            var entityBatch = new EntityBatchInChunk { Chunk = srcEntityInChunk.Chunk, Count = 1, StartIndex = srcEntityInChunk.IndexInChunk };

            Move(entityBatch, ref archetypeChunkFilter);
        }

        void Move(Chunk* srcChunk, ref ArchetypeChunkFilter archetypeChunkFilter)
        {
            if (archetypeChunkFilter.Archetype->SystemStateCleanupComplete)
            {
                ChunkDataUtility.Deallocate(srcChunk);
                return;
            }

            var srcArchetype = srcChunk->Archetype;
            if (ChunkDataUtility.AreLayoutCompatible(srcArchetype, archetypeChunkFilter.Archetype))
            {
                fixed(int* sharedComponentValues = archetypeChunkFilter.SharedComponentValues)
                {
                    ChunkDataUtility.ChangeArchetypeInPlace(srcChunk, archetypeChunkFilter.Archetype, sharedComponentValues);
                }
                return;
            }

            var entityBatch = new EntityBatchInChunk { Chunk = srcChunk, Count = srcChunk->Count, StartIndex = 0 };
            Move(entityBatch, ref archetypeChunkFilter);
        }

        void Move(EntityBatchInChunk entityBatchInChunk, ref ArchetypeChunkFilter archetypeChunkFilter)
        {
            var systemStateCleanupComplete = archetypeChunkFilter.Archetype->SystemStateCleanupComplete;

            var srcChunk = entityBatchInChunk.Chunk;
            var srcRemainingCount = entityBatchInChunk.Count;
            var startIndex = entityBatchInChunk.StartIndex;

            if ((srcRemainingCount == srcChunk->Count) && systemStateCleanupComplete)
            {
                ChunkDataUtility.Deallocate(srcChunk);
                return;
            }

            while (srcRemainingCount > 0)
            {
                var dstChunk = GetChunkWithEmptySlots(ref archetypeChunkFilter);
                var dstCount = Move(new EntityBatchInChunk { Chunk = srcChunk, Count = srcRemainingCount, StartIndex = startIndex }, dstChunk);
                srcRemainingCount -= dstCount;
            }
        }

        // ----------------------------------------------------------------------------------------------------------
        // Core, self-contained functions to change chunks. No other functions should actually move data from
        // one Chunk to another, or otherwise change the structure of a Chunk after creation.
        // ----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Move subset of chunk data into another chunk.
        /// - Chunks can be of same archetype (but differ by shared component values)
        /// - Returns number moved. Caller handles if less than indicated in srcBatch.
        /// </summary>
        /// <returns></returns>
        int Move(in EntityBatchInChunk srcBatch, Chunk* dstChunk)
        {
            var srcChunk = srcBatch.Chunk;
            var srcCount = math.min(dstChunk->UnusedCount, srcBatch.Count);
            var srcStartIndex = srcBatch.StartIndex + srcBatch.Count - srcCount;

            var partialSrcBatch = new EntityBatchInChunk
            {
                Chunk = srcChunk,
                StartIndex = srcStartIndex,
                Count = srcCount
            };

            ChunkDataUtility.Clone(partialSrcBatch, dstChunk);
            ChunkDataUtility.Remove(partialSrcBatch);

            return srcCount;
        }
    }
}

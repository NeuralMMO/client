using System;
using System.Runtime.CompilerServices;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Unity.Entities
{
    internal unsafe partial struct EntityComponentStore
    {
        // ----------------------------------------------------------------------------------------------------------
        // PUBLIC
        // ----------------------------------------------------------------------------------------------------------

        public void CreateEntities(Archetype* archetype, Entity* entities, int count)
        {
            var archetypeChunkFilter = new ArchetypeChunkFilter();
            archetypeChunkFilter.Archetype = archetype;

            while (count != 0)
            {
                var chunk = GetChunkWithEmptySlots(ref archetypeChunkFilter);
                var allocateCount = math.min(count, chunk->UnusedCount);

                ChunkDataUtility.Allocate(chunk, entities, allocateCount);

                entities += allocateCount;
                count -= allocateCount;
            }
        }

        public void DestroyEntities(NativeArray<ArchetypeChunk> chunkArray)
        {
            var chunks = (ArchetypeChunk*)chunkArray.GetUnsafeReadOnlyPtr();
            for (int i = 0; i != chunkArray.Length; i++)
            {
                var chunk = chunks[i].m_Chunk;
                DestroyBatch(new EntityBatchInChunk {Chunk = chunk, StartIndex = 0, Count = chunk->Count});
            }
        }

        public void DestroyEntities(Entity* entities, int count)
        {
            var entityIndex = 0;

            var additionalDestroyList = new UnsafeList(Allocator.Persistent);
            int minDestroyStride = int.MaxValue;
            int maxDestroyStride = 0;

            while (entityIndex != count)
            {
                var entityBatchInChunk =
                    GetFirstEntityBatchInChunk(entities + entityIndex, count - entityIndex);
                var chunk = entityBatchInChunk.Chunk;
                var batchCount = entityBatchInChunk.Count;
                var indexInChunk = entityBatchInChunk.StartIndex;

                if (chunk == null)
                {
                    entityIndex += batchCount;
                    continue;
                }

                AddToDestroyList(chunk, indexInChunk, batchCount, count, ref additionalDestroyList,
                    ref minDestroyStride, ref maxDestroyStride);

                DestroyBatch(new EntityBatchInChunk {Chunk = chunk, StartIndex = indexInChunk, Count = batchCount});

                entityIndex += batchCount;
            }

            // Apply additional destroys from any LinkedEntityGroup
            if (additionalDestroyList.Ptr != null)
            {
                var additionalDestroyPtr = (Entity*)additionalDestroyList.Ptr;
                // Optimal for destruction speed is if entities with same archetype/chunk are followed one after another.
                // So we lay out the to be destroyed objects assuming that the destroyed entities are "similar":
                // Reorder destruction by index in entityGroupArray...

                //@TODO: This is a very specialized fastpath that is likely only going to give benefits in the stress test.
                ///      Figure out how to make this more general purpose.
                if (minDestroyStride == maxDestroyStride)
                {
                    var reordered = (Entity*)UnsafeUtility.Malloc(additionalDestroyList.Length * sizeof(Entity), 16,
                        Allocator.TempJob);
                    int batchCount = additionalDestroyList.Length / minDestroyStride;
                    for (int i = 0; i != batchCount; i++)
                    {
                        for (int j = 0; j != minDestroyStride; j++)
                            reordered[j * batchCount + i] = additionalDestroyPtr[i * minDestroyStride + j];
                    }

                    DestroyEntities(reordered, additionalDestroyList.Length);
                    UnsafeUtility.Free(reordered, Allocator.TempJob);
                }
                else
                {
                    DestroyEntities(additionalDestroyPtr, additionalDestroyList.Length);
                }

                UnsafeUtility.Free(additionalDestroyPtr, Allocator.Persistent);
            }
        }

        public Entity CreateEntityWithValidation(EntityArchetype archetype)
        {
            Entity entity;
            AssertValidArchetype((EntityComponentStore*)UnsafeUtility.AddressOf(ref this), archetype);
            CreateEntities(archetype.Archetype, &entity, 1);
            return entity;
        }

        public void CreateEntityWithValidation(EntityArchetype archetype, Entity* outEntities, int count)
        {
            AssertValidArchetype((EntityComponentStore*)UnsafeUtility.AddressOf(ref this), archetype);
            CreateEntities(archetype.Archetype, outEntities, count);
        }

        public void InstantiateWithValidation(Entity srcEntity, Entity* outputEntities, int count)
        {
            AssertEntitiesExist(&srcEntity, 1);
            AssertCanInstantiateEntities(srcEntity, outputEntities, count);
            InstantiateEntities(srcEntity, outputEntities, count);
        }

        public void DestroyEntityWithValidation(Entity entity)
        {
            DestroyEntityWithValidation(&entity, 1);
        }

        public void DestroyEntityWithValidation(Entity* entities, int count)
        {
            AssertValidEntities(entities, count);
            DestroyEntities(entities, count);
        }

        [Obsolete("CreateChunks is deprecated. (RemovedAfter 2020-06-05)", false)]
        public void CreateChunks(Archetype* archetype, ArchetypeChunk* chunks, int chunksCount, int entityCount)
        {
            fixed(EntityComponentStore* entityComponentStore = &this)
            {
                int* sharedComponentValues = stackalloc int[archetype->NumSharedComponents];

                int chunkIndex = 0;
                while (entityCount != 0)
                {
                    #if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if (chunkIndex >= chunksCount)
                        throw new System.ArgumentException($"CreateChunks chunks array is not large enough to hold the array of chunks {chunksCount}.");
                    #endif

                    var chunk = GetCleanChunk(archetype, sharedComponentValues);
                    var allocateCount = math.min(entityCount, chunk->UnusedCount);

                    ChunkDataUtility.Allocate(chunk, allocateCount);

                    chunks[chunkIndex] = new ArchetypeChunk(chunk, entityComponentStore);

                    entityCount -= allocateCount;
                    chunkIndex++;
                }
                IncrementComponentTypeOrderVersion(archetype);
            }
        }

        public Chunk* GetCleanChunkNoMetaChunk(Archetype* archetype, SharedComponentValues sharedComponentValues)
        {
            var newChunk = AllocateChunk();
            ChunkDataUtility.AddEmptyChunk(archetype, newChunk, sharedComponentValues);

            return newChunk;
        }

        public Chunk* GetCleanChunk(Archetype* archetype, SharedComponentValues sharedComponentValues)
        {
            var newChunk = AllocateChunk();
            ChunkDataUtility.AddEmptyChunk(archetype, newChunk, sharedComponentValues);

            if (archetype->MetaChunkArchetype != null)
                CreateMetaEntityForChunk(newChunk);

            return newChunk;
        }

        public void InstantiateEntities(Entity srcEntity, Entity* outputEntities, int instanceCount)
        {
            if (HasComponent(srcEntity, m_LinkedGroupType))
            {
                var header = (BufferHeader*)GetComponentDataWithTypeRO(srcEntity, m_LinkedGroupType);
                var entityPtr = (Entity*)BufferHeader.GetElementPointer(header);
                var entityCount = header->Length;

                InstantiateEntitiesGroup(entityPtr, entityCount, outputEntities, true, instanceCount, true);
            }
            else
            {
                InstantiateEntitiesOne(srcEntity, outputEntities, instanceCount, null, 0, true);
            }
        }

        public void InstantiateEntities(Entity* srcEntity, Entity* outputEntities, int entityCount, bool removePrefab)
        {
            InstantiateEntitiesGroup(srcEntity, entityCount, outputEntities, false, 1, removePrefab);
        }

        // ----------------------------------------------------------------------------------------------------------
        // INTERNAL
        // ----------------------------------------------------------------------------------------------------------

        internal void DestroyMetaChunkEntity(Entity entity)
        {
            RemoveComponent(entity, m_ChunkHeaderComponentType);
            DestroyEntities(&entity, 1);
        }

        internal void CreateMetaEntityForChunk(Chunk* chunk)
        {
            fixed(EntityComponentStore* entityComponentStore = &this)
            {
                CreateEntities(chunk->Archetype->MetaChunkArchetype, &chunk->metaChunkEntity, 1);

                var chunkHeader = (ChunkHeader*)GetComponentDataWithTypeRW(chunk->metaChunkEntity, m_ChunkHeaderType, GlobalSystemVersion);

                chunkHeader->ArchetypeChunk = new ArchetypeChunk(chunk, entityComponentStore);
            }
        }

        struct InstantiateRemapChunk
        {
            public Chunk* Chunk;
            public int IndexInChunk;
            public int AllocatedCount;
            public int InstanceBeginIndex;
        }

        void AddToDestroyList(Chunk* chunk, int indexInChunk, int batchCount, int inputDestroyCount,
            ref UnsafeList entitiesList, ref int minBufferLength, ref int maxBufferLength)
        {
            int indexInArchetype = ChunkDataUtility.GetIndexInTypeArray(chunk->Archetype, m_LinkedGroupType);
            if (indexInArchetype != -1)
            {
                var baseHeader = ChunkDataUtility.GetComponentDataWithTypeRO(chunk, indexInChunk, m_LinkedGroupType);
                var stride = chunk->Archetype->SizeOfs[indexInArchetype];
                for (int i = 0; i != batchCount; i++)
                {
                    var header = (BufferHeader*)(baseHeader + stride * i);

                    var entityGroupCount = header->Length - 1;
                    if (entityGroupCount <= 0)
                        continue;

                    var entityGroupArray = (Entity*)BufferHeader.GetElementPointer(header) + 1;

                    if (entitiesList.Capacity == 0)
                        entitiesList.SetCapacity<Entity>(inputDestroyCount * entityGroupCount /*, Allocator.TempJob*/);
                    entitiesList.AddRange<Entity>(entityGroupArray, entityGroupCount /*, Allocator.TempJob*/);

                    minBufferLength = math.min(minBufferLength, entityGroupCount);
                    maxBufferLength = math.max(maxBufferLength, entityGroupCount);
                }
            }
        }

        void DestroyBatch(in EntityBatchInChunk batch)
        {
            var chunk = batch.Chunk;
            var archetype = chunk->Archetype;

            if (!archetype->SystemStateCleanupNeeded)
            {
                ChunkDataUtility.Deallocate(batch);
            }
            else
            {
                var startIndex = batch.StartIndex;
                var count = batch.Count;

                var systemStateResidueArchetype = archetype->SystemStateResidueArchetype;
                if (archetype == systemStateResidueArchetype)
                    return;

                var dstArchetypeChunkFilter = new ArchetypeChunkFilter();
                dstArchetypeChunkFilter.Archetype = systemStateResidueArchetype;

                if (RequiresBuildingResidueSharedComponentIndices(archetype, dstArchetypeChunkFilter.Archetype))
                {
                    BuildResidueSharedComponentIndices(archetype, dstArchetypeChunkFilter.Archetype, chunk->SharedComponentValues, dstArchetypeChunkFilter.SharedComponentValues);
                }
                else
                {
                    chunk->SharedComponentValues.CopyTo(dstArchetypeChunkFilter.SharedComponentValues, 0, archetype->NumSharedComponents);
                }

                if (count == chunk->Count)
                    Move(chunk, ref dstArchetypeChunkFilter);
                else
                    Move(new EntityBatchInChunk {Chunk = chunk, StartIndex = startIndex, Count = count}, ref dstArchetypeChunkFilter);
            }
        }

        bool RequiresBuildingResidueSharedComponentIndices(Archetype* srcArchetype,
            Archetype* dstArchetype)
        {
            return dstArchetype->NumSharedComponents > 0 &&
                dstArchetype->NumSharedComponents != srcArchetype->NumSharedComponents;
        }

        void BuildResidueSharedComponentIndices(Archetype* srcArchetype, Archetype* dstArchetype,
            SharedComponentValues srcSharedComponentValues, int* dstSharedComponentValues)
        {
            int oldFirstShared = srcArchetype->FirstSharedComponent;
            int newFirstShared = dstArchetype->FirstSharedComponent;
            int newCount = dstArchetype->NumSharedComponents;

            for (int oldIndex = 0, newIndex = 0; newIndex < newCount; ++newIndex, ++oldIndex)
            {
                var t = dstArchetype->Types[newIndex + newFirstShared];
                while (t != srcArchetype->Types[oldIndex + oldFirstShared])
                    ++oldIndex;
                dstSharedComponentValues[newIndex] = srcSharedComponentValues[oldIndex];
            }
        }

        int InstantiateEntitiesOne(Entity srcEntity, Entity* outputEntities, int instanceCount, InstantiateRemapChunk* remapChunks, int remapChunksCount, bool removePrefab)
        {
            var src = GetEntityInChunk(srcEntity);
            var srcArchetype = src.Chunk->Archetype;

            var dstArchetype = removePrefab ? srcArchetype->InstantiateArchetype : srcArchetype->CopyArchetype;

            var archetypeChunkFilter = new ArchetypeChunkFilter();
            archetypeChunkFilter.Archetype = dstArchetype;

            if (RequiresBuildingResidueSharedComponentIndices(srcArchetype, dstArchetype))
            {
                BuildResidueSharedComponentIndices(srcArchetype, dstArchetype, src.Chunk->SharedComponentValues, archetypeChunkFilter.SharedComponentValues);
            }
            else
            {
                // Always copy shared component indices since GetChunkWithEmptySlots might reallocate the storage of SharedComponentValues
                src.Chunk->SharedComponentValues.CopyTo(archetypeChunkFilter.SharedComponentValues, 0, dstArchetype->NumSharedComponents);
            }

            int instanceBeginIndex = 0;
            while (instanceBeginIndex != instanceCount)
            {
                var chunk = GetChunkWithEmptySlots(ref archetypeChunkFilter);
                var indexInChunk = chunk->Count;
                var allocateCount = math.min(instanceCount - instanceBeginIndex, chunk->UnusedCount);

                ChunkDataUtility.AllocateClone(chunk, outputEntities + instanceBeginIndex, allocateCount, srcEntity);

                if (remapChunks != null)
                {
                    remapChunks[remapChunksCount].Chunk = chunk;
                    remapChunks[remapChunksCount].IndexInChunk = indexInChunk;
                    remapChunks[remapChunksCount].AllocatedCount = allocateCount;
                    remapChunks[remapChunksCount].InstanceBeginIndex = instanceBeginIndex;
                    remapChunksCount++;
                }

                instanceBeginIndex += allocateCount;
            }

            return remapChunksCount;
        }

        void InstantiateEntitiesGroup(Entity* srcEntities, int srcEntityCount, Entity* outputRootEntities, bool outputRootEntityOnly, int instanceCount, bool removePrefab)
        {
            int totalCount = srcEntityCount * instanceCount;

            var tempAllocSize = sizeof(Entity) * totalCount +
                sizeof(InstantiateRemapChunk) * totalCount + sizeof(Entity) * instanceCount;
            byte* allocation;
            const int kMaxStackAllocSize = 16 * 1024;

            if (tempAllocSize > kMaxStackAllocSize)
            {
                allocation = (byte*)UnsafeUtility.Malloc(tempAllocSize, 16, Allocator.Temp);
            }
            else
            {
                var temp = stackalloc byte[tempAllocSize];

                allocation = temp;
            }

            var entityRemap = (Entity*)allocation;
            var remapChunks = (InstantiateRemapChunk*)(entityRemap + totalCount);
            var outputEntities = (Entity*)(remapChunks + totalCount);

            var remapChunksCount = 0;

            for (int i = 0; i != srcEntityCount; i++)
            {
                var srcEntity = srcEntities[i];

                remapChunksCount = InstantiateEntitiesOne(srcEntity, outputEntities, instanceCount, remapChunks, remapChunksCount, removePrefab);

                for (int r = 0; r != instanceCount; r++)
                {
                    var ptr = entityRemap + (r * srcEntityCount + i);
                    *ptr = outputEntities[r];
                }

                if (outputRootEntityOnly)
                {
                    if (i == 0)
                    {
                        for (int r = 0; r != instanceCount; r++)
                            outputRootEntities[r] = outputEntities[r];
                    }
                }
                else
                {
                    for (int r = 0; r != instanceCount; r++)
                        outputRootEntities[r * srcEntityCount + i] = outputEntities[r];
                }
            }


            for (int i = 0; i != remapChunksCount; i++)
            {
                var chunk = remapChunks[i].Chunk;
                var dstArchetype = chunk->Archetype;
                var allocatedCount = remapChunks[i].AllocatedCount;
                var indexInChunk = remapChunks[i].IndexInChunk;
                var instanceBeginIndex = remapChunks[i].InstanceBeginIndex;

                var localRemap = entityRemap + instanceBeginIndex * srcEntityCount;

                EntityRemapUtility.PatchEntitiesForPrefab(dstArchetype->ScalarEntityPatches + 1, dstArchetype->ScalarEntityPatchCount - 1,
                    dstArchetype->BufferEntityPatches, dstArchetype->BufferEntityPatchCount,
                    chunk->Buffer, indexInChunk, allocatedCount, srcEntities, localRemap, srcEntityCount);

                if (dstArchetype->ManagedEntityPatchCount > 0)
                {
                    ManagedChangesTracker.PatchEntitiesForPrefab(dstArchetype, chunk, indexInChunk, allocatedCount, srcEntities, localRemap, srcEntityCount, Allocator.Temp);
                }
            }

            if (tempAllocSize > kMaxStackAllocSize)
                UnsafeUtility.Free(allocation, Allocator.Temp);
        }

        EntityBatchInChunk GetFirstEntityBatchInChunk(Entity* entities, int count)
        {
            // This is optimized for the case where the array of entities are allocated contigously in the chunk
            // Thus the compacting of other elements can be batched

            // Calculate baseEntityIndex & chunk
            var baseEntityIndex = entities[0].Index;

            var versions = m_VersionByEntity;
            var chunkData = m_EntityInChunkByEntity;

            var chunk = versions[baseEntityIndex] == entities[0].Version
                ? m_EntityInChunkByEntity[baseEntityIndex].Chunk
                : null;
            var indexInChunk = chunkData[baseEntityIndex].IndexInChunk;
            var batchCount = 0;

            while (batchCount < count)
            {
                var entityIndex = entities[batchCount].Index;
                var curChunk = chunkData[entityIndex].Chunk;
                var curIndexInChunk = chunkData[entityIndex].IndexInChunk;

                if (versions[entityIndex] == entities[batchCount].Version)
                {
                    if (curChunk != chunk || curIndexInChunk != indexInChunk + batchCount)
                        break;
                }
                else
                {
                    if (chunk != null)
                        break;
                }

                batchCount++;
            }

            return new EntityBatchInChunk
            {
                Chunk = chunk,
                Count = batchCount,
                StartIndex = indexInChunk
            };
        }

        public static JobHandle GetCreatedAndDestroyedEntities(EntityComponentStore* store, NativeList<int> state, NativeList<Entity> createdEntities, NativeList<Entity> destroyedEntities, bool async)
        {
            // Early outwhen no entities were created or destroyed compared to the last time this method was called
            if (state.Length != 0 && store->EntityOrderVersion == state[0])
            {
                createdEntities.Clear();
                destroyedEntities.Clear();
                return default;
            }

            var jobData = new GetOrCreateDestroyedEntitiesJob
            {
                State = state,
                CreatedEntities = createdEntities,
                DestroyedEntities = destroyedEntities,
                Store = store
            };

            if (async)
                return jobData.Schedule();
            else
            {
                jobData.Run();
                return default;
            }
        }

        [BurstCompile]
        struct GetOrCreateDestroyedEntitiesJob : IJob
        {
            public NativeList<int>    State;
            public NativeList<Entity> CreatedEntities;
            public NativeList<Entity> DestroyedEntities;

            [NativeDisableUnsafePtrRestriction]
            public EntityComponentStore* Store;

            public void Execute()
            {
                var capacity = Store->m_EntitiesCapacity;
                var versionByEntity = Store->m_VersionByEntity;
                var entityInChunkByEntity = Store->m_EntityInChunkByEntity;

                CreatedEntities.Clear();
                DestroyedEntities.Clear();
                State.Resize(capacity + 1, NativeArrayOptions.ClearMemory);

                State[0] = Store->EntityOrderVersion;
                var state = State.AsArray().GetSubArray(1, capacity);

                for (int i = 0; i != capacity; i++)
                {
                    if (state[i] == versionByEntity[i])
                        continue;

                    // Was a valid entity but version was incremented, thus destroyed
                    if (state[i] != 0)
                    {
                        DestroyedEntities.Add(new Entity { Index = i, Version = state[i] });
                        state[i] = 0;
                    }

                    // It is now a valid entity, but version has changed
                    if (entityInChunkByEntity[i].Chunk != null)
                    {
                        CreatedEntities.Add(new Entity { Index = i, Version = versionByEntity[i] });
                        state[i] = versionByEntity[i];
                    }
                }
            }
        }
    }
}

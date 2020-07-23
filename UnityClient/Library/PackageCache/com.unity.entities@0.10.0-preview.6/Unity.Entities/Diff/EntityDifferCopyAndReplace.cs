using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Entities
{
    static unsafe partial class EntityDiffer
    {
        [BurstCompile]
        struct PatchAndAddClonedChunks : IJobParallelFor
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> SrcChunks;
            [ReadOnly] public NativeArray<ArchetypeChunk> DstChunks;

            [NativeDisableUnsafePtrRestriction] public EntityComponentStore* DstEntityComponentStore;

            public void Execute(int index)
            {
                var srcChunk = SrcChunks[index].m_Chunk;
                var dstChunk = DstChunks[index].m_Chunk;

                var srcArchetype = srcChunk->Archetype;
                var dstArchetype = dstChunk->Archetype;

                ChunkDataUtility.CloneChangeVersions(srcArchetype, srcChunk->ListIndex, dstArchetype, dstChunk->ListIndex);

                DstEntityComponentStore->AddExistingEntitiesInChunk(dstChunk);
            }
        }

        internal static void CopyAndReplaceChunks(
            EntityManager srcEntityManager,
            EntityManager dstEntityManager,
            EntityQuery dstEntityQuery,
            ArchetypeChunkChanges archetypeChunkChanges)
        {
            s_CopyAndReplaceChunksProfilerMarker.Begin();
            var dstAccess = dstEntityManager.GetCheckedEntityDataAccess();
            var srcAccess = srcEntityManager.GetCheckedEntityDataAccess();

            var archetypeChanges = dstAccess->EntityComponentStore->BeginArchetypeChangeTracking();

            DestroyChunks(dstEntityManager, archetypeChunkChanges.DestroyedDstChunks.Chunks);
            CloneAndAddChunks(srcEntityManager, dstEntityManager, archetypeChunkChanges.CreatedSrcChunks.Chunks);

            dstAccess->EntityComponentStore->EndArchetypeChangeTracking(archetypeChanges, dstAccess->EntityQueryManager);

            //@TODO-opt: use a query that searches for all chunks that have chunk components on it
            //@TODO-opt: Move this into a job
            // Any chunk might have been recreated, so the ChunkHeader might be invalid
            using (var allDstChunks = dstEntityQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var chunk in allDstChunks)
                {
                    var metaEntity = chunk.m_Chunk->metaChunkEntity;
                    if (metaEntity != Entity.Null)
                    {
                        if (dstEntityManager.Exists(metaEntity))
                            dstEntityManager.SetComponentData(metaEntity, new ChunkHeader { ArchetypeChunk = chunk });
                    }
                }
            }

            srcAccess->EntityComponentStore->IncrementGlobalSystemVersion();
            dstAccess->EntityComponentStore->IncrementGlobalSystemVersion();
            s_CopyAndReplaceChunksProfilerMarker.End();
        }

        static void DestroyChunks(EntityManager entityManager, NativeList<ArchetypeChunk> chunks)
        {
            var access = entityManager.GetCheckedEntityDataAccess();
            var ecs = access->EntityComponentStore;

            for (var i = 0; i < chunks.Length; i++)
            {
                Assert.IsTrue(chunks[i].m_EntityComponentStore == access);
                DestroyChunkForDiffing(entityManager, chunks[i].m_Chunk);
            }
        }

        static void DestroyChunkForDiffing(EntityManager entityManager, Chunk* chunk)
        {
            var access = entityManager.GetCheckedEntityDataAccess();
            var ecs = access->EntityComponentStore;
            var mcs = access->ManagedComponentStore;

            var count = chunk->Count;
            ChunkDataUtility.DeallocateBuffers(chunk);
            ecs->DeallocateManagedComponents(chunk, 0, count);

            chunk->Archetype->EntityCount -= chunk->Count;
            ecs->FreeEntities(chunk);

            ChunkDataUtility.SetChunkCountKeepMetaChunk(chunk, 0);

            mcs.Playback(ref ecs->ManagedChangesTracker);
        }

        static void CloneAndAddChunks(EntityManager srcEntityManager, EntityManager dstEntityManager, NativeList<ArchetypeChunk> chunks)
        {
            var cloned = new NativeArray<ArchetypeChunk>(chunks.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var srcAccess = srcEntityManager.GetCheckedEntityDataAccess();
            var dstAccess = dstEntityManager.GetCheckedEntityDataAccess();

            for (var i = 0; i < chunks.Length; i++)
            {
                var srcChunk = chunks[i].m_Chunk;

                var dstChunk = CloneChunkWithoutAllocatingEntities(
                    dstEntityManager,
                    srcChunk,
                    srcAccess->ManagedComponentStore);

                cloned[i] = new ArchetypeChunk {m_Chunk = dstChunk};
            }

            // Ensure capacity in the dst world before we start linking entities.
            dstAccess->EntityComponentStore->EnsureCapacity(srcEntityManager.EntityCapacity);
            dstAccess->EntityComponentStore->CopyNextFreeEntityIndex(srcAccess->EntityComponentStore);

            new PatchAndAddClonedChunks
            {
                SrcChunks = chunks,
                DstChunks = cloned,
                DstEntityComponentStore = dstAccess->EntityComponentStore
            }.Schedule(chunks.Length, 64).Complete();

            cloned.Dispose();
        }

        static Chunk* CloneChunkWithoutAllocatingEntities(EntityManager dstEntityManager, Chunk* srcChunk, ManagedComponentStore srcManagedComponentStore)
        {
            var dstAccess = dstEntityManager.GetCheckedEntityDataAccess();
            var dstEntityComponentStore = dstAccess->EntityComponentStore;
            var dstManagedComponentStore = dstAccess->ManagedComponentStore;

            // Copy shared component data
            var dstSharedIndices = stackalloc int[srcChunk->Archetype->NumSharedComponents];
            srcChunk->SharedComponentValues.CopyTo(dstSharedIndices, 0, srcChunk->Archetype->NumSharedComponents);
            dstManagedComponentStore.CopySharedComponents(srcManagedComponentStore, dstSharedIndices, srcChunk->Archetype->NumSharedComponents);

            // @TODO: Why don't we memcpy the whole chunk. So we include all extra fields???

            // Allocate a new chunk
            var srcArchetype = srcChunk->Archetype;
            var dstArchetype = dstEntityComponentStore->GetOrCreateArchetype(srcArchetype->Types, srcArchetype->TypesCount);

            var dstChunk = dstEntityComponentStore->GetCleanChunkNoMetaChunk(dstArchetype, dstSharedIndices);
            dstManagedComponentStore.Playback(ref dstEntityComponentStore->ManagedChangesTracker);

            dstChunk->metaChunkEntity = srcChunk->metaChunkEntity;

            // Release any references obtained by GetCleanChunk & CopySharedComponents
            for (var i = 0; i < srcChunk->Archetype->NumSharedComponents; i++)
                dstManagedComponentStore.RemoveReference(dstSharedIndices[i]);

            ChunkDataUtility.SetChunkCountKeepMetaChunk(dstChunk, srcChunk->Count);
            dstManagedComponentStore.Playback(ref dstEntityComponentStore->ManagedChangesTracker);

            dstChunk->Archetype->EntityCount += srcChunk->Count;

            var copySize = Chunk.GetChunkBufferSize();
            UnsafeUtility.MemCpy((byte*)dstChunk + Chunk.kBufferOffset, (byte*)srcChunk + Chunk.kBufferOffset, copySize);

            var numManagedComponents = dstChunk->Archetype->NumManagedComponents;
            var hasHybridComponents = dstArchetype->HasHybridComponents;
            for (int t = 0; t < numManagedComponents; ++t)
            {
                int indexInArchetype = t + dstChunk->Archetype->FirstManagedComponent;

                if (hasHybridComponents)
                {
                    // We consider hybrid components as always different, there's no reason to clone those at this point
                    var typeCategory = TypeManager.GetTypeInfo(dstChunk->Archetype->Types[indexInArchetype].TypeIndex).Category;
                    if (typeCategory == TypeManager.TypeCategory.Class)
                        continue;
                }

                var offset = dstChunk->Archetype->Offsets[indexInArchetype];
                var a = (int*)(dstChunk->Buffer + offset);

                dstManagedComponentStore.CloneManagedComponentsFromDifferentWorld(a, dstChunk->Count, srcManagedComponentStore, ref *dstAccess->EntityComponentStore);
            }

            BufferHeader.PatchAfterCloningChunk(dstChunk);
            dstChunk->SequenceNumber = srcChunk->SequenceNumber;

            return dstChunk;
        }
    }
}

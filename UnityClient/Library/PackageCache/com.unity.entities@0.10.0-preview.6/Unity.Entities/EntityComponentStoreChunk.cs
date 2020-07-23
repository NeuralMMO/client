using System;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Entities
{
    internal unsafe partial struct EntityComponentStore
    {
        // ----------------------------------------------------------------------------------------------------------
        // PUBLIC
        // ----------------------------------------------------------------------------------------------------------

        // ----------------------------------------------------------------------------------------------------------
        // INTERNAL
        // ----------------------------------------------------------------------------------------------------------


        //                              | ChangeVersion | OrderVersion |
        // -----------------------------|---------------|--------------|
        // Remove(EntityBatchInChunk)   | NO            | YES          |

        // Write Component to Chunk     | YES           | NO           |
        // Remove Component In-Place    | NO            | NO           |
        // Add Entities in Chunk        | YES           | YES          |
        // Add Component In-Place       | YES           | NO           |
        // Move Chunk World             | YES           | YES          |
        //
        // ChangeVersion : e.g. Should I update LocalToWorld from Translation?
        // OrderVersion : e.g. Should I re-allocate a lookaside cache based on chunk data?


        internal void AllocateEntities(Archetype* arch, Chunk* chunk, int baseIndex, int count, Entity* outputEntities)
        {
            Assert.AreEqual(chunk->Archetype->Offsets[0], 0);
            Assert.AreEqual(chunk->Archetype->SizeOfs[0], sizeof(Entity));

            var entityInChunkStart = (Entity*)chunk->Buffer + baseIndex;

            for (var i = 0; i != count; i++)
            {
                var entityIndexInChunk = m_EntityInChunkByEntity[m_NextFreeEntityIndex].IndexInChunk;
                if (entityIndexInChunk == -1)
                {
                    IncreaseCapacity();
                    entityIndexInChunk = m_EntityInChunkByEntity[m_NextFreeEntityIndex].IndexInChunk;
                }

                var entityVersion = m_VersionByEntity[m_NextFreeEntityIndex];

                if (outputEntities != null)
                {
                    outputEntities[i].Index = m_NextFreeEntityIndex;
                    outputEntities[i].Version = entityVersion;
                }

                var entityInChunk = entityInChunkStart + i;

                entityInChunk->Index = m_NextFreeEntityIndex;
                entityInChunk->Version = entityVersion;

                m_EntityInChunkByEntity[m_NextFreeEntityIndex].IndexInChunk = baseIndex + i;
                m_ArchetypeByEntity[m_NextFreeEntityIndex] = arch;
                m_EntityInChunkByEntity[m_NextFreeEntityIndex].Chunk = chunk;
#if UNITY_EDITOR
                m_NameByEntity[m_NextFreeEntityIndex] = new NumberedWords();
#endif

                m_NextFreeEntityIndex = entityIndexInChunk;
            }
        }

        internal void DeallocateDataEntitiesInChunk(Chunk* chunk, int indexInChunk, int batchCount)
        {
            DeallocateBuffers(chunk, indexInChunk, batchCount);
            DeallocateManagedComponents(chunk, indexInChunk, batchCount);

            var freeIndex = m_NextFreeEntityIndex;
            var entities = (Entity*)chunk->Buffer + indexInChunk;

            for (var i = batchCount - 1; i >= 0; --i)
            {
                var entityIndex = entities[i].Index;

                m_EntityInChunkByEntity[entityIndex].Chunk = null;
                m_VersionByEntity[entityIndex]++;
                m_EntityInChunkByEntity[entityIndex].IndexInChunk = freeIndex;
#if UNITY_EDITOR
                m_NameByEntity[entityIndex] = new NumberedWords();
#endif
                freeIndex = entityIndex;
            }

            m_NextFreeEntityIndex = freeIndex;

            // Compute the number of things that need to moved and patched.
            int patchCount = Math.Min(batchCount, chunk->Count - indexInChunk - batchCount);

            if (0 == patchCount)
                return;

            // updates indexInChunk to point to where the components will be moved to
            //Assert.IsTrue(chunk->archetype->sizeOfs[0] == sizeof(Entity) && chunk->archetype->offsets[0] == 0);
            var movedEntities = (Entity*)chunk->Buffer + (chunk->Count - patchCount);
            for (var i = 0; i != patchCount; i++)
                m_EntityInChunkByEntity[movedEntities[i].Index].IndexInChunk = indexInChunk + i;

            // Move component data from the end to where we deleted components
            ChunkDataUtility.Copy(chunk, chunk->Count - patchCount, chunk, indexInChunk, patchCount);
        }

        void DeallocateBuffers(Chunk* chunk, int indexInChunk, int batchCount)
        {
            var archetype = chunk->Archetype;

            for (var ti = 0; ti < archetype->TypesCount; ++ti)
            {
                var type = archetype->Types[ti];

                if (!type.IsBuffer)
                    continue;

                var basePtr = chunk->Buffer + archetype->Offsets[ti];
                var stride = archetype->SizeOfs[ti];

                for (int i = 0; i < batchCount; ++i)
                {
                    byte* bufferPtr = basePtr + stride * (indexInChunk + i);
                    BufferHeader.Destroy((BufferHeader*)bufferPtr);
                }
            }
        }
    }
}

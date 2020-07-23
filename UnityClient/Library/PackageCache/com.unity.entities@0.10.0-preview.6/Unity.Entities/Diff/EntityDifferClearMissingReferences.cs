using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Entities
{
    static unsafe partial class EntityDiffer
    {
        [BurstCompile]
        struct ClearMissingReferencesJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction] public TypeManager.TypeInfo* TypeInfo;
            [NativeDisableUnsafePtrRestriction] public TypeManager.EntityOffsetInfo* EntityOffsetInfo;
            [ReadOnly] public uint GlobalSystemVersion;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly, NativeDisableUnsafePtrRestriction] public EntityComponentStore* EntityComponentStore;

            public void Execute(int index)
            {
                var chunk = Chunks[index].m_Chunk;

                ChunkDataUtility.ClearMissingReferences(chunk);
            }
        }

        static void ClearMissingReferences(EntityManager entityManager, NativeArray<ArchetypeChunk> chunks, out JobHandle jobHandle, JobHandle dependsOn)
        {
            jobHandle = new ClearMissingReferencesJob
            {
                TypeInfo = TypeManager.GetTypeInfoPointer(),
                EntityOffsetInfo = TypeManager.GetEntityOffsetsPointer(),
                GlobalSystemVersion = entityManager.GlobalSystemVersion,
                Chunks = chunks,
                EntityComponentStore = entityManager.GetCheckedEntityDataAccess()->EntityComponentStore,
            }.Schedule(chunks.Length, 64, dependsOn);
        }
    }
}

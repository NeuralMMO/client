using System;
using Unity.Collections;
using Unity.Jobs;

namespace Unity.Entities
{
    public unsafe partial struct EntityManager
    {
        struct IsolateCopiedEntities : IComponentData {}

        /// <summary>
        /// Instantiates / Copies all entities from srcEntityManager and copies them into this EntityManager.
        /// Entity references on components that are being cloned to entities inside the srcEntities set are remapped to the instantiated entities.
        /// </summary>
        public void CopyEntitiesFrom(EntityManager srcEntityManager, NativeArray<Entity> srcEntities, NativeArray<Entity> outputEntities = default)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (outputEntities.IsCreated && outputEntities.Length != srcEntities.Length)
                throw  new ArgumentException("outputEntities.Length must match srcEntities.Length");
#endif

            using (var srcManagerInstances = new NativeArray<Entity>(srcEntities.Length, Allocator.Temp))
            {
                srcEntityManager.CopyEntities(srcEntities, srcManagerInstances);
                srcEntityManager.AddComponent(srcManagerInstances, ComponentType.ReadWrite<IsolateCopiedEntities>());

                var instantiated = srcEntityManager.CreateEntityQuery(new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(IsolateCopiedEntities) },
                    Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
                });

                using (var entityRemapping = srcEntityManager.CreateEntityRemapArray(Allocator.TempJob))
                {
                    MoveEntitiesFromInternalQuery(srcEntityManager, instantiated, entityRemapping);

                    EntityRemapUtility.GetTargets(out var output, entityRemapping);
                    RemoveComponent(output, ComponentType.ReadWrite<IsolateCopiedEntities>());
                    output.Dispose();

                    if (outputEntities.IsCreated)
                    {
                        for (int i = 0; i != outputEntities.Length; i++)
                            outputEntities[i] = entityRemapping[srcManagerInstances[i].Index].Target;
                    }
                }
            }
        }

        /// <summary>
        /// Copies all entities from srcEntityManager and replaces all entities in this EntityManager
        /// </summary>
        /// <remarks>
        /// Guarantees that the chunk layout & order of the entities will match exactly, thus this method can be used for deterministic rollback.
        /// This feature is not complete and only supports a subset of the EntityManager features at the moment:
        /// * Currently it copies all SystemStateComponents (They should not be copied)
        /// * Currently does not support class based components
        /// </remarks>
        public void CopyAndReplaceEntitiesFrom(EntityManager srcEntityManager)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!srcEntityManager.IsCreated)
                throw new ArgumentNullException(nameof(srcEntityManager));
            if (!IsCreated)
                throw new ArgumentException("This EntityManager has been destroyed");
#endif

            srcEntityManager.CompleteAllJobs();
            CompleteAllJobs();

            var srcAccess = srcEntityManager.GetCheckedEntityDataAccess();
            var selfAccess = GetCheckedEntityDataAccess();

            using (var srcChunks = srcAccess->ManagedEntityDataAccess.m_UniversalQueryWithChunks.CreateArchetypeChunkArrayAsync(Allocator.TempJob, out var srcChunksJob))
            using (var dstChunks = selfAccess->ManagedEntityDataAccess.m_UniversalQueryWithChunks.CreateArchetypeChunkArrayAsync(Allocator.TempJob, out var dstChunksJob))
            {
                using (var archetypeChunkChanges = EntityDiffer.GetArchetypeChunkChanges(
                    srcChunks,
                    dstChunks,
                    Allocator.TempJob,
                    jobHandle: out var archetypeChunkChangesJob,
                    dependsOn: JobHandle.CombineDependencies(srcChunksJob, dstChunksJob)))
                {
                    archetypeChunkChangesJob.Complete();

                    EntityDiffer.CopyAndReplaceChunks(srcEntityManager, this, selfAccess->ManagedEntityDataAccess.m_UniversalQueryWithChunks, archetypeChunkChanges);
                    Unity.Entities.EntityComponentStore.AssertSameEntities(srcAccess->EntityComponentStore, selfAccess->EntityComponentStore);
                }
            }
        }
    }
}

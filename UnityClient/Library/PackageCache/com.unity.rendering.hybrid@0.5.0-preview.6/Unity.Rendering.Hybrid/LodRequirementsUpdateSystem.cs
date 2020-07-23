using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Unity.Rendering
{
    public struct PerInstanceCullingTag : IComponentData {}

    struct RootLodRequirement : IComponentData
    {
        public LodRequirement LOD;
    }

    struct LodRequirement : IComponentData
    {
        public float3 WorldReferencePosition;
        public float MinDist;
        public float MaxDist;

        public LodRequirement(MeshLODGroupComponent lodGroup, LocalToWorld localToWorld, int lodMask)
        {
            var referencePoint = math.transform(localToWorld.Value, lodGroup.LocalReferencePoint);
            float minDist = float.MaxValue;
            float maxDist = 0.0F;
            if ((lodMask & 0x01) == 0x01)
            {
                minDist = 0.0f;
                maxDist = math.max(maxDist, lodGroup.LODDistances0.x);
            }
            if ((lodMask & 0x02) == 0x02)
            {
                minDist = math.min(minDist, lodGroup.LODDistances0.x);
                maxDist = math.max(maxDist, lodGroup.LODDistances0.y);
            }
            if ((lodMask & 0x04) == 0x04)
            {
                minDist = math.min(minDist, lodGroup.LODDistances0.y);
                maxDist = math.max(maxDist, lodGroup.LODDistances0.z);
            }
            if ((lodMask & 0x08) == 0x08)
            {
                minDist = math.min(minDist, lodGroup.LODDistances0.z);
                maxDist = math.max(maxDist, lodGroup.LODDistances0.w);
            }
            if ((lodMask & 0x10) == 0x10)
            {
                minDist = math.min(minDist, lodGroup.LODDistances0.w);
                maxDist = math.max(maxDist, lodGroup.LODDistances1.x);
            }
            if ((lodMask & 0x20) == 0x20)
            {
                minDist = math.min(minDist, lodGroup.LODDistances1.x);
                maxDist = math.max(maxDist, lodGroup.LODDistances1.y);
            }
            if ((lodMask & 0x40) == 0x40)
            {
                minDist = math.min(minDist, lodGroup.LODDistances1.y);
                maxDist = math.max(maxDist, lodGroup.LODDistances1.z);
            }
            if ((lodMask & 0x80) == 0x80)
            {
                minDist = math.min(minDist, lodGroup.LODDistances1.z);
                maxDist = math.max(maxDist, lodGroup.LODDistances1.w);
            }

            WorldReferencePosition = referencePoint;
            MinDist = minDist;
            MaxDist = maxDist;
        }
    }

    [ConverterVersion("joe", 2)]
    [UpdateInGroup(typeof(StructuralChangePresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    [ExecuteAlways]
    public class AddLodRequirementComponents : ComponentSystem
    {
        EntityQuery m_MissingRootLodRequirement;
        EntityQuery m_MissingLodRequirement;

        protected override void OnCreate()
        {
            m_MissingLodRequirement = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {ComponentType.ReadOnly<MeshLODComponent>()},
                None = new[] {ComponentType.ReadOnly<LodRequirement>()},
                Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
            });

            m_MissingRootLodRequirement = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] {ComponentType.ReadOnly<MeshLODComponent>()},
                None = new[] {ComponentType.ReadOnly<RootLodRequirement>()},
                Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
            });
        }

        protected override void  OnUpdate()
        {
            EntityManager.AddComponent(m_MissingLodRequirement, typeof(LodRequirement));
            EntityManager.AddComponent(m_MissingRootLodRequirement, typeof(RootLodRequirement));
        }
    }

    [ConverterVersion("joe", 1)]
    [UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    [ExecuteAlways]
    public class LodRequirementsUpdateSystem : JobComponentSystem
    {
        EntityQuery m_LodRenderers;

        [BurstCompile]
        struct UpdateLodRequirementsJob : IJobChunk
        {
            [ReadOnly] public ComponentDataFromEntity<MeshLODGroupComponent>    MeshLODGroupComponent;

            [ReadOnly] public ArchetypeChunkComponentType<MeshLODComponent>     MeshLODComponent;
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld>             LocalToWorldLookup;

            public ArchetypeChunkComponentType<LodRequirement>                  LodRequirement;
            public ArchetypeChunkComponentType<RootLodRequirement>              RootLodRequirement;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var lodRequirement = chunk.GetNativeArray(LodRequirement);
                var rootLodRequirement = chunk.GetNativeArray(RootLodRequirement);
                var meshLods = chunk.GetNativeArray(MeshLODComponent);
                var instanceCount = chunk.Count;

                for (int i = 0; i < instanceCount; i++)
                {
                    var meshLod = meshLods[i];
                    var lodGroupEntity = meshLod.Group;
                    var lodMask = meshLod.LODMask;
                    var lodGroup = MeshLODGroupComponent[lodGroupEntity];

                    // Cannot take LocalToWorld from the instances, because they might not all share the same pivot
                    lodRequirement[i] = new LodRequirement(lodGroup, LocalToWorldLookup[lodGroupEntity], lodMask);
                }

                for (int i = 0; i < instanceCount; i++)
                {
                    var meshLod = meshLods[i];
                    var lodGroupEntity = meshLod.Group;
                    var lodGroup = MeshLODGroupComponent[lodGroupEntity];
                    var parentMask = lodGroup.ParentMask;
                    var parentGroupEntity = lodGroup.ParentGroup;

                    RootLodRequirement rootLod;

                    if (parentGroupEntity == Entity.Null)
                    {
                        rootLod.LOD.WorldReferencePosition = new float3(0, 0, 0);
                        rootLod.LOD.MinDist = 0;
                        rootLod.LOD.MaxDist = 1048576.0f;
                    }
                    else
                    {
                        var parentLodGroup = MeshLODGroupComponent[parentGroupEntity];
                        rootLod.LOD = new LodRequirement(parentLodGroup, LocalToWorldLookup[parentGroupEntity], parentMask);

                        if (parentLodGroup.ParentGroup != Entity.Null)
                            throw new System.NotImplementedException("Deep HLOD is not supported yet");
                    }

                    rootLodRequirement[i] = rootLod;
                }
            }
        }

        protected override void OnCreate()
        {
            m_LodRenderers = GetEntityQuery(ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadOnly<MeshLODComponent>(), typeof(LodRequirement), typeof(RootLodRequirement));
        }

        protected override JobHandle OnUpdate(JobHandle dependency)
        {
            //@TODO: Updating of LodRequirement & RootLodRequirement has to be push based,
            //       Otherwise how do we quickly early out when nothing has changed.

            var updateLodJob = new UpdateLodRequirementsJob
            {
                MeshLODGroupComponent = GetComponentDataFromEntity<MeshLODGroupComponent>(true),
                MeshLODComponent = GetArchetypeChunkComponentType<MeshLODComponent>(true),
                LocalToWorldLookup = GetComponentDataFromEntity<LocalToWorld>(true),
                LodRequirement = GetArchetypeChunkComponentType<LodRequirement>(),
                RootLodRequirement = GetArchetypeChunkComponentType<RootLodRequirement>(),
            };
            return updateLodJob.ScheduleParallel(m_LodRenderers, dependency);
        }
    }
}

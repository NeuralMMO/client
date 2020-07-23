using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;

namespace Unity.Rendering
{
    /// <summary>
    /// Copies the BoneIndexOffsets on the skinned entities to the index offsets material properties of the skinned mesh entities.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
#if ENABLE_HYBRID_RENDERER_V2
    [UpdateBefore(typeof(HybridRendererSystem))]
#else
    [UpdateBefore(typeof(RenderMeshSystemV2))]
#endif
    public class CopySkinnedEntityDataToRenderEntity : JobComponentSystem
    {
#pragma warning disable 618
        [BurstCompile]
        private struct IterateSkinnedEntityRefJob : IJobForEachWithEntity<SkinnedEntityReference, BoneIndexOffsetMaterialProperty>
        {
            [ReadOnly] public ComponentDataFromEntity<BoneIndexOffset> BoneIndexOffsets;

            public void Execute(
                Entity entity,
                int index,
                ref SkinnedEntityReference skinnedEntity,
                ref BoneIndexOffsetMaterialProperty boneIndexOffset)
            {
                boneIndexOffset = new BoneIndexOffsetMaterialProperty { Value = BoneIndexOffsets[skinnedEntity.Value].Value };
            }
        }
#pragma warning restore 618

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new IterateSkinnedEntityRefJob
            {
                BoneIndexOffsets = GetComponentDataFromEntity<BoneIndexOffset>(true),
            };

            return job.Schedule(this, inputDeps);
        }
    }
}

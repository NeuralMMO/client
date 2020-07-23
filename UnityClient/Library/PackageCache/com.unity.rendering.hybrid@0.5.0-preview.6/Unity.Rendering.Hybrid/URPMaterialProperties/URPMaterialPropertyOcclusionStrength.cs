using Unity.Entities;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("_OcclusionStrength", MaterialPropertyFormat.Float)]
    [GenerateAuthoringComponent]
    public struct URPMaterialPropertyOcclusionStrength : IComponentData
    {
        public float Value;
    }
}
#endif

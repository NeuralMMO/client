using Unity.Entities;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("_BumpScale", MaterialPropertyFormat.Float)]
    [GenerateAuthoringComponent]
    public struct URPMaterialPropertyBumpScale : IComponentData
    {
        public float Value;
    }
}
#endif

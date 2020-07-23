using Unity.Entities;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("_SmoothnessRemapMin"   , MaterialPropertyFormat.Float)]
    [GenerateAuthoringComponent]
    public struct HDRPMaterialPropertySmoothnessRemapMin : IComponentData { public float  Value; }
}
#endif

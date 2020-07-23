using Unity.Entities;
using Unity.Mathematics;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("_EmissiveColor"        , MaterialPropertyFormat.Float3)]
    [GenerateAuthoringComponent]
    public struct HDRPMaterialPropertyEmissiveColor : IComponentData { public float3 Value; }
}
#endif

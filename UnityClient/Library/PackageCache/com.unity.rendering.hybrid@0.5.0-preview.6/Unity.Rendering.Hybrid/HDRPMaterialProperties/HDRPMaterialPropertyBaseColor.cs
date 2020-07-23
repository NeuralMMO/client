using Unity.Entities;
using Unity.Mathematics;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("_BaseColor"            , MaterialPropertyFormat.Float4)]
    [GenerateAuthoringComponent]
    public struct HDRPMaterialPropertyBaseColor : IComponentData { public float4 Value; }
}
#endif

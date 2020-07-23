using Unity.Entities;
using Unity.Mathematics;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("unity_SpecCube0_HDR", MaterialPropertyFormat.Float4)]
    public struct BuiltinMaterialPropertyUnity_SpecCube0_HDR : IComponentData
    {
        public float4 Value;
    }
}
#endif

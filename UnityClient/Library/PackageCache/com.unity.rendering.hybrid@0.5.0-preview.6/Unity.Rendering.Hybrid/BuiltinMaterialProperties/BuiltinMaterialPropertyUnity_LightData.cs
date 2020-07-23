using Unity.Entities;
using Unity.Mathematics;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("unity_LightData", MaterialPropertyFormat.Float4)]
    public struct BuiltinMaterialPropertyUnity_LightData : IComponentData
    {
        public float4 Value;
    }
}
#endif

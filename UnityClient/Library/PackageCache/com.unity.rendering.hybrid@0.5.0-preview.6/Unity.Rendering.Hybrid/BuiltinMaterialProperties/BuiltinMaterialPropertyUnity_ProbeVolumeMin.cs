using Unity.Entities;
using Unity.Mathematics;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("unity_ProbeVolumeMin"          , MaterialPropertyFormat.Float4)] public struct BuiltinMaterialPropertyUnity_ProbeVolumeMin : IComponentData { public float4   Value; }
}
#endif

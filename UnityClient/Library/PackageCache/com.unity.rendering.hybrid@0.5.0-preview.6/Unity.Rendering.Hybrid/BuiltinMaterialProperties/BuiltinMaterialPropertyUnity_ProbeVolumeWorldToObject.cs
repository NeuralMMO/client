using Unity.Entities;
using Unity.Mathematics;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("unity_ProbeVolumeWorldToObject", MaterialPropertyFormat.Float4x4)] public struct BuiltinMaterialPropertyUnity_ProbeVolumeWorldToObject : IComponentData { public float4x4 Value; }
}
#endif

using Unity.Entities;
using Unity.Mathematics;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("unity_ProbeVolumeSizeInv"      , MaterialPropertyFormat.Float4)] public struct BuiltinMaterialPropertyUnity_ProbeVolumeSizeInv : IComponentData { public float4   Value; }
}
#endif

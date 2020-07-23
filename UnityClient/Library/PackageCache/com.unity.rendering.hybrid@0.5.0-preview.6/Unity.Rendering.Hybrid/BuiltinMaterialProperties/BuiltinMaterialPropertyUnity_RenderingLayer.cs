using Unity.Entities;
using Unity.Mathematics;

#if ENABLE_HYBRID_RENDERER_V2
namespace Unity.Rendering
{
    [MaterialProperty("unity_RenderingLayer", MaterialPropertyFormat.Float4)]
    public struct BuiltinMaterialPropertyUnity_RenderingLayer : IComponentData
    {
        public uint4 Value;
    }
}
#endif

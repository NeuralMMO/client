using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    // Previous matrix components always exist, to prevent compilation errors,
    // but they become material properties only when V2 is enabled
#if ENABLE_HYBRID_RENDERER_V2
    [MaterialProperty("unity_MatrixPreviousMI", MaterialPropertyFormat.Float4x4, 4 * 4 * 4)]
#endif
    // TODO: Remove this component completely after verifying that the previous inverse is
    // not needed by HDRP.
    public struct BuiltinMaterialPropertyUnity_MatrixPreviousMI_Tag : IComponentData
    {
    }
}

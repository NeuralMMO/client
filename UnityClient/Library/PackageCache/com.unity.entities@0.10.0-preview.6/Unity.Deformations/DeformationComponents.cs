using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Deformations
{
    /// <summary>
    /// Float buffer containing weight values that determine how much a corresponding blend shape is applied to the mesh.
    /// The data structure is used for mesh deformations.
    /// </summary>
    public struct BlendShapeWeight : IBufferElementData
    {
        public float Value;
    }

    /// <summary>
    ///  Matrix buffer containing the skinned transformations of bones in relation to the bind pose.
    ///  The data structure is used for mesh deformations.
    /// </summary>
    public struct SkinMatrix : IBufferElementData
    {
        public float3x4 Value;
    }
}

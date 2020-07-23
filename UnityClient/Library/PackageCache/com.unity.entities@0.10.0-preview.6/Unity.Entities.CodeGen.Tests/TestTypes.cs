using Unity.Entities;

namespace Unity.Entities.CodeGen.Tests
{
    public struct Boid : IComponentData
    {
    }

    public struct Translation : IComponentData
    {
        public float Value;
    }

    public struct Velocity : IComponentData
    {
        public float Value;
    }

    public struct Acceleration : IComponentData
    {
        public float Value;
    }

    public struct MyBufferInt : IBufferElementData
    {
        public int Value;
    }

    public struct MyBufferFloat : IBufferElementData
    {
        public float Value;
    }
}

using System;

namespace Unity.Entities.Tests
{
    [GenerateAuthoringComponent]
    public struct CodeGenTestComponent : IComponentData
    {
        public Entity Entity;
        public float Float;
        public int Int;
        public bool Bool;
        public WorldFlags Enum;
        public char Char;
    }
}

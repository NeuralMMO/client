namespace Unity.Entities.Editor.Tests
{
    struct EcsTestData : IComponentData
    {
        public int value;
    }

    struct EcsTestData2 : IComponentData
    {
        public int value0;
        public int value1;
    }

    struct EcsTestSharedComp : ISharedComponentData
    {
        public int value;

        public override string ToString()
        {
            return $"{nameof(value)}: {value}";
        }
    }
}

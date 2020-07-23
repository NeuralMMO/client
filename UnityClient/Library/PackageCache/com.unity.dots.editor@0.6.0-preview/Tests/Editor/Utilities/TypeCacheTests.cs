using System;
using NUnit.Framework;

namespace Unity.Entities.Editor.Tests
{
    class TypeCacheTests
    {
        internal struct TestColor
        {
            public float R;
            public float G;
            public float B;
            public float A;

            public static TestColor Default { get; } = new TestColor {R = 1, G = 1, B = 1, A = 1};
        }

        internal struct MyAwesomeStruct
        {
            public TestColor Color;
            public float Float;
            public char Char;

            public static MyAwesomeStruct Default { get; } = new MyAwesomeStruct
            {
                Color = TestColor.Default,
                Float = 25.0f,
                Char = 'f'
            };
        }

        internal struct MyComponent : IComponentData
        {
            public TestColor TestColor;
            public MyAwesomeStruct Struct;

            public static MyComponent Default { get; } = new MyComponent
            {
                Struct = MyAwesomeStruct.Default,
                TestColor = default
            };
        }


        struct NoAttributes
        {
        }

        [Serializable]
        struct SomeAttributes
        {
        }

        [Test]
        public void DefaultCacheReturnsProperDefaultValue()
        {
            Assert.That(TestColor.Default, Is.EqualTo(TypeCache.GetDefaultValueForStruct<TestColor>()));
            Assert.That(MyAwesomeStruct.Default, Is.EqualTo(TypeCache.GetDefaultValueForStruct<MyAwesomeStruct>()));
            Assert.That(MyComponent.Default, Is.EqualTo(TypeCache.GetDefaultValueForStruct<MyComponent>()));

            Assert.That(TestColor.Default, Is.EqualTo((TestColor)TypeCache.GetDefaultValue(typeof(TestColor))));
            Assert.That(MyAwesomeStruct.Default, Is.EqualTo((MyAwesomeStruct)TypeCache.GetDefaultValue(typeof(MyAwesomeStruct))));
            Assert.That(MyComponent.Default, Is.EqualTo((MyComponent)TypeCache.GetDefaultValue(typeof(MyComponent))));
        }

        [Test]
        public void CanQueryIfTypeHasAttribute()
        {
            Assert.That(TypeCache.HasAttribute<int, SerializableAttribute>(), Is.True);
            Assert.That(TypeCache.HasAttribute<NoAttributes, SerializableAttribute>(), Is.False);
            Assert.That(TypeCache.HasAttribute<SomeAttributes, SerializableAttribute>(), Is.True);
        }
    }
}

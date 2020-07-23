#pragma warning disable 649
using System;
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties.UI.Internal;
using RangeAttribute = UnityEngine.RangeAttribute;

namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    class PropertyDrawerTests : WindowTestsFixtureBase
    {
        static class Types
        {
            public class WithMinField
            {
                [Min(-15.0f)] public float Min;
            }
            
            public class WithRangeField
            {
                [Range(-15.0f, 15.0f)] public float Range;
            }

            public class AllSupportedMinTypes
            {
                [Min(2)] public sbyte SByte;
                [Min(2)] public byte Byte;
                [Min(2)] public short Short;
                [Min(2)] public ushort UShort;
                [Min(2)] public int Int;
                [Min(2)] public uint UInt;
                [Min(2)] public long Long;
                [Min(2)] public ulong ULong;
                [Min(2)] public float Float;
                [Min(2)] public double Double;
            }
            
            public class AllSupportedRangeTypes
            {
                [Range(2, 4)] public sbyte SByte;
                [Range(2, 4)] public byte Byte;
                [Range(2, 4)] public short Short;
                [Range(2, 4)] public ushort UShort;
                [Range(2, 4)] public int Int;
                [Range(2, 4)] public uint UInt;
                [Range(2, 4)] public long Long;
                [Range(2, 4)] public ulong ULong;
                [Range(2, 4)] public float Float;
                [Range(2, 4)] public double Double;
            }
        }

        [Test]
        public void NumericTypes_WithRangeAttribute_AreAllSupported()
        {
            Element.SetTarget(new Types.AllSupportedRangeTypes());
            var rangeElements = Element.Query<CustomInspectorElement>().ToList();

            var expectedTypes = new Type[]
            {
                GetDrawerType<sbyte, RangeAttribute>(),
                GetDrawerType<byte, RangeAttribute>(),
                GetDrawerType<short, RangeAttribute>(),
                GetDrawerType<ushort, RangeAttribute>(),
                GetDrawerType<int, RangeAttribute>(),
                GetDrawerType<uint, RangeAttribute>(),
                GetDrawerType<long, RangeAttribute>(),
                GetDrawerType<ulong, RangeAttribute>(),
                GetDrawerType<float, RangeAttribute>(),
                GetDrawerType<double, RangeAttribute>(),
            };
            Assert.That(rangeElements.Count, Is.EqualTo(expectedTypes.Length));
            for (var i = 0; i < rangeElements.Count; ++i)
            {
                Assert.That(rangeElements[i].Inspector, Is.InstanceOf(expectedTypes[i]));
            }
        }

        [Test]
        public void NumericTypes_WithMinAttribute_AreAllSupported()
        {
            Element.SetTarget(new Types.AllSupportedMinTypes());
            var rangeElements = Element.Query<CustomInspectorElement>().ToList();
            var expectedTypes = new Type[]
            {
                GetDrawerType<sbyte, MinAttribute>(),
                GetDrawerType<byte, MinAttribute>(),
                GetDrawerType<short, MinAttribute>(),
                GetDrawerType<ushort, MinAttribute>(),
                GetDrawerType<int, MinAttribute>(),
                GetDrawerType<uint, MinAttribute>(),
                GetDrawerType<long, MinAttribute>(),
                GetDrawerType<ulong, MinAttribute>(),
                GetDrawerType<float, MinAttribute>(),
                GetDrawerType<double, MinAttribute>(),
            };
            Assert.That(rangeElements.Count, Is.EqualTo(expectedTypes.Length));
            for (var i = 0; i < rangeElements.Count; ++i)
            {
                Assert.That(rangeElements[i].Inspector, Is.InstanceOf(expectedTypes[i]));
            }
        }

        Type GetDrawerType<T, TAttribute>()
            where TAttribute : UnityEngine.PropertyAttribute
        {
            return typeof(PropertyDrawer<T, TAttribute>);
        }

        [Test]
        public void Field_WithMinAttribute_AreCorrectlyClamped()
        {
            Element.SetTarget(new Types.WithMinField());
            var rangeElement = Element.Q<CustomInspectorElement>();

            Assert.That(rangeElement, Is.Not.Null);
            Assert.That(rangeElement.childCount, Is.EqualTo(1));
            Assert.That(rangeElement.Inspector, Is.InstanceOf<PropertyDrawer<float, MinAttribute>>());
            var floatField = rangeElement.Q<FloatField>();
            floatField.value = -25.0f;
            Assert.That(floatField.value, Is.EqualTo(-15.0f));
        }

        [Test]
        public void Field_WithRangeAttribute_AreCorrectlyClamped()
        {
            Element.SetTarget(new Types.WithRangeField());
            var rangeElement = Element.Q<CustomInspectorElement>();

            Assert.That(rangeElement, Is.Not.Null);
            Assert.That(rangeElement.childCount, Is.EqualTo(1));
            Assert.That(rangeElement.Inspector, Is.InstanceOf<PropertyDrawer<float, UnityEngine.RangeAttribute>>());
            var floatField = rangeElement.Q<Slider>();
            floatField.value = -25.0f;
            Assert.That(floatField.value, Is.EqualTo(-15.0f));
            floatField.value = 25.0f;
            Assert.That(floatField.value, Is.EqualTo(15.0f));
        }
    }
}
#pragma warning restore 649
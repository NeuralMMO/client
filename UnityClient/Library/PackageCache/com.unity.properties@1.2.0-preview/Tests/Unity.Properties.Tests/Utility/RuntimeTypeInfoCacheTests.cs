using NUnit.Framework;
using Unity.Properties.Internal;

namespace Unity.Properties.Tests
{
    class RuntimeTypeInfoCacheTests
    {
        [Test]
        public void RuntimeTypeInfoCache_Primitives()
        {
            TestPrimitives<sbyte>();
            TestPrimitives<short>();
            TestPrimitives<int>();
            TestPrimitives<long>();
            TestPrimitives<byte>();
            TestPrimitives<ushort>();
            TestPrimitives<uint>();
            TestPrimitives<ulong>();
            TestPrimitives<float>();
            TestPrimitives<double>();
            TestPrimitives<bool>();
            TestPrimitives<char>();
            TestPrimitives<int>();
        }

        [Test]
        public void RuntimeTypeInfoCache_NullableTypes()
        {
            TestNullablePrimitives<sbyte?>();
            TestNullablePrimitives<short?>();
            TestNullablePrimitives<int?>();
            TestNullablePrimitives<long?>();
            TestNullablePrimitives<byte?>();
            TestNullablePrimitives<ushort?>();
            TestNullablePrimitives<uint?>();
            TestNullablePrimitives<ulong?>();
            TestNullablePrimitives<float?>();
            TestNullablePrimitives<double?>();
            TestNullablePrimitives<bool?>();
            TestNullablePrimitives<char?>();
            TestNullablePrimitives<int?>();
        }

        static void TestPrimitives<T>()
        {
            Assert.That(RuntimeTypeInfoCache<T>.IsPrimitive, Is.True);
            Assert.That(RuntimeTypeInfoCache<T>.IsValueType, Is.True);
            Assert.That(RuntimeTypeInfoCache<T>.IsAbstract, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsNullable, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsArray, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsInterface, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.CanBeNull, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsContainerType, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsEnumFlags, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsAbstractOrInterface, Is.False);
        }
        
        static void TestNullablePrimitives<T>()
        {
            Assert.That(RuntimeTypeInfoCache<T>.IsPrimitive, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsValueType, Is.True);
            Assert.That(RuntimeTypeInfoCache<T>.IsAbstract, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsNullable, Is.True);
            Assert.That(RuntimeTypeInfoCache<T>.IsArray, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsInterface, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.CanBeNull, Is.True);
            Assert.That(RuntimeTypeInfoCache<T>.IsContainerType, Is.True);
            Assert.That(RuntimeTypeInfoCache<T>.IsEnumFlags, Is.False);
            Assert.That(RuntimeTypeInfoCache<T>.IsAbstractOrInterface, Is.False);
        }
    }
}
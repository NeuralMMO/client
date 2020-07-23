using System;
using NUnit.Framework;
using UnityEngine;

namespace Unity.Entities.Tests
{
    public class TypeUtility_UnitTests
    {
        struct Data : IComponentData {}
#pragma warning disable 618 // remove once ComponentDataProxyBase is removed
        [DisallowMultipleComponent]
        [AddComponentMenu("")]
        class DataProxy : ComponentDataProxy<Data> {}

        struct SharedData : ISharedComponentData {}

        [AddComponentMenu("")]
        class SharedDataProxy : SharedComponentDataProxy<SharedData> {}

        struct BufferElement : IBufferElementData {}
        [DisallowMultipleComponent]
        class BufferProxy : DynamicBufferProxy<BufferElement> {}
#pragma warning restore 618 // remove once ComponentDataProxyBase is removed

        [TestCase(typeof(Data), ExpectedResult = typeof(DataProxy), TestName = nameof(IComponentData))]
        [TestCase(typeof(SharedData), ExpectedResult = typeof(SharedDataProxy), TestName = nameof(ISharedComponentData))]
        [TestCase(typeof(BufferElement), ExpectedResult = typeof(BufferProxy), TestName = nameof(IBufferElementData))]
        [TestCase(typeof(float), ExpectedResult = null, TestName = "Invalid data type")]
        public Type GetProxyForType_WhenArgumentIsSpecifiedDataType_ReturnsExpectedValue(Type dataType)
        {
            return TypeUtility.GetProxyForDataType(dataType);
        }
    }
}

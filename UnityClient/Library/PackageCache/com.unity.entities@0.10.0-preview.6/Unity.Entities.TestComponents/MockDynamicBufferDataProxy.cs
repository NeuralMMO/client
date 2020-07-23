using System;
using UnityEngine;

namespace Unity.Entities.Tests
{
    [Serializable]
    public struct MockDynamicBufferData : IBufferElementData
    {
        public int Value;
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    [Obsolete("MockDynamicBufferDataProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class MockDynamicBufferDataProxy : DynamicBufferProxy<MockDynamicBufferData>
    {
    }
}

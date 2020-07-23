using System;
using UnityEngine;

namespace Unity.Entities.Tests
{
    [Serializable]
    public struct MockData : IComponentData
    {
        public int Value;

        public MockData(int value) => Value = value;

        public override string ToString() => Value.ToString();
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    [Obsolete("MockDataProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class MockDataProxy : ComponentDataProxy<MockData>
    {
    }
}

using System;

namespace Unity.Entities.Tests
{
    [Serializable]
    public struct MockSharedData : ISharedComponentData
    {
        public int Value;
    }

    [UnityEngine.AddComponentMenu("")]
    [Obsolete("MockSharedDataProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class MockSharedDataProxy : SharedComponentDataProxy<MockSharedData>
    {
    }
}

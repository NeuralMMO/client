using System;

namespace Unity.Entities.Tests
{
    [Serializable]
    public struct MockSharedDisallowMultiple : ISharedComponentData
    {
        public int Value;
    }

    [UnityEngine.DisallowMultipleComponent]
    [UnityEngine.AddComponentMenu("")]
    [Obsolete("MockSharedDisallowMultipleProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class MockSharedDisallowMultipleProxy : SharedComponentDataProxy<MockSharedDisallowMultiple>
    {
    }
}

using NUnit.Framework;
using UnityEngine;
using UnityEngine.Jobs;
#pragma warning disable 649

namespace Unity.Entities.Tests
{
    class ComponentGroupTransformAccessArrayTests : ECSTestsFixture
    {
        public struct TransformAccessArrayTestTag : IComponentData
        {
        }

#pragma warning disable 618 // remove once ComponentDataProxyBase is removed
        [DisallowMultipleComponent]
        [AddComponentMenu("")]
        public class TransformAccessArrayTestTagProxy : ComponentDataProxy<TransformAccessArrayTestTag> {}
#pragma warning restore 618 // remove once ComponentDataProxyBase is removed

        [Test]
        public void EmptyTransformAccessArrayWorks()
        {
            var group = EmptySystem.GetEntityQuery(typeof(Transform), typeof(TransformAccessArrayTestTag));
            var ta = group.GetTransformAccessArray();
            Assert.AreEqual(0, ta.length);
        }

        [Test]
        public void SingleItemTransformAccessArrayWorks()
        {
            var go = new GameObject();
            go.AddComponent<TransformAccessArrayTestTagProxy>();
            var group = EmptySystem.GetEntityQuery(typeof(Transform), typeof(TransformAccessArrayTestTag));
            var ta = group.GetTransformAccessArray();
            Assert.AreEqual(1, ta.length);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AddAndGetNewTransformAccessArrayUpdatesContent()
        {
            var go = new GameObject();
            go.AddComponent<TransformAccessArrayTestTagProxy>();
            var group = EmptySystem.GetEntityQuery(typeof(Transform), typeof(TransformAccessArrayTestTag));
            var ta = group.GetTransformAccessArray();
            Assert.AreEqual(1, ta.length);

            var go2 = new GameObject();
            go2.AddComponent<TransformAccessArrayTestTagProxy>();
            ta = group.GetTransformAccessArray();
            Assert.AreEqual(2, ta.length);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(go2);
        }

        [Test]
        // The atomic safety handle of TransformAccessArrays are not invalidated when injection changes, the array represents the transforms when you got it
        public void AddAndUseOldTransformAccessArrayDoesNotUpdateContent()
        {
            var go = new GameObject();
            go.AddComponent<TransformAccessArrayTestTagProxy>();
            var group = EmptySystem.GetEntityQuery(typeof(Transform), typeof(TransformAccessArrayTestTag));
            var ta = group.GetTransformAccessArray();
            Assert.AreEqual(1, ta.length);

            var go2 = new GameObject();
            go2.AddComponent<TransformAccessArrayTestTagProxy>();
            Assert.AreEqual(1, ta.length);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(go2);
        }

        [Test]
        public void DestroyAndGetNewTransformAccessArrayUpdatesContent()
        {
            var go = new GameObject();
            go.AddComponent<TransformAccessArrayTestTagProxy>();
            var go2 = new GameObject();
            go2.AddComponent<TransformAccessArrayTestTagProxy>();

            var group = EmptySystem.GetEntityQuery(typeof(Transform), typeof(TransformAccessArrayTestTag));
            var ta = group.GetTransformAccessArray();
            Assert.AreEqual(2, ta.length);

            Object.DestroyImmediate(go);

            ta = group.GetTransformAccessArray();
            Assert.AreEqual(1, ta.length);

            Object.DestroyImmediate(go2);
        }

        [Test]
        // The atomic safety handle of TransformAccessArrays are not invalidated when injection changes, the array represents the transforms when you got it
        public void DestroyAndUseOldTransformAccessArrayDoesNotUpdateContent()
        {
            var go = new GameObject();
            go.AddComponent<TransformAccessArrayTestTagProxy>();
            var go2 = new GameObject();
            go2.AddComponent<TransformAccessArrayTestTagProxy>();

            var group = EmptySystem.GetEntityQuery(typeof(Transform), typeof(TransformAccessArrayTestTag));
            var ta = group.GetTransformAccessArray();
            Assert.AreEqual(2, ta.length);

            Object.DestroyImmediate(go);

            Assert.AreEqual(2, ta.length);

            Object.DestroyImmediate(go2);
        }
    }
}

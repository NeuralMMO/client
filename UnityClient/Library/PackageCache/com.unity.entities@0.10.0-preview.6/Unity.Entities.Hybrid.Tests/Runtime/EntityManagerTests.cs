using NUnit.Framework;
using UnityEngine;

namespace Unity.Entities.Tests
{
#pragma warning disable 618 // remove once ComponentDataProxyBase is removed
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class EcsFooTestProxy : ComponentDataProxy<EcsFooTest> {}

    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class EcsTestProxy : ComponentDataProxy<EcsTestData> {}
#pragma warning restore 618

    class EntityManagerTests : ECSTestsFixture
    {
        [Test]
        public void GetComponentObjectReturnsTheCorrectType()
        {
            var go = new GameObject();
            go.AddComponent<EcsTestProxy>();

            var component = m_Manager.GetComponentObject<Transform>(go.GetComponent<GameObjectEntity>().Entity);

            Assert.NotNull(component, "EntityManager.GetComponentObject returned a null object");
            Assert.AreEqual(typeof(Transform), component.GetType(), "EntityManager.GetComponentObject returned the wrong component type.");
            Assert.AreEqual(go.transform, component, "EntityManager.GetComponentObject returned a different copy of the component.");
        }

        [Test]
        public void GetComponentObjectThrowsIfComponentDoesNotExist()
        {
            var go = new GameObject();
            go.AddComponent<EcsTestProxy>();

            Assert.Throws<System.ArgumentException>(() => m_Manager.GetComponentObject<Rigidbody>(go.GetComponent<GameObjectEntity>().Entity));
        }

        [Test]
        public unsafe void ArchetypeIsManaged()
        {
            var types = new ComponentType[]
            {
                typeof(EcsTestData),
                typeof(ConversionTestHybridComponent),
#if !UNITY_DISABLE_MANAGED_COMPONENTS
                typeof(EcsTestManagedComponent)
#endif
            };

            var archetype = m_Manager.CreateArchetype(types).Archetype;

            Assert.IsFalse(archetype->IsManaged(ChunkDataUtility.GetIndexInTypeArray(archetype, types[0].TypeIndex)));
            Assert.IsTrue(archetype->IsManaged(ChunkDataUtility.GetIndexInTypeArray(archetype, types[1].TypeIndex)));
#if !UNITY_DISABLE_MANAGED_COMPONENTS
            Assert.IsTrue(archetype->IsManaged(ChunkDataUtility.GetIndexInTypeArray(archetype, types[2].TypeIndex)));
#endif
        }
    }
}

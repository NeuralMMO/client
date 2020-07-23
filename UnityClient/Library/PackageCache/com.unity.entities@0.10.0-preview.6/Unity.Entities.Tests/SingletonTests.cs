using System;
using NUnit.Framework;

namespace Unity.Entities.Tests
{
    class SingletonTests : ECSTestsFixture
    {
        [Test]
        public void GetSetSingleton()
        {
            m_Manager.CreateEntity(typeof(EcsTestData));

            EmptySystem.SetSingleton(new EcsTestData(10));
            Assert.AreEqual(10, EmptySystem.GetSingleton<EcsTestData>().value);
        }

        [Test]
        public void SingletonMethodsWithValidFilter_GetsAndSets()
        {
            var queryWithFilter1 = m_Manager.CreateEntityQuery(typeof(EcsTestData), typeof(SharedData1));
            queryWithFilter1.SetSharedComponentFilter(new SharedData1(1));
            var queryWithFilter2 = m_Manager.CreateEntityQuery(typeof(EcsTestData), typeof(SharedData1));
            queryWithFilter2.SetSharedComponentFilter(new SharedData1(2));

            var entity1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(SharedData1));
            m_Manager.SetComponentData(entity1, new EcsTestData(-1));
            m_Manager.SetSharedComponentData(entity1, new SharedData1(1));

            var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(SharedData1));
            m_Manager.SetComponentData(entity2, new EcsTestData(-1));
            m_Manager.SetSharedComponentData(entity2, new SharedData1(2));

            Assert.DoesNotThrow(() => queryWithFilter1.SetSingleton(new EcsTestData(1)));
            Assert.DoesNotThrow(() => queryWithFilter2.SetSingleton(new EcsTestData(2)));

            Assert.DoesNotThrow(() => queryWithFilter1.GetSingletonEntity());
            Assert.DoesNotThrow(() => queryWithFilter2.GetSingletonEntity());

            var data1 = queryWithFilter1.GetSingleton<EcsTestData>();
            Assert.AreEqual(1, data1.value);
            var data2 = queryWithFilter2.GetSingleton<EcsTestData>();
            Assert.AreEqual(2, data2.value);

            // These need to be reset or the AllSharedComponentReferencesAreFromChunks check will fail
            queryWithFilter1.ResetFilter();
            queryWithFilter2.ResetFilter();
        }

        [Test]
        public void SingletonMethodsWithInvalidFilter_Throws()
        {
            var queryWithFilterMissingEntity = m_Manager.CreateEntityQuery(typeof(EcsTestData), typeof(SharedData1));
            queryWithFilterMissingEntity.SetSharedComponentFilter(new SharedData1(1));
            var queryWithFilterWithAdditionalEntity = m_Manager.CreateEntityQuery(typeof(EcsTestData), typeof(SharedData1));
            queryWithFilterWithAdditionalEntity.SetSharedComponentFilter(new SharedData1(2));

            var entity1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(SharedData1));
            m_Manager.SetSharedComponentData(entity1, new SharedData1(2));
            var entity2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(SharedData1));
            m_Manager.SetSharedComponentData(entity2, new SharedData1(2));

            Assert.Throws<InvalidOperationException>(() => queryWithFilterMissingEntity.GetSingleton<EcsTestData>());
            Assert.Throws<InvalidOperationException>(() => queryWithFilterMissingEntity.SetSingleton(new EcsTestData(1)));
            Assert.Throws<InvalidOperationException>(() => queryWithFilterMissingEntity.GetSingletonEntity());

            Assert.Throws<InvalidOperationException>(() => queryWithFilterWithAdditionalEntity.GetSingleton<EcsTestData>());
            Assert.Throws<InvalidOperationException>(() => queryWithFilterWithAdditionalEntity.SetSingleton(new EcsTestData(1)));
            Assert.Throws<InvalidOperationException>(() => queryWithFilterWithAdditionalEntity.GetSingletonEntity());

            // These need to be reset or the AllSharedComponentReferencesAreFromChunks check will fail
            queryWithFilterMissingEntity.ResetFilter();
            queryWithFilterWithAdditionalEntity.ResetFilter();
        }

        [Test]
        public void GetSetSingletonMultipleComponents()
        {
            var entity = m_Manager.CreateEntity(typeof(EcsTestData3), typeof(EcsTestData), typeof(EcsTestData2));

            m_Manager.SetComponentData(entity, new EcsTestData(10));
            Assert.AreEqual(10, EmptySystem.GetSingleton<EcsTestData>().value);

            EmptySystem.SetSingleton(new EcsTestData2(100));
            Assert.AreEqual(100, m_Manager.GetComponentData<EcsTestData2>(entity).value0);
        }

        [Test]
        public void GetSetSingletonZeroThrows()
        {
            Assert.Throws<InvalidOperationException>(() => EmptySystem.SetSingleton(new EcsTestData()));
            Assert.Throws<InvalidOperationException>(() => EmptySystem.GetSingleton<EcsTestData>());
        }

        [Test]
        public void GetSetSingletonMultipleThrows()
        {
            m_Manager.CreateEntity(typeof(EcsTestData));
            m_Manager.CreateEntity(typeof(EcsTestData));

            Assert.Throws<InvalidOperationException>(() => EmptySystem.SetSingleton(new EcsTestData()));
            Assert.Throws<InvalidOperationException>(() => EmptySystem.GetSingleton<EcsTestData>());
        }

        [Test]
        public void RequireSingletonWorks()
        {
            EmptySystem.RequireSingletonForUpdate<EcsTestData>();
            EmptySystem.GetEntityQuery(typeof(EcsTestData2));

            m_Manager.CreateEntity(typeof(EcsTestData2));
            Assert.IsFalse(EmptySystem.ShouldRunSystem());
            m_Manager.CreateEntity(typeof(EcsTestData));
            Assert.IsTrue(EmptySystem.ShouldRunSystem());
        }

        [AlwaysUpdateSystem]
        class TestAlwaysUpdateSystem : ComponentSystem
        {
            protected override void OnUpdate()
            {
            }
        }

        [Test]
        public void RequireSingletonWithAlwaysUpdateThrows()
        {
            var system = World.CreateSystem<TestAlwaysUpdateSystem>();
            Assert.Throws<InvalidOperationException>(() => system.RequireSingletonForUpdate<EcsTestData>());
        }

        [Test]
        public void HasSingletonWorks()
        {
            Assert.IsFalse(EmptySystem.HasSingleton<EcsTestData>());
            m_Manager.CreateEntity(typeof(EcsTestData));
            Assert.IsTrue(EmptySystem.HasSingleton<EcsTestData>());
        }

        [Test]
        public void HasSingleton_ReturnsTrueWithEntityWithOnlyComponent()
        {
            Assert.IsFalse(EmptySystem.HasSingleton<EcsTestData>());

            m_Manager.CreateEntity(typeof(EcsTestData));
            Assert.IsTrue(EmptySystem.HasSingleton<EcsTestData>());

            m_Manager.CreateEntity(typeof(EcsTestData));
            Assert.IsFalse(EmptySystem.HasSingleton<EcsTestData>());
        }

        [Test]
        public void GetSingletonEntityWorks()
        {
            var entity = m_Manager.CreateEntity(typeof(EcsTestData));

            var singletonEntity = EmptySystem.GetSingletonEntity<EcsTestData>();
            Assert.AreEqual(entity, singletonEntity);
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void GetSetSingleton_ManagedComponents()
        {
            m_Manager.CreateEntity(typeof(EcsTestManagedComponent));

            const string kTestVal = "SomeString";
            EmptySystem.SetSingleton(new EcsTestManagedComponent() { value = kTestVal });
            Assert.AreEqual(kTestVal, EmptySystem.GetSingleton<EcsTestManagedComponent>().value);
        }

        [Test]
        public void HasSingletonWorks_ManagedComponents()
        {
            Assert.IsFalse(EmptySystem.HasSingleton<EcsTestManagedComponent>());
            m_Manager.CreateEntity(typeof(EcsTestManagedComponent));
            Assert.IsTrue(EmptySystem.HasSingleton<EcsTestManagedComponent>());
        }

#endif
    }
}

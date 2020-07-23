using System;
using Unity.Entities.Tests;
using Unity.PerformanceTesting;
using Unity.Collections;
using NUnit.Framework;
using Unity.Burst;
using Unity.Jobs;

namespace Unity.Entities.PerformanceTests
{
    public class SingletonAccessTestFixture : ECSTestsFixture
    {
        protected class TestComponentSystem : SystemBase
        {
            readonly int k_Count = 100000;
            EntityQuery m_Query;
            EntityQuery m_QueryWithFilter;

            protected override void OnUpdate()
            {
            }

            protected override void OnCreate()
            {
                base.OnCreate();
                m_Query = EntityManager.CreateEntityQuery(typeof(EcsTestFloatData));
                m_QueryWithFilter = EntityManager.CreateEntityQuery(typeof(EcsTestFloatData), typeof(EcsTestSharedComp));
                m_QueryWithFilter.SetSharedComponentFilter(new EcsTestSharedComp(1));
            }

            public void ClearQueries()
            {
                m_Query.Dispose();
                m_QueryWithFilter.Dispose();
            }

            public void GetSingletonTest(SingletonAccessPerformanceTests.AccessType accessType)
            {
                float accumulate = 0.0f;
                switch (accessType)
                {
                    case SingletonAccessPerformanceTests.AccessType.ThroughSystem:
                        for (int i = 0; i < k_Count; i++)
                            accumulate += GetSingleton<EcsTestFloatData>().Value;
                        break;
                    case SingletonAccessPerformanceTests.AccessType.ThroughQuery:
                        for (int i = 0; i < k_Count; i++)
                            accumulate += m_Query.GetSingleton<EcsTestFloatData>().Value;
                        break;
                    case SingletonAccessPerformanceTests.AccessType.ThroughQueryWithFilter:
                        for (int i = 0; i < k_Count; i++)
                            accumulate += m_QueryWithFilter.GetSingleton<EcsTestFloatData>().Value;
                        break;
                }
            }

            public void GetSingletonEntityTest(SingletonAccessPerformanceTests.AccessType accessType)
            {
                Entity entity;

                switch (accessType)
                {
                    case SingletonAccessPerformanceTests.AccessType.ThroughSystem:
                        for (int i = 0; i < k_Count; i++)
                            entity = GetSingletonEntity<EcsTestFloatData>();
                        break;
                    case SingletonAccessPerformanceTests.AccessType.ThroughQuery:
                        for (int i = 0; i < k_Count; i++)
                            entity = m_Query.GetSingletonEntity();
                        break;
                    case SingletonAccessPerformanceTests.AccessType.ThroughQueryWithFilter:
                        for (int i = 0; i < k_Count; i++)
                            entity = m_QueryWithFilter.GetSingletonEntity();
                        break;
                }

                for (int i = 0; i < k_Count; i++)
                    entity = GetSingletonEntity<EcsTestFloatData>();
            }

            public void HasSingletonTest()
            {
                float accumulate = 0.0f;
                for (int i = 0; i < k_Count; i++)
                    accumulate += HasSingleton<EcsTestFloatData>() ? 1.0f : 0.0f;
            }

            public void SetSingletonTest(SingletonAccessPerformanceTests.AccessType accessType)
            {
                switch (accessType)
                {
                    case SingletonAccessPerformanceTests.AccessType.ThroughSystem:
                        for (int i = 0; i < k_Count; i++)
                            SetSingleton(new EcsTestFloatData());
                        break;
                    case SingletonAccessPerformanceTests.AccessType.ThroughQuery:
                        for (int i = 0; i < k_Count; i++)
                            m_Query.SetSingleton(new EcsTestFloatData());
                        break;
                    case SingletonAccessPerformanceTests.AccessType.ThroughQueryWithFilter:
                        for (int i = 0; i < k_Count; i++)
                            m_QueryWithFilter.SetSingleton(new EcsTestFloatData());
                        break;
                }
            }
        }

        protected TestComponentSystem TestSystem => World.GetOrCreateSystem<TestComponentSystem>();
    }

    [Category("Performance")]
    public class SingletonAccessPerformanceTests : SingletonAccessTestFixture
    {
        Entity m_Entity;

        public enum AccessType
        {
            ThroughSystem,
            ThroughQuery,
            ThroughQueryWithFilter
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_Entity = m_Manager.CreateEntity(typeof(EcsTestFloatData), typeof(EcsTestSharedComp));
            m_Manager.SetSharedComponentData(m_Entity, new EcsTestSharedComp(1));
        }

        [TearDown]
        public override void TearDown()
        {
            m_Manager.DestroyEntity(m_Entity);
            TestSystem.ClearQueries();
            base.TearDown();
        }

        [Test, Performance]
        [Category("Performance")]
        public void GetSingleton([Values] AccessType accessType)
        {
            Measure.Method(() =>
            {
                TestSystem.GetSingletonTest(accessType);
            }).WarmupCount(5).MeasurementCount(100).SampleGroup("SingletonAccess").Run();
        }

        [Test, Performance]
        [Category("Performance")]
        public void GetSingletonEntity([Values] AccessType accessType)
        {
            Measure.Method(() =>
            {
                TestSystem.GetSingletonEntityTest(accessType);
            }).WarmupCount(5).MeasurementCount(100).SampleGroup("SingletonAccess").Run();
        }

        [Test, Performance]
        [Category("Performance")]
        public void HasSingleton()
        {
            Measure.Method(() =>
            {
                TestSystem.HasSingletonTest();
            }).WarmupCount(5).MeasurementCount(100).SampleGroup("SingletonAccess").Run();
        }

        [Test, Performance]
        [Category("Performance")]
        public void SetSingleton([Values] AccessType accessType)
        {
            Measure.Method(() =>
            {
                TestSystem.SetSingletonTest(accessType);
            }).WarmupCount(5).MeasurementCount(100).SampleGroup("SingletonAccess").Run();
        }
    }
}

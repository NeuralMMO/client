using System;
using Unity.Burst;
#if !UNITY_DOTSPLAYER
using NUnit.Framework;
using Unity.Jobs;
#pragma warning disable 649

namespace Unity.Entities.Tests
{
    class SystemBaseComponentAccessTests : ECSTestsFixture
    {
        SystemBase_TestSystem TestSystem;
        static Entity TestEntity1;
        static Entity TestEntity2;

        [SetUp]
        public void SetUp()
        {
            TestSystem = World.GetOrCreateSystem<SystemBase_TestSystem>();

            var myArch = m_Manager.CreateArchetype(
                ComponentType.ReadWrite<EcsTestDataEntity>(),
                ComponentType.ReadWrite<EcsTestData>());

            TestEntity1 = m_Manager.CreateEntity(myArch);
            TestEntity2 = m_Manager.CreateEntity(myArch);
            m_Manager.SetComponentData(TestEntity1, new EcsTestDataEntity() { value0 = 1, value1 = TestEntity2 });
            m_Manager.SetComponentData(TestEntity1, new EcsTestData() { value = 1 });
            m_Manager.SetComponentData(TestEntity2, new EcsTestDataEntity() { value0 = 2, value1 = TestEntity1 });
            m_Manager.SetComponentData(TestEntity2, new EcsTestData() { value = 2 });
        }

        internal enum ScheduleType
        {
            Run,
            Schedule,
            ScheduleParallel
        }

        public class SystemBase_TestSystem : SystemBase
        {
            protected override void OnUpdate() {}

            public void HasComponent_HasComponent(Entity entity, ScheduleType scheduleType)
            {
                switch (scheduleType)
                {
                    case ScheduleType.Run:
                        Entities.ForEach((ref EcsTestData td) => { td.value = HasComponent<EcsTestDataEntity>(entity) ? 333 : 0; }).Run();
                        break;
                    case ScheduleType.Schedule:
                        Entities.ForEach((ref EcsTestData td) => { td.value = HasComponent<EcsTestDataEntity>(entity) ? 333 : 0; }).Schedule();
                        break;
                    case ScheduleType.ScheduleParallel:
                        Entities.ForEach((ref EcsTestData td) => { td.value = HasComponent<EcsTestDataEntity>(entity) ? 333 : 0; }).ScheduleParallel();
                        break;
                }

                Dependency.Complete();
            }

            public void GetComponent_GetsValue(Entity entity, ScheduleType scheduleType)
            {
                switch (scheduleType)
                {
                    case ScheduleType.Run:
                        Entities.ForEach((ref EcsTestData td) => { td.value = GetComponent<EcsTestDataEntity>(entity).value0; }).Run();
                        break;
                    case ScheduleType.Schedule:
                        Entities.ForEach((ref EcsTestData td) => { td.value = GetComponent<EcsTestDataEntity>(entity).value0; }).Schedule();
                        break;
                    case ScheduleType.ScheduleParallel:
                        Entities.ForEach((ref EcsTestData td) => { td.value = GetComponent<EcsTestDataEntity>(entity).value0; }).ScheduleParallel();
                        break;
                }

                Dependency.Complete();
            }

            public void SetComponent_SetsValue(Entity entity, ScheduleType scheduleType)
            {
                switch (scheduleType)
                {
                    case ScheduleType.Run:
                        Entities.ForEach((ref EcsTestDataEntity tde) => { SetComponent(entity, new EcsTestData(){ value = 2 }); }).Run();
                        break;
                    case ScheduleType.Schedule:
                        Entities.ForEach((ref EcsTestDataEntity tde) => { SetComponent(entity, new EcsTestData(){ value = 2 }); }).Schedule();
                        break;
                    case ScheduleType.ScheduleParallel:
                        Entities.ForEach((ref EcsTestDataEntity tde) => { SetComponent(entity, new EcsTestData(){ value = 2 }); }).ScheduleParallel();
                        break;
                }

                Dependency.Complete();
            }

            public void GetComponentThroughGetComponentDataFromEntity_GetsValue(Entity entity, ScheduleType scheduleType)
            {
                switch (scheduleType)
                {
                    case ScheduleType.Run:
                        Entities.ForEach((ref EcsTestData td) => { td.value = GetComponentDataFromEntity<EcsTestDataEntity>(true)[entity].value0; }).Run();
                        break;
                    case ScheduleType.Schedule:
                        Entities.ForEach((ref EcsTestData td) => { td.value = GetComponentDataFromEntity<EcsTestDataEntity>(true)[entity].value0; }).Schedule();
                        break;
                    case ScheduleType.ScheduleParallel:
                        Entities.ForEach((ref EcsTestData td) => { td.value = GetComponentDataFromEntity<EcsTestDataEntity>(true)[entity].value0; }).ScheduleParallel();
                        break;
                }

                Dependency.Complete();
            }

            public void SetComponentThroughGetComponentDataFromEntity_SetsValue(Entity entity, ScheduleType scheduleType)
            {
                switch (scheduleType)
                {
                    case ScheduleType.Run:
                        Entities.ForEach((ref EcsTestDataEntity tde) =>
                        {
                            var cdfe = GetComponentDataFromEntity<EcsTestData>(false);
                            cdfe[entity] = new EcsTestData(){ value = 2 };
                        }).Run();
                        break;
                    case ScheduleType.Schedule:
                        Entities.ForEach((ref EcsTestDataEntity tde) =>
                        {
                            var cdfe = GetComponentDataFromEntity<EcsTestData>(false);
                            cdfe[entity] = new EcsTestData(){ value = 2 };
                        }).Schedule();
                        break;
                    case ScheduleType.ScheduleParallel:
                        Entities.ForEach((ref EcsTestDataEntity tde) =>
                        {
                            var cdfe = GetComponentDataFromEntity<EcsTestData>(false);
                            cdfe[entity] = new EcsTestData(){ value = 2 };
                        }).ScheduleParallel();
                        break;
                }

                Dependency.Complete();
            }

            static int GetComponentDataValueByMethod(ComponentDataFromEntity<EcsTestData> cdfe, Entity entity)
            {
                return cdfe[entity].value;
            }

            public void GetComponentThroughGetComponentDataFromEntityPassedToMethod_GetsValue(Entity entity)
            {
                Entities.ForEach((ref EcsTestDataEntity td) => { td.value0 = GetComponentDataValueByMethod(GetComponentDataFromEntity<EcsTestData>(true), entity); }).Run();
            }

            static void SetComponentDataValueByMethod(ComponentDataFromEntity<EcsTestData> cdfe, Entity entity, int value)
            {
                cdfe[entity] = new EcsTestData() { value = value };
            }

            public void SetComponentThroughGetComponentDataFromEntityPassedToMethod_GetsValue(Entity entity)
            {
                Entities.ForEach((ref EcsTestDataEntity td) => { SetComponentDataValueByMethod(GetComponentDataFromEntity<EcsTestData>(false), entity, 2); }).Run();
            }

            public void GetComponentFromComponentDataField_GetsValue()
            {
                Entities.ForEach((ref EcsTestDataEntity tde) => { tde.value0 = GetComponent<EcsTestData>(tde.value1).value; }).Schedule();
                Dependency.Complete();
            }

            public void GetComponentFromStaticField_GetsValue()
            {
                Entities.ForEach((ref EcsTestDataEntity tde) => { tde.value0 = GetComponent<EcsTestData>(tde.value1).value; }).Schedule();
                Dependency.Complete();
            }

            public void MultipleGetComponents_GetsValues()
            {
                Entities.ForEach((ref EcsTestDataEntity tde) =>
                {
                    tde.value0 = GetComponent<EcsTestData>(tde.value1).value + GetComponent<EcsTestData>(tde.value1).value;
                }).Schedule();
                Dependency.Complete();
            }

            public void GetComponentSetComponent_SetsValue()
            {
                Entities
                    .WithoutBurst()
                    .ForEach((Entity entity, in EcsTestDataEntity tde) =>
                    {
                        var val = GetComponent<EcsTestData>(tde.value1).value;
                        SetComponent(entity, new EcsTestData(val));
                    }).Schedule();
                Dependency.Complete();
            }

            public void GetComponentSetComponentThroughComponentDataFromEntity_SetsValue()
            {
                Entities
                    .WithoutBurst()
                    .ForEach((Entity entity, in EcsTestDataEntity tde) =>
                    {
                        var cdfeWrite = GetComponentDataFromEntity<EcsTestData>(false);
                        cdfeWrite[entity] = new EcsTestData() {value = GetComponentDataFromEntity<EcsTestData>(true)[tde.value1].value};
                    }).Schedule();
                Dependency.Complete();
            }

            public void GetComponentSetComponentThroughLocalFunction_GetsSetsValue()
            {
                Entities.ForEach((Entity entity, in EcsTestDataEntity tde) =>
                {
                    int GetEntityValue(Entity e)
                    {
                        return GetComponent<EcsTestData>(e).value;
                    }

                    void SetEntityValue(Entity e, int value)
                    {
                        SetComponent(e, new EcsTestData(value));
                    }

                    SetEntityValue(entity, GetEntityValue(tde.value1));
                }).Schedule();
                Dependency.Complete();
            }

            public void GetSameComponentInTwoEntitiesForEach_GetsValue()
            {
                Entities.ForEach((Entity entity, ref EcsTestDataEntity tde) =>
                {
                    tde.value0 += GetComponent<EcsTestData>(tde.value1).value;
                    tde.value0 += GetComponent<EcsTestData>(tde.value1).value;
                }).Schedule();
                Entities.ForEach((Entity entity, ref EcsTestDataEntity tde) =>
                {
                    tde.value0 += GetComponent<EcsTestData>(tde.value1).value;
                    tde.value0 += GetComponent<EcsTestData>(tde.value1).value;
                }).Schedule();
                Dependency.Complete();
            }

            public void GetComponentOnOtherSystemInVar_GetsValue(Entity entity)
            {
                var otherSystem = new SystemBase_TestSystem();
                Entities.ForEach((ref EcsTestData td) => { td.value = otherSystem.GetComponent<EcsTestDataEntity>(entity).value0; }).Schedule();
                Dependency.Complete();
            }

            SystemBase_TestSystem otherSystemField;
            public void GetComponentOnOtherSystemInField_GetsValue(Entity entity)
            {
                var systemField = otherSystemField;
                Entities.ForEach((ref EcsTestData td) => { td.value = systemField.GetComponent<EcsTestDataEntity>(entity).value0; }).Schedule();
                Dependency.Complete();
            }

            public void ComponentAccessInEntitiesForEachWithNestedCaptures_ComponentAccessWorks()
            {
                var outerCapture = 2;
                {
                    var innerCapture = 10;
                    Entities
                        .ForEach((Entity entity, in EcsTestDataEntity tde) =>
                    {
                        if (HasComponent<EcsTestDataEntity>(entity))
                            outerCapture = 10;

                        var val = GetComponent<EcsTestData>(tde.value1).value;
                        SetComponent(entity, new EcsTestData(val * innerCapture * outerCapture));
                    }).Run();
                }
            }

            public void GetComponentDataFromEntityInEntitiesForEachWithNestedCaptures_ComponentAccessWorks()
            {
                var outerCapture = 2;
                {
                    var innerCapture = 10;
                    Entities
                        .ForEach((Entity entity, in EcsTestDataEntity tde) =>
                    {
                        if (HasComponent<EcsTestDataEntity>(entity))
                            outerCapture = 10;

                        var cdfeRead = GetComponentDataFromEntity<EcsTestData>(true);
                        var val = cdfeRead[tde.value1].value;
                        var cdfeWrite = GetComponentDataFromEntity<EcsTestData>(false);
                        cdfeWrite[entity] = new EcsTestData(val * innerCapture * outerCapture);
                    }).Run();
                }
            }

            public void ComponentAccessMethodsExpandILPastShortBranchDistance_CausesNoExceptionsAndRuns()
            {
                Entities
                    .ForEach((Entity e, ref EcsTestData data) =>
                {
                    var a = 0;
                    if (data.value < 100)
                    {
                        if (HasComponent<EcsTestDataEntity>(e))
                            a++;
                        if (HasComponent<EcsTestDataEntity>(e))
                            a++;
                        if (HasComponent<EcsTestDataEntity>(e))
                            a++;
                        if (HasComponent<EcsTestDataEntity>(e))
                            a++;
                        if (HasComponent<EcsTestDataEntity>(e))
                            a++;
                        if (HasComponent<EcsTestDataEntity>(e))
                            a++;
                    }
                    data.value = a;
                }).Run();
            }

            public static bool StaticMethod()
            {
                return true;
            }

            public JobHandle CallsComponentAccessMethodAndExecutesStaticMethodWithBurst_CompilesAndRuns()
            {
                if (StaticMethod())
                {
                    return Entities
                        .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
                        .ForEach((in EcsTestDataEntity tde) =>
                        {
                            if (StaticMethod())
                                SetComponent(tde.value1, new EcsTestData(42));
                        }).Schedule(default);
                }

                return default;
            }
        }

        [Test]
        public void HasComponentInRun_HasComponent([Values] ScheduleType scheduleType)
        {
            TestSystem.HasComponent_HasComponent(TestEntity2, scheduleType);
            Assert.AreEqual(333, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void GetComponent_GetsValue([Values] ScheduleType scheduleType)
        {
            TestSystem.GetComponent_GetsValue(TestEntity2, scheduleType);
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void SetComponent_SetsValue([Values(ScheduleType.Run, ScheduleType.Schedule)] ScheduleType scheduleType)
        {
            TestSystem.SetComponent_SetsValue(TestEntity1, scheduleType);
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void SetComponent_Throws([Values(ScheduleType.ScheduleParallel)] ScheduleType scheduleType)
        {
            Assert.Throws<InvalidOperationException>(() => TestSystem.SetComponent_SetsValue(TestEntity1, scheduleType));
        }

        [Test]
        public void GetComponentThroughGetComponentDataFromEntity_GetsValue([Values] ScheduleType scheduleType)
        {
            TestSystem.GetComponentThroughGetComponentDataFromEntity_GetsValue(TestEntity2, scheduleType);
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void SetComponentThroughGetComponentDataFromEntity_SetsValue([Values(ScheduleType.Run, ScheduleType.Schedule)] ScheduleType scheduleType)
        {
            TestSystem.SetComponentThroughGetComponentDataFromEntity_SetsValue(TestEntity1, scheduleType);
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void SetComponentThroughGetComponentDataFromEntity_Throws([Values(ScheduleType.ScheduleParallel)] ScheduleType scheduleType)
        {
            Assert.Throws<InvalidOperationException>(() => TestSystem.SetComponentThroughGetComponentDataFromEntity_SetsValue(TestEntity1, scheduleType));
        }

        [Test]
        public void GetComponentThroughGetComponentDataFromEntityPassedToMethod_GetsValue()
        {
            TestSystem.GetComponentThroughGetComponentDataFromEntityPassedToMethod_GetsValue(TestEntity2);
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestDataEntity>(TestEntity1).value0);
        }

        [Test]
        public void SetComponentThroughGetComponentDataFromEntityPassedToMethod_GetsValue()
        {
            TestSystem.SetComponentThroughGetComponentDataFromEntityPassedToMethod_GetsValue(TestEntity1);
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void GetComponentFromComponentDataField_GetsValue()
        {
            TestSystem.GetComponentFromComponentDataField_GetsValue();
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestDataEntity>(TestEntity1).value0);
        }

        [Test]
        public void GetComponentFromStaticField_GetsValue()
        {
            TestSystem.GetComponentFromStaticField_GetsValue();
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestDataEntity>(TestEntity1).value0);
        }

        [Test]
        public void MultipleGetComponents_GetsValues()
        {
            TestSystem.MultipleGetComponents_GetsValues();
            Assert.AreEqual(4, m_Manager.GetComponentData<EcsTestDataEntity>(TestEntity1).value0);
        }

        [Test]
        public void GetComponentSetComponent_SetsValue()
        {
            TestSystem.GetComponentSetComponent_SetsValue();
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void GetComponentSetComponentThroughComponentDataFromEntity_SetsValue()
        {
            TestSystem.GetComponentSetComponentThroughComponentDataFromEntity_SetsValue();
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void GetComponentSetComponentThroughLocalFunction_GetsSetsValue()
        {
            TestSystem.GetComponentSetComponentThroughLocalFunction_GetsSetsValue();
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void GetSameComponentInTwoEntitiesForEach_GetsValue()
        {
            TestSystem.GetSameComponentInTwoEntitiesForEach_GetsValue();
            Assert.AreEqual(9, m_Manager.GetComponentData<EcsTestDataEntity>(TestEntity1).value0);
        }

        [Test]
        public void GetComponentOnOtherSystemInVar_GetsValue()
        {
            TestSystem.GetComponentOnOtherSystemInVar_GetsValue(TestEntity2);
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void GetComponentOnOtherSystemInField_GetsValue()
        {
            TestSystem.GetComponentOnOtherSystemInField_GetsValue(TestEntity2);
            Assert.AreEqual(2, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void ComponentAccessInEntitiesForEachWithNestedCaptures_ComponentAccessWorks()
        {
            TestSystem.ComponentAccessInEntitiesForEachWithNestedCaptures_ComponentAccessWorks();
            Assert.AreEqual(200, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void GetComponentDataFromEntityInEntitiesForEachWithNestedCaptures_ComponentAccessWorks()
        {
            TestSystem.GetComponentDataFromEntityInEntitiesForEachWithNestedCaptures_ComponentAccessWorks();
            Assert.AreEqual(200, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        // This test is to check that patched component access methods don't expand IL incorrectly past the limits of
        // short branch instructions.  We should call SimplifyMacros on the cloned method to ensure that we aren't
        // using short branch instructions.  If that is not happening this test will fail.
        [Test]
        public void ComponentAccessMethodsExpandILPastShortBranchDistance_CausesNoExceptionsAndRuns()
        {
            TestSystem.ComponentAccessMethodsExpandILPastShortBranchDistance_CausesNoExceptionsAndRuns();
            Assert.AreEqual(6, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }

        [Test]
        public void CallsComponentAccessMethodAndExecutesStaticMethodWithBurst_CompilesAndRuns()
        {
            TestSystem.CallsComponentAccessMethodAndExecutesStaticMethodWithBurst_CompilesAndRuns().Complete();
            Assert.AreEqual(42, m_Manager.GetComponentData<EcsTestData>(TestEntity1).value);
        }
    }
}
#endif

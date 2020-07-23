using NUnit.Framework;
using Unity.Collections;

namespace Unity.Entities.Tests
{
    [TestFixture]
    sealed class CopyEntitiesFromTests : EntityDifferTestFixture
    {
        void CreateTestData(out Entity entity, int value)
        {
            entity = SrcEntityManager.CreateEntity();
            SrcEntityManager.AddComponentData(entity, new EcsTestData(value));
            SrcEntityManager.AddSharedComponentData(entity, new EcsTestSharedComp(6));
        }

        void TestValues(Entity entity, int componentDataValue, int componentChunkValue)
        {
            Assert.AreEqual(componentDataValue, DstEntityManager.GetComponentData<EcsTestData>(entity).value);
            Assert.AreEqual(6, DstEntityManager.GetSharedComponentData<EcsTestSharedComp>(entity).value);
            Assert.AreEqual(componentChunkValue, DstEntityManager.GetChunkComponentData<EcsTestData2>(entity).value0);
        }

    #if !UNITY_DISABLE_MANAGED_COMPONENTS
        [StandaloneFixme]
        [Test]
        public void CopyEntitiesToOtherWorld()
        {
            CreateTestData(out var entity0, 5);
            CreateTestData(out var entity1, 6);

            SrcEntityManager.AddComponentData(entity0, new EcsTestManagedDataEntity("0", entity1));
            SrcEntityManager.AddComponentData(entity1, new Disabled());
            SrcEntityManager.AddComponentData(entity1, new Prefab());

            SrcEntityManager.AddChunkComponentData(SrcEntityManager.UniversalQuery, new EcsTestData2(7));

            using (var srcEntities = new NativeArray<Entity>(new[] { entity0, entity1 }, Allocator.Temp))
            using (var dstEntities = new NativeArray<Entity>(2, Allocator.Temp))
            {
                // create extra entities to ensure entity id's aren't the same by accident
                DstEntityManager.CreateEntity();

                DstEntityManager.CopyEntitiesFrom(SrcEntityManager, srcEntities, dstEntities);

                TestValues(dstEntities[0], 5, 0);
                Assert.AreEqual(dstEntities[1], DstEntityManager.GetComponentData<EcsTestManagedDataEntity>(dstEntities[0]).value1);
                Assert.AreEqual("0", DstEntityManager.GetComponentData<EcsTestManagedDataEntity>(dstEntities[0]).value0);
                Assert.IsFalse(DstEntityManager.HasComponent<EcsTestManagedDataEntity>(dstEntities[1]));

                // Prefab & Disabled tag is kept in the clone - this is a copy not instantiate semantic
                Assert.IsFalse(DstEntityManager.HasComponent<Disabled>(dstEntities[0]));
                Assert.IsFalse(DstEntityManager.HasComponent<Prefab>(dstEntities[0]));
                Assert.IsTrue(DstEntityManager.HasComponent<Disabled>(dstEntities[1]));
                Assert.IsTrue(DstEntityManager.HasComponent<Prefab>(dstEntities[1]));

                TestValues(dstEntities[1], 6, 0);
            }

            Assert.AreEqual(2, SrcEntityManager.UniversalQuery.CalculateEntityCount());
            Assert.AreEqual(3, DstEntityManager.UniversalQuery.CalculateEntityCount());

            SrcEntityManager.Debug.CheckInternalConsistency();
            DstEntityManager.Debug.CheckInternalConsistency();
        }

#endif
    }
}

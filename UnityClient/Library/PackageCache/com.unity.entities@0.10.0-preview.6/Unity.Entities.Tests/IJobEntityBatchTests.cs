using System;
using NUnit.Framework;
using Unity.Collections;


namespace Unity.Entities.Tests
{
    class IJobEntityBatchTests : ECSTestsFixture
    {
        struct WriteBatchIndex : IJobEntityBatch
        {
            public ArchetypeChunkComponentType<EcsTestData> ecsTestType;

            public void Execute(ArchetypeChunk batch, int batchIndex)
            {
                var testDataArray = batch.GetNativeArray(ecsTestType);
                testDataArray[0] = new EcsTestData
                {
                    value = batchIndex
                };
            }
        }

        [Test]
        public void IJobEntityBatchProcess()
        {
            var archetype = m_Manager.CreateArchetype(typeof(EcsTestData));
            var query = m_Manager.CreateEntityQuery(typeof(EcsTestData));

            var entityCount = 100;
            var jobsPerChunk = 4;
            var expectedEntitiesPerBatch = entityCount / jobsPerChunk;

            var entities = m_Manager.CreateEntity(archetype, entityCount, Allocator.Temp);
            var job = new WriteBatchIndex
            {
                ecsTestType = m_Manager.GetArchetypeChunkComponentType<EcsTestData>(false)
            };
            job.ScheduleParallelBatched(query, jobsPerChunk).Complete();

            for (int i = 0; i < jobsPerChunk; ++i)
            {
                Assert.AreEqual(i, m_Manager.GetComponentData<EcsTestData>(entities[i * expectedEntitiesPerBatch]).value);
            }

            query.Dispose();
        }
    }
}

using System;
using NUnit.Framework;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
#if !UNITY_DOTSPLAYER
using System.Text.RegularExpressions;
#endif
using UnityEngine.TestTools;

namespace Unity.Entities.Tests
{
    class EntityTransactionTests : ECSTestsFixture
    {
        EntityQuery m_Group;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_Group = m_Manager.CreateEntityQuery(typeof(EcsTestData));

            // Archetypes can't be created on a job
            m_Manager.CreateArchetype(typeof(EcsTestData));
        }

        struct CreateEntityAddToListJob : IJob
        {
            public ExclusiveEntityTransaction entities;
            public NativeList<Entity> createdEntities;

            public void Execute()
            {
                var entity = entities.CreateEntity(ComponentType.ReadWrite<EcsTestData>());
                entities.SetComponentData(entity, new EcsTestData(42));
                Assert.AreEqual(42, entities.GetComponentData<EcsTestData>(entity).value);

                createdEntities.Add(entity);
            }
        }

        struct CreateEntityJob : IJob
        {
            public ExclusiveEntityTransaction entities;

            public void Execute()
            {
                var entity = entities.CreateEntity(ComponentType.ReadWrite<EcsTestData>());
                entities.SetComponentData(entity, new EcsTestData(42));
                Assert.AreEqual(42, entities.GetComponentData<EcsTestData>(entity).value);
            }
        }

        [Test]
        public void CreateEntitiesChainedJob()
        {
            var job = new CreateEntityAddToListJob();
            job.entities = m_Manager.BeginExclusiveEntityTransaction();
            job.createdEntities = new NativeList<Entity>(0, Allocator.TempJob);

            m_Manager.ExclusiveEntityTransactionDependency = job.Schedule(m_Manager.ExclusiveEntityTransactionDependency);
            m_Manager.ExclusiveEntityTransactionDependency = job.Schedule(m_Manager.ExclusiveEntityTransactionDependency);

            m_Manager.EndExclusiveEntityTransaction();

            var data = m_Group.ToComponentDataArray<EcsTestData>(Allocator.TempJob);
            Assert.AreEqual(2, m_Group.CalculateEntityCount());
            Assert.AreEqual(42, data[0].value);
            Assert.AreEqual(42, data[1].value);

            Assert.IsTrue(m_Manager.Exists(job.createdEntities[0]));
            Assert.IsTrue(m_Manager.Exists(job.createdEntities[1]));

            job.createdEntities.Dispose();
            data.Dispose();
        }

        [Test]
        public void CommitAfterNotRegisteredTransactionJobLogsError()
        {
#if !UNITY_DOTSPLAYER
            var job = new CreateEntityJob();
            job.entities = m_Manager.BeginExclusiveEntityTransaction();

            var jobHandle = job.Schedule(m_Manager.ExclusiveEntityTransactionDependency);

            Assert.Throws<InvalidOperationException>(() => m_Manager.EndExclusiveEntityTransaction());

            jobHandle.Complete();

            m_Manager.EndExclusiveEntityTransaction();
#endif
        }

        [Test]
        public void EntityManagerAccessDuringTransactionThrows()
        {
            var job = new CreateEntityAddToListJob();
            job.entities = m_Manager.BeginExclusiveEntityTransaction();

            Assert.Throws<InvalidOperationException>(() => { m_Manager.CreateEntity(typeof(EcsTestData)); });

            //@TODO:
            //Assert.Throws<InvalidOperationException>(() => { m_Manager.Exists(new Entity()); });
        }

        [Test]
        public void AccessExistingEntityFromTransactionWorks()
        {
            var entity = m_Manager.CreateEntity(typeof(EcsTestData));
            m_Manager.SetComponentData(entity, new EcsTestData(42));

            var transaction = m_Manager.BeginExclusiveEntityTransaction();
            Assert.AreEqual(42, transaction.GetComponentData<EcsTestData>(entity).value);
        }

        [Test]
        [StandaloneFixme] // Needs NativeJobs schedule path
        public void MissingJobCreationDependency()
        {
            var job = new CreateEntityJob();
            job.entities = m_Manager.BeginExclusiveEntityTransaction();

            var jobHandle = job.Schedule();
            Assert.Throws<InvalidOperationException>(() => { job.Schedule(); });

            jobHandle.Complete();
        }

        [Test]
        [StandaloneFixme] // Needs NativeJobs + Safety Handles support
        public void CreationJobAndMainThreadNotAllowedInParallel()
        {
            var job = new CreateEntityJob();
            job.entities = m_Manager.BeginExclusiveEntityTransaction();

            var jobHandle = job.Schedule();

            Assert.Throws<InvalidOperationException>(() => { job.entities.CreateEntity(typeof(EcsTestData)); });

            jobHandle.Complete();
        }

        [Test]
        public void CreatingEntitiesBeyondCapacityInTransactionWorks()
        {
            var arch = m_Manager.CreateArchetype(typeof(EcsTestData));

            var transaction = m_Manager.BeginExclusiveEntityTransaction();
            var entities = new NativeArray<Entity>(1000, Allocator.Persistent);
            transaction.CreateEntity(arch, entities);
            entities.Dispose();
        }

        struct DynamicBufferElement : IBufferElementData
        {
            public int Value;
        }

        struct DynamicBufferJob : IJob
        {
            public ExclusiveEntityTransaction Transaction;
            public Entity OldEntity;
            public NativeArray<Entity> NewEntity;

            public void Execute()
            {
                NewEntity[0] = Transaction.CreateEntity(typeof(DynamicBufferElement));
                var newBuffer = Transaction.GetBuffer<DynamicBufferElement>(NewEntity[0]);

                var oldBuffer = Transaction.GetBuffer<DynamicBufferElement>(OldEntity);
                var oldArray = new NativeArray<DynamicBufferElement>(oldBuffer.Length, Allocator.Temp);
                oldBuffer.AsNativeArray().CopyTo(oldArray);

                foreach (var element in oldArray)
                {
                    newBuffer.Add(new DynamicBufferElement {Value = element.Value * 2});
                }

                oldArray.Dispose();
            }
        }

        [Test]
        public void DynamicBuffer([Values] bool mainThread)
        {
            var entity = m_Manager.CreateEntity(typeof(DynamicBufferElement));
            var buffer = m_Manager.GetBuffer<DynamicBufferElement>(entity);

            buffer.Add(new DynamicBufferElement {Value = 123});
            buffer.Add(new DynamicBufferElement {Value = 234});
            buffer.Add(new DynamicBufferElement {Value = 345});

            var newEntity = new NativeArray<Entity>(1, Allocator.TempJob);

            var job = new DynamicBufferJob();
            job.NewEntity = newEntity;
            job.Transaction = m_Manager.BeginExclusiveEntityTransaction();
            job.OldEntity = entity;

            if (mainThread)
            {
                job.Run();
            }
            else
            {
                job.Schedule().Complete();
            }

            m_Manager.EndExclusiveEntityTransaction();

            Assert.AreNotEqual(entity, job.NewEntity[0]);

            var newBuffer = m_Manager.GetBuffer<DynamicBufferElement>(job.NewEntity[0]);

            Assert.AreEqual(3, newBuffer.Length);

            Assert.AreEqual(123 * 2, newBuffer[0].Value);
            Assert.AreEqual(234 * 2, newBuffer[1].Value);
            Assert.AreEqual(345 * 2, newBuffer[2].Value);

            newEntity.Dispose();
        }

        struct SyncIJobChunk : IJobChunk
        {
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
            }
        }

        struct SyncMiddleJob : IJob
        {
            public ExclusiveEntityTransaction Txn;

            public void Execute()
            {
            }
        }

        struct SyncEntityMgrJob : IJob
        {
            public EntityManager TheManager;

            public void Execute()
            {
            }
        }

        [Test]
        [StandaloneFixme]
        public void TransactionSync1()
        {
            var top = new SyncIJobChunk {}.Schedule(m_Manager.UniversalQuery);
            Assert.Throws<InvalidOperationException>(() =>
            {
                // Cant run exclusive transaction while ijob chunk is running
                var exclusive = m_Manager.BeginExclusiveEntityTransaction();
                var middle = new SyncMiddleJob { Txn = exclusive }.Schedule(top);
            });
            top.Complete();
        }

        [Test]
        [StandaloneFixme]
        public void TransactionSync2()
        {
            var exclusive = m_Manager.BeginExclusiveEntityTransaction();
            var middle = new SyncMiddleJob { Txn = exclusive }.Schedule();
            Assert.Throws<InvalidOperationException>(() =>
            {
                // job wasn't registered & thus couldn't be synced
                m_Manager.EndExclusiveEntityTransaction();
                new SyncIJobChunk {}.Schedule(m_Manager.UniversalQuery).Complete();
            });
            middle.Complete();
        }

        [Test]
        public void TransactionSync3()
        {
            var exclusive = m_Manager.BeginExclusiveEntityTransaction();
            Assert.Throws<InvalidOperationException>(() =>
            {
                // Cant run ijob chunk while in transaction
                new SyncIJobChunk {}.Schedule(m_Manager.UniversalQuery);
            });
            m_Manager.EndExclusiveEntityTransaction();
        }

        [Test]
        [Ignore("Need additional safety handle features to be able to do this")]
        public void TransactionSync4()
        {
            var top = new SyncIJobChunk {}.Schedule(m_Manager.UniversalQuery);
            Assert.Throws<InvalidOperationException>(() =>
            {
                // Cant run exclusive transaction while ijob chunk is running
                new SyncEntityMgrJob { TheManager = m_Manager }.Schedule().Complete();
            });
            top.Complete();
        }

        [Test]
        [Ignore("Need additional safety handle features to be able to do this")]
        public void TransactionSync5()
        {
            var q = m_Manager.UniversalQuery;
            var j = new SyncEntityMgrJob { TheManager = m_Manager }.Schedule();
            Assert.Throws<InvalidOperationException>(() =>
            {
                // Can't schedule IJobChunk while entity manager belongs to job
                new SyncIJobChunk {}.Schedule(q).Complete();
            });
            j.Complete();
        }
    }
}

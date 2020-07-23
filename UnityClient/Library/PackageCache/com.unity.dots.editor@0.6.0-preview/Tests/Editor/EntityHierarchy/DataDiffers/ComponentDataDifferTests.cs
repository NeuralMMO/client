using NUnit.Framework;
using Unity.Collections;

namespace Unity.Entities.Editor.Tests
{
    [Ignore("Temporarily ignored - will be re-enabled on upcoming version including update to burst 1.3.0-preview.11 and improved EntityDiffer")]
    class ComponentDataDifferTests
    {
        World m_World;
        NativeList<Entity> m_NewEntities;
        NativeList<Entity> m_MissingEntities;
        NativeList<byte> m_Storage;
        EntityDiffer m_EntityDiffer;
        ComponentDataDiffer m_ChunkDiffer;

        [SetUp]
        public void Setup()
        {
            m_World = new World("TestWorld");
            m_NewEntities = new NativeList<Entity>(Allocator.TempJob);
            m_MissingEntities = new NativeList<Entity>(Allocator.TempJob);
            m_Storage = new NativeList<byte>(Allocator.TempJob);
            m_EntityDiffer = new EntityDiffer(m_World);
            m_ChunkDiffer = new ComponentDataDiffer(typeof(EcsTestData));
        }

        [TearDown]
        public void TearDown()
        {
            m_World.Dispose();
            m_NewEntities.Dispose();
            m_Storage.Dispose();
            m_MissingEntities.Dispose();
            m_EntityDiffer.Dispose();
            m_ChunkDiffer.Dispose();
        }

        [Test]
        public void ComponentDataDiffer_Simple()
        {
            var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle);
            jobHandle.Complete();

            Assert.That(result.AddedComponentsCount, Is.EqualTo(0));
            Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

            result.Dispose();
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectNewEmptyEntityInArchetype()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            var entityB = m_World.EntityManager.CreateEntity(typeof(EcsTestData));
            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityB, default(EcsTestData))));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectNewAndMissing()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            var entityB = CreateEntity(new EcsTestData { value = 22 });
            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.DestroyEntity(entityA);
            var entityC = CreateEntity(new EcsTestData { value = 32 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(2));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(2));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
                Assert.That(result.GetAddedComponent<EcsTestData>(1), Is.EqualTo((entityC, new EcsTestData { value = 32 })));
                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
                Assert.That(result.GetRemovedComponent<EcsTestData>(1), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_ShouldNotDetectMissingEntity()
        {
            // Entity diff is taken care of by the EntityDiffer, it's fine for this
            // differ to not detect entity changes

            var entityA = CreateEntity(new EcsTestData { value = 12 });
            var entityB = CreateEntity(new EcsTestData { value = 22 });

            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.DestroyEntity(entityB);

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
                Assert.That(result.AddedComponentsCount, Is.EqualTo(0));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectChangedAsNewAndRemoved()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });
            var entityB = CreateEntity(new EcsTestData { value = 22 });

            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.SetComponentData(entityA, new EcsTestData { value = 32 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 32 })));
                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_ResultShouldntBeInterlaced()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });
            var entityB = CreateEntity(new EcsTestData { value = 22 });
            m_World.EntityManager.AddSharedComponentData(entityA, new EcsTestSharedComp { value = 1 });
            m_World.EntityManager.AddSharedComponentData(entityB, new EcsTestSharedComp { value = 1 });
            var entityC = CreateEntity(new EcsTestData { value = 32 });
            m_World.EntityManager.AddSharedComponentData(entityC, new EcsTestSharedComp { value = 2 });

            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.RemoveComponent<EcsTestData>(entityB);
            var entityD = CreateEntity(new EcsTestData { value = 42 });
            m_World.EntityManager.AddSharedComponentData(entityD, new EcsTestSharedComp { value = 2 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityD, new EcsTestData { value = 42 })));
                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectMissingChunk()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });
            var entityInChunk = m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->GetEntityInChunk(entityA);
            Assert.That(entityInChunk.Chunk != null);

            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.DestroyEntity(entityA);
            entityInChunk = m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->GetEntityInChunk(entityA);

            Assert.That(entityInChunk.Chunk == null);

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
                Assert.That(result.AddedComponentsCount, Is.EqualTo(0));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));

                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
            }
        }

        [Test]
        public void ComponentDataDiffer_DetectChangingQuery()
        {
            using (var customQuery = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData), typeof(EcsTestData2)))
            {
                var entityA = CreateEntity(new EcsTestData { value = 12 });
                var entityB = CreateEntity(new EcsTestData { value = 22 });
                m_World.EntityManager.AddComponentData(entityB, new EcsTestData2 { value0 = 32, value1 = 42 });

                using (var result = m_ChunkDiffer.GatherComponentChangesAsync(customQuery, Allocator.TempJob, out var jobHandle))
                {
                    jobHandle.Complete();
                    Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                    Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                    Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
                }

                using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
                {
                    jobHandle.Complete();
                    Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                    Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                    Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
                }

                {
                    var result = m_ChunkDiffer.GatherComponentChangesAsync(customQuery, Allocator.TempJob, out var jobHandle);
                    jobHandle.Complete();
                    Assert.That(result.AddedComponentsCount, Is.EqualTo(0));
                    Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));

                    Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
                    result.Dispose();
                }
            }
        }

        Entity CreateEntity<T>(T data) where T : struct, IComponentData
        {
            var e = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddComponentData(e, data);

            return e;
        }
    }
}

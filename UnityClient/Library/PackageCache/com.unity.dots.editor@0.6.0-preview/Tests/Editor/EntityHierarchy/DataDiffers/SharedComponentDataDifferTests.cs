using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Unity.Collections;

namespace Unity.Entities.Editor.Tests
{
    [Ignore("Temporarily ignored - will be re-enabled on upcoming version including update to burst 1.3.0-preview.11 and improved EntityDiffer")]
    [TestFixture]
    class SharedComponentDataDifferTests
    {
        World m_World;
        SharedComponentDataDiffer m_Differ;

        [SetUp]
        public void Setup()
        {
            m_World = new World("TestWorld");
            m_Differ = new SharedComponentDataDiffer(typeof(EcsTestSharedComp));
        }

        [TearDown]
        public void TearDown()
        {
            m_World.Dispose();
            m_Differ.Dispose();
        }

        [Test]
        public void SharedComponentDataDiffer_Simple()
        {
            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(0));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(0));
            }
        }

        [Test]
        public unsafe void SharedComponentDataDiffer_DetectReplacedChunk()
        {
            var entityA = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityA, new EcsTestSharedComp { value = 1 });
            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(1));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(0));
                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityA, new EcsTestSharedComp { value = 1 })));
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.SetSharedComponentData(entityA, new EcsTestSharedComp { value = 2 });
            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(1));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(1));
                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityA, new EcsTestSharedComp { value = 2 })));
                Assert.That(result.GetRemovedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityA, new EcsTestSharedComp { value = 1 })));
            }
        }

        [Test]
        public void SharedComponentDataDiffer_DetectNewEntity()
        {
            var entityA = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityA, new EcsTestSharedComp { value = 1 });
            var entityB = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityB, new EcsTestSharedComp { value = 2 });

            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(2));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(0));
                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityA, new EcsTestSharedComp { value = 1 })));
                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(1), Is.EqualTo((entityB, new EcsTestSharedComp { value = 2 })));
            }
        }

        [Test]
        public void SharedComponentDataDiffer_DetectEntityOnDefaultComponentValue()
        {
            var archetype = m_World.EntityManager.CreateArchetype(typeof(EcsTestSharedComp));
            var entityA = m_World.EntityManager.CreateEntity(archetype);

            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(1));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(0));
                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityA, new EcsTestSharedComp { value = 0 })));
            }
        }

        [Test]
        public unsafe void SharedComponentDataDiffer_DetectNewAndMissingEntityInExistingChunk()
        {
            var entityA = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityA, new EcsTestSharedComp { value = 1 });
            var entityB = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityB, new EcsTestSharedComp { value = 1 });

            m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob).Dispose();

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.RemoveComponent<EcsTestSharedComp>(entityB);
            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(0));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(1));
                Assert.That(result.GetRemovedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityB, new EcsTestSharedComp { value = 1 })));
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            var entityC = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityC, new EcsTestSharedComp { value = 1 });
            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(1));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(0));
                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityC, new EcsTestSharedComp { value = 1 })));
            }
        }

        [Test]
        public unsafe void SharedComponentDataDiffer_DetectMovedEntitiesAsNewAndRemoved()
        {
            var entityA = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityA, new EcsTestSharedComp { value = 1 });
            var entityB = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityB, new EcsTestSharedComp { value = 1 });

            m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob).Dispose();

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.RemoveComponent<EcsTestSharedComp>(entityA);
            var entityC = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityC, new EcsTestSharedComp { value = 1 });

            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(2));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(2));

                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityB, new EcsTestSharedComp { value = 1 })));
                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(1), Is.EqualTo((entityC, new EcsTestSharedComp { value = 1 })));

                Assert.That(result.GetRemovedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityA, new EcsTestSharedComp { value = 1 })));
                Assert.That(result.GetRemovedEntities<EcsTestSharedComp>(1), Is.EqualTo((entityB, new EcsTestSharedComp { value = 1 })));
            }
        }

        [Test]
        public unsafe void SharedComponentDataDiffer_DetectMissingChunk()
        {
            var entityA = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityA, new EcsTestSharedComp { value = 1 });
            var entityB = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityB, new EcsTestSharedComp { value = 2 });

            m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob).Dispose();

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.DestroyEntity(entityB);
            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(0));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(1));
                Assert.That(result.GetRemovedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityB, new EcsTestSharedComp { value = 2 })));
            }
        }

        [Test]
        public unsafe void SharedComponentData_DetectEntityMovingFromOneChunkToAnother()
        {
            var entityA = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityA, new EcsTestSharedComp { value = 1 });
            var entityB = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddSharedComponentData(entityB, new EcsTestSharedComp { value = 2 });

            m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob).Dispose();
            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.SetSharedComponentData(entityB, new EcsTestSharedComp { value = 1 });
            using (var result = m_Differ.GatherComponentChanges(m_World.EntityManager, m_World.EntityManager.UniversalQuery, Allocator.TempJob))
            {
                Assert.That(result.AddedEntitiesCount, Is.EqualTo(1));
                Assert.That(result.RemovedEntitiesCount, Is.EqualTo(1));
                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityB, new EcsTestSharedComp { value = 1 })));
                Assert.That(result.GetRemovedEntities<EcsTestSharedComp>(0), Is.EqualTo((entityB, new EcsTestSharedComp { value = 2 })));
            }
        }

        [Test]
        public void SharedComponentDataDiffer_ResultShouldThrowIfQueryWrongType()
        {
            var result = new SharedComponentDataDiffer.ComponentChanges(TypeManager.GetTypeIndex(typeof(EcsTestSharedComp)), default, default, default, default, default);

            var ex = Assert.Throws<InvalidOperationException>(() => result.GetAddedEntities<OtherSharedComponent>(0));
            Assert.That(ex.Message, Is.EqualTo($"Unable to retrieve data for component type {typeof(OtherSharedComponent)} (type index {TypeManager.GetTypeIndex<OtherSharedComponent>()}), this container only holds data for the type with type index {TypeManager.GetTypeIndex(typeof(EcsTestSharedComp))}."));
        }

        [Test]
        public void SharedComponentDataDiffer_ResultShouldThrowOutOfRangeException()
        {
            using (var result = new SharedComponentDataDiffer.ComponentChanges(
                TypeManager.GetTypeIndex(typeof(EcsTestSharedComp)),
                new NativeList<GCHandle>(2, Allocator.Temp)
                {
                    GCHandle.Alloc(new EcsTestSharedComp { value = 1 }),
                    GCHandle.Alloc(new EcsTestSharedComp { value = 2 })
                },
                new NativeList<Entity>(1, Allocator.Temp) { new Entity { Index = 1 } },
                new NativeList<int>(1, Allocator.Temp){ 0 },
                new NativeList<Entity>(1, Allocator.Temp) { new Entity { Index = 2 } },
                new NativeList<int>(1, Allocator.Temp){ 1 }))
            {
                Assert.That(result.GetAddedEntities<EcsTestSharedComp>(0), Is.EqualTo((new Entity { Index = 1 }, new EcsTestSharedComp { value = 1 })));
                Assert.That(result.GetRemovedEntities<EcsTestSharedComp>(0), Is.EqualTo((new Entity { Index = 2 }, new EcsTestSharedComp { value = 2 })));
                Assert.Throws<IndexOutOfRangeException>(() => result.GetRemovedEntities<EcsTestSharedComp>(1));
                Assert.Throws<IndexOutOfRangeException>(() => result.GetAddedEntities<EcsTestSharedComp>(1));
            }
        }

        struct OtherSharedComponent : ISharedComponentData
        {
#pragma warning disable 649
            public int SomethingElse;
#pragma warning restore 649
        }
    }
}

using NUnit.Framework;
using Unity.Collections;

namespace Unity.Entities.Editor.Tests
{
    [Ignore("Temporarily ignored - will be re-enabled on upcoming version including update to burst 1.3.0-preview.11 and improved EntityDiffer")]
    class EntityDifferTests
    {
        World m_World;
        NativeList<Entity> m_CreatedEntitiesQueue;
        NativeList<Entity> m_DestroyedEntitiesQueue;
        EntityDiffer m_Differ;

        [SetUp]
        public void Setup()
        {
            m_World = new World("TestWorld");
            m_CreatedEntitiesQueue = new NativeList<Entity>(Allocator.TempJob);
            m_DestroyedEntitiesQueue = new NativeList<Entity>(Allocator.TempJob);
            m_Differ = new EntityDiffer(m_World);
        }

        [TearDown]
        public void TearDown()
        {
            m_World.Dispose();
            m_CreatedEntitiesQueue.Dispose();
            m_DestroyedEntitiesQueue.Dispose();
            m_Differ.Dispose();
        }

        [Test]
        public void EntityDiffer_Simple()
        {
            var(created, destroyed) = GetEntityQueryMatchDiff(m_World.EntityManager.UniversalQuery);

            Assert.That(created, Is.Empty);
            Assert.That(destroyed, Is.Empty);
        }

        [Test]
        public void EntityDiffer_HandleGrowEntityManagerCapacity()
        {
            var initialCapacity = m_World.EntityManager.EntityCapacity;
            var archetype = m_World.EntityManager.CreateArchetype(typeof(EcsTestData));
            using (var entities = m_World.EntityManager.CreateEntity(archetype, initialCapacity + 1, Allocator.TempJob))
            {
                Assert.That(m_World.EntityManager.EntityCapacity, Is.GreaterThan(initialCapacity));

                using (var query = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData)))
                {
                    var(created, _) = GetEntityQueryMatchDiff(query);
                    Assert.That(created, Is.EquivalentTo(entities.ToArray()));
                }
            }
        }

        [Test]
        public void EntityDiffer_DetectEntityChangesReusingSameQuery()
        {
            var entityA = m_World.EntityManager.CreateEntity(typeof(EcsTestData));

            using (var query = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData)))
            {
                var(created, destroyed) = GetEntityQueryMatchDiff(query);

                Assert.That(created, Is.EquivalentTo(new[] { entityA }));
                Assert.That(destroyed, Is.Empty);

                var entityB = m_World.EntityManager.CreateEntity(typeof(EcsTestData));
                m_World.EntityManager.DestroyEntity(entityA);
                (created, destroyed) = GetEntityQueryMatchDiff(query);

                Assert.That(created, Is.EquivalentTo(new[] { entityB }));
                Assert.That(destroyed, Is.EquivalentTo(new[] { entityA }));
            }
        }

        [Test]
        public void EntityDiffer_DetectEntityChanges()
        {
            var entityA = m_World.EntityManager.CreateEntity(typeof(EcsTestData));
            var entityB = m_World.EntityManager.CreateEntity(typeof(EcsTestData2));

            using (var query = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData)))
            {
                var(created, destroyed) = GetEntityQueryMatchDiff(query);

                Assert.That(created, Is.EquivalentTo(new[] { entityA }));
                Assert.That(destroyed, Is.Empty);
            }

            using (var query = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData2)))
            {
                var(created, destroyed) = GetEntityQueryMatchDiff(query);

                Assert.That(created, Is.EquivalentTo(new[] { entityB }));
                Assert.That(destroyed, Is.EquivalentTo(new[] { entityA }));
            }

            m_World.EntityManager.DestroyEntity(entityB);

            using (var query = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData2)))
            {
                var(created, destroyed) = GetEntityQueryMatchDiff(query);

                Assert.That(created, Is.Empty);
                Assert.That(destroyed, Is.EquivalentTo(new[] { entityB }));
            }
        }

        [Test]
        public void EntityDiffer_ReuseIndex()
        {
            var entityA = m_World.EntityManager.CreateEntity(typeof(EcsTestData));
            var entityB = m_World.EntityManager.CreateEntity(typeof(EcsTestData2));
            using (var query = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData)))
            {
                var(created, destroyed) = GetEntityQueryMatchDiff(query);

                Assert.That(created, Is.EquivalentTo(new[] { entityA }));
                Assert.That(destroyed, Is.Empty);
            }

            m_World.EntityManager.DestroyEntity(entityA);
            var entityB2 = m_World.EntityManager.CreateEntity(typeof(EcsTestData2));
            Assert.That(entityB2.Index, Is.EqualTo(entityA.Index));

            using (var query = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData2)))
            {
                var(created, destroyed) = GetEntityQueryMatchDiff(query);
                Assert.That(created, Is.EquivalentTo(new[] { entityB, entityB2 }));
                Assert.That(destroyed, Is.EquivalentTo(new[] { entityA }));
            }
        }

        [Test]
        public void EntityDiffer_MakeSureAllEntitiesAreProcessedWhenExecutedInDifferentBatches()
        {
            var entityA = m_World.EntityManager.CreateEntity(typeof(EcsTestData));
            var entityB = m_World.EntityManager.CreateEntity(typeof(EcsTestData));
            var entityC = m_World.EntityManager.CreateEntity(typeof(EcsTestData));
            using (var query = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData)))
            {
                var(created, destroyed) = GetEntityQueryMatchDiff(query);

                Assert.That(created, Is.EquivalentTo(new[] { entityA, entityB, entityC }));
                Assert.That(destroyed, Is.Empty);
            }
        }

        (Entity[] created, Entity[] destroyed) GetEntityQueryMatchDiff(EntityQuery query)
        {
            m_Differ.GetEntityQueryMatchDiffAsync(query, m_CreatedEntitiesQueue, m_DestroyedEntitiesQueue).Complete();
            using (var created = m_CreatedEntitiesQueue.ToArray(Allocator.TempJob))
            using (var destroyed = m_DestroyedEntitiesQueue.ToArray(Allocator.TempJob))
            {
                return (created.ToArray(), destroyed.ToArray());
            }
        }
    }
}

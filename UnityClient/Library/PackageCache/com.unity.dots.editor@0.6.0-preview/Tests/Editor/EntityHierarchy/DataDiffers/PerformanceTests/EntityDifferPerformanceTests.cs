using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.PerformanceTesting;

namespace Unity.Entities.Editor.Tests
{
    [Ignore("Temporarily ignored - will be re-enabled on upcoming version including update to burst 1.3.0-preview.11 and improved EntityDiffer")]
    [TestFixture]
    [Category("Performance")]
    class EntityDifferPerformanceTests : DifferTestFixture
    {
        [Test, Performance]
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10_000)]
        [TestCase(100_000)]
        [TestCase(250_000)]
        [TestCase(500_000)]
        [TestCase(750_000)]
        [TestCase(1_000_000)]
        public void EntityDiffer_PerformanceTest(int entityCount)
        {
            CreateEntitiesWithMockSharedComponentData(entityCount / 3, typeof(EcsTestData), typeof(EcsTestSharedComp));
            CreateEntitiesWithMockSharedComponentData(entityCount / 3, typeof(EcsTestData2), typeof(EcsTestSharedComp));
            CreateEntitiesWithMockSharedComponentData(entityCount / 3, typeof(EcsTestData), typeof(EcsTestData2), typeof(EcsTestSharedComp));
            var queries = new[]
            {
                World.EntityManager.CreateEntityQuery(typeof(EcsTestData), typeof(EcsTestData2)),
                World.EntityManager.CreateEntityQuery(typeof(EcsTestData)),
                World.EntityManager.CreateEntityQuery(typeof(EcsTestData2))
            };
            var currentQuery = 0;
            var newEntities = new NativeList<Entity>(Allocator.TempJob);
            var missingEntities = new NativeList<Entity>(Allocator.TempJob);
            var differ = new EntityDiffer(World);

            Measure.Method(() => { differ.GetEntityQueryMatchDiffAsync(queries[currentQuery % queries.Length], newEntities, missingEntities).Complete(); })
                .SetUp(() => currentQuery++)
                .SampleGroup("Batched")
                .WarmupCount(10)
                .MeasurementCount(150)
                .Run();

            for (var i = 0; i < queries.Length; i++)
            {
                queries[i].Dispose();
            }

            newEntities.Dispose();
            missingEntities.Dispose();
            differ.Dispose();
        }
    }
}

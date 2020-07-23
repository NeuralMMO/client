using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Unity.Entities.Editor.Tests
{
    public class WorldCategoryHelperTests
    {
        WorldScope m_WorldScope;

        [SetUp]
        public void Setup()
            => m_WorldScope = WorldScope.CaptureAndResetExistingWorlds();

        [TearDown]
        public void Teardown()
            => m_WorldScope.Dispose();

        [Test]
        public void WorldCategoryHelper_CategorizeWorldsInCorrectCategoryOrder()
        {
            var worldsByMainFlag = new Dictionary<WorldFlags, World[]>
            {
                { WorldFlags.Live, new[] { CreateTestWorld(WorldFlags.Editor), CreateTestWorld(WorldFlags.Simulation) } },
                { WorldFlags.Conversion, new[] { CreateTestWorld(WorldFlags.Conversion), CreateTestWorld(WorldFlags.Conversion | WorldFlags.Staging) } },
                { WorldFlags.Shadow, new[] { CreateTestWorld(WorldFlags.Shadow) } },
                { WorldFlags.Streaming, new[] { CreateTestWorld(WorldFlags.Streaming), CreateTestWorld(WorldFlags.Streaming | WorldFlags.Staging) } }
            };

            var generatedCategories = WorldCategoryHelper.Categories;
            Assert.That(generatedCategories.Select(c => c.Flag), Is.EqualTo(new[]
            {
                WorldFlags.Live, WorldFlags.Conversion, WorldFlags.Streaming, WorldFlags.Shadow
            }));

            for (var i = 0; i < generatedCategories.Length; i++)
            {
                var category = generatedCategories[i];
                Assert.That(category.Worlds, Is.EquivalentTo(worldsByMainFlag[category.Flag]));
            }
        }

        [Test]
        public void WorldCategoryHelper_SkipCategoryWhenNoWorldMatches()
        {
            var singleWorld = CreateTestWorld(WorldFlags.Shadow);

            Assert.That(WorldCategoryHelper.Categories.Single().Flag, Is.EqualTo(WorldFlags.Shadow));
            Assert.That(WorldCategoryHelper.Categories.Single().Worlds.Single(), Is.EqualTo(singleWorld));
        }

        [Test]
        public void WorldCategoryHelper_NoneAndStagingOnlyAreIgnored()
        {
            var stagingWorld = CreateTestWorld(WorldFlags.Staging);
            var noneWorld = CreateTestWorld(WorldFlags.None);
            var normalWorld = CreateTestWorld(WorldFlags.Simulation);

            Assert.That(WorldCategoryHelper.Categories.Single().Flag, Is.EqualTo(WorldFlags.Live));
            Assert.That(WorldCategoryHelper.Categories.Single().Worlds.Single(), Is.EqualTo(normalWorld));
        }

        [Test]
        public void WorldCategoryHelper_OrderWorldsByNameInCategory()
        {
            CreateTestWorld(WorldFlags.Live, "a");
            CreateTestWorld(WorldFlags.Live, "c");
            CreateTestWorld(WorldFlags.Live, "b");
            CreateTestWorld(WorldFlags.Live, "3712");

            Assert.That(WorldCategoryHelper.Categories.Single().Worlds, Is.Ordered.By(nameof(World.Name)));
        }

        [Test]
        public void WorldCategoryHelper_DetectNewAndMissingWorlds()
        {
            var w1 = CreateTestWorld(WorldFlags.Live, "b");
            Assert.That(WorldCategoryHelper.Categories.Single().Worlds, Is.EqualTo(new[] { w1 }));

            var w2 = CreateTestWorld(WorldFlags.Live, "a");
            Assert.That(WorldCategoryHelper.Categories.Single().Worlds, Is.EqualTo(new[] { w2, w1 }));

            w1.Dispose();
            var w3 = CreateTestWorld(WorldFlags.Conversion, "c");
            Assert.That(WorldCategoryHelper.Categories.Length, Is.EqualTo(2));
            Assert.That(WorldCategoryHelper.Categories[1].Flag, Is.EqualTo(WorldFlags.Conversion));
            Assert.That(WorldCategoryHelper.Categories[1].Worlds, Is.EqualTo(new[] { w3 }));
            Assert.That(WorldCategoryHelper.Categories[0].Flag, Is.EqualTo(WorldFlags.Live));
            Assert.That(WorldCategoryHelper.Categories[0].Worlds, Is.EqualTo(new[] { w2 }));
        }

        static World CreateTestWorld(WorldFlags worldFlags, string name = null)
            => new World(name ?? worldFlags.ToString(), worldFlags);
    }
}

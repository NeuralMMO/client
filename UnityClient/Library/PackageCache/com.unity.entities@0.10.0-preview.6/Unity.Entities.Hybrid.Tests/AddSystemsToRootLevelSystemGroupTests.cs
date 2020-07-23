using System.Linq;
using NUnit.Framework;

namespace Unity.Entities.Tests
{
    public class AddSystemsToRootLevelSystemGroupTests
    {
        class GetOtherSystemA : ComponentSystem
        {
            public GetOtherSystemB Other;

            protected override void OnCreate()
            {
                Other = World.GetExistingSystem<GetOtherSystemB>();
            }

            protected override void OnUpdate() {}
        }

        class GetOtherSystemB : ComponentSystem
        {
            public GetOtherSystemA Other;

            protected override void OnCreate()
            {
                Other = World.GetExistingSystem<GetOtherSystemA>();
            }

            protected override void OnUpdate() {}
        }

        [Test]
        public void CrossReferenceSystem()
        {
            var world = new World("TestWorld");
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, typeof(GetOtherSystemA), typeof(GetOtherSystemB));

            var systemA = world.GetExistingSystem<GetOtherSystemA>();
            var systemB = world.GetExistingSystem<GetOtherSystemB>();

            Assert.AreEqual(systemB, systemA.Other);
            Assert.AreEqual(systemA, systemB.Other);

            world.Dispose();
        }
    }
}

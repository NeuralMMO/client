using NUnit.Framework;
using Unity.Entities.Hybrid.Tests;
using UnityEngine.TestTools;

namespace Unity.Entities.Tests
{
    public class WorldScriptUpdateOrderTests
    {
        TestWithCustomDefaultGameObjectInjectionWorld m_DefaultWorld = default;

        [SetUp]
        public void Setup()
        {
            m_DefaultWorld.Setup();
        }

        [Test, Explicit]
        public void AddRemoveScriptUpdate()
        {
            DefaultWorldInitialization.Initialize("Test World", true);

            var newWorld = new World("WorldA");
            Assert.IsFalse(ScriptBehaviourUpdateOrder.IsWorldInPlayerLoop(newWorld));

            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(newWorld);
            Assert.IsTrue(ScriptBehaviourUpdateOrder.IsWorldInPlayerLoop(newWorld));

            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(null);
            Assert.IsFalse(ScriptBehaviourUpdateOrder.IsWorldInPlayerLoop(newWorld));

            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.DefaultGameObjectInjectionWorld);
            Assert.IsTrue(ScriptBehaviourUpdateOrder.IsWorldInPlayerLoop(World.DefaultGameObjectInjectionWorld));
        }

        [TearDown]
        public void TearDown()
        {
            m_DefaultWorld.TearDown();
        }
    }
}

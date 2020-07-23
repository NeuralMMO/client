using System;
using Unity.Entities;

namespace Unity.Entities.Hybrid.Tests
{
    [Serializable]
    public struct TestWithCustomDefaultGameObjectInjectionWorld
    {
        public World PreviousGameObjectInjectionWorld;

        public void Setup()
        {
            PreviousGameObjectInjectionWorld = World.DefaultGameObjectInjectionWorld;
            World.DefaultGameObjectInjectionWorld = null;
        }

        public void TearDown()
        {
            if (World.DefaultGameObjectInjectionWorld != null)
                World.DefaultGameObjectInjectionWorld.Dispose();
            if (PreviousGameObjectInjectionWorld != null && !PreviousGameObjectInjectionWorld.IsCreated)
                PreviousGameObjectInjectionWorld = null;
            World.DefaultGameObjectInjectionWorld = PreviousGameObjectInjectionWorld;
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(PreviousGameObjectInjectionWorld);
        }
    }
}

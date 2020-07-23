using System;
using NUnit.Framework;
using Unity.Burst;

namespace Unity.Entities.Tests
{
    [BurstCompile]
    public class EntityManagerUnmanagedTests : ECSTestsFixture
    {
        private struct MyUnmanagedSystem2 : ISystemBase
        {
            public int UpdateCount;
            public int Dummy;

            public void OnCreate(ref SystemState state)
            {
            }

            public void OnDestroy(ref SystemState state)
            {
            }

            public void OnUpdate(ref SystemState state)
            {
                ++UpdateCount;
            }
        }

        [BurstCompile]
        private struct MyUnmanagedSystem2WithBurst : ISystemBase
        {
            public int UpdateCount;
            public int Dummy;

            public void OnCreate(ref SystemState state)
            {
            }

            public void OnDestroy(ref SystemState state)
            {
            }

            [BurstCompile(CompileSynchronously = true)]
            public void OnUpdate(ref SystemState state)
            {
                ++UpdateCount;
            }
        }

        [Test]
        [StandaloneFixme] // Need to initialize SystemBaseRegistry on startup
        public void UnmanagedSystemLifetime()
        {
            SystemRef<MyUnmanagedSystem2> sysRef = default;
            Assert.Throws<InvalidOperationException>(() => World.ResolveSystem(sysRef));

            using (var world = new World("Temp"))
            {
                Assert.Throws<InvalidOperationException>(() => World.ResolveSystem(sysRef));
                sysRef = world.AddSystem<MyUnmanagedSystem2>();
                Assert.IsTrue(world.IsSystemValid(sysRef));
                Assert.IsFalse(World.IsSystemValid(sysRef));
                ref var sys = ref world.ResolveSystem(sysRef);
            }

            Assert.IsFalse(World.IsSystemValid(sysRef));
            Assert.Throws<InvalidOperationException>(() => World.ResolveSystem(sysRef));
        }

        [Test]
        [StandaloneFixme] // Need to initialize SystemBaseRegistry on startup
        public void UnmanagedSystemLookup()
        {
            var sys1 = World.AddSystem<MyUnmanagedSystem2>();
            var sys2 = World.AddSystem<MyUnmanagedSystem2>();

            ref var ref1 = ref World.ResolveSystem(sys1);
            ref var ref2 = ref World.ResolveSystem(sys2);

            ref1.Dummy = 19;
            ref2.Dummy = -19;

            // We don't know which one we'll get currently, but the point is there will be two.
            Assert.AreEqual(19, Math.Abs(World.GetExistingSystem<MyUnmanagedSystem2>().Dummy));

            World.DestroySystem(sys1);

            Assert.AreEqual(19, Math.Abs(World.GetExistingSystem<MyUnmanagedSystem2>().Dummy));

            World.DestroySystem(sys2);

            Assert.Throws<InvalidOperationException>(() => World.GetExistingUnmanagedSystem<MyUnmanagedSystem2>());
        }

        [Test]
        [StandaloneFixme] // Need to initialize SystemBaseRegistry on startup
        public unsafe void RegistryCallManagedToManaged()
        {
            var sysId = World.AddSystem<MyUnmanagedSystem2>();
            var statePtr = World.ResolveSystemUntyped(sysId);
            SystemBaseRegistry.CallOnUpdate(statePtr);
            ref var sys = ref World.ResolveSystem(sysId);
            Assert.AreEqual(1, sys.UpdateCount);
        }

        [Test]
        [StandaloneFixme] // Need to initialize SystemBaseRegistry on startup
        public unsafe void RegistryCallManagedToBurst()
        {
            var sysId = World.AddSystem<MyUnmanagedSystem2WithBurst>();
            var statePtr = World.ResolveSystemUntyped(sysId);
            SystemBaseRegistry.CallOnUpdate(statePtr);
            ref var sys = ref World.ResolveSystem(sysId);
            Assert.AreEqual(1, sys.UpdateCount);
        }

        internal unsafe delegate void DispatchDelegate(SystemState* state);

        [BurstCompile(CompileSynchronously = true)]
        private unsafe static void DispatchUpdate(SystemState* state)
        {
            SystemBase.UnmanagedUpdate(state, out _);
        }

#if !NET_DOTS
        [Test]
        public unsafe void RegistryCallBurstToManaged()
        {
            var sysId = World.AddSystem<MyUnmanagedSystem2>();
            var statePtr = World.ResolveSystemUntyped(sysId);
            BurstCompiler.CompileFunctionPointer<DispatchDelegate>(DispatchUpdate).Invoke(statePtr);
            ref var sys = ref World.ResolveSystem(sysId);
            Assert.AreEqual(1, sys.UpdateCount);
        }

        [Test]
        public unsafe void RegistryCallBurstToBurst()
        {
            var sysId = World.AddSystem<MyUnmanagedSystem2WithBurst>();
            var statePtr = World.ResolveSystemUntyped(sysId);
            BurstCompiler.CompileFunctionPointer<DispatchDelegate>(DispatchUpdate).Invoke(statePtr);
            ref var sys = ref World.ResolveSystem(sysId);
            Assert.AreEqual(1, sys.UpdateCount);
        }

#endif

        private class SnoopGroup : ComponentSystemGroup
        {
        }

        [BurstCompile]
        private struct SnoopSystemBase : ISystemBase
        {
            internal static int Flags = 0;

            public void OnCreate(ref SystemState state)
            {
                Flags |= 1;
            }

            public void OnUpdate(ref SystemState state)
            {
                Flags |= 2;
            }

            public void OnDestroy(ref SystemState state)
            {
                Flags |= 4;
            }
        }

        [Test]
        [StandaloneFixme] // Need to initialize SystemBaseRegistry on startup
        public void UnmanagedSystemUpdate()
        {
            SnoopSystemBase.Flags = 0;

            using (var world = new World("Temp"))
            {
                var group = world.GetOrCreateSystem<SimulationSystemGroup>();

                var sysRef = world.AddSystem<SnoopSystemBase>();

                @group.AddSystemToUpdateList(sysRef);

                Assert.AreEqual(1, SnoopSystemBase.Flags, "OnCreate was not called");

                world.Update();

                Assert.AreEqual(3, SnoopSystemBase.Flags, "OnUpdate was not called");
            }

            Assert.AreEqual(7, SnoopSystemBase.Flags, "OnDestroy was not called");
        }
    }
}

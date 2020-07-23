using Unity.Entities;
using Unity.Jobs;

[assembly: DisableAutoCreation]

namespace Unity.Entities.CodeGen.Tests
{
    public class TestJobComponentSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps) => default;
    }

    public class TestSystemBase : SystemBase
    {
        protected override void OnUpdate() {}
    }
}

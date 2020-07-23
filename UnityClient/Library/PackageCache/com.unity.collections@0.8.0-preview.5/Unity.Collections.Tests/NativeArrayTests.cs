using System;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.Tests;
using Unity.Jobs;

internal class NativeArrayTests
{
    [Test]
    public void NativeArray_DisposeJob()
    {
        var container = new NativeArray<int>(1, Allocator.Persistent);
        Assert.True(container.IsCreated);
        Assert.DoesNotThrow(() => { container[0] = 1; });

        var disposeJob = container.Dispose(default);
        Assert.False(container.IsCreated);
        Assert.Throws<InvalidOperationException>(() => { container[0] = 2; });

        disposeJob.Complete();
    }

    [BurstCompile(CompileSynchronously = true)]
    struct NativeArrayPokeJob : IJob
    {
        NativeArray<int> array;

        public NativeArrayPokeJob(NativeArray<int> array) { this.array = array; }

        public void Execute()
        {
            array[0] = 1;
        }
    }

#if UNITY_2020_1_OR_NEWER
    [Test]
    public void NativeArray_DisposeJobWithMissingDependencyThrows()
    {
        var array = new NativeArray<int>(1, Allocator.Persistent);
        var deps = new NativeArrayPokeJob(array).Schedule();
        Assert.Throws<InvalidOperationException>(() => { array.Dispose(default); });
        deps.Complete();
        array.Dispose();
    }

#endif

    [Test, DotsRuntimeIgnore]
    public void NativeArray_DisposeJobCantBeScheduled()
    {
        var array = new NativeArray<int>(1, Allocator.Persistent);
        var deps = array.Dispose(default);
        Assert.Throws<InvalidOperationException>(() => { new NativeArrayPokeJob(array).Schedule(deps); });
        deps.Complete();
    }
}

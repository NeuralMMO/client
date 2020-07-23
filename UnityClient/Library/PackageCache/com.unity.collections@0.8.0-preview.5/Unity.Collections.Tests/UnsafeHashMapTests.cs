using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;

internal class UnsafeHashMapTests
{
    // Burst error BC1071: Unsupported assert type
    // [BurstCompile(CompileSynchronously = true)]
    public struct UnsafeHashMapAddJob : IJob
    {
        public UnsafeHashMap<int, int>.ParallelWriter Writer;

        public void Execute()
        {
            Assert.True(Writer.TryAdd(123, 1));
        }
    }

    [Test]
    public void UnsafeHashMap_AddJob()
    {
        var hashMap = new UnsafeHashMap<int, int>(32, Allocator.TempJob);

        var job = new UnsafeHashMapAddJob()
        {
            Writer = hashMap.AsParallelWriter(),
        };

        job.Schedule().Complete();

        Assert.True(hashMap.ContainsKey(123));

        hashMap.Dispose();
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct UnsafeMultiHashMapAddJob : IJobParallelFor
    {
        public UnsafeMultiHashMap<int, int>.ParallelWriter Writer;

        public void Execute(int index)
        {
            Writer.Add(123, index);
        }
    }

    [Test]
    public void UnsafeMultiHashMap_AddJob()
    {
        var hashMap = new UnsafeMultiHashMap<int, int>(32, Allocator.TempJob);

        var job = new UnsafeMultiHashMapAddJob()
        {
            Writer = hashMap.AsParallelWriter(),
        };

        job.Schedule(3, 1).Complete();

        Assert.True(hashMap.ContainsKey(123));
        Assert.AreEqual(hashMap.CountValuesForKey(123), 3);

        hashMap.Dispose();
    }
}

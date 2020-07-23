using NUnit.Framework;
using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.Tests;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
internal class NativeMultiHashMapTests_JobDebugger : NativeMultiHashMapTestsFixture
{
    [Test, DotsRuntimeIgnore]
    public void NativeMultiHashMap_Read_And_Write_Without_Fences()
    {
        var hashMap = new NativeMultiHashMap<int, int>(hashMapSize, Allocator.TempJob);
        var writeStatus = new NativeArray<int>(hashMapSize, Allocator.TempJob);
        var readValues = new NativeArray<int>(hashMapSize, Allocator.TempJob);

        var writeData = new MultiHashMapWriteParallelForJob()
        {
            hashMap = hashMap.AsParallelWriter(),
            status = writeStatus,
            keyMod = hashMapSize,
        };

        var readData = new MultiHashMapReadParallelForJob()
        {
            hashMap = hashMap,
            values = readValues,
            keyMod = writeData.keyMod,
        };

        var writeJob = writeData.Schedule(hashMapSize, 1);
        Assert.Throws<InvalidOperationException>(() => { readData.Schedule(hashMapSize, 1); });
        writeJob.Complete();

        hashMap.Dispose();
        writeStatus.Dispose();
        readValues.Dispose();
    }
}
#endif

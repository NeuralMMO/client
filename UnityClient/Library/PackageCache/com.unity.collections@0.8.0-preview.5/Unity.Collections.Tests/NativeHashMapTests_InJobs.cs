using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.Tests;

internal class NativeHashMapTests_InJobs : NativeHashMapTestsFixture
{
    [Test]
    public void NativeHashMap_Read_And_Write()
    {
        var hashMap = new NativeHashMap<int, int>(hashMapSize, Allocator.TempJob);
        var writeStatus = new NativeArray<int>(hashMapSize, Allocator.TempJob);
        var readValues = new NativeArray<int>(hashMapSize, Allocator.TempJob);

        var writeData = new HashMapWriteJob()
        {
            hashMap = hashMap.AsParallelWriter(),
            status = writeStatus,
            keyMod = hashMapSize,
        };

        var readData = new HashMapReadParallelForJob()
        {
            hashMap = hashMap,
            values = readValues,
            keyMod = writeData.keyMod,
        };

        var writeJob = writeData.Schedule();
        var readJob = readData.Schedule(hashMapSize, 1, writeJob);
        readJob.Complete();

        for (int i = 0; i < hashMapSize; ++i)
        {
            Assert.AreEqual(0, writeStatus[i], "Job failed to write value to hash map");
            Assert.AreEqual(i, readValues[i], "Job failed to read from hash map");
        }

        hashMap.Dispose();
        writeStatus.Dispose();
        readValues.Dispose();
    }

    [Test]
    public void NativeHashMap_Read_And_Write_Full()
    {
        var hashMap = new NativeHashMap<int, int>(hashMapSize / 2, Allocator.TempJob);
        var writeStatus = new NativeArray<int>(hashMapSize, Allocator.TempJob);
        var readValues = new NativeArray<int>(hashMapSize, Allocator.TempJob);

        var writeData = new HashMapWriteJob()
        {
            hashMap = hashMap.AsParallelWriter(),
            status = writeStatus,
            keyMod = hashMapSize,
        };

        var readData = new HashMapReadParallelForJob()
        {
            hashMap = hashMap,
            values = readValues,
            keyMod = writeData.keyMod,
        };

        var writeJob = writeData.Schedule();
        var readJob = readData.Schedule(hashMapSize, 1, writeJob);
        readJob.Complete();

        var missing = new HashSet<int>();
        for (int i = 0; i < hashMapSize; ++i)
        {
            if (writeStatus[i] == -2)
            {
                missing.Add(i);
                Assert.AreEqual(-1, readValues[i], "Job read a value form hash map which should not be there");
            }
            else
            {
                Assert.AreEqual(0, writeStatus[i], "Job failed to write value to hash map");
                Assert.AreEqual(i, readValues[i], "Job failed to read from hash map");
            }
        }
        Assert.AreEqual(hashMapSize - hashMapSize / 2, missing.Count, "Wrong indices written to hash map");

        hashMap.Dispose();
        writeStatus.Dispose();
        readValues.Dispose();
    }

    [Test]
    public void NativeHashMap_Key_Collisions()
    {
        var hashMap = new NativeHashMap<int, int>(hashMapSize, Allocator.TempJob);
        var writeStatus = new NativeArray<int>(hashMapSize, Allocator.TempJob);
        var readValues = new NativeArray<int>(hashMapSize, Allocator.TempJob);

        var writeData = new HashMapWriteJob()
        {
            hashMap = hashMap.AsParallelWriter(),
            status = writeStatus,
            keyMod = 16,
        };

        var readData = new HashMapReadParallelForJob()
        {
            hashMap = hashMap,
            values = readValues,
            keyMod = writeData.keyMod,
        };

        var writeJob = writeData.Schedule();
        var readJob = readData.Schedule(hashMapSize, 1, writeJob);
        readJob.Complete();

        var missing = new HashSet<int>();
        for (int i = 0; i < hashMapSize; ++i)
        {
            if (writeStatus[i] == -1)
            {
                missing.Add(i);
                Assert.AreNotEqual(i, readValues[i], "Job read a value form hash map which should not be there");
            }
            else
            {
                Assert.AreEqual(0, writeStatus[i], "Job failed to write value to hash map");
                Assert.AreEqual(i, readValues[i], "Job failed to read from hash map");
            }
        }
        Assert.AreEqual(hashMapSize - writeData.keyMod, missing.Count, "Wrong indices written to hash map");

        hashMap.Dispose();
        writeStatus.Dispose();
        readValues.Dispose();
    }

    [BurstCompile(CompileSynchronously = true)]
    struct Clear : IJob
    {
        public NativeHashMap<int, int> hashMap;

        public void Execute()
        {
            hashMap.Clear();
        }
    }

    [Test]
    public void NativeHashMap_Clear_And_Write()
    {
        var hashMap = new NativeHashMap<int, int>(hashMapSize / 2, Allocator.TempJob);
        var writeStatus = new NativeArray<int>(hashMapSize, Allocator.TempJob);

        var clearJob = new Clear
        {
            hashMap = hashMap
        };

        var clearJobHandle = clearJob.Schedule();

        var writeJob = new HashMapWriteJob
        {
            hashMap = hashMap.AsParallelWriter(),
            status = writeStatus,
            keyMod = hashMapSize,
        };

        var writeJobHandle = writeJob.Schedule(clearJobHandle);
        writeJobHandle.Complete();

        writeStatus.Dispose();
        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_DisposeJob()
    {
        var container0 = new NativeHashMap<int, int>(1, Allocator.Persistent);
        Assert.True(container0.IsCreated);
        Assert.DoesNotThrow(() => { container0.Add(0, 1); });
        Assert.True(container0.ContainsKey(0));

        var container1 = new NativeMultiHashMap<int, int>(1, Allocator.Persistent);
        Assert.True(container1.IsCreated);
        Assert.DoesNotThrow(() => { container1.Add(1, 2); });
        Assert.True(container1.ContainsKey(1));

        var disposeJob0 = container0.Dispose(default);
        Assert.False(container0.IsCreated);
        Assert.Throws<InvalidOperationException>(() => { container0.ContainsKey(0); });

        var disposeJob = container1.Dispose(disposeJob0);
        Assert.False(container1.IsCreated);
        Assert.Throws<InvalidOperationException>(() => { container1.ContainsKey(1); });

        disposeJob.Complete();
    }

    [Test, DotsRuntimeIgnore]
    public void NativeHashMap_DisposeJobWithMissingDependencyThrows()
    {
        var hashMap = new NativeHashMap<int, int>(hashMapSize / 2, Allocator.TempJob);
        var deps = new Clear { hashMap = hashMap }.Schedule();
        Assert.Throws<InvalidOperationException>(() => { hashMap.Dispose(default); });
        deps.Complete();
        hashMap.Dispose();
    }

    [Test, DotsRuntimeIgnore]
    public void NativeHashMap_DisposeJobCantBeScheduled()
    {
        var hashMap = new NativeHashMap<int, int>(hashMapSize / 2, Allocator.TempJob);
        var deps = hashMap.Dispose(default);
        Assert.Throws<InvalidOperationException>(() => { new Clear { hashMap = hashMap }.Schedule(deps); });
        deps.Complete();
    }

    [BurstCompile(CompileSynchronously = true)]
#pragma warning disable 618 // RemovedAfter 2020-07-07
    struct MergeSharedValues : IJobNativeMultiHashMapMergedSharedKeyIndices
#pragma warning restore 618 // RemovedAfter 2020-07-07
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<int> sharedCount;

        [NativeDisableParallelForRestriction]
        public NativeArray<int> sharedIndices;

        public void ExecuteFirst(int index)
        {
            sharedIndices[index] = index;
        }

        public void ExecuteNext(int firstIndex, int index)
        {
            sharedIndices[index] = firstIndex;
            sharedCount[firstIndex]++;
        }
    }

    [Test]
    public void NativeHashMap_MergeCountShared()
    {
        var count = 1024;
        var sharedKeyCount = 16;
        var sharedCount = new NativeArray<int>(count, Allocator.TempJob);
        var sharedIndices = new NativeArray<int>(count, Allocator.TempJob);
        var totalSharedCount = new NativeArray<int>(1, Allocator.TempJob);
        var hashMap = new NativeMultiHashMap<int, int>(count, Allocator.TempJob);

        for (int i = 0; i < count; i++)
        {
            hashMap.Add(i & (sharedKeyCount - 1), i);
            sharedCount[i] = 1;
        }

        var mergeSharedValuesJob = new MergeSharedValues
        {
            sharedCount = sharedCount,
            sharedIndices = sharedIndices,
        };

        var mergetedSharedValuesJobHandle = mergeSharedValuesJob.Schedule(hashMap, 64);
        mergetedSharedValuesJobHandle.Complete();

        for (int i = 0; i < count; i++)
        {
            Assert.AreEqual(count / sharedKeyCount, sharedCount[sharedIndices[i]]);
        }

        sharedCount.Dispose();
        sharedIndices.Dispose();
        totalSharedCount.Dispose();
        hashMap.Dispose();
    }
}

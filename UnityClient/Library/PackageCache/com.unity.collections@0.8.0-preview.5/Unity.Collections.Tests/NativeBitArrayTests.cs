using NUnit.Framework;
using System;
using System.Text.RegularExpressions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.TestTools;

internal class NativeBitArrayTests
{
    [Test]
    public void NativeBitArray_Get_Set()
    {
        var numBits = 256;

        var test = new NativeBitArray(numBits, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        Assert.False(test.IsSet(123));
        test.Set(123, true);
        Assert.True(test.IsSet(123));

        Assert.False(test.TestAll(0, numBits));
        Assert.False(test.TestNone(0, numBits));
        Assert.True(test.TestAny(0, numBits));
        Assert.AreEqual(1, test.CountBits(0, numBits));

        Assert.False(test.TestAll(0, 122));
        Assert.True(test.TestNone(0, 122));
        Assert.False(test.TestAny(0, 122));

        test.Clear();
        Assert.False(test.IsSet(123));
        Assert.AreEqual(0, test.CountBits(0, numBits));

        test.SetBits(40, true, 4);
        Assert.AreEqual(4, test.CountBits(0, numBits));

        test.SetBits(0, true, numBits);
        Assert.False(test.TestNone(0, numBits));
        Assert.True(test.TestAll(0, numBits));

        test.SetBits(0, false, numBits);
        Assert.True(test.TestNone(0, numBits));
        Assert.False(test.TestAll(0, numBits));

        test.SetBits(123, true, 7);
        Assert.True(test.TestAll(123, 7));

        test.Clear();
        test.SetBits(64, true, 64);
        Assert.AreEqual(false, test.IsSet(63));
        Assert.AreEqual(true, test.TestAll(64, 64));
        Assert.AreEqual(false, test.IsSet(128));
        Assert.AreEqual(64, test.CountBits(64, 64));
        Assert.AreEqual(64, test.CountBits(0, numBits));

        test.Clear();
        test.SetBits(65, true, 62);
        Assert.AreEqual(false, test.IsSet(64));
        Assert.AreEqual(true, test.TestAll(65, 62));
        Assert.AreEqual(false, test.IsSet(127));
        Assert.AreEqual(62, test.CountBits(64, 64));
        Assert.AreEqual(62, test.CountBits(0, numBits));

        test.Clear();
        test.SetBits(66, true, 64);
        Assert.AreEqual(false, test.IsSet(65));
        Assert.AreEqual(true, test.TestAll(66, 64));
        Assert.AreEqual(false, test.IsSet(130));
        Assert.AreEqual(64, test.CountBits(66, 64));
        Assert.AreEqual(64, test.CountBits(0, numBits));

        test.Dispose();
    }

    [Test]
    public unsafe void NativeBitArray_Throws()
    {
        var numBits = 256;

        Assert.Throws<ArgumentException>(() => { new NativeBitArray(numBits, Allocator.None); });

        using (var test = new NativeBitArray(numBits, Allocator.Persistent, NativeArrayOptions.ClearMemory))
        {
            Assert.DoesNotThrow(() => { test.TestAll(0, numBits); });
            Assert.DoesNotThrow(() => { test.TestAny(numBits - 1, numBits); });

            Assert.Throws<ArgumentException>(() => { test.IsSet(-1); });
            Assert.Throws<ArgumentException>(() => { test.IsSet(numBits); });
            Assert.Throws<ArgumentException>(() => { test.TestAny(0, 0); });
            Assert.Throws<ArgumentException>(() => { test.TestAny(numBits, 1); });
            Assert.Throws<ArgumentException>(() => { test.TestAny(numBits - 1, 0); });

            // GetBits numBits must be 1-64.
            Assert.Throws<ArgumentException>(() => { test.GetBits(0, 0); });
            Assert.Throws<ArgumentException>(() => { test.GetBits(0, 65); });
            Assert.DoesNotThrow(() => { test.GetBits(63, 2); });
        }
    }

    void GetBitsTest(ref NativeBitArray test, int pos, int numBits)
    {
        test.SetBits(pos, true, numBits);
        Assert.AreEqual(numBits, test.CountBits(0, test.Length));
        Assert.AreEqual(0xfffffffffffffffful >> (64 - numBits), test.GetBits(pos, numBits));
        test.Clear();
    }

    [Test]
    public void NativeBitArray_GetBits()
    {
        var numBits = 256;

        var test = new NativeBitArray(numBits, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        GetBitsTest(ref test, 0, 5);
        GetBitsTest(ref test, 1, 3);
        GetBitsTest(ref test, 1, 64);
        GetBitsTest(ref test, 62, 5);
        GetBitsTest(ref test, 127, 3);
        GetBitsTest(ref test, 250, 6);
        GetBitsTest(ref test, 254, 2);

        test.Dispose();
    }

    static void SetBitsTest(ref NativeBitArray test, int pos, ulong value, int numBits)
    {
        test.SetBits(pos, value, numBits);
        Assert.AreEqual(value, test.GetBits(pos, numBits));
        test.Clear();
    }

    [Test]
    public void NativeBitArray_SetBits()
    {
        var numBits = 256;

        var test = new NativeBitArray(numBits, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        SetBitsTest(ref test, 0, 16, 5);
        SetBitsTest(ref test, 1, 7, 3);
        SetBitsTest(ref test, 1, 32, 64);
        SetBitsTest(ref test, 62, 6, 5);
        SetBitsTest(ref test, 127, 1, 3);
        SetBitsTest(ref test, 60, 0xaa, 8);

        test.Dispose();
    }

    // Burst error BC1071: Unsupported assert type
    // [BurstCompile(CompileSynchronously = true)]
    struct NativeBitArrayTestParallelReader : IJob
    {
        [ReadOnly]
        public NativeBitArray reader;

        public void Execute()
        {
            var rd = reader;
            Assert.Throws<InvalidOperationException>(() => { rd.Set(7, false); });
            Assert.True(reader.IsSet(7));
        }
    }

    [Test]
    public void NativeBitArray_ParallelReader()
    {
        var numBits = 256;

        var reader = new NativeBitArray(numBits, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        reader.Set(7, true);

        var readerJob = new NativeBitArrayTestParallelReader { reader = reader }.Schedule();

        reader.Dispose(readerJob);
        readerJob.Complete();
    }

#if UNITY_2020_1_OR_NEWER
    [Test]
    public void NativeBitArray_UseAfterFree_UsesCustomOwnerTypeName()
    {
        var numBits = 256;

        var test = new NativeBitArray(numBits, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        SetBitsTest(ref test, 0, 16, 5);
        test.Dispose();

        Assert.That(() => test.IsSet(0), Throws.InvalidOperationException.With.Message.Contains($"The {test.GetType()} has been deallocated"));
    }

    [Test]
    public void NativeBitArray_AtomicSafetyHandle_AllocatorTemp_UniqueStaticSafetyIds()
    {
        var numBits = 256;

        var test = new NativeBitArray(numBits, Allocator.Temp, NativeArrayOptions.ClearMemory);

        // All collections that use Allocator.Temp share the same core AtomicSafetyHandle.
        // This test verifies that containers can proceed to assign unique static safety IDs to each
        // AtomicSafetyHandle value, which will not be shared by other containers using Allocator.Temp.
        var test0 = new NativeBitArray(numBits, Allocator.Temp, NativeArrayOptions.ClearMemory);
        var test1 = new NativeBitArray(numBits, Allocator.Temp, NativeArrayOptions.ClearMemory);
        SetBitsTest(ref test0, 0, 16, 5);
        test0.Dispose();
        Assert.That(() => test0.IsSet(0), Throws.InvalidOperationException.With.Message.Contains($"The {test0.GetType()} has been deallocated"));
        SetBitsTest(ref test1, 0, 16, 5);
        test1.Dispose();
        Assert.That(() => test1.IsSet(0), Throws.InvalidOperationException.With.Message.Contains($"The {test1.GetType()} has been deallocated"));
    }

    [BurstCompile(CompileSynchronously = true)]
    struct NativeBitArrayCreateAndUseAfterFreeBurst : IJob
    {
        public void Execute()
        {
            var numBits = 256;

            var test = new NativeBitArray(numBits, Allocator.Temp, NativeArrayOptions.ClearMemory);
            SetBitsTest(ref test, 0, 16, 5);
            test.Dispose();

            SetBitsTest(ref test, 0, 16, 5);
        }
    }

    [Test]
    public void NativeBitArray_CreateAndUseAfterFreeInBurstJob_UsesCustomOwnerTypeName()
    {
        // Make sure this isn't the first container of this type ever created, so that valid static safety data exists
        var numBits = 256;

        var test = new NativeBitArray(numBits, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        test.Dispose();

        var job = new NativeBitArrayCreateAndUseAfterFreeBurst
        {
        };

        // Two things:
        // 1. This exception is logged, not thrown; thus, we use LogAssert to detect it.
        // 2. Calling write operation after container.Dispose() emits an unintuitive error message. For now, all this test cares about is whether it contains the
        //    expected type name.
        job.Run();
        LogAssert.Expect(LogType.Exception,
            new Regex($"InvalidOperationException: The {Regex.Escape(test.GetType().ToString())} has been declared as \\[ReadOnly\\] in the job, but you are writing to it"));
    }

#endif
}

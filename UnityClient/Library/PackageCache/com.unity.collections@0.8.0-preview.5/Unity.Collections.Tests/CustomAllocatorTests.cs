#if !NET_DOTS // can't use Burst function pointers from DOTS runtime (yet)
#define CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
#endif

using System;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

public class CustomAllocatorTests
{
    [Test]
    public void AllocatesAndFreesFromMono()
    {
        AllocatorManager.Initialize();
        const int kLength = 100;
        for (int i = 0; i < kLength; ++i)
        {
            using (var block = AllocatorManager.Persistent.Allocate<int>(Items: i))
            {
                Assert.AreNotEqual(IntPtr.Zero, block.Range.Pointer);
                Assert.AreEqual(i, block.Range.Items);
                Assert.AreEqual(UnsafeUtility.SizeOf<int>(), block.BytesPerItem);
                Assert.AreEqual(UnsafeUtility.AlignOf<int>(), block.Alignment);
                Assert.AreEqual(AllocatorManager.Persistent.Value, block.Range.Allocator.Value);
            }
        }
        AllocatorManager.Shutdown();
    }

#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
    [BurstCompile(CompileSynchronously = true)]
#endif
    struct AllocateJob : IJobParallelFor
    {
        public NativeArray<AllocatorManager.Block> m_blocks;
        public void Execute(int index)
        {
            m_blocks[index] = AllocatorManager.Persistent.Allocate<int>(Items: index);
        }
    }

#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
    [BurstCompile(CompileSynchronously = true)]
#endif
    struct FreeJob : IJobParallelFor
    {
        public NativeArray<AllocatorManager.Block> m_blocks;
        public void Execute(int index)
        {
            var temp = m_blocks[index];
            temp.Free();
            m_blocks[index] = temp;
        }
    }

#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
    [Test]
    public void AllocatesAndFreesFromBurst()
    {
        AllocatorManager.Initialize();

        const int kLength = 100;
        using (var blocks = new NativeArray<AllocatorManager.Block>(kLength, Allocator.Persistent))
        {
            var allocateJob = new AllocateJob();
            allocateJob.m_blocks = blocks;
            allocateJob.Schedule(kLength, 1).Complete();

            for (int i = 0; i < kLength; ++i)
            {
                var block = allocateJob.m_blocks[i];
                Assert.AreNotEqual(IntPtr.Zero, block.Range.Pointer);
                Assert.AreEqual(i, block.Range.Items);
                Assert.AreEqual(UnsafeUtility.SizeOf<int>(), block.BytesPerItem);
                Assert.AreEqual(UnsafeUtility.AlignOf<int>(), block.Alignment);
                Assert.AreEqual(AllocatorManager.Persistent.Value, block.Range.Allocator.Value);
            }

            var freeJob = new FreeJob();
            freeJob.m_blocks = blocks;
            freeJob.Schedule(kLength, 1).Complete();

            for (int i = 0; i < kLength; ++i)
            {
                var block = allocateJob.m_blocks[i];
                Assert.AreEqual(IntPtr.Zero, block.Range.Pointer);
                Assert.AreEqual(0, block.Range.Items);
                Assert.AreEqual(UnsafeUtility.SizeOf<int>(), block.BytesPerItem);
                Assert.AreEqual(UnsafeUtility.AlignOf<int>(), block.Alignment);
                Assert.AreEqual(AllocatorManager.Persistent.Value, block.Range.Allocator.Value);
            }
        }

        AllocatorManager.Shutdown();
    }

#endif

#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
    [BurstCompile(CompileSynchronously = true)]
#endif
    //
    // This allocator wraps UnsafeUtility.Malloc, but also initializes memory to some constant value after allocating.
    //
    struct ClearToValueAllocator : AllocatorManager.IAllocator, IDisposable
    {
        public byte ClearValue;

        /// <summary>
        /// Upper limit on how many bytes this allocator is allowed to allocate.
        /// </summary>
        public long budgetInBytes;
        public long BudgetInBytes => budgetInBytes;

        /// <summary>
        /// Number of currently allocated bytes for this allocator.
        /// </summary>
        public long allocatedBytes;
        public long AllocatedBytes => allocatedBytes;
        public unsafe int Try(ref AllocatorManager.Block block)
        {
            var temp = block.Range.Allocator;
            block.Range.Allocator = AllocatorManager.Persistent;
            var error = AllocatorManager.Try(ref block);
            allocatedBytes += block.Bytes;
            block.Range.Allocator = temp;
            if (error != 0)
                return error;
            if (block.Range.Pointer != IntPtr.Zero) // if we allocated or reallocated...
                UnsafeUtility.MemSet((void*)block.Range.Pointer, ClearValue, block.Bytes); // clear to a value.
            return 0;
        }

#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
        [BurstCompile(CompileSynchronously = true)]
#endif
        public static unsafe int Try(IntPtr state, ref AllocatorManager.Block block)
        {
            return ((ClearToValueAllocator*)state)->Try(ref block);
        }

        public AllocatorManager.TryFunction Function => Try;
        public void Dispose()
        {
        }
    }

    [Test]
    public void UserDefinedAllocatorWorks()
    {
        var customhandle = new AllocatorManager.AllocatorHandle { Value = AllocatorManager.FirstUserIndex };
        AllocatorManager.Initialize();
        using (var installation = new AllocatorManager.AllocatorInstallation<ClearToValueAllocator>(customhandle))
        {
            installation.Allocator.budgetInBytes = 100;
            for (byte ClearValue = 0; ClearValue < 0xF; ++ClearValue)
            {
                installation.Allocator.ClearValue = ClearValue;
                const int kLength = 100;
                for (int i = 1; i < kLength; ++i)
                {
                    using (var block = customhandle.Allocate<int>(Items: i))
                    {
                        Assert.AreNotEqual(IntPtr.Zero, block.Range.Pointer);
                        Assert.AreEqual(i, block.Range.Items);
                        Assert.AreEqual(UnsafeUtility.SizeOf<int>(), block.BytesPerItem);
                        Assert.AreEqual(UnsafeUtility.AlignOf<int>(), block.Alignment);
                        unsafe
                        {
                            Assert.AreEqual(ClearValue, *(byte*)block.Range.Pointer);
                        }
                    }
                }
            }
        }
        AllocatorManager.Shutdown();
    }

    // this is testing for the case where we want to install a stack allocator that itself allocates from a big hunk
    // of memory provided by the default Persistent allocator, and then make allocations on the stack allocator.
    [Test]
    public void StackAllocatorWorks()
    {
        var customhandle = new AllocatorManager.AllocatorHandle { Value = AllocatorManager.FirstUserIndex };
        AllocatorManager.Initialize();
        using (var storage = AllocatorManager.Persistent.Allocate<byte>(100000)) // allocate a block of bytes from Malloc.Persistent
        using (var installation = new AllocatorManager.AllocatorInstallation<AllocatorManager.StackAllocator>(customhandle))  // and make a stack allocator from it
        {
            installation.Allocator.m_storage = storage; // install the block into the allocator
            const int kLength = 100;
            for (int i = 1; i < kLength; ++i)
            {
                var block = customhandle.Allocate<int>(Items: i);
                Assert.AreNotEqual(IntPtr.Zero, block.Range.Pointer);
                Assert.AreEqual(i, block.Range.Items);
                Assert.AreEqual(UnsafeUtility.SizeOf<int>(), block.BytesPerItem);
                Assert.AreEqual(UnsafeUtility.AlignOf<int>(), block.Alignment);
            }
        }
        AllocatorManager.Shutdown();
    }

    // this is testing for the case where we want to install a custom allocator that clears memory to a constant
    // byte value, and then have an UnsafeList use that custom allocator.
    [Test]
    public void CustomAllocatorUnsafeListWorks()
    {
        var customhandle = new AllocatorManager.AllocatorHandle { Value = AllocatorManager.FirstUserIndex };
        AllocatorManager.Initialize();
        using (var installation = new AllocatorManager.AllocatorInstallation<ClearToValueAllocator>(customhandle))
        {
            installation.Allocator.budgetInBytes = 100;
            for (byte ClearValue = 0; ClearValue < 0xF; ++ClearValue)
            {
                installation.Allocator.ClearValue = ClearValue;
                using (var unsafelist = new UnsafeList<byte>(1, customhandle))
                {
                    const int kLength = 100;
                    unsafelist.Resize(kLength);
                    for (int i = 0; i < kLength; ++i)
                        Assert.AreEqual(ClearValue, unsafelist[i]);
                }
            }
        }
        AllocatorManager.Shutdown();
    }

    [Test]
    public void SlabAllocatorWorks()
    {
        var SlabSizeInBytes = 256;
        var SlabSizeInInts = SlabSizeInBytes / sizeof(int);
        var Slabs = 256;
        var customhandle = new AllocatorManager.AllocatorHandle { Value = AllocatorManager.FirstUserIndex };
        AllocatorManager.Initialize();
        using (var storage = AllocatorManager.Persistent.Allocate<byte>(Items: Slabs * SlabSizeInBytes)) // allocate a block of bytes from Malloc.Persistent
        using (var installation = new AllocatorManager.AllocatorInstallation<AllocatorManager.SlabAllocator>(customhandle))  // and make a slab allocator from it
        {
            installation.Allocator = new AllocatorManager.SlabAllocator(storage, SlabSizeInBytes, Slabs * SlabSizeInBytes);

            var block0 = customhandle.Allocate<int>(Items: SlabSizeInInts);
            Assert.AreNotEqual(IntPtr.Zero, block0.Range.Pointer);
            Assert.AreEqual(SlabSizeInInts, block0.Range.Items);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>(), block0.BytesPerItem);
            Assert.AreEqual(UnsafeUtility.AlignOf<int>(), block0.Alignment);
            Assert.AreEqual(1, installation.Allocator.Occupied[0]);

            var block1 = customhandle.Allocate<int>(Items: SlabSizeInInts - 1);
            Assert.AreNotEqual(IntPtr.Zero, block1.Range.Pointer);
            Assert.AreEqual(SlabSizeInInts - 1, block1.Range.Items);
            Assert.AreEqual(UnsafeUtility.SizeOf<int>(), block1.BytesPerItem);
            Assert.AreEqual(UnsafeUtility.AlignOf<int>(), block1.Alignment);
            Assert.AreEqual(3, installation.Allocator.Occupied[0]);

            block0.Dispose();
            Assert.AreEqual(2, installation.Allocator.Occupied[0]);
            block1.Dispose();
            Assert.AreEqual(0, installation.Allocator.Occupied[0]);

            Assert.Throws<ArgumentException>(() =>
            {
                customhandle.Allocate<int>(Items: 65);
            });
        }
        AllocatorManager.Shutdown();
    }
}

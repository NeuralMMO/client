using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Rendering;

namespace Unity.Rendering.Tests
{
    public class HeapAllocatorTests
    {
        [Test]
        public void BasicTests()
        {
            var allocator = new HeapAllocator(1000);

            Assert.AreEqual(allocator.FreeSpace, 1000);

            HeapBlock b10  = allocator.Allocate(10);
            HeapBlock b100 = allocator.Allocate(100);

            // Check that the allocations have sufficient size.
            Assert.GreaterOrEqual(b10.Length, 10);
            Assert.GreaterOrEqual(b100.Length, 100);

            // Check that the amount of free space has decreased accordingly.
            Assert.LessOrEqual(allocator.FreeSpace, 1000 - 100 - 10);

            allocator.Release(b10);
            allocator.Release(b100);

            // Everything should now be freed.
            Assert.AreEqual(allocator.FreeSpace, 1000);
            allocator.Dispose();
        }

        [Test]
        public void AllocateEntireHeap()
        {
            var allocator = new HeapAllocator(100);
            // Check that it's possible to allocate the entire heap.
            Assert.AreEqual(allocator.Allocate(100).Length, 100);
            allocator.Dispose();
        }

        [Test]
        public void Coalescing()
        {
            var allocator = new HeapAllocator(100);

            // Try to allocate ten blocks. These should succeed, because the heap should not be fragmented yet.
            var blocks10 = Enumerable.Range(0, 10).Select(x => allocator.Allocate(10)).ToArray();
            Assert.IsTrue(blocks10.All(b => b.Length == 10));
            Assert.IsTrue(allocator.Full);

            // Release all of them.
            foreach (var b in blocks10) allocator.Release(b);

            // Now try to allocate the entire heap. It should succeed, because everything has been freed.
            Assert.AreEqual(allocator.Allocate(100).Length, 100);
            allocator.Dispose();
        }

        [Test]
        public void RandomStressTest()
        {
            const int HeapSize = 1_000_000;
            const int NumBlocks = 1_000;
            const int NumRounds = 20;
            const int MaxAlloc = 10_000;
            const int OperationsPerRound = 10_000;

            int numAllocs   = 0;
            int numReleases = 0;
            int numFailed = 0;

            var rnd = new System.Random(293875);
            var allocator = new HeapAllocator(HeapSize);
            var blocks = Enumerable.Range(0, NumBlocks).Select(x => new HeapBlock()).ToArray();

            // Stress test the allocator by doing a bunch of random allocs and deallocs and
            // try to verify that allocator internal asserts don't fire, and free space behaves
            // as expected.

            for (int i = 0; i < NumRounds; ++i)
            {
                Assert.IsTrue(allocator.Empty);

                // Perform random alloc/dealloc operations
                for (int j = 0; j < OperationsPerRound; ++j)
                {
                    ulong before = allocator.FreeSpace;

                    int b = rnd.Next(NumBlocks);

                    int size = 0;
                    if (blocks[b].Empty)
                    {
                        size = rnd.Next(1, MaxAlloc);
                        blocks[b] = allocator.Allocate((ulong)size);

                        if (blocks[b].Empty)
                        {
                            size = 0;
                            ++numFailed;
                        }
                        else
                        {
                            size = (int)blocks[b].Length;
                        }

                        ++numAllocs;
                    }
                    else
                    {
                        size = -(int)blocks[b].Length;
                        allocator.Release(blocks[b]);
                        blocks[b] = new HeapBlock();

                        ++numReleases;
                    }

                    ulong after = allocator.FreeSpace;

                    Assert.AreEqual((long)after, (long)before - size);
                }

                for (int b = 0; b < NumBlocks; ++b)
                {
                    if (!blocks[b].Empty)
                    {
                        allocator.Release(blocks[b]);
                        blocks[b] = new HeapBlock();
                    }
                }
                Assert.IsTrue(allocator.Empty);
            }

            Debug.Log($"Allocs: {numAllocs}, Releases: {numReleases}, Failed: {numFailed}");
            allocator.Dispose();
        }

        [Test]
        public void AllocationsDontOverlap()
        {
            // Make sure that allocations given by the allocator are disjoint (i.e. don't alias).

            const int HeapSize = 1_000_000;
            const int NumBlocks = 1_000;
            const int MaxAlloc = 10_000;
            const int OperationsPerRound = 10_000;

            var rnd = new System.Random(9283572);
            var allocator = new HeapAllocator(HeapSize);
            var blocks = Enumerable.Range(0, NumBlocks).Select(x => new HeapBlock()).ToArray();
            var inUse = new ulong[HeapSize / 8 + 1];

            Func<ulong, (ulong, int)> qword = (ulong i) => (i / 64, (int)(i % 64));

            // Perform random alloc/dealloc operations
            for (int i = 0; i < OperationsPerRound; ++i)
            {
                int b = rnd.Next(NumBlocks);
                const ulong kAllOnes = ~0UL;

                int size = 0;
                if (blocks[b].Empty)
                {
                    size = rnd.Next(1, MaxAlloc);
                    blocks[b] = allocator.Allocate((ulong)size);

                    // Mark the block as allocated, and check that it wasn't allocated.

                    // Do tests and sets entire qwords at a time so it's fast
                    var begin = qword(blocks[b].begin);
                    var end   = qword(blocks[b].end);

                    if (begin.Item1 == end.Item1)
                    {
                        ulong qw = begin.Item1;
                        ulong mask = kAllOnes << begin.Item2;
                        mask &= ~(kAllOnes << end.Item2);
                        Assert.IsTrue((inUse[qw] & mask) == 0, "Elements were already allocated");
                        inUse[qw] |= mask;
                    }
                    else
                    {
                        ulong qw = begin.Item1;
                        ulong mask = kAllOnes << begin.Item2;

                        Assert.IsTrue((inUse[qw] & mask) == 0, "Elements were already allocated");
                        inUse[qw] |= mask;

                        for (qw = begin.Item1 + 1; qw < end.Item1; ++qw)
                        {
                            mask = kAllOnes;
                            Assert.IsTrue((inUse[qw] & mask) == 0, "Elements were already allocated");
                            inUse[qw] |= mask;
                        }

                        qw = end.Item1;
                        mask = ~(kAllOnes << end.Item2);
                        Assert.IsTrue((inUse[qw] & mask) == 0, "Elements were already allocated");
                        inUse[qw] |= mask;
                    }
                }
                else
                {
                    allocator.Release(blocks[b]);

                    // Mark the block as not allocated, and check that it was allocated.

                    var begin = qword(blocks[b].begin);
                    var end   = qword(blocks[b].end);

                    if (begin.Item1 == end.Item1)
                    {
                        ulong qw = begin.Item1;
                        ulong mask = kAllOnes << begin.Item2;
                        mask &= ~(kAllOnes << end.Item2);
                        Assert.IsTrue((inUse[qw] & mask) == mask, "Elements were not allocated");
                        inUse[qw] &= ~mask;
                    }
                    else
                    {
                        ulong qw = begin.Item1;
                        ulong mask = kAllOnes << begin.Item2;

                        Assert.IsTrue((inUse[qw] & mask) == mask, "Elements were not allocated");
                        inUse[qw] &= ~mask;

                        for (qw = begin.Item1 + 1; qw < end.Item1; ++qw)
                        {
                            mask = kAllOnes;
                            Assert.IsTrue((inUse[qw] & mask) == mask, "Elements were not allocated");
                            inUse[qw] &= ~mask;
                        }

                        qw = end.Item1;
                        mask = ~(kAllOnes << end.Item2);
                        Assert.IsTrue((inUse[qw] & mask) == mask, "Elements were not allocated");
                        inUse[qw] &= ~mask;
                    }

                    blocks[b] = new HeapBlock();
                }
            }

            allocator.Dispose();
        }
    }
}

// #define DEBUG_ASSERTS

using UnityEngine;
using UnityEngine.Assertions;
using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Rendering
{
    [System.Diagnostics.DebuggerDisplay("({begin}, {end}), Length = {Length}")]
    public struct HeapBlock : IComparable<HeapBlock>, IEquatable<HeapBlock>
    {
        public ulong begin;
        public ulong end;

        public HeapBlock(ulong begin, ulong end)
        {
            this.begin = begin;
            this.end = end;
        }

        public static HeapBlock OfSize(ulong begin, ulong size)
        {
            return new HeapBlock(begin, begin + size);
        }

        public ulong Length { get { return end - begin; } }
        public bool Empty { get { return Length == 0; } }

        public int CompareTo(HeapBlock other) { return begin.CompareTo(other.begin); }
        public bool Equals(HeapBlock other) { return CompareTo(other) == 0; }
    }

    // Generic best-fit heap allocation algorithm that operates on abstract integer indexes,
    // Can be used to suballocate memory, GPU buffer contents, DX12 descriptors, etc.
    // Supports alignments, resizing, coalescing of freed blocks.
    public struct HeapAllocator : IDisposable
    {
        // Create a new HeapAllocator with the given initial size and alignment.
        // The allocator can be resized later.
        public HeapAllocator(ulong size = 0, uint minimumAlignment = 1)
        {
            m_SizeBins = new NativeList<SizeBin>(Allocator.Persistent);
            m_Blocks = new NativeList<BlocksOfSize>(Allocator.Persistent);
            m_FreeEndpoints = new NativeHashMap<ulong, ulong>(0, Allocator.Persistent);
            m_Size = 0;
            m_Free = 0;
            m_MinimumAlignmentLog2 = math.tzcnt(minimumAlignment);
            m_IsCreated = true;

            Resize(size);
        }

        public uint MinimumAlignment { get { return 1u << m_MinimumAlignmentLog2; } }
        public ulong FreeSpace { get { return m_Free; } }
        public ulong UsedSpace { get { return m_Size - m_Free; } }
        public ulong Size { get { return m_Size; } }
        public bool Empty { get { return m_Free == m_Size; } }
        public bool Full { get { return m_Free == 0; } }
        public bool IsCreated { get { return m_IsCreated; } }

        public void Clear()
        {
            var size = m_Size;

            m_SizeBins.Clear();
            m_Blocks.Clear();
            m_FreeEndpoints.Clear();
            m_Size = 0;
            m_Free = 0;

            Resize(size);
        }

        public void Dispose()
        {
            if (!IsCreated)
                return;

            for (int i = 0; i < m_Blocks.Length; ++i)
                m_Blocks[i].Dispose();

            m_FreeEndpoints.Dispose();
            m_Blocks.Dispose();
            m_SizeBins.Dispose();
            m_IsCreated = false;
        }

        // Attempt to grow or shrink the allocator. Growing always succeeds,
        // but shrinking might fail if the end of the heap is allocated.
        // TODO: Shrinking not implemented.
        public bool Resize(ulong newSize)
        {
            // Same size? No need to do anything.
            if (newSize == m_Size)
            {
                return true;
            }
            // Growing? Release a block past the end.
            else if (newSize > m_Size)
            {
                ulong increase = newSize - m_Size;
                HeapBlock newSpace = HeapBlock.OfSize(m_Size, increase);
                Release(newSpace);
                m_Size = newSize;
                return true;
            }
            // Shrinking? TODO
            else
            {
                return false;
            }
        }

        // Attempt to allocate a block from the heap with at least the given
        // size and alignment. The allocated block might be bigger than the
        // requested size, but will never be smaller.
        // If the allocation fails, an empty block is returned.
        public HeapBlock Allocate(ulong size, uint alignment = 1)
        {
            // Always use at least the minimum alignment, and round all sizes
            // to multiples of the minimum alignment.
            size = NextAligned(size, m_MinimumAlignmentLog2);
            alignment = math.max(alignment, MinimumAlignment);

            SizeBin allocBin = new SizeBin(size, alignment);

            int index = FindSmallestSufficientBin(allocBin);
            while (index < m_SizeBins.Length)
            {
                SizeBin bin = m_SizeBins[index];
                if (CanFitAllocation(allocBin, bin))
                {
                    HeapBlock block = PopBlockFromBin(bin, index);
                    return CutAllocationFromBlock(allocBin, block);
                }
                else
                {
                    ++index;
                }
            }

            return new HeapBlock();
        }

        public void Release(HeapBlock block)
        {
            // Merge the newly released block with any free blocks on either
            // side of it. Remove those blocks from the list of free blocks,
            // as they no longer exist as separate blocks.
            block = Coalesce(block);

            SizeBin bin = new SizeBin(block);
            int index = FindSmallestSufficientBin(bin);

            // If the exact bin doesn't exist, add it.
            if (index >= m_SizeBins.Length || bin.CompareTo(m_SizeBins[index]) != 0)
            {
                bin.blocksId = m_Blocks.Length;
                m_Blocks.Add(new BlocksOfSize(0));
                index = AddNewBin(bin, index);
            }

            m_Blocks[m_SizeBins[index].blocksId].Push(block);
            m_Free += block.Length;

#if DEBUG_ASSERTS
            Assert.IsFalse(m_FreeEndpoints.ContainsKey(block.begin));
            Assert.IsFalse(m_FreeEndpoints.ContainsKey(block.end));
#endif

            // Store both endpoints of the free block to the hashmap for
            // easy coalescing.
            m_FreeEndpoints[block.begin] = block.end;
            m_FreeEndpoints[block.end]   = block.begin;
        }

        public const int MaxAlignmentLog2 = 0x3f;
        public const int AlignmentBits = 6;

        [System.Diagnostics.DebuggerDisplay("Size = {Size}, Alignment = {Alignment}")]
        private struct SizeBin : IComparable<SizeBin>, IEquatable<SizeBin>
        {
            public ulong sizeClass;
            public int   blocksId;

            public SizeBin(ulong size, uint alignment = 1)
            {
                int alignLog2 = math.tzcnt(alignment);
                alignLog2 = math.min(MaxAlignmentLog2, alignLog2);
                sizeClass = (size << AlignmentBits) | (uint)alignLog2;
                blocksId  = -1;

#if DEBUG_ASSERTS
                Assert.AreEqual(math.countbits(alignment), 1, "Only power-of-two alignments supported");
#endif
            }

            public SizeBin(HeapBlock block)
            {
                int alignLog2 = math.tzcnt(block.begin);
                alignLog2 = math.min(MaxAlignmentLog2, alignLog2);
                sizeClass = (block.Length << AlignmentBits) | (uint)alignLog2;
                blocksId  = -1;
            }

            public int CompareTo(SizeBin other) { return sizeClass.CompareTo(other.sizeClass); }
            public bool Equals(SizeBin other) { return CompareTo(other) == 0; }

            public bool HasCompatibleAlignment(SizeBin requiredAlignment)
            {
                int myAlign = AlignmentLog2;
                int required = requiredAlignment.AlignmentLog2;
                return myAlign >= required;
            }

            public ulong Size { get { return sizeClass >> AlignmentBits; } }
            public int AlignmentLog2 { get { return (int)sizeClass & MaxAlignmentLog2; } }
            public uint Alignment { get { return 1u << AlignmentLog2; } }
        }

        private unsafe struct BlocksOfSize : IDisposable
        {
            private UnsafeList *m_Blocks;

            public BlocksOfSize(int dummy)
            {
                m_Blocks = (UnsafeList*)UnsafeUtility.Malloc(
                    UnsafeUtility.SizeOf<UnsafeList>(),
                    UnsafeUtility.AlignOf<UnsafeList>(),
                    Allocator.Persistent);
                UnsafeUtility.MemClear(m_Blocks, UnsafeUtility.SizeOf<UnsafeList>());
                m_Blocks->Allocator = Allocator.Persistent;
            }

            public bool Empty { get { return m_Blocks->Length == 0; } }

            // TODO: Priority queue semantics for address-ordered allocation

            public void Push(HeapBlock block)
            {
                m_Blocks->Add(block);
            }

            public HeapBlock Pop()
            {
                int len = m_Blocks->Length;

                if (len == 0)
                    return new HeapBlock();

                HeapBlock block = Block(len - 1);
                m_Blocks->Resize<HeapBlock>(len - 1);
                return block;
            }

            public bool Remove(HeapBlock block)
            {
                for (int i = 0; i < m_Blocks->Length; ++i)
                {
                    if (block.CompareTo(Block(i)) == 0)
                    {
                        m_Blocks->RemoveAtSwapBack<HeapBlock>(i);
                        return true;
                    }
                }

                return false;
            }

            public void Dispose()
            {
                m_Blocks->Dispose();
                UnsafeUtility.Free(m_Blocks, Allocator.Persistent);
            }

            private unsafe HeapBlock Block(int i) { return UnsafeUtility.ReadArrayElement<HeapBlock>(m_Blocks->Ptr, i); }
        }

        private NativeList<SizeBin> m_SizeBins;
        private NativeList<BlocksOfSize> m_Blocks;
        private NativeHashMap<ulong, ulong> m_FreeEndpoints;
        private ulong m_Size;
        private ulong m_Free;
        private readonly int m_MinimumAlignmentLog2;
        private bool m_IsCreated;

        private int FindSmallestSufficientBin(SizeBin needle)
        {
            if (m_SizeBins.Length == 0)
                return 0;

            int lo = 0;                 // Low endpoint of search, inclusive
            int hi = m_SizeBins.Length; // High endpoint of search, exclusive

            for (;;)
            {
                int d2 = (hi - lo) / 2;

                // Search has terminated. If lo is large enough, return it.
                if (d2 == 0)
                {
                    if (needle.CompareTo(m_SizeBins[lo]) <= 0)
                        return lo;
                    else
                        return lo + 1;
                }

                int probe = lo + d2;
                int cmp = needle.CompareTo(m_SizeBins[probe]);

                // Needle is smaller than probe?
                if (cmp < 0)
                {
                    hi = probe;
                }
                // Needle is greater than probe?
                else if (cmp > 0)
                {
                    lo = probe;
                }
                // Found needle exactly.
                else
                {
                    return probe;
                }
            }
        }

        private unsafe int AddNewBin(SizeBin bin, int index)
        {
            int tail = m_SizeBins.Length - index;
            m_SizeBins.ResizeUninitialized(m_SizeBins.Length + 1);
            SizeBin *p = (SizeBin *)m_SizeBins.GetUnsafePtr();
            UnsafeUtility.MemMove(
                p + (index + 1),
                p + index,
                tail * UnsafeUtility.SizeOf<SizeBin>());
            p[index] = bin;
            return index;
        }

        private unsafe void RemoveEmptyBins(SizeBin bin, int index)
        {
            if (!m_Blocks[bin.blocksId].Empty)
                return;

            int tail = m_SizeBins.Length - (index + 1);
            SizeBin* p = (SizeBin*)m_SizeBins.GetUnsafePtr();
            UnsafeUtility.MemMove(
                p + index,
                p + (index + 1),
                tail * UnsafeUtility.SizeOf<SizeBin>());
            m_SizeBins.ResizeUninitialized(m_SizeBins.Length - 1);
        }

        private unsafe HeapBlock PopBlockFromBin(SizeBin bin, int index)
        {
            HeapBlock block = m_Blocks[bin.blocksId].Pop();
            RemoveEndpoints(block);
            m_Free -= block.Length;

            RemoveEmptyBins(bin, index);

            return block;
        }

        private void RemoveEndpoints(HeapBlock block)
        {
            m_FreeEndpoints.Remove(block.begin);
            m_FreeEndpoints.Remove(block.end);
        }

        private void RemoveFreeBlock(HeapBlock block)
        {
            RemoveEndpoints(block);

            SizeBin bin = new SizeBin(block);
            int index = FindSmallestSufficientBin(bin);

#if DEBUG_ASSERTS
            Assert.IsTrue(index >= 0 && m_SizeBins[index].sizeClass == bin.sizeClass,
                "Expected to find exact match for size bin since block was supposed to exist");
#endif

            bool removed = m_Blocks[m_SizeBins[index].blocksId].Remove(block);
            RemoveEmptyBins(m_SizeBins[index], index);

#if DEBUG_ASSERTS
            Assert.IsTrue(removed, "Block was supposed to exist");
#endif

            m_Free -= block.Length;
        }

        private HeapBlock Coalesce(HeapBlock block, ulong endpoint)
        {
            if (m_FreeEndpoints.TryGetValue(endpoint, out ulong otherEnd))
            {
#if DEBUG_ASSERTS
                if (math.min(endpoint, otherEnd) == block.begin &&
                    math.max(endpoint, otherEnd) == block.end)
                {
                    UnityEngine.Debug.Log("kek");
                }
                Assert.IsFalse(
                    math.min(endpoint, otherEnd) == block.begin &&
                    math.max(endpoint, otherEnd) == block.end,
                    "Block was already freed.");
#endif

                if (endpoint == block.begin)
                {
#if DEBUG_ASSERTS
                    Assert.IsTrue(otherEnd < endpoint, "Unexpected endpoints");
#endif
                    var coalesced = new HeapBlock(otherEnd, block.begin);
                    RemoveFreeBlock(coalesced);
                    return new HeapBlock(coalesced.begin, block.end);
                }
                else
                {
#if DEBUG_ASSERTS
                    Assert.IsTrue(otherEnd > endpoint, "Unexpected endpoints");
#endif
                    var coalesced = new HeapBlock(block.end, otherEnd);
                    RemoveFreeBlock(coalesced);
                    return new HeapBlock(block.begin, coalesced.end);
                }
            }
            else
            {
                return block;
            }
        }

        private HeapBlock Coalesce(HeapBlock block)
        {
            block = Coalesce(block, block.begin); // Left
            block = Coalesce(block, block.end);   // Right
            return block;
        }

        private bool CanFitAllocation(SizeBin allocation, SizeBin bin)
        {
#if DEBUG_ASSERTS
            Assert.IsTrue(bin.sizeClass >= allocation.sizeClass, "Should have compatible size classes to begin with");
#endif

            // Check that the bin is not empty.
            if (m_Blocks[bin.blocksId].Empty)
                return false;

            // If the bin meets alignment restrictions, it is usable.
            if (bin.HasCompatibleAlignment(allocation))
            {
                return true;
            }
            // Else, require one alignment worth of extra space so we can guarantee space.
            else
            {
                return bin.Size >= (allocation.Size + allocation.Alignment);
            }
        }

        private static ulong NextAligned(ulong offset, int alignmentLog2)
        {
            int toNext = (1 << alignmentLog2) - 1;
            ulong aligned = ((offset + (ulong)toNext) >> alignmentLog2) << alignmentLog2;
            return aligned;
        }

        private HeapBlock CutAllocationFromBlock(SizeBin allocation, HeapBlock block)
        {
#if DEBUG_ASSERTS
            Assert.IsTrue(block.Length >= allocation.Size, "Block is not large enough.");
#endif

            // If the match is exact, no need to cut.
            if (allocation.Size == block.Length)
                return block;

            // Otherwise, round the begin to next multiple of alignment, and then cut away the required size,
            // potentially leaving empty space on both ends.
            ulong alignedBegin = NextAligned(block.begin, allocation.AlignmentLog2);
            ulong alignedEnd = alignedBegin + allocation.Size;

            if (alignedBegin > block.begin)
                Release(new HeapBlock(block.begin, alignedBegin));

            if (alignedEnd < block.end)
                Release(new HeapBlock(alignedEnd, block.end));

            return new HeapBlock(alignedBegin, alignedEnd);
        }
    }
}

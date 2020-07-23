using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities
{
    internal unsafe struct BlockAllocator : IDisposable
    {
        private UnsafePtrList m_blocks;
        private UnsafeIntList m_allocations;
        private AllocatorManager.AllocatorHandle m_handle;
        private int m_nextByteOffset;

        private const int ms_Log2BlockSize = 16;
        private const int ms_BlockSize = 1 << ms_Log2BlockSize;
        private const int ms_BlockAlignment = 64; //cache line size

        private int ms_BudgetInBytes => m_blocks.Capacity << ms_Log2BlockSize;

        public BlockAllocator(AllocatorManager.AllocatorHandle handle, int budgetInBytes)
        {
            m_handle = handle;
            m_nextByteOffset = 0;
            var blocks = (budgetInBytes + ms_BlockSize - 1) >> ms_Log2BlockSize;
            m_blocks = new UnsafePtrList(blocks, handle);
            m_allocations = new UnsafeIntList(blocks, handle);
        }

        public void Dispose()
        {
            for (var i = m_blocks.Length - 1; i >= 0; --i)
                AllocatorManager.Free(m_handle, (void*)m_blocks[i]);
            m_allocations.Dispose();
            m_blocks.Dispose();
        }

        public void Free(void* pointer)
        {
            if (pointer == null)
                return;
            var blocks = m_allocations.Length; // how many blocks have we allocated?
            for (var i = blocks - 1; i >= 0; --i)
            {
                var block = (byte*)m_blocks[i]; // get a pointer to the block.
                if (pointer >= block && pointer < block + ms_BlockSize) // is the pointer we want to free in this block?
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if (m_allocations.Ptr[i] <= 0) // if that block has no allocations, we can't proceed
                        throw new ArgumentException($"Cannot free this pointer from BlockAllocator: no more allocations to free in its block.");
#endif
                    if (--m_allocations.Ptr[i] == 0) // if this was the last allocation in the block,
                    {
                        if (i == blocks - 1) // if it's the last block,
                            m_nextByteOffset = 0; // just forget that we allocated anything from it, but keep it for later allocations
                        else
                        {
                            AllocatorManager.Free(m_handle, (void*)m_blocks[i]);  // delete the block
                            m_blocks.RemoveAtSwapBack(i); // and forget we ever saw it
                            m_allocations.RemoveAtSwapBack(i); // and your dog toto, too
                        }
                    }
                    return;
                }
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new ArgumentException($"Cannot free this pointer from BlockAllocator: can't be found in any block.");
#endif
        }

        public byte* Allocate(int bytesToAllocate, int alignment)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (bytesToAllocate > ms_BlockSize)
                throw new ArgumentException($"Cannot allocate more than {ms_BlockSize} in BlockAllocator. Requested: {bytesToAllocate}");
#endif
            var nextByteOffsetAligned = (m_nextByteOffset + alignment - 1) & ~(alignment - 1);
            var nextBlockSize = nextByteOffsetAligned + bytesToAllocate;
            if (m_blocks.Length == 0 || nextBlockSize > ms_BlockSize)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (m_blocks.Length == m_blocks.Capacity)
                    throw new ArgumentException($"Cannot exceed budget of {ms_BudgetInBytes} in BlockAllocator.");
#endif
                // Allocate a fresh block of memory
                m_blocks.Add((byte*)AllocatorManager.Allocate(m_handle, sizeof(byte), ms_BlockAlignment, ms_BlockSize));
                m_allocations.Add(0);
                nextByteOffsetAligned = 0;
            }
            var blockIndex = m_blocks.Length - 1;

            var pointer = (byte*)m_blocks[blockIndex] + nextByteOffsetAligned;
            m_nextByteOffset = nextByteOffsetAligned + bytesToAllocate;
            m_allocations.Ptr[blockIndex]++;
            return pointer;
        }

        public T* Allocate<T>(int items = 1) where T : unmanaged
        {
            return (T*)Allocate(items * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
        }

        public byte* Construct(int size, int alignment, void* src)
        {
            var res = Allocate(size, alignment);
            UnsafeUtility.MemCpy(res, src, size);
            return res;
        }

        public T* Construct<T>(T* src) where T : unmanaged
        {
            return (T*)Construct(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), src);
        }
    }
}

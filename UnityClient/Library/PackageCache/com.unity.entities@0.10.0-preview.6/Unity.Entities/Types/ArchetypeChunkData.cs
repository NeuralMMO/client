using System.Diagnostics;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities
{
    // Stores change version numbers, shared component indices, and entity count for all chunks belonging to an archetype in SOA layout
    [DebuggerTypeProxy(typeof(ArchetypeChunkDataDebugView))]
    internal unsafe struct ArchetypeChunkData
    {
        public Chunk** p;
        public int Capacity;
        public int Count;

        readonly int SharedComponentCount;
        readonly int ComponentCount;

        // ChangeVersions and SharedComponentValues stored like:
        //    type0: chunk0 chunk1 chunk2 ...
        //    type1: chunk0 chunk1 chunk2 ...
        //    type2: chunk0 chunk1 chunk2 ...
        //    ...

        ulong ChunkPtrSize => (ulong)(sizeof(Chunk*) * Capacity);
        ulong ChangeVersionSize  => (ulong)(sizeof(uint) * ComponentCount * Capacity);
        ulong EntityCountSize => (ulong)(sizeof(int) * Capacity);
        ulong SharedComponentValuesSize => (ulong)(sizeof(int) * SharedComponentCount * Capacity);
        ulong BufferSize => ChunkPtrSize + ChangeVersionSize + EntityCountSize + SharedComponentValuesSize;

        // ChangeVersions[ComponentCount * Capacity]
        //   - Order version is ChangeVersion[0] which is ChangeVersion[Entity]
        uint* ChangeVersions => (uint*)(((ulong)p) + ChunkPtrSize);

        // EntityCount[Capacity]
        int* EntityCount => (int*)(((ulong)ChangeVersions) + ChangeVersionSize);

        // SharedComponentValues[SharedComponentCount * Capacity]
        int* SharedComponentValues => (int*)(((ulong)EntityCount) + EntityCountSize);

        public ArchetypeChunkData(int componentCount, int sharedComponentCount)
        {
            p = null;
            Capacity = 0;
            Count = 0;
            SharedComponentCount = sharedComponentCount;
            ComponentCount = componentCount;
        }

        public void Grow(int nextCapacity)
        {
            Assert.IsTrue(nextCapacity > Capacity);

            ulong nextChunkPtrSize = (ulong)(sizeof(Chunk*) * nextCapacity);
            ulong nextChangeVersionSize  = (ulong)(sizeof(uint) * ComponentCount * nextCapacity);
            ulong nextEntityCountSize = (ulong)(sizeof(int) * nextCapacity);
            ulong nextSharedComponentValuesSize = (ulong)(sizeof(int) * SharedComponentCount * nextCapacity);
            ulong nextBufferSize = nextChunkPtrSize + nextChangeVersionSize + nextEntityCountSize + nextSharedComponentValuesSize;
            ulong nextBufferPtr = (ulong)UnsafeUtility.Malloc((long)nextBufferSize, 16, Allocator.Persistent);

            Chunk** nextChunkData = (Chunk**)nextBufferPtr;
            nextBufferPtr += nextChunkPtrSize;
            uint* nextChangeVersions = (uint*)nextBufferPtr;
            nextBufferPtr += nextChangeVersionSize;
            int* nextEntityCount = (int*)nextBufferPtr;
            nextBufferPtr += nextEntityCountSize;
            int* nextSharedComponentValues = (int*)nextBufferPtr;
            nextBufferPtr += nextSharedComponentValuesSize;

            int prevCount = Count;
            int prevCapacity = Capacity;
            Chunk** prevChunkData = p;
            uint* prevChangeVersions = ChangeVersions;
            int* prevEntityCount = EntityCount;
            int* prevSharedComponentValues = SharedComponentValues;

            UnsafeUtility.MemCpy(nextChunkData, prevChunkData, (sizeof(Chunk*) * prevCount));

            for (int i = 0; i < ComponentCount; i++)
                UnsafeUtility.MemCpy(nextChangeVersions + (i * nextCapacity), prevChangeVersions + (i * prevCapacity), sizeof(uint) * Count);

            for (int i = 0; i < SharedComponentCount; i++)
                UnsafeUtility.MemCpy(nextSharedComponentValues + (i * nextCapacity), prevSharedComponentValues + (i * prevCapacity), sizeof(uint) * Count);

            UnsafeUtility.MemCpy(nextEntityCount, prevEntityCount, sizeof(int) * Count);

            UnsafeUtility.Free(p, Allocator.Persistent);

            p = nextChunkData;
            Capacity = nextCapacity;
        }

        public bool InsideAllocation(ulong addr)
        {
            ulong startAddr = (ulong)p;
            return (addr >= startAddr) && (addr <= (startAddr + BufferSize));
        }

        public int* GetSharedComponentValueArrayForType(int sharedComponentIndexInArchetype)
        {
            return SharedComponentValues + (sharedComponentIndexInArchetype * Capacity);
        }

        public int GetSharedComponentValue(int sharedComponentIndexInArchetype, int chunkIndex)
        {
            var sharedValues = GetSharedComponentValueArrayForType(sharedComponentIndexInArchetype);
            return sharedValues[chunkIndex];
        }

        public void SetSharedComponentValue(int sharedComponentIndexInArchetype, int chunkIndex, int value)
        {
            var sharedValues = GetSharedComponentValueArrayForType(sharedComponentIndexInArchetype);
            sharedValues[chunkIndex] = value;
        }

        public SharedComponentValues GetSharedComponentValues(int chunkIndex)
        {
            return new SharedComponentValues
            {
                firstIndex = SharedComponentValues + chunkIndex,
                stride = Capacity * sizeof(int)
            };
        }

        public uint* GetChangeVersionArrayForType(int indexInArchetype)
        {
            return ChangeVersions + (indexInArchetype * Capacity);
        }

        public uint GetChangeVersion(int indexInArchetype, int chunkIndex)
        {
            var changeVersions = GetChangeVersionArrayForType(indexInArchetype);
            return changeVersions[chunkIndex];
        }

        public uint GetOrderVersion(int chunkIndex)
        {
            return GetChangeVersion(0, chunkIndex);
        }

        public void SetChangeVersion(int indexInArchetype, int chunkIndex, uint version)
        {
            var changeVersions = GetChangeVersionArrayForType(indexInArchetype);
            changeVersions[chunkIndex] = version;
        }

        public void SetAllChangeVersion(int chunkIndex, uint version)
        {
            for (int i = 1; i < ComponentCount; ++i)
                ChangeVersions[(i * Capacity) + chunkIndex] = version;
        }

        public void SetOrderVersion(int chunkIndex, uint changeVersion)
        {
            SetChangeVersion(0, chunkIndex, changeVersion);
        }

        public int* GetChunkEntityCountArray()
        {
            return EntityCount;
        }

        public int GetChunkEntityCount(int chunkIndex)
        {
            return EntityCount[chunkIndex];
        }

        public void SetChunkEntityCount(int chunkIndex, int count)
        {
            EntityCount[chunkIndex] = count;
        }

        public void Add(Chunk* chunk, SharedComponentValues sharedComponentIndices, uint changeVersion)
        {
            var chunkIndex = Count++;

            p[chunkIndex] = chunk;

            for (int i = 0; i < SharedComponentCount; i++)
                SharedComponentValues[(i * Capacity) + chunkIndex] = sharedComponentIndices[i];

            // New chunk, so all versions are reset.
            for (int i = 0; i < ComponentCount; i++)
                ChangeVersions[(i * Capacity) + chunkIndex] = changeVersion;

            EntityCount[chunkIndex] = chunk->Count;
        }

        public void RemoveAtSwapBack(int chunkIndex)
        {
            Count--;

            if (chunkIndex == Count)
                return;

            p[chunkIndex] = p[Count];

            for (int i = 0; i < SharedComponentCount; i++)
                SharedComponentValues[(i * Capacity) + chunkIndex] = SharedComponentValues[(i * Capacity) + Count];

            // On *chunk order* change, no versions changed, just moved to new location.
            for (int i = 0; i < ComponentCount; i++)
                ChangeVersions[(i * Capacity) + chunkIndex] = ChangeVersions[(i * Capacity) + Count];

            EntityCount[chunkIndex] = EntityCount[Count];
        }

        public void Dispose()
        {
            UnsafeUtility.Free(p, Allocator.Persistent);
            p = null;
            Capacity = 0;
            Count = 0;
        }
    }
}

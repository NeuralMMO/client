using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities
{
    public unsafe partial struct EntityManager
    {
        [Obsolete("LockChunk has been deprecated, and its usage is a no-op. (RemovedAfter 2020-06-05)")]
        public void LockChunk(ArchetypeChunk chunk)
        {
        }

        [Obsolete("LockChunk has been deprecated, and its usage is a no-op. (RemovedAfter 2020-06-05)")]
        public void LockChunk(NativeArray<ArchetypeChunk> chunks)
        {
        }

        [Obsolete("UnlockChunk has been deprecated, and its usage is a no-op. (RemovedAfter 2020-06-05)")]
        public void UnlockChunk(ArchetypeChunk chunk)
        {
        }
    }
}

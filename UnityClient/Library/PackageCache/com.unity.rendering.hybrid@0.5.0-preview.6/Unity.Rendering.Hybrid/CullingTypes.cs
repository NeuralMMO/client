// This file contains types used by Hybrid Renderer culling, and that are mostly shared
// between Hybrid V1 and Hybrid V2

using Unity.Mathematics;

namespace Unity.Rendering
{
    public unsafe struct CullingStats
    {
        public const int kChunkTotal = 0;
        public const int kChunkCountAnyLod = 1;
        public const int kChunkCountInstancesProcessed = 2;
        public const int kChunkCountFullyIn = 3;
        public const int kInstanceTests = 4;
        public const int kLodTotal = 5;
        public const int kLodNoRequirements = 6;
        public const int kLodChanged = 7;
        public const int kLodChunksTested = 8;
        public const int kCountRootLodsSelected = 9;
        public const int kCountRootLodsFailed = 10;
        public const int kCount = 11;
        public fixed int Stats[kCount];
        public float CameraMoveDistance;
        public fixed int CacheLinePadding[15 - kCount];
    }

    internal struct Fixed16CamDistance
    {
        public const float kRes = 100.0f;

        public static ushort FromFloatCeil(float f)
        {
            return (ushort)math.clamp((int)math.ceil(f * kRes), 0, 0xffff);
        }

        public static ushort FromFloatFloor(float f)
        {
            return (ushort)math.clamp((int)math.floor(f * kRes), 0, 0xffff);
        }
    }

    public unsafe struct ChunkInstanceLodEnabled
    {
        public fixed ulong Enabled[2];
    }
}

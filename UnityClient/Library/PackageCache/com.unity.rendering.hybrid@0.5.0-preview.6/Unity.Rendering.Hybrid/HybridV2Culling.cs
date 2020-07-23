using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering;

/*
 * Batch-oriented culling.
 *
 * This culling approach oriented from Megacity and works well for relatively
 * slow-moving cameras in a large, dense environment.
 *
 * The primary CPU costs involved in culling all the chunks of mesh instances
 * in megacity is touching the chunks of memory. A naive culling approach would
 * look like this:
 *
 *     for each chunk:
 *       select what instances should be enabled based on camera position (lod selection)
 *
 *     for each frustum:
 *       for each chunk:
 *         if the chunk is completely out of the frustum:
 *           discard
 *         else:
 *           for each instance in the chunk:
 *             if the instance is inside the frustum:
 *               write index of instance to output index buffer
 *
 * The approach implemented here does essentially this, but has been optimized
 * so that chunks need to be accessed as infrequently as possible:
 *
 * - Because the chunks are static, we can cache bounds information outside the chunks
 *
 * - Because the camera moves relatively slowly, we can compute a grace
 *   distance which the camera has to move (in any direction) before the LOD
 *   selection would compute a different result
 *
 * - Because only a some chunks straddle the frustum boundaries, we can treat
 *   them as "in" rather than "partial" to save touching their chunk memory
 *
 *
 * The code below is complicated by the fact that we maintain two indexing schemes.
 *
 * The external indices are the C++ batch renderer's idea of a batch. A batch
 * can contain up to 1023 model instances. This index set changes when batches
 * are removed, and these external indices are swapped from the end to maintain
 * a packed index set. The culling code here needs to maintain these external
 * batch indices only to communicate to the downstream renderer.
 *
 * Furthermore, we keep an internal index range. This is so that we have stable
 * indices that don't change as batches are removed. Because they are stable we
 * can use them as hash table indices and store information related to them freely.
 *
 * The core data organization is around this internal index space.
 *
 * We map from 1 internal index to N chunks. Each chunk directly corresponds to
 * an ECS chunk of instances to be culled and rendered.
 *
 * The chunk data tracks the bounds and some other bits of information that would
 * be expensive to reacquire from the chunk data itself.
 */

namespace Unity.Rendering
{
    [BurstCompile]
    unsafe struct SelectLodEnabled : IJobChunk
    {
        [ReadOnly] public LODGroupExtensions.LODParams LODParams;
        [ReadOnly] public NativeArray<byte> ForceLowLOD;
        [ReadOnly] public ArchetypeChunkComponentType<RootLodRequirement> RootLodRequirements;
        [ReadOnly] public ArchetypeChunkComponentType<LodRequirement> InstanceLodRequirements;
        public ushort CameraMoveDistanceFixed16;
        public float DistanceScale;
        public bool DistanceScaleChanged;

        public ArchetypeChunkComponentType<HybridChunkInfo> HybridChunkInfo;
        [ReadOnly] public ArchetypeChunkComponentType<ChunkHeader> ChunkHeader;

#if UNITY_EDITOR
        [NativeDisableUnsafePtrRestriction] public CullingStats* Stats;

#pragma warning disable 649
        [NativeSetThreadIndex] public int ThreadIndex;
#pragma warning restore 649

#endif

        public void Execute(ArchetypeChunk archetypeChunk, int chunkIndex, int firstEntityIndex)
        {
            var hybridChunkInfoArray = archetypeChunk.GetNativeArray(HybridChunkInfo);
            var chunkHeaderArray = archetypeChunk.GetNativeArray(ChunkHeader);

            for (var entityIndex = 0; entityIndex < archetypeChunk.Count; entityIndex++)
            {
                var hybridChunkInfo = hybridChunkInfoArray[entityIndex];
                if (!hybridChunkInfo.Valid)
                    continue;

                var chunkHeader = chunkHeaderArray[entityIndex];

#if UNITY_EDITOR
                Stats[ThreadIndex].Stats[CullingStats.kLodTotal]++;
#endif
                var internalBatchIndex = hybridChunkInfo.InternalIndex;
                var chunkInstanceCount = chunkHeader.ArchetypeChunk.Count;
                var isOrtho = LODParams.isOrtho;

                ref var chunkCullingData = ref hybridChunkInfo.CullingData;
                ChunkInstanceLodEnabled chunkEntityLodEnabled = chunkCullingData.InstanceLodEnableds;

#if UNITY_EDITOR
                ChunkInstanceLodEnabled oldEntityLodEnabled = chunkEntityLodEnabled;
#endif
                var forceLowLOD = ForceLowLOD[internalBatchIndex];

                if (0 == (chunkCullingData.Flags & HybridChunkCullingData.kFlagHasLodData))
                {
#if UNITY_EDITOR
                    Stats[ThreadIndex].Stats[CullingStats.kLodNoRequirements]++;
#endif
                    chunkEntityLodEnabled.Enabled[0] = 0;
                    chunkEntityLodEnabled.Enabled[1] = 0;
                    chunkCullingData.ForceLowLODPrevious = forceLowLOD;

                    for (int i = 0; i < chunkInstanceCount; ++i)
                    {
                        int wordIndex = i >> 6;
                        int bitIndex = i & 63;
                        chunkEntityLodEnabled.Enabled[wordIndex] |= 1ul << bitIndex;
                    }
                }
                else
                {
                    int diff = (int)chunkCullingData.MovementGraceFixed16 - CameraMoveDistanceFixed16;
                    chunkCullingData.MovementGraceFixed16 = (ushort)math.max(0, diff);

                    var graceExpired = chunkCullingData.MovementGraceFixed16 == 0;
                    var forceLodChanged = forceLowLOD != chunkCullingData.ForceLowLODPrevious;

                    if (graceExpired || forceLodChanged || DistanceScaleChanged)
                    {
                        chunkEntityLodEnabled.Enabled[0] = 0;
                        chunkEntityLodEnabled.Enabled[1] = 0;

#if UNITY_EDITOR
                        Stats[ThreadIndex].Stats[CullingStats.kLodChunksTested]++;
#endif
                        var chunk = chunkHeader.ArchetypeChunk;

                        var rootLodRequirements = chunk.GetNativeArray(RootLodRequirements);
                        var instanceLodRequirements = chunk.GetNativeArray(InstanceLodRequirements);

                        float graceDistance = float.MaxValue;

                        for (int i = 0; i < chunkInstanceCount; i++)
                        {
                            var rootLodRequirement = rootLodRequirements[i];

                            var rootLodDistance =
                                math.select(
                                    DistanceScale *
                                    math.length(LODParams.cameraPos - rootLodRequirement.LOD.WorldReferencePosition),
                                    DistanceScale, isOrtho);

                            float rootMinDist = math.select(rootLodRequirement.LOD.MinDist, 0.0f, forceLowLOD == 1);
                            float rootMaxDist = rootLodRequirement.LOD.MaxDist;

                            graceDistance = math.min(math.abs(rootLodDistance - rootMinDist), graceDistance);
                            graceDistance = math.min(math.abs(rootLodDistance - rootMaxDist), graceDistance);

                            var rootLodIntersect = (rootLodDistance < rootMaxDist) && (rootLodDistance >= rootMinDist);

                            if (rootLodIntersect)
                            {
                                var instanceLodRequirement = instanceLodRequirements[i];
                                var instanceDistance =
                                    math.select(
                                        DistanceScale *
                                        math.length(LODParams.cameraPos -
                                            instanceLodRequirement.WorldReferencePosition), DistanceScale,
                                        isOrtho);

                                var instanceLodIntersect =
                                    (instanceDistance < instanceLodRequirement.MaxDist) &&
                                    (instanceDistance >= instanceLodRequirement.MinDist);

                                graceDistance = math.min(math.abs(instanceDistance - instanceLodRequirement.MinDist),
                                    graceDistance);
                                graceDistance = math.min(math.abs(instanceDistance - instanceLodRequirement.MaxDist),
                                    graceDistance);

                                if (instanceLodIntersect)
                                {
                                    var index = i;
                                    var wordIndex = index >> 6;
                                    var bitIndex = index & 0x3f;
                                    var lodWord = chunkEntityLodEnabled.Enabled[wordIndex];

                                    lodWord |= 1UL << bitIndex;
                                    chunkEntityLodEnabled.Enabled[wordIndex] = lodWord;
                                }
                            }
                        }

                        chunkCullingData.MovementGraceFixed16 = Fixed16CamDistance.FromFloatFloor(graceDistance);
                        chunkCullingData.ForceLowLODPrevious = forceLowLOD;
                    }
                }


#if UNITY_EDITOR
                if (oldEntityLodEnabled.Enabled[0] != chunkEntityLodEnabled.Enabled[0] ||
                    oldEntityLodEnabled.Enabled[1] != chunkEntityLodEnabled.Enabled[1])
                {
                    Stats[ThreadIndex].Stats[CullingStats.kLodChanged]++;
                }
#endif

                chunkCullingData.InstanceLodEnableds = chunkEntityLodEnabled;
                hybridChunkInfoArray[entityIndex] = hybridChunkInfo;
            }
        }
    }

    [BurstCompile]
    unsafe struct ZeroVisibleCounts : IJobParallelFor
    {
        public NativeArray<BatchVisibility> Batches;

        public void Execute(int index)
        {
            // Can't set individual fields of structs inside NativeArray, so do it via raw pointer
            ((BatchVisibility*)Batches.GetUnsafePtr())[index].visibleCount = 0;
        }
    }

    [BurstCompile]
    unsafe struct SimpleCullingJob : IJobChunk
    {
        [DeallocateOnJobCompletion][ReadOnly] public NativeArray<FrustumPlanes.PlanePacket4> Planes;
        [ReadOnly] public NativeArray<int> InternalToExternalRemappingTable;

        [ReadOnly] public ArchetypeChunkComponentType<WorldRenderBounds> BoundsComponent;
        public ArchetypeChunkComponentType<HybridChunkInfo> HybridChunkInfo;
        [ReadOnly] public ArchetypeChunkComponentType<ChunkHeader> ChunkHeader;
        [ReadOnly] public ArchetypeChunkComponentType<ChunkWorldRenderBounds> ChunkWorldRenderBounds;

        [NativeDisableParallelForRestriction] public NativeArray<int> IndexList;
        [NativeDisableParallelForRestriction] public NativeArray<BatchVisibility> Batches;

        public const uint kMaxEntitiesPerChunk = 128;
        [NativeDisableParallelForRestriction] public NativeArray<int> ThreadLocalIndexLists;

#pragma warning disable 649
        [NativeSetThreadIndex] public int ThreadIndex;
#pragma warning restore 649

#if UNITY_EDITOR
        [NativeDisableUnsafePtrRestriction] public CullingStats* Stats;
#endif

        public void Execute(ArchetypeChunk archetypeChunk, int chunkIndex, int firstEntityIndex)
        {
            var hybridChunkInfoArray = archetypeChunk.GetNativeArray(HybridChunkInfo);
            var chunkHeaderArray = archetypeChunk.GetNativeArray(ChunkHeader);
            var chunkBoundsArray = archetypeChunk.GetNativeArray(ChunkWorldRenderBounds);

            for (var entityIndex = 0; entityIndex < archetypeChunk.Count; entityIndex++)
            {
                var hybridChunkInfo = hybridChunkInfoArray[entityIndex];
                if (!hybridChunkInfo.Valid)
                    continue;

                var chunkHeader = chunkHeaderArray[entityIndex];
                var chunkBounds = chunkBoundsArray[entityIndex];

#if UNITY_EDITOR
                ref var stats = ref Stats[ThreadIndex];
                stats.Stats[CullingStats.kChunkTotal]++;
#endif

                int internalBatchIndex = hybridChunkInfo.InternalIndex;
                int externalBatchIndex = InternalToExternalRemappingTable[internalBatchIndex];

                ref var chunkCullingData = ref hybridChunkInfo.CullingData;

                // Debug.Log($"DEBUGCULLING Execute: internalBatchIndex: {internalBatchIndex}, externalBatchIndex: {externalBatchIndex}, hash: {chunkHeader.ArchetypeChunk.GetHashCode():x8}, count: {chunkHeader.ArchetypeChunk.Count}, capacity: {chunkHeader.ArchetypeChunk.Capacity}");

                var pBatch = ((BatchVisibility*)Batches.GetUnsafePtr()) + externalBatchIndex;

                // var inState = BatchCullingStates[internalBatchIndex];

                int batchOutputOffset = pBatch->offset;
                int processedInstanceCount = chunkCullingData.BatchOffset;

                var chunkInstanceCount = chunkHeader.ArchetypeChunk.Count;
                var chunkEntityLodEnabled = chunkCullingData.InstanceLodEnableds;
                var anyLodEnabled = (chunkEntityLodEnabled.Enabled[0] | chunkEntityLodEnabled.Enabled[1]) != 0;

                if (anyLodEnabled)
                {
#if UNITY_EDITOR
                    stats.Stats[CullingStats.kChunkCountAnyLod]++;
#endif

                    var perInstanceCull = 0 != (chunkCullingData.Flags & HybridChunkCullingData.kFlagInstanceCulling);

                    var chunkIn = perInstanceCull
                        ? FrustumPlanes.Intersect2(Planes, chunkBounds.Value)
                        : FrustumPlanes.Intersect2NoPartial(Planes, chunkBounds.Value);

                    if (chunkIn == FrustumPlanes.IntersectResult.Partial)
                    {
#if UNITY_EDITOR
                        int instanceTestCount = 0;
#endif

                        // Find out where the scratch area is for this thread
                        // Output there first, then atomic allocate space for the correct amount of instances,
                        // and finally memcpy from the scratch to the real output.
                        int scratchIndex = (int)(kMaxEntitiesPerChunk * (uint)ThreadIndex);
                        int* scratch = ((int*)ThreadLocalIndexLists.GetUnsafePtr()) + scratchIndex;

                        var chunk = chunkHeader.ArchetypeChunk;
                        var chunkInstanceBounds = chunk.GetNativeArray(BoundsComponent);

                        int instanceOutputOffset = 0;

                        for (int j = 0; j < 2; j++)
                        {
                            var lodWord = chunkEntityLodEnabled.Enabled[j];

                            while (lodWord != 0)
                            {
                                var bitIndex = math.tzcnt(lodWord);
                                var finalIndex = (j << 6) + bitIndex;

                                scratch[instanceOutputOffset] = processedInstanceCount + finalIndex;
                                // IndexList[batchOutputOffset + batchOutputCount] = processedInstanceCount + finalIndex;
                                // Debug.Log($"DEBUGCULLING Partial {externalBatchIndex}: [{batchOutputOffset + batchOutputCount}] = {processedInstanceCount + finalIndex}");

                                int advance = FrustumPlanes.Intersect2(Planes, chunkInstanceBounds[finalIndex].Value) !=
                                    FrustumPlanes.IntersectResult.Out
                                    ? 1
                                    : 0;
                                instanceOutputOffset += advance;

                                lodWord ^= 1ul << bitIndex;

#if UNITY_EDITOR
                                instanceTestCount++;
#endif
                            }
                        }

                        int chunkOutputCount = instanceOutputOffset;

                        if (chunkOutputCount > 0)
                        {
                            int chunkOutputOffset = System.Threading.Interlocked.Add(
                                ref pBatch->visibleCount, chunkOutputCount) - chunkOutputCount;

                            chunkOutputOffset += batchOutputOffset;

                            var pVisibleIndices = ((int*)IndexList.GetUnsafePtr()) + chunkOutputOffset;

                            UnsafeUtility.MemCpy(
                                pVisibleIndices,
                                scratch,
                                chunkOutputCount * sizeof(int));
                        }

#if UNITY_EDITOR
                        stats.Stats[CullingStats.kChunkCountInstancesProcessed]++;
                        stats.Stats[CullingStats.kInstanceTests] += instanceTestCount;
#endif
                    }
                    else if (chunkIn == FrustumPlanes.IntersectResult.In)
                    {
#if UNITY_EDITOR
                        stats.Stats[CullingStats.kChunkCountFullyIn]++;
#endif

                        // Since the chunk is fully in, we can easily count how many instances we will output
                        int chunkOutputCount = math.countbits(chunkEntityLodEnabled.Enabled[0]) +
                            math.countbits(chunkEntityLodEnabled.Enabled[1]);

                        int chunkOutputOffset = System.Threading.Interlocked.Add(
                            ref pBatch->visibleCount, chunkOutputCount) - chunkOutputCount;

                        chunkOutputOffset += batchOutputOffset;

                        int instanceOutputOffset = 0;

                        for (int j = 0; j < 2; j++)
                        {
                            var lodWord = chunkEntityLodEnabled.Enabled[j];

                            while (lodWord != 0)
                            {
                                var bitIndex = math.tzcnt(lodWord);
                                var finalIndex = (j << 6) + bitIndex;
                                IndexList[chunkOutputOffset + instanceOutputOffset] =
                                    processedInstanceCount + finalIndex;
                                // Debug.Log($"DEBUGCULLING Full {externalBatchIndex} : [{batchOutputOffset + batchOutputCount}] = {processedInstanceCount + finalIndex}");
                                instanceOutputOffset += 1;
                                lodWord ^= 1ul << bitIndex;
                            }
                        }
                    }
                }

                hybridChunkInfoArray[entityIndex] = hybridChunkInfo;
            }
        }
    }
}

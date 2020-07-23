using System;
using Unity.Burst;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;

namespace Unity.Entities
{
    [JobProducerType(typeof(JobEntityBatchExtensions.JobEntityBatchProducer<>))]
    public interface IJobEntityBatch
    {
        void Execute(ArchetypeChunk batchInChunk, int batchIndex);
    }

    public static class JobEntityBatchExtensions
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        [NativeContainer]
        internal struct EntitySafetyHandle
        {
            internal AtomicSafetyHandle m_Safety;
        }
#endif
        internal struct JobEntityBatchWrapper<T> where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
#pragma warning disable 414
            [ReadOnly] public EntitySafetyHandle safety;
#pragma warning restore
#endif
            public T JobData;

            [DeallocateOnJobCompletion]
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<ArchetypeChunk> Batches;

            public int JobsPerChunk;
            public int IsParallel;
        }

        public static unsafe JobHandle ScheduleSingle<T>(
            this T jobData,
            EntityQuery query,
            JobHandle dependsOn = default(JobHandle))
            where T : struct, IJobEntityBatch
        {
            return ScheduleInternal(ref jobData, query, dependsOn, ScheduleMode.Batched, 1, false);
        }

        public static unsafe JobHandle ScheduleParallel<T>(
            this T jobData,
            EntityQuery query,
            JobHandle dependsOn = default(JobHandle))
            where T : struct, IJobEntityBatch
        {
            return ScheduleInternal(ref jobData, query, dependsOn, ScheduleMode.Batched, 1, true);
        }

        public static unsafe JobHandle ScheduleParallelBatched<T>(
            this T jobData,
            EntityQuery query,
            int batchesPerChunk,
            JobHandle dependsOn = default(JobHandle))
            where T : struct, IJobEntityBatch
        {
            return ScheduleInternal(ref jobData, query, dependsOn, ScheduleMode.Batched, batchesPerChunk, true);
        }

        public static unsafe void Run<T>(this T jobData, EntityQuery query)
            where T : struct, IJobEntityBatch
        {
            ScheduleInternal(ref jobData, query, default(JobHandle), ScheduleMode.Run, 1, false);
        }

        internal static unsafe JobHandle ScheduleInternal<T>(
            ref T jobData,
            EntityQuery query,
            JobHandle dependsOn,
            ScheduleMode mode,
            int batchesPerChunk,
            bool isParallel = true)
            where T : struct, IJobEntityBatch
        {
            var queryImpl = query._GetImpl();
            var filteredChunkCount = queryImpl->CalculateChunkCount();
            var batches = new NativeArray<ArchetypeChunk>(filteredChunkCount * batchesPerChunk, Allocator.TempJob);

            var prefilterHandle = new PrefilterForJobEntityBatch
            {
                MatchingArchetypes = queryImpl->_QueryData->MatchingArchetypes,
                Filter = queryImpl->_Filter,
                BatchesPerChunk = batchesPerChunk,
                EntityComponentStore = queryImpl->_Access->EntityComponentStore,
                Batches = batches
            }.Schedule(dependsOn);

            JobEntityBatchWrapper<T> jobEntityBatchWrapper = new JobEntityBatchWrapper<T>
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                // All IJobEntityBatch jobs have a EntityManager safety handle to ensure that BeforeStructuralChange throws an error if
                // jobs without any other safety handles are still running (haven't been synced).
                safety = new EntitySafetyHandle {m_Safety = queryImpl->SafetyHandles->GetEntityManagerSafetyHandle()},
#endif

                JobData = jobData,
                Batches = batches,

                JobsPerChunk = batchesPerChunk,
                IsParallel = isParallel ? 1 : 0
            };

            var scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref jobEntityBatchWrapper),
                isParallel
                ? JobEntityBatchProducer<T>.InitializeParallel()
                : JobEntityBatchProducer<T>.InitializeSingle(),
                prefilterHandle,
                mode);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            try
            {
#endif
            if (!isParallel)
            {
                return JobsUtility.Schedule(ref scheduleParams);
            }
            else
            {
                return JobsUtility.ScheduleParallelFor(ref scheduleParams, filteredChunkCount * batchesPerChunk, 1);
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        }

        catch (InvalidOperationException e)
        {
            prefilterHandle.Complete();
            batches.Dispose();
            throw e;
        }
#endif
        }

        internal struct JobEntityBatchProducer<T>
            where T : struct, IJobEntityBatch
        {
            static IntPtr s_JobReflectionDataParallel;
            static IntPtr s_JobReflectionDataSingle;

            public static IntPtr InitializeSingle()
            {
                if (s_JobReflectionDataSingle == IntPtr.Zero)
                    s_JobReflectionDataSingle = JobsUtility.CreateJobReflectionData(
                        typeof(JobEntityBatchWrapper<T>),
                        typeof(T),
                        JobType.Single,
                        (ExecuteJobFunction)Execute);

                return s_JobReflectionDataSingle;
            }

            public static IntPtr InitializeParallel()
            {
                if (s_JobReflectionDataParallel == IntPtr.Zero)
                    s_JobReflectionDataParallel = JobsUtility.CreateJobReflectionData(
                        typeof(JobEntityBatchWrapper<T>),
                        typeof(T),
                        JobType.ParallelFor,
                        (ExecuteJobFunction)Execute);

                return s_JobReflectionDataParallel;
            }

            public delegate void ExecuteJobFunction(
                ref JobEntityBatchWrapper<T> jobWrapper,
                IntPtr additionalPtr,
                IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex);

            public static void Execute(
                ref JobEntityBatchWrapper<T> jobWrapper,
                IntPtr additionalPtr,
                IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex)
            {
                ExecuteInternal(ref jobWrapper, ref ranges, jobIndex);
            }

            internal unsafe static void ExecuteInternal(
                ref JobEntityBatchWrapper<T> jobWrapper,
                ref JobRanges ranges,
                int jobIndex)
            {
                var batches = jobWrapper.Batches;

                bool isParallel = jobWrapper.IsParallel == 1;
                while (true)
                {
                    int beginBatchIndex = 0;
                    int endBatchIndex = batches.Length;

                    // If we are running the job in parallel, steal some work.
                    if (isParallel)
                    {
                        // If we have no range to steal, exit the loop.
                        if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out beginBatchIndex, out endBatchIndex))
                            break;
                    }

                    // Do the actual user work.
                    for (int batchIndex = beginBatchIndex; batchIndex < endBatchIndex; ++batchIndex)
                    {
                        jobWrapper.JobData.Execute(batches[batchIndex], batchIndex);
                    }

                    // If we are not running in parallel, our job is done.
                    if (!isParallel)
                        break;
                }
            }
        }
    }

    [BurstCompile]
    unsafe struct PrefilterForJobEntityBatch : IJob
    {
        [NativeDisableUnsafePtrRestriction] public UnsafeMatchingArchetypePtrList MatchingArchetypes;
        public EntityQueryFilter Filter;
        public int BatchesPerChunk;
        [NativeDisableUnsafePtrRestriction] public EntityComponentStore* EntityComponentStore;

        [NativeDisableParallelForRestriction] public NativeArray<ArchetypeChunk> Batches;

        public void Execute()
        {
            var batchCounter = 0;

            for (var m = 0; m < MatchingArchetypes.Length; ++m)
            {
                var match = MatchingArchetypes.Ptr[m];
                if (match->Archetype->EntityCount <= 0)
                    continue;

                var archetype = match->Archetype;
                int chunkCount = archetype->Chunks.Count;

                for (int chunkIndex = 0; chunkIndex < chunkCount; ++chunkIndex)
                {
                    var chunk = archetype->Chunks.p[chunkIndex];
                    for (int batchIndex = 0; batchIndex < BatchesPerChunk; ++batchIndex)
                    {
                        if (match->ChunkMatchesFilter(chunkIndex, ref Filter))
                            Batches[batchCounter++] = ArchetypeChunk.EntityBatchFromChunk(chunk, BatchesPerChunk, batchIndex, EntityComponentStore);
                    }
                }
            }
        }
    }
}

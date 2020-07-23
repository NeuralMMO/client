using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Entities.Editor
{
    class EntityDiffer : IDisposable
    {
        readonly World m_World;

        NativeList<int> m_VersionByEntity;
        UnsafeBitArray m_QueryMatchCacheByEntity;
        EntityQuery m_LastQuery;

        public EntityDiffer(World world)
        {
            m_World = world;
            m_VersionByEntity = new NativeList<int>(m_World.EntityManager.EntityCapacity, Allocator.Persistent);
            m_QueryMatchCacheByEntity = new UnsafeBitArray(m_World.EntityManager.EntityCapacity, Allocator.Persistent);
        }

        public unsafe JobHandle GetEntityQueryMatchDiffAsync(EntityQuery query, NativeList<Entity> newEntities, NativeList<Entity> missingEntities)
        {
            var createdEntities = new NativeList<Entity>(Allocator.TempJob);
            var destroyedEntities = new NativeList<Entity>(Allocator.TempJob);
            var getAllCreatedAndDestroyedEntities = m_World.EntityManager.GetCreatedAndDestroyedEntitiesAsync(m_VersionByEntity, createdEntities, destroyedEntities);

            if (m_QueryMatchCacheByEntity.Length < m_World.EntityManager.EntityCapacity)
            {
                var newBitArray = new UnsafeBitArray(m_World.EntityManager.EntityCapacity, Allocator.Persistent);
                // Length is divided by 8 in the memcpy because UnsafeBitArray.Length reprensent the length in BITS and not BYTES
                UnsafeUtility.MemCpy(newBitArray.Ptr, m_QueryMatchCacheByEntity.Ptr, m_QueryMatchCacheByEntity.Length / 8);
                m_QueryMatchCacheByEntity.Dispose();
                m_QueryMatchCacheByEntity = newBitArray;
            }

            newEntities.Clear();
            missingEntities.Clear();
            var queryMask = m_World.EntityManager.GetEntityQueryMask(query);

            var missingEntitiesQueue = new NativeQueue<Entity>(Allocator.TempJob);
            var missingEntitiesParallelWriter = missingEntitiesQueue.AsParallelWriter();
            var detectMissingEntitiesJob = new DetectMissingEntitiesJob
            {
                DestroyedEntities = destroyedEntities.AsDeferredJobArray(),
                MissingEntities = missingEntitiesParallelWriter,
                MatchedLastQueryByEntity = m_QueryMatchCacheByEntity
            }.Schedule(getAllCreatedAndDestroyedEntities);

            var newEntitiesQueue = new NativeQueue<Entity>(Allocator.TempJob);
            JobHandle handle;
            if (query == m_LastQuery)
            {
                handle = new DetectNewEntitiesJob
                {
                    QueryMask = queryMask,
                    CreatedEntities = createdEntities.AsDeferredJobArray(),
                    MatchedLastQueryByEntity = m_QueryMatchCacheByEntity,
                    NewEntities = newEntitiesQueue.AsParallelWriter()
                }.Schedule(detectMissingEntitiesJob);
            }
            else
            {
                m_LastQuery = query;

                handle = new DetectNewAndMissingEntitiesFromStateBatchedJob
                {
                    VersionByEntity = m_VersionByEntity.AsDeferredJobArray(),
                    QueryMask = queryMask,
                    MatchedLastQueryByEntity = m_QueryMatchCacheByEntity,
                    CreatedEntities = createdEntities,
                    NewEntities = newEntitiesQueue.AsParallelWriter(),
                    MissingEntities = missingEntitiesParallelWriter,
                }.ScheduleBatch(m_World.EntityManager.EntityCapacity, m_World.EntityManager.EntityCapacity / JobsUtility.MaxJobThreadCount, detectMissingEntitiesJob);
            }

            var copyNewEntitiesJob = new CopyNativeQueueToNativeList<Entity>
            {
                Queue = newEntitiesQueue,
                List = newEntities
            }.Schedule(handle);

            var copyMissingEntitiesJob = new CopyNativeQueueToNativeList<Entity>
            {
                Queue = missingEntitiesQueue,
                List = missingEntities
            }.Schedule(handle);

            var handles = new NativeArray<JobHandle>(6, Allocator.Temp)
            {
                [0] = destroyedEntities.Dispose(detectMissingEntitiesJob),
                [1] = createdEntities.Dispose(handle),
                [2] = newEntitiesQueue.Dispose(copyNewEntitiesJob),
                [3] = missingEntitiesQueue.Dispose(copyMissingEntitiesJob),
                [4] = copyNewEntitiesJob,
                [5] = copyMissingEntitiesJob
            };
            var jobHandle = JobHandle.CombineDependencies(handles);
            handles.Dispose();

            return jobHandle;
        }

        [BurstCompile]
        struct DetectMissingEntitiesJob : IJob
        {
            public UnsafeBitArray MatchedLastQueryByEntity;
            [ReadOnly] public NativeArray<Entity> DestroyedEntities;
            [WriteOnly] public NativeQueue<Entity>.ParallelWriter MissingEntities;

            public void Execute()
            {
                for (var i = 0; i < DestroyedEntities.Length; i++)
                {
                    if (MatchedLastQueryByEntity.IsSet(DestroyedEntities[i].Index))
                        MissingEntities.Enqueue(DestroyedEntities[i]);
                }
            }
        }

        [BurstCompile]
        struct DetectNewEntitiesJob : IJob
        {
            [ReadOnly] public EntityQueryMask QueryMask;
            public UnsafeBitArray MatchedLastQueryByEntity;
            [ReadOnly] public NativeArray<Entity> CreatedEntities;
            [WriteOnly] public NativeQueue<Entity>.ParallelWriter NewEntities;

            public void Execute()
            {
                for (var i = 0; i < CreatedEntities.Length; i++)
                {
                    var match = QueryMask.Matches(CreatedEntities[i]);
                    MatchedLastQueryByEntity.Set(CreatedEntities[i].Index, match);
                    if (match)
                        NewEntities.Enqueue(CreatedEntities[i]);
                }
            }
        }

        [BurstCompile]
        struct DetectNewAndMissingEntitiesFromStateBatchedJob : IJobParallelForBatch
        {
            [ReadOnly] public NativeArray<int> VersionByEntity;
            [ReadOnly] public EntityQueryMask QueryMask;
            public UnsafeBitArray MatchedLastQueryByEntity;
            [ReadOnly] public NativeList<Entity> CreatedEntities;
            [WriteOnly] public NativeQueue<Entity>.ParallelWriter NewEntities;
            [WriteOnly] public NativeQueue<Entity>.ParallelWriter MissingEntities;

            public void Execute(int startIndex, int count)
            {
                var createdEntitiesIdx = 0;
                for (int i = startIndex, endIndex = startIndex + count; i < endIndex; i++)
                {
                    if (VersionByEntity[i] == 0)
                        continue;

                    var e = new Entity { Index = i, Version = VersionByEntity[i] };
                    var match = QueryMask.Matches(e);
                    var wasMatch = MatchedLastQueryByEntity.IsSet(i);
                    MatchedLastQueryByEntity.Set(i, match);

                    if (match)
                    {
                        if (!wasMatch)
                            NewEntities.Enqueue(e); // Entity wasn't matching but now matches -> New entity detected
                        else
                        {
                            // Entity was matching and still matches, if the entity is in CreatedEntities then -> New entity detected
                            // Otherwise it's just the same entity as before no need to detect it as new.
                            var isNewEntity = false;
                            for (; createdEntitiesIdx < CreatedEntities.Length; createdEntitiesIdx++)
                            {
                                var entityIndex = CreatedEntities[createdEntitiesIdx].Index;
                                if (entityIndex > i)
                                    break;

                                if (entityIndex == i)
                                {
                                    isNewEntity = true;
                                    break;
                                }
                            }

                            if (isNewEntity)
                                NewEntities.Enqueue(e);
                        }
                    }
                    else if (wasMatch) // Entity is not matching but was matching before -> Missing entity detected
                        MissingEntities.Enqueue(e);
                }
            }
        }

        [BurstCompile]
        unsafe struct CopyNativeQueueToNativeList<T> : IJob where T : struct
        {
            [ReadOnly] public NativeQueue<T> Queue;
            public NativeList<T> List;

            public void Execute()
            {
                if (Queue.Count == 0)
                    return;

                var tmp = Queue.ToArray(Allocator.Temp);
                List.AddRange(tmp.GetUnsafeReadOnlyPtr(), tmp.Length);
                tmp.Dispose();
            }
        }

        public void Dispose()
        {
            m_VersionByEntity.Dispose();
            m_QueryMatchCacheByEntity.Dispose();
        }
    }
}

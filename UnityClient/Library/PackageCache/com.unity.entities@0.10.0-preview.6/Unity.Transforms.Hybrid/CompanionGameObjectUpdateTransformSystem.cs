#if !UNITY_DISABLE_MANAGED_COMPONENTS
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine.Jobs;

struct CompanionGameObjectUpdateTransformSystemState : ISystemStateComponentData
{
}

[UnityEngine.ExecuteAlways]
[UpdateAfter(typeof(TransformSystemGroup))]
public class CompanionGameObjectUpdateTransformSystem : JobComponentSystem
{
    NativeArray<Entity> m_Entities;
    TransformAccessArray m_TransformAccessArray;

    EntityQuery m_NewQuery;
    EntityQuery m_ExistingQuery;
    EntityQuery m_DestroyedQuery;

    protected override void OnCreate()
    {
        m_Entities = new NativeArray<Entity>(0, Allocator.Persistent);
        m_TransformAccessArray = new TransformAccessArray(0);

        m_NewQuery = GetEntityQuery(
            new EntityQueryDesc
            {
                All = new[] {ComponentType.ReadOnly<CompanionLink>()},
                None = new[] {ComponentType.ReadOnly<CompanionGameObjectUpdateTransformSystemState>()}
            }
        );

        m_ExistingQuery = GetEntityQuery(
            new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<CompanionLink>(),
                    ComponentType.ReadOnly<CompanionGameObjectUpdateTransformSystemState>()
                }
            }
        );

        m_DestroyedQuery = GetEntityQuery(
            new EntityQueryDesc
            {
                All = new[] {ComponentType.ReadOnly<CompanionGameObjectUpdateTransformSystemState>()},
                None = new[] {ComponentType.ReadOnly<CompanionLink>()}
            }
        );
    }

    protected override void OnDestroy()
    {
        m_Entities.Dispose();
        m_TransformAccessArray.Dispose();
    }

    protected unsafe override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!m_DestroyedQuery.IsEmptyIgnoreFilter || !m_NewQuery.IsEmptyIgnoreFilter)
        {
            EntityManager.AddComponent<CompanionGameObjectUpdateTransformSystemState>(m_NewQuery);
            EntityManager.RemoveComponent<CompanionGameObjectUpdateTransformSystemState>(m_DestroyedQuery);

            m_Entities.Dispose();
            m_Entities = m_ExistingQuery.ToEntityArray(Allocator.Persistent);

            var transforms = new UnityEngine.Transform[m_Entities.Length];
            for (int i = 0; i < m_Entities.Length; i++)
            {
                var link = EntityManager.GetComponentData<CompanionLink>(m_Entities[i]);
                transforms[i] = link.Companion.transform;
            }

            m_TransformAccessArray.SetTransforms(transforms);
        }
        else
        {
            m_ExistingQuery.SetChangedVersionFilter(typeof(CompanionLink));
            var iterator = m_ExistingQuery.GetArchetypeChunkIterator();
            var indexInEntityQuery = m_ExistingQuery.GetIndexInEntityQuery(TypeManager.GetTypeIndex<CompanionLink>());

            var access = EntityManager.GetCheckedEntityDataAccess();
            var mcs = access->ManagedComponentStore;

            var entityCounter = 0;
            while (iterator.MoveNext())
            {
                var chunk = iterator.CurrentArchetypeChunk;
                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    var link = (CompanionLink)iterator.GetManagedObject(mcs, indexInEntityQuery, entityIndex);
                    m_TransformAccessArray[entityCounter++] = link.Companion.transform;
                }
            }
            m_ExistingQuery.ResetFilter();
        }

        return new CopyTransformJob
        {
            localToWorld = GetComponentDataFromEntity<LocalToWorld>(),
            entities = m_Entities
        }.Schedule(m_TransformAccessArray, inputDeps);
    }

    [BurstCompile]
    struct CopyTransformJob : IJobParallelForTransform
    {
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<LocalToWorld> localToWorld;
        [ReadOnly] public NativeArray<Entity> entities;

        public unsafe void Execute(int index, TransformAccess transform)
        {
            var ltw = localToWorld[entities[index]];
            var mat = *(UnityEngine.Matrix4x4*) & ltw;
            transform.localPosition = ltw.Position;
            transform.localRotation = mat.rotation;
            transform.localScale = mat.lossyScale;
        }
    }
}
#endif

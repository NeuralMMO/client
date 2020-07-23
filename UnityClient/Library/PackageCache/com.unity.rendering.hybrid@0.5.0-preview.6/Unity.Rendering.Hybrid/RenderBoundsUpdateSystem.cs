using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Unity.Rendering
{
    /// <summary>
    /// During conversion flow we want to generate a scene bounding volume for each section
    /// </summary>
    [ConverterVersion("joe", 1)]
    [WorldSystemFilter(WorldSystemFilterFlags.EntitySceneOptimizations)]
    [UpdateAfter(typeof(RenderBoundsUpdateSystem))]
    class UpdateSceneBoundingVolumeFromRendererBounds : ComponentSystem
    {
        [BurstCompile]
        struct CollectSceneBoundsJob : IJob
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<WorldRenderBounds> RenderBounds;

            public Entity SceneBoundsEntity;
            public ComponentDataFromEntity<SceneBoundingVolume> SceneBounds;

            public void Execute()
            {
                var minMaxAabb = MinMaxAABB.Empty;
                for (int i = 0; i != RenderBounds.Length; i++)
                    minMaxAabb.Encapsulate(RenderBounds[i].Value);

                SceneBounds[SceneBoundsEntity] = new SceneBoundingVolume { Value = minMaxAabb };
            }
        }

        protected override void OnUpdate()
        {
            //@TODO: API does not allow me to use ChunkComponentData.
            //Review with simon how we can improve it.

            var query = GetEntityQuery(typeof(WorldRenderBounds), typeof(SceneSection));

            var sections = new List<SceneSection>();
            EntityManager.GetAllUniqueSharedComponentData(sections);
            foreach (var section in sections)
            {
                if (section.Equals(default(SceneSection)))
                    continue;

                query.SetSharedComponentFilter(section);

                var entity = EntityManager.CreateEntity(typeof(SceneBoundingVolume));
                EntityManager.AddSharedComponentData(entity, section);

                var job = new CollectSceneBoundsJob();
                job.RenderBounds = query.ToComponentDataArray<WorldRenderBounds>(Allocator.TempJob);
                job.SceneBoundsEntity = entity;
                job.SceneBounds = GetComponentDataFromEntity<SceneBoundingVolume>();
                job.Run();
            }

            query.ResetFilter();
        }
    }


    [ConverterVersion("joe", 1)]
    [UpdateInGroup(typeof(StructuralChangePresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    [ExecuteAlways]
    class AddWorldAndChunkRenderBounds : ComponentSystem
    {
        EntityQuery m_MissingWorldRenderBounds;
        EntityQuery m_MissingWorldChunkRenderBounds;

        protected override void OnCreate()
        {
            m_MissingWorldRenderBounds = GetEntityQuery
                (
                new EntityQueryDesc
                {
                    All = new[] {ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LocalToWorld>()},
                    None = new[] {ComponentType.ReadOnly<WorldRenderBounds>()},
                    Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
                }
                );

            m_MissingWorldChunkRenderBounds = GetEntityQuery
                (
                new EntityQueryDesc
                {
                    All = new[] {ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LocalToWorld>()},
                    None = new[] { ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>() },
                    Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
                }
                );
        }

        protected override void OnUpdate()
        {
            EntityManager.AddComponent(m_MissingWorldRenderBounds, ComponentType.ReadWrite<WorldRenderBounds>());
            EntityManager.AddComponent(m_MissingWorldChunkRenderBounds, ComponentType.ChunkComponent<ChunkWorldRenderBounds>());
        }
    }

    /// <summary>
    /// Updates WorldRenderBounds for anything that has LocalToWorld and RenderBounds (and ensures WorldRenderBounds exists)
    /// </summary>
    [ConverterVersion("joe", 2)]
    [UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    [ExecuteAlways]
    class RenderBoundsUpdateSystem : JobComponentSystem
    {
        EntityQuery m_WorldRenderBounds;

        [BurstCompile]
        struct BoundsJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkComponentType<RenderBounds> RendererBounds;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> LocalToWorld;
            public ArchetypeChunkComponentType<WorldRenderBounds> WorldRenderBounds;
            public ArchetypeChunkComponentType<ChunkWorldRenderBounds> ChunkWorldRenderBounds;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var worldBounds = chunk.GetNativeArray(WorldRenderBounds);
                var localBounds = chunk.GetNativeArray(RendererBounds);
                var localToWorld = chunk.GetNativeArray(LocalToWorld);
                MinMaxAABB combined = MinMaxAABB.Empty;
                for (int i = 0; i != localBounds.Length; i++)
                {
                    var transformed = AABB.Transform(localToWorld[i].Value, localBounds[i].Value);

                    worldBounds[i] = new WorldRenderBounds { Value = transformed };
                    combined.Encapsulate(transformed);
                }

                chunk.SetChunkComponentData(ChunkWorldRenderBounds, new ChunkWorldRenderBounds { Value = combined });
            }
        }

        protected override void OnCreate()
        {
            m_WorldRenderBounds = GetEntityQuery
                (
                new EntityQueryDesc
                {
                    All = new[] { ComponentType.ChunkComponent<ChunkWorldRenderBounds>(), ComponentType.ReadWrite<WorldRenderBounds>(), ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LocalToWorld>() },
                }
                );
            m_WorldRenderBounds.SetChangedVersionFilter(new[] { ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LocalToWorld>()});
        }

        protected override JobHandle OnUpdate(JobHandle dependency)
        {
            var boundsJob = new BoundsJob
            {
                RendererBounds = GetArchetypeChunkComponentType<RenderBounds>(true),
                LocalToWorld = GetArchetypeChunkComponentType<LocalToWorld>(true),
                WorldRenderBounds = GetArchetypeChunkComponentType<WorldRenderBounds>(),
                ChunkWorldRenderBounds = GetArchetypeChunkComponentType<ChunkWorldRenderBounds>(),
            };
            return boundsJob.ScheduleParallel(m_WorldRenderBounds, dependency);
        }

#if false
        public void DrawGizmos()
        {
            var boundsQuery = GetEntityQuery(typeof(LocalToWorld), typeof(WorldRenderBounds), typeof(RenderBounds));
            var localToWorlds = boundsQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            var worldBounds = boundsQuery.ToComponentDataArray<WorldRenderBounds>(Allocator.TempJob);
            var localBounds = boundsQuery.ToComponentDataArray<RenderBounds>(Allocator.TempJob);

            var chunkBoundsQuery = GetEntityQuery(ComponentType.ReadOnly<ChunkWorldRenderBounds>(), typeof(ChunkHeader));
            var chunksBounds = chunkBoundsQuery.ToComponentDataArray<ChunkWorldRenderBounds>(Allocator.TempJob);

            Gizmos.matrix = Matrix4x4.identity;

            // world bounds
            Gizmos.color = Color.green;
            for (int i = 0; i != worldBounds.Length; i++)
                Gizmos.DrawWireCube(worldBounds[i].Value.Center, worldBounds[i].Value.Size);

            // chunk world bounds
            Gizmos.color = Color.yellow;
            for (int i = 0; i != chunksBounds.Length; i++)
                Gizmos.DrawWireCube(chunksBounds[i].Value.Center, chunksBounds[i].Value.Size);

            // local render bounds
            Gizmos.color = Color.blue;
            for (int i = 0; i != localToWorlds.Length; i++)
            {
                Gizmos.matrix = new Matrix4x4(localToWorlds[i].Value.c0, localToWorlds[i].Value.c1, localToWorlds[i].Value.c2, localToWorlds[i].Value.c3);
                Gizmos.DrawWireCube(localBounds[i].Value.Center, localBounds[i].Value.Size);
            }

            localToWorlds.Dispose();
            worldBounds.Dispose();
            localBounds.Dispose();
            chunksBounds.Dispose();
        }

        //@TODO: We really need a system level gizmo callback.
        [UnityEditor.DrawGizmo(UnityEditor.GizmoType.NonSelected)]
        public static void DrawGizmos(Light light, UnityEditor.GizmoType type)
        {
            if (light.type == LightType.Directional && light.isActiveAndEnabled)
            {
                if (World.DefaultGameObjectInjectionWorld == null)
                    return;

                var renderer = World.DefaultGameObjectInjectionWorld.GetExistingSystem<RenderBoundsUpdateSystem>();
                if (renderer != null)
                    renderer.DrawGizmos();
            }
        }

#endif
    }
}

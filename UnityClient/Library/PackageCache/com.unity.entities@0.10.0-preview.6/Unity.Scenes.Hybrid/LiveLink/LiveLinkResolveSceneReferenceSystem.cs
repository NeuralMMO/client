//#define LOG_RESOLVING

using System.Diagnostics;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Unity.Scenes
{
    internal struct WaitingForEditor : IComponentData
    {
    }

    internal struct EditorTriggeredLoad : IComponentData
    {
    }

#if UNITY_EDITOR
    [DisableAutoCreation]
#endif
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(SceneSystemGroup))]
    [UpdateAfter(typeof(SceneSystem))]
    class LiveLinkResolveSceneReferenceSystem : ComponentSystem
    {
        private EntityQuery m_NotYetRequestedScenes;
        private EntityQuery m_WaitingForEditorScenes;
        private EntityQuery m_ResolvedScenes;

        [Conditional("LOG_RESOLVING")]
        void LogResolving(string type, Hash128 sceneGUID)
        {
            Debug.Log(type + ": " + sceneGUID);
        }

        void ResolveScene(Entity sceneEntity, ref SceneReference scene, RequestSceneLoaded requestSceneLoaded, Hash128 artifactHash)
        {
            // Resolve first (Even if the file doesn't exist we want to stop continously trying to load the section)
            EntityManager.AddBuffer<ResolvedSectionEntity>(sceneEntity);
            EntityManager.AddComponentData(sceneEntity, new ResolvedSceneHash { ArtifactHash = artifactHash });

            var sceneHeaderPath = EntityScenesPaths.GetLiveLinkCachePath(artifactHash, EntityScenesPaths.PathType.EntitiesHeader, -1);

            if (!BlobAssetReference<SceneMetaData>.TryRead(sceneHeaderPath, SceneMetaDataSerializeUtility.CurrentFileFormatVersion, out var sceneMetaDataRef))
            {
                Debug.LogError("Loading Entity Scene failed because the entity header file was an old version or doesn't exist: " + scene.SceneGUID);
                return;
            }

            LogResolving("ResolveScene (success)", scene.SceneGUID);

            ref var sceneMetaData = ref sceneMetaDataRef.Value;

#if UNITY_EDITOR
            var sceneName = sceneMetaData.SceneName.ToString();
            EntityManager.SetName(sceneEntity, $"Scene: {sceneName}");
#endif

            var loadSections = !requestSceneLoaded.LoadFlags.HasFlag(SceneLoadFlags.DisableAutoLoad);

            for (int i = 0; i != sceneMetaData.Sections.Length; i++)
            {
                var sectionEntity = EntityManager.CreateEntity();
                var sectionIndex = sceneMetaData.Sections[i].SubSectionIndex;
#if UNITY_EDITOR
                EntityManager.SetName(sectionEntity, $"SceneSection: {sceneName} ({sectionIndex})");
#endif

                if (loadSections)
                {
                    EntityManager.AddComponentData(sectionEntity, requestSceneLoaded);
                }

                EntityManager.AddComponentData(sectionEntity, sceneMetaData.Sections[i]);
                EntityManager.AddComponentData(sectionEntity, new SceneBoundingVolume { Value = sceneMetaData.Sections[i].BoundingVolume });
                EntityManager.AddComponentData(sectionEntity, new SceneEntityReference {SceneEntity = sceneEntity});

                var sectionPath = new ResolvedSectionPath();
                var hybridPath = EntityScenesPaths.GetLiveLinkCachePath(artifactHash, EntityScenesPaths.PathType.EntitiesUnitObjectReferencesBundle, sectionIndex);
                var scenePath = EntityScenesPaths.GetLiveLinkCachePath(artifactHash, EntityScenesPaths.PathType.EntitiesBinary, sectionIndex);

                sectionPath.ScenePath.SetString(scenePath);
                if (hybridPath != null)
                    sectionPath.HybridPath.SetString(hybridPath);

                EntityManager.AddComponentData(sectionEntity, sectionPath);

                ResolveSceneSectionUtility.AddSectionMetadataComponents(sectionEntity, ref sceneMetaData.SceneSectionCustomMetadata[i], EntityManager);

                var buffer = EntityManager.GetBuffer<ResolvedSectionEntity>(sceneEntity);
                buffer.Add(new ResolvedSectionEntity { SectionEntity = sectionEntity });
            }
            sceneMetaDataRef.Dispose();
        }

        protected override void OnUpdate()
        {
            Enabled = LiveLinkUtility.LiveLinkEnabled;
            if (!Enabled)
                return;

            var sceneSystem = World.GetExistingSystem<SceneSystem>();
            var buildConfigurationGUID = sceneSystem.BuildConfigurationGUID;
            var liveLinkPlayerAssetRefreshSystem = World.GetExistingSystem<LiveLinkPlayerAssetRefreshSystem>();

            // For each resolved scene, check whether the hash has changed. If so, return the scene to the 'not yet
            // requested' state.
            Entities.With(m_ResolvedScenes).ForEach((Entity sceneEntity, ref SceneReference scene, ref ResolvedSceneHash resolvedScene) =>
            {
                var subSceneGUID = new SubSceneGUID(scene.SceneGUID, buildConfigurationGUID);
                if (liveLinkPlayerAssetRefreshSystem.TrackedSubScenes[subSceneGUID] != resolvedScene.ArtifactHash)
                {
                    if (sceneEntity != Entity.Null && !EntityManager.HasComponent<DisableSceneResolveAndLoad>(sceneEntity))
                    {
                        var unloadFlags = SceneSystem.UnloadParameters.DestroySectionProxyEntities | SceneSystem.UnloadParameters.DontRemoveRequestSceneLoaded;
                        sceneSystem.UnloadScene(sceneEntity, unloadFlags);
                    }
                }
            });

            // For each scene that we are waiting for, check whether we have received all data from the editor. Then
            // mark it as resolved and start streaming it in.
            Entities.With(m_WaitingForEditorScenes).ForEach((Entity sceneEntity, ref SceneReference scene, ref RequestSceneLoaded requestSceneLoaded) =>
            {
                var subSceneGUID = new SubSceneGUID(scene.SceneGUID, buildConfigurationGUID);
                // Check if Scene is ready?
                if (liveLinkPlayerAssetRefreshSystem.IsSubSceneReady(subSceneGUID))
                {
                    var trackedTargetHash = liveLinkPlayerAssetRefreshSystem.GetTrackedSubSceneTargetHash(subSceneGUID);
                    EntityManager.RemoveComponent<WaitingForEditor>(sceneEntity);
                    ResolveScene(sceneEntity, ref scene, requestSceneLoaded, trackedTargetHash);
                }
            });

            // We are seeing this scene for the first time, so we need to schedule a request. This moves the scene into
            // the 'waiting for editor' state.
            Entities.With(m_NotYetRequestedScenes).ForEach((Entity sceneEntity, ref SceneReference scene, ref RequestSceneLoaded requestSceneLoaded) =>
            {
                var subSceneGUID = new SubSceneGUID(scene.SceneGUID, buildConfigurationGUID);

                liveLinkPlayerAssetRefreshSystem.TrackedSubScenes[subSceneGUID] = new Hash128();
                var archetype = EntityManager.CreateArchetype(typeof(SubSceneGUID));
                var entity = EntityManager.CreateEntity(archetype);
                EntityManager.SetComponentData(entity, subSceneGUID);
            });
            EntityManager.AddComponent<WaitingForEditor>(m_NotYetRequestedScenes);
        }

        protected override void OnCreate()
        {
            // Each scene is in one of three states:
            //  * not yet requested
            //  * waiting for update from editor
            //  * resolved
            // The OnUpdate method handles the transition between these three states.
            // A new scene starts as 'not yet requested'

            m_NotYetRequestedScenes = GetEntityQuery(ComponentType.ReadWrite<SceneReference>(),
                ComponentType.ReadOnly<EditorTriggeredLoad>(),
                ComponentType.ReadWrite<RequestSceneLoaded>(),

                ComponentType.Exclude<ResolvedSectionEntity>(),
                ComponentType.Exclude<WaitingForEditor>(),
                ComponentType.Exclude<DisableSceneResolveAndLoad>());

            m_WaitingForEditorScenes = GetEntityQuery(ComponentType.ReadWrite<SceneReference>(),
                ComponentType.ReadOnly<EditorTriggeredLoad>(),
                ComponentType.ReadWrite<RequestSceneLoaded>(),
                ComponentType.ReadWrite<WaitingForEditor>(),

                ComponentType.Exclude<ResolvedSectionEntity>(),
                ComponentType.Exclude<DisableSceneResolveAndLoad>());

            m_ResolvedScenes = GetEntityQuery(ComponentType.ReadWrite<SceneReference>(),
                ComponentType.ReadOnly<EditorTriggeredLoad>(),
                ComponentType.ReadWrite<RequestSceneLoaded>(),
                ComponentType.ReadWrite<ResolvedSectionEntity>(),
                ComponentType.ReadWrite<ResolvedSceneHash>(),

                ComponentType.Exclude<DisableSceneResolveAndLoad>());
        }
    }
}

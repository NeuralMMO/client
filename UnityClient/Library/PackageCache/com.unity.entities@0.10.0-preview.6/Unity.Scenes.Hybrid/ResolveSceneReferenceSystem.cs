//#define LOG_RESOLVING

using System.Diagnostics;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Unity.Scenes
{
    public struct ResolvedSectionEntity : ISystemStateBufferElementData
    {
        public Entity SectionEntity;
    }

    struct SceneEntityReference : IComponentData
    {
        public Entity SceneEntity;
    }
    struct ResolvedSceneHash : IComponentData
    {
        public Hash128 ArtifactHash;
    }
    struct ResolvedSectionPath : IComponentData
    {
        //@TODO: Switch back to NativeString512 once bugs are fixed
        public Words ScenePath;
        public Words HybridPath;
    }

    struct SceneSectionCustomMetadata
    {
        public ulong StableTypeHash;
        public BlobArray<byte> Data;
    }

    struct SceneMetaData
    {
        public BlobArray<SceneSectionData> Sections;
        public BlobString                  SceneName;
        public BlobArray<BlobArray<SceneSectionCustomMetadata>> SceneSectionCustomMetadata;
    }

    public struct DisableSceneResolveAndLoad : IComponentData
    {
    }


    static class SceneMetaDataSerializeUtility
    {
        public static readonly int CurrentFileFormatVersion = 1;
    }

    /// <summary>
    /// Scenes are made out of sections, but to find out how many sections there are and extract their data like bounding volume or file size.
    /// The meta data for the scene has to be loaded first.
    /// ResolveSceneReferenceSystem creates section entities for each scene by loading the scenesection's metadata from disk.
    /// </summary>
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(SceneSystemGroup))]
    [UpdateAfter(typeof(SceneSystem))]
    class ResolveSceneReferenceSystem : ComponentSystem
    {
        private NativeList<Hash128> m_ChangedScenes = new NativeList<Hash128>(Allocator.Persistent);
        private EntityQuery m_ScenesToRequest;
#if UNITY_EDITOR && !USE_SUBSCENE_EDITORBUNDLES
        private EntityQuery m_ImportingScenes;
#endif
        private EntityQuery m_ResolvedScenes;

        public void NotifySceneContentsHasChanged(Hash128 scene)
        {
            m_ChangedScenes.Add(scene);
        }

        [Conditional("LOG_RESOLVING")]
        void LogResolving(string type, Hash128 sceneGUID)
        {
            Debug.Log(type + ": " + sceneGUID);
        }

        void UpdateSceneContentsChanged(Hash128 buildConfigurationGUID)
        {
#if UNITY_EDITOR
            Entities.With(m_ResolvedScenes).ForEach((Entity sceneEntity, ref SceneReference scene, ref ResolvedSceneHash resolvedScene) =>
            {
                LogResolving("Queuing UpdateSceneContentsChanged", scene.SceneGUID);
                var hash = EntityScenesPaths.GetSubSceneArtifactHash(scene.SceneGUID, buildConfigurationGUID, ImportMode.Asynchronous);
                if ((hash != default) && (hash != resolvedScene.ArtifactHash))
                {
                    LogResolving("Scene hash changed", scene.SceneGUID);
                    NotifySceneContentsHasChanged(scene.SceneGUID);
                }
            });
#endif

            if (m_ChangedScenes.Length != 0)
            {
                var sceneSystem = World.GetExistingSystem<SceneSystem>();
                foreach (var scene in m_ChangedScenes)
                {
                    var sceneEntity = sceneSystem.GetSceneEntity(scene);

                    // Don't touch it if the scene is under live link control (@Todo: SubSceneStreamingSystem.IgnoreTag could be live link specific?)
                    if (sceneEntity != Entity.Null && !EntityManager.HasComponent<DisableSceneResolveAndLoad>(sceneEntity))
                    {
                        var unloadFlags = SceneSystem.UnloadParameters.DestroySectionProxyEntities | SceneSystem.UnloadParameters.DontRemoveRequestSceneLoaded;
                        sceneSystem.UnloadScene(sceneEntity, unloadFlags);
                    }
                    Assertions.Assert.IsTrue(EntityManager.GetEntityQueryMask(m_ScenesToRequest).Matches(sceneEntity));
                }
                m_ChangedScenes.Clear();
            }
        }

        void ResolveScene(Entity sceneEntity, ref SceneReference scene, RequestSceneLoaded requestSceneLoaded, Hash128 artifactHash)
        {
            if (ResolveSceneSectionUtility.ResolveSceneSections(EntityManager, sceneEntity, scene.SceneGUID, requestSceneLoaded, artifactHash))
            {
                LogResolving("ResolveScene (success)", scene.SceneGUID);
                Assertions.Assert.IsTrue(EntityManager.GetEntityQueryMask(m_ResolvedScenes).Matches(sceneEntity));
            }
            else
                LogResolving("ResolveScene (failed)", scene.SceneGUID);
        }

        //@TODO: What happens if we change source assets between queuing a request for the first time and it being resolved?

        protected override void OnUpdate()
        {
            //TODO: How can we disable systems in specific builds?
#if !UNITY_EDITOR
            Enabled = !LiveLinkUtility.LiveLinkEnabled;
            if (!Enabled)
                return;
#else
            SceneWithBuildConfigurationGUIDs.ValidateBuildSettingsCache();
#endif
            var buildConfigurationGUID = World.GetExistingSystem<SceneSystem>().BuildConfigurationGUID;

            UpdateSceneContentsChanged(buildConfigurationGUID);

#if UNITY_EDITOR && !USE_SUBSCENE_EDITORBUNDLES
            Entities.With(m_ImportingScenes).ForEach((Entity sceneEntity, ref SceneReference scene, ref RequestSceneLoaded requestSceneLoaded) =>
            {
                var hash = EntityScenesPaths.GetSubSceneArtifactHash(scene.SceneGUID, buildConfigurationGUID, ImportMode.NoImport);
                if (hash.IsValid)
                {
                    LogResolving("Polling Importing (completed)", scene.SceneGUID);
                    ResolveScene(sceneEntity, ref scene, requestSceneLoaded, hash);
                }
                else
                {
                    LogResolving("Polling Importing (not complete)", scene.SceneGUID);
                }
            });
#endif


            //@TODO: Temporary workaround to prevent crash after build player
            if (m_ScenesToRequest.IsEmptyIgnoreFilter)
                return;

            // We are seeing this scene for the first time, so we need to schedule a request.
            Entities.With(m_ScenesToRequest).ForEach((Entity sceneEntity, ref SceneReference scene, ref RequestSceneLoaded requestSceneLoaded) =>
            {
#if UNITY_EDITOR && !USE_SUBSCENE_EDITORBUNDLES
                var blocking = (requestSceneLoaded.LoadFlags & SceneLoadFlags.BlockOnImport) != 0;
                var importMode = blocking ? ImportMode.Synchronous : ImportMode.Asynchronous;

                var hash = EntityScenesPaths.GetSubSceneArtifactHash(scene.SceneGUID, buildConfigurationGUID, importMode);
                if (hash.IsValid)
                {
                    LogResolving(blocking ? "Blocking import (completed)" : "Queue not yet requested (completed)", scene.SceneGUID);
                    ResolveScene(sceneEntity, ref scene, requestSceneLoaded, hash);
                }
                else
                    LogResolving(blocking ? "Blocking import (failed)" : "Queue not yet requested (not complete)", scene.SceneGUID);
#else
                ResolveScene(sceneEntity, ref scene, requestSceneLoaded, new Hash128());
#endif
            });
            EntityManager.AddComponent(m_ScenesToRequest, ComponentType.ReadWrite<ResolvedSectionEntity>());
        }

        protected override void OnCreate()
        {
            m_ScenesToRequest = GetEntityQuery(ComponentType.ReadWrite<SceneReference>(),
                ComponentType.ReadWrite<RequestSceneLoaded>(),
                ComponentType.Exclude<ResolvedSectionEntity>(),
                ComponentType.Exclude<DisableSceneResolveAndLoad>());

#if UNITY_EDITOR && !USE_SUBSCENE_EDITORBUNDLES
            m_ImportingScenes = GetEntityQuery(ComponentType.ReadWrite<SceneReference>(),
                ComponentType.ReadWrite<RequestSceneLoaded>(),
                ComponentType.ReadWrite<ResolvedSectionEntity>(),
                ComponentType.Exclude<ResolvedSceneHash>(),
                ComponentType.Exclude<DisableSceneResolveAndLoad>());
#endif

            m_ResolvedScenes = GetEntityQuery(ComponentType.ReadWrite<SceneReference>(),
                ComponentType.ReadWrite<RequestSceneLoaded>(),
                ComponentType.ReadWrite<ResolvedSectionEntity>(),
#if UNITY_EDITOR && !USE_SUBSCENE_EDITORBUNDLES
                ComponentType.ReadWrite<ResolvedSceneHash>(),
#endif
                ComponentType.Exclude<DisableSceneResolveAndLoad>());
        }

        protected override void OnDestroy()
        {
            m_ChangedScenes.Dispose();
        }
    }
}

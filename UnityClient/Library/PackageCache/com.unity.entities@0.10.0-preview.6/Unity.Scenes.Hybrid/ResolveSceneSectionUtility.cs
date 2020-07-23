using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Unity.Scenes
{
    public static class ResolveSceneSectionUtility
    {
        public static bool ResolveSceneSections(EntityManager EntityManager, Entity sceneEntity, Hash128 sceneGUID, RequestSceneLoaded requestSceneLoaded, Hash128 artifactHash)
        {
            // Resolve first (Even if the file doesn't exist we want to stop continously trying to load the section)
            EntityManager.AddBuffer<ResolvedSectionEntity>(sceneEntity);

    #if UNITY_EDITOR && !USE_SUBSCENE_EDITORBUNDLES
            EntityManager.AddComponentData(sceneEntity, new ResolvedSceneHash { ArtifactHash = artifactHash });

            AssetDatabaseCompatibility.GetArtifactPaths(artifactHash, out var paths);

            var sceneHeaderPath = EntityScenesPaths.GetLoadPathFromArtifactPaths(paths, EntityScenesPaths.PathType.EntitiesHeader);
    #else
            var sceneHeaderPath = EntityScenesPaths.GetLoadPath(sceneGUID, EntityScenesPaths.PathType.EntitiesHeader, -1);
    #endif

            // @TODO: AsyncReadManager currently crashes with empty path.
            //        It should be possible to remove this after that is fixed.
            if (String.IsNullOrEmpty(sceneHeaderPath))
            {
                Debug.LogError($"Loading Entity Scene failed because the entity header file couldn't be resolved: guid={sceneGUID}.");
                return false;
            }

            if (!BlobAssetReference<SceneMetaData>.TryRead(sceneHeaderPath, SceneMetaDataSerializeUtility.CurrentFileFormatVersion, out var sceneMetaDataRef))
            {
    #if UNITY_EDITOR
                Debug.LogError($"Loading Entity Scene failed because the entity header file was an old version or doesn't exist: guid={sceneGUID} path={sceneHeaderPath}");
    #else
                Debug.LogError($"Loading Entity Scene failed because the entity header file was an old version or doesn't exist: {sceneGUID}\nNOTE: In order to load SubScenes in the player you have to use the new BuildConfiguration asset based workflow to build & run your player.\n{sceneHeaderPath}");
    #endif
                return false;
            }

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
    #if !UNITY_EDITOR || USE_SUBSCENE_EDITORBUNDLES
                var hybridPath = EntityScenesPaths.GetLoadPath(sceneGUID, EntityScenesPaths.PathType.EntitiesUnityObjectReferences, sectionIndex);
                var scenePath = EntityScenesPaths.GetLoadPath(sceneGUID, EntityScenesPaths.PathType.EntitiesBinary, sectionIndex);
    #else
                var scenePath = EntityScenesPaths.GetLoadPathFromArtifactPaths(paths, EntityScenesPaths.PathType.EntitiesBinary, sectionIndex);
                var hybridPath = EntityScenesPaths.GetLoadPathFromArtifactPaths(paths, EntityScenesPaths.PathType.EntitiesUnityObjectReferences, sectionIndex);
    #endif

                sectionPath.ScenePath.SetString(scenePath);
                if (hybridPath != null)
                    sectionPath.HybridPath.SetString(hybridPath);

                EntityManager.AddComponentData(sectionEntity, sectionPath);

    #if UNITY_EDITOR
                if (EntityManager.HasComponent<SubScene>(sceneEntity))
                    EntityManager.AddComponentObject(sectionEntity, EntityManager.GetComponentObject<SubScene>(sceneEntity));
    #endif

                AddSectionMetadataComponents(sectionEntity, ref sceneMetaData.SceneSectionCustomMetadata[i], EntityManager);

                var buffer = EntityManager.GetBuffer<ResolvedSectionEntity>(sceneEntity);
                buffer.Add(new ResolvedSectionEntity { SectionEntity = sectionEntity });
            }
            sceneMetaDataRef.Dispose();

            return true;
        }

        internal static unsafe void AddSectionMetadataComponents(Entity sectionEntity, ref BlobArray<SceneSectionCustomMetadata> sectionMetaDataArray, EntityManager entityManager)
        {
            // Deserialize the SceneSection custom metadata
            for (var i = 0; i < sectionMetaDataArray.Length; i++)
            {
                ref var metadata = ref sectionMetaDataArray[i];
                var customTypeIndex = TypeManager.GetTypeIndexFromStableTypeHash(metadata.StableTypeHash);

                // Couldn't find the type...
                if (customTypeIndex == -1)
                {
                    UnityEngine.Debug.LogError(
                        $"Couldn't import SceneSection metadata, couldn't find the type to deserialize with stable hash {metadata.StableTypeHash}");
                    continue;
                }

                entityManager.AddComponent(sectionEntity, ComponentType.FromTypeIndex(customTypeIndex));

                if (TypeManager.IsZeroSized(customTypeIndex))
                    continue;

                void* componentPtr = entityManager.GetComponentDataRawRW(sectionEntity, customTypeIndex);
                UnsafeUtility.MemCpy(componentPtr, metadata.Data.GetUnsafePtr(), metadata.Data.Length);
            }
        }
    }
}

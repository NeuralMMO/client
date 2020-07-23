using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Properties;
using Unity.Serialization.Json;
using Unity.Serialization.Json.Adapters;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Unity.Build.Common
{
    /// <summary>
    /// Contains information about the scenes for the build.
    /// </summary>
    public sealed class SceneList : IBuildComponent
    {
        /// <summary>
        /// Info for each scene included in the build.
        /// </summary>
        public struct SceneInfo
        {
            /// <summary>
            /// The scene asset identifier.
            /// </summary>
            [CreateProperty, AssetGuid(typeof(SceneAsset))]
            public GlobalObjectId Scene { get; set; }

            /// <summary>
            /// If true, the scene is auto loaded in the player.  The first scene is always auto loaded.
            /// </summary>
            [CreateProperty]
            public bool AutoLoad { get; set; }

            /// <summary>
            /// Get scene path.
            /// </summary>
            public string Path => AssetDatabase.GUIDToAssetPath(Scene.assetGUID.ToString());
        }

        /// <summary>
        /// If selected, the scenes currently open will be returned by the GetScenesPathsToLoad & GetScenePathsForBuild.
        /// </summary>
        [CreateProperty]
        public bool BuildCurrentScene { get; set; }

        /// <summary>
        /// The list of scene infos for the build.
        /// </summary>
        [CreateProperty]
        public List<SceneInfo> SceneInfos { get; set; } = new List<SceneInfo>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Scenes has been replaced by SceneInfos. (RemovedAfter 2020-05-01)")]
        public List<GlobalObjectId> Scenes
        {
            get => throw null;
            set => throw null;
        }

        /// <summary>
        /// Returns all scenes marked as auto load.  The first scene is always included.
        /// </summary>
        /// <returns>The array of scene paths to atuomatically load.</returns>
        public string[] GetScenePathsToLoad()
        {
            if (BuildCurrentScene)
            {
                return GetScenePathsForBuild();
            }

            var initialScenes = new List<string>();
            for (int i = 0; i < SceneInfos.Count; i++)
            {
                if (i == 0 || SceneInfos[i].AutoLoad)
                {
                    initialScenes.Add(AssetDatabase.GUIDToAssetPath(SceneInfos[i].Scene.assetGUID.ToString()));
                }
            }
            return initialScenes.ToArray();
        }

        /// <summary>
        /// Gets the scene paths that will be included in the build.
        /// </summary>
        /// <returns>The array of scene paths.</returns>
        public string[] GetScenePathsForBuild()
        {
            return GetSceneInfosForBuild().Select(s => s.Path).ToArray();
        }

        /// <summary>
        /// Gets the current scene infos for the build. If BuildCurrentScene is checked, all open scenes are returned.
        /// </summary>
        /// <returns>The array of SceneInfos.</returns>
        public SceneInfo[] GetSceneInfosForBuild()
        {
            if (BuildCurrentScene)
            {
                // Build a list of the root scenes
                var rootScenes = new List<SceneInfo>();
                for (int i = 0; i != EditorSceneManager.sceneCount; i++)
                {
                    var scene = EditorSceneManager.GetSceneAt(i);
                    if (scene.isSubScene)
                        continue;
                    if (!scene.isLoaded)
                        continue;
                    if (EditorSceneManager.IsPreviewScene(scene))
                        continue;
                    if (string.IsNullOrEmpty(scene.path))
                        continue;
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                    rootScenes.Add(new SceneInfo() { AutoLoad = true, Scene = GlobalObjectId.GetGlobalObjectIdSlow(sceneAsset) });
                }

                return rootScenes.ToArray();
            }
            else
            {
                return SceneInfos.ToArray();
            }
        }

        class SceneListMigration : IJsonMigration<SceneList>
        {
            [InitializeOnLoadMethod]
            static void Register() => JsonSerialization.AddGlobalMigration(new SceneListMigration());

            public int Version => 1;

            public SceneList Migrate(JsonMigrationContext context)
            {
                context.TryRead<SceneList>(out var sceneList);
                if (context.SerializedVersion == 0)
                {
                    if (context.TryRead<List<GlobalObjectId>>("Scenes", out var scenes))
                    {
                        for (var i = 0; i < scenes.Count; ++i)
                        {
                            sceneList.SceneInfos.Add(new SceneInfo() { Scene = scenes[i], AutoLoad = i == 0 });
                        }
                    }
                }
                return sceneList;
            }
        }
    }
}

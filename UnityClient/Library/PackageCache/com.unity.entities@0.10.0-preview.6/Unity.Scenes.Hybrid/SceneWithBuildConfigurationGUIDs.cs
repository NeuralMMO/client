#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental;

namespace Unity.Scenes
{
    struct SceneWithBuildConfigurationGUIDs
    {
        public Hash128 SceneGUID;
        public Hash128 BuildConfiguration;

        // Currently used to allow us to force subscenes to reimport
        // TODO: Remove this when we have the ability to solve this with the asset database
        public long DirtyValue;

        static HashSet<Hash128> s_BuildConfigurationCreated = new HashSet<Hash128>();
        private static long s_AssetRefreshCounter = 0;

        const string k_SceneDependencyCachePath = "Assets/SceneDependencyCache";

        internal static void ClearBuildSettingsCache()
        {
            s_BuildConfigurationCreated.Clear();
            AssetDatabase.DeleteAsset(k_SceneDependencyCachePath);
        }

        internal static void ValidateBuildSettingsCache()
        {
            // Invalidate cache if we had an asset refresh
            var refreshDelta = AssetDatabaseExperimental.counters.import.refresh.total;
            if (s_AssetRefreshCounter != refreshDelta)
                s_BuildConfigurationCreated.Clear();

            s_AssetRefreshCounter = refreshDelta;
        }

        public static string GetSceneWithBuildSettingsPath(ref Hash128 guid)
        {
            return $"{k_SceneDependencyCachePath}/{guid}.sceneWithBuildSettings";
        }

        public static Hash128 Dirty(Hash128 sceneGUID, Hash128 buildConfigurationGUID)
        {
            var guid = ComputeBuildConfigurationGUID(sceneGUID, buildConfigurationGUID);
            var fileName = GetSceneWithBuildSettingsPath(ref guid);

            if (File.Exists(fileName))
            {
                var sceneWithBuildConfigurationGUIDs = new SceneWithBuildConfigurationGUIDs { SceneGUID = sceneGUID, BuildConfiguration = buildConfigurationGUID, DirtyValue = DateTime.UtcNow.Ticks};
                WriteSceneWithBuildSettings(ref guid, ref sceneWithBuildConfigurationGUIDs, fileName);
            }

            return guid;
        }

        private static unsafe void WriteSceneWithBuildSettings(ref Hash128 guid, ref SceneWithBuildConfigurationGUIDs sceneWithBuildConfigurationGUIDs, string path)
        {
            Directory.CreateDirectory(k_SceneDependencyCachePath);
            using (var writer = new StreamBinaryWriter(path))
            {
                fixed(void* vp = &sceneWithBuildConfigurationGUIDs)
                {
                    writer.WriteBytes(vp, sizeof(SceneWithBuildConfigurationGUIDs));
                }
            }
            File.WriteAllText(path + ".meta",
                $"fileFormatVersion: 2\nguid: {guid}\nDefaultImporter:\n  externalObjects: {{}}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n");

            // Refresh is necessary because it appears the asset pipeline
            // can't depend on an asset on disk that has not yet been refreshed.
            AssetDatabase.Refresh();
        }

        public static unsafe Hash128 EnsureExistsFor(Hash128 sceneGUID, Hash128 buildConfigurationGUID)
        {
            var guid = ComputeBuildConfigurationGUID(sceneGUID, buildConfigurationGUID);

            if (s_BuildConfigurationCreated.Contains(guid))
                return guid;

            var sceneWithBuildConfigurationGUIDs = new SceneWithBuildConfigurationGUIDs { SceneGUID = sceneGUID, BuildConfiguration = buildConfigurationGUID, DirtyValue = 0};

            var fileName = GetSceneWithBuildSettingsPath(ref guid);
            if (!File.Exists(fileName))
            {
                WriteSceneWithBuildSettings(ref guid, ref sceneWithBuildConfigurationGUIDs, fileName);
            }

            s_BuildConfigurationCreated.Add(guid);

            return guid;
        }

        public static unsafe SceneWithBuildConfigurationGUIDs ReadFromFile(string path)
        {
            SceneWithBuildConfigurationGUIDs sceneWithBuildConfiguration = default;
            using (var reader = new StreamBinaryReader(path))
            {
                reader.ReadBytes(&sceneWithBuildConfiguration, sizeof(SceneWithBuildConfigurationGUIDs));
            }
            return sceneWithBuildConfiguration;
        }

        static unsafe Hash128 ComputeBuildConfigurationGUID(Hash128 sceneGUID, Hash128 buildConfigurationGUID)
        {
            var guids = new SceneWithBuildConfigurationGUIDs { SceneGUID = sceneGUID, BuildConfiguration = buildConfigurationGUID};
            Hash128 guid;
            guid.Value.x = math.hash(&guids, sizeof(SceneWithBuildConfigurationGUIDs));
            guid.Value.y = math.hash(&guids, sizeof(SceneWithBuildConfigurationGUIDs), 0x96a755e2);
            guid.Value.z = math.hash(&guids, sizeof(SceneWithBuildConfigurationGUIDs), 0x4e936206);
            guid.Value.w = math.hash(&guids, sizeof(SceneWithBuildConfigurationGUIDs), 0xac602639);
            return guid;
        }
    }
}
#endif

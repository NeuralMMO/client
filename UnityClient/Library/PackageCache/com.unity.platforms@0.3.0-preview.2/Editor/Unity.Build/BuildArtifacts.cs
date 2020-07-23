using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    /// <summary>
    /// API for managing build artifacts.
    /// </summary>
    public static class BuildArtifacts
    {
        static readonly Dictionary<string, ArtifactData> s_ArtifactDataCache = new Dictionary<string, ArtifactData>();
        internal static string BaseDirectory => "Library/BuildArtifacts";

        class ArtifactData
        {
            public BuildResult Result;
            public List<IBuildArtifact> Artifacts = new List<IBuildArtifact>();
        }

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <see cref="Type"/>.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <param name="type">The type of the build artifact.</param>
        /// <returns>The build artifact if found, <see langword="null"/> otherwise.</returns>
        public static IBuildArtifact GetBuildArtifact(BuildConfiguration config, Type type)
        {
            if (config == null || !config)
            {
                return null;
            }

            if (type == null || type == typeof(object))
            {
                return null;
            }

            if (!typeof(IBuildArtifact).IsAssignableFrom(type))
            {
                return null;
            }

            var artifactData = GetArtifactData(config);
            if (artifactData == null || artifactData.Artifacts == null || artifactData.Artifacts.Count == 0)
            {
                return null;
            }

            return artifactData.Artifacts.FirstOrDefault(a => type.IsAssignableFrom(a.GetType()));
        }

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the build artifact.</typeparam>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build artifact if found, <see langword="null"/> otherwise.</returns>
        public static T GetBuildArtifact<T>(BuildConfiguration config) where T : class, IBuildArtifact => (T)GetBuildArtifact(config, typeof(T));

        /// <summary>
        /// Get the last build result from building the build configuration specified.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build result if found, <see langword="null"/> otherwise.</returns>
        public static BuildResult GetBuildResult(BuildConfiguration config) => GetArtifactData(config)?.Result;

        /// <summary>
        /// Clean all build artifact files.
        /// </summary>
        public static void Clean()
        {
            s_ArtifactDataCache.Clear();
            if (Directory.Exists(BaseDirectory))
            {
                Directory.Delete(BaseDirectory, true);
            }
        }

        internal static void Store(BuildResult result, IBuildArtifact[] artifacts) => SetArtifactData(result, artifacts);

        internal static string GetArtifactPath(BuildConfiguration config) => GetArtifactsPath(GetBuildConfigurationName(config));

        static string GetBuildConfigurationName(BuildConfiguration config)
        {
            var name = config.name;
            if (string.IsNullOrEmpty(name))
            {
                name = GlobalObjectId.GetGlobalObjectIdSlow(config).ToString();
            }
            return name;
        }

        static string GetArtifactsPath(string name) => Path.Combine(BaseDirectory, name + ".json").ToForwardSlash();

        static ArtifactData GetArtifactData(BuildConfiguration config)
        {
            if (config == null)
            {
                return null;
            }

            var name = GetBuildConfigurationName(config);
            var assetPath = GetArtifactsPath(name);
            if (!File.Exists(assetPath))
            {
                if (s_ArtifactDataCache.ContainsKey(name))
                {
                    s_ArtifactDataCache.Remove(name);
                }
                return null;
            }

            if (!s_ArtifactDataCache.TryGetValue(name, out var artifactData))
            {
                try
                {
                    artifactData = new ArtifactData();
                    JsonSerialization.TryFromJsonOverride(new FileInfo(assetPath), ref artifactData, out var result, new JsonSerializationParameters
                    {
                        DisableRootAdapters = true,
                        SerializedType = typeof(ArtifactData)
                    });

                    if (!result.DidSucceed())
                    {
                        var errors = result.Events.Select(e => e.ToString());
                        LogDeserializeError(string.Join("\n", errors), artifactData, assetPath);
                        artifactData = null;
                    }
                }
                catch (Exception e)
                {
                    LogDeserializeError(e.Message, artifactData, assetPath);
                    artifactData = null;
                }

                s_ArtifactDataCache.Add(name, artifactData);
            }

            return artifactData;
        }

        static void LogDeserializeError(string message, ArtifactData container, string assetPath)
        {
            var what = !string.IsNullOrEmpty(assetPath) ? assetPath.ToHyperLink() : $"memory container of type '{container.GetType().FullName}'";
            Debug.LogError($"Failed to deserialize {what}:\n{message}");
        }

        static void SetArtifactData(BuildResult result, IBuildArtifact[] artifacts)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.BuildConfiguration == null)
            {
                throw new ArgumentNullException(nameof(result.BuildConfiguration));
            }

            if (artifacts == null)
            {
                throw new ArgumentNullException(nameof(artifacts));
            }

            var name = GetBuildConfigurationName(result.BuildConfiguration);
            if (!s_ArtifactDataCache.TryGetValue(name, out var artifactData) || artifactData == null)
            {
                artifactData = new ArtifactData();
                s_ArtifactDataCache.Add(name, artifactData);
            }

            artifactData.Result = result;
            artifactData.Artifacts = artifacts.ToList();

            var assetPath = GetArtifactsPath(name);
            var file = new FileInfo(assetPath);
            file.WriteAllText(JsonSerialization.ToJson(artifactData, new JsonSerializationParameters
            {
                DisableRootAdapters = true,
                SerializedType = typeof(ArtifactData)
            }));
        }
    }
}

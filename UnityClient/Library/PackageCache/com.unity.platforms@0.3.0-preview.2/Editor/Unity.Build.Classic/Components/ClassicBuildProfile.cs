using System;
using System.ComponentModel;
using Unity.BuildSystem.NativeProgramSupport;
using Unity.Properties;
using Unity.Serialization;
using Unity.Serialization.Json;
using Unity.Serialization.Json.Adapters;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Classic
{
    [FormerName("Unity.Build.Common.ClassicBuildProfile, Unity.Build.Common")]
    public sealed class ClassicBuildProfile : IBuildPipelineComponent
    {
        Platform m_Platform;
        BuildPipelineBase m_Pipeline;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Target has been replaced with Platform. (RemovedAfter 2020-07-01)", true)]
        public BuildTarget Target
        {
            get => throw null;
            set => throw null;
        }

        [CreateProperty]
        internal Platform Platform
        {
            get => m_Platform;
            set
            {
                if (value == null)
                {
                    m_Platform = null;
                    m_Pipeline = null;
                }
                else if (value.GetType() != m_Platform?.GetType())
                {
                    m_Platform = value;
                    m_Pipeline = ConstructPipeline(m_Platform);
                }
            }
        }

        /// <summary>
        /// Gets or sets which <see cref="BuildType"/> this profile is going to use for the build.
        /// </summary>
        [CreateProperty]
        public BuildType Configuration { get; set; } = BuildType.Develop;

        public BuildPipelineBase Pipeline
        {
            get => m_Pipeline;
            set => throw new InvalidOperationException($"Cannot explicitly set {nameof(Pipeline)}, set {nameof(Platform)} property instead.");
        }

        public int SortingIndex => throw new NotImplementedException();

        public bool SetupEnvironment() => throw new NotImplementedException();

        public ClassicBuildProfile()
        {
#if UNITY_EDITOR_WIN
            m_Platform = new WindowsPlatform();
#elif UNITY_EDITOR_OSX
            m_Platform = new MacOSXPlatform();
#elif UNITY_EDITOR_LINUX
            m_Platform = new LinuxPlatform();
#endif
            m_Pipeline = ConstructPipeline(m_Platform);
        }

        internal static string GetExecutableExtension(BuildTarget target)
        {
#pragma warning disable 618
            switch (target)
            {
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                    return ".app";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
                case BuildTarget.NoTarget:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.Stadia:
                    return string.Empty;
                case BuildTarget.Android:
                    if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                        return "";
                    else if (EditorUserBuildSettings.buildAppBundle)
                        return ".aab";
                    else
                        return ".apk";
                case BuildTarget.Lumin:
                    return ".mpk";
                case BuildTarget.iOS:
                case BuildTarget.tvOS:
                default:
                    return "";
            }
#pragma warning restore 618
        }

        static Platform ConstructPlatform(BuildTarget target)
        {
#pragma warning disable 618
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return new WindowsPlatform();
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                    return new MacOSXPlatform();
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    return new LinuxPlatform();
                case BuildTarget.WSAPlayer:
                    return new UniversalWindowsPlatform();
                case BuildTarget.Android:
                    return new AndroidPlatform();
                case BuildTarget.iOS:
                    return new IosPlatform();
                case BuildTarget.tvOS:
                    return new TvosPlatform();
                case BuildTarget.WebGL:
                    return new WebGLPlatform();
                case BuildTarget.PS4:
                    return new PS4Platform();
                case BuildTarget.XboxOne:
                    return new XboxOnePlatform();
                case BuildTarget.Switch:
                    return new SwitchPlatform();
                default:
                    throw new NotImplementedException($"Could not map {nameof(BuildTarget)} '{target.ToString()}' to a known {nameof(Platform)}.");
            }
#pragma warning restore 618
        }

        static BuildPipelineBase ConstructPipeline(Platform platform)
        {
            if (platform == null)
            {
                return null;
            }

            return TypeCacheHelper.ConstructTypeDerivedFrom<BuildPipelineSelectorBase>().SelectFor(platform);
        }

        class ClassicBuildProfileMigration : IJsonMigration<ClassicBuildProfile>
        {
            [InitializeOnLoadMethod]
            static void Register() => JsonSerialization.AddGlobalMigration(new ClassicBuildProfileMigration());

            public int Version => 1;

            public ClassicBuildProfile Migrate(JsonMigrationContext context)
            {
                context.TryRead<ClassicBuildProfile>(out var profile);
                if (context.SerializedVersion == 0)
                {
                    if (context.TryRead<BuildTarget>("Target", out var target))
                    {
                        if (target != BuildTarget.NoTarget)
                        {
                            profile.Platform = ConstructPlatform(target);
                        }
                    }

                    if (context.TryRead<string>("Pipeline", out var pipeline))
                    {
                        if (GlobalObjectId.TryParse(pipeline, out var id))
                        {
                            var assetGuid = id.assetGUID.ToString();
                            CheckForCustomBuildPipeline(AssetDatabase.GUIDToAssetPath(assetGuid));
                            CheckForHybridLiveLinkBuildPipeline(assetGuid);
                        }
                    }
                }
                return profile;
            }

            void CheckForCustomBuildPipeline(string assetPath)
            {
                if (string.IsNullOrEmpty(assetPath))
                {
                    return;
                }

                try
                {
                    using (var reader = new SerializedObjectReader(assetPath))
                    {
                        var root = reader.ReadObject();
                        if (ContainsCustomBuildSteps(root) || ContainsCustomRunStep(root))
                        {
                            var config = string.Empty;
                            if (!string.IsNullOrEmpty(BuildConfiguration.CurrentDeserializationAssetPath))
                            {
                                config = BuildConfiguration.CurrentDeserializationAssetPath.ToHyperLink();
                            }
                            Debug.LogWarning($"{config} uses custom build pipeline {assetPath.ToHyperLink()}. Custom build pipelines are no longer supported.\n" +
                                $"Several options can be set using build components (like {nameof(PlayerConnectionSettings)}, {nameof(EnableHeadlessMode)}, etc).");
                        }
                    }
                }
                catch
                {
                }
            }

            bool ContainsCustomBuildSteps(SerializedObjectView view)
            {
                if (!view.TryGetMember("BuildSteps", out var member))
                {
                    return false;
                }

                var valueView = member.Value();
                if (valueView.Type != TokenType.Array)
                {
                    return false;
                }

                var arrayView = valueView.AsArrayView();
                foreach (var value in arrayView)
                {
                    if (value.Type != TokenType.String)
                    {
                        continue;
                    }

                    if (ContainsCustomType(value.AsStringView().ToString()))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool ContainsCustomRunStep(SerializedObjectView view)
            {
                if (!view.TryGetMember("RunStep", out var member))
                {
                    return false;
                }

                var valueView = member.Value();
                if (valueView.Type != TokenType.String)
                {
                    return false;
                }

                return ContainsCustomType(valueView.AsStringView().ToString());
            }

            bool ContainsCustomType(string assemblyQualifiedTypeName)
            {
                var parts = assemblyQualifiedTypeName.Split(',');
                foreach (var part in parts)
                {
                    if (!part.Trim(' ').StartsWith("Unity."))
                    {
                        return true;
                    }
                }
                return false;
            }

            void CheckForHybridLiveLinkBuildPipeline(string assetGuid)
            {
                if (string.IsNullOrEmpty(assetGuid))
                {
                    return;
                }

                var migration = TypeCacheHelper.ConstructTypeDerivedFrom<HybridBuildPipelineMigrationBase>();
                if (migration != null)
                {
                    migration.Migrate(BuildConfiguration.CurrentDeserializationAsset, assetGuid);
                }
            }
        }
    }

    /// <summary>
    /// For internal use only.
    /// </summary>
    internal abstract class BuildPipelineSelectorBase
    {
        public abstract BuildPipelineBase SelectFor(Platform platform);
    }

    /// <summary>
    /// For internal use only.
    /// </summary>
    internal abstract class HybridBuildPipelineMigrationBase
    {
        public abstract void Migrate(BuildConfiguration config, string assetGuid);
    }
}

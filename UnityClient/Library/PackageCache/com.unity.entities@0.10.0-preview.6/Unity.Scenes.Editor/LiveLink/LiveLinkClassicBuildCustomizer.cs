using System;
using System.IO;
using System.Linq;
using Unity.Build.Classic;
using Unity.Scenes.Editor.Build;
using UnityEditor;

namespace Unity.Scenes.Editor
{
    class LiveLinkClassicBuildCustomizer : ClassicBuildPipelineCustomizer
    {
        public override Type[] UsedComponents { get; } =
        {
            typeof(LiveLink)
        };

        public override void RegisterAdditionalFilesToDeploy(Action<string, string> registerAdditionalFileToDeploy)
        {
            if (!Context.HasComponent<LiveLink>())
                return;

            var tempFile = Path.Combine(WorkingDirectory, LiveLinkUtility.LiveLinkBootstrapFileName);
            LiveLinkUtility.WriteBootstrap(tempFile, new GUID(Context.BuildConfigurationAssetGUID));
            registerAdditionalFileToDeploy(tempFile, Path.Combine(StreamingAssetsDirectory, LiveLinkUtility.LiveLinkBootstrapFileName));
        }

        const string k_EmptyScenePath = "Packages/com.unity.entities/Unity.Scenes.Editor/LiveLink/Assets/empty.unity";

        public override string[] ModifyEmbeddedScenes(string[] scenes)
        {
            if (!Context.HasComponent<LiveLink>())
                return scenes;

            var nonLiveLinkable = scenes.Where(s => !SceneImporterData.CanLiveLinkScene(s)).ToArray();

            if (nonLiveLinkable.Length > 0)
                return nonLiveLinkable;

            return new[] { k_EmptyScenePath};
        }

        public override BuildOptions ProvideBuildOptions()
        {
            if (!Context.HasComponent<LiveLink>())
                return BuildOptions.None;
            return BuildTarget == BuildTarget.Android
                ? BuildOptions.WaitForPlayerConnection
                : BuildOptions.ConnectToHost;
        }
    }
}

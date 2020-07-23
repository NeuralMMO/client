using System;
using Unity.Build.Classic;
using Unity.Build.Common;

namespace Unity.Scenes.Editor
{
    class SubSceneFilesProvider : ClassicBuildPipelineCustomizer
    {
        public override Type[] UsedComponents { get; } =
        {
            typeof(SceneList),
            typeof(ClassicBuildProfile)
        };

        public override void RegisterAdditionalFilesToDeploy(Action<string, string> registerAdditionalFileToDeploy)
        {
            SubSceneBuildCode.PrepareAdditionalFiles(
                Context.BuildConfigurationAssetGUID,
                EmbeddedScenes,
                BuildTarget,
                registerAdditionalFileToDeploy,
                StreamingAssetsDirectory,
                $"Library/SubsceneBundles");

            var sceneList = Context.GetComponentOrDefault<SceneList>();
            var tempFile = System.IO.Path.Combine(WorkingDirectory, SceneSystem.k_SceneInfoFileName);
            ResourceCatalogBuildCode.WriteCatalogFile(sceneList, tempFile);
            registerAdditionalFileToDeploy(tempFile, System.IO.Path.Combine(StreamingAssetsDirectory, SceneSystem.k_SceneInfoFileName));
        }
    }
}

using System;
using System.IO;
using Unity.Build.Common;
using UnityEditor;

namespace Unity.Build.Classic.Private
{
    sealed class BuildPlayerStep : BuildStepBase
    {
        public override Type[] UsedComponents { get; } =
        {
            typeof(ClassicBuildProfile),
            typeof(SceneList),
            typeof(GeneralSettings),
            typeof(OutputBuildDirectory)
        };

        public override BuildResult Run(BuildContext context)
        {
            var classicSharedData = context.GetValue<ClassicSharedData>();
            var target = classicSharedData.BuildTarget;
            if (target <= 0)
                return context.Failure($"Invalid build target '{target.ToString()}'.");
            if (target != EditorUserBuildSettings.activeBuildTarget)
                return context.Failure($"{nameof(EditorUserBuildSettings.activeBuildTarget)} must be switched before {nameof(BuildPlayerStep)} step.");

            var embeddedScenes = context.GetValue<EmbeddedScenesValue>().Scenes;
            if (embeddedScenes.Length == 0)
                return context.Failure("There are no scenes to build.");

            var outputPath = context.GetOutputBuildDirectory();
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            string locationPathName;
            if (context.HasValue<LocationInfo>())
            {
                locationPathName = context.GetValue<LocationInfo>().Path;
            }
            else
            {
                var generalSettings = context.GetComponentOrDefault<GeneralSettings>();
                locationPathName = Path.Combine(outputPath, generalSettings.ProductName + ClassicBuildProfile.GetExecutableExtension(target));
            }

            var buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = embeddedScenes,
                target = target,
                locationPathName = locationPathName,
                targetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(target),
            };

            buildPlayerOptions.options = BuildOptions.None;

            foreach (var customizer in classicSharedData.Customizers)
                buildPlayerOptions.options |= customizer.ProvideBuildOptions();

            var report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
            context.SetValue(report);

            return context.FromReport(report);
        }
    }
}

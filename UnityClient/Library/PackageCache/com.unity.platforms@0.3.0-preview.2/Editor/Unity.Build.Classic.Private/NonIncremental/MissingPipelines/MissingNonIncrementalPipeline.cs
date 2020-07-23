using NiceIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Build.Common;
using Unity.Build.Editor;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Unity.Build.Classic.Private
{
    sealed class CopyAdditionallyProvidedFilesStepBeforeBuild : BuildStepBase
    {
        class ProviderInfo
        {
            public List<NPath> Paths = new List<NPath>();
        }

        public override BuildResult Run(BuildContext context)
        {
            var info = new ProviderInfo();

            var classicSharedData = context.GetValue<ClassicSharedData>();

            var oldStreamingAssetsDirectory = classicSharedData.StreamingAssetsDirectory;
            classicSharedData.StreamingAssetsDirectory = "Assets/StreamingAssets";

            foreach (var customizer in classicSharedData.Customizers)
            {
                customizer.RegisterAdditionalFilesToDeploy((from, to) =>
                {
                    var toNPath = new NPath(to).MakeAbsolute();
                    new NPath(from).MakeAbsolute().Copy(toNPath.EnsureParentDirectoryExists());
                    info.Paths.Add(toNPath);
                });
            }
            classicSharedData.StreamingAssetsDirectory = oldStreamingAssetsDirectory;
            context.SetValue(info);
            return context.Success();
        }

        public override BuildResult Cleanup(BuildContext context)
        {
            var info = context.GetValue<ProviderInfo>();
            foreach (var f in info.Paths)
            {
                f.DeleteIfExists();
            }
            return context.Success();
        }
    }


    /// <summary>
    /// Base class for classic non incremental pipelines which don't have implementation.
    /// This class is also responsible for informing <see cref="BuildPlayerStep"/> that BuildOptions.AutoRunPlayer should be used
    /// in case Build & Run is used, since pipeline doesn't implement its BuildStepBase.Run method.
    /// </summary>
    abstract class MissingNonIncrementalPipeline : ClassicNonIncrementalPipelineBase
    {
        public override BuildStepCollection BuildSteps { get; } = new[]
        {
            typeof(SaveScenesAndAssetsStep),
            typeof(ApplyUnitySettingsStep),
            typeof(SwitchPlatfomStep),
            typeof(CopyAdditionallyProvidedFilesStepBeforeBuild),   // Tricks providers into copying files into "Assets/StreamingAssets" thus make BuildPipeline.BuildPlayer to pick them up
            typeof(BuildPlayerStep),
            // Note: Don't add any steps below, since if you do Build & Run, the Run will happen during BuildPlayerStep without steps below executed
            //       If you really need to do this switch to OnBuildSplit(context); in OnBuild function below, it uses OnPostprocessBuild trick to execute remaining steps
            //typeof(CopyAdditionallyProvidedFilesStep)
        };

        private bool UsingBuildAndRun => BuildConfigurationScriptedImporterEditor.CurrentBuildAction.Equals(BuildConfigurationScriptedImporterEditor.s_BuildAndRunAction);

        protected override void PrepareContext(BuildContext context)
        {
            base.PrepareContext(context);
            var classicData = context.GetValue<ClassicSharedData>();
            var outDir = context.GetOutputBuildDirectory();
            var dataDir = $"{context.GetComponentOrDefault<GeneralSettings>().ProductName}_Data";
            classicData.StreamingAssetsDirectory = Path.Combine(outDir, dataDir, "StreamingAssets");

            // Enable AutoRunPlayer only if "Build and Run" is selected
            if (UsingBuildAndRun)
            {
                context.SetComponent<AutoRunPlayer>();
            }
        }

        protected override RunResult OnRun(RunContext context)
        {
            if (UsingBuildAndRun)
                return context.Success();
            throw new NotSupportedException("This build pipeline doesn't support Run. Only Build&Run is supported.");
        }

        protected override BuildResult OnBuild(BuildContext context)
        {
            PrepareContext(context);

            //return OnBuildSplit(context);
            return BuildSteps.Run(context);
        }

        private static BuildStepCollection s_CachedPostProcessCollection;
        private static BuildContext s_CachedContext;

        /// <summary>
        /// When we build using BuildPlayerStep with BuildOptions.AutoRunPlayer we run the player before CopyAdditionallyProvidedFilesStep is executed
        /// Sometimes you'll get lucky, since while the player is launching, CopyAdditionallyProvidedFilesStep will be executed in time
        /// And subscenes will be found...But in other cases, the player will launch without subscenes
        /// By using OnPostprocessBuild, we ensure all our steps are executed before running the player.
        /// The code is not pretty, but it's also temporary until we implement missing pipelines with their respective Run functions
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private BuildResult OnBuildSplit(BuildContext context)
        {
            var normalSteps = new List<Type>();
            var postProcessSteps = new List<Type>();
            bool buildPlayerStepEncountered = false;
            foreach (var b in BuildSteps)
            {
                if (buildPlayerStepEncountered)
                    postProcessSteps.Add(b.GetType());
                else
                    normalSteps.Add(b.GetType());

                if (b.GetType() == typeof(BuildPlayerStep))
                    buildPlayerStepEncountered = true;
            }

            var normalStepsCollection = new BuildStepCollection(normalSteps.ToArray());
            s_CachedPostProcessCollection = new BuildStepCollection(postProcessSteps.ToArray());
            s_CachedContext = context;

            var result = normalStepsCollection.Run(context);
            s_CachedPostProcessCollection = null;
            s_CachedContext = null;

            return result;
        }

        [PostProcessBuildAttribute()]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (s_CachedPostProcessCollection == null || s_CachedContext == null)
                return;
            if (s_CachedPostProcessCollection.Count() == 0)
                return;
            var result = s_CachedPostProcessCollection.Run(s_CachedContext);
            if (result.Failed)
                throw new Exception(result.ToString());
        }

    }
}

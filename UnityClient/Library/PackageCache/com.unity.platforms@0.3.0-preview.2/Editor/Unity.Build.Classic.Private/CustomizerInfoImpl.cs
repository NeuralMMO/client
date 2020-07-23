using NiceIO;
using UnityEditor;

namespace Unity.Build.Classic.Private
{
    class CustomizerInfoImpl : ClassicBuildPipelineCustomizer.Info
    {
        public CustomizerInfoImpl(BuildContext context)
        {
            Context = context;
            new NPath(WorkingDirectory).MakeAbsolute().EnsureDirectoryExists();
        }

        public override BuildContext Context { get; }
        public override string StreamingAssetsDirectory => Context.GetValue<ClassicSharedData>().StreamingAssetsDirectory;
        public override string OutputBuildDirectory => Context.GetValue<ClassicSharedData>().OutputBuildDirectory;
        public override string WorkingDirectory => $"Library/BuildWorkingDir/{Context.BuildConfigurationName}";
        public override BuildTarget BuildTarget => Context.GetValue<ClassicSharedData>().BuildTarget;
        public override string[] EmbeddedScenes => Context.GetValue<EmbeddedScenesValue>().Scenes;
    }
}

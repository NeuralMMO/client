using System;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("BuildSettings has been renamed to BuildConfiguration. (RemovedAfter 2020-05-01)")]
    public sealed class BuildSettings : HierarchicalComponentContainer<BuildSettings, IBuildSettingsComponent>
    {
        public const string AssetExtension = ".buildsettings";
        public BuildPipeline GetBuildPipeline() => throw null;
        public bool CanBuild(out string reason) => throw null;
        public BuildPipelineResult Build() => throw null;
        public bool CanRun(out string reason) => throw null;
        public RunStepResult Run() => throw null;
    }
}

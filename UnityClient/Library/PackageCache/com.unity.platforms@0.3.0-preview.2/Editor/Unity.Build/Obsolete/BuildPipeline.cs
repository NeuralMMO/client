using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Replace with BuildPipelineBase. (RemovedAfter 2020-07-01)", true)]
    public sealed class BuildPipeline : ScriptableObjectPropertyContainer<BuildPipeline>, IBuildStep
    {
        public List<IBuildStep> BuildSteps;
        public RunStep RunStep;
        public const string AssetExtension = ".buildpipeline";
        public bool CanBuild(BuildConfiguration config, out string reason) => throw null;
        public BuildPipelineResult Build(BuildConfiguration config, BuildProgress progress = null, Action<BuildContext> mutator = null) => throw null;
        public bool CanRun(BuildConfiguration config, out string reason) => throw null;
        public RunStepResult Run(BuildConfiguration config) => throw null;
    }
}

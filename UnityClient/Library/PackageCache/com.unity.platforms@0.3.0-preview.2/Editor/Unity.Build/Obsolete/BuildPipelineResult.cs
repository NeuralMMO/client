using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Replace with BuildResult. (RemovedAfter 2020-07-01)", true)]
    public sealed class BuildPipelineResult : ResultBase
    {
        public List<BuildStepResult> BuildStepsResults { get => throw null; internal set => throw null; }
        public bool TryGetBuildStepResult(IBuildStep buildStep, out BuildStepResult value) => throw null;
        public static implicit operator bool(BuildPipelineResult result) => throw null;
        public static BuildPipelineResult Success(BuildPipeline pipeline, BuildConfiguration config) => throw null;
        public static BuildPipelineResult Failure(BuildPipeline pipeline, BuildConfiguration config, string message) => throw null;
        public static BuildPipelineResult Failure(BuildPipeline pipeline, BuildConfiguration config, Exception exception) => throw null;
        public BuildPipelineResult() => throw null;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("BuildSettings has been renamed to BuildConfiguration. (RemovedAfter 2020-05-01) (UnityUpgradable) -> BuildConfiguration")]
        public BuildConfiguration BuildSettings { get; internal set; }
    }
}

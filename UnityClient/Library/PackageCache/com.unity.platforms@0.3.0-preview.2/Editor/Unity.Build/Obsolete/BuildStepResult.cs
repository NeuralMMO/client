using System;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Replace with BuildResult. (RemovedAfter 2020-07-01)", true)]
    public sealed class BuildStepResult : ResultBase
    {
        public BuildStep BuildStep { get => throw null; internal set => throw null; }
        public string Description => throw null;
        public static implicit operator bool(BuildStepResult result) => throw null;
        public BuildStepResult(BuildStep step, UnityEditor.Build.Reporting.BuildReport report) => throw null;
        public static BuildStepResult Success(BuildStep step) => throw null;
        public static BuildStepResult Failure(BuildStep step, string message) => throw null;
        public static BuildStepResult Failure(BuildStep step, Exception exception) => throw null;
        public BuildStepResult() => throw null;
    }
}

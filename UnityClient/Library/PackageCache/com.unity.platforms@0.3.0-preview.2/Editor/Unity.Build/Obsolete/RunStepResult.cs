using System;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Replace with RunResult. (RemovedAfter 2020-07-01)", true)]
    public sealed class RunStepResult : ResultBase, IDisposable
    {
        public RunStep RunStep { get => throw null; internal set => throw null; }
        public IRunInstance RunInstance { get => throw null; internal set => throw null; }
        public static implicit operator bool(RunStepResult result) => throw null;
        public static RunStepResult Success(BuildConfiguration config, RunStep step, IRunInstance instance) => throw null;
        public static RunStepResult Failure(BuildConfiguration config, RunStep step, string message) => throw null;
        public static RunStepResult Failure(BuildConfiguration config, RunStep step, Exception exception) => throw null;
        public void Dispose() => throw null;
        public RunStepResult() => throw null;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("BuildSettings has been renamed to BuildConfiguration. (RemovedAfter 2020-05-01) (UnityUpgradable) -> BuildConfiguration")]
        public BuildConfiguration BuildSettings { get; internal set; }
    }
}

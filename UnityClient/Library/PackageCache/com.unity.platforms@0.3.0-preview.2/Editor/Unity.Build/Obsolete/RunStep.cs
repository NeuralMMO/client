using System;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Remove usage. (RemovedAfter 2020-07-01)", true)]
    public abstract class RunStep
    {
        public string Name => throw null;
        public string Category => throw null;
        public bool IsShown => throw null;
        public virtual bool CanRun(BuildConfiguration config, out string reason) => throw null;
        public abstract RunStepResult Start(BuildConfiguration config);
        public RunStepResult Success(BuildConfiguration config, IRunInstance instance) => throw null;
        public RunStepResult Failure(BuildConfiguration config, string message) => throw null;
        public static string GetName(Type type) => throw null;
        public static string GetName<T>() where T : RunStep => throw null;
        public static string GetCategory(Type type) => throw null;
        public static string GetCategory<T>() where T : RunStep => throw null;
        public static bool GetIsShown(Type type) => throw null;
        public static bool GetIsShown<T>() where T : RunStep => throw null;
    }
}

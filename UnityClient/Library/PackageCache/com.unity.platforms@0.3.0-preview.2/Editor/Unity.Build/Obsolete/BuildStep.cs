using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Unity.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Replace with BuildStepBase. (RemovedAfter 2020-07-01)", true)]
    public abstract class BuildStep : IBuildStep
    {
        public string Name => throw null;
        public string Description => throw null;
        public string Category => throw null;
        public bool IsShown => throw null;
        public virtual Type[] RequiredComponents => throw null;
        public virtual Type[] OptionalComponents => throw null;
        public virtual bool IsEnabled(BuildContext context) => throw null;
        public abstract BuildStepResult RunBuildStep(BuildContext context);
        public virtual BuildStepResult CleanupBuildStep(BuildContext context) => throw null;
        public bool HasRequiredComponent(BuildContext context, Type type) => throw null;
        public bool HasRequiredComponent<T>(BuildContext context) where T : IBuildComponent => throw null;
        public IBuildComponent GetRequiredComponent(BuildContext context, Type type) => throw null;
        public T GetRequiredComponent<T>(BuildContext context) where T : IBuildComponent => throw null;
        public IEnumerable<IBuildComponent> GetRequiredComponents(BuildContext context) => throw null;
        public IEnumerable<IBuildComponent> GetRequiredComponents(BuildContext context, Type type) => throw null;
        public IEnumerable<T> GetRequiredComponents<T>(BuildContext context) where T : IBuildComponent => throw null;
        public bool HasOptionalComponent(BuildContext context, Type type) => throw null;
        public bool HasOptionalComponent<T>(BuildContext context) where T : IBuildComponent => throw null;
        public IBuildComponent GetOptionalComponent(BuildContext context, Type type) => throw null;
        public T GetOptionalComponent<T>(BuildContext context) where T : IBuildComponent => throw null;
        public IEnumerable<IBuildComponent> GetOptionalComponents(BuildContext context) => throw null;
        public IEnumerable<IBuildComponent> GetOptionalComponents(BuildContext context, Type type) => throw null;
        public IEnumerable<T> GetOptionalComponents<T>(BuildContext context) => throw null;
        public BuildStepResult Success() => throw null;
        public BuildStepResult Failure(string message) => throw null;
        public static string GetName(Type type) => throw null;
        public static string GetName<T>() where T : IBuildStep => throw null;
        public static string GetDescription(Type type) => throw null;
        public static string GetDescription<T>() where T : IBuildStep => throw null;
        public static string GetCategory(Type type) => throw null;
        public static string GetCategory<T>() where T : IBuildStep => throw null;
        public static bool GetIsShown(Type type) => throw null;
        public static bool GetIsShown<T>() where T : IBuildStep => throw null;
    }
}

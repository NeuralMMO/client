using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Unity.Build.Classic.Private
{
    static class BuildContextExtensions
    {
        /// <summary>
        /// Create a new <see cref="BuildResult"/> from a <see cref="BuildReport"/> object.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="report">The build report.</param>
        /// <returns>A new <see cref="BuildResult"/> instance created from the <see cref="BuildReport"/> object.</returns>
        public static BuildResult FromReport(this BuildContext context, BuildReport report) => new BuildResult
        {
            Succeeded = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded,
            BuildPipeline = context.BuildPipeline,
            BuildConfiguration = context.BuildConfiguration,
            Message = report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded ? report.summary.ToString() : null
        };

        public static bool UsesIL2CPP(this BuildContext context) => context.TryGetComponent(out Unity.Build.Classic.ClassicScriptingSettings css) && css.ScriptingBackend == ScriptingImplementation.IL2CPP;
        public static bool IsDevelopmentBuild(this BuildContext context) => context.TryGetComponent(out Unity.Build.Classic.ClassicBuildProfile classicBuildProfile) && classicBuildProfile.Configuration == BuildType.Develop;
    }
}

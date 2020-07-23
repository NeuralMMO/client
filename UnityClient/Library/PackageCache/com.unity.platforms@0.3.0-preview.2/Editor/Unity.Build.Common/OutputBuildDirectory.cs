using System;
using System.ComponentModel;
using Unity.Serialization;

namespace Unity.Build.Common
{
    /// <summary>
    /// Overrides the default output directory of Builds/BuildConfiguration.name to an arbitrary location. 
    /// </summary>
    [FormerName("Unity.Build.Common.OutputBuildDirectory, Unity.Build.Common")]
    public class OutputBuildDirectory : IBuildComponent
    {
        public string OutputDirectory;
    }

    public static class BuildConfigurationExtensions
    {
        /// <summary>
        /// Get the output build directory override for this build configuration.
        /// The output build directory can be overridden using a <see cref="OutputBuildDirectory"/> component.
        /// </summary>
        /// <param name="config">This build config.</param>
        /// <returns>The output build directory.</returns>
        public static string GetOutputBuildDirectory(this BuildConfiguration config)
        {
            if (config.TryGetComponent<OutputBuildDirectory>(out var value))
            {
                return value.OutputDirectory;
            }
            return $"Builds/{config.name}";
        }
    }

    public static class BuildContextExtensions
    {
        /// <summary>
        /// Get the output build directory override used in this build context.
        /// The output build directory can be overridden using a <see cref="OutputBuildDirectory"/> component.
        /// </summary>
        /// <param name="step">This build step.</param>
        /// <param name="context">The build context used throughout this build.</param>
        /// <returns>The output build directory.</returns>
        public static string GetOutputBuildDirectory(this BuildContext context)
        {
            if (context.TryGetComponent<OutputBuildDirectory>(out var value))
            {
                return value.OutputDirectory;
            }
            return $"Builds/{context.BuildConfigurationName}";
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Remove usage. (RemovedAfter 2020-07-01)", true)]
    public static class BuildStepExtensions
    {
        public static string GetOutputBuildDirectory(this BuildStep step, BuildContext context) => throw null;
    }
}

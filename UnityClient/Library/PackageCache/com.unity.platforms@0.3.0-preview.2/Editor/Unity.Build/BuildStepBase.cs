using System;
using System.Reflection;

namespace Unity.Build
{
    /// <summary>
    /// Base class for build steps.
    /// </summary>
    public abstract class BuildStepBase
    {
        /// <summary>
        /// Description of this build step displayed in build progress reporting.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// List of build components used by this build step.
        /// </summary>
        public virtual Type[] UsedComponents { get; } = Array.Empty<Type>();

        /// <summary>
        /// Default constructor for <see cref="BuildStepBase"/>.
        /// </summary>
        public BuildStepBase()
        {
            var type = GetType();
            Description = type.GetCustomAttribute<BuildStepAttribute>()?.Description ?? $"Running {type.Name}";
        }

        /// <summary>
        /// Determine if this build step will be executed or not.
        /// </summary>
        /// <returns><see langword="true"/> if enabled, otherwise <see langword="false"/>.</returns>
        public virtual bool IsEnabled(BuildContext context) => true;

        /// <summary>
        /// Run this build step. If a previous build step fails, this build step will not run.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <returns>The result of running this build step.</returns>
        public abstract BuildResult Run(BuildContext context);

        /// <summary>
        /// Cleanup this build step. Cleanup will only be called if this build step ran.
        /// </summary>
        /// <param name="context">Current build context.</param>
        /// <returns>The result of cleaning this build step.</returns>
        public virtual BuildResult Cleanup(BuildContext context) => context.Success();
    }
}

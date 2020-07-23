using System;

namespace Unity.Build
{
    /// <summary>
    /// Container for results happening when building a build pipeline.
    /// </summary>
    public sealed class BuildResult : ResultBase
    {
        /// <summary>
        /// Get a build result representing a success.
        /// </summary>
        /// <param name="pipeline">The build pipeline.</param>
        /// <param name="config">The build configuration.</param>
        /// <returns>A new build result instance.</returns>
        public static BuildResult Success(BuildPipelineBase pipeline, BuildConfiguration config) => new BuildResult
        {
            Succeeded = true,
            BuildPipeline = pipeline,
            BuildConfiguration = config
        };

        /// <summary>
        /// Get a build result representing a failure.
        /// </summary>
        /// <param name="pipeline">The build pipeline.</param>
        /// <param name="config">The build configuration.</param>
        /// <param name="reason">The reason of the failure.</param>
        /// <returns>A new build result instance.</returns>
        public static BuildResult Failure(BuildPipelineBase pipeline, BuildConfiguration config, string reason) => new BuildResult
        {
            Succeeded = false,
            BuildPipeline = pipeline,
            BuildConfiguration = config,
            Message = reason
        };

        /// <summary>
        /// Get a build result representing a failure.
        /// </summary>
        /// <param name="pipeline">The build pipeline.</param>
        /// <param name="config">The build configuration.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A new build result instance.</returns>
        public static BuildResult Failure(BuildPipelineBase pipeline, BuildConfiguration config, Exception exception) => new BuildResult
        {
            Succeeded = false,
            BuildPipeline = pipeline,
            BuildConfiguration = config,
            Exception = exception
        };

        public override string ToString() => $"Build {base.ToString()}";

        public BuildResult() { }
    }
}

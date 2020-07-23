using System;
using System.ComponentModel;

namespace Unity.Build
{
    /// <summary>
    /// Holds contextual information when building a build pipeline.
    /// </summary>
    public sealed class BuildContext : ContextBase
    {
        /// <summary>
        /// The build progress object used througout the build.
        /// </summary>
        public BuildProgress BuildProgress { get; }

        /// <summary>
        /// Quick access to build manifest value.
        /// </summary>
        public BuildManifest BuildManifest => GetOrCreateValue<BuildManifest>();

        /// <summary>
        /// Get a build result representing a success.
        /// </summary>
        /// <returns>A new build result instance.</returns>
        public BuildResult Success() => BuildResult.Success(BuildPipeline, BuildConfiguration);

        /// <summary>
        /// Get a build result representing a failure.
        /// </summary>
        /// <param name="reason">The reason of the failure.</param>
        /// <returns>A new build result instance.</returns>
        public BuildResult Failure(string reason) => BuildResult.Failure(BuildPipeline, BuildConfiguration, reason);

        /// <summary>
        /// Get a build result representing a failure.
        /// </summary>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A new build result instance.</returns>
        public BuildResult Failure(Exception exception) => BuildResult.Failure(BuildPipeline, BuildConfiguration, exception);

        internal BuildContext() : base() { }

        internal BuildContext(BuildPipelineBase pipeline, BuildConfiguration config, BuildProgress progress = null) :
            base(pipeline, config)
        {
            BuildProgress = progress;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Remove usage, since BuildPipelineResult is now obsolete. (RemovedAfter 2020-07-01)", true)]
        public BuildPipelineResult BuildPipelineStatus { get; }
    }
}

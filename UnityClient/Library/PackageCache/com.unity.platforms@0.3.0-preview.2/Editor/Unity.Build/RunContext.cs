using System;

namespace Unity.Build
{
    /// <summary>
    /// Holds contextual information when running a build pipeline.
    /// </summary>
    public sealed class RunContext : ContextBase
    {
        /// <summary>
        /// Get a run result representing a success.
        /// </summary>
        /// <param name="instance">The run process instance.</param>
        /// <returns>A new run result instance.</returns>
        public RunResult Success(IRunInstance instance = null) => RunResult.Success(BuildPipeline, BuildConfiguration, instance);

        /// <summary>
        /// Get a run result representing a failure.
        /// </summary>
        /// <param name="reason">The reason of the failure.</param>
        /// <returns>A new run result instance.</returns>
        public RunResult Failure(string reason) => RunResult.Failure(BuildPipeline, BuildConfiguration, reason);

        /// <summary>
        /// Get a run result representing a failure.
        /// </summary>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A new run result instance.</returns>
        public RunResult Failure(Exception exception) => RunResult.Failure(BuildPipeline, BuildConfiguration, exception);

        internal RunContext() : base() { }

        internal RunContext(BuildPipelineBase pipeline, BuildConfiguration config)
            : base(pipeline, config)
        {
        }
    }
}

using System;

namespace Unity.Build
{
    /// <summary>
    /// Container for results happening when running a build pipeline.
    /// </summary>
    public sealed class RunResult : ResultBase, IDisposable
    {
        /// <summary>
        /// The run process instance.
        /// </summary>
        public IRunInstance RunInstance { get; internal set; }

        /// <summary>
        /// Get a run result representing a success.
        /// </summary>
        /// <param name="pipeline">The build pipeline.</param>
        /// <param name="config">The build configuration.</param>
        /// <param name="instance">The run process instance.</param>
        /// <returns>A new run result instance.</returns>
        public static RunResult Success(BuildPipelineBase pipeline, BuildConfiguration config, IRunInstance instance = null) => new RunResult
        {
            Succeeded = true,
            BuildPipeline = pipeline,
            BuildConfiguration = config,
            RunInstance = instance
        };

        /// <summary>
        /// Get a run result representing a failure.
        /// </summary>
        /// <param name="pipeline">The build pipeline.</param>
        /// <param name="config">The build configuration.</param>
        /// <param name="reason">The reason of the failure.</param>
        /// <returns>A new run result instance.</returns>
        public static RunResult Failure(BuildPipelineBase pipeline, BuildConfiguration config, string reason) => new RunResult
        {
            Succeeded = false,
            BuildPipeline = pipeline,
            BuildConfiguration = config,
            Message = reason
        };

        /// <summary>
        /// Get a run result representing a failure.
        /// </summary>
        /// <param name="pipeline">The build pipeline.</param>
        /// <param name="config">The build configuration.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A new run result instance.</returns>
        public static RunResult Failure(BuildPipelineBase pipeline, BuildConfiguration config, Exception exception) => new RunResult
        {
            Succeeded = false,
            BuildPipeline = pipeline,
            BuildConfiguration = config,
            Exception = exception
        };

        public override string ToString() => $"Run {base.ToString()}";

        public void Dispose()
        {
            if (RunInstance != null)
            {
                RunInstance.Dispose();
            }
        }

        public RunResult() { }
    }
}

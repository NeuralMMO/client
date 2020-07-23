using System;
using System.Diagnostics;
using System.Linq;
using UnityEditor;

namespace Unity.Build
{
    public abstract class BuildPipelineBase
    {
        /// <summary>
        /// Optional list of build steps used by this build pipeline.
        /// </summary>
        public virtual BuildStepCollection BuildSteps { get; } = new BuildStepCollection();

        /// <summary>
        /// List of build component types used by this build pipeline.
        /// </summary>
        public virtual Type[] UsedComponents => BuildSteps.SelectMany(step => step.UsedComponents).Distinct().ToArray();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BuildPipelineBase() { }

        /// <summary>
        /// Construct build pipeline from build step collection.
        /// </summary>
        /// <param name="steps">List of build steps.</param>
        public BuildPipelineBase(BuildStepCollection steps)
        {
            BuildSteps = steps;
        }

        /// <summary>
        /// Determine if the build pipeline satisfy requirements to build.
        /// </summary>
        /// <param name="config">The build configuration to be used by this build pipeline.</param>
        /// <returns>A result describing if the pipeline can build or not.</returns>
        public BoolResult CanBuild(BuildConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            using (var context = new BuildContext(this, config))
            {
                return CanBuild(context);
            }
        }

        /// <summary>
        /// Build this build pipeline using the specified build configuration.
        /// </summary>
        /// <param name="config">The build configuration to be used by this build pipeline.</param>
        /// <param name="progress">Optional build progress report.</param>
        /// <returns>A result describing if build is successful or not.</returns>
        public BuildResult Build(BuildConfiguration config, BuildProgress progress = null)
        {
            using (var process = BuildIncremental(config, progress))
            {
                while (process.Update()) { }
                return process.Result;
            }
        }

        /// <summary>
        /// Start an incremental build of this build pipeline using the specified build configuration.
        /// The <see cref="BuildProcess.Update"/> method needs to be called until it returns <see langword="false"/>, indicating that the build has completed.
        /// The <see cref="BuildResult"/> can then be queried from the <see cref="BuildProcess.Result"/> property.
        /// </summary>
        /// <param name="config">The build configuration to be used by this build pipeline.</param>
        /// <param name="progress">Optional build progress report.</param>
        /// <returns>The build process.</returns>
        public BuildProcess BuildIncremental(BuildConfiguration config, BuildProgress progress = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (EditorApplication.isCompiling)
            {
                throw new InvalidOperationException("Building is not allowed while Unity is compiling.");
            }

            if (EditorUtility.scriptCompilationFailed)
            {
                throw new InvalidOperationException("Building is not allowed because scripts have compile errors in the editor.");
            }

            var context = new BuildContext(this, config, progress);
            var canBuild = CanBuild(context);
            if (!canBuild.Result)
            {
                return BuildProcess.Failure(this, config, canBuild.Reason);
            }

            return new BuildProcess(context, OnBuild);
        }

        /// <summary>
        /// Determine if the build pipeline satisfy requirements to run the last build.
        /// </summary>
        /// <param name="config">The build configuration corresponding to the build to be run.</param>
        /// <returns>A result describing if the pipeline can run or not.</returns>
        public BoolResult CanRun(BuildConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            using (var context = new RunContext(this, config))
            {
                return CanRun(context);
            }
        }

        /// <summary>
        /// Run the last build of this build pipeline corresponding to the specified build configuration.
        /// </summary>
        /// <param name="config">The build configuration corresponding to the build to be run.</param>
        /// <returns>A result describing if run is successful or not.</returns>
        public RunResult Run(BuildConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            RunResult result = null;
            try
            {
                using (var context = new RunContext(this, config))
                {
                    var canRun = CanRun(context);
                    if (!canRun.Result)
                    {
                        return RunResult.Failure(this, config, canRun.Reason);
                    }

                    var timer = Stopwatch.StartNew();
                    result = OnRun(context);
                    timer.Stop();

                    if (result != null)
                    {
                        result.Duration = timer.Elapsed;
                    }
                }
            }
            catch (Exception exception)
            {
                result = RunResult.Failure(this, config, exception);
            }
            return result;
        }

        /// <summary>
        /// Provides additional implementation to determine if the build pipeline satisfy requirements to build.
        /// </summary>
        /// <param name="context">The build context for the scope of the build operation.</param>
        /// <returns>A result describing if the pipeline can build or not.</returns>
        protected virtual BoolResult OnCanBuild(BuildContext context) => BoolResult.True();

        /// <summary>
        /// Provides implementation to build this build pipeline using the specified build configuration.
        /// When using <see cref="BuildIncremental"/>, this method is called repeatedly until a build result is returned.
        /// </summary>
        /// <param name="context">The build context for the scope of the build operation.</param>
        /// <returns>A result describing if build is successful or not.</returns>
        protected abstract BuildResult OnBuild(BuildContext context);

        /// <summary>
        /// Provides additional implementation to determine if the build pipeline satisfy requirements to run the last build.
        /// </summary>
        /// <param name="context">The run context for the scope of the run operation.</param>
        /// <returns>A result describing if the pipeline can run or not.</returns>
        protected virtual BoolResult OnCanRun(RunContext context) => BoolResult.True();

        /// <summary>
        /// Provides implementation to run the last build of this build pipeline corresponding to the specified build configuration.
        /// </summary>
        /// <param name="context">The run context for the scope of the run operation.</param>
        /// <returns>A result describing if run is successful or not.</returns>
        protected abstract RunResult OnRun(RunContext context);

        internal static void BuildAsync(BuildBatchDescription buildBatchDescription)
        {
            var buildEntities = buildBatchDescription.BuildItems;
            // ToDo: when running multiple builds, should we stop at first failure?
            var buildPipelineResults = new BuildResult[buildEntities.Length];

            for (int i = 0; i < buildEntities.Length; i++)
            {
                var config = buildEntities[i].BuildConfiguration;
                var canBuild = config.CanBuild();
                if (!canBuild.Result)
                {
                    buildPipelineResults[i] = BuildResult.Failure(config.GetBuildPipeline(), config, canBuild.Reason);
                }
                else
                {
                    buildPipelineResults[i] = null;
                }
            }

            var queue = BuildQueue.instance;
            for (int i = 0; i < buildEntities.Length; i++)
            {
                var config = buildEntities[i].BuildConfiguration;
                queue.QueueBuild(config, buildPipelineResults[i]);
            }

            queue.FlushBuilds(buildBatchDescription.OnBuildCompleted);
        }

        internal static void CancelBuildAsync()
        {
            BuildQueue.instance.Clear();
        }

        BoolResult CanBuild(BuildContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var type in UsedComponents)
            {
                BuildConfiguration.CheckComponentTypeAndThrowIfInvalid(type);
            }

            return OnCanBuild(context);
        }

        BoolResult CanRun(RunContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = BuildArtifacts.GetBuildResult(context.BuildConfiguration);
            if (result == null)
            {
                return BoolResult.False($"No build result found for {context.BuildConfiguration.ToHyperLink()}.");
            }

            if (result.Failed)
            {
                return BoolResult.False($"Last build failed with error:\n{result.Message}");
            }

            return OnCanRun(context);
        }
    }
}

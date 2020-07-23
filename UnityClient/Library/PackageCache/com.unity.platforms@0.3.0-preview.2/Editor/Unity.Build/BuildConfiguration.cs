using System;

namespace Unity.Build
{
    /// <summary>
    /// Can stores a set of hierarchical build components per type, which can be inherited or overridden using dependencies.
    /// </summary>
    public sealed class BuildConfiguration : HierarchicalComponentContainer<BuildConfiguration, IBuildComponent>
    {
        /// <summary>
        /// File extension for build configuration assets.
        /// </summary>
        public const string AssetExtension = ".buildconfiguration";

        /// <summary>
        /// Retrieve the build pipeline of this build configuration.
        /// </summary>
        /// <returns>The build pipeline if found, otherwise <see langword="null"/>.</returns>
        public BuildPipelineBase GetBuildPipeline() => TryGetComponent<IBuildPipelineComponent>(out var component) ? component.Pipeline : null;

        /// <summary>
        /// Determine if the build pipeline of this build configuration can build.
        /// </summary>
        /// <returns>A result describing if the pipeline can build or not.</returns>
        public BoolResult CanBuild()
        {
            var pipeline = GetBuildPipeline();
            var canBuild = CanBuildOrRun(pipeline);
            return canBuild.Result ? pipeline.CanBuild(this) : canBuild;
        }

        /// <summary>
        /// Run the build pipeline of this build configuration to build the target.
        /// </summary>
        /// <returns>The result of the build pipeline build.</returns>
        public BuildResult Build()
        {
            var pipeline = GetBuildPipeline();
            var canBuild = CanBuildOrRun(pipeline);
            if (!canBuild.Result)
            {
                return BuildResult.Failure(pipeline, this, canBuild.Reason);
            }

            var what = !string.IsNullOrEmpty(name) ? $" {name}" : string.Empty;
            using (var progress = new BuildProgress($"Building{what}", "Please wait..."))
            {
                return pipeline.Build(this, progress);
            }
        }

        /// <summary>
        /// Determine if the build pipeline of this build configuration can run.
        /// </summary>
        /// <returns>A result describing if the pipeline can run or not.</returns>
        public BoolResult CanRun()
        {
            var pipeline = GetBuildPipeline();
            var canRun = CanBuildOrRun(pipeline);
            return canRun.Result ? pipeline.CanRun(this) : canRun;
        }

        /// <summary>
        /// Run the resulting target from building the build pipeline of this build configuration.
        /// </summary>
        /// <returns></returns>
        public RunResult Run()
        {
            var pipeline = GetBuildPipeline();
            var canRun = CanBuildOrRun(pipeline);
            if (!canRun.Result)
            {
                return RunResult.Failure(pipeline, this, canRun.Reason);
            }
            return pipeline.Run(this);
        }

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <see cref="Type"/>.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <param name="type">The type of the build artifact.</param>
        /// <returns>The build artifact if found, otherwise <see langword="null"/>.</returns>
        public IBuildArtifact GetLastBuildArtifact(Type type) => BuildArtifacts.GetBuildArtifact(this, type);

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the build artifact.</typeparam>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build artifact if found, otherwise <see langword="null"/>.</returns>
        public T GetLastBuildArtifact<T>() where T : class, IBuildArtifact => BuildArtifacts.GetBuildArtifact<T>(this);

        /// <summary>
        /// Get the last build result for this build configuration.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build result if found, otherwise <see langword="null"/>.</returns>
        public BuildResult GetLastBuildResult() => BuildArtifacts.GetBuildResult(this);

        BoolResult CanBuildOrRun(BuildPipelineBase pipeline)
        {
            if (pipeline == null)
            {
                return BoolResult.False($"No valid build pipeline found for {this.ToHyperLink()}. At least one component that derives from {nameof(IBuildPipelineComponent)} must be present.");
            }
            return BoolResult.True();
        }
    }
}

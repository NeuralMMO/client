namespace Unity.Build
{
    /// <summary>
    /// Base interface for build configuration components that provides a build pipeline.
    /// </summary>
    public interface IBuildPipelineComponent : IBuildComponent
    {
        /// <summary>
        /// Build pipeline used by this build configuration.
        /// </summary>
        BuildPipelineBase Pipeline { get; set; }

        /// <summary>
        /// Returns index which is used for sorting builds when they're batch in build queue
        /// </summary>
        int SortingIndex { get; }

        /// <summary>
        /// Sets the editor environment before starting the build
        /// </summary>
        /// <returns>Returns true, when editor domain reload is required.</returns>
        bool SetupEnvironment();
    }
}

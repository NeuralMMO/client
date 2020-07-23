using Unity.BuildSystem.NativeProgramSupport;
using UnityEditor;

namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for Linux
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class LinuxClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new LinuxPlatform();

        protected override BuildTarget BuildTarget => BuildTarget.StandaloneLinux64;
    }
}

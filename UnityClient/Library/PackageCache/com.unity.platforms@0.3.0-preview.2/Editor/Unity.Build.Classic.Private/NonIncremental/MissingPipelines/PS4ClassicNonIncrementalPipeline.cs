using Unity.BuildSystem.NativeProgramSupport;
using UnityEditor;

namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for PS4
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class PS4ClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new PS4Platform();

        protected override BuildTarget BuildTarget => BuildTarget.PS4;
    }
}

using Unity.BuildSystem.NativeProgramSupport;
using UnityEditor;

namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for tvOS
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class tvOSClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new TvosPlatform();

        protected override BuildTarget BuildTarget => BuildTarget.tvOS;
    }
}

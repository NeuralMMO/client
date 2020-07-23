using Unity.BuildSystem.NativeProgramSupport;
using UnityEditor;

namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for iOS
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class iOSClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new IosPlatform();

        protected override BuildTarget BuildTarget => BuildTarget.iOS;
    }
}

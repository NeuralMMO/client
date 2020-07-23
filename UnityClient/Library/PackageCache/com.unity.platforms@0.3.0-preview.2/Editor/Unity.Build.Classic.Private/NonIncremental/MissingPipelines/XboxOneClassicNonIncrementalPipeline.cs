using Unity.BuildSystem.NativeProgramSupport;
using UnityEditor;


namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for XboxOne
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class XboxOneClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new XboxOnePlatform();

        protected override BuildTarget BuildTarget => BuildTarget.XboxOne;
    }
}

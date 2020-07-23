using Unity.BuildSystem.NativeProgramSupport;
using UnityEditor;

namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for Windows Store
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class WSAClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new UniversalWindowsPlatform();

        protected override BuildTarget BuildTarget => BuildTarget.WSAPlayer;
    }
}

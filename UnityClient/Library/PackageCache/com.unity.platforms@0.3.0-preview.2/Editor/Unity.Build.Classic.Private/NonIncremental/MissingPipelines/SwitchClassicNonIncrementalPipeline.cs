using Unity.BuildSystem.NativeProgramSupport;
using UnityEditor;

namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for Switch
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class SwitchClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new SwitchPlatform();

        protected override BuildTarget BuildTarget => BuildTarget.Switch;
    }
}

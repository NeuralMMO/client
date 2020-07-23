using Unity.BuildSystem.NativeProgramSupport;
using UnityEditor;

namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for WebGL
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class WebGLClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new WebGLPlatform();

        protected override BuildTarget BuildTarget => BuildTarget.WebGL;
    }
}

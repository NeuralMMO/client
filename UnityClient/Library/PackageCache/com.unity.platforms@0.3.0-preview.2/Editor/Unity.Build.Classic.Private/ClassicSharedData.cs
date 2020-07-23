using UnityEditor;

namespace Unity.Build.Classic.Private
{
    public class ClassicSharedData
    {
        public string PlaybackEngineDirectory { get; internal set; }

        public string BuildToolsDirectory { get; internal set; }

        public string OutputBuildDirectory { get; internal set; }

        public string StreamingAssetsDirectory { set; get; }

        public bool DevelopmentPlayer { get; internal set; }

        public BuildTarget BuildTarget { get; set; }

        public ClassicBuildPipelineCustomizer[] Customizers { get; set; }
    }
}

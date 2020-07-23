using Unity.Build;
using Unity.Build.Classic;
using Unity.Scenes.Editor.Build;

namespace Unity.Scenes.Editor
{
    class HybridBuildPipelineMigration : HybridBuildPipelineMigrationBase
    {
        // Legacy DOTS Hybrid LiveLink build pipeline guids
        const string HybridLiveLink = "b08acfa97ea29e84597342a6a2afa6b2";
        const string AndroidHybridLiveLink = "2796e060b2ae7a44f885faf3f9e933e9";
        const string LinuxHybridLiveLink = "79829fe695c07ce638c43de9254d2cf6";
        const string OSXHybridLiveLink = "a0e649094aaed7c45959a7d34723978a";
        const string WindowsHybridLiveLink = "b9a325ffa9c6f024cb1086a2e1e01d42";

        public override void Migrate(BuildConfiguration config, string assetGuid)
        {
            if (assetGuid == HybridLiveLink ||
                assetGuid == AndroidHybridLiveLink ||
                assetGuid == LinuxHybridLiveLink ||
                assetGuid == OSXHybridLiveLink ||
                assetGuid == WindowsHybridLiveLink)
            {
                config.SetComponent<LiveLink>();
            }
        }
    }
}

using UnityEngine.Events;

namespace Unity.Build
{
    internal struct BuildBatchItem
    {
        public BuildConfiguration BuildConfiguration;
    }

    internal struct BuildBatchDescription
    {
        public BuildBatchItem[] BuildItems;
        public UnityAction<BuildResult[]> OnBuildCompleted;
    }
}

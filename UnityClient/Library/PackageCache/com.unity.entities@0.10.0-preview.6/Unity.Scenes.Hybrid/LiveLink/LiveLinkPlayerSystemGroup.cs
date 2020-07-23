using Unity.Entities;
using UnityEngine;

namespace Unity.Scenes
{
    //@TODO: #ifdefs massively increase iteration time right now when building players (Should be fixed in 20.1)
    //       Until then always have the live link code present.
#if UNITY_EDITOR
    [DisableAutoCreation]
#endif
    [ExecuteAlways]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(SceneSystemGroup))]
    class LiveLinkRuntimeSystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            LiveLinkUtility.LiveLinkBoot();
            Enabled = LiveLinkUtility.LiveLinkEnabled;
            if (Enabled)
            {
                World.GetOrCreateSystem<SceneSystem>().BuildConfigurationGUID = LiveLinkUtility.BuildConfigurationGUID;
            }
        }
    }
}

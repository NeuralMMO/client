using System;
using System.Collections.Generic;
using Unity.Scenes;
using Unity.Transforms;

namespace Unity.Entities.Hybrid.EndToEnd.Tests
{
    public static class TestUtilities
    {
        [Flags]
        public enum SystemCategories
        {
            Streaming = 1,
#if !UNITY_DISABLE_MANAGED_COMPONENTS
            HybridComponents = 2,
#endif
        }

        public static void RegisterSystems(World world, SystemCategories categories)
        {
            var systems = new List<Type>();

            if ((categories & SystemCategories.Streaming) == SystemCategories.Streaming)
            {
                systems.AddRange(new[]
                {
                    typeof(SceneSystemGroup),
                    typeof(SceneSystem),
                    typeof(ResolveSceneReferenceSystem),
                    typeof(SceneSectionStreamingSystem)
                });
            }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
            if ((categories & SystemCategories.HybridComponents) == SystemCategories.HybridComponents)
            {
                systems.AddRange(new[]
                {
                    typeof(CompanionGameObjectUpdateSystem),
                    typeof(CompanionGameObjectUpdateTransformSystem),
                    typeof(TransformSystemGroup) // empty but required to satisfy constraint
                });
            }
#endif

            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
        }
    }
}

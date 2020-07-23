using System;
using System.Linq;
using Unity.Build.Classic;
using Unity.Build.Classic.Private;
using Unity.BuildSystem.NativeProgramSupport;
using Unity.Properties.Editor;
using Unity.Properties.UI;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    sealed class PlatformInspector : TypeInspector<Platform>
    {
        public override string SearcherTitle => "Select Platform";
        public override Func<Type, bool> TypeFilter => type =>
        {
            // If there is a pipeline that supports this platform, we want the platform to appear in the list.
            if (TypeCacheHelper.ConstructTypesDerivedFrom<ClassicPipelineBase>().Any(pipeline => pipeline.Platform.GetType() == type))
                return true;

            // If there is not, but it is a known common platform, we also want it in the list, so we have a way
            // to inform users that they have to install a package to build for that platform.
            return KnownPlatforms.All.ContainsKey(type);
        };
        public override Func<Type, string> TypeNameResolver => type => TypeConstruction.Construct<Platform>(type).DisplayName;
        public override Func<Type, string> TypeCategoryResolver => type => null;
    }

    sealed class ClassicBuildProfileInspector : Inspector<ClassicBuildProfile>
    {
        public override VisualElement Build()
        {
            var root = base.Build();
            var platformElement = root.Q<VisualElement>("Platform");
            if (platformElement == null)
            {
                return root;
            }

            var inspectorProperty = platformElement.GetType().GetProperty("Inspector");
            if (inspectorProperty == null)
            {
                return root;
            }

            var platformInspector = inspectorProperty.GetValue(platformElement) as PlatformInspector;
            if (platformInspector == null)
            {
                return root;
            }

            if (Target.Pipeline == null && Target.Platform != null)
            {
                if (KnownPlatforms.All.TryGetValue(Target.Platform.GetType(), out var packageName))
                {
                    platformInspector.ErrorMessage = $"Platform {Target.Platform.DisplayName} requires package '{packageName}' to be installed.";
                }
                else
                {
                    platformInspector.ErrorMessage = $"Platform {Target.Platform.DisplayName} requires a package to be installed.";
                }
                platformInspector.Update();
            }

            return root;
        }
    }
}

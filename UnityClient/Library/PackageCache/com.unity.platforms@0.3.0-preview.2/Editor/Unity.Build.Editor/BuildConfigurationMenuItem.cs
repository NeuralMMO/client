using System.IO;
using UnityEditor;

namespace Unity.Build.Editor
{
    public static class BuildConfigurationMenuItem
    {
        public const string k_BuildConfigurationMenu = "Assets/Create/Build/";
        const string k_CreateBuildConfigurationAssetEmpty = k_BuildConfigurationMenu + "Empty Build Configuration";

        [MenuItem(k_CreateBuildConfigurationAssetEmpty, true)]
        static bool CreateBuildConfigurationAssetValidation()
        {
            return Directory.Exists(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        [MenuItem(k_CreateBuildConfigurationAssetEmpty)]
        static void CreateBuildConfigurationAsset()
        {
            Selection.activeObject = CreateAssetInActiveDirectory("Empty");
        }

        public static BuildConfiguration CreateAssetInActiveDirectory(string prefix, params IBuildComponent[] components)
        {
            var dependency = Selection.activeObject as BuildConfiguration;
            return BuildConfiguration.CreateAssetInActiveDirectory(prefix + $"{nameof(BuildConfiguration)}{BuildConfiguration.AssetExtension}", (config) =>
            {
                if (dependency != null)
                {
                    config.AddDependency(dependency);
                }

                foreach (var component in components)
                {
                    config.SetComponent(component.GetType(), component);
                }
            });
        }
    }
}

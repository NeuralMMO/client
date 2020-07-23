using System.IO;

namespace Unity.Build
{
    internal class Package
    {
        public static string PackageName => "com.unity.platforms";
        public static string PackagePath => "Packages/" + PackageName;

        public static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            var assetPath = Path.Combine(PackagePath, path).Replace('\\', '/');
            if (!File.Exists(assetPath))
            {
                UnityEngine.Debug.LogError($"{typeof(T).Name} asset {assetPath.ToHyperLink()} not found.");
                return null;
            }

            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null || !asset)
            {
                UnityEngine.Debug.LogError($"Failed to load {typeof(T).Name} asset {assetPath.ToHyperLink()}.");
                return null;
            }

            return asset;
        }
    }
}

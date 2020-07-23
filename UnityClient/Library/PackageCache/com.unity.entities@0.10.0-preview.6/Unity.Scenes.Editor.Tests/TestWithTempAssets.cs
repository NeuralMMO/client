using System;
using System.IO;
using UnityEditor;

namespace Unity.Scenes.Editor.Tests
{
    [Serializable]
    struct TestWithTempAssets
    {
        public string TempAssetDir;
        public int AssetCounter;

        public void SetUp()
        {
            string path;
            do
            {
                path = Path.GetRandomFileName();
            }
            while (AssetDatabase.IsValidFolder(Path.Combine("Assets", path)));

            var guid = AssetDatabase.CreateFolder("Assets", path);
            TempAssetDir = AssetDatabase.GUIDToAssetPath(guid);
        }

        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TempAssetDir);
        }

        public string GetNextPath() => Path.Combine(TempAssetDir, (AssetCounter++).ToString());
        public string GetNextPath(string ext) => GetNextPath() + ext;
    }
}

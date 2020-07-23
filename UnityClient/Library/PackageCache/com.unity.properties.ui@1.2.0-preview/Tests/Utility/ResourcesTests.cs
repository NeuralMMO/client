using NUnit.Framework;
using UnityEditor;
using Unity.Properties.UI.Internal;

namespace Unity.Properties.UI.Tests
{
    class ResourcesTests
    {
        [Test]
        public void Resources_MainFolders_Exists()
        {
            Assert.That(AssetDatabase.AssetPathToGUID(Resources.BasePath), Is.Not.Null);
            Assert.That(AssetDatabase.AssetPathToGUID(Resources.ResourcesPath), Is.Not.Null);
            Assert.That(AssetDatabase.AssetPathToGUID(Resources.Uxml), Is.Not.Null);
            Assert.That(AssetDatabase.AssetPathToGUID(Resources.Uss), Is.Not.Null);
            Assert.That(AssetDatabase.AssetPathToGUID(Resources.Icons), Is.Not.Null);
        }
    }
}
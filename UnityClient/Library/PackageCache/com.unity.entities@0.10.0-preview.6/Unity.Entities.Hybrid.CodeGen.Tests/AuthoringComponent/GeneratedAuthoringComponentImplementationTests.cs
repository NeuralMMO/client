using System.Collections.Generic;
using NUnit.Framework;
using Unity.Entities.Hybrid.Internal;
using UnityEditor;
using UnityEngine;

namespace Unity.Entities.Hybrid.CodeGen.Tests
{
    public class GeneratedAuthoringComponentImplementationTests
    {
        [Test]
        public void AddReferencedPrefabs_WithNullValues_DoesNotThrow()
        {
            var prefabs = new List<GameObject>();
            Assert.DoesNotThrow(() => GeneratedAuthoringComponentImplementation.AddReferencedPrefabs(prefabs, new List<GameObject> { null }));
            Assert.IsEmpty(prefabs);
        }

        [Test]
        public void AddReferencedPrefab_AddsPrefabs()
        {
            var path = "Packages/com.unity.entities/Unity.Entities.Hybrid.CodeGen.Tests/AuthoringComponent/TestPrefab.prefab";

            var prefabs = new List<GameObject>();
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(p);
            GeneratedAuthoringComponentImplementation.AddReferencedPrefab(prefabs, p);
            Assert.Contains(p, prefabs);
            Assert.AreEqual(1, prefabs.Count);
        }

        [Test]
        public void AddReferencedPrefab_DoesNotAddNonPrefab()
        {
            var go = new GameObject();
            try
            {
                var prefabs = new List<GameObject>();
                GeneratedAuthoringComponentImplementation.AddReferencedPrefab(prefabs, go);
                Assert.IsEmpty(prefabs);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

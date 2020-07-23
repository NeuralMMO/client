using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.Conversion;
using UnityEngine;

namespace Unity.Entities.Tests.Conversion
{
    class ConversionDependencyTests : ConversionTestFixtureBase
    {
        ConversionDependencies m_Dependencies;
        [SetUp]
        public new void Setup()
        {
            m_Dependencies = new ConversionDependencies(true);
        }

        [TearDown]
        public new void TearDown()
        {
            m_Dependencies.Dispose();
        }

        void AssertDependencyExists<T>(UnsafeMultiHashMap<T, int> map, T key, GameObject dependent) where T : unmanaged, IEquatable<T>
            => AssertDependencyExists(m_Dependencies, map, key, dependent);

        static void AssertDependencyExists<T>(ConversionDependencies dependencies, UnsafeMultiHashMap<T, int> map, T key, GameObject dependent) where T : unmanaged, IEquatable<T>
        {
            if (!dependencies.GameObjectIndexByInstanceId.TryGetValue(dependent.GetInstanceID(), out int idx))
                Assert.Fail($"The conversion system didn't store the dependent game object {dependent.name}.");
            Assert.AreEqual(dependent, dependencies.DependentGameObjects[idx]);
            if (!map.TryGetFirstValue(key, out var value, out _))
                Assert.Fail("The dependent wasn't registered to the key of the dependency.");
            Assert.AreEqual(idx, value);
        }

        [Test]
        public void GameObjectDependencies_AreCollected_WhenLiveLinked([Values] bool isLiveLink)
        {
            var goA = CreateGameObject("A");
            var goB = CreateGameObject("B");
            using (var dep = new ConversionDependencies(isLiveLink))
            {
                dep.DependOnGameObject(goA, goB);

                if (isLiveLink)
                    AssertDependencyExists(dep, dep.GameObjectDependentsByInstanceId, goB.GetInstanceID(), goA);
                else
                {
                    CollectionAssert.IsEmpty(dep.DependentGameObjects);
                    Assert.IsFalse(dep.GameObjectDependentsByInstanceId.IsCreated);
                }
            }
        }

        [Test]
        public void GameObjectDependencies_WithInvalidDependent_Throws()
            => Assert.Throws<ArgumentNullException>(() => m_Dependencies.DependOnGameObject(null, CreateGameObject("Test")));

        [Test]
        public void GameObjectDependencies_WithInvalidDependency_Throws()
            => Assert.Throws<ArgumentNullException>(() => m_Dependencies.DependOnGameObject(CreateGameObject("Test"), null));

        [Test]
        public void GameObjectDependencies_CalculateDependents_TransitiveDependentsAreIncluded()
        {
            var goA = CreateGameObject("A");
            var goB = CreateGameObject("B");
            var goC = CreateGameObject("C");

            m_Dependencies.DependOnGameObject(goA, goB);
            m_Dependencies.DependOnGameObject(goB, goC);
            var dependents = m_Dependencies.CalculateDependents(new[] { goC });
            Assert.AreEqual(3, dependents.Count);
            Assert.IsTrue(dependents.Contains(goA), "Failed to include transitive dependency");
            Assert.IsTrue(dependents.Contains(goB), "Failed to include direct dependency");
            Assert.IsTrue(dependents.Contains(goC), "Failed to include self among dependents");
        }

        [Test]
        public void AssetDependencies_AreCollected()
        {
            var go = CreateGameObject("A");
            var prefab = LoadPrefab("Prefab");
            m_Dependencies.DependOnAsset(go, prefab);
            AssertDependencyExists(m_Dependencies.AssetDependentsByInstanceId, prefab.GetInstanceID(), go);
        }

        [Test]
        public void AssetDependencies_InvalidAssetThrows()
        {
            var goA = CreateGameObject("A");
            var goB = CreateGameObject("B");
            Assert.Throws<ArgumentException>(() => m_Dependencies.DependOnAsset(goA, goB), "not an asset");
        }
    }
}

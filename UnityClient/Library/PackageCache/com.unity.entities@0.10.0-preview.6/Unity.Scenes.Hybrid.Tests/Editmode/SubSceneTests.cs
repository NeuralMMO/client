using System;
using System.Collections;
using NUnit.Framework;
#if UNITY_EDITOR
using Unity.Build;
using Unity.Build.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
#endif
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Tests;
using UnityEditor.Experimental;
using Hash128 = Unity.Entities.Hash128;

namespace Unity.Scenes.Hybrid.Tests
{
    public class SubSceneTests
    {
        //string m_ScenePath = "Packages/com.unity.entities/Unity.Scenes.Hybrid.Tests/TestSceneWithSubScene.unity";
        #if UNITY_EDITOR
        static string m_SubScenePath =
            "Packages/com.unity.entities/Unity.Scenes.Hybrid.Tests/TestSceneWithSubScene/TestSubScene.unity";
        static string m_TempPath = "Assets/Temp";
        static string m_BuildConfigPath = $"{m_TempPath}/BuildConfig.buildconfiguration";
        static GUID m_BuildConfigurationGUID;
        static string m_SceneWithBuildSettingsPath;
        #endif

        static Hash128 m_SceneGUID;

        [OneTimeSetUp]
        public void SetUpOnce()
        {
            #if UNITY_EDITOR
            try
            {
                BuildConfiguration.CreateAsset(m_BuildConfigPath, config =>
                {
                    config.SetComponent(new SceneList
                    {
                        SceneInfos = new List<SceneList.SceneInfo>
                        {
                            new SceneList.SceneInfo
                            {
                                Scene = GlobalObjectId.GetGlobalObjectIdSlow(AssetDatabase.LoadAssetAtPath<SceneAsset>(m_SubScenePath))
                            }
                        }
                    });
                });
                m_BuildConfigurationGUID = new GUID(AssetDatabase.AssetPathToGUID(m_BuildConfigPath));
                m_SceneGUID = new GUID(AssetDatabase.AssetPathToGUID(m_SubScenePath));

                var guid = SceneWithBuildConfigurationGUIDs.EnsureExistsFor(m_SceneGUID, m_BuildConfigurationGUID);
                m_SceneWithBuildSettingsPath = SceneWithBuildConfigurationGUIDs.GetSceneWithBuildSettingsPath(ref guid);
                EntityScenesPaths.GetSubSceneArtifactHash(m_SceneGUID, m_BuildConfigurationGUID, ImportMode.Synchronous);
            }
            catch
            {
                AssetDatabase.DeleteAsset(m_TempPath);
                AssetDatabase.DeleteAsset(m_SceneWithBuildSettingsPath);
                throw;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            #else
            //TODO: Playmode test not supported yet
            var sceneGuid = new Unity.Entities.Hash128();
            #endif
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            #if UNITY_EDITOR
            AssetDatabase.DeleteAsset(m_TempPath);
            AssetDatabase.DeleteAsset(m_SceneWithBuildSettingsPath);
            #endif
        }

        [UnityTest]
        [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]
        public IEnumerator LoadMultipleSubscenes_Async_WithAssetBundles()
        {
            using (var worldA = TestWorldSetup.CreateEntityWorld("World A", false))
            using (var worldB = TestWorldSetup.CreateEntityWorld("World B", false))
            {
                var sceneSystemA = worldA.GetExistingSystem<SceneSystem>();
                var sceneSystemB = worldB.GetExistingSystem<SceneSystem>();
                Assert.IsTrue(m_SceneGUID.IsValid);

                var worldAScene = sceneSystemA.LoadSceneAsync(m_SceneGUID);
                var worldBScene = sceneSystemB.LoadSceneAsync(m_SceneGUID);

                Assert.IsFalse(sceneSystemA.IsSceneLoaded(worldAScene));
                Assert.IsFalse(sceneSystemB.IsSceneLoaded(worldBScene));

                while (!sceneSystemA.IsSceneLoaded(worldAScene) || !sceneSystemB.IsSceneLoaded(worldBScene))
                {
                    worldA.Update();
                    worldB.Update();
                    yield return null;
                }

                var worldAEntities = worldA.EntityManager.GetAllEntities(Allocator.TempJob);
                var worldBEntities = worldB.EntityManager.GetAllEntities(Allocator.TempJob);
                using (worldAEntities)
                using (worldBEntities)
                {
                    Assert.AreEqual(worldAEntities.Length, worldBEntities.Length);
                }

                var worldAQuery = worldA.EntityManager.CreateEntityQuery(typeof(SharedWithMaterial));
                var worldBQuery = worldB.EntityManager.CreateEntityQuery(typeof(SharedWithMaterial));
                Assert.AreEqual(worldAQuery.CalculateEntityCount(), worldBQuery.CalculateEntityCount());
                Assert.AreEqual(1, worldAQuery.CalculateEntityCount());

                // Get Material on RenderMesh
                var sharedEntitiesA = worldAQuery.ToEntityArray(Allocator.TempJob);
                var sharedEntitiesB = worldBQuery.ToEntityArray(Allocator.TempJob);

                SharedWithMaterial sharedA;
                SharedWithMaterial sharedB;
                using (sharedEntitiesA)
                using (sharedEntitiesB)
                {
                    sharedA = worldA.EntityManager.GetSharedComponentData<SharedWithMaterial>(sharedEntitiesA[0]);
                    sharedB = worldB.EntityManager.GetSharedComponentData<SharedWithMaterial>(sharedEntitiesB[0]);
                }

                Assert.AreSame(sharedA.material, sharedB.material);
                Assert.IsTrue(sharedA.material != null, "sharedA.material != null");

                var material = sharedA.material;

#if !UNITY_EDITOR
                Assert.AreEqual(1, SceneBundleHandle.GetLoadedCount());
#else
                Assert.AreEqual(0, SceneBundleHandle.GetLoadedCount());
#endif
                Assert.AreEqual(0, SceneBundleHandle.GetUnloadingCount());

                worldA.GetOrCreateSystem<SceneSystem>().UnloadScene(worldAScene);
                worldA.Update();

                worldB.GetOrCreateSystem<SceneSystem>().UnloadScene(worldBScene);
                worldB.Update();

                Assert.AreEqual(0, SceneBundleHandle.GetLoadedCount());
                Assert.AreEqual(0, SceneBundleHandle.GetUnloadingCount());
#if !UNITY_EDITOR
                Assert.IsTrue(material == null);
#endif
            }
        }

        // Only works in Editor for now until we can support SubScene building with new build settings in a test
        [Test]
        [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]
        public void LoadMultipleSubscenes_Blocking_WithAssetBundles()
        {
            using (var worldA = TestWorldSetup.CreateEntityWorld("World A", false))
            using (var worldB = TestWorldSetup.CreateEntityWorld("World B", false))
            {
                var loadParams = new SceneSystem.LoadParameters
                {
                    Flags = SceneLoadFlags.BlockOnImport | SceneLoadFlags.BlockOnStreamIn
                };

                Assert.IsTrue(m_SceneGUID.IsValid);

                var worldAScene = worldA.GetOrCreateSystem<SceneSystem>().LoadSceneAsync(m_SceneGUID, loadParams);
                var worldBScene = worldB.GetOrCreateSystem<SceneSystem>().LoadSceneAsync(m_SceneGUID, loadParams);
                Assert.IsFalse(worldA.GetExistingSystem<SceneSystem>().IsSceneLoaded(worldAScene));
                Assert.IsFalse(worldB.GetExistingSystem<SceneSystem>().IsSceneLoaded(worldBScene));

                worldA.Update();
                worldB.Update();

                Assert.IsTrue(worldA.GetExistingSystem<SceneSystem>().IsSceneLoaded(worldAScene));
                Assert.IsTrue(worldB.GetExistingSystem<SceneSystem>().IsSceneLoaded(worldBScene));

                var worldAEntities = worldA.EntityManager.GetAllEntities(Allocator.TempJob);
                var worldBEntities = worldB.EntityManager.GetAllEntities(Allocator.TempJob);
                using (worldAEntities)
                using (worldBEntities)
                {
                    Assert.AreEqual(worldAEntities.Length, worldBEntities.Length);
                }

                var worldAQuery = worldA.EntityManager.CreateEntityQuery(typeof(SharedWithMaterial));
                var worldBQuery = worldB.EntityManager.CreateEntityQuery(typeof(SharedWithMaterial));
                Assert.AreEqual(worldAQuery.CalculateEntityCount(), worldBQuery.CalculateEntityCount());
                Assert.AreEqual(1, worldAQuery.CalculateEntityCount());

                // Get Material on RenderMesh
                var sharedEntitiesA = worldAQuery.ToEntityArray(Allocator.TempJob);
                var sharedEntitiesB = worldBQuery.ToEntityArray(Allocator.TempJob);

                SharedWithMaterial sharedA;
                SharedWithMaterial sharedB;
                using (sharedEntitiesA)
                using (sharedEntitiesB)
                {
                    sharedA = worldA.EntityManager.GetSharedComponentData<SharedWithMaterial>(sharedEntitiesA[0]);
                    sharedB = worldB.EntityManager.GetSharedComponentData<SharedWithMaterial>(sharedEntitiesB[0]);
                }

                Assert.AreSame(sharedA.material, sharedB.material);
                Assert.IsTrue(sharedA.material != null, "sharedA.material != null");

                var material = sharedA.material;

#if !UNITY_EDITOR
                Assert.AreEqual(1, SceneBundleHandle.GetLoadedCount());
#else
                Assert.AreEqual(0, SceneBundleHandle.GetLoadedCount());
#endif
                Assert.AreEqual(0, SceneBundleHandle.GetUnloadingCount());

                worldA.GetOrCreateSystem<SceneSystem>().UnloadScene(worldAScene);
                worldA.Update();

                worldB.GetOrCreateSystem<SceneSystem>().UnloadScene(worldBScene);
                worldB.Update();

                Assert.AreEqual(0, SceneBundleHandle.GetLoadedCount());
                Assert.AreEqual(0, SceneBundleHandle.GetUnloadingCount());
#if !UNITY_EDITOR
                Assert.IsTrue(material == null);
#endif
            }
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS // PostLoadCommandBuffer is a managed component
        // Only works in Editor for now until we can support SubScene building with new build settings in a test
        [UnityTest]
        [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]
        public IEnumerator LoadSubscene_With_PostLoadCommandBuffer([Values] bool loadAsync, [Values] bool addCommandBufferToSection)
        {
            var postLoadCommandBuffer = new PostLoadCommandBuffer();
            postLoadCommandBuffer.CommandBuffer = new EntityCommandBuffer(Allocator.Persistent, PlaybackPolicy.MultiPlayback);
            var postLoadEntity = postLoadCommandBuffer.CommandBuffer.CreateEntity();
            postLoadCommandBuffer.CommandBuffer.AddComponent(postLoadEntity, new TestProcessAfterLoadData {Value = 42});

            using (var world = TestWorldSetup.CreateEntityWorld("World", false))
            {
                if (addCommandBufferToSection)
                {
                    var resolveParams = new SceneSystem.LoadParameters
                    {
                        Flags = SceneLoadFlags.BlockOnImport | SceneLoadFlags.DisableAutoLoad
                    };
                    world.GetOrCreateSystem<SceneSystem>().LoadSceneAsync(m_SceneGUID, resolveParams);
                    world.Update();
                    var section = world.EntityManager.CreateEntityQuery(typeof(SceneSectionData)).GetSingletonEntity();
                    world.EntityManager.AddComponentData(section, postLoadCommandBuffer);
                }

                var loadParams = new SceneSystem.LoadParameters
                {
                    Flags = loadAsync ? 0 : SceneLoadFlags.BlockOnImport | SceneLoadFlags.BlockOnStreamIn
                };

                Assert.IsTrue(m_SceneGUID.IsValid);

                var scene = world.GetOrCreateSystem<SceneSystem>().LoadSceneAsync(m_SceneGUID, loadParams);
                if (!addCommandBufferToSection)
                    world.EntityManager.AddComponentData(scene, postLoadCommandBuffer);

                if (loadAsync)
                {
                    while (!world.GetExistingSystem<SceneSystem>().IsSceneLoaded(scene))
                    {
                        world.Update();
                        yield return null;
                    }
                }
                else
                {
                    world.Update();
                    Assert.IsTrue(world.GetExistingSystem<SceneSystem>().IsSceneLoaded(scene));
                }

                var ecsTestDataQuery = world.EntityManager.CreateEntityQuery(typeof(TestProcessAfterLoadData));
                Assert.AreEqual(1, ecsTestDataQuery.CalculateEntityCount());
                Assert.AreEqual(43, ecsTestDataQuery.GetSingleton<TestProcessAfterLoadData>().Value);
            }

            // Check that command buffer has been Disposed
            Assert.IsFalse(postLoadCommandBuffer.CommandBuffer.IsCreated);
        }

#endif
    }

    public struct TestProcessAfterLoadData : IComponentData
    {
        public int Value;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ProcessAfterLoad)]
    public class IncrementEcsTestDataProcessAfterLoadSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref TestProcessAfterLoadData data) =>
            {
                data.Value++;
            }).Run();
        }
    }
}

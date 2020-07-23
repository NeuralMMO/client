using NUnit.Framework;
#if UNITY_EDITOR
using Unity.Build;
using Unity.Build.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
#endif
using Unity.Entities;
using Unity.Entities.Tests;
using Hash128 = Unity.Entities.Hash128;

namespace Unity.Scenes.Hybrid.Tests
{
    public class SectionMetadataTests
    {
        #if UNITY_EDITOR
        static string m_SubScenePath =
            "Packages/com.unity.entities/Unity.Scenes.Hybrid.Tests/TestSceneWithSubScene/TestSubSceneWithSectionMetadata.unity";
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

        // Only works in Editor for now until we can support SubScene building with new build settings in a test
        [Test]
        [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]
        public void SectionMetadata()
        {
            using (var world = TestWorldSetup.CreateEntityWorld("World", false))
            {
                var resolveParams = new SceneSystem.LoadParameters
                {
                    Flags = SceneLoadFlags.BlockOnImport | SceneLoadFlags.DisableAutoLoad
                };
                var sceneSystem = world.GetOrCreateSystem<SceneSystem>();
                var sceneEntity = sceneSystem.LoadSceneAsync(m_SceneGUID, resolveParams);
                world.Update();
                var manager = world.EntityManager;
                var sectionEntities = manager.GetBuffer<ResolvedSectionEntity>(sceneEntity);

                Assert.AreEqual(3, sectionEntities.Length);
                Assert.IsTrue(manager.HasComponent<TestMetadata>(sectionEntities[0].SectionEntity));
                Assert.IsFalse(manager.HasComponent<TestMetadata>(sectionEntities[1].SectionEntity));
                Assert.IsTrue(manager.HasComponent<TestMetadata>(sectionEntities[2].SectionEntity));

                Assert.IsTrue(manager.HasComponent<TestMetadataTag>(sectionEntities[0].SectionEntity));
                Assert.IsFalse(manager.HasComponent<TestMetadataTag>(sectionEntities[1].SectionEntity));
                Assert.IsTrue(manager.HasComponent<TestMetadataTag>(sectionEntities[2].SectionEntity));

                // These components should not be added, instead an error is logged that meta info components can't contain entities or blob assets
                var filteredTypes = new[]
                {
                    typeof(TestMetadataWithEntity), typeof(TestMetadataWithBlobAsset), typeof(EcsTestSharedComp), typeof(EcsIntElement), typeof(EcsState1),
#if !UNITY_DISABLE_MANAGED_COMPONENTS
                    typeof(EcsTestManagedComponent)
#endif
                };

                foreach (var type in filteredTypes)
                {
                    var componentType = ComponentType.FromTypeIndex(TypeManager.GetTypeIndex(type));
                    Assert.IsFalse(manager.HasComponent(sectionEntities[0].SectionEntity, componentType));
                }

                Assert.AreEqual(0, manager.GetComponentData<TestMetadata>(sectionEntities[0].SectionEntity).SectionIndex);
                Assert.AreEqual(13, manager.GetComponentData<TestMetadata>(sectionEntities[0].SectionEntity).Value);
                Assert.AreEqual(42, manager.GetComponentData<TestMetadata>(sectionEntities[2].SectionEntity).SectionIndex);
                Assert.AreEqual(100, manager.GetComponentData<TestMetadata>(sectionEntities[2].SectionEntity).Value);

                var hash = EntityScenesPaths.GetSubSceneArtifactHash(m_SceneGUID, sceneSystem.BuildConfigurationGUID, ImportMode.Synchronous);
                Assert.IsTrue(hash.IsValid);
                AssetDatabaseCompatibility.GetArtifactPaths(hash, out var paths);
                var logPath = EntityScenesPaths.GetLoadPathFromArtifactPaths(paths, EntityScenesPaths.PathType.EntitiesConversionLog);
                Assert.NotNull(logPath);
                var log = System.IO.File.ReadAllText(logPath);
                Assert.IsTrue(log.Contains("The component type must contains only blittable/basic data types"));
            }
        }
    }
}

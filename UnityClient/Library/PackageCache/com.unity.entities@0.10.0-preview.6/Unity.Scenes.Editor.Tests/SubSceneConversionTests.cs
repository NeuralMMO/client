using System;
using System.IO;
using NUnit.Framework;
using Unity.Entities.Tests;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Scenes.Editor.Tests
{
    public class SubSceneConversionTests
    {
        string m_TempAssetDir;

        [OneTimeSetUp]
        public void SetUp()
        {
            var guid = AssetDatabase.CreateFolder("Assets", Path.GetRandomFileName());
            m_TempAssetDir = AssetDatabase.GUIDToAssetPath(guid);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AssetDatabase.DeleteAsset(m_TempAssetDir);
            SceneWithBuildConfigurationGUIDs.ClearBuildSettingsCache();
        }

        SubScene CreateSubScene(string subSceneName, string parentSceneName, InteractionMode interactionMode = InteractionMode.AutomatedAction, SubSceneContextMenu.NewSubSceneMode mode = SubSceneContextMenu.NewSubSceneMode.MoveSelectionToScene)
        {
            var mainScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            SceneManager.SetActiveScene(mainScene);

            var path = Path.Combine(m_TempAssetDir, $"{parentSceneName}.unity");
            EditorSceneManager.SaveScene(mainScene, path);

            var go = new GameObject();
            go.name = subSceneName;
            Selection.activeGameObject = go;

            var args = new SubSceneContextMenu.NewSubSceneArgs
            {
                target = go,
                newSubSceneMode = mode
            };
            return SubSceneContextMenu.CreateNewSubScene(go.name, args, interactionMode);
        }

        [Test]
        public void SubScene_WithDependencyOnAsset_IsInvalidatedWhenAssetChanges()
        {
            var subScene = CreateSubScene("SubScene", nameof(SubScene_WithDependencyOnAsset_IsInvalidatedWhenAssetChanges));
            SubSceneInspectorUtility.EditScene(subScene);
            var go = new GameObject();
            var authoring = go.AddComponent<DependencyTestAuthoring>();

            var dependency = new GameObject();
            authoring.GameObject = dependency;
            var texture = new Texture2D(64, 64);
            authoring.Asset = texture;
            var assetPath = Path.Combine(m_TempAssetDir, "Texture.asset");
            AssetDatabase.CreateAsset(authoring.Asset, assetPath);
            SceneManager.MoveGameObjectToScene(dependency, subScene.EditingScene);
            SceneManager.MoveGameObjectToScene(go, subScene.EditingScene);
            EditorSceneManager.SaveScene(subScene.EditingScene);

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(subScene.SceneAsset, out var guid, out long _);

            var buildSettings = default(Unity.Entities.Hash128);
            var subSceneGuid = new GUID(guid);
            var hash = EntityScenesPaths.GetSubSceneArtifactHash(subSceneGuid, buildSettings, ImportMode.Synchronous);
            Assert.IsTrue(hash.IsValid);

            texture.wrapMode = texture.wrapMode == TextureWrapMode.Repeat ? TextureWrapMode.Mirror : TextureWrapMode.Repeat;
            AssetDatabase.SaveAssets();
            var newHash = EntityScenesPaths.GetSubSceneArtifactHash(subSceneGuid, buildSettings, ImportMode.NoImport);
            Assert.AreNotEqual(hash, newHash);
            Assert.IsFalse(newHash.IsValid);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Scenes.Editor.Tests
{
    static class SubSceneTestsHelper
    {
        public static SubScene CreateSubSceneInSceneFromObjects(string name, bool keepOpen, Scene parentScene, Func<List<GameObject>> createObjects = null)
        {
            var args = new SubSceneContextMenu.NewSubSceneArgs
            {
                parentScene = parentScene,
                newSubSceneMode = SubSceneContextMenu.NewSubSceneMode.EmptyScene
            };
            SceneManager.SetActiveScene(parentScene);

            var subScene = SubSceneContextMenu.CreateNewSubScene(name, args, InteractionMode.AutomatedAction);
            SubSceneInspectorUtility.EditScene(subScene);
            var objects = createObjects?.Invoke();
            if (objects != null)
            {
                foreach (var obj in objects)
                    SceneManager.MoveGameObjectToScene(obj, subScene.EditingScene);
            }

            EditorSceneManager.SaveScene(subScene.EditingScene);
            EditorSceneManager.SaveScene(parentScene);
            if (!keepOpen)
                SubSceneInspectorUtility.CloseSceneWithoutSaving(subScene);
            return subScene;
        }

        public static Scene CreateScene(string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            SceneManager.SetActiveScene(scene);
            var dir = Path.GetDirectoryName(scenePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            AssetDatabase.DeleteAsset(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);
            return scene;
        }

        public static Scene CreateTmpScene(ref TestWithTempAssets testAssets)
        {
            var parentScenePath = testAssets.GetNextPath() + ".unity";
            return CreateScene(parentScenePath);
        }

        public static SubScene CreateSubSceneFromObjects(ref TestWithTempAssets testAssets, string name, bool keepOpen, Func<List<GameObject>> createObjects)
        {
            return CreateSubSceneInSceneFromObjects(name, keepOpen, CreateTmpScene(ref testAssets), createObjects);
        }
    }
}

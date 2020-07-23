using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Hybrid.Tests;
using Unity.Entities.Tests;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace Unity.Scenes.Editor.Tests
{
    /*
     * These tests provide some coverage for LiveLink in the editor. LiveLink, by default, is used in edit mode and in
     * play mode whenever there is an open subscene. Its contents are converted to entities in the background, that is
     * the essential feature of LiveLink.
     *
     * The setup here is as follows:
     *  - all subscenes are created in a new temporary directory per test,
     *  - that directory is cleaned up when the test finished,
     *  - we also flush the entity scene paths cache to get rid of any subscene build files,
     *  - we clearly separate all tests into setup and test, because the latter might run in play mode.
     * That last point is crucial: Entering playmode serializes the test fixture, but not the contents of variables
     * within the coroutine that represents a test. This means that you cannot rely on the values of any variables and
     * you can get very nasty exceptions by assigning a variable from setup in play mode (due to the way enumerator
     * functions are compiled). Any data that needs to persist between edit and play mode must be stored on the class
     * itself.
     */
    [Serializable]
    [TestFixture]
    class LiveLinkEditorTests
    {
        [SerializeField]
        TestWithTempAssets m_Assets;
        [SerializeField]
        TestWithCustomDefaultGameObjectInjectionWorld m_DefaultWorld;
        [SerializeField]
        bool m_WasLiveLinkEnabled;
        [SerializeField]
        EnterPlayModeOptions m_EnterPlayModeOptions;
        [SerializeField]
        bool m_UseEnterPlayerModeOptions;

        [SerializeField]
        string m_PrefabPath;

        [SerializeField]
        Material m_TestMaterial;
        [SerializeField]
        Texture m_TestTexture;

        [OneTimeSetUp]
        public void SetUp()
        {
            if (m_Assets.TempAssetDir != null)
            {
                // this setup code is run again when we switch to playmode
                return;
            }

            // Create a temporary folder for test assets
            m_Assets.SetUp();
            m_DefaultWorld.Setup();
            m_WasLiveLinkEnabled = SubSceneInspectorUtility.LiveLinkEnabledInEditMode;
            m_EnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            m_UseEnterPlayerModeOptions = EditorSettings.enterPlayModeOptionsEnabled;
            SubSceneInspectorUtility.LiveLinkEnabledInEditMode = true;

            m_TestTexture = AssetDatabase.LoadAssetAtPath<Texture>(AssetPath("TestTexture.asset"));
            m_TestMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetPath("TestMaterial.mat"));
            AssetDatabase.SaveAssets();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Clean up all test assets
            m_Assets.TearDown();
            m_DefaultWorld.TearDown();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            SceneWithBuildConfigurationGUIDs.ClearBuildSettingsCache();
            SubSceneInspectorUtility.LiveLinkEnabledInEditMode = m_WasLiveLinkEnabled;
            EditorSettings.enterPlayModeOptions = m_EnterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = m_UseEnterPlayerModeOptions;
        }

        static string AssetPath(string name) => "Packages/com.unity.entities/Unity.Scenes.Editor.Tests/Assets/" + name;
        static string ScenePath(string name) => AssetPath(name) + ".unity";

        static void OpenAllSubScenes() => SubSceneInspectorUtility.EditScene(SubScene.AllSubScenes.ToArray());

        Scene CreateTmpScene() => SubSceneTestsHelper.CreateScene(m_Assets.GetNextPath() + ".unity");

        SubScene CreateSubSceneFromObjects(string name, bool keepOpen, Func<List<GameObject>> createObjects) =>
            SubSceneTestsHelper.CreateSubSceneInSceneFromObjects(name, keepOpen, CreateTmpScene(), createObjects);

        SubScene CreateEmptySubScene(string name, bool keepOpen) => CreateSubSceneFromObjects(name, keepOpen, null);

        static World GetLiveLinkWorld(bool playMode)
        {
            if (!playMode)
                DefaultWorldInitialization.DefaultLazyEditModeInitialize();
            return World.DefaultGameObjectInjectionWorld;
        }

        static IEditModeTestYieldInstruction GetEnterPlayMode(bool usePlayMode)
        {
            if (usePlayMode)
                return new EnterPlayMode();
            // ensure that the editor world is initialized
            GetLiveLinkWorld(false);
            return null;
        }

        static void SetDomainReload(EnteringPlayMode useDomainReload)
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = useDomainReload == EnteringPlayMode.WithDomainReload ? EnterPlayModeOptions.None : EnterPlayModeOptions.DisableDomainReload;
        }

        public enum EnteringPlayMode
        {
            WithDomainReload,
            WithoutDomainReload,
        }

        [UnityTest, Explicit]
        public IEnumerator OpenSubSceneStaysOpen_Play([Values] EnteringPlayMode useDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateEmptySubScene("TestSubScene", true);
            }

            yield return GetEnterPlayMode(true);

            {
                var subScene = Object.FindObjectOfType<SubScene>();
                Assert.IsTrue(subScene.IsLoaded);
            }
        }

        [UnityTest, Explicit]
        public IEnumerator ClosedSubSceneStaysClosed_Play([Values] EnteringPlayMode useDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateEmptySubScene("TestSubScene", false);
            }

            yield return GetEnterPlayMode(true);

            {
                var subScene = Object.FindObjectOfType<SubScene>();
                Assert.IsFalse(subScene.IsLoaded);
            }
        }

        [UnityTest, Explicit]
        public IEnumerator ClosedSubSceneCanBeOpened_Play([Values] EnteringPlayMode useDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateEmptySubScene("TestSubScene", false);
            }

            yield return GetEnterPlayMode(true);

            {
                var subScene = Object.FindObjectOfType<SubScene>();
                Assert.IsFalse(subScene.IsLoaded);
                SubSceneInspectorUtility.EditScene(subScene);
                yield return null;
                Assert.IsTrue(subScene.IsLoaded);
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkConvertsSubScenes_Edit() => LiveLinkConvertsSubScenes(false);

        [UnityTest, Explicit]
        public IEnumerator LiveLinkConvertsSubScenes_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkConvertsSubScenes(true, useDomainReload);

        IEnumerator LiveLinkConvertsSubScenes(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                var scene = CreateTmpScene();
                SubSceneTestsHelper.CreateSubSceneInSceneFromObjects("TestSubScene1", true, scene, () =>
                {
                    var go = new GameObject("TestGameObject1");
                    go.AddComponent<TestPrefabComponentAuthoring>().IntValue = 1;
                    return new List<GameObject> { go };
                });
                SubSceneTestsHelper.CreateSubSceneInSceneFromObjects("TestSubScene2", true, scene, () =>
                {
                    var go = new GameObject("TestGameObject2");
                    go.AddComponent<TestPrefabComponentAuthoring>().IntValue = 2;
                    return new List<GameObject> { go };
                });
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var w = GetLiveLinkWorld(usePlayMode);

                var subSceneQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<SubScene>());
                var subScenes = subSceneQuery.ToComponentArray<SubScene>();
                var subSceneObjects = Object.FindObjectsOfType<SubScene>();
                foreach (var subScene in subSceneObjects)
                    Assert.Contains(subScene, subScenes);

                var componentQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());

                Assert.AreEqual(2, componentQuery.CalculateEntityCount(), "Expected a game object to be converted");
                using (var components = componentQuery.ToComponentDataArray<TestPrefabComponent>(Allocator.TempJob))
                {
                    Assert.IsTrue(components.Contains(new TestPrefabComponent {IntValue = 1}), "Failed to find contents of subscene 1");
                    Assert.IsTrue(components.Contains(new TestPrefabComponent {IntValue = 2}), "Failed to find contents of subscene 2");
                }
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkRemovesDeletedSubScene_Edit() => LiveLinkRemovesDeletedSubScene(false);

        [UnityTest, Explicit]
        public IEnumerator LiveLinkRemovesDeletedSubScene_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkRemovesDeletedSubScene(true, useDomainReload);

        IEnumerator LiveLinkRemovesDeletedSubScene(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                var scene = CreateTmpScene();
                SubSceneTestsHelper.CreateSubSceneInSceneFromObjects("TestSubScene1", true, scene, () =>
                {
                    var go = new GameObject("TestGameObject");
                    go.AddComponent<TestPrefabComponentAuthoring>().IntValue = 1;
                    return new List<GameObject> { go };
                });
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var subScene = Object.FindObjectOfType<SubScene>();
                var w = GetLiveLinkWorld(usePlayMode);
                var subSceneQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<SubScene>());
                Assert.Contains(subScene, subSceneQuery.ToComponentArray<SubScene>(), "SubScene was not loaded");

                var componentQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(1, componentQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(1, componentQuery.GetSingleton<TestPrefabComponent>().IntValue);

                Object.DestroyImmediate(subScene.gameObject);

                yield return null;

                Assert.IsTrue(subSceneQuery.IsEmptyIgnoreFilter, "SubScene was not unloaded");
                Assert.AreEqual(0, componentQuery.CalculateEntityCount());
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkConvertsObjects_Edit() => LiveLinkConvertsObjects(false);

        [UnityTest, Explicit]
        public IEnumerator LiveLinkConvertsObjects_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkConvertsObjects(true, useDomainReload);

        IEnumerator LiveLinkConvertsObjects(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateSubSceneFromObjects("TestSubScene", true, () =>
                {
                    var go = new GameObject("TestGameObject");
                    go.AddComponent<TestPrefabComponentAuthoring>();
                    return new List<GameObject> { go };
                });
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var w = GetLiveLinkWorld(usePlayMode);
                var testTagQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkCreatesEntitiesWhenObjectIsCreated_Edit() => LiveLinkCreatesEntitiesWhenObjectIsCreated(false);

        [UnityTest, Explicit, Ignore("Doesn't currently work, since Undo.RegisterCreatedObjectUndo isn't reliably picked up by Undo.postprocessModifications and Scenes are never marked dirty in play mode. A reconversion is never triggered.")]
        public IEnumerator LiveLinkCreatesEntitiesWhenObjectIsCreated_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkCreatesEntitiesWhenObjectIsCreated(true, useDomainReload);

        IEnumerator LiveLinkCreatesEntitiesWhenObjectIsCreated(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateEmptySubScene("TestSubScene", true);
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var subScene = Object.FindObjectOfType<SubScene>();
                var w = GetLiveLinkWorld(usePlayMode);
                var testTagQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(0, testTagQuery.CalculateEntityCount());

                SceneManager.SetActiveScene(subScene.EditingScene);
                var go = new GameObject("CloneMe", typeof(TestPrefabComponentAuthoring));
                Undo.RegisterCreatedObjectUndo(go, "Create new object");
                Assert.AreEqual(go.scene, subScene.EditingScene);

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Undo.PerformUndo();

                yield return null;

                Assert.AreEqual(0, testTagQuery.CalculateEntityCount(), "Expected an entity to be removed, undo failed");
                Undo.PerformRedo();

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted, redo failed");
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkCreatesEntitiesWhenObjectMoves_Edit() => LiveLinkCreatesEntitiesWhenObjectMoves(false);

        [UnityTest, Explicit]
        public IEnumerator LiveLinkCreatesEntitiesWhenObjectMoves_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkCreatesEntitiesWhenObjectMoves(true, useDomainReload);

        IEnumerator LiveLinkCreatesEntitiesWhenObjectMoves(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateEmptySubScene("TestSubScene", true);
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var subScene = Object.FindObjectOfType<SubScene>();
                var w = GetLiveLinkWorld(usePlayMode);
                var testTagQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(0, testTagQuery.CalculateEntityCount());

                var go = new GameObject("TestGameObject");
                go.AddComponent<TestPrefabComponentAuthoring>();
                Undo.MoveGameObjectToScene(go, subScene.EditingScene, "Test Move1");

                // this doesn't work:
                //    SceneManager.MoveGameObjectToScene(go, subScene.EditingScene);

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Undo.PerformUndo();

                yield return null;

                Assert.AreEqual(0, testTagQuery.CalculateEntityCount(), "Expected an entity to be removed, undo failed");
                Undo.PerformRedo();

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted, redo failed");
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkDestroysEntitiesWhenObjectMoves_Edit() => LiveLinkCreatesEntitiesWhenObjectMoves(false);
        [UnityTest, Explicit, Ignore("Doesn't currently work, since Undo.MoveGameObjectToScene isn't reliably picked up by Undo.postprocessModifications and Scenes are never marked dirty in play mode. A reconversion is never triggered.")]
        public IEnumerator LiveLinkDestroysEntitiesWhenObjectMoves_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkDestroysEntitiesWhenObjectMoves(true, useDomainReload);

        IEnumerator LiveLinkDestroysEntitiesWhenObjectMoves(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateSubSceneFromObjects("TestSubScene", true, () =>
                {
                    var go = new GameObject("TestGameObject");
                    go.AddComponent<TestPrefabComponentAuthoring>();
                    return new List<GameObject> { go };
                });
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var subScene = Object.FindObjectOfType<SubScene>();
                var w = GetLiveLinkWorld(usePlayMode);
                var testTagQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");

                var go = Object.FindObjectOfType<TestPrefabComponentAuthoring>().gameObject;
                Undo.MoveGameObjectToScene(go, subScene.EditingScene, "Test Move1");

                yield return null;

                Assert.AreEqual(0, testTagQuery.CalculateEntityCount(), "Expected an entity to be removed");
                Undo.PerformUndo();

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted, undo failed");
                Undo.PerformRedo();

                yield return null;

                Assert.AreEqual(0, testTagQuery.CalculateEntityCount(), "Expected an entity to be removed, redo failed");
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkSupportsAddComponentAndUndo_Edit() => LiveLinkSupportsAddComponentAndUndo(false);

        [UnityTest, Explicit, Ignore("Doesn't currently work, since Undo.AddComponent isn't picked up by Undo.postprocessModifications and Scenes are never marked dirty in play mode. A reconversion is never triggered.")]
        public IEnumerator LiveLinkSupportsAddComponentAndUndo_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkSupportsAddComponentAndUndo(true, useDomainReload);

        IEnumerator LiveLinkSupportsAddComponentAndUndo(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateEmptySubScene("TestSubScene", true);
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var w = GetLiveLinkWorld(usePlayMode);
                var subScene = Object.FindObjectOfType<SubScene>();
                var go = new GameObject("TestGameObject");
                Undo.MoveGameObjectToScene(go, subScene.EditingScene, "Test Move");
                Undo.IncrementCurrentGroup();

                yield return null;

                Undo.AddComponent<TestPrefabComponentAuthoring>(go);
                Undo.IncrementCurrentGroup();
                Assert.IsNotNull(go.GetComponent<TestPrefabComponentAuthoring>());

                yield return null;

                var testTagQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted and gain a component");

                Undo.PerformUndo();
                Assert.IsNull(go.GetComponent<TestPrefabComponentAuthoring>());

                yield return null;

                Assert.AreEqual(0, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted and lose a component, undo add failed");

                Undo.PerformRedo();
                Assert.IsNotNull(go.GetComponent<TestPrefabComponentAuthoring>());

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted and gain a component, redo add failed");
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkSupportsRemoveComponentAndUndo_Edit() => LiveLinkSupportsRemoveComponentAndUndo(false);

        [UnityTest, Explicit, Ignore("Doesn't currently work, since Undo.DestroyObjectImmediate isn't picked up by Undo.postprocessModifications and Scenes are never marked dirty in play mode. A reconversion is never triggered.")]
        public IEnumerator LiveLinkSupportsRemoveComponentAndUndo_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkSupportsRemoveComponentAndUndo(true, useDomainReload);

        IEnumerator LiveLinkSupportsRemoveComponentAndUndo(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateEmptySubScene("TestSubScene", true);
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var w = GetLiveLinkWorld(usePlayMode);
                var subScene = Object.FindObjectOfType<SubScene>();
                var go = new GameObject("TestGameObject");
                go.AddComponent<TestPrefabComponentAuthoring>();
                Undo.MoveGameObjectToScene(go, subScene.EditingScene, "Test Move");
                Undo.IncrementCurrentGroup();

                yield return null;

                var testTagQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted with a component");

                Undo.DestroyObjectImmediate(go.GetComponent<TestPrefabComponentAuthoring>());
                Undo.IncrementCurrentGroup();

                Assert.IsNull(go.GetComponent<TestPrefabComponentAuthoring>());

                yield return null;

                Assert.AreEqual(0, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted and lose a component");

                Undo.PerformUndo();
                Assert.IsNotNull(go.GetComponent<TestPrefabComponentAuthoring>());

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted and gain a component, undo remove failed");

                Undo.PerformRedo();
                Assert.IsNull(go.GetComponent<TestPrefabComponentAuthoring>());

                yield return null;

                Assert.AreEqual(0, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted and lose a component, redo remove failed");
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkReflectsChangedComponentValues_Edit() => LiveLinkReflectsChangedComponentValues(false);

        [UnityTest, Explicit]
        public IEnumerator LiveLinkReflectsChangedComponentValues_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkReflectsChangedComponentValues(true, useDomainReload);

        IEnumerator LiveLinkReflectsChangedComponentValues(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                var subScene = CreateEmptySubScene("TestSubScene", true);

                var go = new GameObject("TestGameObject");
                var authoring = go.AddComponent<TestPrefabComponentAuthoring>();
                authoring.IntValue = 15;
                SceneManager.MoveGameObjectToScene(go, subScene.EditingScene);
            }

            yield return GetEnterPlayMode(usePlayMode);
            {
                var w = GetLiveLinkWorld(usePlayMode);
                var authoring = Object.FindObjectOfType<TestPrefabComponentAuthoring>();
                Assert.AreEqual(authoring.IntValue, 15);

                var testTagQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(15, testTagQuery.GetSingleton<TestPrefabComponent>().IntValue);

                Undo.RecordObject(authoring, "Change component value");
                authoring.IntValue = 2;

                // it takes an extra frame to establish that something has changed when using RecordObject unless Flush is called
                Undo.FlushUndoRecordObjects();

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(2, testTagQuery.GetSingleton<TestPrefabComponent>().IntValue, "Expected a component value to change");

                Undo.PerformUndo();

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(15, testTagQuery.GetSingleton<TestPrefabComponent>().IntValue, "Expected a component value to change, undo failed");

                Undo.PerformRedo();

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(2, testTagQuery.GetSingleton<TestPrefabComponent>().IntValue, "Expected a component value to change, redo failed");
            }
        }

        [UnityTest]
        public IEnumerator LiveLinkDisablesEntityWhenGameObjectIsDisabled_Edit() => LiveLinkDisablesEntityWhenGameObjectIsDisabled(false);

        [UnityTest, Explicit, Ignore("Doesn't currently work, since Scenes are never marked dirty in play mode. A reconversion is never triggered.")]
        public IEnumerator LiveLinkDisablesEntityWhenGameObjectIsDisabled_Play([Values] EnteringPlayMode useDomainReload) => LiveLinkDisablesEntityWhenGameObjectIsDisabled(true, useDomainReload);

        IEnumerator LiveLinkDisablesEntityWhenGameObjectIsDisabled(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateSubSceneFromObjects("TestSubScene", true, () =>
                {
                    var go = new GameObject("TestGameObject");
                    go.AddComponent<TestPrefabComponentAuthoring>();
                    return new List<GameObject> { go };
                });
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var w = GetLiveLinkWorld(usePlayMode);
                var queryWithoutDisabled = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(1, queryWithoutDisabled.CalculateEntityCount(), "Expected a game object to be converted");

                var go = Object.FindObjectOfType<TestPrefabComponentAuthoring>().gameObject;
                Undo.RecordObject(go, "DisableObject");
                go.SetActive(false);
                Undo.FlushUndoRecordObjects();

                w.Update();

                var queryWithDisabled = w.EntityManager.CreateEntityQuery(new EntityQueryDesc
                {
                    All = new[] { ComponentType.ReadWrite<TestPrefabComponent>(), ComponentType.ReadWrite<Disabled>() },
                    Options = EntityQueryOptions.IncludeDisabled
                });
                Assert.AreEqual(1, queryWithDisabled.CalculateEntityCount(), "Expected a game object to be converted and disabled");

                Assert.AreEqual(0, queryWithoutDisabled.CalculateEntityCount(), "Expected a game object to be converted and disabled");
            }
        }

        [UnityTest, Ignore("Requires refactoring to work in immutable package state, DOTS-1644")]
        public IEnumerator LiveLink_WithTextureDependency_ChangeCausesReconversion_Edit() => LiveLink_WithTextureDependency_ChangeCausesReconversion(false);
        [UnityTest, Explicit]
        public IEnumerator LiveLink_WithTextureDependency_ChangeCausesReconversion_Play([Values] EnteringPlayMode useDomainReload) => LiveLink_WithTextureDependency_ChangeCausesReconversion(true, useDomainReload);

        IEnumerator LiveLink_WithTextureDependency_ChangeCausesReconversion(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                EditorSceneManager.OpenScene(ScenePath("SceneWithTextureDependency"));
                OpenAllSubScenes();
            }

            yield return GetEnterPlayMode(usePlayMode);
            yield return null;

            {
                var w = GetLiveLinkWorld(usePlayMode);
                var testQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<ConversionDependencyData>());
                Assert.AreEqual(1, testQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.IsTrue(testQuery.GetSingleton<ConversionDependencyData>().HasTexture);
                Assert.AreEqual(m_TestTexture.filterMode, testQuery.GetSingleton<ConversionDependencyData>().TextureFilterMode, "Initial conversion reported the wrong value");

                m_TestTexture.filterMode = m_TestTexture.filterMode == FilterMode.Bilinear ? FilterMode.Point : FilterMode.Bilinear;
                AssetDatabase.SaveAssets();

                yield return null;

                Assert.AreEqual(1, testQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(m_TestTexture.filterMode, testQuery.GetSingleton<ConversionDependencyData>().TextureFilterMode, "Updated conversion shows the wrong value");
            }
        }

        [UnityTest, Ignore("Requires refactoring to work in immutable package state, DOTS-1644")]
        public IEnumerator LiveLink_WithMaterialDependency_ChangeCausesReconversion_Edit() => LiveLink_WithMaterialDependency_ChangeCausesReconversion(false);
        [UnityTest, Explicit]
        public IEnumerator LiveLink_WithMaterialDependency_ChangeCausesReconversion_Play([Values] EnteringPlayMode useDomainReload) => LiveLink_WithMaterialDependency_ChangeCausesReconversion(true, useDomainReload);

        IEnumerator LiveLink_WithMaterialDependency_ChangeCausesReconversion(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                EditorSceneManager.OpenScene(ScenePath("SceneWithMaterialDependency"));
                OpenAllSubScenes();
            }

            yield return GetEnterPlayMode(usePlayMode);
            yield return null;

            {
                var w = GetLiveLinkWorld(usePlayMode);
                var testQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<ConversionDependencyData>());
                Assert.AreEqual(1, testQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(m_TestMaterial.color, testQuery.GetSingleton<ConversionDependencyData>().MaterialColor);

                m_TestMaterial.color = m_TestMaterial.color == Color.blue ? Color.red : Color.blue;
                AssetDatabase.SaveAssets();

                yield return null;

                Assert.AreEqual(1, testQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(m_TestMaterial.color, testQuery.GetSingleton<ConversionDependencyData>().MaterialColor, "The game object with the asset dependency has not been reconverted");
            }
        }

        [UnityTest, Ignore("Requires refactoring to work in immutable package state, DOTS-1644")]
        public IEnumerator LiveLink_WithMultipleScenes_WithAssetDependencies_ChangeCausesReconversion_Edit() => LiveLink_WithMultipleScenes_WithAssetDependencies_ChangeCausesReconversion(false);
        [UnityTest, Explicit]
        public IEnumerator LiveLink_WithMultipleScenes_WithAssetDependencies_ChangeCausesReconversion_Play([Values] EnteringPlayMode useDomainReload) => LiveLink_WithMultipleScenes_WithAssetDependencies_ChangeCausesReconversion(true, useDomainReload);

        IEnumerator LiveLink_WithMultipleScenes_WithAssetDependencies_ChangeCausesReconversion(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                EditorSceneManager.OpenScene(ScenePath("SceneWithMaterialDependency"));
                EditorSceneManager.OpenScene(ScenePath("SceneWithTextureDependency"), OpenSceneMode.Additive);
                OpenAllSubScenes();
            }

            yield return GetEnterPlayMode(usePlayMode);
            yield return null;

            {
                var w = GetLiveLinkWorld(usePlayMode);
                var testQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<ConversionDependencyData>());
                Assert.AreEqual(2, testQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Entity textureEntity, materialEntity;
                using (var entities = testQuery.ToEntityArray(Allocator.TempJob))
                {
                    if (GetData(entities[0]).HasMaterial)
                    {
                        materialEntity = entities[0];
                        textureEntity = entities[1];
                    }
                    else
                    {
                        materialEntity = entities[1];
                        textureEntity = entities[0];
                    }
                }

                Assert.AreEqual(m_TestMaterial.color, GetData(materialEntity).MaterialColor);
                Assert.AreEqual(m_TestTexture.filterMode, GetData(textureEntity).TextureFilterMode);

                m_TestMaterial.color = m_TestMaterial.color == Color.blue ? Color.red : Color.blue;
                AssetDatabase.SaveAssets();

                yield return null;

                Assert.AreEqual(m_TestMaterial.color, GetData(materialEntity).MaterialColor, "The game object with the material asset dependency has not been reconverted");

                m_TestTexture.filterMode = m_TestTexture.filterMode == FilterMode.Bilinear ? FilterMode.Point : FilterMode.Bilinear;
                AssetDatabase.SaveAssets();

                yield return null;

                Assert.AreEqual(m_TestTexture.filterMode, GetData(textureEntity).TextureFilterMode, "The game object with the texture asset dependency has not been reconverted.");

                ConversionDependencyData GetData(Entity e) => w.EntityManager.GetComponentData<ConversionDependencyData>(e);
            }
        }

        // TODO: DISABLED until engine/trunk fix comes
        [UnityTest, Explicit]
        public IEnumerator FOO_LiveLink_LoadAndUnload_WithChanges_Edit() => LiveLink_LoadAndUnload_WithChanges(false);
        [UnityTest, Explicit]
        public IEnumerator LiveLink_LoadAndUnload_WithChanges_Play([Values] EnteringPlayMode useDomainReload) => LiveLink_LoadAndUnload_WithChanges(true, useDomainReload);

        IEnumerator LiveLink_LoadAndUnload_WithChanges(bool usePlayMode, EnteringPlayMode useDomainReload = EnteringPlayMode.WithoutDomainReload)
        {
            {
                SetDomainReload(useDomainReload);
                CreateSubSceneFromObjects("TestSubScene", true, () =>
                {
                    var go = new GameObject("TestGameObject");
                    var authoring = go.AddComponent<TestPrefabComponentAuthoring>();
                    authoring.Material = m_TestMaterial;
                    authoring.IntValue = 15;

                    return new List<GameObject> { go };
                });
            }

            yield return GetEnterPlayMode(usePlayMode);

            {
                var authoring = Object.FindObjectOfType<TestPrefabComponentAuthoring>();
                Assert.AreEqual(authoring.IntValue, 15);
                Assert.AreEqual(authoring.Material, m_TestMaterial);

                var w = GetLiveLinkWorld(usePlayMode);
                var testTagQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<TestPrefabComponent>());
                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(15, testTagQuery.GetSingleton<TestPrefabComponent>().IntValue);

                var testSceneQuery = w.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<SceneReference>());
                Assert.AreEqual(1, testSceneQuery.CalculateEntityCount());

                Undo.RecordObject(authoring, "Change component value");
                authoring.IntValue = 2;

                // it takes an extra frame to establish that something has changed when using RecordObject unless Flush is called
                Undo.FlushUndoRecordObjects();

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted");
                Assert.AreEqual(2, testTagQuery.GetSingleton<TestPrefabComponent>().IntValue, "Expected a component value to change");

                var subScene = Object.FindObjectOfType<SubScene>();
                Assert.IsNotNull(subScene);

                subScene.gameObject.SetActive(false);
                yield return null;
                Assert.AreEqual(0, testSceneQuery.CalculateEntityCount(), "Expected no Scene Entities after disabling the SubScene MonoBehaviour");

                subScene.gameObject.SetActive(true);
                yield return null;
                Assert.AreEqual(1, testSceneQuery.CalculateEntityCount(), "Expected Scene Entity after enabling the SubScene MonoBehaviour");

                // Do conversion again
                Undo.RecordObject(authoring, "Change component value");
                authoring.IntValue = 42;

                // it takes an extra frame to establish that something has changed when using RecordObject unless Flush is called
                Undo.FlushUndoRecordObjects();

                yield return null;

                Assert.AreEqual(1, testTagQuery.CalculateEntityCount(), "Expected a game object to be converted after unloading and loading subscene");
                Assert.AreEqual(42, testTagQuery.GetSingleton<TestPrefabComponent>().IntValue, "Expected a component value to change after unloading and loading subscene");
            }
        }
    }
}

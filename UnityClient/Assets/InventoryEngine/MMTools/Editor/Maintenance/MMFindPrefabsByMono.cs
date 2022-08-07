#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    /// <summary>
    /// As static class that lets you look for missing scripts on any prefab in your project, or for prefabs equipped with a certain type of MonoBehaviour
    /// </summary>
    public class MMFindPrefabsByMono : EditorWindow
    {
        protected Vector2 _scrollView;
        protected string[] _tabs = new string[] { "Find prefabs with missing components", "Find prefabs by MonoBehaviour" };
        protected int _selectedTab;
        protected int _lastSelectedTab = -1;
        protected MonoScript _searchedMonoBehaviour;
        protected MonoScript _lastSearchedMonoBehaviour;
        protected string _searchedMonoBehaviourName = "";
        protected List<string> _resultsList;

        static GUIStyle _padded;
        static GUIStyle _horizontalPadded;
        static int _horizontalPadding = 20;
        static int _verticalPadding = 20;
        static RectOffset _padding;
        static RectOffset _horizontalPaddingOnly;

        /// <summary>
        /// Menu bound method
        /// </summary>
        [MenuItem("Tools/More Mountains/Prefab Finder", false, 504)]
        public static void MenuAction()
        {
            OpenWindow();
        }

        /// <summary>
        /// Opens and resizes the window
        /// </summary>
        public static void OpenWindow()
        {
            InitializePaddingAndStyles();
            MMFindPrefabsByMono window = (MMFindPrefabsByMono)EditorWindow.GetWindow(typeof(MMFindPrefabsByMono));
            window.position = new Rect(400, 400, 800, 600);
            window.titleContent = new GUIContent("MM Prefabs Finder");
            window.Show();
        }

        /// <summary>
        /// Initializes padding variables and GUI styles
        /// </summary>
        static void InitializePaddingAndStyles()
        {
            if (_padding == null)
            {
                _padding = new RectOffset(_horizontalPadding, _horizontalPadding, _verticalPadding, _verticalPadding);
                _horizontalPaddingOnly = new RectOffset(_horizontalPadding, _horizontalPadding, 0, 0);
                _padded = new GUIStyle
                {
                    name = "padded",
                    padding = _padding
                };
                _horizontalPadded = new GUIStyle
                {
                    name = "horizontalPadded",
                    padding = _horizontalPaddingOnly
                };
            }                 
        }

        /// <summary>
        /// Draws tab buttons
        /// </summary>
        protected virtual void DrawTabs()
        {
            GUI.skin.box.padding = _padding;
            GUILayout.BeginHorizontal("box");
                GUILayout.Space(10);            
                _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Detects changes in tabs selection
        /// </summary>
        protected virtual void HandleTabsChange()
        {
            if (_lastSelectedTab != _selectedTab)
            {
                _lastSelectedTab = _selectedTab;
                _resultsList = new List<string>();
                _searchedMonoBehaviourName = _searchedMonoBehaviour == null ? "" : _searchedMonoBehaviour.name;
                _lastSearchedMonoBehaviour = null;
            }
        }

        /// <summary>
        /// Draws the content of the selected tab
        /// </summary>
        protected virtual void DrawSelectedTab()
        {
            switch (_selectedTab)
            {
                case 0:
                    DrawSearchMissing();
                    break;
                case 1:
                    DrawSearchByMonoBehaviour();
                    break;
            }
        }

        /// <summary>
        /// Draws the search by mono form
        /// </summary>
        protected virtual void DrawSearchByMonoBehaviour()
        {
            GUILayout.BeginHorizontal("box");
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                    GUILayout.Label("Select a MonoBehaviour to search for:");
                    _searchedMonoBehaviour = (MonoScript)EditorGUILayout.ObjectField(_searchedMonoBehaviour, typeof(MonoScript), false);
                GUILayout.EndVertical();
                GUILayout.Space(10);

                if (_searchedMonoBehaviour != _lastSearchedMonoBehaviour)
                {
                    string[] allPrefabsInProject = GetAllPrefabsInProject();

                    _lastSearchedMonoBehaviour = _searchedMonoBehaviour;
                    _searchedMonoBehaviourName = _searchedMonoBehaviour.name;
                    AssetDatabase.SaveAssets();
                    string searchedMonoBehaviourPath = AssetDatabase.GetAssetPath(_searchedMonoBehaviour);
                    _resultsList = new List<string>();
                    foreach (string prefab in allPrefabsInProject)
                    {
                        string[] pathName = new string[] { prefab };
                        string[] monoDependenciesPaths = AssetDatabase.GetDependencies(pathName, false);
                        foreach (string monoDependencyPath in monoDependenciesPaths)
                        {
                            if (monoDependencyPath == searchedMonoBehaviourPath)
                            {
                                _resultsList.Add(prefab);
                            }
                        }
                    }
                }
            GUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Draws the search missing form
        /// </summary>
        protected virtual void DrawSearchMissing()
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(20);
            if (GUILayout.Button("Search the project for prefabs with missing scripts"))
            {
                string[] allPrefabs = GetAllPrefabsInProject();
                _resultsList = new List<string>();
                foreach (string prefab in allPrefabs)
                {
                    UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(prefab);
                    GameObject assetGameObject;
                    try
                    {
                        assetGameObject = (GameObject)asset;
                        Component[] components = assetGameObject.GetComponentsInChildren<Component>(true);
                        foreach (Component component in components)
                        {
                            if (component == null)
                            {
                                _resultsList.Add(prefab);
                            }
                        }
                    }
                    catch
                    {
                        Debug.Log("An error occured with prefab " + prefab);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the result list
        /// </summary>
        protected virtual void DrawResultsList()
        {
            GUILayout.BeginHorizontal(_padded); 
            if (_resultsList != null)
            {
                if (_resultsList.Count == 0)
                {
                    switch (_selectedTab)
                    {
                        case 0:
                            GUILayout.Label("No prefabs have missing components.", EditorStyles.boldLabel);
                            break;

                        case 1:
                            if (_searchedMonoBehaviourName != "")
                            {
                                GUILayout.Label("No prefabs use component " + _searchedMonoBehaviourName, EditorStyles.boldLabel);
                            }                            
                            break;
                    }
                    GUILayout.EndHorizontal(); // end padded
                }
                else
                {
                    switch (_selectedTab)
                    {
                        case 0:
                            GUILayout.Label("These prefabs have missing components :", EditorStyles.boldLabel);
                            break;

                        case 1:
                            GUILayout.Label("MonoBehaviour " + _searchedMonoBehaviourName + " was found in these prefabs :", EditorStyles.boldLabel);
                            break;
                    }
                    GUILayout.EndHorizontal(); // end padded

                    GUILayout.BeginHorizontal();
                    GUI.skin.scrollView.padding = _padding;
                    _scrollView = GUILayout.BeginScrollView(_scrollView);
                    foreach (string s in _resultsList)
                    {
                        GUILayout.BeginHorizontal(_horizontalPadded);
                        GUILayout.Label(s, GUILayout.Width(4 * (position.width - 4 * _horizontalPadding) / 5));
                        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                        if (GUILayout.Button("Select prefab", GUILayout.Width((position.width - 4 * _horizontalPadding) / 5 - 20)))
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(s);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndHorizontal();
                }
            }
        }
        
        /// <summary>
        /// On GUI we draw our window's contents
        /// </summary>
        protected virtual void OnGUI()
        {
            InitializePaddingAndStyles();
            DrawTabs();
            HandleTabsChange();
            DrawSelectedTab();
            DrawResultsList();
        }

        /// <summary>
        /// Gets all prefabs and sorts them alphabetically
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllPrefabsInProject()
        {
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
            List<string> results = new List<string>();
            foreach (string assetPath in assetPaths)
            {
                if (assetPath.Contains(".prefab"))
                {
                    results.Add(assetPath);
                }
            }
            results.Sort();
            return results.ToArray();
        }
    }
}
#endif
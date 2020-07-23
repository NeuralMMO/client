using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unity.QuickSearch
{
    class CustomItemSelectionWindow : DropdownWindow<CustomItemSelectionWindow>
    {
        public class Config
        {
            public bool initialFocus;
            public Action<TreeView, TreeViewItem, object, string, Rect, int, bool, bool> drawRow;
            public Func<string, string, string> drawSearchField;
            public bool showSearchField;
            public Action<TreeView> listInit;
            public float rowWidth;
            public float rowHeight;
            public float windowHeight;
            public IList models;
            public Action<int> elementSelectedHandler;

            public Config(IList models, Action<int> elementSelectedHandler)
            {
                showSearchField = true;
                initialFocus = true;
                this.models = models;
                this.elementSelectedHandler = elementSelectedHandler;
                windowHeight = 200;
                rowWidth = 200;
            }
        }

        const string k_SearchField = "CustomItemSelectionWindowSearchField";

        private Action<int> m_ElementSelectedHandler;
        private CustomItemListView m_ListView;
        private bool m_NeedFocus;
        private string m_SearchValue;
        private Config m_Config;

        [UsedImplicitly]
        protected override void OnEnable()
        {
            base.OnEnable();
            m_NeedFocus = true;
        }

        public static void SelectionButton(Rect rect, GUIContent content, GUIStyle style, Func<Config> getConfig)
        {
            DropDownButton(rect, content, style, () => SetupWindow(getConfig()));
        }

        public static void CheckShowWindow(Rect rect, Func<Config> getConfig)
        {
            CheckShowWindow(rect, () => SetupWindow(getConfig()));
        }

        [UsedImplicitly]
        internal void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Escape)
                {
                    Event.current.Use();
                    Close();
                    m_ElementSelectedHandler(-1);
                }
                else if (Event.current.keyCode == KeyCode.DownArrow &&
                    GUI.GetNameOfFocusedControl() == k_SearchField)
                {
                    m_ListView.SetFocusAndEnsureSelectedItem();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.UpArrow &&
                    m_ListView.HasFocus() &&
                    m_ListView.IsFirstItemSelected())
                {
                    EditorGUI.FocusTextInControl(k_SearchField);
                    Event.current.Use();
                }
            }

            if (m_Config.showSearchField)
            {
                EditorGUI.BeginChangeCheck();
                if (m_Config.drawSearchField != null)
                {
                    m_SearchValue = m_Config.drawSearchField(k_SearchField, m_SearchValue);
                }
                else
                {
                    GUI.SetNextControlName(k_SearchField);
                    m_SearchValue = SearchField(m_SearchValue);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    m_ListView.searchString = m_SearchValue;
                }
                if (m_NeedFocus)
                {
                    m_NeedFocus = false;
                    GUI.FocusControl(k_SearchField);
                }
            }
            else if (m_NeedFocus)
            {
                m_NeedFocus = false;
                m_ListView.SetFocusAndEnsureSelectedItem();
            }
            
            var rect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandHeight(true));
            m_ListView.OnGUI(rect);
        }

        private static CustomItemSelectionWindow SetupWindow(Config config)
        {
            if (config != null)
            {
                var window = CreateInstance<CustomItemSelectionWindow>();
                window.position = new Rect(window.position.x, window.position.y, config.rowWidth, config.windowHeight);
                window.InitWindow(config);
                return window;
            }

            return null;
        }

        private void InitWindow(Config config)
        {
            m_Config = config;
            m_NeedFocus = config.initialFocus;
            m_ElementSelectedHandler = config.elementSelectedHandler;
            m_ListView = new CustomItemListView(config.models, config.rowHeight, config.drawRow);
            config.listInit?.Invoke(m_ListView);
            m_ListView.elementActivated += OnElementActivated;
        }

        private void OnElementActivated(int indexSelected)
        {
            Close();
            m_ElementSelectedHandler?.Invoke(indexSelected);
        }

        static MethodInfo ToolbarSearchField;
        private static string SearchField(string value, params GUILayoutOption[] options)
        {
            if (ToolbarSearchField == null)
            {
                ToolbarSearchField = typeof(EditorGUILayout).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(mi => mi.Name == "ToolbarSearchField" && mi.GetParameters().Length == 2);
            }

            return ToolbarSearchField.Invoke(null, new[] { value, (object)options }) as string;
        }
    }
}
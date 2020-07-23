using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    class ListSelectionWindow : DropdownWindow<ListSelectionWindow>
    {
        private Action<int> m_ElementSelectedHandler;
        private StringListView m_ListView;
        private string m_SearchValue;
        private bool m_SearchFieldGiveFocus;
        const string k_SearchField = "ListSearchField";

        [UsedImplicitly]
        protected override void OnEnable()
        {
            base.OnEnable();
            m_SearchFieldGiveFocus = true;
        }

        public static void SelectionButton(Rect rect, Vector2 windowSize, string content, GUIStyle style, string[] models, Action<int> elementSelectedHandler)
        {
            SelectionButton(rect, windowSize, new GUIContent(content), style, models, elementSelectedHandler);
        }

        public static void SelectionButton(Rect rect, Vector2 windowSize, GUIContent content, GUIStyle style, string[] models, Action<int> elementSelectedHandler)
        {
            DropDownButton(rect, content, style, () =>
            {
                var window = CreateInstance<ListSelectionWindow>();
                window.InitWindow(content.text, models, elementSelectedHandler);
                window.position = new Rect(window.position.position, windowSize);
                return window;
            });
        }

        [UsedImplicitly]
        internal void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                    m_ElementSelectedHandler(-1);
                }
                else  if ( Event.current.keyCode == KeyCode.DownArrow &&
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

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(k_SearchField);
            m_SearchValue = SearchField(m_SearchValue);
            if (EditorGUI.EndChangeCheck())
            {
                m_ListView.searchString = m_SearchValue;
            }
            if (m_SearchFieldGiveFocus)
            {
                m_SearchFieldGiveFocus = false;
                GUI.FocusControl(k_SearchField);
            }

            var rect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandHeight(true));
            m_ListView.OnGUI(rect);
        }

        private void InitWindow(string value, string[] models, Action<int> elementSelectedHandler)
        {
            m_ElementSelectedHandler = elementSelectedHandler;
            m_ListView = new StringListView(value, models);
            m_ListView.elementActivated += OnElementActivated;
        }

        private void OnElementActivated(int indexSelected)
        {
            Close();
            m_ElementSelectedHandler.Invoke(indexSelected);
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
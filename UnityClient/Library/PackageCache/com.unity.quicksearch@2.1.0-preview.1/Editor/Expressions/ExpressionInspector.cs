using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.QuickSearch
{
    class ExpressionInspector : IMGUIContainer, IExpressionInspector
    {
        enum ValueType
        {
            Boolean,
            Number,
            String,
            Asset,
            Default = String
        }

        static class Styles
        {
            public static readonly string[] constantTypes = new string[]
            {
                nameof(ValueType.Boolean),
                nameof(ValueType.Number),
                nameof(ValueType.String),
                nameof(ValueType.Asset)
            };

            public static readonly GUIStyle variableLabel = new GUIStyle(EditorStyles.label)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 2)
            };

            public static readonly GUIStyle toggle = new GUIStyle(EditorStyles.toggle)
            {
                margin = new RectOffset(0, 0, 6, 4),
                padding = new RectOffset(18, 0, -2, 0)
            };

            public static readonly GUIStyle selector = new GUIStyle(EditorStyles.miniPullDown)
            {
                richText = true
            };

            public static readonly float scrollbarWidth = new GUIStyle("VerticalScrollbar").fixedWidth;
        }

        private SearchExpressionNode m_Node;
        private readonly List<string> m_VariablesList = new List<string>();
        private readonly ReorderableList m_VariablesReorderableList;
        private Vector2 m_ScrollPostion;
        private bool resetHeight = true;
        private float m_ContentHeight = 0f;
        private string[] m_ExpressionPaths = new string[0];

        private bool hasScrollbar => m_ContentHeight > contentRect.height;
        private float scrollbarWidth => hasScrollbar ? Styles.scrollbarWidth : 0f;
        private float maxContentWidth => contentRect.width - scrollbarWidth - 6f;

        public event Action<SearchExpressionNode> propertiesChanged;
        public event Action<SearchExpressionNode, string> variableAdded;
        public event Action<SearchExpressionNode, string> variableRemoved;
        public event Action<SearchExpressionNode, string, string> variableRenamed;

        public ExpressionInspector()
        {
            style.overflow = Overflow.Hidden;
            onGUIHandler = OnGUI;
            m_VariablesReorderableList = new ReorderableList(m_VariablesList, typeof(string), true, false, true, true);
            m_VariablesReorderableList.onAddCallback += OnAddVariable;
            m_VariablesReorderableList.onRemoveCallback += OnRemoveVariable;
            m_VariablesReorderableList.drawHeaderCallback += OnDrawVariablesHeader;
            m_VariablesReorderableList.drawElementCallback += OnDrawVariable;
        }

        protected override void Dispose(bool dispose)
        {
            m_VariablesReorderableList.onAddCallback -= OnAddVariable;
            m_VariablesReorderableList.onRemoveCallback -= OnRemoveVariable;
            m_VariablesReorderableList.drawHeaderCallback -= OnDrawVariablesHeader;
            m_VariablesReorderableList.drawElementCallback -= OnDrawVariable;

            base.Dispose(dispose);
        }

        private void OnDrawVariable(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (isActive && (isFocused || GUIUtility.keyboardControl != 0))
            {
                using (var has = new EditorGUI.ChangeCheckScope())
                {
                    var textFieldRect = rect;
                    textFieldRect.y += 1;
                    textFieldRect.height -= 2;
                    var oldValue = m_VariablesList[index];
                    m_VariablesList[index] = GUI.TextField(textFieldRect, oldValue);
                    if (has.changed)
                    {
                        m_Node.RenameVariable(oldValue, m_VariablesList[index]);
                        variableRenamed?.Invoke(m_Node, oldValue, m_VariablesList[index]);
                    }
                }
            }
            else
                GUI.Label(rect, m_VariablesList[index], Styles.variableLabel);
        }

        private void OnDrawVariablesHeader(Rect rect)
        {
            GUI.Label(rect, "Variables");
        }

        private void OnAddVariable(ReorderableList list)
        {
            int idx = m_VariablesList.Count+1;
            var uniqueVarName = $"var{idx++}";
            while (m_VariablesList.Contains(uniqueVarName))
                uniqueVarName = $"var{idx++}";
            m_VariablesList.Add(uniqueVarName);
            var newVar = m_Node.AddVariable(uniqueVarName, null);
            variableAdded?.Invoke(m_Node, newVar.name);
            resetHeight = true;
        }

        private void OnRemoveVariable(ReorderableList list)
        {
            var varName = m_VariablesList[m_VariablesReorderableList.index];
            if (m_VariablesList.Remove(varName))
            {
                if (m_Node.RemoveVariable(varName) >= 1)
                {
                    variableRemoved?.Invoke(m_Node, varName);
                    resetHeight = true;
                }
            }
        }

        private void DrawNameEditor()
        {
            var name = EditorGUILayout.TextField("Name", m_Node.name, GUILayout.ExpandWidth(true));
            if (name != m_Node.name)
            {
                if (!String.IsNullOrEmpty(name))
                    m_Node.name = name;
                else
                    m_Node.name = null;
                propertiesChanged?.Invoke(m_Node);
                resetHeight = true;
            }
        }

        private void DrawCommonControls()
        {
            var newColor = EditorGUILayout.ColorField("Color", m_Node.color);
            if (newColor != m_Node.color)
            {
                m_Node.color = newColor;
                propertiesChanged?.Invoke(m_Node);
                resetHeight = true;
            }
        }

        private void OnGUI()
        {
            if (m_Node == null)
            {
                CollapseEditor();
                return;
            }

            using (var scope = new EditorGUILayout.ScrollViewScope(m_ScrollPostion))
            using (new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(maxContentWidth)))
            {
                EditorGUIUtility.labelWidth = 80f;

                DrawCommonControls();

                switch (m_Node.type)
                {
                    case ExpressionType.Map:
                        DrawNameEditor();
                        DrawMapEditor();
                        break;

                    case ExpressionType.Search:
                        DrawNameEditor();
                        DrawSearchEditor();
                        break;

                    case ExpressionType.Value:
                        DrawNameEditor();
                        DrawValueEditor();
                        break;

                    case ExpressionType.Provider:
                        DrawProviderEditor();
                        break;

                    case ExpressionType.Union:
                    case ExpressionType.Intersect:
                    case ExpressionType.Except:
                    case ExpressionType.Results:
                        DrawResultsEditor();
                        break;

                    case ExpressionType.Select:
                        DrawSelectEditor();
                        break;

                    case ExpressionType.Expression:
                        DrawNameEditor();
                        DrawNestedExpressionEditor();
                        break;

                    default:
                        throw new NotSupportedException($"No inspector for {m_Node.type} {m_Node.id}");
                }

                ResizeEditor();
                m_ScrollPostion = scope.scrollPosition;
            }
        }

        private void DrawNestedExpressionEditor()
        {
            string selectLabel = m_Node.value != null ? Convert.ToString(m_Node.value) : null;
            EditorGUILayout.BeginHorizontal();
            DrawSelectionPopup("Expression", selectLabel ?? "Select expression...", m_ExpressionPaths, selectedIndex =>
            {
                m_Node.source = null;
                m_Node.value = m_ExpressionPaths[selectedIndex];
                propertiesChanged?.Invoke(m_Node);
                resetHeight = true;
            });
            if (GUILayout.Button("Clear"))
            {
                m_Node.value = m_Node.source = null;
                propertiesChanged?.Invoke(m_Node);
                resetHeight = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectEditor()
        {
            var labels = ExpressionSelectors.names;
            var selectType = m_Node.value as string;

            DrawSelectionPopup("Select", selectType ?? "Choose select type...", labels, selectedIndex =>
            {
                selectType = labels[selectedIndex];
                m_Node.value = selectType;
                propertiesChanged?.Invoke(m_Node);
                resetHeight = true;
            }, 0f);

            if (selectType == null || !ExpressionSelectors.Draw(selectType, this))
                GUILayout.Space(40f);
        }

        public void DrawSelectionPopup(string label, string value, IEnumerable<string> choices, Action<int> selectedHandler, float extraWidth = 200f)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(79));
            var btnRect = EditorGUILayout.GetControlRect(true, Styles.selector.fixedHeight, Styles.selector);
            var dropdownSize = new Vector2(btnRect.width + extraWidth, 300);
            ListSelectionWindow.SelectionButton(btnRect, dropdownSize, value, Styles.selector, choices.ToArray(), selectedIndex =>
            {
                if (selectedIndex != -1)
                    selectedHandler?.Invoke(selectedIndex);
            });
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProviderEditor()
        {
            var providerNames = new string []{"Custom", ""}
                .Concat(SearchService.Providers.Select(p => p.name.displayName)).ToArray();
            var selectedIndex = SearchService.Providers.FindIndex(p => p.name.id == (string)m_Node.value);
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup("Provider", selectedIndex+2, providerNames)-2;
            if (EditorGUI.EndChangeCheck())
            {
                if (selectedIndex < 0)
                    m_Node.value = null;
                else
                    m_Node.value = SearchService.Providers.ElementAt(selectedIndex).name.id;

                propertiesChanged?.Invoke(m_Node);
                resetHeight = true;
            }

            if (selectedIndex < 0)
            {
                var customValue = EditorGUILayout.TextField("Source", Convert.ToString(m_Node.value), GUILayout.ExpandWidth(true));
                if (!customValue.Equals(m_Node.value))
                {
                    if (!String.IsNullOrEmpty(customValue))
                        m_Node.value = customValue;
                    else
                        m_Node.value = null;
                    propertiesChanged?.Invoke(m_Node);
                    resetHeight = true;
                }
            }
            else
                GUILayout.Space(20);
        }

        private void DrawValueEditor()
        {
            using (new EditorGUILayout.HorizontalScope())
            using (var has = new EditorGUI.ChangeCheckScope())
            {
                var selectedType = ValueType.Default;
                var booleanValue = false;

                // Parse value;
                var stringValue = Convert.ToString(m_Node.value);
                if (Utils.TryGetNumber(m_Node.value, out var number))
                    selectedType = ValueType.Number;
                else if ("true".Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                {
                    selectedType = ValueType.Boolean;
                    booleanValue = true;
                }
                else if ("false".Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                {
                    selectedType = ValueType.Boolean;
                    booleanValue = false;
                }
                else if (File.Exists(stringValue) || Directory.Exists(stringValue))
                {
                    selectedType = ValueType.Asset;
                }

                // Display value controls
                var newSelectedType = (ValueType)EditorGUILayout.Popup((int)selectedType, Styles.constantTypes, GUILayout.Width(80f));
                if (newSelectedType == ValueType.Number)
                {
                    if (selectedType != newSelectedType)
                        number = 0;
                    EditorGUIUtility.labelWidth = 70f;
                    number = EditorGUILayout.DoubleField("Value", number, GUILayout.ExpandWidth(true));
                    m_Node.value = number;
                }
                else if (newSelectedType == ValueType.String)
                {
                    if (selectedType != newSelectedType)
                        m_Node.value = "";

                    var newValue = EditorGUILayout.DelayedTextField(Convert.ToString(m_Node.value), GUILayout.ExpandWidth(true));
                    if (double.TryParse(newValue, out number))
                        m_Node.value = number;
                    else
                        m_Node.value = newValue;
                }
                else if (newSelectedType == ValueType.Boolean)
                {
                    if (selectedType != newSelectedType)
                        m_Node.value = booleanValue;

                    var newValue = GUILayout.Toggle(booleanValue, "Value", Styles.toggle, GUILayout.ExpandWidth(true));
                    if (booleanValue != newValue)
                        m_Node.value = newValue;
                }
                else if (newSelectedType == ValueType.Asset)
                {
                    if (selectedType != newSelectedType)
                        m_Node.value = "Assets";

                    var constantAsset = AssetDatabase.LoadMainAssetAtPath((string)m_Node.value);
                    var selectedAsset = EditorGUILayout.ObjectField(constantAsset, typeof(UnityEngine.Object), false);
                    m_Node.value = AssetDatabase.GetAssetPath(selectedAsset);
                }

                if (has.changed)
                {
                    propertiesChanged?.Invoke(m_Node);
                    resetHeight = true;
                }
            }
        }

        private void DrawResultsEditor()
        {
            if (!resetHeight)
                GUILayout.FlexibleSpace();
            GUILayout.Label("Expression results");
        }

        private void DrawMapEditor()
        {
            var mapping = (Mapping)m_Node.GetProperty(nameof(Mapping), (int)Mapping.Count);
            var groupBy = m_Node.GetProperty(ExpressionKeyName.GroupBy, "");
            EditorGUI.BeginChangeCheck();
            mapping = (Mapping)EditorGUILayout.EnumPopup(nameof(Mapping), mapping);
            if (m_Node.TryGetVariableSource(ExpressionKeyName.X, out var xSource) && xSource != null && mapping != Mapping.Table)
                groupBy = EditorGUILayout.DelayedTextField("Group By", groupBy);
            else
                GUILayout.Space(20);
            if (EditorGUI.EndChangeCheck())
            {
                m_Node.SetProperty(nameof(Mapping), (int)mapping);
                m_Node.SetProperty(ExpressionKeyName.GroupBy, groupBy);
                propertiesChanged?.Invoke(m_Node);
            }
        }

        private void DrawSearchEditor()
        {
            using (var has = new EditorGUI.ChangeCheckScope())
            {
                m_Node.value = EditorGUILayout.TextField("Query", (string)m_Node.value, GUILayout.MaxWidth(maxContentWidth));
                if (has.changed)
                {
                    propertiesChanged?.Invoke(m_Node);
                    resetHeight = true;
                }
            }
            m_VariablesReorderableList.DoLayoutList();
        }

        private void CollapseEditor()
        {
            // This will collapse the editor view so it doesn't take empty space for nothing.
            ResizeEditor(0f);
        }

        private void ResizeEditor(float forcedSize = -1f)
        {
            var lastRect = forcedSize < 0f ? GUILayoutUtility.GetLastRect() : new Rect(0, 0, 0, forcedSize);
            var isRepaintEvent = Event.current.type == EventType.Repaint;
            if (isRepaintEvent)
                m_ContentHeight = lastRect.yMax;

            if (!resetHeight || !isRepaintEvent)
                return;

            var newHeight = lastRect.yMax + 4f;
            style.height = newHeight;
            resetHeight = false;
            MarkDirtyRepaint();
        }

        public void SetSelection(SearchExpressionNode node)
        {
            if (node == m_Node)
                return;

            m_Node = node;
            if (m_Node == null)
                return;

            m_VariablesList.Clear();

            if (m_Node.type == ExpressionType.Search)
            {
                if (m_Node.variables != null)
                    m_VariablesList.AddRange(m_Node.variables.Select(v => v.name));
            }

            m_ExpressionPaths = AssetDatabase.FindAssets($"t:{nameof(SearchExpressionAsset)}").Select(AssetDatabase.GUIDToAssetPath).ToArray();

            resetHeight = true;
            MarkDirtyRepaint();
        }

        public void ClearSelection()
        {
            resetHeight = true;
            m_Node = null;
            m_VariablesList.Clear();
            MarkDirtyRepaint();
        }

        public T GetProperty<T>(string name, T defaultValue)
        {
            if (m_Node == null)
                throw new ExpressionException($"Cannot get property {name} for invalid node");
            return m_Node.GetProperty(name, defaultValue);
        }

        public int GetProperty(string name, int defaultValue)
        {
            if (m_Node == null)
                throw new ExpressionException($"Cannot get property {name} for invalid node");
            return m_Node.GetProperty(name, defaultValue);
        }

        public void SetProperty(string name, object value)
        {
            if (m_Node == null)
                throw new ExpressionException($"Cannot set property {name} for invalid node");

            m_Node.SetProperty(name, value);
            propertiesChanged?.Invoke(m_Node);
            resetHeight = true;
        }
    }
}

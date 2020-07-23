using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace Unity.Entities.Editor
{
    class EntityIMGUIVisitor : PropertyVisitor
    {
        const int kBufferPageLength = 5;

        class ScrollData
        {
            public float PageHeight;
            int m_Page;
            int m_Count;

            public int Page
            {
                get => m_Page;
                set => m_Page = math.max(0, math.min(value, LastPage));
            }

            public int Count
            {
                get => m_Count;
                set
                {
                    m_Count = value;
                    Page = math.min(Page, LastPage);
                }
            }

            public int LastPage => (Count - 1) / kBufferPageLength;
        }

        class Styles
        {
            public GUIContent PageLabel { get; }

            public Styles()
            {
                PageLabel = new GUIContent(L10n.Tr("Page"), L10n.Tr("Use the slider to navigate pages of buffer elements"));
            }
        }

        static Styles s_Styles;

        readonly Dictionary<int, ScrollData> m_ScrollData = new Dictionary<int, ScrollData>();

        public EntityIMGUIVisitor(SelectEntityButtonCallback selectEntityButtonCallback, ResolveEntityNameCallback resolveEntityNameCallback)
        {
            AddAdapter(new IMGUIAdapter(selectEntityButtonCallback, resolveEntityNameCallback));
        }

        protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            if (IsContainerType(ref value))
            {
                if (typeof(TContainer) == typeof(EntityContainer))
                {
                    var enabled = GUI.enabled;
                    GUI.enabled = true;
                    EditorGUILayout.LabelField(property.Name, new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.Bold });
                    GUI.enabled = enabled;
                }
                else
                {
                    EditorGUILayout.LabelField(IMGUIAdapter.GetDisplayName(property));
                }

                if (typeof(IComponentData).IsAssignableFrom(typeof(TValue)) && TypeManager.IsZeroSized(TypeManager.GetTypeIndex<TValue>()))
                {
                    return;
                }

                EditorGUI.indentLevel++;
                base.VisitProperty(property, ref container, ref value);
                EditorGUI.indentLevel--;
            }
            else
            {
                base.VisitProperty(property, ref container, ref value);
            }
        }

        protected override void VisitCollection<TContainer, TCollection, TElement>(Property<TContainer, TCollection> property, ref TContainer container, ref TCollection value)
        {
            if (typeof(TContainer) == typeof(EntityContainer))
            {
                var enabled = GUI.enabled;
                GUI.enabled = true;
                EditorGUILayout.LabelField(property.Name, new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.Bold });
                GUI.enabled = enabled;
            }
            else
            {
                EditorGUILayout.LabelField(IMGUIAdapter.GetDisplayName(property));
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.IntField("Size", value.Count);

            property.Visit(this, ref value);

            EditorGUI.indentLevel--;
        }

        protected override void VisitList<TContainer, TList, TElement>(Property<TContainer, TList> property, ref TContainer container, ref TList value)
        {
            if (s_Styles == null) s_Styles = new Styles();

            if (IsDynamicBufferContainer(value.GetType()) && value.Count > kBufferPageLength)
            {
                var enabled = GUI.enabled;
                GUI.enabled = true;

                EditorGUILayout.LabelField(property.Name, new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.Bold });
                EditorGUI.indentLevel++;

                GUI.enabled = false;

                var hash = value.GetHashCode();
                if (!m_ScrollData.ContainsKey(hash)) m_ScrollData.Add(hash, new ScrollData());
                var scrollData = m_ScrollData[hash];

                scrollData.Count = value.Count;

                EditorGUILayout.IntField("Size", value.Count);

                GUILayout.BeginVertical(GUILayout.MinHeight(scrollData.PageHeight));
                for (var index = scrollData.Page * kBufferPageLength; index < (scrollData.Page + 1) * kBufferPageLength && index < value.Count; index++)
                {
                    EditorGUILayout.LabelField($"Element {index}");

                    EditorGUI.indentLevel++;
                    PropertyContainer.Visit(value[index], this);
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();

                GUI.enabled = true;
                scrollData.Page = EditorGUILayout.IntSlider(s_Styles.PageLabel, scrollData.Page, 0, scrollData.LastPage);
                GUI.enabled = enabled;
                scrollData.PageHeight = math.max(scrollData.PageHeight, GUILayoutUtility.GetLastRect().height);
                EditorGUI.indentLevel--;
                return;
            }

            base.VisitList<TContainer, TList, TElement>(property, ref container, ref value);
        }

        static bool IsDynamicBufferContainer(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DynamicBufferContainer<>);
        }

        static bool IsContainerType<TValue>(ref TValue value)
        {
            var type = typeof(TValue);

            if (!type.IsValueType && null != value)
            {
                type = value.GetType();
            }

            return !(type.IsPrimitive || type.IsEnum || type == typeof(string));
        }
    }
}

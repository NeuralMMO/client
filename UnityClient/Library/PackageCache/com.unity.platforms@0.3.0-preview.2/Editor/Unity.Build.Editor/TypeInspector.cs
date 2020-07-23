using System;
using Unity.Properties.Editor;
using Unity.Properties.UI;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    /// <summary>
    /// Base inspector class for <see cref="Type"/> searcher field.
    /// </summary>
    /// <typeparam name="T">Type to populate the searcher with.</typeparam>
    public abstract class TypeInspector<T> : Inspector<T>
    {
        TextElement m_Text;
        VisualElement m_HelpBox;
        Label m_Message;

        /// <summary>
        /// The title displayed on the searcher window.
        /// </summary>
        public virtual string SearcherTitle => $"Select {typeof(T).Name}";

        /// <summary>
        /// A function that returns whether or not the type should be filtered.
        /// </summary>
        public virtual Func<Type, bool> TypeFilter { get; }

        /// <summary>
        /// A function that returns the display name of the type.
        /// </summary>
        public virtual Func<Type, string> TypeNameResolver { get; }

        /// <summary>
        /// A function that returns the display category name of the type.
        /// </summary>
        public virtual Func<Type, string> TypeCategoryResolver { get; }

        /// <summary>
        /// Error message to display below the inspector as a help box.
        /// </summary>
        public string ErrorMessage { get; set; }

        public override VisualElement Build()
        {
            var typeField = Assets.LoadVisualTreeAsset(nameof(TypeInspector<T>)).CloneTree();
            typeField.AddStyleSheetAndVariant(nameof(TypeInspector<T>));

            var label = typeField.Q<Label>("label");
            label.text = DisplayName;

            var input = typeField.Q<VisualElement>("input");
            input.RegisterCallback<MouseUpEvent>(mouseUpEvent =>
            {
                var database = TypeSearcherDatabase.Populate<T>(TypeFilter, TypeNameResolver, TypeCategoryResolver);
                var searcher = new Searcher(database, new AddTypeSearcherAdapter(SearcherTitle));
                var position = input.worldBound.min + Vector2.up * (input.worldBound.height + 19f);
                var alignment = new SearcherWindow.Alignment(SearcherWindow.Alignment.Vertical.Top, SearcherWindow.Alignment.Horizontal.Left);
                SearcherWindow.Show(EditorWindow.focusedWindow, searcher, OnTypeSelected, position, null);
            });

            m_HelpBox = typeField.Q<VisualElement>("helpbox");

            var icon = m_HelpBox.Q<Image>("icon");
            icon.image = EditorGUIUtility.IconContent("d_console.erroricon.sml").image;
            icon.scaleMode = ScaleMode.ScaleToFit;

            m_Message = m_HelpBox.Q<Label>("message");
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                m_HelpBox.style.display = DisplayStyle.Flex;
                m_Message.text = ErrorMessage;
            }
            else
            {
                m_HelpBox.style.display = DisplayStyle.None;
                m_Message.text = string.Empty;
            }

            var type = Target?.GetType();
            if (type != null)
            {
                m_Text = typeField.Q<TextElement>("text");
                m_Text.text = TypeNameResolver?.Invoke(type) ?? type.Name;
            }

            return typeField;
        }

        public override void Update()
        {
            var type = Target?.GetType();
            if (type != null)
            {
                var text = TypeNameResolver?.Invoke(type) ?? type.Name;
                if (m_Text.text != text)
                {
                    m_Text.text = TypeNameResolver?.Invoke(type) ?? type.Name;
                    NotifyChanged();
                }
            }

            if ((m_Message.text ?? string.Empty) != (ErrorMessage ?? string.Empty))
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    m_HelpBox.style.display = DisplayStyle.Flex;
                    m_Message.text = ErrorMessage;
                }
                else
                {
                    m_HelpBox.style.display = DisplayStyle.None;
                    m_Message.text = string.Empty;
                }
            }
        }

        bool OnTypeSelected(SearcherItem item)
        {
            if (item is TypeSearcherItem typeItem && TypeConstruction.TryConstruct<T>(typeItem.Type, out var instance))
            {
                Target = instance;
                return true;
            }
            return false;
        }
    }
}

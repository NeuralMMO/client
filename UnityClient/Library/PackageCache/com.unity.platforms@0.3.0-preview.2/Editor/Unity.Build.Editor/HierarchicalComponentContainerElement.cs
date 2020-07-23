using System;
using Unity.Properties;
using Unity.Properties.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    interface IChangeHandler
    {
        event Action OnChanged;
    }

    sealed class HierarchicalComponentContainerElement<TContainer, TComponent, T> : VisualElement, IChangeHandler, IBinding, IBindable
        where TContainer : HierarchicalComponentContainer<TContainer, TComponent>
        where T : TComponent
    {
        internal static class ClassNames
        {
            public const string BaseClassName = nameof(HierarchicalComponentContainer<TContainer, TComponent>);
            public const string Component = BaseClassName + "__component";
            public const string Header = BaseClassName + "__component-header";
            public const string Inherited = BaseClassName + "__inherited-component";
            public const string Overridden = BaseClassName + "__overridden-component";
            public const string AddComponent = BaseClassName + "__add-component-button";
            public const string RemoveComponent = BaseClassName + "__remove-component-button";
            public const string Fields = BaseClassName + "__component-fields";
        }

        readonly HierarchicalComponentContainer<TContainer, TComponent> m_Container;
        readonly bool m_IsOptional;
        readonly PropertyElement m_Element;
        readonly Button m_AddButton;
        readonly Button m_RemoveButton;
        readonly Label m_MissingComponentLabel;

        public event Action OnChanged = delegate { };

        public HierarchicalComponentContainerElement(HierarchicalComponentContainer<TContainer, TComponent> container, T component, bool optional)
        {
            this.AddStyleSheetAndVariant(ClassNames.BaseClassName);

            m_Container = container;
            m_IsOptional = optional;

            AddToClassList(ClassNames.BaseClassName);

            var componentContainerName = component.GetType().Name;
            var foldout = new Foldout { text = ObjectNames.NicifyVariableName(componentContainerName) };
            foldout.AddToClassList(ClassNames.Component);
            foldout.AddToClassList(componentContainerName);
            Add(foldout);

            var toggle = foldout.Q<Toggle>();
            toggle.AddToClassList(ClassNames.Header);

            m_AddButton = new Button(AddComponent);
            m_AddButton.AddToClassList(ClassNames.AddComponent);
            toggle.Add(m_AddButton);

            m_RemoveButton = new Button(RemoveComponent);
            m_RemoveButton.AddToClassList(ClassNames.RemoveComponent);
            toggle.Add(m_RemoveButton);

            m_Element = new PropertyElement();
            m_Element.OnChanged += ElementOnOnChanged;
            m_Element.SetTarget(component);

            foldout.contentContainer.Add(m_Element);
            foldout.contentContainer.AddToClassList(ClassNames.Fields);

            m_MissingComponentLabel = new Label($"Component of type {typeof(T).Name} is missing");
            m_MissingComponentLabel.style.display = DisplayStyle.None;
            foldout.contentContainer.Add(m_MissingComponentLabel);

            SetStyle();
        }

        void AddComponent()
        {
            m_Container.SetComponent<T>();
            OnChanged();
        }

        void RemoveComponent()
        {
            m_Container.RemoveComponent<T>();
            OnChanged();
        }

        void SetStyle()
        {
            var inherited = m_Container.IsComponentInherited<T>();
            var overridden = m_Container.IsComponentOverridden<T>();
            var optional = m_IsOptional;

            if (inherited || optional)
            {
                AddToClassList(ClassNames.Inherited);
            }
            else
            {
                RemoveFromClassList(ClassNames.Inherited);
            }

            if (overridden)
            {
                AddToClassList(ClassNames.Overridden);
            }
            else
            {
                RemoveFromClassList(ClassNames.Overridden);
            }

            m_AddButton.style.display = optional ? DisplayStyle.Flex : DisplayStyle.None;
            m_RemoveButton.style.display = (overridden || (!inherited && !optional)) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void ElementOnOnChanged(PropertyElement element, PropertyPath path)
        {
            m_Container.SetComponent(element.GetTarget<T>());
            element.SetTarget(m_Container.GetComponent<T>());
            SetStyle();
        }

        public void PreUpdate()
        {
        }

        public void Update()
        {
            if (m_Container.HasComponent<T>() || m_IsOptional)
            {
                m_Element.style.display = DisplayStyle.Flex;
                m_MissingComponentLabel.style.display = DisplayStyle.None;
                m_Element.SetTarget(m_Container.GetComponentOrDefault<T>());
            }
            else
            {
                m_Element.style.display = DisplayStyle.None;
                m_MissingComponentLabel.style.display = DisplayStyle.Flex;
            }
        }

        public void Release()
        {
        }

        public IBinding binding
        {
            get => this;
            set { }
        }
        public string bindingPath { get; set; }
    }
}

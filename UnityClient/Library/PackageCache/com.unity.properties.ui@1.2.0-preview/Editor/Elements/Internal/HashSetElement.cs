using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class HashSetElement<TSet, TValue> : NullableFoldout<TSet>
        where TSet : ISet<TValue>
        { 
            class AddSetValueElement : VisualElement, IBinding, IBindable
        {
            struct NewSetValue
            {
#pragma warning disable 649
                public TValue Value;
#pragma warning restore 649
            }

            readonly PropertyElement m_PropertyElement;
            readonly Button m_ShowAddKeyContainerButton;
            readonly VisualElement m_AddValueContainer;
            readonly Button m_AddValueToSetButton;
            readonly VisualElement m_ErrorIcon;
            const string NoNullKeysTooltip = "A key cannot be a null value.";
            const string KeyAlreadyExistsTooltip = "Key is already contained in the set.";
            ISet<TValue> m_Set;
            readonly IReloadableElement m_Reload;
            
            IBinding IBindable.binding { get; set; }
            string IBindable.bindingPath { get; set; }

            public AddSetValueElement(IReloadableElement reload)
            {
                m_Reload = reload;
                (this as IBindable).binding = this;
                AddToClassList(UssClasses.Variables);
                Resources.Templates.AddCollectionItem.Clone(this);
                m_ShowAddKeyContainerButton = this.Q<Button>(className: UssClasses.AddKeyDictionaryElement.ShowContainerButton);
                m_ShowAddKeyContainerButton.clickable.clicked += ShowContainer;
                m_AddValueContainer = this.Q<VisualElement>(className: UssClasses.AddKeyDictionaryElement.Container);
                m_AddValueContainer.Hide();
                m_PropertyElement = this.Q<PropertyElement>(className: UssClasses.AddKeyDictionaryElement.Key);
                this.Q<Button>(className:UssClasses.AddKeyDictionaryElement.Cancel).clickable.clicked += OnCancel;
                m_AddValueToSetButton = this.Q<Button>(className: UssClasses.AddKeyDictionaryElement.Add);
                m_AddValueToSetButton.clickable.clicked += OnAdd;
                m_ErrorIcon = this.Q(className: UssClasses.AddKeyDictionaryElement.Error);
            }
            
            public void SetCollection(ISet<TValue> dictionary)
            {
                m_Set = dictionary;
            }

            void OnAdd()
            {
                var key = m_PropertyElement.GetTarget<NewSetValue>().Value;
                m_Set.Add(key);
                m_ShowAddKeyContainerButton.Show();
                m_AddValueContainer.Hide();
                m_PropertyElement.ClearTarget();
                m_Reload.Reload();
            }

            void ShowContainer()
            {
                m_ShowAddKeyContainerButton.Hide();
                m_PropertyElement.SetTarget(new NewSetValue());
                m_AddValueContainer.Show();
            }

            void OnCancel()
            {
                m_ShowAddKeyContainerButton.Show();
                m_AddValueContainer.Hide();
                m_PropertyElement.ClearTarget();
            }

            void IBinding.PreUpdate()
            {
            }

            void IBinding.Update()
            {
                if (!m_PropertyElement.TryGetTarget<NewSetValue>(out var target))
                    return;

                if (!typeof(TValue).IsValueType && null == target.Value)
                {
                    m_ErrorIcon.tooltip = NoNullKeysTooltip;
                    m_ErrorIcon.Show();
                    m_AddValueToSetButton.SetEnabledSmart(false);
                }
                else if (m_Set.Contains(target.Value))
                {
                    m_ErrorIcon.tooltip = KeyAlreadyExistsTooltip;
                    m_ErrorIcon.Show();
                    m_AddValueToSetButton.SetEnabledSmart(false);
                }
                else
                {
                    m_ErrorIcon.Hide();
                    m_AddValueToSetButton.SetEnabledSmart(true);
                }
            }

            void IBinding.Release()
            {
            }
        }

        readonly VisualElement m_Content;
        readonly AddSetValueElement m_AddValueRoot;
        readonly List<PropertyElement> m_Elements;
        
        struct SetValue
        {
            public SetValue(TValue value)
            {
                Value = value;
            }
#pragma warning disable 649
            public TValue Value;
#pragma warning restore 649
        }
        
        public HashSetElement()
        {
            Resources.Templates.SetElement.Clone(this);
            Resources.Templates.SetElementDefaultStyling.AddStyles(this);
            
            m_Elements = new List<PropertyElement>();
            m_Content = new VisualElement();
            m_AddValueRoot = new AddSetValueElement(this);
            
            Add(m_Content);
            Add(m_AddValueRoot);
        }
        
        public override void OnContextReady()
        {
            m_AddValueRoot.SetCollection(GetValue());
        }

        protected override void OnUpdate()
        {
            var set = GetValue();
            if (null == set)
                return;

            if (set.Count == m_Content.childCount)
            {
                foreach (var property in m_Elements)
                {
                    var v = property.GetTarget<SetValue>().Value;
                    if (set.Contains(v))
                        continue;
                    Reload();
                    return;
                }
                return;
            }

            Reload();
        }
        
        public override void Reload(IProperty property)
        {
            m_Content.Clear();
            m_Elements.Clear();

            var set = GetValue();
            if (null == set)
            {
                return;
            }

            foreach (var v in set)
            {
                var key = v;
                
                var root = new PropertyElement();
                m_Elements.Add(root);
                root.AddToClassList(UssClasses.SetElement.ItemContainer);
                root.AddToClassList(UssClasses.Variables);
                root.SetTarget(new SetValue(key)); 
                
                var element = root[0];

                if (null == element)
                    continue;
                
                VisualElement toRemoveParent;
                VisualElement contextMenuParent;
                
                if (element is Foldout foldout)
                {
                    foldout.AddToClassList(UssClasses.SetElement.Item);
                    var toggle = foldout.Q<Toggle>();
                    toggle.AddToClassList(UssClasses.SetElement.ItemFoldout);
                    contextMenuParent = foldout.Q<VisualElement>(className: UssClasses.Unity.ToggleInput); 
                    
                    toRemoveParent = toggle;
                    foldout.contentContainer.AddToClassList(UssClasses.SetElement.ItemContent);
                    foldout.contentContainer.SetEnabledSmart(false);
                }
                else
                {
                    toRemoveParent = root;
                    contextMenuParent = root.Q<Label>();
                    element.AddToClassList(UssClasses.SetElement.ItemNoFoldout);
                    element.contentContainer.SetEnabledSmart(false);
                    root.style.flexDirection = FlexDirection.Row;
                }
                
                contextMenuParent.AddManipulator(
                    new ContextualMenuManipulator(evt =>
                    {
                        evt.menu.AppendSeparator();
                        evt.menu.AppendAction("Delete", action => { OnRemoveItem(key); });
                    }));
                
                var button = new Button();
                button.AddToClassList(UssClasses.SetElement.RemoveItemButton);
                button.clickable.clicked += () => { OnRemoveItem(key); };
                toRemoveParent.Add(button);
                m_Content.Add(root);
            }
            
        }
            void OnRemoveItem(TValue value)
            {
                var set = GetValue();
                if (null == set)
                    return;

                set.Remove(value);
                Reload();
            }
    }
}
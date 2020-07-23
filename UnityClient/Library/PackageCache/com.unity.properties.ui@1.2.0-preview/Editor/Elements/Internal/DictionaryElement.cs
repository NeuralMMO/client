using System.Collections.Generic;
using Unity.Properties.Adapters;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class DictionaryElement<TDictionary, TKey, TValue> : NullableFoldout<TDictionary>
        where TDictionary : IDictionary<TKey, TValue>
    {
        class AddDictionaryKeyElement : VisualElement, IBinding, IBindable
        {
            struct NewDictionaryKey
            {
#pragma warning disable 649
                public TKey Key;
#pragma warning restore 649
            }

            readonly PropertyElement m_PropertyElement;
            readonly Button m_ShowAddKeyContainerButton;
            readonly VisualElement m_AddKeyContainer;
            readonly Button m_AddKeyToDictionaryButton;
            readonly VisualElement m_ErrorIcon;
            const string NoNullKeysTooltip = "A key cannot be a null value.";
            const string KeyAlreadyExistsTooltip = "Key is already contained in the dictionary.";
            TDictionary m_Dictionary;
            readonly IReloadableElement m_Reload;
            
            IBinding IBindable.binding { get; set; }
            string IBindable.bindingPath { get; set; }

            public AddDictionaryKeyElement(IReloadableElement reload)
            {
                m_Reload = reload;
                (this as IBindable).binding = this;
                Resources.Templates.AddCollectionItem.Clone(this);
                m_ShowAddKeyContainerButton = this.Q<Button>(className: UssClasses.AddKeyDictionaryElement.ShowContainerButton);
                m_ShowAddKeyContainerButton.clickable.clicked += ShowContainer;
                m_AddKeyContainer = this.Q<VisualElement>(className: UssClasses.AddKeyDictionaryElement.Container);
                m_AddKeyContainer.Hide();
                m_PropertyElement = this.Q<PropertyElement>(className: UssClasses.AddKeyDictionaryElement.Key);
                this.Q<Button>(className:UssClasses.AddKeyDictionaryElement.Cancel).clickable.clicked += OnCancel;
                m_AddKeyToDictionaryButton = this.Q<Button>(className: UssClasses.AddKeyDictionaryElement.Add);
                m_AddKeyToDictionaryButton.clickable.clicked += OnAdd;
                m_ErrorIcon = this.Q(className: UssClasses.AddKeyDictionaryElement.Error);
            }
            
            public void SetCollection(TDictionary dictionary)
            {
                m_Dictionary = dictionary;
            }

            void OnAdd()
            {
                var key = m_PropertyElement.GetTarget<NewDictionaryKey>().Key;
                m_Dictionary.Add(key, default);
                m_ShowAddKeyContainerButton.Show();
                m_AddKeyContainer.Hide();
                m_Reload.Reload();
            }

            void ShowContainer()
            {
                m_ShowAddKeyContainerButton.Hide();
                m_PropertyElement.SetTarget(new NewDictionaryKey());
                m_AddKeyContainer.Show();
            }

            void OnCancel()
            {
                m_ShowAddKeyContainerButton.Show();
                m_AddKeyContainer.Hide();
            }

            void IBinding.PreUpdate()
            {
            }

            void IBinding.Update()
            {
                if (!m_PropertyElement.TryGetTarget<NewDictionaryKey>(out var target))
                    return;

                if (!typeof(TKey).IsValueType && null == target.Key)
                {
                    m_ErrorIcon.tooltip = NoNullKeysTooltip;
                    m_ErrorIcon.Show();
                    m_AddKeyToDictionaryButton.SetEnabledSmart(false);
                }
                else if (m_Dictionary.ContainsKey(target.Key))
                {
                    m_ErrorIcon.tooltip = KeyAlreadyExistsTooltip;
                    m_ErrorIcon.Show();
                    m_AddKeyToDictionaryButton.SetEnabledSmart(false);
                }
                else
                {
                    m_ErrorIcon.Hide();
                    m_AddKeyToDictionaryButton.SetEnabledSmart(true);
                }
            }

            void IBinding.Release()
            {
            }
        }

        class DictionaryAdapter : IVisit<KeyValuePair<TKey, TValue>>
        {
            DictionaryElement<TDictionary, TKey, TValue> DictionaryElement;

            public DictionaryAdapter(DictionaryElement<TDictionary, TKey, TValue>  dictionaryElement)
            {
                DictionaryElement = dictionaryElement;
            }

            public VisitStatus Visit<TContainer>(Property<TContainer, KeyValuePair<TKey, TValue>> property, ref TContainer container, ref KeyValuePair<TKey, TValue> value)
            {
                var visitor = DictionaryElement.GetVisitor();
                visitor.AddToPath(property);
                try
                {
                    var inspector = (IInspector<KeyValuePair<TKey, TValue>>) new KeyValuePairInspector<TDictionary, TKey, TValue>
                    {
                        DictionaryElement = DictionaryElement
                    };
            
                    inspector.Context = new InspectorContext<KeyValuePair<TKey, TValue>>(
                        visitor.VisitorContext.Root,
                        visitor.GetCurrentPath(),
                        property
                    );
            
                    visitor.VisitorContext.Parent.contentContainer.Add(new CustomInspectorElement(
                        visitor.GetCurrentPath(),
                        inspector,
                        visitor.VisitorContext.Root));
                }
                finally
                {
                    visitor.RemoveFromPath(property);
                }
            
                return VisitStatus.Stop;
            }
        }

        readonly DictionaryAdapter s_Adapter;
        readonly VisualElement m_Content;
        readonly AddDictionaryKeyElement m_AddKeyRoot;

        public DictionaryElement()
        {
            Resources.Templates.DictionaryElement.Clone(this);
            AddToClassList(UssClasses.DictionaryElement.Dictionary);

            s_Adapter = new DictionaryAdapter(this);
            m_Content = new VisualElement();
            m_AddKeyRoot = new AddDictionaryKeyElement(this);

            Add(m_Content);
            Add(m_AddKeyRoot);
        }
 
        public override void OnContextReady()
        {
            m_AddKeyRoot.SetCollection(GetValue());
        }

        protected override void OnUpdate()
        {
            var dictionary = GetValue();
            if (null == dictionary)
                return;

            if (dictionary.Count == m_Content.childCount)
            {
                var list = ListPool<PropertyElement>.Get();
                try
                {
                    this.Query<PropertyElement>(className: UssClasses.KeyValuePairElement.Key)
                        .ToList(list);
                    foreach (var keyElement in list)
                    {
                        if (ContainsKey(keyElement
                            .GetTarget<KeyValuePairInspector<TDictionary, TKey, TValue>.KeyContainer>().Key))
                            continue;

                        Reload();
                        return;
                    }

                    return;
                }
                finally{
                {
                    ListPool<PropertyElement>.Release(list);
                }}
            }

            Reload();
        }

        public override void Reload(IProperty property)
        {
            var dictionary = GetValue();
            if (null == dictionary)
            {
                return;
            }

            var visitor = GetVisitor() as PropertyVisitor;
            if (null == visitor)
                return;
            
            m_Content.Clear();

            visitor.AddAdapter(s_Adapter);
            try
            {
                var list = ListPool<TKey>.Get();
                try
                {
                    list.AddRange(dictionary.Keys);
                    foreach (var key in list)
                    {
                        Path.PushKey(key);
                        Root.VisitAtPath(Path, m_Content);
                        Path.Pop();
                    }
                }
                finally
                {
                    ListPool<TKey>.Release(list);
                }
            }
            finally
            {
                visitor.RemoveAdapter(s_Adapter);
            }
        }

        internal bool ContainsKey(TKey key)
        {
            return Root.TryGetValue(Path, out TDictionary dictionary) && dictionary.ContainsKey(key);
        }

        internal void RemoveAtKey(TKey key)
        {
            if (Root.TryGetValue(Path, out TDictionary dictionary) && dictionary.Remove(key))
            {
                Reload();
            }
        }

        internal void SetAtKey(TKey key, TValue newValue)
        {
            var dictionary = GetValue();
            if (null == dictionary)
                return;

            if (dictionary.TryGetValue(key, out var v) && (v?.Equals(newValue) ?? false))
                return;

            dictionary[key] = newValue;
            var changedPath = new PropertyPath();
            changedPath.PushPath(Path);
            changedPath.PushKey(key);
            changedPath.PushName("Value");
            Root.NotifyChanged(changedPath);
        }
    }
}
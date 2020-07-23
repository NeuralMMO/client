using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class KeyValuePairInspector<TDictionary, TKey, TValue> : Inspector<KeyValuePair<TKey, TValue>>
        where TDictionary : IDictionary<TKey, TValue>
    {
        VisualElement m_Root;
        PropertyElement m_Key;
        PropertyElement m_Value;
        
        public DictionaryElement<TDictionary, TKey, TValue> DictionaryElement;        
        
        internal struct KeyContainer
        {
            public KeyContainer(TKey key)
            {
                Key = key;
            }
            public readonly TKey Key;
        }
        
        struct ValueContainer
        {
            public ValueContainer(TValue value)
            {
                Value = value;
            }
            public TValue Value;
        }
        
        public override VisualElement Build()
        {
            m_Root = new VisualElement();
            Resources.Templates.KeyValuePairElement.Clone(m_Root);
            m_Root.AddToClassList(UssClasses.KeyValuePairElement.KeyValuePair);
            
            m_Key = m_Root.Q<PropertyElement>(className: UssClasses.KeyValuePairElement.Key);
            m_Key.SetTarget(new KeyContainer(Target.Key));
            if (m_Key.contentContainer.Q<Foldout>() is Foldout foldout)
            {
                foldout.SetEnabled(true);
                foldout.contentContainer.SetEnabled(false);                
            }
            else
            {
                m_Key.contentContainer.SetEnabled(false);
            }

            var remove = m_Root.Q<Button>(className: UssClasses.KeyValuePairElement.RemoveButton);
            remove.clickable.clicked += () =>
            {
                DictionaryElement.RemoveAtKey(m_Key.GetTarget<KeyContainer>().Key);
            };
            
            m_Value = m_Root.Q<PropertyElement>(className: UssClasses.KeyValuePairElement.Value);
            m_Value.SetTarget(new ValueContainer(Target.Value));
            m_Value.OnChanged += OnValueChanged;

            return m_Root;
        }

        void OnValueChanged(PropertyElement element, PropertyPath path)
        {
            DictionaryElement.SetAtKey(m_Key.GetTarget<KeyContainer>().Key, m_Value.GetTarget<ValueContainer>().Value);
        }

        public override void Update()
        {
            m_Key.SetTarget(new KeyContainer(Target.Key));
            m_Value.SetTarget(new ValueContainer(Target.Value));
        }
    }
}

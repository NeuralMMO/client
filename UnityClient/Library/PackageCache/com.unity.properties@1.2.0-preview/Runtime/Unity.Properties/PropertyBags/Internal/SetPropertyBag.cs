using System;
using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    class SetElementProperty<TSet, TValue> : Property<TSet, TValue>, ICollectionElementProperty
        where TSet : ISet<TValue>
    {
        internal TValue m_Value;

        public override string Name => m_Value.ToString();
        public override bool IsReadOnly => true;

        public override TValue GetValue(ref TSet container) => m_Value;
        public override void SetValue(ref TSet container, TValue value) => throw new InvalidOperationException("Property is ReadOnly.");
    }
    
    class SetPropertyBag<TSet, TElement> : PropertyBag<TSet>, ISetPropertyBag<TSet, TElement>, IPropertyKeyable<TSet, object>
        where TSet : ISet<TElement>
    {
        readonly SetElementProperty<TSet, TElement> m_Property = new SetElementProperty<TSet, TElement>();
        
        internal override IEnumerable<IProperty<TSet>> GetProperties(ref TSet container)
        {
            return GetPropertiesImpl(container);
        }

        IEnumerable<IProperty<TSet>> GetPropertiesImpl(TSet container)
        {
            foreach (var element in container)
            {
                m_Property.m_Value = element;
                yield return m_Property;
            }
        }

        void ICollectionPropertyBagAccept<TSet>.Accept(ICollectionPropertyBagVisitor visitor, ref TSet container)
        {
            visitor.Visit(this, ref container); 
        }
        
        void ISetPropertyBagAccept<TSet>.Accept(ISetPropertyBagVisitor visitor, ref TSet container)
        {
            visitor.Visit(this, ref container);
        }

        void ISetPropertyAccept<TSet>.Accept<TContainer>(ISetPropertyVisitor visitor, Property<TContainer, TSet> property, ref TContainer container, ref TSet dictionary)
        {
            using ((m_Property as IAttributes).CreateAttributesScope(property))
            {
                visitor.Visit<TContainer, TSet, TElement>(property, ref container, ref dictionary);
            }
        }

        public bool TryGetProperty(ref TSet container, object key, out IProperty<TSet> property)
        {
            if (container.Contains((TElement) key))
            {
                property = new SetElementProperty<TSet, TElement> {m_Value = (TElement) key};
                return true;
            }

            property = default;
            return false;
        }
    }
}
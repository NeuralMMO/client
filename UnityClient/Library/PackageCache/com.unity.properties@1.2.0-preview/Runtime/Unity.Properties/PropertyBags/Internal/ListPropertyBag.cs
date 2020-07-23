using System.Collections;
using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    class ListElementProperty<TList, TValue> : Property<TList, TValue>, IListElementProperty
        where TList : IList<TValue>
    {
        internal int m_Index;
        internal bool m_IsReadOnly;

        /// <inheritdoc/>
        public int Index => m_Index;
        
        /// <inheritdoc/>
        public override string Name => Index.ToString();
        
        /// <inheritdoc/>
        public override bool IsReadOnly => m_IsReadOnly;
        
        /// <inheritdoc/>
        public override TValue GetValue(ref TList container) => container[m_Index];
        
        /// <inheritdoc/>
        public override void SetValue(ref TList container, TValue value) => container[m_Index] = value;
    }
    
    class ListPropertyBag<TList, TElement> : PropertyBag<TList>, IListPropertyBag<TList, TElement>
        where TList : IList<TElement>
    {
        /// <summary>
        /// Internal collection used to dynamically return the same instance pointing to a different index.
        /// </summary>
        internal struct PropertyCollection : IEnumerable<IProperty<TList>>
        {
            public struct Enumerator : IEnumerator<IProperty<TList>>
            {
                readonly TList m_List;
                readonly ListElementProperty<TList, TElement> m_Property;
                readonly int m_Previous;
                int m_Position;

                internal Enumerator(TList list, ListElementProperty<TList, TElement> property)
                {
                    m_List = list;
                    m_Property = property;
                    m_Previous = property.m_Index;
                    m_Position = -1;
                }

                /// <inheritdoc/>
                public IProperty<TList> Current => m_Property;

                /// <inheritdoc/>
                object IEnumerator.Current => Current;

                /// <inheritdoc/>
                public bool MoveNext()
                {
                    m_Position++;
                    
                    if (m_Position < m_List.Count)
                    {
                        m_Property.m_Index = m_Position;
                        m_Property.m_IsReadOnly = false;
                        return true;
                    }
                    
                    m_Property.m_Index = m_Previous;
                    m_Property.m_IsReadOnly = false;
                    return false;
                }

                /// <inheritdoc/>
                public void Reset()
                {
                    m_Position = -1;
                    m_Property.m_Index = m_Previous;
                    m_Property.m_IsReadOnly = false;
                }

                /// <inheritdoc/>
                public void Dispose()
                {
                }
            }

            readonly TList m_List;
            readonly ListElementProperty<TList, TElement> m_Property;
            
            public PropertyCollection(TList list, ListElementProperty<TList, TElement> property)
            {
                m_List = list;
                m_Property = property;
            }
            
            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="PropertyCollection"/>.
            /// </summary>
            /// <returns>A <see cref="PropertyCollection.Enumerator"/> for the <see cref="PropertyCollection"/>.</returns>
            public Enumerator GetEnumerator() => new Enumerator(m_List, m_Property);
            
            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            
            /// <inheritdoc/>
            IEnumerator<IProperty<TList>> IEnumerable<IProperty<TList>>.GetEnumerator() => GetEnumerator();
        }
        
        /// <summary>
        /// Shared instance of a list element property. We re-use the same instance to avoid allocations.
        /// </summary>
        readonly ListElementProperty<TList, TElement> m_Property = new ListElementProperty<TList, TElement>();

        /// <inheritdoc/>
        internal override IEnumerable<IProperty<TList>> GetProperties(ref TList container)
        {
            return new PropertyCollection(container, m_Property);
        }
        
        /// <inheritdoc/>
        PropertyCollection IListPropertyBag<TList, TElement>.GetProperties(ref TList container)
        {
            return new PropertyCollection(container, m_Property);
        }

        /// <inheritdoc/>
        public bool TryGetProperty(ref TList container, int index, out IProperty<TList> property)
        {
            if ((uint) index >= (uint) container.Count)
            {
                property = null;
                return false;
            }
            
            property = new ListElementProperty<TList, TElement>
            {
                m_Index = index,
                m_IsReadOnly = false
            };

            return true;
        }
        
        void ICollectionPropertyBagAccept<TList>.Accept(ICollectionPropertyBagVisitor visitor, ref TList container)
        {
            visitor.Visit(this, ref container); 
        }
        
        void IListPropertyBagAccept<TList>.Accept(IListPropertyBagVisitor visitor, ref TList list)
        {
            visitor.Visit(this, ref list);
        }
        
        void IListPropertyAccept<TList>.Accept<TContainer>(IListPropertyVisitor visitor, Property<TContainer, TList> property, ref TContainer container, ref TList list)
        {
            using ((m_Property as IAttributes).CreateAttributesScope(property))
            {
                visitor.Visit<TContainer, TList, TElement>(property, ref container, ref list);
            }
        }
    }
}
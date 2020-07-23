using System.Collections;
using System.Collections.Generic;

namespace Unity.Properties.Internal
{    
    class KeyValuePairProperty<TDictionary, TKey, TValue> : Property<TDictionary, KeyValuePair<TKey, TValue>>, IDictionaryElementProperty<TKey>
        where TDictionary : IDictionary<TKey, TValue>
    {
        public override string Name => Key.ToString();
        public override bool IsReadOnly => false;

        public override KeyValuePair<TKey, TValue> GetValue(ref TDictionary container)
        {
            return new KeyValuePair<TKey, TValue>(Key, container[Key]);
        }

        public override void SetValue(ref TDictionary container, KeyValuePair<TKey, TValue> value)
        {
            container[value.Key] = value.Value;
        }

        public TKey Key { get; internal set; }
        public object ObjectKey => Key;
    }
    
    class DictionaryElementProperty<TDictionary, TKey, TValue> : Property<TDictionary, TValue>
        where TDictionary : IDictionary<TKey, TValue>
    {
        public override string Name => Key.ToString();
        public override bool IsReadOnly => false;

        public override TValue GetValue(ref TDictionary container)
        {
            return container[Key];
        }

        public override void SetValue(ref TDictionary container, TValue value)
        {
            container[Key] = value;
        }

        public TKey Key { get; internal set; }
    }
    
    class DictionaryPropertyBag<TDictionary, TKey, TValue> : PropertyBag<TDictionary>, IDictionaryPropertyBag<TDictionary, TKey, TValue>
        where TDictionary : IDictionary<TKey, TValue>
    {
        static readonly Pool<List<TKey>> s_Pool = new Pool<List<TKey>>(() => new List<TKey>(), l => l.Clear());
        
        /// <summary>
        /// Collection used to dynamically return the same instance pointing to a different <see cref="KeyValuePair{TKey,TValue}"/>.
        /// </summary>
        internal struct PropertyCollection : IEnumerable<IProperty<TDictionary>>
        {
            public struct Enumerator : IEnumerator<IProperty<TDictionary>>
            {
                readonly TDictionary m_Dictionary;
                readonly KeyValuePairProperty<TDictionary, TKey, TValue> m_Property;
                readonly TKey m_Previous;
                readonly List<TKey> Keys;
                int m_Position;

                internal Enumerator(TDictionary dictionary, KeyValuePairProperty<TDictionary, TKey, TValue> property)
                {
                    m_Dictionary = dictionary;
                    m_Property = property;
                    m_Previous = property.Key;
                    m_Position = -1;
                    Keys = s_Pool.Get();
                    Keys.AddRange(m_Dictionary.Keys);
                }

                /// <inheritdoc/>
                public IProperty<TDictionary> Current => m_Property;

                /// <inheritdoc/>
                object IEnumerator.Current => Current;

                /// <inheritdoc/>
                public bool MoveNext()
                {
                    m_Position++;
                    
                    if (m_Position < m_Dictionary.Count)
                    {
                        m_Property.Key = Keys[m_Position];
                        return true;
                    }
                    
                    m_Property.Key = m_Previous;
                    return false;
                }

                /// <inheritdoc/>
                public void Reset()
                {
                    m_Position = -1;
                    m_Property.Key = m_Previous;
                }

                /// <inheritdoc/>
                public void Dispose()
                {
                    s_Pool.Release(Keys);
                }
            }

            readonly TDictionary m_Dictionary;
            readonly KeyValuePairProperty<TDictionary, TKey, TValue> m_Property;
            
            public PropertyCollection(TDictionary dictionary, KeyValuePairProperty<TDictionary, TKey, TValue> property)
            {
                m_Dictionary = dictionary;
                m_Property = property;
            }
            
            /// <summary>
            /// Returns an enumerator that iterates through the <see cref="PropertyCollection"/>.
            /// </summary>
            /// <returns>A <see cref="PropertyCollection.Enumerator"/> for the <see cref="PropertyCollection"/>.</returns>
            public Enumerator GetEnumerator() => new Enumerator(m_Dictionary, m_Property);
            
            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            
            /// <inheritdoc/>
            IEnumerator<IProperty<TDictionary>> IEnumerable<IProperty<TDictionary>>.GetEnumerator() => GetEnumerator();
        }
        
        /// <inheritdoc/>
        internal override IEnumerable<IProperty<TDictionary>> GetProperties(ref TDictionary container)
        {
            return new PropertyCollection(container, m_KeyValuePairProperty);
        }
        
        /// <inheritdoc/>
        PropertyCollection IDictionaryPropertyBag<TDictionary, TKey, TValue>.GetProperties(ref TDictionary container)
        {
            return new PropertyCollection(container, m_KeyValuePairProperty);
        }
        
        /// <summary>
        /// Shared instance of a dictionary element property. We re-use the same instance to avoid allocations.
        /// </summary>
        readonly KeyValuePairProperty<TDictionary, TKey, TValue> m_KeyValuePairProperty = new KeyValuePairProperty<TDictionary, TKey, TValue>();
        
        void ICollectionPropertyBagAccept<TDictionary>.Accept(ICollectionPropertyBagVisitor visitor, ref TDictionary container)
        {
            visitor.Visit(this, ref container); 
        }
        
        void IDictionaryPropertyBagAccept<TDictionary>.Accept(IDictionaryPropertyBagVisitor visitor, ref TDictionary container)
        {
            visitor.Visit(this, ref container); 
        }
        
        void IDictionaryPropertyAccept<TDictionary>.Accept<TContainer>(IDictionaryPropertyVisitor visitor, Property<TContainer, TDictionary> property, ref TContainer container,
            ref TDictionary dictionary)
        {
            using ((m_KeyValuePairProperty as IAttributes).CreateAttributesScope(property))
            {
                visitor.Visit<TContainer, TDictionary, TKey, TValue>(property, ref container, ref dictionary);
            }
        }

        /// <inheritdoc/>
        bool IPropertyKeyable<TDictionary, object>.TryGetProperty(ref TDictionary container, object key, out IProperty<TDictionary> property)
        {
            if (container.ContainsKey((TKey)key))
            {
                property = new KeyValuePairProperty<TDictionary, TKey, TValue> { Key = (TKey)key };
                return true;
            }

            property = default;
            return false;
        }
    }
}
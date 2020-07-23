using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Base class for implementing a static property bag for a specified container type. This is an abstract class.
    /// </summary>
    /// <remarks>
    /// A <see cref="ContainerPropertyBag{TContainer}"/> is used to describe and traverse the properties for a specified <typeparamref name="TContainer"/> type.
    ///
    /// In order for properties to operate on a type, a <see cref="ContainerPropertyBag{TContainer}"/> must exist and be pre-registered for that type.
    ///
    /// _NOTE_ In editor use cases property bags can be generated dynamically through reflection. (see Unity.Properties.Reflection)
    /// </remarks>
    /// <typeparam name="TContainer">The container type.</typeparam>
    public abstract class ContainerPropertyBag<TContainer> : PropertyBag<TContainer>, IPropertyList<TContainer>, IPropertyNameable<TContainer>
    {
        static ContainerPropertyBag()
        {
            if (!RuntimeTypeInfoCache.IsContainerType(typeof(TContainer)))
            {
                throw new InvalidOperationException($"Failed to create a property bag for Type=[{typeof(TContainer)}]. The type is not a valid container type.");
            }
        }

        /// <summary>
        /// An <see cref="PropertyCollection"/> used to store a collection of properties for a given container type. This is an internal class.
        /// </summary>
        class PropertyCollection : IEnumerable<IProperty<TContainer>>
        {
            readonly List<IProperty<TContainer>> m_PropertiesList = new List<IProperty<TContainer>>();
            readonly Dictionary<string, IProperty<TContainer>> m_PropertiesHash = new Dictionary<string, IProperty<TContainer>>();

            /// <summary>
            /// Adds a property to the collection.
            /// </summary>
            /// <param name="property">The property to add.</param>
            /// <exception cref="ArgumentNullException">The property is null.</exception>
            public void Add(IProperty<TContainer> property)
            {
                if (null == property)
                {
                    throw new ArgumentNullException(nameof(property));
                }

                m_PropertiesList.Add(property);
                m_PropertiesHash.Add(property.Name, property);
            }

            /// <summary>
            /// Removes a property from the collection.
            /// </summary>
            /// <param name="property">The property to remove.</param>
            /// <exception cref="ArgumentNullException">The property is null.</exception>
            public bool Remove(IProperty<TContainer> property)
            {
                if (null == property)
                {
                    throw new ArgumentNullException(nameof(property));
                }

                return m_PropertiesList.Remove(property) && m_PropertiesHash.Remove(property.Name);
            }

            /// <summary>
            /// Clears all properties from the collection.
            /// </summary>
            public void Clear()
            {
                m_PropertiesList.Clear();
                m_PropertiesHash.Clear();
            }

            /// <summary>
            /// Gets the property associated with the specified name.
            /// </summary>
            /// <param name="name">The name of the property to get.</param>
            /// <param name="property">When this method returns, contains the property associated with the specified name, if the name is found; otherwise, null.</param>
            /// <returns><see langword="true"/> if the <see cref="ContainerPropertyBag{TContainer}"/> contains a property with the specified name; otherwise, <see langword="false"/>.</returns>
            public bool TryGetProperty(string name, out IProperty<TContainer> property)
            {
                return m_PropertiesHash.TryGetValue(name, out property);
            }

            /// <inheritdoc/>
            IEnumerator<IProperty<TContainer>> IEnumerable<IProperty<TContainer>>.GetEnumerator() => m_PropertiesList.GetEnumerator();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => m_PropertiesList.GetEnumerator();

            /// <summary>
            /// Returns the internal collection of properties.
            /// </summary>
            /// <returns></returns>
            public List<IProperty<TContainer>> GetList() => m_PropertiesList;
        }

        readonly PropertyCollection m_Properties = new PropertyCollection();

        /// <summary>
        /// Adds a <see cref="Property{TContainer,TValue}"/> to the property bag.
        /// </summary>
        /// <param name="property">The <see cref="Property{TContainer,TValue}"/> to add.</param>
        /// <typeparam name="TValue">The value type for the given property.</typeparam>
        protected void AddProperty<TValue>(Property<TContainer, TValue> property)
        {
            m_Properties.Add(property);
        }

        /// <inheritdoc/>
        internal override IEnumerable<IProperty<TContainer>> GetProperties(ref TContainer container)
        {
            return m_Properties;
        }

        /// <inheritdoc/>
        public bool TryGetProperty(ref TContainer container, string name, out IProperty<TContainer> property)
        {
            return m_Properties.TryGetProperty(name, out property);
        }

        /// <inheritdoc/>
        List<IProperty<TContainer>> IPropertyList<TContainer>.GetProperties(ref TContainer container)
        {
            return m_Properties.GetList();
        }
    }
}
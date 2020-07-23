using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Base interface for working with properties.
    /// </summary>
    /// <remarks>
    /// This is used to pass or store properties without knowing the underlying container or value type.
    /// * <seealso cref="IProperty{TContainer}"/>
    /// * <seealso cref="Property{TContainer,TValue}"/>
    /// </remarks>
    public interface IProperty
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets a value indicating whether the property is read-only or not.
        /// </summary>
        bool IsReadOnly { get; }
        
        /// <summary>
        /// Returns the declared value type of the property.
        /// </summary>
        /// <returns>The declared value type.</returns>
        Type DeclaredValueType();
        
        /// <summary>
        /// Returns true if the property has any attributes of the given type.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type to check for.</typeparam>
        /// <returns><see langword="true"/> if the property has the given attribute type; otherwise, <see langword="false"/>.</returns>
        bool HasAttribute<TAttribute>()
            where TAttribute : Attribute;

        /// <summary>
        /// Returns the first attribute of the given type.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type to get.</typeparam>
        /// <returns>The attribute of the given type for this property.</returns>
        TAttribute GetAttribute<TAttribute>()
            where TAttribute : Attribute;

        /// <summary>
        /// Returns all attribute of the given type.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type to get.</typeparam>
        /// <returns>An <see cref="IEnumerable{TAttribute}"/> for all attributes of the given type.</returns>
        IEnumerable<TAttribute> GetAttributes<TAttribute>()
            where TAttribute : Attribute;
        
        /// <summary>
        /// Returns all attribute for this property.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{Attribute}"/> for all attributes.</returns>
        IEnumerable<Attribute> GetAttributes();
    }
    
    /// <summary>
    /// Base interface for working with properties.
    /// </summary>
    /// <remarks>
    /// This is used to pass or store properties without knowing the underlying value type.
    /// * <seealso cref="Property{TContainer,TValue}"/>
    /// </remarks>
    /// <typeparam name="TContainer">The container type this property operates on.</typeparam>
    public interface IProperty<TContainer> : IProperty
    {
        /// <summary>
        /// Returns the property value of a specified container.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <returns>The property value of the given container.</returns>
        object GetValue(ref TContainer container);

        /// <summary>
        /// Sets the property value of a specified container.
        /// </summary>
        /// <param name="container">The container whose property value will be set.</param>
        /// <param name="value">The new property value.</param>
        /// <returns><see langword="true"/> if the value was set; otherwise, <see langword="false"/>.</returns>
        bool TrySetValue(ref TContainer container, object value);
    }
    
    /// <summary>
    /// Base class for implementing properties. This is an abstract class. 
    /// </summary>
    /// <remarks>
    /// A <see cref="IProperty"/> is used as an accessor to the underlying data of a container.
    /// </remarks>
    /// <typeparam name="TContainer">The container type this property operates on.</typeparam>
    /// <typeparam name="TValue">The value type for this property.</typeparam>
    public abstract class Property<TContainer, TValue> : IProperty<TContainer>, IPropertyAccept<TContainer>, IAttributes
    {
        [UnityEngine.Scripting.Preserve]
        static void Preserve()
        {
            AOT.PropertyGenerator<TContainer, TValue>.Preserve();
        }
        
        List<Attribute> m_Attributes;

        /// <summary>
        /// Collection of attributes for this <see cref="Property{TContainer,TValue}"/>.
        /// </summary>
        List<Attribute> IAttributes.Attributes
        {
            get => m_Attributes;
            set => m_Attributes = value;
        }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract bool IsReadOnly { get; }
        
        /// <inheritdoc/>
        public Type DeclaredValueType() => typeof(TValue);
        
        /// <inheritdoc/>
        void IPropertyAccept<TContainer>.Accept(IPropertyVisitor visitor, ref TContainer container) => visitor.Visit(this, ref container);

        /// <inheritdoc/>
        object IProperty<TContainer>.GetValue(ref TContainer container) => GetValue(ref container);
        
        /// <inheritdoc/>
        bool IProperty<TContainer>.TrySetValue(ref TContainer container, object value)
        {
            if (!TypeConversion.TryConvert(value, out TValue typed))
                return false;

            SetValue(ref container, typed);
            return true;
        }

        /// <summary>
        /// Returns the property value of a specified container.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <returns>The property value of the given container.</returns>
        public abstract TValue GetValue(ref TContainer container);
        
        /// <summary>
        /// Sets the property value of a specified container.
        /// </summary>
        /// <param name="container">The container whose property value will be set.</param>
        /// <param name="value">The new property value.</param>
        public abstract void SetValue(ref TContainer container, TValue value);
        
        /// <summary>
        /// Adds an attribute to the property.
        /// </summary>
        /// <param name="attribute">The attribute to add.</param>
        protected void AddAttribute(Attribute attribute) => ((IAttributes) this).AddAttribute(attribute);
        
        /// <summary>
        /// Adds a set of attributes to the property.
        /// </summary>
        /// <param name="attributes">The attributes to add.</param>
        protected void AddAttributes(IEnumerable<Attribute> attributes) => ((IAttributes) this).AddAttributes(attributes);
        
        /// <inheritdoc/>
        void IAttributes.AddAttribute(Attribute attribute)
        {
            if (null == attribute || attribute.GetType() == typeof(CreatePropertyAttribute)) return;
            if (null == m_Attributes) m_Attributes = new List<Attribute>();
            m_Attributes.Add(attribute);
        }
        
        /// <inheritdoc/>
        void IAttributes.AddAttributes(IEnumerable<Attribute> attributes)
        {
            if (null == m_Attributes) m_Attributes = new List<Attribute>();
            
            foreach (var attribute in attributes)
            {
                if (null == attribute || attribute.GetType() == typeof(CreatePropertyAttribute))
                    continue;
                
                m_Attributes.Add(attribute);
            }
        }
        
        /// <inheritdoc/>
        public bool HasAttribute<TAttribute>() where TAttribute : Attribute
        {
            for (var i = 0; i < m_Attributes?.Count; i++)
            {
                if (m_Attributes[i] is TAttribute)
                {
                    return true;
                }
            }

            return default;
        }

        /// <inheritdoc/>
        public TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute
        {
            for (var i = 0; i < m_Attributes?.Count; i++)
            {
                if (m_Attributes[i] is TAttribute typed)
                {
                    return typed;
                }
            }

            return default;
        }

        /// <inheritdoc/>
        public IEnumerable<TAttribute> GetAttributes<TAttribute>() where TAttribute : Attribute
        {
            for (var i = 0; i < m_Attributes?.Count; i++)
            {
                if (m_Attributes[i] is TAttribute typed)
                {
                    yield return typed;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Attribute> GetAttributes()
        {
            for (var i = 0; i < m_Attributes?.Count; i++)
            {
                yield return m_Attributes[i];
            }
        }

        /// <inheritdoc/>
        AttributesScope IAttributes.CreateAttributesScope(IAttributes attributes) => new AttributesScope(this, attributes?.Attributes);
    }

    /// <summary>
    /// A set of property extension methods for visitation.
    /// </summary>
    public static class PropertyVisitExtensions
    {
        /// <summary>
        /// Call this method to net visit all the elements of the given value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="visitor">The property visitor to use.</param>
        /// <param name="value">The value to visit.</param>
        /// <typeparam name="TValue">The value type for this property.</typeparam>
        public static void Visit<TValue>(this IProperty property, PropertyVisitor visitor, ref TValue value)
        {
            PropertyContainer.Visit(ref value, visitor, out _);
        }
        
        /// <summary>
        /// Visits a specific element of the given container by index. 
        /// </summary>
        /// <remarks>
        /// This method can be used to selectively visit list elements while maintaining the visit context.
        /// </remarks>
        /// <param name="property">The property this method extends.</param>
        /// <param name="visitor">The visitor to use.</param>
        /// <param name="container">The container to visit.</param>
        /// <param name="index">The index to visit at.</param>
        /// <typeparam name="TValue">The value type for this property.</typeparam>
        /// <exception cref="IndexOutOfRangeException">The given index could not be resolved.</exception>
        /// <exception cref="InvalidOperationException">The given container is not indexable.</exception>
        public static void Visit<TValue>(this IProperty property, PropertyVisitor visitor, ref TValue container, int index)
        {
            var properties = PropertyBagStore.GetPropertyBag<TValue>();

            if (properties is IPropertyIndexable<TValue> indexable)
            {
                if (indexable.TryGetProperty(ref container, index, out var p))
                {
                    using (((IAttributes) p).CreateAttributesScope((IAttributes) property))
                    {
                        ((IPropertyAccept<TValue>) p).Accept(visitor, ref container);
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException($"Failed to visit element at index, not property was found for Index=[{index}].");
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to visit element at index, container is not indexable.");
            }
        }
        
        /// <summary>
        /// Visits a specific element of the given container by key. 
        /// </summary>
        /// <remarks>
        /// This method can be used to selectively visit dictionary elements while maintaining the visit context.
        /// </remarks>
        /// <param name="property">The property this method extends.</param>
        /// <param name="visitor">The visitor to use.</param>
        /// <param name="container">The container to visit.</param>
        /// <param name="key">The key to visit at.</param>
        /// <typeparam name="TValue">The value type for this property.</typeparam>
        /// <exception cref="IndexOutOfRangeException">The given index could not be resolved.</exception>
        /// <exception cref="InvalidOperationException">The given container is not indexable.</exception>
        public static void Visit<TValue>(this IProperty property, PropertyVisitor visitor, ref TValue container, object key)
        {
            var properties = PropertyBagStore.GetPropertyBag<TValue>();

            if (properties is IPropertyKeyable<TValue, object> keyable)
            {
                if (keyable.TryGetProperty(ref container, key, out var p))
                {
                    using (((IAttributes) p).CreateAttributesScope((IAttributes) property))
                    {
                        ((IPropertyAccept<TValue>) p).Accept(visitor, ref container);
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException($"Failed to visit element at key, not property was found for Key=[{key}].");
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to visit element at index, container is not keyable.");
            }
        }
        
        /// <summary>
        /// Call this method to visit all the elements of the given value.
        /// </summary>
        /// <param name="property">The property this method extends.</param>
        /// <param name="visitor">The property visitor to use.</param>
        /// <param name="value">The value to visit.</param>
        /// <typeparam name="TValue">The value type for this property.</typeparam>
        internal static void Visit<TValue>(this IProperty property, IPropertyBagVisitor visitor, ref TValue value)
        {
            PropertyContainer.Visit(ref value, visitor, out _);
        }
    }
    
    /// <summary>
    /// A set of property extension methods for attributes.
    /// </summary>
    public static class PropertyAttributeExtensions
    {
        /// <summary>
        /// Adds the specified attribute to the property.
        /// </summary>
        /// <param name="property">The property this method extends.</param>
        /// <param name="attribute">The attribute to add.</param>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <returns>The property this method extends.</returns>
        /// <exception cref="InvalidOperationException">The property does not implement attributes.</exception>
        public static TProperty WithAttribute<TProperty>(this TProperty property, Attribute attribute)
            where TProperty : IProperty
        {
            if (property is IAttributes accessor)
            {
                accessor.AddAttribute(attribute);
            }
            else
            {
                throw new InvalidOperationException();
            }
            
            return property;
        }
        
        /// <summary>
        /// Adds the specified attributes to the property.
        /// </summary>
        /// <param name="property">The property this method extends.</param>
        /// <param name="attributes">The attributes to add.</param>
        /// <typeparam name="TProperty">The property type.</typeparam>
        /// <returns>The property this method extends.</returns>
        /// <exception cref="InvalidOperationException">The property does not implement attributes.</exception>
        public static TProperty WithAttributes<TProperty>(this TProperty property, IEnumerable<Attribute> attributes)
            where TProperty : IProperty
        {
            if (property is IAttributes accessor)
            {
                accessor.AddAttributes(attributes);
            }
            else
            {
                throw new InvalidOperationException();
            }
            
            return property;
        }
    }
}
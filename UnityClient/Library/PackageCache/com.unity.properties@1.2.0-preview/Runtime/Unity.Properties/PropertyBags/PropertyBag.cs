using System;
using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Static registration for property bags.
    /// </summary>
    public static class PropertyBag
    {
        /// <summary>
        /// Registers a strongly typed <see cref="ContainerPropertyBag{TContainer}"/> for a type.
        /// </summary>
        /// <param name="propertyBag">The <see cref="ContainerPropertyBag{TContainer}"/> to register.</param>
        /// <typeparam name="TContainer">The container type this property bag describes.</typeparam>
        public static void Register<TContainer>(ContainerPropertyBag<TContainer> propertyBag)
        {
            PropertyBagStore.AddPropertyBag(propertyBag);
        }

        /// <summary>
        /// Registers an internal list property bag for the given types.
        /// </summary>
        /// <typeparam name="TContainer">The container type to register.</typeparam>
        /// <typeparam name="TList">The generic list type to register.</typeparam>
        /// <typeparam name="TElement">The element type to register.</typeparam>
        public static void RegisterList<TContainer, TList, TElement>()
            where TList : IList<TElement>
        {
            AOT.ListPropertyGenerator<TContainer, TList, TElement>.Preserve();
            
            if (PropertyBagStore.TypedStore<IPropertyBag<TList>>.PropertyBag == null)
            {
                PropertyBagStore.AddPropertyBag(new ListPropertyBag<TList, TElement>());
            }
        }
        
        /// <summary>
        /// Registers an internal list property bag for the given types.
        /// </summary>
        /// <typeparam name="TContainer">The container type to register.</typeparam>
        /// <typeparam name="TSet">The generic set type to register.</typeparam>
        /// <typeparam name="TElement">The element type to register.</typeparam>
        public static void RegisterSet<TContainer, TSet, TElement>()
            where TSet : ISet<TElement>
        {
            AOT.SetPropertyGenerator<TContainer, TSet, TElement>.Preserve();
            
            if (PropertyBagStore.TypedStore<IPropertyBag<TSet>>.PropertyBag == null)
            {
                PropertyBagStore.AddPropertyBag(new SetPropertyBag<TSet, TElement>());
            }
        }
        
        /// <summary>
        /// Registers an internal dictionary property bag for the given types.
        /// </summary>
        /// <typeparam name="TContainer">The container type to register.</typeparam>
        /// <typeparam name="TDictionary">The generic dictionary type to register.</typeparam>
        /// <typeparam name="TKey">The key type to register.</typeparam>
        /// <typeparam name="TValue">The value type to register.</typeparam>
        public static void RegisterDictionary<TContainer, TDictionary, TKey, TValue>()
            where TDictionary : IDictionary<TKey, TValue>
        {
            AOT.DictionaryPropertyGenerator<TContainer, TDictionary, TKey, TValue>.Preserve();
            
            if (PropertyBagStore.TypedStore<IPropertyBag<TDictionary>>.PropertyBag == null)
            {
                PropertyBagStore.AddPropertyBag(new DictionaryPropertyBag<TDictionary, TKey, TValue>());
                PropertyBagStore.AddPropertyBag(new KeyValuePairPropertyBag<TKey, TValue>());
            }
        }
        
        internal static void AcceptWithSpecializedVisitor<TContainer>(IPropertyBag<TContainer> properties, IVisitor visitor, ref TContainer container)
        {
            switch (properties)
            {
                case IDictionaryPropertyBagAccept<TContainer> accept when visitor is IDictionaryPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                case IListPropertyBagAccept<TContainer> accept when visitor is IListPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                case ISetPropertyBagAccept<TContainer> accept when visitor is ISetPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                case ICollectionPropertyBagAccept<TContainer> accept when visitor is ICollectionPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                case IPropertyBagAccept<TContainer> accept when visitor is IPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                default:
                    throw new ArgumentException($"{visitor.GetType()} does not implement any IPropertyBagAccept<T> interfaces.");
            }
        }
    }
    
    /// <summary>
    /// Base class for implementing a property bag for a specified container type. This is an abstract class.
    /// </summary>
    /// <remarks>
    /// This is used as the base class internally and should NOT be extended.
    ///
    /// When implementing custom property bags use:
    /// * <seealso cref="ContainerPropertyBag{TContainer}"/>.
    /// * <seealso cref="ListPropertyBag{TContainer,TValue}"/>.
    /// </remarks>
    /// <typeparam name="TContainer">The container type.</typeparam>
    public abstract class PropertyBag<TContainer> : IPropertyBag<TContainer>
    {
        static PropertyBag()
        {
            AOT.PropertyBagGenerator<TContainer>.Preserve();
        
            if (!RuntimeTypeInfoCache.IsContainerType(typeof(TContainer)))
            {
                throw new InvalidOperationException($"Failed to create a property bag for Type=[{typeof(TContainer)}]. The type is not a valid container type.");
            }
        }

        /// <remarks>
        /// This method exists to block direct use of this class externally.
        /// </remarks>
        internal PropertyBag() { }
        
        /// <inheritdoc/>
        void IPropertyBag.Register()
        {
            PropertyBagStore.AddPropertyBag(this);
        }

        /// <summary>
        /// Accepts visitation from a specified <see cref="IContainerTypeVisitor"/>.
        /// </summary>
        /// <param name="visitor">The visitor handling visitation.</param>
        /// <exception cref="ArgumentNullException">The visitor is null.</exception>
        void IContainerTypeAccept.Accept(IContainerTypeVisitor visitor)
        {
            if (null == visitor)
            {
                throw new ArgumentNullException(nameof(visitor));
            }
            
            visitor.Visit<TContainer>();
        }

        /// <summary>
        /// Accepts visitation from a specified <see cref="IPropertyBagVisitor"/> using an object as the container.
        /// </summary>
        /// <param name="visitor">The visitor handling the visitation.</param>
        /// <param name="container">The container being visited.</param>
        /// <exception cref="ArgumentNullException">The container is null.</exception>
        /// <exception cref="InvalidCastException">The container type does not match the property bag type.</exception>
        void IPropertyBagAccept.Accept(IVisitor visitor, ref object container)
        {
            if (null == container)
            {
                throw new ArgumentNullException(nameof(container));
            }
            
            if (!(container is TContainer typedContainer))
            {
                throw new ArgumentException($"The given ContainerType=[{container.GetType()}] does not match the PropertyBagType=[{typeof(TContainer)}]");
            }

            PropertyBag.AcceptWithSpecializedVisitor(this, visitor, ref typedContainer);
            
            container = typedContainer;
        }

        /// <summary>
        /// Accepts visitation from a specified <see cref="IPropertyBagVisitor"/> using a strongly typed container.
        /// </summary>
        /// <param name="visitor">The visitor handling the visitation.</param>
        /// <param name="container">The container being visited.</param>
        void IPropertyBagAccept<TContainer>.Accept(IPropertyBagVisitor visitor, ref TContainer container)
        {
            visitor.Visit(this, ref container);
        }

        /// <inheritdoc/>
        IEnumerable<IProperty<TContainer>> IPropertyEnumerable<TContainer>.GetProperties(ref TContainer container)
        {
            return GetProperties(ref container);
        }

        /// <summary>
        /// Implement this method to returns an enumerator that iterates through all properties for the <typeparamref name="TContainer"/>.
        /// </summary>
        /// <param name="container">The container hosting the data.</param>
        /// <returns>A <see cref="IEnumerator{IProperty}"/> structure for each property.</returns>
        internal abstract IEnumerable<IProperty<TContainer>> GetProperties(ref TContainer container);
    }
}
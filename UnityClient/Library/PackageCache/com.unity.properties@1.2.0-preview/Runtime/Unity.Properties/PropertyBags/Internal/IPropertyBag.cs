using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// This interface provides access to all <see cref="IProperty{TContainer}"/> objects of a <see cref="IPropertyBag{TContainer}"/>. This is an internal interface.
    /// </summary>
    /// <typeparam name="TContainer">The container type to access.</typeparam>
    interface IPropertyEnumerable<TContainer>
    {
        /// <summary>
        /// Returns an enumerator that iterates through all properties for the given container.
        /// </summary>
        /// <remarks>
        /// If the container is a collection type all elements will be iterated.
        /// </remarks>
        /// <param name="container">The container hosting the data.</param>
        /// <returns>A <see cref="IEnumerator{IProperty}"/> structure for all properties.</returns>
        IEnumerable<IProperty<TContainer>> GetProperties(ref TContainer container);
    }

    /// <summary>
    /// This interface provides access to all <see cref="IProperty{TContainer}"/> objects of a <see cref="IPropertyBag{TContainer}"/> as an <see cref="ICollection"/>. This is an internal interface.
    /// </summary>
    /// <typeparam name="TContainer">The container type to access.</typeparam>
    interface IPropertyList<TContainer>
    {
        /// <summary>
        /// Returns a collection for all properties.
        /// </summary>
        /// <remarks>
        /// If the container is a collection type all elements will be iterated.
        /// </remarks>
        /// <param name="container">The container hosting the data.</param>
        /// <returns>A <see cref="ICollection{IProperty}"/> structure for all properties.</returns>
        List<IProperty<TContainer>> GetProperties(ref TContainer container);
    }

    /// <summary>
    /// This interface provides access to <see cref="IProperty{TContainer}"/> of a <see cref="IPropertyBag{TContainer}"/> by index. This is an internal interface.
    /// </summary>
    /// <typeparam name="TContainer">The container type to access.</typeparam>
    interface IPropertyIndexable<TContainer> 
    {
        /// <summary>
        /// Gets the property associated with the specified index.
        /// </summary>
        /// <param name="container">The container hosting the data.</param>
        /// <param name="index">The index of the property to get.</param>
        /// <param name="property">When this method returns, contains the property associated with the specified index, if the name is found; otherwise, null.</param>
        /// <returns><see langword="true"/> if the <see cref="IPropertyIndexable{TContainer}"/> contains a property for the specified index; otherwise, <see langword="false"/>.</returns>
        bool TryGetProperty(ref TContainer container, int index, out IProperty<TContainer> property);
    }

    /// <summary>
    /// This interface provides access to <see cref="IProperty{TContainer}"/> of a <see cref="IPropertyBag{TContainer}"/> by name. This is an internal interface.
    /// </summary>
    /// <typeparam name="TContainer">The container type to access.</typeparam>
    interface IPropertyNameable<TContainer>
    {
        /// <summary>
        /// Gets the property associated with the specified name.
        /// </summary>
        /// <param name="container">The container hosting the data.</param>
        /// <param name="name">The name of the property to get.</param>
        /// <param name="property">When this method returns, contains the property associated with the specified name, if the name is found; otherwise, null.</param>
        /// <returns><see langword="true"/> if the <see cref="IPropertyNameable{TContainer}"/> contains a property with the specified name; otherwise, <see langword="false"/>.</returns>
        bool TryGetProperty(ref TContainer container, string name, out IProperty<TContainer> property);
    }

    /// <summary>
    /// This interface provides access to <see cref="IProperty{TContainer}"/> of a <see cref="IPropertyBag{TContainer}"/> by an object key. This is an internal interface.
    /// </summary>
    /// <typeparam name="TContainer">The container type to access.</typeparam>
    /// <typeparam name="TKey">The key type to access the property with.</typeparam>
    interface IPropertyKeyable<TContainer, TKey>
    {
        /// <summary>
        /// Gets the property associated with the specified name.
        /// </summary>
        /// <param name="container">The container hosting the data.</param>
        /// <param name="key">The key to lookup.</param>
        /// <param name="property">When this method returns, contains the property associated with the specified name, if the name is found; otherwise, null.</param>
        /// <returns><see langword="true"/> if the <see cref="IPropertyNameable{TContainer}"/> contains a property with the specified name; otherwise, <see langword="false"/>.</returns>
        bool TryGetProperty(ref TContainer container, TKey key, out IProperty<TContainer> property);
    }
    
    /// <summary>
    /// Base untyped interface for implementing property bags. This is an internal interface.
    /// </summary>
    interface IPropertyBag : IPropertyBagAccept, IContainerTypeAccept
    {
        /// <summary>
        /// Statically registers the property bag through the <see cref="PropertyBagStore"/>.
        /// </summary>
        void Register();
    }

    /// <summary>
    /// Base untyped interface for implementing collection based property bags. This is an internal interface.
    /// </summary>
    interface ICollectionPropertyBag : IPropertyBag
    {
        
    }
    
    /// <summary>
    /// Base typed interface for implementing property bags. This is an internal interface.
    /// </summary>
    interface IPropertyBag<TContainer> : IPropertyBag, IPropertyBagAccept<TContainer>, IPropertyEnumerable<TContainer>
    {
    }
    
    /// <summary>
    /// Base untyped interface for implementing collection based property bags. This is an internal interface.
    /// </summary>
    interface ICollectionPropertyBag<TCollection, TElement> : IPropertyBag<TCollection>, ICollectionPropertyBag, ICollectionPropertyBagAccept<TCollection>
        where TCollection : ICollection<TElement>
    {
        
    }
    
    /// <summary>
    /// Base typed interface for implementing list based property bags. This is an internal interface.
    /// </summary>
    interface IListPropertyBag<TList, TElement> : ICollectionPropertyBag<TList, TElement>, IListPropertyBagAccept<TList>, IListPropertyAccept<TList>, IPropertyIndexable<TList>
        where TList : IList<TElement>
    {
        /// <summary>
        /// Returns the internal property collection for the <see cref="ListPropertyBag{TList,TElement}"/>. This is an internal method.
        /// </summary>
        /// <remarks>
        /// This method exists to avoid boxing when enumerating element properties.
        /// </remarks>
        /// <param name="list">The container being visited.</param>
        /// <returns>The internal property collection.</returns>
        new ListPropertyBag<TList, TElement>.PropertyCollection GetProperties(ref TList list);
    }
    
    /// <summary>
    /// Base typed interface for implementing dictionary based property bags. This is an internal interface.
    /// </summary>
    interface ISetPropertyBag<TSet, TElement> : ICollectionPropertyBag<TSet, TElement>, ISetPropertyBagAccept<TSet>, ISetPropertyAccept<TSet>
        where TSet : ISet<TElement>
    {
    }
    
    /// <summary>
    /// Base typed interface for implementing dictionary based property bags. This is an internal interface.
    /// </summary>
    interface IDictionaryPropertyBag<TDictionary, TKey, TValue> : ICollectionPropertyBag<TDictionary, KeyValuePair<TKey, TValue>>, IDictionaryPropertyBagAccept<TDictionary>, IDictionaryPropertyAccept<TDictionary>, IPropertyKeyable<TDictionary, object>
        where TDictionary : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Returns the internal property collection for the <see cref="DictionaryPropertyBag{TDictionary,TKey,TValue}"/>. This is an internal method.
        /// </summary>
        /// <remarks>
        /// This method exists to avoid boxing when enumerating element properties.
        /// </remarks>
        /// <param name="list">The container being visited.</param>
        /// <returns>The internal property collection.</returns>
        new DictionaryPropertyBag<TDictionary,TKey,TValue>.PropertyCollection GetProperties(ref TDictionary list);
    }
}
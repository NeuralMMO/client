using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// Base interface for all visitation handlers.
    /// </summary>
    interface IVisitor
    {
        
    }

    /// <summary>
    /// Interface for visiting a container type. This is an internal interface.
    /// </summary>
    interface IContainerTypeVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a container type.
        /// </summary>
        /// <remarks>
        /// This method is invoked by <see cref="IContainerTypeAccept.Accept(IVisitor)"/>.
        /// </remarks>
        /// <typeparam name="TContainer">The container type.</typeparam>
        void Visit<TContainer>();
    }

    /// <summary>
    /// Interface for visiting property bags. This is an internal interface.
    /// </summary>
    /// <remarks>
    /// <seealso cref="IPropertyBagAccept" />
    /// <seealso cref="IPropertyBagAccept{TContainer}" />
    /// </remarks>
    interface IPropertyBagVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a collection of properties.
        /// </summary>
        /// <remarks>
        /// This method is invoked by <see cref="IPropertyBagAccept{TContainer}.Accept(IPropertyBagVisitor,ref TContainer)"/>.
        /// </remarks>
        /// <param name="properties">The properties of the container.</param>
        /// <param name="container">The container being visited.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        void Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container);
    }
    
    /// <summary>
    /// Interface for visiting property bags. This is an internal interface.
    /// </summary>
    interface ICollectionPropertyBagVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a collection of properties.
        /// </summary>
        /// <remarks>
        /// This method is invoked by <see cref="ICollectionPropertyBagAccept{TContainer}.Accept(ICollectionPropertyBagVisitor,ref TContainer)"/>.
        /// </remarks>
        /// <param name="properties">The properties of the container.</param>
        /// <param name="container">The container being visited.</param>
        /// <typeparam name="TCollection">The list type.</typeparam>
        /// <typeparam name="TElement">The element type.</typeparam>
        void Visit<TCollection, TElement>(ICollectionPropertyBag<TCollection, TElement> properties, ref TCollection container) where TCollection : ICollection<TElement>;
    }
    
    /// <summary>
    /// Interface for visiting property bags. This is an internal interface.
    /// </summary>
    interface IListPropertyBagVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a collection of properties.
        /// </summary>
        /// <remarks>
        /// This method is invoked by <see cref="IListPropertyBagAccept{TContainer}.Accept(IListPropertyBagVisitor,ref TContainer)"/>.
        /// </remarks>
        /// <param name="properties">The properties of the container.</param>
        /// <param name="container">The container being visited.</param>
        /// <typeparam name="TList">The list type.</typeparam>
        /// <typeparam name="TElement">The element type.</typeparam>
        void Visit<TList, TElement>(IListPropertyBag<TList, TElement> properties, ref TList container) where TList : IList<TElement>;
    }
    
    /// <summary>
    /// Interface for visiting property bags. This is an internal interface.
    /// </summary>
    interface ISetPropertyBagVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a collection of properties.
        /// </summary>
        /// <remarks>
        /// This method is invoked by <see cref="ISetPropertyBagAccept{TContainer}.Accept(ISetPropertyBagVisitor,ref TContainer)"/>.
        /// </remarks>
        /// <param name="properties">The properties of the container.</param>
        /// <param name="container">The container being visited.</param>
        /// <typeparam name="TSet">The set type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        void Visit<TSet, TValue>(ISetPropertyBag<TSet, TValue> properties, ref TSet container) where TSet : ISet<TValue>;
    }
    
    /// <summary>
    /// Interface for visiting property bags. This is an internal interface.
    /// </summary>
    interface IDictionaryPropertyBagVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a collection of properties.
        /// </summary>
        /// <remarks>
        /// This method is invoked by <see cref="IDictionaryPropertyBagAccept{TContainer}.Accept(IDictionaryPropertyBagVisitor,ref TContainer)"/>.
        /// </remarks>
        /// <param name="properties">The properties of the container.</param>
        /// <param name="container">The container being visited.</param>
        /// <typeparam name="TDictionary">The dictionary type.</typeparam>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        void Visit<TDictionary, TKey, TValue>(IDictionaryPropertyBag<TDictionary, TKey, TValue> properties, ref TDictionary container) where TDictionary : IDictionary<TKey, TValue>;
    }
    
    /// <summary>
    /// Interface for receiving strongly typed property callbacks. This is an internal interface.
    /// </summary>
    /// <remarks>
    /// <seealso cref="IPropertyAccept{TContainer}" />
    /// </remarks>
    interface IPropertyVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a specific property.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <see cref="IPropertyAccept{TContainer}.Accept(IPropertyVisitor,ref TContainer)"/>
        /// </remarks>
        /// <param name="property">The property being visited.</param>
        /// <param name="container">The container being visited.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        void Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container);
    }

    /// <summary>
    /// Interface for receiving strongly typed property callbacks for collections. This is an internal interface.
    /// </summary>
    /// <remarks>
    /// <seealso cref="ICollectionPropertyAccept{TList}" />
    /// </remarks>
    interface ICollectionPropertyVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a specialized collection property.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <see cref="ICollectionPropertyBag{TList}.Accept{TContainer}"/>
        /// </remarks>
        /// <param name="property">The property being visited.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="collection">The collection value.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TCollection">The collection value type.</typeparam>
        /// <typeparam name="TElement">The collection element type.</typeparam>
        void Visit<TContainer, TCollection, TElement>(Property<TContainer, TCollection> property, ref TContainer container, ref TCollection collection)
            where TCollection : ICollection<TElement>;
    }
    
    /// <summary>
    /// Interface for receiving strongly typed property callbacks for lists. This is an internal interface.
    /// </summary>
    /// <remarks>
    /// <seealso cref="IListPropertyAccept{TList}" />
    /// </remarks>
    interface IListPropertyVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a specialized list property.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <see cref="IListPropertyBag{TList}.Accept{TContainer}"/>
        /// </remarks>
        /// <param name="property">The property being visited.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="list">The list value.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TList">The list value type.</typeparam>
        /// <typeparam name="TElement">The collection element type.</typeparam>
        void Visit<TContainer, TList, TElement>(Property<TContainer, TList> property, ref TContainer container, ref TList list)
            where TList : IList<TElement>;
    }
    
    /// <summary>
    /// Interface for receiving strongly typed property callbacks for sets. This is an internal interface.
    /// </summary>
    /// <remarks>
    /// <seealso cref="ISetPropertyAccept{TSet}" />
    /// </remarks>
    interface ISetPropertyVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a specialized set property.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <see cref="ISetPropertyBag{THashSet}.Accept{TContainer}"/>
        /// </remarks>
        /// <param name="property">The property being visited.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="set">The hash set value.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TSet">The set value type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        void Visit<TContainer, TSet, TValue>(Property<TContainer, TSet> property, ref TContainer container, ref TSet set)
            where TSet : ISet<TValue>;
    }
    
    /// <summary>
    /// Interface for receiving strongly typed property callbacks for dictionaries. This is an internal interface.
    /// </summary>
    /// <remarks>
    /// <seealso cref="IDictionaryPropertyAccept{TList}" />
    /// </remarks>
    interface IDictionaryPropertyVisitor : IVisitor
    {
        /// <summary>
        /// Implement this method to accept visitation for a specialized dictionary property.
        /// </summary>
        /// <remarks>
        /// This method is invoked from <see cref="IDictionaryPropertyBag{TDictionary}.Accept{TContainer}"/>
        /// </remarks>
        /// <param name="property">The property being visited.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="dictionary">The dictionary value.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TDictionary">The dictionary value type.</typeparam>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        void Visit<TContainer, TDictionary, TKey, TValue>(Property<TContainer, TDictionary> property, ref TContainer container, ref TDictionary dictionary)
            where TDictionary : IDictionary<TKey, TValue>;
    }
}
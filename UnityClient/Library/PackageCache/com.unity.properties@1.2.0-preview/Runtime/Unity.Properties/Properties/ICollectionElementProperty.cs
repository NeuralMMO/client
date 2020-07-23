namespace Unity.Properties
{
    /// <summary>
    /// Base interface for working with a collection element property.
    /// </summary>
    public interface ICollectionElementProperty
    {
        
    }
    
    /// <summary>
    /// Interface over a property representing a list element.
    /// </summary>
    public interface IListElementProperty : ICollectionElementProperty
    {
        /// <summary>
        /// The index of this property in the list.
        /// </summary>
        int Index { get; }
    }    
    
    /// <summary>
    /// Interface over a property representing a untyped dictionary element.
    /// </summary>
    public interface IDictionaryElementProperty : ICollectionElementProperty
    {
        /// <summary>
        /// The key of this property in the dictionary.
        /// </summary>
        object ObjectKey { get; }
    }
    
    /// <summary>
    /// Interface over a property representing a typed dictionary element.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    public interface IDictionaryElementProperty<out TKey> : IDictionaryElementProperty
    {
        /// <summary>
        /// The key of this property in the dictionary.
        /// </summary>
        TKey Key { get; }
    }
}
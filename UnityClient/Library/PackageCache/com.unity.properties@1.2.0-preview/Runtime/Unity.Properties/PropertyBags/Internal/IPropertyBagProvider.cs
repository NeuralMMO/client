namespace Unity.Properties.Internal
{
    /// <summary>
    /// Interface to allow external assemblies to provide dynamic property bags. This is an internal interface.
    /// </summary>
    interface IPropertyBagProvider
    {
        /// <summary>
        /// Implement this method to generate a dynamic property bag for the given container type.
        /// </summary>
        /// <param name="type">The container type to create a property bag for.</param>
        /// <returns>A <see cref="IPropertyBag"/> instance for the given container type</returns>
        IPropertyBag CreatePropertyBag(System.Type type);
    }
}
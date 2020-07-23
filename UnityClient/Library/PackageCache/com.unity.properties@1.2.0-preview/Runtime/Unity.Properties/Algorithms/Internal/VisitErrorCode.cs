namespace Unity.Properties.Internal
{
    /// <summary>
    /// Internal error code used during path visitation.
    /// </summary>
    enum VisitErrorCode
    {
        /// <summary>
        /// The path was resolved successfully.
        /// </summary>
        Ok,
        
        /// <summary>
        /// The container being visited was null.
        /// </summary>
        NullContainer,
        
        /// <summary>
        /// The given container type is not valid for visitation.
        /// </summary>
        InvalidContainerType,
        
        /// <summary>
        /// No property bag was found for the given container type.
        /// </summary>
        MissingPropertyBag,
        
        /// <summary>
        /// Failed to resolve some part of the path (e.g. Name, Index or Key).
        /// </summary>
        InvalidPath,
        
        /// <summary>
        /// Failed to reinterpret the target value as the requested type.
        /// </summary>
        InvalidCast
    }
}
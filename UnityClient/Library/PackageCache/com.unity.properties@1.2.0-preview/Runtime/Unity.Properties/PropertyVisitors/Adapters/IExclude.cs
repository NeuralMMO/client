namespace Unity.Properties
{
    namespace Adapters
    {
        /// <summary>
        /// Implement this interface to filter visitation for a specific <see cref="TContainer"/> and <see cref="TValue"/> pair.
        /// </summary>
        /// <typeparam name="TContainer">The container type being visited.</typeparam>
        /// <typeparam name="TValue">The value type being visited.</typeparam>
        public interface IExclude<TContainer, TValue> : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters specific a <see cref="TContainer"/> and <see cref="TValue"/> pair.
            /// </summary>
            /// <param name="property">The property being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            /// <returns><see langword="true"/> if visitation should be skipped, <see langword="false"/> otherwise.</returns>
            bool IsExcluded(Property<TContainer, TValue> property, ref TContainer container, ref TValue value);
        }
    
        /// <summary>
        /// Implement this interface to filter visitation for a specific <see cref="TValue"/> type.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        public interface IExclude<TValue> : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters specific a <see cref="TValue"/>.
            /// </summary>
            /// <param name="property">The property being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            /// <typeparam name="TContainer">The container type being visited.</typeparam>
            /// <returns><see langword="true"/> if visitation should be skipped, <see langword="false"/> otherwise.</returns>
            bool IsExcluded<TContainer>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value);
        }
    
        /// <summary>
        /// Implement this interface to filter visitation.
        /// </summary>
        public interface IExclude : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters any property.
            /// </summary>
            /// <param name="property">The property being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            /// <typeparam name="TContainer">The container type being visited.</typeparam>
            /// <typeparam name="TValue">The value type being visited.</typeparam>
            /// <returns><see langword="true"/> if visitation should be skipped, <see langword="false"/> otherwise.</returns>
            bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value);
        }
    }
    
    namespace Adapters.Contravariant
    {
        /// <summary>
        /// Implement this interface to filter visitation for a specific <see cref="TContainer"/> and <see cref="TValue"/> pair.
        /// </summary>
        /// <typeparam name="TContainer">The container type being visited.</typeparam>
        /// <typeparam name="TValue">The value type being visited.</typeparam>
        public interface IExclude<TContainer, in TValue> : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters specific a <see cref="TContainer"/> and <see cref="TValue"/> pair.
            /// </summary>
            /// <param name="property">The property being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            /// <returns><see langword="true"/> if visitation should be skipped, <see langword="false"/> otherwise.</returns>
            bool IsExcluded(IProperty<TContainer> property, ref TContainer container, TValue value);
        }
    
        /// <summary>
        /// Implement this interface to filter visitation for a specific <see cref="TValue"/> type.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        public interface IExclude<in TValue> : IPropertyVisitorAdapter
        {
            /// <summary>
            /// Invoked when the visitor encounters any property.
            /// </summary>
            /// <param name="property">The property being visited.</param>
            /// <param name="container">The container being visited.</param>
            /// <param name="value">The value being visited.</param>
            /// <typeparam name="TContainer">The container type being visited.</typeparam>
            /// <returns><see langword="true"/> if visitation should be skipped, <see langword="false"/> otherwise.</returns>
            bool IsExcluded<TContainer>(IProperty<TContainer> property, ref TContainer container, TValue value);
        }
    }
}
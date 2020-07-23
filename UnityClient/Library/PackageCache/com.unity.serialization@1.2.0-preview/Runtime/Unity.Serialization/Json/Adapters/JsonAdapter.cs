namespace Unity.Serialization.Json.Adapters
{
    /// <summary>
    /// Base interface for json adapters.
    /// </summary>
    public interface IJsonAdapter
    {
        
    }

    /// <summary>
    /// Implement this interface to override serialization and deserialization behaviour for a given type.
    /// </summary>
    /// <typeparam name="TValue">The type to override serialization for.</typeparam>
    public interface IJsonAdapter<TValue> : IJsonAdapter
    {
        /// <summary>
        /// Invoked during serialization to handle writing out the specified <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="value">The value to write.</param>
        void Serialize(JsonStringBuffer writer, TValue value);
        
        /// <summary>
        /// Invoked during deserialization to handle reading the specified <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="view">The view to read from.</param>
        /// <returns>The deserialized value.</returns>
        TValue Deserialize(SerializedValueView view);
    }

    namespace Contravariant
    {
        /// <summary>
        /// Implement this interface to override serialization and deserialization behaviour for a given type.
        /// </summary>
        /// <typeparam name="TValue">The type to override serialization for.</typeparam>
        public interface IJsonAdapter<in TValue> : IJsonAdapter
        {
            /// <summary>
            /// Invoked during serialization to handle writing out the specified <typeparamref name="TValue"/>.
            /// </summary>
            /// <param name="writer">The stream to write to.</param>
            /// <param name="value">The value to write.</param>
            void Serialize(JsonStringBuffer writer, TValue value);
            
            /// <summary>
            /// Invoked during deserialization to handle reading the specified <typeparamref name="TValue"/>.
            /// </summary>
            /// <param name="view">The view to read from.</param>
            /// <returns>The deserialized value.</returns>
            object Deserialize(SerializedValueView view);
        }
    }
}
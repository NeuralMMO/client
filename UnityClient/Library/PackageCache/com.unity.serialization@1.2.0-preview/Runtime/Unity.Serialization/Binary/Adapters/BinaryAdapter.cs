using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization.Binary.Adapters
{
    /// <summary>
    /// Base interface for binary adapters.
    /// </summary>
    public interface IBinaryAdapter
    {
        
    }
    
    /// <summary>
    /// Implement this interface to override serialization and deserialization behaviour for a given type.
    /// </summary>
    /// <typeparam name="TValue">The type to override serialization for.</typeparam>
    public unsafe interface IBinaryAdapter<TValue> : IBinaryAdapter
    {
        /// <summary>
        /// Invoked during serialization to handle writing out the specified <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="value">The value to write.</param>
        void Serialize(UnsafeAppendBuffer* writer, TValue value);
        
        /// <summary>
        /// Invoked during deserialization to handle reading the specified <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <returns>The deserialized value.</returns>
        TValue Deserialize(UnsafeAppendBuffer.Reader* reader);
    }

    namespace Contravariant
    {
        /// <summary>
        /// Implement this interface to override serialization and deserialization behaviour for a given type.
        /// </summary>
        /// <typeparam name="TValue">The type to override serialization for.</typeparam>
        public unsafe interface IBinaryAdapter<in TValue> : IBinaryAdapter
        {
            /// <summary>
            /// Invoked during serialization to handle writing out the specified <typeparamref name="TValue"/>.
            /// </summary>
            /// <param name="writer">The stream to write to.</param>
            /// <param name="value">The value to write.</param>
            void Serialize(UnsafeAppendBuffer* writer, TValue value);
            
            /// <summary>
            /// Invoked during deserialization to handle reading the specified <typeparamref name="TValue"/>.
            /// </summary>
            /// <param name="reader">The stream to read from.</param>
            /// <returns>The deserialized value.</returns>
            object Deserialize(UnsafeAppendBuffer.Reader* reader);
        }
    }
}
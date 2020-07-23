using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;
using Unity.Properties.Internal;

namespace Unity.Serialization.Binary
{
    static unsafe partial class BinarySerialization
    {
        /// <summary>
        /// Deserializes from the specified stream and returns a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="parameters">The parameters to use when reading.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>A new instance of <typeparamref name="T"/> constructed from the serialized data.</returns>
        public static T FromBinary<T>(UnsafeAppendBuffer.Reader* stream, BinarySerializationParameters parameters = default)
        {
            var context = parameters.Context ?? (parameters.RequiresThreadSafety ? new BinarySerializationContext() : GetSharedContext());
            var visitor = context.GetBinaryPropertyReader();
            
            visitor.SetStream(stream);
            visitor.SetSerializedType(parameters.SerializedType);
            visitor.SetDisableRootAdapters(parameters.DisableRootAdapters);
            visitor.SetGlobalAdapters(GetGlobalAdapters());
            visitor.SetUserDefinedAdapters(parameters.UserDefinedAdapters);
            visitor.SetSerializedReferences(parameters.DisableSerializedReferences ? default : context.GetSerializedReferences());
            
            var container = new PropertyWrapper<T>(default);
            using (visitor.Lock()) PropertyContainer.Visit(ref container, visitor);
            return container.Value;
        }
    }
}
using System.IO;
using Unity.Collections;
using Unity.Properties;
using Unity.Properties.Internal;

namespace Unity.Serialization.Json
{
    public static partial class JsonSerialization
    {
        static readonly JsonPropertyWriter s_SharedJsonPropertyWriter = new JsonPropertyWriter();
        
        static JsonPropertyWriter GetSharedJsonPropertyWriter()
        {
            return s_SharedJsonPropertyWriter;
        }
        
        /// <summary>
        /// Serializes the given object to a json file at the specified path.
        /// </summary>
        /// <param name="file">The file to write to.</param>
        /// <param name="container">The object to serialize.</param>
        /// <param name="parameters">The parameters to use when writing.</param>
        /// <typeparam name="T">The type to serialize.</typeparam>
        public static void ToJson<T>(FileInfo file, T container, JsonSerializationParameters parameters = default)
        {
            File.WriteAllText(file.FullName, ToJson(container, parameters));
        }

        /// <summary>
        /// Writes a property container to a json string.
        /// </summary>
        /// <param name="value">The container to write.</param>
        /// <param name="parameters">The parameters to use when writing.</param>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <returns>A json string.</returns>
        public static string ToJson<T>(T value, JsonSerializationParameters parameters = default)
        {
            using (var writer = new JsonStringBuffer(parameters.InitialCapacity, Allocator.Temp))
            {
                ToJson(writer, value, parameters);
                return writer.ToString();
            }
        }
        
        /// <summary>
        /// Writes a property container the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write the object to.</param>
        /// <param name="value">The container to write.</param>
        /// <param name="parameters">The parameters to use when writing.</param>
        /// <typeparam name="T">The type to serialize.</typeparam>
        public static void ToJson<T>(JsonStringBuffer buffer, T value, JsonSerializationParameters parameters = default)
        {
            var container = new PropertyWrapper<T>(value);
            
            var serializedReferences = default(SerializedReferences);

            if (!parameters.DisableSerializedReferences)
            {
                serializedReferences = parameters.RequiresThreadSafety ? new SerializedReferences() : GetSharedSerializedReferences();
                var serializedReferenceVisitor = parameters.RequiresThreadSafety ? new SerializedReferenceVisitor() : GetSharedSerializedReferenceVisitor();
                serializedReferenceVisitor.SetSerializedReference(serializedReferences);
                PropertyContainer.Visit(ref container, serializedReferenceVisitor);
            }

            var visitor = parameters.RequiresThreadSafety || s_SharedJsonPropertyWriter.IsLocked ? new JsonPropertyWriter() : GetSharedJsonPropertyWriter();
            
            visitor.SetStringWriter(buffer);
            visitor.SetSerializedType(parameters.SerializedType);
            visitor.SetDisableRootAdapters(parameters.DisableRootAdapters);
            visitor.SetGlobalAdapters(GetGlobalAdapters());
            visitor.SetUserDefinedAdapters(parameters.UserDefinedAdapters);
            visitor.SetGlobalMigrations(GetGlobalMigrations());
            visitor.SetUserDefinedMigration(parameters.UserDefinedMigrations);
            visitor.SetSerializedReferences(serializedReferences);
            visitor.SetMinified(parameters.Minified);
            visitor.SetSimplified(parameters.Simplified);
            
            using (visitor.Lock()) PropertyContainer.Visit(ref container, visitor);
        }
    }
}
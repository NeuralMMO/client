using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using Unity.Properties;
using Unity.Properties.Internal;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// The type of the event encountered during deserialization.
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// <see cref="EventType"/> used for errors.
        /// </summary>
        Error,
        
        /// <summary>
        /// <see cref="EventType"/> used for assertions.
        /// </summary>
        Assert,
        
        /// <summary>
        /// <see cref="EventType"/> used for warnings.
        /// </summary>
        Warning,
        
        /// <summary>
        /// <see cref="EventType"/> used for logs.
        /// </summary>
        Log,
        
        /// <summary>
        /// <see cref="EventType"/> used for exceptions.
        /// </summary>
        Exception
    }
    
    /// <summary>
    /// Structure to events that occur during deserialization.
    /// </summary>
    public readonly struct DeserializationEvent
    {
        /// <summary>
        /// The type of event.
        /// </summary>
        public readonly EventType Type;
        
        /// <summary>
        /// The payload for the event.
        /// </summary>
        public readonly object Payload;

        /// <summary>
        /// Initializes a new instance of <see cref="DeserializationEvent"/> with the specified parameters.
        /// </summary>
        /// <param name="type">The type of event that occured.</param>
        /// <param name="payload">The payload or message for the event.</param>
        internal DeserializationEvent(EventType type, object payload)
        {
            Type = type;
            Payload = payload;
        }
        
        /// <inheritdoc/>
        public override string ToString() => Payload.ToString();
    }
    
    /// <summary>
    /// Object containing the results of a deserialization. Use this to capture any errors or events.
    /// </summary>
    public readonly struct DeserializationResult
    {
        readonly List<DeserializationEvent> m_Events;

        /// <summary>
        /// Returns all <see cref="DeserializationEvent"/> that occured during deserialization.
        /// </summary>
        public IEnumerable<DeserializationEvent> Events => m_Events ?? Enumerable.Empty<DeserializationEvent>();

        /// <summary>
        /// Returns any events with <see cref="EventType.Log"/> that occured during deserialization.
        /// </summary>
        public IEnumerable<DeserializationEvent> Logs => Events.Where(e => e.Type == EventType.Log);
        
        /// <summary>
        /// Returns any events with <see cref="EventType.Error"/> that occured during deserialization.
        /// </summary>
        public IEnumerable<DeserializationEvent> Errors => Events.Where(e => e.Type == EventType.Error);
        
        /// <summary>
        /// Returns any events with <see cref="EventType.Warning"/> that occured during deserialization.
        /// </summary>
        public IEnumerable<DeserializationEvent> Warnings => Events.Where(e => e.Type == EventType.Warning);
        
        /// <summary>
        /// Returns any events with <see cref="EventType.Exception"/> that occured during deserialization.
        /// </summary>
        public IEnumerable<DeserializationEvent> Exceptions => Events.Where(e => e.Type == EventType.Exception);
        
        internal DeserializationResult(List<DeserializationEvent> events) => m_Events = events;
        
        /// <summary>
        /// Rethrows any errors encountered during deserialization. 
        /// </summary>
        /// <remarks>
        /// If a single exception was encountered the exception is re-thrown. If multiple exceptions were encountered a <see cref="AggregateException"/> is thrown.
        /// </remarks>
        public void Throw()
        {
            var exceptions = Events
                             .Where(e => e.Payload is Exception)
                             .Select(e => (Exception) e.Payload);
            
            if (exceptions.Count() == 1) 
                throw exceptions.First();

            throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Gets the status of the deserialization.
        /// </summary>
        /// <returns><see langword="true"/> if deserialization succeeded; otherwise, false.</returns>
        public bool DidSucceed() => !Events.Any(e => e.Type == EventType.Exception || e.Type == EventType.Error);
    }
    
    public static partial class JsonSerialization
    {
        static readonly JsonPropertyReader s_SharedJsonPropertyReader = new JsonPropertyReader();
        static readonly List<DeserializationEvent> s_SharedDeserializationEvents = new List<DeserializationEvent>();

        static JsonPropertyReader GetSharedJsonPropertyReader()
        {
            return s_SharedJsonPropertyReader;
        }
        
        static List<DeserializationEvent> GetSharedDeserializationEvents()
        {
            s_SharedDeserializationEvents.Clear();
            return s_SharedDeserializationEvents;
        }

        static SerializedObjectReaderConfiguration GetDefaultConfigurationForString(string json, JsonSerializationParameters parameters = default)
        {
            var configuration = SerializedObjectReaderConfiguration.Default;
            
            configuration.UseReadAsync = false;
            configuration.ValidationType = parameters.Simplified ? JsonValidationType.Simple : JsonValidationType.Standard;
            configuration.BlockBufferSize = math.max(json.Length * sizeof(char), 16);
            configuration.TokenBufferSize = math.max(json.Length / 2, 16);
            configuration.OutputBufferSize = math.max(json.Length * sizeof(char), 16);

            return configuration;
        }
        
        static SerializedObjectReaderConfiguration GetDefaultConfigurationForFile(FileInfo file, JsonSerializationParameters parameters = default)
        {
            var configuration = SerializedObjectReaderConfiguration.Default;

            if (!file.Exists)
            {
                throw new FileNotFoundException();
            }

            configuration.UseReadAsync = file.Length > 512 << 10;
            configuration.ValidationType = parameters.Simplified ? JsonValidationType.Simple : JsonValidationType.Standard;
            configuration.BlockBufferSize = math.min((int) file.Length, 512 << 10); // 512 kb max
            configuration.OutputBufferSize = math.min((int) file.Length, 1024 << 10); // 1 mb max

            return configuration;
        }
        
        static SerializedObjectReaderConfiguration GetDefaultConfigurationForStream(Stream stream, JsonSerializationParameters parameters = default)
        {
            var configuration = SerializedObjectReaderConfiguration.Default;

            configuration.UseReadAsync = stream.Length > 512 << 10;
            configuration.ValidationType = parameters.Simplified ? JsonValidationType.Simple : JsonValidationType.Standard;
            configuration.BlockBufferSize = math.min((int) stream.Length, 512 << 10); // 512 kb max
            configuration.OutputBufferSize = math.min((int) stream.Length, 1024 << 10); // 1 mb max

            return configuration;
        }
        
        /// <summary>
        /// Deserializes from the specified json string and returns a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="json">The json string to read from.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>A new instance of <typeparamref name="T"/> constructed from the serialized data.</returns>
        public static T FromJson<T>(string json, JsonSerializationParameters parameters = default)
        {
            if (!TryFromJson<T>(json, out var container, out var result, parameters))
            {
                result.Throw();
            }
            
            return container;
        }
        
        /// <summary>
        /// Deserializes from the specified json string and returns a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="json">The json string to read from.</param>
        /// <param name="container">When this method returns, contains the deserialized value.</param>
        /// <param name="result">The results structure containing any errors or exceptions.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>True if the deserialization succeeded; otherwise, false.</returns>
        public static bool TryFromJson<T>(string json, out T container, out DeserializationResult result, JsonSerializationParameters parameters = default)
        {
            container = default;
            return TryFromJsonOverride(json, ref container, out result, parameters);
        }

        /// <summary>
        /// Deserializes from the specified json string in to an existing instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="json">The json string to read from.</param>
        /// <param name="container">The reference to be overwritten.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        public static void FromJsonOverride<T>(string json, ref T container, JsonSerializationParameters parameters = default)
        {
            if (!TryFromJsonOverride(json, ref container, out var result, parameters))
            {
                result.Throw();
            }
        }

        /// <summary>
        /// Deserializes from the specified json string in to an existing instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="json">The json string to read from.</param>
        /// <param name="container">The reference to be overwritten.</param>
        /// <param name="result">The results structure containing any errors or exceptions.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>True if the deserialization succeeded; otherwise, false.</returns>
        public static bool TryFromJsonOverride<T>(string json, ref T container, out DeserializationResult result, JsonSerializationParameters parameters = default)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }
            
            unsafe
            {
                fixed (char* ptr = json)
                {
                    using (var reader = new SerializedObjectReader(new UnsafeBuffer<char>(ptr, json.Length), GetDefaultConfigurationForString(json, parameters)))
                    {
                        return TryFromJson(reader, ref container, out result, parameters);
                    }
                }
            }
        }
        
        /// <summary>
        /// Deserializes from the specified path and returns a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="file">The file to read from.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>A new instance of <typeparamref name="T"/> constructed from the serialized data.</returns>
        public static T FromJson<T>(FileInfo file, JsonSerializationParameters parameters = default)
        {
            if (!TryFromJson<T>(file, out var container, out var result, parameters))
            {
                result.Throw();
            }
            
            return container;
        }
        
        /// <summary>
        /// Deserializes from the specified path and returns a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="file">The file to read from.</param>
        /// <param name="container">When this method returns, contains the deserialized value.</param>
        /// <param name="result">The results structure containing any errors or exceptions.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>True if the deserialization succeeded; otherwise, false.</returns>
        public static bool TryFromJson<T>(FileInfo file, out T container, out DeserializationResult result, JsonSerializationParameters parameters = default)
        {
            container = default;
            return TryFromJsonOverride(file, ref container, out result, parameters);
        }

        /// <summary>
        /// Deserializes from the specified path in to an existing instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="file">The file to read from.</param>
        /// <param name="container">The reference to be overwritten.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        public static void FromJsonOverride<T>(FileInfo file, ref T container, JsonSerializationParameters parameters = default)
        {
            if (!TryFromJsonOverride(file, ref container, out var result, parameters))
            {
                result.Throw();
            }
        }
        
        /// <summary>
        /// Deserializes from the specified path in to an existing instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="file">The file to read from.</param>
        /// <param name="container">The reference to be overwritten.</param>
        /// <param name="result">The results structure containing any errors or exceptions.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <returns>True if the deserialization succeeded; otherwise, false.</returns>
        public static bool TryFromJsonOverride<T>(FileInfo file, ref T container, out DeserializationResult result, JsonSerializationParameters parameters = default)
        {
            using (var reader = new SerializedObjectReader(file.FullName, GetDefaultConfigurationForFile(file, parameters)))
            {
                return TryFromJson(reader, ref container, out result, parameters);
            }
        }

        /// <summary>
        /// Deserializes from the specified stream and returns a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <returns>A new instance of <typeparamref name="T"/> constructed from the serialized data.</returns>
        public static T FromJson<T>(Stream stream, JsonSerializationParameters parameters = default)
        {
            if (!TryFromJson<T>(stream, out var container, out var result, parameters))
            {
                result.Throw();
            }
            
            return container;
        }
        
        /// <summary>
        /// Deserializes from the specified stream and returns a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="container">When this method returns, contains the deserialized value.</param>
        /// <param name="result">The results structure containing any errors or exceptions.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>True if the deserialization succeeded; otherwise, false.</returns>
        public static bool TryFromJson<T>(Stream stream, out T container, out DeserializationResult result, JsonSerializationParameters parameters = default)
        {
            container = default;
            return TryFromJsonOverride(stream, ref container, out result);
        }
        
        /// <summary>
        /// Deserializes from the specified stream in to an existing instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="container">The reference to be overwritten.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        public static void FromJsonOverride<T>(Stream stream, ref T container, JsonSerializationParameters parameters = default)
        {
            if (!TryFromJsonOverride(stream, ref container, out var result, parameters))
            {
                result.Throw();
            }
        }

        /// <summary>
        /// Deserializes from the specified stream in to an existing instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="container">The reference to be overwritten.</param>
        /// <param name="result">The results structure containing any errors or exceptions.</param>
        /// <param name="parameters">The reader parameters to use.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>True if the deserialization succeeded; otherwise, false.</returns>
        public static bool TryFromJsonOverride<T>(Stream stream, ref T container, out DeserializationResult result, JsonSerializationParameters parameters = default)
        {
            using (var reader = new SerializedObjectReader(stream, GetDefaultConfigurationForStream(stream, parameters)))
            {
                return TryFromJson(reader, ref container, out result, parameters);
            }
        }
        
        static bool TryFromJson<T>(SerializedObjectReader reader, ref T value, out DeserializationResult result, JsonSerializationParameters parameters = default)
        {
            var serializedReferences = default(SerializedReferences);

            if (!parameters.DisableSerializedReferences)
            {
                serializedReferences = parameters.RequiresThreadSafety ? new SerializedReferences() : GetSharedSerializedReferences();
            }
            
            reader.Read(out var document);
            
            var createReader = parameters.RequiresThreadSafety || s_SharedJsonPropertyReader.IsLocked;
            
            var visitor = createReader ? new JsonPropertyReader() : GetSharedJsonPropertyReader();
            var events = createReader ? new List<DeserializationEvent>() : GetSharedDeserializationEvents();
            
            visitor.SetView(document.AsUnsafe());
            visitor.SetSerializedType(parameters.SerializedType);
            visitor.SetDisableRootAdapters(parameters.DisableRootAdapters);
            visitor.SetGlobalAdapters(GetGlobalAdapters());
            visitor.SetUserDefinedAdapters(parameters.UserDefinedAdapters);
            visitor.SetGlobalMigrations(GetGlobalMigrations());
            visitor.SetUserDefinedMigrations(parameters.UserDefinedMigrations);
            visitor.SetEvents(events);
            visitor.SetSerializedReferences(serializedReferences);
            
            var container = new PropertyWrapper<T>(value);
            try
            {
                using (visitor.Lock()) PropertyContainer.Visit(ref container, visitor);
            }
            catch (Exception e)
            {
                events.Add(new DeserializationEvent(EventType.Exception, e));
            }
            value = container.Value;
            
            result = CreateResult(events);
            return result.DidSucceed();
        }

        static DeserializationResult CreateResult(List<DeserializationEvent> events)
            => events.Count > 0 ? new DeserializationResult(events.ToList()) : default;
    }
}

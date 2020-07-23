#if !NET_DOTS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;
using Unity.Serialization.Binary;
using Unity.Serialization.Binary.Adapters;

[assembly: InternalsVisibleTo("Unity.Scenes.Hybrid")]

[assembly: GeneratePropertyBagsForTypesQualifiedWith(typeof(Unity.Entities.ISharedComponentData))]
[assembly: GeneratePropertyBagsForTypesQualifiedWith(typeof(Unity.Entities.IComponentData), TypeOptions.ReferenceType)]

namespace Unity.Entities.Serialization
{
    /// <summary>
    /// Writer to write managed objects to a <see cref="UnsafeAppendBuffer"/> stream.
    /// </summary>
    /// <remarks>
    /// This is used as a wrapper around <see cref="Unity.Serialization.Binary.BinarySerialization"/> with a custom layer for <see cref="UnityEngine.Object"/>.
    /// </remarks>
    unsafe class ManagedObjectBinaryWriter
    {
        readonly UnsafeAppendBuffer* m_Stream;
        readonly UnityEngineObjectBinaryAdapter m_UnityEngineObjectAdapter;
        readonly BinarySerializationParameters m_Params;

        /// <summary>
        /// Initializes a new instance of <see cref="ManagedObjectBinaryWriter"/> which can be used to write managed objects to the given stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public ManagedObjectBinaryWriter(UnsafeAppendBuffer* stream)
        {
            m_Stream = stream;
            m_UnityEngineObjectAdapter = new UnityEngineObjectBinaryAdapter(null);
            m_Params = new BinarySerializationParameters
            {
                UserDefinedAdapters = new List<IBinaryAdapter> {m_UnityEngineObjectAdapter},
                Context = new BinarySerializationContext()
            };
        }

        /// <summary>
        /// Gets all <see cref="UnityEngine.Object"/> types encountered during serialization.
        /// </summary>
        /// <returns>A set of all <see cref="UnityEngine.Object"/> types encountered during serialization</returns>
        public UnityEngine.Object[] GetObjectTable() => m_UnityEngineObjectAdapter.GetSerializeObjectTable();

        /// <summary>
        /// Writes the given boxed object to the binary stream.
        /// </summary>
        /// <remarks>
        /// Any <see cref="UnityEngine.Object"/> references are added to the object table and can be retrieved by calling <see cref="GetObjectTable"/>.
        /// </remarks>
        /// <param name="obj">The object to serialize.</param>
        public void WriteObject(object obj)
        {
            var parameters = m_Params;
            parameters.SerializedType = obj?.GetType();
            BinarySerialization.ToBinary(m_Stream, obj, parameters);
        }
    }

    /// <summary>
    /// Reader to read managed objects from a <see cref="UnsafeAppendBuffer.Reader"/> stream.
    /// </summary>
    /// <remarks>
    /// This is used as a wrapper around <see cref="Unity.Serialization.Binary.BinarySerialization"/> with a custom layer for <see cref="UnityEngine.Object"/>.
    /// </remarks>
    unsafe class ManagedObjectBinaryReader
    {
        readonly UnsafeAppendBuffer.Reader* m_Stream;
        readonly BinarySerializationParameters m_Params;

        /// <summary>
        /// Initializes a new instance of <see cref="ManagedObjectBinaryReader"/> which can be used to read managed objects from the given stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="unityObjectTable">The table containing all <see cref="UnityEngine.Object"/> references. This is produce by the <see cref="ManagedObjectBinaryWriter"/>.</param>
        public ManagedObjectBinaryReader(UnsafeAppendBuffer.Reader* stream, UnityEngine.Object[] unityObjectTable)
        {
            m_Stream = stream;
            m_Params = new BinarySerializationParameters
            {
                UserDefinedAdapters = new List<IBinaryAdapter> {new UnityEngineObjectBinaryAdapter(unityObjectTable)},
                Context = new BinarySerializationContext()
            };
        }

        /// <summary>
        /// Reads from the binary stream and returns the next object.
        /// </summary>
        /// <remarks>
        /// The type is given as a hint to the serializer to avoid writing root type information.
        /// </remarks>
        /// <param name="type">The root type.</param>
        /// <returns>The deserialized object value.</returns>
        public object ReadObject(Type type)
        {
            var parameters = m_Params;
            parameters.SerializedType = type;
            return BinarySerialization.FromBinary<object>(m_Stream, parameters);
        }
    }

    /// <summary>
    /// By default, we don't have a way to write a <see cref="UnityEngine.Object"/> reference to the binary stream for runtime.
    ///
    /// This adapter is used to intercept all Serialize and Deserialize calls for any type inheriting from <see cref="UnityEngine.Object"/>.
    /// The intercepted objects are added to an internal map and the index is written to the stream. It is the responsibility of the caller
    /// to take the object table and persist it along side the binary data to be reconstructed on deserialization.
    /// </summary>
    unsafe class UnityEngineObjectBinaryAdapter : Unity.Serialization.Binary.Adapters.Contravariant.IBinaryAdapter<UnityEngine.Object>
    {
        readonly List<UnityEngine.Object> m_SerializeObjectTable = new List<UnityEngine.Object>();
        readonly Dictionary<UnityEngine.Object, int> m_SerializeObjectTableMap = new Dictionary<UnityEngine.Object, int>();
        readonly UnityEngine.Object[] m_DeserializeObjectTable;

        /// <summary>
        /// Gets the set of all <see cref="UnityEngine.Object"/> references encountered during serialization.
        /// </summary>
        /// <returns>An array of all <see cref="UnityEngine.Object"/> references encountered during serialization.</returns>
        public UnityEngine.Object[] GetSerializeObjectTable() => m_SerializeObjectTable.ToArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityEngineObjectBinaryAdapter"/>.
        /// This is used to have custom serialize/deserialize handling for <see cref="UnityEngine.Object"/> types.
        /// </summary>
        /// <param name="deserializeObjectTable"></param>
        public UnityEngineObjectBinaryAdapter(UnityEngine.Object[] deserializeObjectTable)
        {
            m_DeserializeObjectTable = deserializeObjectTable;
        }

        /// <summary>
        /// Invoked during serialization any time we encounter a <see cref="UnityEngine.Object"/> type or any derived type.
        ///
        /// We add the object to the internal table and write the index to the stream.
        /// </summary>
        /// <remarks>
        /// Objects can be retrieved by calling <see cref="GetSerializeObjectTable"/> after writing all data.
        /// </remarks>
        /// <param name="writer">The stream to write to.</param>
        /// <param name="value">The value to write.</param>
        public void Serialize(UnsafeAppendBuffer* writer, UnityEngine.Object value)
        {
            var index = -1;

            if (value != null)
            {
                if (!m_SerializeObjectTableMap.TryGetValue(value, out index))
                {
                    index = m_SerializeObjectTable.Count;
                    m_SerializeObjectTableMap.Add(value, index);
                    m_SerializeObjectTable.Add(value);
                }
            }

            writer->Add(index);
        }

        /// <summary>
        /// Invoked during deserialization any time we encounter a <see cref="UnityEngine.Object"/> type or any derived type.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The adapter was not initialized with a valid object table.</exception>
        public object Deserialize(UnsafeAppendBuffer.Reader* reader)
        {
            if (m_DeserializeObjectTable == null)
                throw new ArgumentException("We are reading a UnityEngine.Object however no ObjectTable was provided to the ManagedObjectBinaryReader.");

            var index = reader->ReadNext<int>();

            if (index == -1)
                return null;

            if ((uint)index >= m_DeserializeObjectTable.Length)
                throw new ArgumentException("We are reading a UnityEngine.Object but the deserialized index is out of range for the given object table.");

            return m_DeserializeObjectTable[index];
        }
    }
}
#endif

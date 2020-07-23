using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Binary.Adapters;

namespace Unity.Serialization.Binary
{
    /// <summary>
    /// This class is used to store state between multiple serialization calls.
    /// By passing this to <see cref="BinarySerializationParameters"/> will allow visitors and serialized references to be re-used.
    /// </summary>
    class BinarySerializationContext
    {
        BinaryPropertyWriter m_BinaryPropertyWriter;
        BinaryPropertyReader m_BinaryPropertyReader;
        SerializedReferences m_SerializedReferences;

        /// <summary>
        /// Gets the shared <see cref="BinaryPropertyWriter"/>.
        /// </summary>
        /// <returns>The <see cref="BinaryPropertyWriter"/>.</returns>
        internal BinaryPropertyWriter GetBinaryPropertyWriter()
            => m_BinaryPropertyWriter ?? (m_BinaryPropertyWriter = new BinaryPropertyWriter());
        
        /// <summary>
        /// Gets the shared <see cref="BinaryPropertyReader"/>.
        /// </summary>
        /// <returns>The <see cref="BinaryPropertyReader"/>.</returns>
        internal BinaryPropertyReader GetBinaryPropertyReader()
            => m_BinaryPropertyReader ?? (m_BinaryPropertyReader = new BinaryPropertyReader());
        
        /// <summary>
        /// Gets the shared <see cref="SerializedReferences"/>.
        /// </summary>
        /// <returns>The <see cref="SerializedReferences"/>.</returns>
        internal SerializedReferences GetSerializedReferences()
            => m_SerializedReferences ?? (m_SerializedReferences = new SerializedReferences());

        /// <summary>
        /// Clears the serialized references state.
        /// </summary>
        internal void ClearSerializedReferences()
        {
            m_SerializedReferences?.Clear();
        }
    }
    
    /// <summary>
    /// Custom parameters to use for binary serialization or deserialization.
    /// </summary>
    public struct BinarySerializationParameters
    {
        /// <summary>
        /// By default, a polymorphic root type will have it's assembly qualified type name written to the stream. Use this
        /// parameter to provide a known root type at both serialize and deserialize time to avoid writing this information.
        /// </summary>
        public Type SerializedType { get; set; }
        
        /// <summary>
        /// By default, adapters are evaluated for root objects. Use this to change the default behaviour.
        /// </summary>
        public bool DisableRootAdapters { get; set; }
        
        /// <summary>
        /// Provide a custom set of adapters for the serialization and deserialization.
        /// </summary>
        /// <remarks>
        /// These adapters will be evaluated first before any global or built in adapters.
        /// </remarks>
        public List<IBinaryAdapter> UserDefinedAdapters { get; set; }
        
        /// <summary>
        /// This parameter indicates if the serializer should be thread safe. The default value is false.
        /// </summary>
        /// <remarks>
        /// Setting this to true will cause managed allocations for the internal visitor.
        /// </remarks>
        public bool RequiresThreadSafety { get; set; }
        
        /// <summary>
        /// By default, references between objects are serialized. Use this to always write a copy of the object to the output.
        /// </summary>
        public bool DisableSerializedReferences { get; set; }
        
        /// <summary>
        /// Sets the context object for serialization. This can be used to shared resources across multiple calls to serialize and deserialize.
        /// </summary>
        internal BinarySerializationContext Context { get; set; }
    }

    /// <summary>
    /// High level API for serializing or deserializing json data from string, file or stream.
    /// </summary>
    public static partial class BinarySerialization
    {
        static readonly List<IBinaryAdapter> s_Adapters = new List<IBinaryAdapter>();
        static readonly BinarySerializationContext m_SharedContext = new BinarySerializationContext();

        static BinarySerializationContext GetSharedContext()
        {
            m_SharedContext.ClearSerializedReferences();
            return m_SharedContext;
        }
        
        /// <summary>
        /// Adds the specified <see cref="IBinaryAdapter"/> to the set of global adapters. This is be included by default in all BinarySerialization calls.
        /// </summary>
        /// <param name="adapter">The adapter to add.</param>
        /// <exception cref="ArgumentException">The given adapter is already registered.</exception>
        public static void AddGlobalAdapter(IBinaryAdapter adapter)
        {
            if (s_Adapters.Contains(adapter))
                throw new ArgumentException("IBinaryAdapter has already been registered.");
            
            s_Adapters.Add(adapter);
        }

        static List<IBinaryAdapter> GetGlobalAdapters() => s_Adapters;

        internal static unsafe void WritePrimitiveUnsafe<TValue>(UnsafeAppendBuffer* stream, ref TValue value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, sbyte>(ref value));
                    return;
                case TypeCode.Int16:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, short>(ref value));
                    return;
                case TypeCode.Int32:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, int>(ref value));
                    return;
                case TypeCode.Int64:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, long>(ref value));
                    return;
                case TypeCode.Byte:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, byte>(ref value));
                    return;
                case TypeCode.UInt16:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, ushort>(ref value));
                    return;
                case TypeCode.UInt32:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, uint>(ref value));
                    return;
                case TypeCode.UInt64:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, ulong>(ref value));
                    return;
                case TypeCode.Single:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, float>(ref value));
                    return;
                case TypeCode.Double:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, double>(ref value));
                    return;
                case TypeCode.Boolean:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, bool>(ref value) ? (byte) 1 : (byte) 0);
                    return;
                case TypeCode.Char:
                    stream->Add(System.Runtime.CompilerServices.Unsafe.As<TValue, char>(ref value));
                    return;
                case TypeCode.String:
                    stream->Add(value as string);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        } 
        
        internal static unsafe void WritePrimitiveBoxed(UnsafeAppendBuffer* stream, object value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                    stream->Add((sbyte) value);
                    return;
                case TypeCode.Int16:
                    stream->Add((short) value);
                    return;
                case TypeCode.Int32:
                    stream->Add((int) value);
                    return;
                case TypeCode.Int64:
                    stream->Add((long) value);
                    return;
                case TypeCode.Byte:
                    stream->Add((byte) value);
                    return;
                case TypeCode.UInt16:
                    stream->Add((ushort) value);
                    return;
                case TypeCode.UInt32:
                    stream->Add((uint) value);
                    return;
                case TypeCode.UInt64:
                    stream->Add((ulong) value);
                    return;
                case TypeCode.Single:
                    stream->Add((float) value);
                    return;
                case TypeCode.Double:
                    stream->Add((double) value);
                    return;
                case TypeCode.Boolean:
                    stream->Add((bool) value ? (byte) 1 : (byte) 0);
                    return;
                case TypeCode.Char:
                    stream->Add((char) value);
                    return;
                case TypeCode.String:
                    stream->Add(value as string);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        internal static unsafe void ReadPrimitiveUnsafe<TValue>(UnsafeAppendBuffer.Reader* stream, ref TValue value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                    stream->ReadNext<sbyte>(out var _sbyte);
                    value = System.Runtime.CompilerServices.Unsafe.As<sbyte, TValue>(ref _sbyte);
                    return;
                case TypeCode.Int16:
                    stream->ReadNext<short>(out var _short);
                    value = System.Runtime.CompilerServices.Unsafe.As<short, TValue>(ref _short);
                    return;
                case TypeCode.Int32:
                    stream->ReadNext<int>(out var _int);
                    value = System.Runtime.CompilerServices.Unsafe.As<int, TValue>(ref _int);
                    return;
                case TypeCode.Int64:
                    stream->ReadNext<long>(out var _long);
                    value = System.Runtime.CompilerServices.Unsafe.As<long, TValue>(ref _long);
                    return;
                case TypeCode.Byte:
                    stream->ReadNext<byte>(out var _byte);
                    value = System.Runtime.CompilerServices.Unsafe.As<byte, TValue>(ref _byte);
                    return;
                case TypeCode.UInt16:
                    stream->ReadNext<ushort>(out var _ushort);
                    value = System.Runtime.CompilerServices.Unsafe.As<ushort, TValue>(ref _ushort);
                    return;
                case TypeCode.UInt32:
                    stream->ReadNext<uint>(out var _uint);
                    value = System.Runtime.CompilerServices.Unsafe.As<uint, TValue>(ref _uint);
                    return;
                case TypeCode.UInt64:
                    stream->ReadNext<ulong>(out var _ulong);
                    value = System.Runtime.CompilerServices.Unsafe.As<ulong, TValue>(ref _ulong);
                    return;
                case TypeCode.Single:
                    stream->ReadNext<float>(out var _float);
                    value = System.Runtime.CompilerServices.Unsafe.As<float, TValue>(ref _float);
                    return;
                case TypeCode.Double:
                    stream->ReadNext<double>(out var _double);
                    value = System.Runtime.CompilerServices.Unsafe.As<double, TValue>(ref _double);
                    return;
                case TypeCode.Boolean:
                    stream->ReadNext<byte>(out var _boolean);
                    var b = _boolean == 1;
                    value = System.Runtime.CompilerServices.Unsafe.As<bool, TValue>(ref b);
                    return;
                case TypeCode.Char:
                    stream->ReadNext<char>(out var _char);
                    value = System.Runtime.CompilerServices.Unsafe.As<char, TValue>(ref _char);
                    return;
                case TypeCode.String:
                    stream->ReadNext(out string _string);
                    value = (TValue) (object) _string;
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static unsafe void ReadPrimitiveBoxed<TValue>(UnsafeAppendBuffer.Reader* stream, ref TValue value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                    value = (TValue) (object) stream->ReadNext<sbyte>();
                    return;
                case TypeCode.Int16:
                    value = (TValue) (object) stream->ReadNext<short>();
                    return;
                case TypeCode.Int32:
                    value = (TValue) (object) stream->ReadNext<int>();
                    return;
                case TypeCode.Int64:
                    value = (TValue) (object) stream->ReadNext<long>();
                    return;
                case TypeCode.Byte:
                    value = (TValue) (object) stream->ReadNext<byte>();
                    return;
                case TypeCode.UInt16:
                    value = (TValue) (object) stream->ReadNext<ushort>();
                    return;
                case TypeCode.UInt32:
                    value = (TValue) (object) stream->ReadNext<uint>();
                    return;
                case TypeCode.UInt64:
                    value = (TValue) (object) stream->ReadNext<ulong>();
                    return;
                case TypeCode.Single:
                    value = (TValue) (object) stream->ReadNext<float>();
                    return;
                case TypeCode.Double:
                    value = (TValue) (object) stream->ReadNext<double>();
                    return;
                case TypeCode.Boolean:
                    value = (TValue) (object) (stream->ReadNext<byte>() == 1);
                    return;
                case TypeCode.Char:
                    value = (TValue) (object) stream->ReadNext<char>();
                    return;
                case TypeCode.String:
                    stream->ReadNext(out string _string);
                    value = (TValue) (object) _string;
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
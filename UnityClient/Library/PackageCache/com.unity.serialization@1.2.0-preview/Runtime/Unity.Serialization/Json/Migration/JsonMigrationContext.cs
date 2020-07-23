using System;
using Unity.Properties;
using Unity.Properties.Internal;

namespace Unity.Serialization.Json.Adapters
{
    /// <summary>
    /// A migration context used to deserialize and migrate types.
    /// </summary>
    public readonly struct JsonMigrationContext
    {
        /// <summary>
        /// The deserialized version of the type.
        /// </summary>
        public readonly int SerializedVersion;
        
        /// <summary>
        /// The in-memory representation of the value being deserialized.
        /// </summary>
        public readonly SerializedObjectView SerializedObject;
        
        /// <summary>
        /// The serialized type as reported by the underlying stream. This can be used in contravariant migrations. 
        /// </summary>
        public readonly Type SerializedType;
        
        /// <summary>
        /// The internal visitor, used to re-enter in to normal deserialization.
        /// </summary>
        readonly JsonPropertyReader m_Visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMigrationContext"/> structure. This is an internal method.
        /// </summary>
        /// <param name="serializedVersion">The serialized version read from the stream.</param>
        /// <param name="serializedObject">The view over the serialized data.</param>
        /// <param name="serializedType">The serialized type from the stream.</param>
        /// <param name="visitor">The current deserialization visitor, used for re-entry into normal deserialization.</param>
        internal JsonMigrationContext(int serializedVersion, SerializedObjectView serializedObject, Type serializedType, JsonPropertyReader visitor)
        {
            SerializedVersion = serializedVersion;
            SerializedObject = serializedObject;
            SerializedType = serializedType;
            m_Visitor = visitor;
        }

        /// <summary>
        /// Reads the root object as the specified <typeparamref name="TValue"/> type.
        /// </summary>
        /// <param name="value">When this method returns, contains the deserialized value, if successful. otherwise the default value for the <typeparamref name="TValue"/>.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value was read successfully; otherwise, <see langword="false"/></returns>
        public bool TryRead<TValue>(out TValue value)
            => TryRead<TValue>(SerializedObject, out value);

        /// <summary>
        /// Reads the root object as the specified <typeparamref name="TValue"/> type.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>A new instance of <typeparamref name="TValue"/> initialized with data from the root object.</returns>
        public TValue Read<TValue>()
            => Read<TValue>(SerializedObject);
        
        /// <summary>
        /// Reads a top level member as the specified <typeparamref name="TValue"/> type.
        /// </summary>
        /// <param name="name">The top level member name.</param>
        /// <param name="value">When this method returns, contains the deserialized value, if successful. otherwise the default value for the <typeparamref name="TValue"/>.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value was read successfully; otherwise, <see langword="false"/></returns>
        public bool TryRead<TValue>(string name, out TValue value)
        {
            try
            {
                value = Read<TValue>(name);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }
        
        /// <summary>
        /// Reads a top level member as the specified <typeparamref name="TValue"/> type.
        /// </summary>
        /// <param name="name">The top level member name.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>A new instance of <typeparamref name="TValue"/> initialized with data from the view.</returns>
        public TValue Read<TValue>(string name)
            => Read<TValue>(SerializedObject[name]);

        /// <summary>
        /// Reads the specified <see cref="SerializedValueView"/> as the specified <typeparamref name="TValue"/> type.
        /// </summary>
        /// <param name="view">The view to read.</param>
        /// <param name="value">When this method returns, contains the deserialized value, if successful. otherwise the default value for the <typeparamref name="TValue"/>.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value was read successfully; otherwise, <see langword="false"/></returns>
        public bool TryRead<TValue>(SerializedObjectView view, out TValue value)
        {
            try
            {
                value = Read<TValue>(view);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }
        /// <summary>
        /// Reads the specified <see cref="SerializedValueView"/> as the specified <typeparamref name="TValue"/> type.
        /// </summary>
        /// <param name="view">The serialized object.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>A new instance of <typeparamref name="TValue"/> initialized with data from the view.</returns>
        public TValue Read<TValue>(SerializedValueView view)
        {
            var value = default(TValue);
            Read(ref value, view);
            return value;
        }
        
        /// <summary>
        /// Reads the specified <see cref="SerializedValueView"/> in to the given reference.
        /// </summary>
        /// <param name="value">The existing reference to read in to.</param>
        /// <param name="view">The serialized value.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        public void Read<TValue>(ref TValue value, SerializedValueView view)
        {
            switch (view.Type)
            {
                case TokenType.String:
                {
                    TypeConversion.TryConvert(view.AsStringView().ToString(), out value);
                    break;
                }
                case TokenType.Primitive:
                {
                    var p = view.AsPrimitiveView();

                    if (p.IsIntegral())
                    {
                        if (p.IsSigned())
                        {
                            TypeConversion.TryConvert(p.AsInt64(), out value);
                        }
                        else
                        {
                            TypeConversion.TryConvert(p.AsUInt64(), out value);
                        }
                    }
                    else if (p.IsDecimal() || p.IsInfinity() || p.IsNaN())
                    {
                        TypeConversion.TryConvert(p.AsFloat(), out value);
                    }
                    else if (p.IsBoolean())
                    {
                        TypeConversion.TryConvert(p.AsBoolean(), out value);
                    }
                    else if (p.IsNull())
                    {
                        value = default;
                    }

                    break;
                }
                case TokenType.Object:
                case TokenType.Array:
                {
                    var serializedType = !RuntimeTypeInfoCache<TValue>.IsAbstractOrInterface
                        ? typeof(TValue) 
                        : null;
                    
                    using (m_Visitor.CreateSerializedTypeScope(serializedType))
                    using (m_Visitor.CreateViewScope(view.AsUnsafe()))
                    using (m_Visitor.CreateDisableRootMigrationScope(true))
                    {
                        var container = new PropertyWrapper<TValue>(value);
                        PropertyContainer.Visit(ref container, m_Visitor, out _);
                        value = container.Value;
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
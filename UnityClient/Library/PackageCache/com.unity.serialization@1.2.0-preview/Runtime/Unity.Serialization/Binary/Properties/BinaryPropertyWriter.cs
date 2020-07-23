using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;
using Unity.Properties.Internal;
using Unity.Serialization.Binary.Adapters;

namespace Unity.Serialization.Binary
{
    unsafe class BinaryPropertyWriter : BinaryPropertyVisitor,
        IPropertyBagVisitor,
        IListPropertyBagVisitor,
        ISetPropertyBagVisitor,
        IDictionaryPropertyBagVisitor,
        IPropertyVisitor
    {
        UnsafeAppendBuffer* m_Stream;
        Type m_SerializedType;
        bool m_DisableRootAdapters;
        BinaryAdapterCollection m_Adapters;
        SerializedReferences m_SerializedReferences;

        public void SetStream(UnsafeAppendBuffer* stream)
            => m_Stream = stream;

        public void SetSerializedType(Type type)
            => m_SerializedType = type;

        public void SetDisableRootAdapters(bool disableRootAdapters)
            => m_DisableRootAdapters = disableRootAdapters;

        public void SetGlobalAdapters(List<IBinaryAdapter> adapters)
            => m_Adapters.GlobalAdapters = adapters;

        public void SetUserDefinedAdapters(List<IBinaryAdapter> adapters)
            => m_Adapters.UserDefinedAdapters = adapters;
        
        public void SetSerializedReferences(SerializedReferences serializedReferences)
            => m_SerializedReferences = serializedReferences;

        public BinaryPropertyWriter()
        {
            m_Adapters.InternalAdapter = this;
        }

        void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
        {
            if (properties is IPropertyList<TContainer> collection)
            {
                foreach (var property in collection.GetProperties(ref container))
                {
                    if (property.HasAttribute<NonSerializedAttribute>() || property.HasAttribute<DontSerializeAttribute>())
                        continue;
                    
                    ((IPropertyAccept<TContainer>) property).Accept(this, ref container);
                }
            }
            else
            {
                foreach (var property in properties.GetProperties(ref container))
                {
                    if (property.HasAttribute<NonSerializedAttribute>() || property.HasAttribute<DontSerializeAttribute>())
                        continue;

                    ((IPropertyAccept<TContainer>) property).Accept(this, ref container);
                }
            }
        }

        void IListPropertyBagVisitor.Visit<TList, TElement>(IListPropertyBag<TList, TElement> properties, ref TList container)
        {
            m_Stream->Add(container.Count);

            for (var i = 0; i < container.Count; i++)
            {
                WriteValue(container[i]);
            }
        }

        void ISetPropertyBagVisitor.Visit<TSet, TValue>(ISetPropertyBag<TSet, TValue> properties, ref TSet container)
        {
            m_Stream->Add(container.Count);

            foreach (var value in container)
            {
                WriteValue(value);
            }
        }

        void IDictionaryPropertyBagVisitor.Visit<TDictionary, TKey, TValue>(IDictionaryPropertyBag<TDictionary, TKey, TValue> properties, ref TDictionary container)
        {
            m_Stream->Add(container.Count);

            foreach (var kvp in container)
            {
                WriteValue(kvp.Key);
                WriteValue(kvp.Value);
            }
        }

        void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
        {
            var isRootProperty = property is IPropertyWrapper;
            WriteValue(property.GetValue(ref container), isRootProperty);
        }

        void WriteValue<TValue>(TValue value, bool isRoot = false)
        {
            var runAdapters = !(isRoot && m_DisableRootAdapters);

            if (runAdapters && m_Adapters.TrySerialize(m_Stream, ref value))
            {
                return;
            }
            
            if (RuntimeTypeInfoCache<TValue>.IsEnum)
            {
                BinarySerialization.WritePrimitiveUnsafe(m_Stream, ref value, Enum.GetUnderlyingType(typeof(TValue)));
                return;
            }

            if (RuntimeTypeInfoCache<TValue>.CanBeNull && null == value)
            {
                m_Stream->Add(k_TokenNull);
                return;
            }

            if (RuntimeTypeInfoCache<TValue>.IsNullable)
            {
                m_Stream->Add(k_TokenNone);
                BinarySerialization.WritePrimitiveBoxed(m_Stream, value, Nullable.GetUnderlyingType(typeof(TValue)));
                return;
            }
            
            if (!RuntimeTypeInfoCache<TValue>.IsValueType)
            {
#if !UNITY_DOTSPLAYER
                if (runAdapters && value is UnityEngine.Object unityEngineObject)
                {
                    // Special path for polymorphic unity object references.
                    m_Stream->Add(k_TokenUnityEngineObjectReference);
                    m_Adapters.TrySerialize(m_Stream, ref unityEngineObject);
                    return;
                }
#endif
                
                if (null != m_SerializedReferences && !value.GetType().IsValueType)
                {
                    // At this point we don't know if an object will reference this value.
                    // To avoid a second visitation pass we always create an entry for each managed object reference.
                    var id = m_SerializedReferences.AddSerializedReference(value);

                    if (!m_SerializedReferences.SetSerialized(value))
                    {
                        // This is the second time encountering this object during serialization.
                        // We instead of writing out the object we simply write the id.
                        m_Stream->Add(k_TokenSerializedReference);
                        m_Stream->Add(id);
                        return;
                    }
                }
                
                // This is a very common case. At serialize time we are serializing something that is polymorphic or an object
                // However at deserialize time the user known the System.Type, we can avoid writing out the fully qualified type name in this case.
                var isRootAndTypeWasGiven = isRoot && null != m_SerializedType;

                if (typeof(TValue) != value.GetType() && !isRootAndTypeWasGiven)
                {
                    m_Stream->Add(k_TokenPolymorphic);
                    m_Stream->Add(value.GetType().AssemblyQualifiedName);
                }
                else
                {
                    m_Stream->Add(k_TokenNone);
                }
            }
            
            if (RuntimeTypeInfoCache<TValue>.IsObjectType && !RuntimeTypeInfoCache.IsContainerType(value.GetType()))
            {
                BinarySerialization.WritePrimitiveBoxed(m_Stream, value, value.GetType());
                return;
            }
            
            if (!PropertyContainer.Visit(ref value, this, out var errorCode))
            {
                switch (errorCode)
                {
                    case VisitErrorCode.NullContainer:
                        throw new ArgumentNullException(nameof(value));
                    case VisitErrorCode.InvalidContainerType:
                        throw new InvalidContainerTypeException(value.GetType());
                    case VisitErrorCode.MissingPropertyBag:
                        throw new MissingPropertyBagException(value.GetType());
                    default:
                        throw new Exception($"Unexpected {nameof(VisitErrorCode)}=[{errorCode}]");
                }
            }
        }
    }
}
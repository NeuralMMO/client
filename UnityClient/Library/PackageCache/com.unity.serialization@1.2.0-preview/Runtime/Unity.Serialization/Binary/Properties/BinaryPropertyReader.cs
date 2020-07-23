using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;
using Unity.Properties.Internal;
using Unity.Serialization.Binary.Adapters;

namespace Unity.Serialization.Binary
{
    unsafe class BinaryPropertyReader : BinaryPropertyVisitor,
        ISerializedTypeProvider,
        IPropertyBagVisitor,
        IListPropertyBagVisitor,
        ISetPropertyBagVisitor,
        IDictionaryPropertyBagVisitor,
        IPropertyVisitor
    {
        UnsafeAppendBuffer.Reader* m_Stream;
        Type m_SerializedType;
        bool m_DisableRootAdapters;
        BinaryAdapterCollection m_Adapters;
        SerializedReferences m_SerializedReferences;

        public void SetStream(UnsafeAppendBuffer.Reader* stream)
        {
            m_Stream = stream;
        }
        
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

        public BinaryPropertyReader()
        {
            m_Adapters.InternalAdapter = this;
        }

        void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
        {
            m_SerializedReferences?.AddDeserializedReference(container);

            if (properties is IPropertyList<TContainer> collection)
            {
                // no boxing
                foreach (var property in collection.GetProperties(ref container))
                {
                    if (property.HasAttribute<NonSerializedAttribute>() || property.HasAttribute<DontSerializeAttribute>())
                        continue;

                    ((IPropertyAccept<TContainer>) property).Accept(this, ref container);
                }
            }
            else
            {
                // boxing
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
            m_SerializedReferences?.AddDeserializedReference(container);

            var count = m_Stream->ReadNext<int>();

            if (typeof(TList).IsArray)
            {
                for (var i = 0; i < count; i++)
                {
                    container[i] = ReadValue<TElement>();
                }
            }
            else
            {
                container.Clear();
                for (var i = 0; i < count; i++)
                {
                    container.Add(ReadValue<TElement>());
                }
            }
        }

        void ISetPropertyBagVisitor.Visit<TSet, TValue>(ISetPropertyBag<TSet, TValue> properties, ref TSet container)
        {
            m_SerializedReferences?.AddDeserializedReference(container);

            container.Clear();
            var count = m_Stream->ReadNext<int>();
            
            for (var i = 0; i < count; i++)
            {
                container.Add(ReadValue<TValue>());
            }
        }

        void IDictionaryPropertyBagVisitor.Visit<TDictionary, TKey, TValue>(IDictionaryPropertyBag<TDictionary, TKey, TValue> properties, ref TDictionary container)
        {
            m_SerializedReferences?.AddDeserializedReference(container);

            container.Clear();
            var count = m_Stream->ReadNext<int>();
            
            for (var i = 0; i < count; i++)
            {
                container.Add(ReadValue<TKey>(), ReadValue<TValue>());
            }
        }

        void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
        {
            var value = property.GetValue(ref container);
            var isRoot = property is IPropertyWrapper;
            
            ReadValue(ref value, isRoot);

            if (!property.IsReadOnly)
            {
                property.SetValue(ref container, value);
            }
            else if (PropertyChecks.CheckReadOnlyPropertyForDeserialization(property, ref container, ref value, out var error))
            {
                throw new SerializationException(error);
            }
        }

        TValue ReadValue<TValue>()
        {
            var value = default(TValue);
            ReadValue(ref value);
            return value;
        }

        void ReadValue<TValue>(ref TValue value, bool isRoot = false)
        {
            var runAdapters = !(isRoot && m_DisableRootAdapters);
            
            if (runAdapters && m_Adapters.TryDeserialize(m_Stream, ref value))
            {
                return;
            }
            
            if (RuntimeTypeInfoCache<TValue>.IsEnum)
            {
                BinarySerialization.ReadPrimitiveUnsafe(m_Stream, ref value, Enum.GetUnderlyingType(typeof(TValue)));
                return;
            }
            
            var token = default(byte);
            
            if (RuntimeTypeInfoCache<TValue>.CanBeNull)
            {
                token = m_Stream->ReadNext<byte>();
                
                switch (token)
                {
                    case k_TokenNull:
                        value = default;
                        return;
                    case k_TokenSerializedReference:
                        var id = m_Stream->ReadNext<int>();
                        if (null == m_SerializedReferences)
                            throw new Exception("Deserialization encountered a serialized object reference while running with DisableSerializedReferences.");
                        value = (TValue) m_SerializedReferences.GetDeserializedReference(id);
                        return;
                }
            }
            
            if (RuntimeTypeInfoCache<TValue>.IsNullable)
            {
                BinarySerialization.ReadPrimitiveBoxed(m_Stream, ref value, Nullable.GetUnderlyingType(typeof(TValue)));
                return;
            }

#if !UNITY_DOTSPLAYER
            if (runAdapters && token == k_TokenUnityEngineObjectReference)
            {
                var unityEngineObject = default(UnityEngine.Object);
                m_Adapters.TryDeserialize(m_Stream, ref unityEngineObject);
                value = (TValue) (object) unityEngineObject;
                return;
            }
#endif

            if (token == k_TokenPolymorphic)
            {
                m_Stream->ReadNext(out var assemblyQualifiedTypeName);

                if (string.IsNullOrEmpty(assemblyQualifiedTypeName))
                {
                    throw new ArgumentException();
                }

                var concreteType = Type.GetType(assemblyQualifiedTypeName);

                if (null == concreteType)
                {
                    if (FormerNameAttribute.TryGetCurrentTypeName(assemblyQualifiedTypeName, out var currentAssemblyQualifiedTypeName))
                    {
                        concreteType = Type.GetType(currentAssemblyQualifiedTypeName);
                    }

                    if (null == concreteType)
                    {
                        throw new ArgumentException();
                    }
                }

                m_SerializedTypeProviderSerializedType = concreteType;
            }
            else
            {
                // If we have a user provided root type pass it to the type construction.
                m_SerializedTypeProviderSerializedType = isRoot ? m_SerializedType : null;
            }
            
            DefaultTypeConstruction.Construct(ref value, this);

            if (RuntimeTypeInfoCache<TValue>.IsObjectType && !RuntimeTypeInfoCache.IsContainerType(value.GetType()))
            {
                BinarySerialization.ReadPrimitiveBoxed(m_Stream, ref value, value.GetType());
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
        
        Type m_SerializedTypeProviderSerializedType;

        Type ISerializedTypeProvider.GetSerializedType()
        {
            return m_SerializedTypeProviderSerializedType;
        }

        int ISerializedTypeProvider.GetArrayLength()
        {
            var pos = m_Stream->Offset;
            var count = m_Stream->ReadNext<int>();
            m_Stream->Offset = pos;
            return count;
        }

        object ISerializedTypeProvider.GetDefaultObject()
        {
            throw new InvalidOperationException();
        }
    }
}
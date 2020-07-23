using System;
using Unity.Properties.Internal;

namespace Unity.Serialization
{
    /// <summary>
    /// Interface to allow the serialization backend to provide type information to the type construction system.
    /// </summary>
    interface ISerializedTypeProvider
    {
        /// <summary>
        /// Returns the <see cref="System.Type"/> resolved by the serialization backend.
        /// </summary>
        /// <returns>The <see cref="System.Type"/> which was serialized.</returns>
        Type GetSerializedType();
        
        /// <summary>
        /// Returns the array length. 
        /// </summary>
        /// <returns></returns>
        int GetArrayLength();
        
        object GetDefaultObject();
    }
    
    static class DefaultTypeConstruction
    {
        internal static void Construct<TValue>(ref TValue value, ISerializedTypeProvider provider)
        {
            if (RuntimeTypeInfoCache<TValue>.IsValueType)
            {
                return;
            }
            
            var serializedType = provider.GetSerializedType();

            if (null != serializedType) 
            {
                if (!typeof(TValue).IsAssignableFrom(serializedType))
                {
                    throw new ArgumentException($"Type mismatch. DeclaredType=[{typeof(TValue)}] SerializedType=[{serializedType}]");
                }

                ConstructFromSerializedType(ref value, serializedType, provider);
                return;
            }
            
            if (RuntimeTypeInfoCache<TValue>.IsObjectType && null == value)
            {
                value = (TValue) provider.GetDefaultObject();
                return;
            }
            
            if (RuntimeTypeInfoCache<TValue>.IsAbstractOrInterface)
            {
                throw new ArgumentException();
            }

            ConstructFromDeclaredType(ref value, provider);
        }

        static void ConstructFromSerializedType<TValue>(ref TValue value, Type type, ISerializedTypeProvider provider)
        {
#if !UNITY_DOTSPLAYER
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return;
            }
#endif
            
            if (type.IsArray)
            {
                var count = provider.GetArrayLength();
                
                if (null == value || (value as Array)?.Length != count)
                {
                    var elementType = type.GetElementType();

                    if (elementType == null)
                    {
                        throw new ArgumentException("Failed to construct type. Missing array element type.");
                    }

                    value = (TValue) (object) Array.CreateInstance(elementType, count);
                }

                return;
            }
            
            if (null != value && value.GetType() == type)
            {
                return;
            }
                
            value = (TValue) Activator.CreateInstance(type);
        }

        static void ConstructFromDeclaredType<TValue>(ref TValue value, ISerializedTypeProvider provider)
        {
#if !UNITY_DOTSPLAYER
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(TValue)))
            {
                return;
            }
#endif
            
            if (typeof(TValue).IsArray)
            {
                var count = provider.GetArrayLength();
                
                if (null == value || (value as Array)?.Length != count)
                {
                    var elementType = typeof(TValue).GetElementType();

                    if (elementType == null)
                    {
                        throw new ArgumentException("Failed to construct type. Missing array element type.");
                    }

                    value = (TValue) (object) Array.CreateInstance(elementType, count);
                }

                return;
            }

            if (null == value)
            {
                value = Activator.CreateInstance<TValue>();
            }
        }
    }
}
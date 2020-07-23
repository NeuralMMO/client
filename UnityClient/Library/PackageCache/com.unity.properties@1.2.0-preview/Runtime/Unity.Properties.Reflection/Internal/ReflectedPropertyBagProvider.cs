#if !NET_DOTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Properties.Internal;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Properties.Reflection.Internal
{
#if !ENABLE_IL2CPP
    static class Registration
    {
        static bool s_Registered;
        
        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        static void Initialize()
        {
            if (s_Registered)
            {
                return;
            }

            s_Registered = true;
            PropertyBagStore.RegisterProvider(new ReflectedPropertyBagProvider());
        }
    }
#endif

    class ReflectedPropertyBagProvider : IPropertyBagProvider
    {
        readonly MethodInfo m_CreatePropertyBagMethod;
        readonly MethodInfo m_CreatePropertyMethod;
        readonly MethodInfo m_CreateListPropertyBagMethod;
        readonly MethodInfo m_CreateSetPropertyBagMethod;
        readonly MethodInfo m_CreateDictionaryPropertyBagMethod;
        readonly MethodInfo m_CreateKeyValuePairPropertyBagMethod;
        
        public ReflectedPropertyBagProvider()
        {
            m_CreatePropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethods(BindingFlags.Instance | BindingFlags.Public).First(x => x.Name == nameof(CreatePropertyBag) && x.IsGenericMethod);
            m_CreatePropertyMethod = typeof(ReflectedPropertyBagProvider).GetMethod(nameof(CreateProperty), BindingFlags.Instance | BindingFlags.NonPublic);
            m_CreateListPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod(nameof(CreateListPropertyBag), BindingFlags.Instance | BindingFlags.NonPublic);
            m_CreateSetPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod(nameof(CreateSetPropertyBag), BindingFlags.Instance | BindingFlags.NonPublic);
            m_CreateDictionaryPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod(nameof(CreateDictionaryPropertyBag), BindingFlags.Instance | BindingFlags.NonPublic);
            m_CreateKeyValuePairPropertyBagMethod = typeof(ReflectedPropertyBagProvider).GetMethod(nameof(CreateKeyValuePairPropertyBag), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public IPropertyBag CreatePropertyBag(Type type)
        {
            return (IPropertyBag) m_CreatePropertyBagMethod.MakeGenericMethod(type).Invoke(this, null);
        }

        public IPropertyBag<TContainer> CreatePropertyBag<TContainer>()
        {
            if (!RuntimeTypeInfoCache<TContainer>.IsContainerType || RuntimeTypeInfoCache<TContainer>.IsObjectType)
            {                
                throw new InvalidOperationException("Invalid container type.");
            }

            if (typeof(TContainer).IsArray)
            {
                return (IPropertyBag<TContainer>) m_CreateListPropertyBagMethod.MakeGenericMethod(typeof(TContainer), typeof(TContainer).GetElementType()).Invoke(this, new object[0]);
            }
            
            if (typeof(TContainer).IsGenericType && (typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) || typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(IList<>))))
            {
                return (IPropertyBag<TContainer>) m_CreateListPropertyBagMethod.MakeGenericMethod(typeof(TContainer), typeof(TContainer).GetGenericArguments().First()).Invoke(this, new object[0]);
            }
            
            if (typeof(TContainer).IsGenericType && (typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(HashSet<>)) || typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(ISet<>))))
            {
                return (IPropertyBag<TContainer>) m_CreateSetPropertyBagMethod.MakeGenericMethod(typeof(TContainer), typeof(TContainer).GetGenericArguments().First()).Invoke(this, new object[0]);
            }
            
            if (typeof(TContainer).IsGenericType && (typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)) || typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(IDictionary<,>))))
            {
                var types = typeof(TContainer).GetGenericArguments().ToArray();
                return (IPropertyBag<TContainer>) m_CreateDictionaryPropertyBagMethod.MakeGenericMethod(typeof(TContainer), types[0], types[1]).Invoke(this, new object[0]);
            }
            
            if (typeof(TContainer).IsGenericType && typeof(TContainer).GetGenericTypeDefinition().IsAssignableFrom(typeof(KeyValuePair<,>)))
            {
                var types = typeof(TContainer).GetGenericArguments().ToArray();
                return (IPropertyBag<TContainer>) m_CreateKeyValuePairPropertyBagMethod.MakeGenericMethod(types[0], types[1]).Invoke(this, new object[0]);
            }

            var propertyBag = new ReflectedPropertyBag<TContainer>();

            foreach (var member in GetPropertyMembers(typeof(TContainer)))
            {
                IMemberInfo info;

                switch (member)
                {
                    case FieldInfo field:
                        info = new FieldMember(field);
                        break;
                    case PropertyInfo property:
                        info = new PropertyMember(property);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                m_CreatePropertyMethod.MakeGenericMethod(typeof(TContainer), info.ValueType).Invoke(this, new object[]
                {
                    info,
                    propertyBag
                });
            }

            return propertyBag;
        }

        [Preserve]
        void CreateProperty<TContainer, TValue>(IMemberInfo member, ReflectedPropertyBag<TContainer> propertyBag)
        {
            if (typeof(TValue).IsPointer)
            {
                return;
            }
            
            propertyBag.AddProperty(new ReflectedMemberProperty<TContainer, TValue>(member));
        }

        [Preserve]
        IPropertyBag<TSet> CreateSetPropertyBag<TSet, TValue>()
            where TSet : ISet<TValue>
        {
            return new SetPropertyBag<TSet, TValue>();
        }

        [Preserve]
        IPropertyBag<TList> CreateListPropertyBag<TList, TElement>()
            where TList : IList<TElement>
        {
            return new ListPropertyBag<TList, TElement>();
        }
        
        [Preserve]
        IPropertyBag<TDictionary> CreateDictionaryPropertyBag<TDictionary, TKey, TValue>()
            where TDictionary : IDictionary<TKey, TValue>
        {
            return new DictionaryPropertyBag<TDictionary, TKey, TValue>();
        }
        
        [Preserve]
        IPropertyBag<KeyValuePair<TKey, TValue>> CreateKeyValuePairPropertyBag<TKey, TValue>()
        {
            return new KeyValuePairPropertyBag<TKey, TValue>();
        }

        static IEnumerable<MemberInfo> GetPropertyMembers(Type type)
        {
            do
            {
                var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).OrderBy(x => x.MetadataToken);

                foreach (var member in members)
                {
                    if (member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
                    {
                        continue;
                    }

                    if (member.DeclaringType != type)
                    {
                        continue;
                    }

                    if (!IsValidMemberType(member))
                    {
                        continue;
                    }

                    // Gather all possible attributes we care about.
                    var hasDontCreatePropertyAttribute = member.GetCustomAttribute<DontCreatePropertyAttribute>() != null;
                    var hasCreatePropertyAttribute = member.GetCustomAttribute<CreatePropertyAttribute>() != null;
                    var hasNonSerializedAttribute = member.GetCustomAttribute<NonSerializedAttribute>() != null;
                    var hasSerializedFieldAttribute = member.GetCustomAttribute<SerializeField>() != null;

                    if (hasDontCreatePropertyAttribute)
                    {
                        // This attribute trumps all others. No matter what a property should NOT be generated.
                        continue;
                    }
                    
                    if (hasCreatePropertyAttribute)
                    {
                        // The user explicitly requests an attribute, one will be generated, regardless of serialization attributes.
                        yield return member;
                        continue;
                    }
                    
                    if (hasNonSerializedAttribute)
                    {
                        // If property generation was not explicitly specified lets keep behaviour consistent with Unity.
                        continue;
                    }
                    
                    if (hasSerializedFieldAttribute)
                    {
                        // If property generation was not explicitly specified lets keep behaviour consistent with Unity.
                        yield return member;
                        continue;
                    }
                    
                    // No attributes were specified, if this is a public field we will generate one by implicitly.
                    if (member is FieldInfo field && field.IsPublic)
                    {
                        yield return member;
                    }
                }

                type = type.BaseType;
            } 
            while (type != null && type != typeof(object));
        }

        static bool IsValidMemberType(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return !(fieldInfo.IsStatic || fieldInfo.FieldType.IsPointer);
                case PropertyInfo propertyInfo:
                    return !(null == propertyInfo.GetMethod || propertyInfo.GetMethod.IsStatic || propertyInfo.PropertyType.IsPointer);
            }

            return false;
        }
    }
}
#endif
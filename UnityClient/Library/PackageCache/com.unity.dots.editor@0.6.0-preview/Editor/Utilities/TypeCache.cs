using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Unity.Entities.Editor
{
    class TypeCache
    {
        static class DefaultValueCache<T> where T : struct
        {
            static DefaultValueCache()
            {
                try
                {
                    var type = typeof(T);
                    const string defaultPropertyName = "Default";
                    var defaultProperty = type.GetProperty(defaultPropertyName, BindingFlags.Public | BindingFlags.Static);
                    if (defaultProperty != null && defaultProperty.GetMethod != null &&
                        defaultProperty.GetMethod.ReturnType == type)
                    {
                        DefaultValue = (T)defaultProperty.GetValue(null);
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }

            public static readonly T DefaultValue;
        }

        public static class AttributeCache<T>
        {
            private struct Lookup<TAttribute>
            {
                public static readonly bool Any;

                static Lookup()
                {
                    var type = typeof(T);
                    Any = type.GetCustomAttributes(typeof(TAttribute), true).Any();

                    if (!Any && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DynamicBufferContainer<>))
                    {
                        Any = type.GetGenericArguments()[0].GetCustomAttributes(typeof(TAttribute), true).Any();
                    }
                }
            }

            public static bool HasAttribute<TAttribute>()
                where TAttribute : Attribute
            {
                return Lookup<TAttribute>.Any;
            }
        }

        public static T GetDefaultValueForStruct<T>() where T : struct
        {
            return DefaultValueCache<T>.DefaultValue;
        }

        public static object GetDefaultValue(Type type)
        {
            if (!type.IsValueType)
            {
                return null;
            }

            var generic = typeof(DefaultValueCache<>).MakeGenericType(type);
            RuntimeHelpers.RunClassConstructor(generic.TypeHandle);
            const string defaultPropertyName = "DefaultValue";
            return generic.GetField(defaultPropertyName, BindingFlags.Public | BindingFlags.Static) ?
                .GetValue(null);
        }

        public static bool HasAttribute<TType, TAttribute>()
            where TAttribute : Attribute
        {
            return AttributeCache<TType>.HasAttribute<TAttribute>();
        }
    }
}

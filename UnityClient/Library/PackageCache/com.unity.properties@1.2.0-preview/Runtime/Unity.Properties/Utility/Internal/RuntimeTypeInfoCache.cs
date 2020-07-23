using System;
using System.Reflection;

namespace Unity.Properties.Internal
{
    struct RuntimeTypeInfoCache
    {
        public static bool IsContainerType(Type type)
        {
            return !(type.IsPrimitive || type.IsPointer || type.IsEnum || type == typeof(string));
        }
    }

    /// <summary>
    /// Helper class to avoid paying the cost of runtime type lookups.
    ///
    /// This is also used to abstract underlying type info in the runtime (e.g. RuntimeTypeHandle vs StaticTypeReg)
    /// </summary>
    struct RuntimeTypeInfoCache<T>
    {
        public static readonly bool IsValueType;
        public static readonly bool IsPrimitive;
        public static readonly bool IsInterface;
        public static readonly bool IsAbstract;
        public static readonly bool IsGeneric;
        public static readonly bool IsArray;
        public static readonly bool IsEnum;
        public static readonly bool IsEnumFlags;
        public static readonly bool IsNullable;

        public static readonly bool IsObjectType;
        public static readonly bool IsStringType;
        public static readonly bool IsContainerType;

        public static readonly bool CanBeNull;
        public static readonly bool IsNullableOrEnum;
        public static readonly bool IsPrimitiveOrString;
        public static readonly bool IsAbstractOrInterface;

        public static readonly bool IsLazyLoadReference;

        static RuntimeTypeInfoCache()
        {
            var type = typeof(T);
            IsValueType = type.IsValueType;
            IsPrimitive = type.IsPrimitive;
            IsInterface = type.IsInterface;
            IsAbstract = type.IsAbstract;
            IsGeneric = type.IsGenericType;
            IsArray = type.IsArray;
            IsEnum = type.IsEnum;

#if !NET_DOTS
            IsEnumFlags = IsEnum && null != type.GetCustomAttribute<FlagsAttribute>();
            IsNullable = Nullable.GetUnderlyingType(typeof(T)) != null;
#else
            IsEnumFlags = false;
            IsNullable = false;
#endif

            IsObjectType = type == typeof(object);
            IsStringType = type == typeof(string);
            IsContainerType = RuntimeTypeInfoCache.IsContainerType(type);

            CanBeNull = !IsValueType || IsNullable;
            IsNullableOrEnum = IsNullable || IsEnum;
            IsPrimitiveOrString = IsPrimitive || IsStringType;
            IsAbstractOrInterface = IsAbstract || IsInterface;

            IsLazyLoadReference = IsGeneric && type.GetGenericTypeDefinition() == typeof(UnityEngine.LazyLoadReference<>);
        }
    }
}
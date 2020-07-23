using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Unity.Properties.Editor
{
    /// <summary>
    /// Helper class to create new instances for given types.
    /// </summary>
    public static class TypeConstruction
    {
        /// <summary>
        /// Represents the method that will handle constructing a specified <typeparamref name="TType"/>.
        /// </summary>
        /// <typeparam name="TType">The type this delegate constructs.</typeparam>
        public delegate TType ConstructorMethod<out TType>();

        static TypeConstruction()
        {
            RegisterBuiltInConstructors();
        }

        static void RegisterBuiltInConstructors()
        {
            TypeConstructionCache.SetExplicitConstruction(() => string.Empty);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the specified type is constructable.
        /// </summary>
        /// <remarks>
        /// Constructable is defined as either having a default or implicit constructor or having a registered construction method.
        /// </remarks>
        /// <param name="type">The type to query.</param>
        /// <returns><see langword="true"/> if the given type is constructable.</returns>
        public static bool CanBeConstructed(Type type)
        {
            return TypeConstructionCache.CanBeConstructed(type);
        }

        /// <summary>
        /// Returns <see langword="true"/> if type <see cref="T"/> is constructable.
        /// </summary>
        /// <remarks>
        /// Constructable is defined as either having a default or implicit constructor or having a registered construction method.
        /// </remarks>
        /// <typeparam name="T">The type to query.</typeparam>
        /// <returns><see langword="true"/> if type <see cref="T"/> is constructable.</returns>
        public static bool CanBeConstructed<T>()
        {
            return TypeConstructionCache.CanBeConstructed<T>();
        }

        /// <summary>
        /// Constructs a new instance of the specified <see cref="TType"/>.
        /// </summary>
        /// <param name="instance">When this method returns, contains the created instance, if type construction succeeded; otherwise, the default value for <typeparamref name="TType"/>.</param>
        /// <typeparam name="TType">The type to create an instance of.</typeparam>
        /// <returns><see langword="true"/> if a new instance of type <see cref="TType"/> was created; otherwise, <see langword="false"/>.</returns>
        public static bool TryConstruct<TType>(out TType instance)
        {
            return TypeConstructionCache.TryConstruct(out instance);
        }

        /// <summary>
        /// Constructs a new instance of the specified <see cref="TType"/>.
        /// </summary>
        /// <typeparam name="TType">The type we want to create a new instance of.</typeparam>
        /// <returns>A new instance of the <see cref="TType"/>.</returns>
        public static TType Construct<TType>()
        {
            return TypeConstructionCache.Construct<TType>();
        }

        /// <summary>
        /// Sets the explicit construction method for the <see cref="TType"/>.
        /// </summary>
        /// <param name="constructor">The construction method.</param>
        /// <typeparam name="TType">The type to set the explicit construction method.</typeparam>
        /// <returns><see langword="true"/> if the constructor was set; otherwise, <see langword="false"/>.</returns>
        public static bool TrySetExplicitConstructionMethod<TType>(ConstructorMethod<TType> constructor)
        {
            if (TypeConstructionCache.HasExplicitConstruction<TType>())
                return false;

            TypeConstructionCache.SetExplicitConstruction(constructor);
            return true;
        }

        /// <summary>
        /// Un-sets the explicit construction method for the <see cref="TType"/> type.
        /// </summary>
        /// <remarks>
        /// An explicit construction method can only be unset if it was previously set with the same instance.
        /// </remarks>
        /// <param name="constructor">The construction method.</param>
        /// <typeparam name="TType">The type to set the explicit construction method.</typeparam>
        /// <returns><see langword="true"/> if the constructor was unset; otherwise, <see langword="false"/>.</returns>
        public static bool TryUnsetExplicitConstructionMethod<TType>(ConstructorMethod<TType> constructor)
        {
            if (TypeConstructionCache.GetExplicitConstruction<TType>() != constructor)
                return false;

            TypeConstructionCache.SetExplicitConstruction<TType>(null);
            return true;
        }

        /// <summary>
        /// Sets an explicit construction method for the <see cref="TType"/> type.
        /// </summary>
        /// <param name="constructor">The construction method.</param>
        /// <typeparam name="TType">The type to set the explicit construction method.</typeparam>
        public static void SetExplicitConstructionMethod<TType>(ConstructorMethod<TType> constructor)
        {
            if (!TrySetExplicitConstructionMethod(constructor))
                throw new InvalidOperationException();
        }

        /// <summary>
        /// Un-sets the explicit construction method for the <see cref="TType"/> type.
        /// </summary>
        /// <remarks>
        /// An explicit construction method can only be unset if it was previously set with the same instance.
        /// </remarks>
        /// <param name="constructor">The construction method.</param>
        /// <typeparam name="TType">The type to set the explicit construction method.</typeparam>
        public static void UnsetExplicitConstructionMethod<TType>(ConstructorMethod<TType> constructor)
        {
            if (!TryUnsetExplicitConstructionMethod(constructor))
                throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns a list of all the constructable types from the <see cref="TType"/> type.
        /// </summary>
        /// <typeparam name="TType">The type to query.</typeparam>
        /// <returns>A list of all the constructable types from the <see cref="TType"/> type.</returns>
        public static IEnumerable<Type> GetAllConstructableTypes<TType>()
        {
            return TypeConstructionCache.GetConstructableTypes<TType>();
        }

        /// <summary>
        /// Adds all the constructable types from the <see cref="TType"/> type to the given list. 
        /// </summary>
        /// <param name="result">List to contain the results.</param>
        /// <typeparam name="TType">The type to query.</typeparam>
        public static void GetAllConstructableTypes<TType>(List<Type> result)
        {
            result.AddRange(TypeConstructionCache.GetConstructableTypes<TType>());
        }

        /// <summary>
        /// Returns a list of all the constructable types from the provided type.
        /// </summary>
        /// /// <param name="type">The type to query.</param>
        /// <returns>A list of all the constructable types from the provided type.</returns>
        public static IEnumerable<Type> GetAllConstructableTypes(Type type)
        {
            return TypeConstructionCache.GetConstructableTypes(type);
        }
        
        /// <summary>
        /// Adds all the constructable types from the provided type to the given list. 
        /// </summary>
        /// <param name="type">The type to query.</param>
        /// <param name="result">List to contain the results.</param>
        public static void GetAllConstructableTypes(Type type, List<Type> result)
        {
            result.AddRange(TypeConstructionCache.GetConstructableTypes(type));
        }
        
        /// <summary>
        /// Returns <see langword="true"/> if type <see cref="T"/> is constructable from any of its derived types.
        /// </summary>
        /// <remarks>
        /// Constructable is defined as either having a default or implicit constructor or having a registered construction method.
        /// </remarks>
        /// <typeparam name="T">The type to query.</typeparam>
        /// <returns><see langword="true"/> if type <see cref="T"/> is constructable from any of its derived types.</returns>
        public static bool CanBeConstructedFromDerivedType<T>()
        {
            return TypeConstructionCache.GetConstructableTypes<T>().Any(type => type != typeof(T));
        }
        
        /// <summary>
        /// Constructs a new instance of the given type type and returns it as <see cref="TType"/>.
        /// </summary>
        /// <param name="derivedType">The type we want to create a new instance of.</param>
        /// <typeparam name="TType">The type we want to create a new instance of.</typeparam>
        /// <returns>a new instance of the <see cref="TType"/> type.</returns>
        /// <exception cref="ArgumentException">Thrown when the given type is not assignable to <see cref="TType"/></exception>
        public static TType Construct<TType>(Type derivedType)
        {
            return TypeConstructionCache.Construct<TType>(derivedType);
        }

        /// <summary>
        /// Tries to constructs a new instance of the given type type and returns it as <see cref="TType"/>.
        /// </summary>
        /// <param name="derivedType">The type we want to create a new instance of.</param>
        /// <param name="value">When this method returns, contains the created instance, if type construction succeeded; otherwise, the default value for <typeparamref name="TType"/>.</param>
        /// <typeparam name="TType">The type we want to create a new instance of.</typeparam>
        /// <returns><see langword="true"/> if a new instance of the given type could be created.</returns>
        public static bool TryConstruct<TType>(Type derivedType, out TType value)
        {
            if (TypeConstructionCache.TryConstruct(derivedType, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Tries to construct a new instance of an array with the given count.
        /// </summary>
        /// <param name="count">The count the array should have.</param>
        /// <param name="instance">When this method returns, contains the created instance, if type construction succeeded; otherwise, the default value for <typeparamref name="TType"/>.</param>
        /// <typeparam name="TArray">The array type.</typeparam>
        /// <returns><see langword="true"/> if the type was constructed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown is count is negative or if <see cref="TArray"/> is not an array type.</exception>
        public static bool TryConstructArray<TArray>(int count, out TArray instance)
        {
            if (count < 0)
            {
                instance = default;
                return false;
            }

            var type = typeof(TArray);

            if (!type.IsArray)
            {
                instance = default;
                return false;   
            }

            var elementType = type.GetElementType();
            if (null == elementType)
            {
                instance = default;
                return false;
            }
            
            instance = (TArray)(object) Array.CreateInstance(elementType, count);
            return true;
        }
        
        /// <summary>
        /// Construct a new instance of an array with the given count.
        /// </summary>
        /// <param name="count">The size of the array to construct.</param>
        /// <typeparam name="TArray">The array type to construct.</typeparam>
        /// <returns>The array newly constructed array.</returns>
        /// <exception cref="ArgumentException">Thrown is count is negative or if <see cref="TArray"/> is not an array type.</exception>
        public static TArray ConstructArray<TArray>(int count = 0)
        {
            if (count < 0)
            {
                throw new ArgumentException(
                    $"{nameof(TypeConstruction)}: Cannot construct an array with {nameof(count)}={count}");
            }

            var type = typeof(TArray);

            if (!type.IsArray)
            {
                throw new ArgumentException(
                    $"{nameof(TypeConstruction)}: Cannot construct an array, since{typeof(TArray).Name} is not an array type.");
            }

            var elementType = type.GetElementType();
            if (null == elementType)
            {
                throw new ArgumentException(
                    $"{nameof(TypeConstruction)}: Cannot construct an array, since{typeof(TArray).Name}.{nameof(Type.GetElementType)}() returned null.");
            }
            
            return (TArray) (object) Array.CreateInstance(elementType, count);
        }

        static class TypeConstructionCache
        {
            static readonly MethodInfo CacheTypeMethodInfo;
            
            static TypeConstructionCache()
            {
                CacheTypeMethodInfo = typeof(TypeConstructionCache).GetMethod(nameof(CacheStaticTypeInfoImpl),
                    BindingFlags.NonPublic | BindingFlags.Static);
            }
            
            interface IConstructor
            {
                bool TryConstruct<T>(out T instance);
            }

            interface IConstructor<out TType> : IConstructor
            {
                ConstructorMethod<TType> Construct { get; }
            }

            class Constructor<TType> : IConstructor<TType>
            {
                public Constructor(ConstructorMethod<TType> method)
                {
                    Construct = method;
                }

                public ConstructorMethod<TType> Construct { get; }
                
                bool IConstructor.TryConstruct<T>(out T instance)
                {
                    if (typeof(T).IsAssignableFrom(typeof(TType)))
                    {
                        instance = (T)(object) Construct.Invoke();
                        return true;
                    }

                    instance = default;
                    return false;
                }
            }

            static readonly HashSet<Type> ConsideredTypes = new HashSet<Type>();
            static readonly Dictionary<Type, IConstructor> ExplicitConstruction = new Dictionary<Type, IConstructor>();
            static readonly Dictionary<Type, IConstructor> ImplicitConstruction = new Dictionary<Type, IConstructor>();

            public static IEnumerable<Type> GetConstructableTypes<TType>()
            {
                if (CanBeConstructed<TType>())
                {
                    yield return typeof(TType);
                }

                foreach (var type in UnityEditor.TypeCache.GetTypesDerivedFrom<TType>().Where(CanBeConstructed))
                {
                    yield return type;
                }
            }

            public static IEnumerable<Type> GetConstructableTypes(Type type)
            {
                if (CanBeConstructed(type))
                {
                    yield return type;
                }

                foreach (var t in UnityEditor.TypeCache.GetTypesDerivedFrom(type).Where(CanBeConstructed))
                {
                    yield return t;
                }
            }
            
            public static bool TryConstruct<TType>(out TType instance)
            {
                if (HasExplicitConstruction<TType>())
                {
                    instance = GetExplicitConstruction<TType>().Invoke();
                    return true;
                }

                if (HasImplicitConstruction<TType>())
                {
                    instance = GetImplicitConstruction<TType>().Invoke();
                    return true;
                }

                instance = default;
                return false;
            }

            public static bool TryConstruct<TType>(Type derivedType, out TType instance)
            {
                if (!typeof(TType).IsAssignableFrom(derivedType))
                {
                    instance = default;
                    return false;
                }

                if (HasExplicitConstruction(derivedType))
                {
                    if (GetExplicitConstruction(derivedType).TryConstruct(out instance))
                    {
                        return true;
                    }
                }

                if (HasImplicitConstruction(derivedType))
                {
                    if (GetImplicitConstruction(derivedType).TryConstruct(out instance))
                    {
                        return true;
                    }
                }
                
                instance = default;
                return false;
            }
            
            public static TType Construct<TType>()
            {
                if (TryConstruct(out TType instance))
                {
                    return instance;
                }

                throw new InvalidOperationException(
                    $"Type `{typeof(TType).Name}` could not be constructed. A parameter-less constructor or an explicit construction method is required.");
            }

            public static TType Construct<TType>(Type derivedType)
            {
                if (!typeof(TType).IsAssignableFrom(derivedType))
                {
                    throw new ArgumentException(
                        $"Could not create instance of type `{derivedType.Name}` and convert to `{typeof(TType).Name}`: given type is not assignable to target type.");
                }

                if (TryConstruct(derivedType, out TType instance))
                {
                    return instance;
                }

                throw new InvalidOperationException(
                    $"Type `{derivedType.Name}` could not be constructed. A parameter-less constructor or an explicit construction method is required.");
            }
            
            public static bool CanBeConstructed(Type type)
                => HasExplicitConstruction(type) || HasImplicitConstruction(type);

            public static bool CanBeConstructed<TType>()
                => CanBeConstructedImpl<TType>();

            static bool CanBeConstructedImpl<TType>()
                => HasExplicitConstruction<TType>() || HasImplicitConstruction<TType>();

            public static bool HasExplicitConstruction<TType>()
                => HasExplicitConstruction(typeof(TType));

            public static bool HasExplicitConstruction(Type type)
                => ExplicitConstruction.ContainsKey(type);

            public static ConstructorMethod<TType> GetExplicitConstruction<TType>()
            {
                if (ExplicitConstruction.TryGetValue(typeof(TType), out var method) &&
                    method is Constructor<TType> typed)
                {
                    return typed.Construct;
                }

                return default;
            }

            public static void SetExplicitConstruction<TType>(ConstructorMethod<TType> constructor)
            {
                if (null == constructor)
                {
                    ExplicitConstruction.Remove(typeof(TType));
                }
                else
                {
                    ExplicitConstruction[typeof(TType)] = new Constructor<TType>(constructor);
                }
            }

            static bool HasImplicitConstruction<TType>()
            {
                CacheStaticTypeInfoImpl<TType>();
                return ImplicitConstruction.TryGetValue(typeof(TType), out var method)
                       && method is Constructor<TType>;
            }

            static bool HasImplicitConstruction(Type type)
            {
                CacheStaticTypeInfo(type);
                return ImplicitConstruction.ContainsKey(type);
            }

            static ConstructorMethod<TType> GetImplicitConstruction<TType>()
            {
                if (ImplicitConstruction.TryGetValue(typeof(TType), out var method) &&
                    method is Constructor<TType> typed)
                {
                    return typed.Construct;
                }

                return default;
            }

            static void CacheStaticTypeInfo(Type type)
            {
                if (ConsideredTypes.Contains(type))
                    return;
                
                if (type.ContainsGenericParameters)
                {
                    ConsideredTypes.Add(type);
                    return;
                }
                
                CacheTypeMethodInfo
                    .MakeGenericMethod(type)
                    .Invoke(null, null);
            }

            static void CacheStaticTypeInfoImpl<TType>()
            {
                if (ConsideredTypes.Contains(typeof(TType)))
                    return;

                var type = typeof(TType);
                ConsideredTypes.Add(type);

                if (type.IsValueType)
                {
                    ImplicitConstruction[type] = new Constructor<TType>(CreateValueTypeInstance<TType>);
                    return;
                }

                if (type.IsAbstract)
                {
                    return;
                }

                if (typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    ImplicitConstruction[type] = new Constructor<TType>(CreateScriptableObjectInstance<TType>);
                    return;
                }

                if (null != type.GetConstructor(Array.Empty<Type>()))
                {
                    ImplicitConstruction[type] = new Constructor<TType>(CreateClassInstance<TType>);
                }
            }

            static TType CreateValueTypeInstance<TType>()
            {
                return default;
            }

            static TType CreateScriptableObjectInstance<TType>()
            {
                return (TType) (object) ScriptableObject.CreateInstance(typeof(TType));
            }

            static TType CreateClassInstance<TType>()
            {
                return Activator.CreateInstance<TType>();
            }
            
            static IConstructor GetExplicitConstruction(Type type)
            {
                return ExplicitConstruction.TryGetValue(type, out var constructor) ? constructor : default;
            }
            
            static IConstructor GetImplicitConstruction(Type type)
            {
                return ImplicitConstruction.TryGetValue(type, out var constructor) ? constructor : default;
            }
        }
    }
}

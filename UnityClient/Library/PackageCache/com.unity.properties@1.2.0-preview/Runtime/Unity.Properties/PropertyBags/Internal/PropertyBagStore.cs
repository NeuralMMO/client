using System;
using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// Static class used to store all property bags. This is an internal class.
    /// </summary>
    /// <remarks>
    /// This storage is used to resolve <see cref="IPropertyBag{TContainer}"/> types by internal properties algorithms.
    /// </remarks>
    static class PropertyBagStore
    {
        internal struct TypedStore<TContainer>
        {
            public static IPropertyBag<TContainer> PropertyBag;
        }
        
#if !NET_DOTS
        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, IPropertyBag> s_PropertyBags = new System.Collections.Concurrent.ConcurrentDictionary<Type, IPropertyBag>();
#else 
        static readonly Dictionary<Type, IPropertyBag> s_PropertyBags = new Dictionary<Type, IPropertyBag>();
#endif
        
        /// <summary>
        /// Instance of the dynamic property bag provider. This is used to allow an external assembly to generate property bags for us.
        /// </summary>
        static IPropertyBagProvider s_PropertyBagProvider;
        
        /// <summary>
        /// Registers a dynamic property bag provider. This is an internal method.
        /// </summary>
        /// <param name="provider">The property bag provider to add.</param>
        /// <exception cref="ArgumentNullException">The provider is null.</exception>
        /// <exception cref="InvalidOperationException">A provider has already been registered.</exception>
        internal static void RegisterProvider(IPropertyBagProvider provider)
        {
            if (null != s_PropertyBagProvider)
            {
                throw new InvalidOperationException($"An existing {nameof(IPropertyBagProvider)} has already been registered. Current provider is {s_PropertyBagProvider.GetType()}.");
            }
            
            s_PropertyBagProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Adds a <see cref="ContainerPropertyBag{TContainer}"/> to the store.
        /// </summary>
        /// <param name="propertyBag">The <see cref="ContainerPropertyBag{TContainer}"/> to add.</param>
        /// <typeparam name="TContainer">The container type this <see cref="ContainerPropertyBag{TContainer}"/> describes.</typeparam>
        internal static void AddPropertyBag<TContainer>(IPropertyBag<TContainer> propertyBag)
        {
            if (!RuntimeTypeInfoCache<TContainer>.IsContainerType)
            {
                throw new Exception($"PropertyBagStore Type=[{typeof(TContainer)}] is not a valid container type. Type can not be primitive, enum or string.");
            }

            if (RuntimeTypeInfoCache<TContainer>.IsAbstractOrInterface)
            {
                throw new Exception($"PropertyBagStore Type=[{typeof(TContainer)}] is not a valid container type. Type can not be abstract or interface.");
            }
            
            TypedStore<TContainer>.PropertyBag = propertyBag;
            s_PropertyBags[typeof(TContainer)] = propertyBag;
        }
        
        /// <summary>
        /// Gets the strongly typed <see cref="ContainerPropertyBag{TContainer}"/> for the given <typeparamref name="TContainer"/>.
        /// </summary>
        /// <typeparam name="TContainer">The container type to resolve the property bag for.</typeparam>
        /// <returns>The resolved property bag, strongly typed.</returns>
        internal static IPropertyBag<TContainer> GetPropertyBag<TContainer>()
        {
            if (null != TypedStore<TContainer>.PropertyBag)
            {
                return TypedStore<TContainer>.PropertyBag;
            }

            var untyped = GetPropertyBag(typeof(TContainer));

            if (null == untyped)
            {
                return null;
            }
            
            if (!(untyped is IPropertyBag<TContainer> typed))
            {
                throw new InvalidOperationException($"PropertyBag type container type mismatch.");
            }
            
            return typed;
        }

        /// <summary>
        /// Gets an interface to the <see cref="ContainerPropertyBag{TContainer}"/> for the given type.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="IPropertyBag"/> can be used to get the strongly typed generic using the <see cref="IPropertyBag.Accept"/> method. 
        /// </remarks>
        /// <param name="type">The container type to resolve the property bag for.</param>
        /// <returns>The resolved property bag.</returns>
        internal static IPropertyBag GetPropertyBag(Type type)
        {
            if (s_PropertyBags.TryGetValue(type, out var propertyBag))
            {
                return propertyBag;
            }

            if (!RuntimeTypeInfoCache.IsContainerType(type))
            {
                return null;
            }

            if (type.IsInterface || type.IsAbstract)
            {
                return null;
            }
            
            if (null != s_PropertyBagProvider)
            {
                propertyBag = s_PropertyBagProvider.CreatePropertyBag(type);

                if (null == propertyBag)
                {
                    s_PropertyBags.TryAdd(type, null);
                }
                else
                {
                    propertyBag.Register();
                    return propertyBag;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the given type has a static property bag registered.
        /// </summary>
        /// <typeparam name="TContainer"></typeparam>
        /// <returns><see langword="true"/> if the property bag exists; otherwise, <see langword="false"/>.</returns>
        internal static bool Exists<TContainer>()
        {
            return null != TypedStore<TContainer>.PropertyBag;
        }

        /// <summary>
        /// Returns true if the given <paramref name="value"/> type is backed by a property bag.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <typeparam name="TContainer">The container type to check.</typeparam>
        /// <returns><see langword="true"/> if the property bag exists; otherwise, <see langword="false"/>.</returns>
        internal static bool Exists<TContainer>(ref TContainer value)
        {
            if (!RuntimeTypeInfoCache<TContainer>.CanBeNull)
            {
                return GetPropertyBag<TContainer>() != null;
            }
            
            // We can't reliably determine if there is a property bag.
            if (EqualityComparer<TContainer>.Default.Equals(value, default(TContainer)))
            {
                return false;
            }
            
            return GetPropertyBag(value.GetType()) != null;
        }
        
        /// <summary>
        /// Gets a property bag for the concrete type of the given value.
        /// </summary>
        /// <param name="value">The value type to retrieve a property bag for.</param>
        /// <param name="propertyBag">When this method returns, contains the property bag associated with the specified value, if the bag is found; otherwise, null.</param>
        /// <typeparam name="TValue"></typeparam>
        /// <returns><see langword="true"/> if the property bag was found for the specified value; otherwise, <see langword="false"/>.</returns>
        internal static bool TryGetPropertyBagForValue<TValue>(ref TValue value, out IPropertyBag propertyBag)
        {
            // We can not recurse on a null value.
            if (RuntimeTypeInfoCache<TValue>.CanBeNull)
            {
                if (EqualityComparer<TValue>.Default.Equals(value, default))
                {
                    propertyBag = null;
                    return false;
                }
            }
            
            // early out for primitive types that don't have associated containers
            // note: GetPropertyBag checks for RuntimeTypeInfoCache.IsContainerType(type) already
            if (!RuntimeTypeInfoCache<TValue>.IsContainerType)
            {
                propertyBag = null;
                return false;
            }

            propertyBag = GetPropertyBag(value.GetType());
            return null != propertyBag;
        }
    }
}
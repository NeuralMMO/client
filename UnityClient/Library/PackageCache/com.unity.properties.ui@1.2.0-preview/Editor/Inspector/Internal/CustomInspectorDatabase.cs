using System;
using System.Collections.Generic;
using Unity.Properties.Internal;
using UnityEditor;
using UnityEngine;

namespace Unity.Properties.UI.Internal
{
    /// <summary>
    /// Maintains a database of all the inspector-related types and allows creation of new instances of inspectors.
    /// </summary>
    static class CustomInspectorDatabase
    {
        static readonly Dictionary<Type, List<Type>> s_InspectorsPerType;

        static CustomInspectorDatabase()
        {
            s_InspectorsPerType = new Dictionary<Type, List<Type>>();
            RegisterCustomInspectors();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="IInspector{TValue}"/> that can act as a root inspector.
        /// </summary>
        /// <param name="constraints">Constraints that filter the candidate inspector types.</param>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <returns>The inspector instance or null</returns>
        public static Inspector<TValue> GetRootInspector<TValue>(params IInspectorConstraint[] constraints)
        {
            return GetInspector<TValue>(
                InspectorConstraint.Combine(InspectorConstraint.Not.AssignableTo<IPropertyDrawer>(), constraints));
        }

        /// <summary>
        /// Creates a new instance of a <see cref="PropertyDrawer{TValue, TAttribute}"/> that can act as a property drawer
        /// for a given field.
        /// </summary>
        /// <param name="constraints">Constraints that filter the candidate property drawer types.</param>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <typeparam name="TAttribute">The attribute type the value was tagged with</typeparam>
        /// <returns>The property drawer instance or null</returns>
        public static PropertyDrawer<TValue, TAttribute> GetPropertyDrawer<TValue, TAttribute>(
            params IInspectorConstraint[] constraints)
            where TAttribute : UnityEngine.PropertyAttribute
        {
            return (PropertyDrawer<TValue, TAttribute>) GetInspector<TValue>(
                InspectorConstraint.Combine(InspectorConstraint.AssignableTo<IPropertyDrawer<TAttribute>>(),
                    constraints));
        }

        /// <summary>
        /// Returns all the inspector candidate types that satisfy the constraints.
        /// </summary>
        /// <param name="constraints">Constraints that filter the candidate property drawer types.</param>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <returns>The candidate inspector types</returns>
        internal static IEnumerable<Type> GetInspectorTypes<TValue>(params IInspectorConstraint[] constraints)
        {
            return GetInspectorTypes(typeof(TValue), constraints);
        }

        /// <summary>
        /// Creates an inspector instance that satisfy the constraints.
        /// </summary>
        /// <param name="constraints">Constraints that filter the candidate property drawer types.</param>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <returns>An inspector instance of null</returns>
        internal static Inspector<TValue> GetInspector<TValue>(params IInspectorConstraint[] constraints)
        {
            foreach (var type in GetInspectorTypes<TValue>(constraints))
            {
                return (Inspector<TValue>) Activator.CreateInstance(type);
            }

            return null;
        }

        internal static IInspector<TValue> GetPropertyDrawer<TValue>(IProperty property)
        {
            return GetPropertyDrawer<TValue>(property.GetAttributes<UnityEngine.PropertyAttribute>() 
                                             ?? Array.Empty<UnityEngine.PropertyAttribute>());
        }
        
        internal static IInspector<TValue> GetPropertyDrawer<TValue>(IEnumerable<Attribute> attributes)
        {
            foreach(var drawerAttribute in attributes)
            {
                if (!(drawerAttribute is PropertyAttribute)) 
                    continue;
                
                var drawer = typeof(IPropertyDrawer<>).MakeGenericType(drawerAttribute.GetType());
                var inspector = GetPropertyDrawer<TValue>(InspectorConstraint.AssignableTo(drawer));
                if (null != inspector)
                {
                    return inspector;
                }
            }

            return null;
        }
        
        internal static IInspector<TValue> GetBestInspectorType<TValue>(IProperty property)
        {
            var inspector = default(IInspector<TValue>);
            foreach(var drawerAttribute in property.GetAttributes<UnityEngine.PropertyAttribute>() ?? Array.Empty<UnityEngine.PropertyAttribute>())
            {
                var drawer = typeof(IPropertyDrawer<>).MakeGenericType(drawerAttribute.GetType());
                inspector = GetPropertyDrawer<TValue>(InspectorConstraint.AssignableTo(drawer));
                if (null != inspector)
                {
                    break;
                }
            }
            return inspector ?? GetRootInspector<TValue>();
        }
        
        internal static IInspector<TValue> GetPropertyDrawer<TValue>(params IInspectorConstraint[] constraints)
        {
            return GetInspector<TValue>(
                InspectorConstraint.Combine(InspectorConstraint.AssignableTo<IPropertyDrawer>(), constraints));
        }

        static IEnumerable<Type> GetInspectorTypes(Type type, params IInspectorConstraint[] constraints)
        {
            if (!s_InspectorsPerType.TryGetValue(type, out var inspectors))
            {
                yield break;
            }

            foreach (var inspector in inspectors)
            {
                var any = false;
                foreach (var r in constraints)
                {
                    if (r.Satisfy(inspector)) 
                        continue;
                    
                    any = true;
                    break;
                }

                if (any)
                {
                    continue;
                }

                yield return inspector;
            }
        }

        static void RegisterCustomInspectors()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom(typeof(IInspector<>)))
            {
                RegisterInspectorType(s_InspectorsPerType, typeof(IInspector<>), type);
            }
        }

        static void RegisterInspectorType(IDictionary<Type, List<Type>> typeMap, Type interfaceType, Type inspectorType)
        {
            var inspectorInterface = inspectorType.GetInterface(interfaceType.FullName);
            if (null == inspectorInterface || inspectorType.IsAbstract || inspectorType.ContainsGenericParameters)
            {
                return;
            }

            var genericArguments = inspectorInterface.GetGenericArguments();
            var componentType = genericArguments[0];

            if (null == inspectorType.GetConstructor(Array.Empty<Type>()))
            {
                Debug.LogError(
                    $"Could not create a custom inspector for type `{inspectorType.Name}`: no default or empty constructor found.");
            }

            if (!typeMap.TryGetValue(componentType, out var list))
            {
                typeMap[componentType] = list = new List<Type>();
            }

            list.Add(inspectorType);
        }
    }
}
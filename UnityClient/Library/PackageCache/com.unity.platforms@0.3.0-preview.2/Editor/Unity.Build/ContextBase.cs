using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    public abstract class ContextBase : IDisposable
    {
        readonly Dictionary<Type, object> m_Values = new Dictionary<Type, object>();
        readonly Dictionary<Type, IBuildComponent> m_Components = new Dictionary<Type, IBuildComponent>();
        internal BuildPipelineBase BuildPipeline { get; }
        internal BuildConfiguration BuildConfiguration { get; }

        /// <summary>
        /// List of all values stored.
        /// </summary>
        public object[] Values => m_Values.Values.ToArray();

        /// <summary>
        /// The build configuration name.
        /// </summary>
        /// <returns>The build configuration name.</returns>
        public string BuildConfigurationName => BuildConfiguration.name;

        /// <summary>
        /// The build configuration asset path.
        /// </summary>
        /// <returns>The build configuration asset path.</returns>
        public string BuildConfigurationAssetPath => AssetDatabase.GetAssetPath(BuildConfiguration);

        /// <summary>
        /// The build configuration asset GUID.
        /// </summary>
        /// <returns>The build configuration asset GUID.</returns>
        public string BuildConfigurationAssetGUID => AssetDatabase.AssetPathToGUID(BuildConfigurationAssetPath);

        /// <summary>
        /// Determine if the value of type <typeparamref name="T"/> exists.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns><see langword="true"/> if value is found, <see langword="false"/> otherwise.</returns>
        public bool HasValue<T>() where T : class => m_Values.Keys.Any(type => typeof(T).IsAssignableFrom(type));

        /// <summary>
        /// Get value of type <typeparamref name="T"/> if found, otherwise <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value of type <typeparamref name="T"/> if found, otherwise <see langword="null"/>.</returns>
        public T GetValue<T>() where T : class => m_Values.FirstOrDefault(pair => typeof(T).IsAssignableFrom(pair.Key)).Value as T;

        /// <summary>
        /// Get value of type <typeparamref name="T"/> if found.
        /// Otherwise a new instance of type <typeparamref name="T"/> constructed using <see cref="TypeConstruction"/> utility and then set on this build context.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value or new instance of type <typeparamref name="T"/>.</returns>
        public T GetOrCreateValue<T>() where T : class
        {
            var value = GetValue<T>();
            if (value == null)
            {
                value = TypeConstruction.Construct<T>();
                SetValue(value);
            }
            return value;
        }

        /// <summary>
        /// Get value of type <typeparamref name="T"/> if found.
        /// Otherwise a new instance of type <typeparamref name="T"/> constructed using <see cref="TypeConstruction"/> utility.
        /// The build context is not modified.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value or new instance of type <typeparamref name="T"/>.</returns>
        public T GetValueOrDefault<T>() where T : class => GetValue<T>() ?? TypeConstruction.Construct<T>();

        /// <summary>
        /// Set value of type <typeparamref name="T"/> to this build context.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value to set.</param>
        public void SetValue<T>(T value) where T : class
        {
            if (value == null)
            {
                return;
            }

            var type = value.GetType();
            if (type == typeof(object))
            {
                return;
            }

            m_Values[type] = value;
        }

        /// <summary>
        /// Set value of type <typeparamref name="T"/> to this build context to its default using <see cref="TypeConstruction"/> utility.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        public void SetValue<T>() where T : class => SetValue(TypeConstruction.Construct<T>());

        /// <summary>
        /// Remove value of type <typeparamref name="T"/> from this build context.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value was removed, otherwise <see langword="false"/>.</returns>
        public bool RemoveValue<T>() where T : class => m_Values.Remove(typeof(T));

        /// <summary>
        /// Determine if a component type is stored in this container or its dependencies.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        /// <returns><see langword="true"/> if the is found, otherwise <see langword="false"/>.</returns>
        public bool HasComponent(Type type)
        {
            CheckUsedComponentTypesAndThrowIfMissing(type);
            BuildConfiguration.CheckComponentTypeAndThrowIfInvalid(type);
            return m_Components.ContainsKey(type) || BuildConfiguration.HasComponent(type);
        }

        /// <summary>
        /// Determine if a component type is stored in this container or its dependencies.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the is found, otherwise <see langword="false"/>.</returns>
        public bool HasComponent<T>() where T : IBuildComponent => HasComponent(typeof(T));

        /// <summary>
        /// Determine if a component type is inherited from a dependency.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns><see langword="true"/> if the component is inherited from dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentInherited(Type type)
        {
            CheckUsedComponentTypesAndThrowIfMissing(type);
            return BuildConfiguration.IsComponentInherited(type);
        }

        /// <summary>
        /// Determine if a component type is inherited from a dependency.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the component is inherited from dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentInherited<T>() where T : IBuildComponent => IsComponentInherited(typeof(T));

        /// <summary>
        /// Determine if a component type overrides a dependency.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns><see langword="true"/> if the component overrides a dependency, <see langword="false"/> otherwise</returns>
        public bool IsComponentOverridden(Type type)
        {
            CheckUsedComponentTypesAndThrowIfMissing(type);
            return BuildConfiguration.IsComponentOverridden(type);
        }

        /// <summary>
        /// Determine if a component type overrides a dependency.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the component overrides a dependency, <see langword="false"/> otherwise</returns>
        public bool IsComponentOverridden<T>() where T : IBuildComponent => IsComponentOverridden(typeof(T));

        /// <summary>
        /// Try to get the value of a component type.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <param name="value">The component value.</param>
        /// <returns><see langword="true"/> if the component is found, otherwise <see langword="false"/>.</returns>
        public bool TryGetComponent(Type type, out IBuildComponent value)
        {
            CheckUsedComponentTypesAndThrowIfMissing(type);

            BuildConfiguration.CheckComponentTypeAndThrowIfInvalid(type);
            if (m_Components.TryGetValue(type, out value))
            {
                return true;
            }

            return BuildConfiguration.TryGetComponent(type, out value);
        }

        /// <summary>
        /// Try to get the value of a component type.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="value">The component value.</param>
        /// <returns><see langword="true"/> if the component is found, otherwise <see langword="false"/>.</returns>
        public bool TryGetComponent<T>(out T value) where T : IBuildComponent
        {
            if (TryGetComponent(typeof(T), out var result))
            {
                value = (T)result;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Get the value of a component type if found.
        /// Otherwise an instance created using <see cref="TypeConstruction"/> utility.
        /// The container is not modified.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns>The component value.</returns>
        public IBuildComponent GetComponentOrDefault(Type type)
        {
            BuildConfiguration.CheckComponentTypeAndThrowIfInvalid(type);
            if (m_Components.TryGetValue(type, out var value))
            {
                return value;
            }

            CheckUsedComponentTypesAndThrowIfMissing(type);
            return BuildConfiguration.GetComponentOrDefault(type);
        }

        /// <summary>
        /// Get the value of a component type if found.
        /// Otherwise an instance created using <see cref="TypeConstruction"/> utility.
        /// The container is not modified.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>The component value.</returns>
        public T GetComponentOrDefault<T>() where T : IBuildComponent => (T)GetComponentOrDefault(typeof(T));

        /// <summary>
        /// Get a flatten list of all components recursively from this container and its dependencies.
        /// Throws if a component type is not in UsedComponents list.
        /// </summary>
        /// <returns>The list of components.</returns>
        public IEnumerable<IBuildComponent> GetComponents()
        {
            var lookup = new Dictionary<Type, IBuildComponent>();
            var components = BuildConfiguration.GetComponents();
            foreach (var component in components)
            {
                var componentType = component.GetType();
                CheckUsedComponentTypesAndThrowIfMissing(componentType);
                lookup[componentType] = component;
            }

            foreach (var pair in m_Components)
            {
                lookup[pair.Key] = pair.Value;
            }

            return lookup.Values;
        }

        /// <summary>
        /// Get a flatten list of all components recursively from this container and its dependencies, that matches <see cref="Type"/>.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">Type of the components.</param>
        /// <returns>The list of components.</returns>
        public IEnumerable<IBuildComponent> GetComponents(Type type)
        {
            CheckUsedComponentTypesAndThrowIfMissing(type);

            var lookup = new Dictionary<Type, IBuildComponent>();
            var components = BuildConfiguration.GetComponents(type);
            foreach (var component in components)
            {
                lookup[component.GetType()] = component;
            }

            foreach (var pair in m_Components)
            {
                if (type.IsAssignableFrom(pair.Key))
                {
                    lookup[pair.Key] = pair.Value;
                }
            }

            return lookup.Values;
        }

        /// <summary>
        /// Get a flatten list of all components recursively from this container and its dependencies, that matches <typeparamref name="T"/>.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">Type of the components.</typeparam>
        /// <returns>The list of components.</returns>
        public IEnumerable<T> GetComponents<T>() where T : IBuildComponent => GetComponents(typeof(T)).Cast<T>();

        /// <summary>
        /// Get a flatten list of all component types from this container and its dependencies.
        /// Throws if a component type is not in UsedComponents list.
        /// </summary>
        /// <returns>List of component types.</returns>
        public IEnumerable<Type> GetComponentTypes()
        {
            var types = BuildConfiguration.GetComponentTypes();
            foreach (var type in types)
            {
                CheckUsedComponentTypesAndThrowIfMissing(type);
            }
            return types.Concat(m_Components.Keys).Distinct();
        }

        /// <summary>
        /// Set the value of a component type on this context.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        /// <param name="value">Value of the component to set.</param>
        public void SetComponent(Type type, IBuildComponent value)
        {
            BuildConfiguration.CheckComponentTypeAndThrowIfInvalid(type);
            if (type.IsInterface || type.IsAbstract)
            {
                throw new InvalidOperationException($"{nameof(type)} cannot be interface or abstract.");
            }
            m_Components[type] = value;
        }

        /// <summary>
        /// Set the value of a component type on this context.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <param name="value">Value of the component to set.</param>
        /// <typeparam name="T">Type of the component.</typeparam>
        public void SetComponent<T>(T value) where T : IBuildComponent => SetComponent(typeof(T), value);

        /// <summary>
        /// Set the value of a component type on this context using an instance created using <see cref="TypeConstruction"/> utility.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <param name="type">Type of the component.</param>
        public void SetComponent(Type type) => SetComponent(type, TypeConstruction.Construct<IBuildComponent>(type));

        /// <summary>
        /// Set the value of a component type on this context using an instance created using <see cref="TypeConstruction"/> utility.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        public void SetComponent<T>() where T : IBuildComponent => SetComponent(typeof(T));

        /// <summary>
        /// Remove a component type from this context.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        public bool RemoveComponent(Type type)
        {
            BuildConfiguration.CheckComponentTypeAndThrowIfInvalid(type);
            return m_Components.RemoveAll((key, value) => type.IsAssignableFrom(key)) > 0;
        }

        /// <summary>
        /// Remove all <typeparamref name="T"/> components from this container.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        public bool RemoveComponent<T>() where T : IBuildComponent => RemoveComponent(typeof(T));

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <see cref="Type"/>.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <param name="type">The type of the build artifact.</param>
        /// <returns>The build artifact if found, otherwise <see langword="null"/>.</returns>
        public IBuildArtifact GetLastBuildArtifact(Type type) => BuildConfiguration.GetLastBuildArtifact(type);

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the build artifact.</typeparam>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build artifact if found, otherwise <see langword="null"/>.</returns>
        public T GetLastBuildArtifact<T>() where T : class, IBuildArtifact => BuildConfiguration.GetLastBuildArtifact<T>();

        /// <summary>
        /// Get the last build result for this build configuration.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build result if found, otherwise <see langword="null"/>.</returns>
        public BuildResult GetLastBuildResult() => BuildConfiguration.GetLastBuildResult();

        /// <summary>
        /// Provides a mechanism for releasing unmanaged resources.
        /// </summary>
        public virtual void Dispose() { }

        internal ContextBase() { }

        internal ContextBase(BuildPipelineBase pipeline, BuildConfiguration config)
        {
            BuildPipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            BuildConfiguration = config ?? throw new ArgumentNullException(nameof(config));

            // Prevent the build configuration asset from being destroyed during a build
            BuildConfiguration.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
        }

        void CheckUsedComponentTypesAndThrowIfMissing(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!BuildPipeline.UsedComponents.Contains(type))
            {
                throw new InvalidOperationException($"Type '{type.Name}' is missing in build pipeline '{BuildPipeline.GetType().Name}' {nameof(BuildPipeline.UsedComponents)} list.");
            }
        }
    }
}

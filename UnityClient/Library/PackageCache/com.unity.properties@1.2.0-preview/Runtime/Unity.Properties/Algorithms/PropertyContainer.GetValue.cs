using System;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    public static partial class PropertyContainer
    {
        class GetValueVisitor<TSrcValue> : PathVisitor
        {
            public static readonly Pool<GetValueVisitor<TSrcValue>> Pool = new Pool<GetValueVisitor<TSrcValue>>(() => new GetValueVisitor<TSrcValue>(), v => v.Reset());
            public TSrcValue Value;

            public override void Reset()
            {
                base.Reset();
                Value = default;
            }

            protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property,
                ref TContainer container, ref TValue value)
            {
                if (!TypeConversion.TryConvert(value, out Value))
                {
                    ErrorCode = VisitErrorCode.InvalidCast;
                }
            }
        }

        /// <summary>
        /// Gets the value of a property by name.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <param name="name">The name of the property to get.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>The value for the specified name.</returns>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidCastException">The specified <typeparamref name="TValue"/> could not be assigned to the property.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="name"/> was not found or could not be resolved.</exception>
        public static TValue GetValue<TValue>(ref object container, string name)
            => GetValue<object, TValue>(ref container, name);

        /// <summary>
        /// Gets the value of a property by name.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <param name="name">The name of the property to get.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>The value for the specified name.</returns>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidCastException">The specified <typeparamref name="TValue"/> could not be assigned to the property.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="name"/> was not found or could not be resolved.</exception>
        public static TValue GetValue<TContainer, TValue>(ref TContainer container, string name)
        {
            var path = PropertyPath.Pool.Get();
            try
            {
                path.AppendPath(name);
                return GetValue<TContainer, TValue>(ref container, path);
            }
            finally
            {
                PropertyPath.Pool.Release(path);
            }
        }

        /// <summary>
        /// Gets the value of a property by path.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <param name="path">The path of the property to get.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>The value at the specified path.</returns>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidCastException">The specified <typeparamref name="TValue"/> could not be assigned to the property.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="path"/> was not found or could not be resolved.</exception>
        public static TValue GetValue<TValue>(ref object container, PropertyPath path) 
            => GetValue<object, TValue>(ref container, path);
        
        /// <summary>
        /// Gets the value of a property by path.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <param name="path">The path of the property to get.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>The value at the specified path.</returns>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidCastException">The specified <typeparamref name="TValue"/> could not be assigned to the property.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="path"/> was not found or could not be resolved.</exception>
        public static TValue GetValue<TContainer, TValue>(ref TContainer container, PropertyPath path)
        {
            if (null == path)
                throw new ArgumentNullException(nameof(path));

            if (path.PartsCount <= 0)
                throw new InvalidPathException("The specified PropertyPath is empty.");

            if (GetValue(ref container, path, out TValue value, out var errorCode))
                return value;

            switch (errorCode)
            {
                case VisitErrorCode.NullContainer:
                    throw new ArgumentNullException(nameof(container));
                case VisitErrorCode.InvalidContainerType:
                    throw new InvalidContainerTypeException(container.GetType());
                case VisitErrorCode.MissingPropertyBag:
                    throw new MissingPropertyBagException(container.GetType());
                case VisitErrorCode.InvalidCast:
                    throw new InvalidCastException($"Failed to GetValue of Type=[{typeof(TValue).Name}] for property with path=[{path}]");
                case VisitErrorCode.InvalidPath:
                    throw new InvalidPathException($"Failed to GetValue for property with Path=[{path}]");
                default:
                    throw new Exception($"Unexpected {nameof(VisitErrorCode)}=[{errorCode}]");
            }
        }

        /// <summary>
        /// Gets the value of a property by name.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <param name="name">The name of the property to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified name, if the property is found. otherwise the default value for the <typeparamref name="TValue"/>.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value exists for the specified name; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetValue<TValue>(ref object container, string name, out TValue value)
            => TryGetValue<object, TValue>(ref container, name, out value);

        /// <summary>
        /// Gets the value of a property by name.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <param name="name">The name of the property to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified name, if the property is found. otherwise the default value for the <typeparamref name="TValue"/>.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value exists for the specified name; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetValue<TContainer, TValue>(ref TContainer container, string name, out TValue value)
        {
            var path = PropertyPath.Pool.Get();
            try
            {
                path.AppendPath(name);
                return GetValue(ref container, path, out value, out _);
            }
            finally
            {
                PropertyPath.Pool.Release(path);
            }
        }
        
        /// <summary>
        /// Gets the value of a property by path.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <param name="path">The path of the property to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified path, if the property is found. otherwise the default value for the <typeparamref name="TValue"/>.</param>
        /// <returns>The property value of the given container.</returns>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <returns><see langword="true"/> if the value exists at the specified path; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetValue<TValue>(ref object container, PropertyPath path, out TValue value)
            => GetValue(ref container, path, out value, out _);

        /// <summary>
        /// Gets the value of a property by path.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <param name="path">The path of the property to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified path, if the property is found. otherwise the default value for the <typeparamref name="TValue"/>.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value exists at the specified path; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetValue<TContainer, TValue>(ref TContainer container, PropertyPath path, out TValue value)
            => GetValue(ref container, path, out value, out _);

        /// <summary>
        /// Gets the value of a property by path.
        /// </summary>
        /// <param name="container">The container whose property value will be returned.</param>
        /// <param name="path">The path of the property to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified path, if the property is found. otherwise the default value for the <typeparamref name="TValue"/>.</param>
        /// <param name="errorCode">When this method returns, contains the error code.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value exists at the specified path; otherwise, <see langword="false"/>.</returns>
        static bool GetValue<TContainer, TValue>(ref TContainer container, PropertyPath path, out TValue value, out VisitErrorCode errorCode)
        {
            if (null == path || path.PartsCount <= 0)
            {
                errorCode = VisitErrorCode.InvalidPath;
                value = default;
                return false;
            }

            var visitor = GetValueVisitor<TValue>.Pool.Get();
            visitor.Path = path;

            try
            {
                if (!Visit(ref container, visitor, out errorCode))
                {
                    value = default;
                    return false;
                }
                
                value = visitor.Value;
                errorCode = visitor.ErrorCode;
            }
            finally
            {
                GetValueVisitor<TValue>.Pool.Release(visitor);
            }

            return errorCode == VisitErrorCode.Ok;
        }
    }
}
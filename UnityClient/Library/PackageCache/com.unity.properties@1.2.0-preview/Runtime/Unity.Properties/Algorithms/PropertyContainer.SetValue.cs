using System;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    public static partial class PropertyContainer
    {
        class SetValueVisitor<TSrcValue> : PathVisitor
        {
            public static readonly Pool<SetValueVisitor<TSrcValue>> Pool = new Pool<SetValueVisitor<TSrcValue>>(() => new SetValueVisitor<TSrcValue>(), v => v.Reset()); 
            public TSrcValue Value;

            public override void Reset()
            {
                base.Reset();
                Value = default;
            }

            protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property,
                ref TContainer container, ref TValue value)
            {
                if (TypeConversion.TryConvert(Value, out TValue v))
                {
                    property.SetValue(ref container, v);
                }
                else
                {
                    ErrorCode = VisitErrorCode.InvalidCast;
                }
            }
        }

        /// <summary>
        /// Sets the value of a property by name to the given value.
        /// </summary>
        /// <remarks>
        /// This method is NOT thread safe.
        /// </remarks>
        /// <param name="container">The container whose property will be set.</param>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidCastException">The specified <typeparamref name="TValue"/> could not be assigned to the property.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="name"/> was not found or could not be resolved.</exception>
        public static void SetValue<TValue>(ref object container, string name, TValue value)
            => SetValue<object, TValue>(ref container, name, value);

        /// <summary>
        /// Sets the value of a property by name to the given value.
        /// </summary>
        /// <remarks>
        /// This method is NOT thread safe.
        /// </remarks>
        /// <param name="container">The container whose property will be set.</param>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <typeparam name="TContainer">The container type to set the value on.</typeparam>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidCastException">The specified <typeparamref name="TValue"/> could not be assigned to the property.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="name"/> was not found or could not be resolved.</exception>
        public static void SetValue<TContainer, TValue>(ref TContainer container, string name, TValue value)
        {
            var path = PropertyPath.Pool.Get();
            try
            {
                path.AppendPath(name);
                SetValue(ref container, path, value);
            }
            finally
            {
                PropertyPath.Pool.Release(path);
            }
        }

        /// <summary>
        /// Sets the value of a property at the given path to the given value.
        /// </summary>
        /// <remarks>
        /// This method is NOT thread safe.
        /// </remarks>
        /// <param name="container">The container whose property will be set.</param>
        /// <param name="path">The path of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidCastException">The specified <typeparamref name="TValue"/> could not be assigned to the property.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="path"/> was not found or could not be resolved.</exception>
        public static void SetValue<TValue>(ref object container, PropertyPath path, TValue value)
            => SetValue<object, TValue>(ref container, path, value);

        /// <summary>
        /// Sets the value of a property at the given path to the given value.
        /// </summary>
        /// <remarks>
        /// This method is NOT thread safe.
        /// </remarks>
        /// <param name="container">The container whose property will be set.</param>
        /// <param name="path">The path of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <typeparam name="TContainer">The container type to set the value on.</typeparam>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidCastException">The specified <typeparamref name="TValue"/> could not be assigned to the property.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="path"/> was not found or could not be resolved.</exception>
        public static void SetValue<TContainer, TValue>(ref TContainer container, PropertyPath path, TValue value)
        {
            if (null == path)
                throw new ArgumentNullException(nameof(path));
            
            if (path.PartsCount <= 0)
                throw new InvalidPathException("The specified PropertyPath is empty.");

            if (SetValue(ref container, path, value, out var errorCode))
                return;
            
            switch (errorCode)
            {
                case VisitErrorCode.NullContainer:
                    throw new ArgumentNullException(nameof(container));
                case VisitErrorCode.InvalidContainerType:
                    throw new InvalidContainerTypeException(container.GetType());
                case VisitErrorCode.MissingPropertyBag:
                    throw new MissingPropertyBagException(container.GetType());
                case VisitErrorCode.InvalidCast:
                    throw new InvalidCastException($"Failed to SetValue of Type=[{typeof(TValue).Name}] for property with path=[{path}]");
                case VisitErrorCode.InvalidPath:
                    throw new InvalidPathException($"Failed to SetValue for property with Path=[{path}]");
                default:
                    throw new Exception($"Unexpected {nameof(VisitErrorCode)}=[{errorCode}]");
            }
        }
        
        /// <summary>
        /// Tries to set the value of a property at the given path to the given value.
        /// </summary>
        /// <remarks>
        /// This method is NOT thread safe.
        /// </remarks>
        /// <param name="container">The container whose property will be set.</param>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <returns><see langword="true"/> if the value was set correctly; <see langword="false"/> otherwise.</returns>
        public static bool TrySetValue<TValue>(ref object container, string name, TValue value)
            => TrySetValue<object, TValue>(ref container, name, value);
        
        /// <summary>
        /// Tries to set the value of a property at the given path to the given value.
        /// </summary>
        /// <remarks>
        /// This method is NOT thread safe.
        /// </remarks>
        /// <param name="container">The container whose property will be set.</param>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <typeparam name="TContainer">The container type to set the value on.</typeparam>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <returns><see langword="true"/> if the value was set correctly; <see langword="false"/> otherwise.</returns>
        public static bool TrySetValue<TContainer, TValue>(ref TContainer container, string name, TValue value)
        {
            var path = PropertyPath.Pool.Get();
            try
            {
                path.AppendPath(name);
                return TrySetValue(ref container, path, value);
            }
            finally
            {
                PropertyPath.Pool.Release(path);
            }
        }

        /// <summary>
        /// Tries to set the value of a property at the given path to the given value.
        /// </summary>
        /// <remarks>
        /// This method is NOT thread safe.
        /// </remarks>
        /// <param name="container">The container whose property will be set.</param>
        /// <param name="path">The path of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <returns><see langword="true"/> if the value was set correctly; <see langword="false"/> otherwise.</returns>
        public static bool TrySetValue<TValue>(ref object container, PropertyPath path, TValue value)
            => TrySetValue<object, TValue>(ref container, path, value);
        
        /// <summary>
        /// Tries to set the value of a property at the given path to the given value.
        /// </summary>
        /// <remarks>
        /// This method is NOT thread safe.
        /// </remarks>
        /// <param name="container">The container whose property will be set.</param>
        /// <param name="path">The path of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <typeparam name="TContainer">The container type to set the value on.</typeparam>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <returns><see langword="true"/> if the value was set correctly; <see langword="false"/> otherwise.</returns>
        public static bool TrySetValue<TContainer, TValue>(ref TContainer container, PropertyPath path, TValue value)
            => SetValue(ref container, path, value, out _);

        /// <summary>
        /// Tries to set the value of a property at the given path to the given value.
        /// </summary>
        /// <remarks>
        /// This method is NOT thread safe.
        /// </remarks>
        /// <param name="container">The container whose property will be set.</param>
        /// <param name="path">The path of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <param name="errorCode">When this method returns, contains the error code.</param>
        /// <typeparam name="TContainer">The container type to set the value on.</typeparam>
        /// <typeparam name="TValue">The value type to set.</typeparam>
        /// <returns><see langword="true"/> if the value was set correctly; <see langword="false"/> otherwise.</returns>
        static bool SetValue<TContainer, TValue>(ref TContainer container, PropertyPath path, TValue value, out VisitErrorCode errorCode)
        {
            if (null == path)
            {
                errorCode = VisitErrorCode.InvalidPath;
                return false;
            }

            if (path.PartsCount <= 0)
            {
                errorCode = VisitErrorCode.InvalidPath;
                return false;
            }

            var visitor = SetValueVisitor<TValue>.Pool.Get();
            visitor.Path = path;
            visitor.Value = value;
            try
            {
                if (!Visit(ref container, visitor, out errorCode))
                {
                    return false;
                }
                
                errorCode = visitor.ErrorCode;
            }
            finally
            {
                SetValueVisitor<TValue>.Pool.Release(visitor);
            }

            return errorCode == VisitErrorCode.Ok;
        }
    }
}
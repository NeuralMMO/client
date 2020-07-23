using System;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    public static partial class PropertyContainer
    {
        class GetPropertyVisitor : PathVisitor
        {
            public static readonly Pool<GetPropertyVisitor> Pool = new Pool<GetPropertyVisitor>(() => new GetPropertyVisitor(), v => v.Reset());

            public IProperty Property;

            public override void Reset()
            {
                base.Reset();
                Property = default;
            }

            protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property,
                ref TContainer container, ref TValue value)
            {
                Property = property;
            }
        }

        /// <summary>
        /// Gets an <see cref="IProperty"/> on the specified container for the given <see cref="PropertyPath"/>.
        /// </summary>
        /// <remarks>
        /// While the container data is not actually read from or written to. The container itself is needed to resolve polymorphic fields and list elements.
        /// </remarks>
        /// <param name="container">The container tree to search.</param>
        /// <param name="path">The property path to resolve.</param>
        /// <returns>The <see cref="IProperty"/> for the given path.</returns>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="path"/> was not found or could not be resolved.</exception>
        public static IProperty GetProperty(ref object container, PropertyPath path)
            => GetProperty<object>(ref container, path);

        /// <summary>
        /// Gets an <see cref="IProperty"/> on the specified container for the given <see cref="PropertyPath"/>.
        /// </summary>
        /// <remarks>
        /// While the container data is not actually read from or written to. The container itself is needed to resolve polymorphic fields and list elements.
        /// </remarks>
        /// <param name="container">The container whose property will be returned.</param>
        /// <param name="path">The property path to resolve.</param>
        /// <typeparam name="TContainer">The strongly typed container.</typeparam>
        /// <returns>The <see cref="IProperty"/> for the given path.</returns>
        /// <exception cref="ArgumentNullException">The specified container or path is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The specified container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">The specified container type has no property bag associated with it.</exception>
        /// <exception cref="InvalidPathException">The specified <paramref name="path"/> was not found or could not be resolved.</exception>
        public static IProperty GetProperty<TContainer>(ref TContainer container, PropertyPath path)
        {
            if (GetProperty(ref container, path, out var property, out var errorCode))
            {
                return property;
            }
            
            switch (errorCode)
            {
                case VisitErrorCode.NullContainer:
                    throw new ArgumentNullException(nameof(container));
                case VisitErrorCode.InvalidContainerType:
                    throw new InvalidContainerTypeException(container.GetType());
                case VisitErrorCode.MissingPropertyBag:
                    throw new MissingPropertyBagException(container.GetType());
                case VisitErrorCode.InvalidPath:
                    throw new ArgumentException($"Failed to get property for path=[{path}]");
                default:
                    throw new Exception($"Unexpected {nameof(VisitErrorCode)}=[{errorCode}]");
            }
        }

        /// <summary>
        /// Gets an <see cref="IProperty"/> on the specified container for the given <see cref="PropertyPath"/>.
        /// </summary>
        /// <remarks>
        /// While the container data is not actually read from or written to. The container itself is needed to resolve polymorphic fields and list elements.
        /// </remarks>
        /// <param name="container">The container tree to search.</param>
        /// <param name="path">The property path to resolve.</param>
        /// <param name="property">When this method returns, contains the property associated with the specified path, if the property is found; otherwise, null.</param>
        /// <returns><see langword="true"/> if the property was found at the specified path; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetProperty(ref object container, PropertyPath path, out IProperty property)
            => GetProperty(ref container, path, out property, out _);
        
        /// <summary>
        /// Gets an <see cref="IProperty"/> on the specified container for the given <see cref="PropertyPath"/>.
        /// </summary>
        /// <remarks>
        /// While the container data is not actually read from or written to. The container itself is needed to resolve polymorphic fields and list elements.
        /// </remarks>
        /// <param name="container">The container tree to search.</param>
        /// <param name="path">The property path to resolve.</param>
        /// <param name="property">When this method returns, contains the property associated with the specified path, if the property is found; otherwise, null.</param>
        /// <typeparam name="TContainer">The strongly typed container.</typeparam>
        /// <returns><see langword="true"/> if the property was found at the specified path; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetProperty<TContainer>(ref TContainer container, PropertyPath path, out IProperty property)
            => GetProperty(ref container, path, out property, out _);

        /// <summary>
        /// Gets an <see cref="IProperty"/> on the specified container for the given <see cref="PropertyPath"/>.
        /// </summary>
        /// <remarks>
        /// While the container data is not actually read from or written to. The container itself is needed to resolve polymorphic fields and list elements.
        /// </remarks>
        /// <param name="container">The container tree to search.</param>
        /// <param name="path">The property path to resolve.</param>
        /// <param name="property">When this method returns, contains the property associated with the specified path, if the property is found; otherwise, null.</param>
        /// <param name="errorCode">When this method returns, contains the error code.</param>
        /// <typeparam name="TContainer">The strongly typed container.</typeparam>
        /// <returns><see langword="true"/> if the property was found at the specified path; otherwise, <see langword="false"/>.</returns>
        static bool GetProperty<TContainer>(ref TContainer container, PropertyPath path, out IProperty property, out VisitErrorCode errorCode)
        {
            var getPropertyVisitor = GetPropertyVisitor.Pool.Get();
            try
            {
                getPropertyVisitor.Path = path;
                if (!Visit(ref container, getPropertyVisitor, out errorCode))
                {
                    property = default;
                    return false;
                }
                errorCode = getPropertyVisitor.ErrorCode;
                property = getPropertyVisitor.Property;
                return errorCode == VisitErrorCode.Ok;
            }
            finally
            {
                GetPropertyVisitor.Pool.Release(getPropertyVisitor);
            }
        }
    }
}
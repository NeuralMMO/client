using System;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    public static partial class PropertyContainer
    {
        class ValueAtPathVisitor : PathVisitor
        {
            public static readonly Pool<ValueAtPathVisitor> Pool = new Pool<ValueAtPathVisitor>(() => new ValueAtPathVisitor(), v => v.Reset()); 
            public IPropertyVisitor Visitor;

            public override void Reset()
            {
                base.Reset();
                Visitor = default;
            }

            protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property,
                ref TContainer container, ref TValue value)
            {
                ((IPropertyAccept<TContainer>) property).Accept(Visitor, ref container);
            }
        }
        
        class ExistsAtPathVisitor : PathVisitor
        {
            public static readonly Pool<ExistsAtPathVisitor> Pool = new Pool<ExistsAtPathVisitor>(() => new ExistsAtPathVisitor(), v => v.Reset()); 
            public bool Exists;

            public override void Reset()
            {
                base.Reset();
                Exists = default;
            }

            protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
            {
                Exists = true;
            }
        }
        
        /// <summary>
        /// Returns <see langword="true"/> if a property exists at the specified <see cref="PropertyPath"/>.
        /// </summary>
        /// <param name="container">The container tree to search.</param>
        /// <param name="path">The property path to resolve.</param>
        /// <returns><see langword="true"/> if a property can be found at path.</returns>
        public static bool IsPathValid(ref object container, PropertyPath path)
            => IsPathValid<object>(ref container, path);

        /// <summary>
        /// Returns <see langword="true"/> if a property exists at the specified <see cref="PropertyPath"/>.
        /// </summary>
        /// <param name="container">The container tree to search.</param>
        /// <param name="path">The property path to resolve.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <returns><see langword="true"/> if a property can be found at path.</returns>
        public static bool IsPathValid<TContainer>(ref TContainer container, PropertyPath path)
        {
            var visitor = ExistsAtPathVisitor.Pool.Get();
            try
            {
                visitor.Path = path;
                Visit(ref container, visitor);
                return visitor.Exists;
            }
            finally
            {
                ExistsAtPathVisitor.Pool.Release(visitor);
            }
        }

        /// <summary>
        /// Visit the specified <paramref name="container"/> using the specified <paramref name="visitor"/> at the given <see cref="PropertyPath"/>.
        /// </summary>
        /// <param name="container">The container to visit.</param>
        /// <param name="visitor">The visitor.</param>
        /// <param name="path">The property path to visit.</param>
        /// <param name="parameters">The visit parameters to use.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <exception cref="ArgumentNullException">The container is null.</exception>
        /// <exception cref="InvalidContainerTypeException">The given container type is not valid for visitation.</exception>
        /// <exception cref="MissingPropertyBagException">No property bag was found for the given container.</exception>
        public static void Visit<TContainer>(ref TContainer container, PropertyVisitor visitor, PropertyPath path, VisitParameters parameters = default)
        {
            var visitAtPath = ValueAtPathVisitor.Pool.Get();
            try
            {
                visitAtPath.Path = path;
                visitAtPath.Visitor = visitor;
                
                Visit(ref container, visitAtPath, parameters);

                if ((parameters.IgnoreExceptions & VisitExceptionType.Internal) != 0)
                {
                    switch (visitAtPath.ErrorCode)
                    {
                        case VisitErrorCode.Ok:
                            break;
                        case VisitErrorCode.InvalidPath:
                            throw new InvalidPathException($"Failed to Visit at Path=[{path}]");
                        default:
                            throw new Exception($"Unexpected {nameof(VisitErrorCode)}=[{visitAtPath.ErrorCode}]");
                    }
                }
            }
            finally
            {
                ValueAtPathVisitor.Pool.Release(visitAtPath);
            }
        }
    }
}
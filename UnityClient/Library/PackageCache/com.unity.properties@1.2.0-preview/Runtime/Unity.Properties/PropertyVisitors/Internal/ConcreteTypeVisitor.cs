namespace Unity.Properties.Internal
{
    /// <summary>
    /// Base class to implement a visitor responsible for getting an objects concrete type as a generic.
    /// </summary>
    abstract class ConcreteTypeVisitor : IPropertyBagVisitor
    {
        /// <summary>
        /// Implement this method to receive the strongly typed callback for a given container.
        /// </summary>
        /// <param name="container">The reference to the container.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        protected abstract void VisitContainer<TContainer>(ref TContainer container);
            
        void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
            => VisitContainer(ref container);


        internal static class AOT
        {
            internal static void RegisterType<TContainer>(TContainer container = default)
            {
                ((ConcreteTypeVisitor) default).VisitContainer(ref container);
            }
        }
    }
}
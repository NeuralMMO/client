using Unity.Properties.Internal;

namespace Unity.Properties.UI.Internal
{
    class CustomInspectorVisitor<TDeclaredValueType> : ConcreteTypeVisitor
    {
        public IInspector Inspector { get; private set; }
        public PropertyElement Root { get; set; }
        public PropertyPath PropertyPath { get; set; }
        public IProperty Property { get; set; }

        protected override void VisitContainer<TValue>(ref TValue value)
        {
            Inspector = GetInspector<TValue>() ?? GetInspector<TDeclaredValueType>();
        }

        IInspector GetInspector<T>()
        {
            var inspector = CustomInspectorDatabase.GetBestInspectorType<T>(Property);
            if (null != inspector)
            {
                inspector.Context = new InspectorContext<T>(
                    Root,
                    PropertyPath,
                    Property
                );
            }

            return inspector;
        }
    }
}
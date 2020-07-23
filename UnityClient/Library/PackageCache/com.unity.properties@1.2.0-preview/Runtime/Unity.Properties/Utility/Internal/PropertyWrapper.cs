namespace Unity.Properties.Internal
{
    interface IPropertyWrapper
    {
    }

    struct PropertyWrapper<T> : IPropertyWrapper
    {
        class PropertyBag : ContainerPropertyBag<PropertyWrapper<T>>, IPropertyWrapper
        {
            public PropertyBag()
            {
                AddProperty(new Property());
            }
        }
        
        class Property : Property<PropertyWrapper<T>, T>, IPropertyWrapper
        {
            public override string Name => nameof(Value);
            public override bool IsReadOnly => false;
            public override T GetValue(ref PropertyWrapper<T> container) => container.Value;
            public override void SetValue(ref PropertyWrapper<T> container, T value) => container.Value = value;
        }

        public T Value;

        public PropertyWrapper(T value)
        {
            if (!PropertyBagStore.Exists<PropertyWrapper<T>>())
            {
                PropertyBagStore.AddPropertyBag(new PropertyBag());
            }
            
            Value = value;
        }
    }
}
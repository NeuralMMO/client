#if !NET_DOTS
using System;

namespace Unity.Properties.Reflection.Internal
{
    class ReflectedPropertyBag<TContainer> : ContainerPropertyBag<TContainer>
    {
        internal new void AddProperty<TValue>(Property<TContainer, TValue> property)
        {
            var container = default(TContainer);
            
            if (TryGetProperty(ref container, property.Name, out var existing))
            {
                if (existing.DeclaredValueType() == typeof(TValue))
                {
                    // Property with the same name and value type, it's safe to ignore.
                    return;
                }
                
                throw new InvalidOperationException($"A property with name '{property.Name}' already exist in property bag for type '{typeof(TContainer).FullName}'. This can be caused by class inheritance.");
            }
            
            base.AddProperty(property);
        }
    }
}
#endif
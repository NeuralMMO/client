# Properties

The `com.unity.properties` package offers a generic API to visit .Net objects at runtime. This API is found in the `Unity.Properties` assembly (referenced automatically for convenience), under the `Unity.Properties` namespace.

One can build various functionalities on top of the visitor pattern, including serialization, data migration, deep data comparisons, etc.

This manual targets developers either creating data types compatible with the _Properties API_, or creating new property visitors and adapters for domain-specific use cases.

# Glossary

**Properties** are objects implementing the `Unity.Properties.IProperty` interface. In their most primitive form, they have a name, a value type, and _attributes_. Properties are automatically created from _public field members_, or on members with the `Unity.Properties.CreatePropertyAttribute`.

**Property Attributes** are standard .Net `System.Attribute` associated with a given property.

**Property Bags** are collections of properties for a given .Net object type. Property bags are created lazily through reflection (in the Editor), or as a build post-processing step from types instrumented with the `Unity.Properties.GeneratePropertyBagAttribute` (in Player builds).

**Property Containers** are any .Net objects whose `System.Type` is associated with a property bag.

**Property Paths** are constructed from strings, and can be used to resolve a specific property instance from a root object. For example, the path `"foo.bar.baz[12]"` resolves the 13th element of the `baz` list container found in the `bar` container, which is itself found in the `foo` container. You can create and manipulate property paths using the `Unity.Properties.PropertyPath` class.

**Property Visitors** are the algorithms you build on top of the Properties API. Use the `Unity.Properties.PropertyVisitor` base class to create your own visitors.

**Visitor Adapters** can be used to specialize visitors for given container or value types. Adapters all implement the `Unity.Properties.IPropertyVisitorAdapter` interface. Adapters are optional, but highly recommended, and as such they are supported by default when using the `Unity.Properties.PropertyVisitor` base class.

# Getting Started

Here's a short example illustrating a basic usage of the Properties API.

```c#
namespace Unity.Properties.Samples
{
    using System;
    using UnityEngine;
    using UnityEngine.Profiling;
    
    public class PropertyTest : MonoBehaviour
    {
        [SerializeField] MyContainer m_Container = new MyContainer() { X = 42 };
        
        PropertyPath m_PathToX = new PropertyPath(nameof(MyContainer.X));
        MyVisitor m_Visitor = new MyVisitor();
        int m_FrameNumber;

        void Update()
        {
            Profiler.BeginSample("PropertyTest.GetValue");
            var value = PropertyContainer.GetValue<MyContainer, int>(ref m_Container, m_PathToX);
            Profiler.EndSample();

            Profiler.BeginSample("PropertyTest.Visit");
            PropertyContainer.Visit(ref m_Container, m_Visitor);
            Profiler.EndSample();
            
            ++m_FrameNumber;
            var printMessage = m_FrameNumber % 100 == 0;
            
            if (printMessage)
            {
                Debug.LogFormat("PropertyTest.GetValue: X = {0}", value.ToString());
                Debug.LogFormat("PropertyTest.Visit: LastIntValueVisited = {0}", m_Visitor.LastIntValueVisited.ToString());
            }
        }
    }

    [Serializable, GeneratePropertyBag]
    struct MyContainer
    {
        [SerializeField] int m_X;

        [CreateProperty]
        public int X
        {
            get => m_X;
            set => m_X = value;
        }
    }

    class MyVisitor : PropertyVisitor
    {
        IntAdapter m_IntAdapter = new IntAdapter();

        public int LastIntValueVisited => m_IntAdapter.lastValue;
        
        public MyVisitor()
        {
            AddAdapter(m_IntAdapter);
        }

        class IntAdapter : Adapters.IVisit<int>
        {
            public int lastValue;
            
            public VisitStatus Visit<TContainer>(Property<TContainer, int> property, ref TContainer container, ref int value)
            {
                lastValue = value;
                return VisitStatus.Stop;
            }
        }
    }
}
```

This sample doesn't perform any useful work: it prints the `m_Container.X` value to the Console twice every 100 frames. However, it explains a few things:

1. How to implement a simple visitor using the `PropertyVisitor` base class
2. How to implement and add an adapter to a `PropertyVisitor` instance
3. How to use get explicit property values using `PropertyContainer.GetValue` and `PropertyPath`
4. How to visit property containers using your custom visitor using `PropertyContainer.Visit`
5. How to use the `[CreateProperty]` attribute to expose a .Net property

Calling `PropertyContainer.Visit` is the most common way to use the _Properties API_.

# Performance Considerations

## Property Paths

`Unity.Properties.PropertyPath` is a class, and currently allocates during construction. Avoid creating these objects in hot code paths (like the `MonoBehaviour.Update` method). Instead, create and cache them during initialization routines.

## Reflected Property Bags (Editor)

Property bags and strongly-typed properties are created using .Net reflection in the Editor, which can be quite slow the first time a property bag is requested for a given container type.

To mitigate this issue, you can explicitly visit containers of a given type during initialization routines. This will _pre-warm_ the internal property bag cache.

> Note: this is not ideal. We'll work out a way to make this easier in a future release.

## Reflected Field Properties on .Net Standard (Editor)

While we support properties for field members, these properties will allocate garbage when visited _only on .Net Standard_, and _only when reflection is used to create these properties_. The reason is we use `System.Reflection.FieldInfo` directly for these properties, and boxing is unavoidable.

We're looking at ways to optimize this, but meanwhile you can manually wrap fields in .Net properties with the `[CreateProperty]` attribute on them.

This solution works on all .Net runtimes, AOT platforms, and where reflection may not be available. It also forces you to encapsulate fields, which is not a bad thing...


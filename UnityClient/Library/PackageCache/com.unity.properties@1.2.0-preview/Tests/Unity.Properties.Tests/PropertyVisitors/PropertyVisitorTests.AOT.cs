using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Properties.Tests
{
    partial class PropertyVisitorTests
    {
        /// <summary>
        /// This fixture is scoped to prevent access to the type.
        /// This is to ensure that no API calls are used with the specific generic combination to validate that the AOT generators correctly work.
        /// </summary>
        [TestFixture]
        class AOT
        {
            interface IData
            {
            }

            struct UserDataA : IData
            {
                public int A;

                internal class PropertyBag : ContainerPropertyBag<UserDataA>
                {
                    public PropertyBag()
                    {
                        AddProperty(new DelegateProperty<UserDataA, int>(
                                        name: nameof(A), 
                                        getter: (ref UserDataA c) => c.A, 
                                        setter: (ref UserDataA c, int v) => c.A = v));
                    }
                }
            }

            struct UserDataB : IData
            {
                public int B;

                internal class PropertyBag : ContainerPropertyBag<UserDataB>
                {
                    public PropertyBag()
                    {
                        AddProperty(new DelegateProperty<UserDataB, int>(
                                        name: nameof(B), 
                                        getter: (ref UserDataB c) => c.B, 
                                        setter: (ref UserDataB c, int v) => c.B = v));
                    }
                }
            }

            struct Nested
            {
                public int Int32Value;

                internal class PropertyBag : ContainerPropertyBag<Nested>
                {
                    public PropertyBag()
                    {
                        AddProperty(new DelegateProperty<Nested, int>(
                                        name: nameof(Int32Value), 
                                        getter: (ref Nested c) => c.Int32Value, 
                                        setter: (ref Nested c, int v) => c.Int32Value = v));
                    }
                }
            }

            struct Container
            {
                public int Int32Value;
                public Nested NestedValue;
                public List<int> Int32List;
                public List<Nested> NestedList;
                public List<IData> InterfaceList;
                public List<List<float>> Float32ListList;

                internal class PropertyBag : ContainerPropertyBag<Container>
                {
                    static PropertyBag()
                    {
                        Properties.PropertyBag.RegisterList<Container, List<int>, int>();
                        Properties.PropertyBag.RegisterList<Container, List<Nested>, Nested>();
                        Properties.PropertyBag.RegisterList<Container, List<IData>, IData>();
                        Properties.PropertyBag.RegisterList<Container, List<List<float>>, List<float>>();
                        Properties.PropertyBag.RegisterList<List<List<float>>, List<float>, float>();
                    }
                    
                    public PropertyBag()
                    {
                        AddProperty(new DelegateProperty<Container, int>(
                                        name: nameof(Int32Value), 
                                        getter: (ref Container c) => c.Int32Value, 
                                        setter: (ref Container c, int v) => c.Int32Value = v));
                        
                        AddProperty(new DelegateProperty<Container, Nested>(
                                        name: nameof(NestedValue), 
                                        getter: (ref Container c) => c.NestedValue, 
                                        setter: (ref Container c, Nested v) => c.NestedValue = v));
                        
                        AddProperty(new DelegateProperty<Container, List<int>>(
                                        name: nameof(Int32List), 
                                        getter: (ref Container c) => c.Int32List, 
                                        setter: (ref Container c, List<int> v) => c.Int32List = v));
                        
                        AddProperty(new DelegateProperty<Container, List<Nested>>(
                                        name: nameof(NestedList), 
                                        getter: (ref Container c) => c.NestedList, 
                                        setter: (ref Container c, List<Nested> v) => c.NestedList = v));
                        
                        AddProperty(new DelegateProperty<Container, List<IData>>(
                                        name: nameof(InterfaceList), 
                                        getter: (ref Container c) => c.InterfaceList, 
                                        setter: (ref Container c, List<IData> v) => c.InterfaceList = v));
                        
                        AddProperty(new DelegateProperty<Container, List<List<float>>>(
                                        name: nameof(Float32ListList), 
                                        getter: (ref Container c) => c.Float32ListList, 
                                        setter: (ref Container c, List<List<float>> v) => c.Float32ListList = v));
                    }
                }
            }
            
            class UserDefinedVisitor : PropertyVisitor
            {
                protected override bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
                {
                    return false;
                }

                protected override void VisitList<TContainer, TList, TElement>(Property<TContainer, TList> property, ref TContainer container, ref TList value)
                {
                    Debug.Log($"Name=[{property.Name}] TContainer=[{typeof(TContainer)}] TList=[{typeof(TList)}] TElement=[{typeof(TElement)}]");

                    new UserDefinedFeature().Execute<TContainer, TList>(value);
                    
                    property.Visit(this, ref value);
                }

                protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
                {
                    Debug.Log($"Name=[{property.Name}] TContainer=[{typeof(TContainer)}] TValue=[{typeof(TValue)}]");
                        
                    new UserDefinedFeature().Execute<TContainer, TValue>(value);

                    property.Visit(this, ref value);
                }
            }

            class UserDefinedFeature
            {
                public virtual void Execute<TContainer, TValue>(TValue value)
                {
                    Debug.Log(value.ToString());
                }
            }
            
            abstract class AbstractVisitor : PropertyVisitor
            {
                protected override bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
                {
                    return false;
                }

                protected override void VisitList<TContainer, TList, TElement>(Property<TContainer, TList> property, ref TContainer container, ref TList value)
                {
                    Debug.Log($"Name=[{property.Name}] TContainer=[{typeof(TContainer)}] TList=[{typeof(TList)}] TElement=[{typeof(TElement)}]");
                    
                    UserDefinedOpenGeneric<TContainer, TList>();

                    property.Visit(this, ref value);
                }

                protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
                {
                    Debug.Log($"Name=[{property.Name}] TContainer=[{typeof(TContainer)}] TValue=[{typeof(TValue)}]");
                        
                    UserDefinedOpenGeneric<TContainer, TValue>();
                    
                    property.Visit(this, ref value);
                }

                public abstract void UserDefinedOpenGeneric<TContainer, TValue>();
            }

            class UserDefinedVisitorWithOpenGeneric : AbstractVisitor
            {
                public override void UserDefinedOpenGeneric<TContainer, TValue>()
                {
                    Debug.Log($"UserDefinedOpenGeneric {typeof(TContainer).Name} + {typeof(TValue).Name}");
                }
            }

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                PropertyBag.Register(new Nested.PropertyBag());
                PropertyBag.Register(new Container.PropertyBag());
                PropertyBag.Register(new UserDataA.PropertyBag());
                PropertyBag.Register(new UserDataB.PropertyBag());
            }
            
            [Test]
            public void PropertyVisitor_VisitingAUserDefinedContainerUsingAUserDefinedVisitor_DoesNotThrow()
            {
                var container = (object) new Container
                {
                    Int32List = new List<int>
                    {
                        1, 3, 6, 9
                    },
                    NestedList = new List<Nested>
                    {
                        new Nested()
                    },
                    InterfaceList = new List<IData>
                    {
                        new UserDataA(),
                        new UserDataB()
                    },
                    Float32ListList = new List<List<float>>
                    {
                        new List<float> { 1, 2, 3 },
                        new List<float> { 4, 5, 6 }
                    }
                };
                PropertyContainer.Visit(container, new UserDefinedVisitor());
            }
            
            [Test, Ignore("Not supported.")]
            public void PropertyVisitor_VisitingAUserDefinedContainerUsingAUserDefinedVisitorWithOpenGenerics_DoesNotThrow()
            {
                var container = (object) new Container
                {
                    Int32List = new List<int>
                    {
                        1, 3, 6, 9
                    },
                    NestedList = new List<Nested>
                    {
                        new Nested()
                    },
                    InterfaceList = new List<IData>
                    {
                        new UserDataA(),
                        new UserDataB()
                    },
                    Float32ListList = new List<List<float>>
                    {
                        new List<float> { 1, 2, 3 },
                        new List<float> { 4, 5, 6 }
                    }
                };
                PropertyContainer.Visit(container, new UserDefinedVisitorWithOpenGeneric());
            }
        }
    }
}
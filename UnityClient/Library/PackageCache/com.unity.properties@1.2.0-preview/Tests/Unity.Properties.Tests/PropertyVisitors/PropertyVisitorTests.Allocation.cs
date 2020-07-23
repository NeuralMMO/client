using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    partial class PropertyVisitorTests
    {
        [TestFixture]
        public class Allocations
        {
#pragma warning disable 649
            [GeneratePropertyBag]
            class EmptyClass
            {
            }

            [GeneratePropertyBag]
            struct EmptyStruct
            {
            }

            [GeneratePropertyBag]
            struct StructWithPrimitives
            {
                public int Int32Field;
                public float Float32Field;
            }

            [GeneratePropertyBag]
            public struct StructWithPrimitiveProperties
            {
                [CreateProperty] public int Int32Field { get; set; }
                [CreateProperty] public float Float32Field { get; set; }
            }

            [GeneratePropertyBag]
            public class ClassWithCollectionProperties
            {
                [CreateProperty] public List<float> Float32List { get; set; }
            }
#pragma warning restore 649
            
            class CountVisitor : PropertyVisitor
            {
                public int Count;

                protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
                {
                    ++Count;
                    base.VisitProperty(property, ref container, ref value);
                }
            }

            [Test]
            public void PropertyVisitor_VisitingAnEmptyStruct_DoesNotAllocate()
            {
                var container = new EmptyStruct();
                var visitor = new CountVisitor();

                GCAllocTest.Method(() =>
                           {
                               visitor.Count = 0;
                               PropertyContainer.Visit(ref container, visitor);
                           })
                           .ExpectedCount(0)
                           .Warmup()
                           .Run();

                Assert.That(visitor.Count, Is.EqualTo(0));
            }

            [Test]
            public void PropertyVisitor_VisitingAnEmptyClass_DoesNotAllocate()
            {
                var container = new EmptyClass();
                var visitor = new CountVisitor();

                GCAllocTest.Method(() =>
                           {
                               visitor.Count = 0;
                               PropertyContainer.Visit(ref container, visitor);
                           })
                           .ExpectedCount(0)
                           .Warmup()
                           .Run();

                Assert.That(visitor.Count, Is.EqualTo(0));
            }

            [Test]
            public void PropertyVisitor_VisitingAStructWithPrimitiveProperties_DoesNotAllocate()
            {
                var container = new StructWithPrimitiveProperties();
                var visitor = new CountVisitor();

                GCAllocTest.Method(() =>
                           {
                               visitor.Count = 0;
                               PropertyContainer.Visit(ref container, visitor);
                           })
                           .ExpectedCount(0)
                           .Warmup()
                           .Run();

                Assert.That(visitor.Count, Is.EqualTo(2));
            }

            [Test]
            [TestRequires_NET_4_6("Dynamic method invocation is not available on NET_STANDARD")]
            public void PropertyVisitor_VisitingAStructWithPrimitiveFields_DoesNotAllocate()
            {
                var container = new StructWithPrimitives();
                var visitor = new CountVisitor();

                GCAllocTest.Method(() =>
                           {
                               visitor.Count = 0;
                               PropertyContainer.Visit(ref container, visitor);
                           })
                           .ExpectedCount(0)
                           .Warmup()
                           .Run();

                Assert.That(visitor.Count, Is.EqualTo(2));
            }

            [Test]
            public void PropertyVisitor_VisitingAClassWithCollectionProperties_DoesNotAllocate()
            {
                var container = new ClassWithCollectionProperties
                {
                    Float32List = new List<float> {1, 2, 3}
                };
                var visitor = new CountVisitor();

                GCAllocTest.Method(() =>
                           {
                               visitor.Count = 0;
                               PropertyContainer.Visit(ref container, visitor);
                           })
                           .ExpectedCount(0)
                           .Warmup()
                           .Run();

                Assert.That(visitor.Count, Is.EqualTo(4));
            }
        }
    }
}
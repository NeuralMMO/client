using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Properties.Reflection.Tests
{
    [TestFixture]
    class PropertyVisitorTests
    {
        class Visitor : PropertyVisitor
        {
            public int Count { get; private set; }
            
            protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
            {
                Count++;
                base.VisitProperty(property, ref container, ref value);
            }
        }

        [Test]
        public void Visit_NestedLists_PropertyBagsAreGenerated()
        {
            var container = new List<List<List<int>>>
            {
                new List<List<int>>
                {
                    new List<int> {1, 2, 3},
                    new List<int> {4, 5, 6}
                },
                new List<List<int>>
                {
                    new List<int> {7, 8, 9},
                    new List<int> {10, 11, 12}
                },
                new List<List<int>>
                {
                    new List<int> {13, 14, 15},
                    new List<int> {16, 17, 18}
                }
            };
            
            var visitor = new Visitor();
            
            PropertyContainer.Visit(ref container, visitor);
            
            Assert.That(visitor.Count, Is.EqualTo(27));
        }
    }
}
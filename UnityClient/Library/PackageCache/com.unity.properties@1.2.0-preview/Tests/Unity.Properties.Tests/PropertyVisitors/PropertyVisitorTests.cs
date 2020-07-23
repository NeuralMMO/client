using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    [TestFixture]
    partial class PropertyVisitorTests : PropertiesTestFixture
    {
        [GeneratePropertyBag]
        class Node
        {
            public string Name;
            public List<Node> Children;
            
            public Node(string name) => Name = name;
        }

        class PropertyStateValidationVisitor : PropertyVisitor
        {
            public int Count;
            protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
            {
                var index = GetIndex(property);
                    
                Count++;

                property.Visit(this, ref value);

                Assert.That(GetIndex(property), Is.EqualTo(index));
            }

            static int GetIndex(IProperty property) => property is IListElementProperty l ? l.Index : -1;
        }

        [Test]
        public void PropertyVisitor_ContainerWithRecursiveTypes_PropertyStateIsCorrect()
        {
            var visitor = new PropertyStateValidationVisitor();

            PropertyContainer.Visit(new Node("Root")
            {
                Children = new List<Node>
                {
                    new Node("A")
                    {
                        Children = new List<Node>()
                        {
                            new Node("a.1"),
                            new Node("a.2"),
                            new Node("a.3")
                        }
                    },
                    new Node("B"),
                    new Node("C")
                    {
                        Children = new List<Node>()
                        {
                            new Node("c.1"),
                            new Node("c.2"),
                            new Node("c.3"),
                            new Node("c.4"),
                            new Node("c.5"),
                            new Node("c.6")
                        }
                    },
                    new Node("D")
                },
            }, visitor);

            Assert.That(visitor.Count, Is.EqualTo(41));
        }
    }
}
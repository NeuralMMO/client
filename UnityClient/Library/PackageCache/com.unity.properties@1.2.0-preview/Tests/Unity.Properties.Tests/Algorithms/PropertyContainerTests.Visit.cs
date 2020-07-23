using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    partial class PropertyContainerTests
    {
        class AssertNameAtPath : PropertyVisitor
        {
            readonly StringBuilder m_Builder = new StringBuilder();

            public void Matches(string str)
            {
                Assert.That(m_Builder.ToString(), Is.EqualTo(str));
            }
            
            public void Reset()
            {
                m_Builder.Clear();
            }
        
            protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
            {
                m_Builder.Append(property.Name + ".");
                property.Visit(this, ref value);
            }
        }
        
        
        [Test]
        [TestCase(
            nameof(ClassWithPolymorphicFields.ObjectValue),
            nameof(ClassWithPolymorphicFields.ObjectValue) + "." + nameof(ClassDerivedB.AbstractInt32Value) + "." + nameof(ClassDerivedB.DerivedBInt32Value) + ".")]
        [TestCase(
            nameof(ClassWithPolymorphicFields.AbstractValue),
            nameof(ClassWithPolymorphicFields.AbstractValue) + "." + nameof(ClassDerivedA.AbstractInt32Value) + "." + nameof(ClassDerivedA.DerivedAInt32Value) + ".")]
        [TestCase(nameof(ClassWithPolymorphicFields.InterfaceValue),
            nameof(ClassWithPolymorphicFields.InterfaceValue) + "." + nameof(ClassDerivedA1.AbstractInt32Value) + "." + nameof(ClassDerivedA1.DerivedAInt32Value) + "." + nameof(ClassDerivedA1.DerivedA1Int32Value) + ".")]
        public void PropertyContainer_VisitAtPath_VisitsCorrectPath(string path, string expected)
        {
            var visitor = new AssertNameAtPath();
            var container = new ClassWithPolymorphicFields
            {
                ObjectValue = new ClassDerivedB(),
                AbstractValue = new ClassDerivedA(),
                InterfaceValue = new ClassDerivedA1()
            };

            PropertyContainer.Visit(ref container, visitor, new PropertyPath(path));
            visitor.Matches(expected);
            visitor.Reset();
        }
    }
}
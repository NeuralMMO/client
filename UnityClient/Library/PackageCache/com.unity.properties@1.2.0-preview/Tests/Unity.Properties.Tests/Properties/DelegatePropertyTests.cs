using System;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    [TestFixture]
    class DelegatePropertyTests
    {
        struct Container
        {
            public int Value;
        }
        
        [Test]
        public void CreatingAValueProperty_WithNullSetter_ShouldCreateAsReadOnly()
        {
            var property = new DelegateProperty<Container, int>
            (
                name: "Value",
                getter: (ref Container c) => c.Value
            );
            
            Assert.That(property.Name, Is.EqualTo("Value"));
            Assert.That(property.IsReadOnly, Is.True);
        }
        
        [Test]
        public void GetValue_StructContainerWithIntField_ShouldReturnCorrectValue()
        {
            var property = new DelegateProperty<Container, int>
            (
                name: "Value",
                getter: (ref Container c) => c.Value
            );

            var container = new Container {Value = 42};

            Assert.That(property.GetValue(ref container), Is.EqualTo(42));
        }
        
        [Test]
        public void SetValue_StructContainerWithIntField_ValueShouldBeSet()
        {
            var property = new DelegateProperty<Container, int>
            (
                name: "Value",
                getter: (ref Container c) => c.Value,
                setter: (ref Container c, int v) => c.Value = v
            );

            var container = new Container {Value = 0};

            property.SetValue(ref container, 42);
            
            Assert.That(container.Value, Is.EqualTo(42));
        }
        
        [Test]
        public void SetValue_WhenPropertyIsReadOnly_ShouldThrow()
        {
            var property = new DelegateProperty<Container, int>
            (
                name: "Value",
                getter: (ref Container c) => c.Value
            );

            var container = new Container {Value = 0};

            Assert.Throws<InvalidOperationException>(() => { property.SetValue(ref container, 42); });
        }
    }
}
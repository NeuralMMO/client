using System;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    partial class PropertyContainerTests
    {
        [Test]
        public void GetValue_WithPropertyOfTheSameType_ValueShouldBeReturned()
        {
            var container = new StructWithPrimitives {Int32Value = 42};
            
            var value = PropertyContainer.GetValue<StructWithPrimitives, int>(ref container, nameof(StructWithPrimitives.Int32Value));
            
            Assert.That(value, Is.EqualTo(42));
        }
        
        [Test]
        public void GetValue_WithNonConvertibleType_ShouldThrow()
        {
            var container = new StructWithPrimitives();

            Assert.Throws<InvalidCastException>(() =>
            {
                PropertyContainer.GetValue<StructWithPrimitives, StructWithNoFields>(ref container, nameof(StructWithPrimitives.Int32Value));
            });
        }

        [Test]
        public void GetValue_WithInvalidName_ShouldThrow()
        {
            var container = new StructWithPrimitives();

            Assert.Throws<InvalidPathException>(() =>
            {
                PropertyContainer.GetValue<StructWithPrimitives, int>(ref container, "missing");
            });
        }
        
        [Test]
        public void GetValue_WithConcreteImplementationOfInterface_ValueShouldBeAssigned()
        {
            var container = new ClassWithPolymorphicFields {InterfaceValue = new ClassDerivedA {DerivedAInt32Value = 3}};
            
            var value = PropertyContainer.GetValue<ClassWithPolymorphicFields, ClassDerivedA>(ref container, nameof(ClassWithPolymorphicFields.InterfaceValue));

            Assert.That(value.DerivedAInt32Value, Is.EqualTo(3));
        }
        
        [Test]
        public void GetValue_WithInterface_ValueShouldBeAssigned()
        {
            var container = new ClassWithPolymorphicFields {InterfaceValue = new ClassDerivedA {DerivedAInt32Value = 3}};
            
            var value = PropertyContainer.GetValue<ClassWithPolymorphicFields, IContainerInterface>(ref container, nameof(ClassWithPolymorphicFields.InterfaceValue));

            Assert.That(value.GetType(), Is.EqualTo(typeof(ClassDerivedA)));
        }
    }
}
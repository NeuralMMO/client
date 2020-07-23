using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    partial class PropertyContainerTests
    {
        [Test]
        public void SetValue_AssigningAPropertyOfTheSameType_ValueShouldBeSet()
        {
            var container = new StructWithPrimitives();
            
            PropertyContainer.SetValue(ref container, nameof(StructWithPrimitives.Int32Value), 42);
            
            Assert.That(container.Int32Value, Is.EqualTo(42));
        }
        
        [Test]
        public void SetValue_WithNonConvertibleType_ShouldThrow()
        {
            var container = new StructWithPrimitives();

            Assert.Throws<InvalidCastException>(() =>
            {
                PropertyContainer.SetValue(ref container, nameof(StructWithPrimitives.Int32Value), new StructWithNoFields());
            });
        }
        
        [Test]
        public void SetValue_WithInvalidName_ShouldThrow()
        {
            var container = new StructWithPrimitives();

            Assert.Throws<InvalidPathException>(() =>
            {
                PropertyContainer.SetValue(ref container, "missing", 10);
            });
        }
        
        [Test]
        public void SetValue_AssigningAnInterfaceToAStruct_ValueShouldBeSet()
        {
            var container = new ClassWithPolymorphicFields();

            PropertyContainer.SetValue(ref container, nameof(ClassWithPolymorphicFields.InterfaceValue), new ClassDerivedA());
            
            Assert.That(container.InterfaceValue.GetType(), Is.EqualTo(typeof(ClassDerivedA)));
        }
        
        [Test]
        public void SetValue_AssigningAGenericListField_ValueShouldBeSet()
        {
            var container = new ClassWithLists();
            
            Assert.That(container.Int32List, Is.Null);

            PropertyContainer.SetValue(ref container, nameof(ClassWithLists.Int32List), new List<int> { 1, 2, 3 });
            
            Assert.That(container.Int32List, Is.Not.Null);
            Assert.That(container.Int32List.Count, Is.EqualTo(3));
        }
    }
}
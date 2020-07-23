using NUnit.Framework;

namespace Unity.Properties.Tests
{
    partial class PropertyContainerTests
    {
        [Test]
        [TestCase("ObjectValue")]
        [TestCase("ObjectValue.BoolValue")]
        [TestCase("ObjectValue.EnumInt32Unordered")]
        [TestCase("InterfaceValue")]
        [TestCase("InterfaceValue.AbstractInt32Value")]
        [TestCase("InterfaceValue.DerivedAInt32Value")]
        [TestCase("InterfaceValue.DerivedA1Int32Value")]
        [TestCase("AbstractValue")]
        [TestCase("AbstractValue.AbstractInt32Value")]
        [TestCase("AbstractValue.DerivedBInt32Value")]
        public void IsPathValid_WithValidPath_ReturnTrue(string path)
        {
            var container = new ClassWithPolymorphicFields
            {
                ObjectValue = new StructWithPrimitives(),
                InterfaceValue = new ClassDerivedA1(),
                AbstractValue = new ClassDerivedB()
            };
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath(path)), Is.True);
        }
        
        [Test]
        [TestCase("ObjecatValue")]
        [TestCase("ObjectValue.BoolValuse")]
        [TestCase("ObjectValue.EnumInt32fUnordered")]
        [TestCase("InterfabceValue")]
        [TestCase("InterfaceValue.AbstractsInt32Value")]
        [TestCase("InterfaceValue.DerivedAInbt32Value")]
        [TestCase("InterfaceValue.DerivedA1Inat32Value")]
        [TestCase("AbstractVaalue")]
        [TestCase("AbstractValue.AbstractInt32Vaalue")]
        [TestCase("AbstractValue.DerivedBInt32Vvalue")]
        public void IsPathValid_WithInvalidPath_ReturnFalse(string path)
        {
            var container = new ClassWithPolymorphicFields
            {
                ObjectValue = new StructWithPrimitives(),
                InterfaceValue = new ClassDerivedA1(),
                AbstractValue = new ClassDerivedB()
            };
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath(path)), Is.False);
        }
        
        [Test]
        public void IsPathValid_WithDynamicPaths_RemainsValid()
        {
            var container = new ClassWithPolymorphicFields();
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath("ObjectValue")), Is.True);
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath("ObjectValue.BoolValue")), Is.False);
            container.ObjectValue = new StructWithPrimitives();
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath("ObjectValue.BoolValue")), Is.True);
            container.ObjectValue = new ClassDerivedA1();
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath("ObjectValue.BoolValue")), Is.False);
            Assert.That(PropertyContainer.IsPathValid(ref container, new PropertyPath("ObjectValue.AbstractInt32Value")), Is.True);
        }
    }
}
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    [TestFixture]
    public abstract class PropertiesTestFixture
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            PropertyBag.Register(new ClassWithNoFields.PropertyBag());
            PropertyBag.Register(new StructWithNoFields.PropertyBag());
            PropertyBag.Register(new ClassWithPrimitives.PropertyBag());
            PropertyBag.Register(new StructWithPrimitives.PropertyBag());
            PropertyBag.Register(new ClassWithNestedClass.PropertyBag());
            PropertyBag.Register(new ClassWithNestedStruct.PropertyBag());
            PropertyBag.Register(new StructWithNestedClass.PropertyBag());
            PropertyBag.Register(new StructWithNestedStruct.PropertyBag());
            PropertyBag.Register(new ClassWithNestedClassRecursive.PropertyBag());
            PropertyBag.Register(new ClassWithArrays.PropertyBag());
            PropertyBag.Register(new ClassWithLists.PropertyBag());
            PropertyBag.Register(new ClassWithDictionaries.PropertyBag());
            PropertyBag.Register(new ClassWithPolymorphicFields.PropertyBag());
            PropertyBag.Register(new ClassDerivedA.PropertyBag());
            PropertyBag.Register(new ClassDerivedB.PropertyBag());
            PropertyBag.Register(new ClassDerivedA1.PropertyBag());
            PropertyBag.Register(new ClassWithNullable.PropertyBag());
            PropertyBag.Register(new ScriptableObjectWithPrimitives.PropertyBag());
            PropertyBag.Register(new ClassWithUnityObjects.PropertyBag());
        }
    }
}
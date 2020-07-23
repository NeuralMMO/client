using NUnit.Framework;
using Unity.Properties.Reflection.Internal;

namespace Unity.Properties.Reflection.Tests
{
    partial class ReflectedPropertyBagTests
    {
        class ClassWithPrivateProperties
        {
            public static string IntPropertyName => nameof(IntProperty);
            public static string FloatPropertyName => nameof(FloatProperty);
            public static string NonMaskedPropertyName => nameof(NonMaskedProperty);

            private int IntProperty { get; set; } = 42;
            [CreateProperty] private float FloatProperty { get; set; } = 123.456f;
            [CreateProperty] private int NonMaskedProperty { get; set; } = 1;
        }

        class DerivedClassWithPrivateProperties : ClassWithPrivateProperties
        {
            public static string BoolPropertyName => nameof(BoolProperty);
            public static string StringPropertyName => nameof(StringProperty);

            private bool BoolProperty { get; set; } = true;
            [CreateProperty] private string StringProperty { get; set; } = "Hello the World!";
            [CreateProperty] private int NonMaskedProperty { get; set; } = 2;
        }
        
        [Test]
        public void CreatePropertyBag_ClassWithPrivateProperties_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<ClassWithPrivateProperties>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithPrivateProperties.IntPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateProperties.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateProperties.NonMaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPrivateProperties.BoolPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPrivateProperties.StringPropertyName), Is.False);
            
            var container = new ClassWithPrivateProperties();
            
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPrivateProperties.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPrivateProperties.NonMaskedPropertyName), Is.EqualTo(1));
        }

        [Test]
        public void CreatePropertyBag_DerivedClassWithPrivateProperties_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<DerivedClassWithPrivateProperties>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithPrivateProperties.IntPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateProperties.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateProperties.NonMaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPrivateProperties.BoolPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPrivateProperties.StringPropertyName), Is.True);
            
            var container = new DerivedClassWithPrivateProperties();
            
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPrivateProperties.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPrivateProperties.NonMaskedPropertyName), Is.EqualTo(2));
            Assert.That(propertyBag.GetPropertyValue(ref container, DerivedClassWithPrivateProperties.StringPropertyName), Is.EqualTo("Hello the World!"));
        }
    }
}
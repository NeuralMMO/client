using NUnit.Framework;
using Unity.Properties.Reflection.Internal;

namespace Unity.Properties.Reflection.Tests
{
    partial class ReflectedPropertyBagTests
    {
#pragma warning disable 414
        
        class ClassWithPublicFields
        {            
            public static string IntPropertyName => nameof(m_IntField);
            public static string FloatPropertyName => nameof(m_FloatField);
            public static string MaskedPropertyName => nameof(m_MaskedField);
            public static string SkippedPropertyName => nameof(skipMe);

            public int m_IntField = 42;
            [CreateProperty] public float m_FloatField = 123.456f;
            [CreateProperty] public int m_MaskedField = 1;
            
            [DontCreateProperty]
            public int skipMe = 456;
        }
        
        class DerivedClassWithPublicFields : ClassWithPublicFields
        {
            public static string BoolPropertyName => nameof(m_BoolField);
            public static string StringPropertyName => nameof(m_StringField);

            public bool m_BoolField = true;
            [CreateProperty] public string m_StringField = "Hello the World!";
            [CreateProperty] public new int m_MaskedField = 2;
        }
        
#pragma warning restore 414
        
        [Test]
        public void CreatePropertyBag_ClassWithPublicFields_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<ClassWithPublicFields>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithPublicFields.IntPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicFields.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicFields.MaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicFields.SkippedPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPublicFields.BoolPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPublicFields.StringPropertyName), Is.False);
            
            var container = new ClassWithPublicFields();

            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicFields.IntPropertyName), Is.EqualTo(42));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicFields.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicFields.MaskedPropertyName), Is.EqualTo(1));
        }
        
        [Test]
        public void CreatePropertyBag_DerivedClassWithPublicFields_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<DerivedClassWithPublicFields>();

            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithPublicFields.IntPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicFields.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicFields.MaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicFields.SkippedPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPublicFields.BoolPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPublicFields.StringPropertyName), Is.True);

            var container = new DerivedClassWithPublicFields();

            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicFields.IntPropertyName), Is.EqualTo(42));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicFields.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicFields.MaskedPropertyName), Is.EqualTo(2));
            Assert.That(propertyBag.GetPropertyValue(ref container, DerivedClassWithPublicFields.BoolPropertyName), Is.EqualTo(true));
            Assert.That(propertyBag.GetPropertyValue(ref container, DerivedClassWithPublicFields.StringPropertyName), Is.EqualTo("Hello the World!"));
        }
    }
}
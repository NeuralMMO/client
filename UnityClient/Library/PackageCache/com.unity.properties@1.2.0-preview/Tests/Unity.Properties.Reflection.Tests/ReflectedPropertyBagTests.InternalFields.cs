using NUnit.Framework;
using Unity.Properties.Reflection.Internal;

namespace Unity.Properties.Reflection.Tests
{
    partial class ReflectedPropertyBagTests
    {
        class ClassWithInternalFields
        {
            public static string IntPropertyName => nameof(m_IntField);
            public static string FloatPropertyName => nameof(m_FloatField);
            public static string MaskedPropertyName => nameof(m_MaskedField);

            internal int m_IntField = 42;
            [CreateProperty] internal float m_FloatField = 123.456f;
            [CreateProperty] internal int m_MaskedField = 1;
        }

        class DerivedClassWithInternalFields : ClassWithInternalFields
        {
            public static string BoolPropertyName => nameof(m_BoolField);
            public static string StringPropertyName => nameof(m_StringField);

            internal bool m_BoolField = true;
            [CreateProperty] internal string m_StringField = "Hello the World!";
            [CreateProperty] internal new int m_MaskedField = 2;
        }
        
        [Test]
        public void CreatePropertyBag_ClassWithInternalFields_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<ClassWithInternalFields>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithInternalFields.IntPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithInternalFields.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithInternalFields.MaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(DerivedClassWithInternalFields.BoolPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithInternalFields.StringPropertyName), Is.False);
            
            var container = new ClassWithInternalFields();

            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithInternalFields.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithInternalFields.MaskedPropertyName), Is.EqualTo(1));
        }

        [Test]
        public void CreatePropertyBag_DerivedClassWithInternalFields_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<DerivedClassWithInternalFields>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithInternalFields.IntPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithInternalFields.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithInternalFields.MaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(DerivedClassWithInternalFields.BoolPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithInternalFields.StringPropertyName), Is.True);
            
            var container = new DerivedClassWithInternalFields();
            
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithInternalFields.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithInternalFields.MaskedPropertyName), Is.EqualTo(2));
            Assert.That(propertyBag.GetPropertyValue(ref container, DerivedClassWithInternalFields.StringPropertyName), Is.EqualTo("Hello the World!"));
        }
    }
}
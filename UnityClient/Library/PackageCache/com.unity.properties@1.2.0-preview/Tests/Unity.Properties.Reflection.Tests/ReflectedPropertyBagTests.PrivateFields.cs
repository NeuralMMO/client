using NUnit.Framework;
using Unity.Properties.Reflection.Internal;

namespace Unity.Properties.Reflection.Tests
{
    partial class ReflectedPropertyBagTests
    {
#pragma warning disable 414
        
        class ClassWithPrivateFields
        {
            public static string IntPropertyName => nameof(m_IntField);
            public static string FloatPropertyName => nameof(m_FloatField);
            public static string NonMaskedPropertyName => nameof(m_NonMaskedField);
            public static string DoublePropertyName => nameof(m_DoubleField);
            public static string LongPropertyName => nameof(m_LongField);

            private int m_IntField = 42;
            [CreateProperty] private float m_FloatField = 123.456f;
            [CreateProperty] private int m_NonMaskedField = 1;

            // ReSharper disable once MemberCanBePrivate.Local
            protected double m_DoubleField = 42.0; // Family
            // ReSharper disable once MemberCanBePrivate.Local
            protected internal long m_LongField = 123; // FamilyOrAssembly
        }

        class DerivedClassWithPrivateFields : ClassWithPrivateFields
        {
            public static string BoolPropertyName => nameof(m_BoolField);
            public static string StringPropertyName => nameof(m_StringField);

            private bool m_BoolField = true;
            [CreateProperty] private string m_StringField = "Hello the World!";
            [CreateProperty] private int m_NonMaskedField = 2;
        }
        
#pragma warning restore 414
        
        [Test]
        public void CreatePropertyBag_ClassWithPrivateFields_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<ClassWithPrivateFields>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.IntPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.NonMaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.LongPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.DoublePropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPrivateFields.BoolPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPrivateFields.StringPropertyName), Is.False);
            
            var container = new ClassWithPrivateFields();

            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPrivateFields.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPrivateFields.NonMaskedPropertyName), Is.EqualTo(1));
        }
        
        [Test]
        public void CreatePropertyBag_DerivedClassWithPrivateFields_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<DerivedClassWithPrivateFields>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.IntPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.NonMaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.LongPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithPrivateFields.DoublePropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPrivateFields.BoolPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPrivateFields.StringPropertyName), Is.True);
            
            var container = new DerivedClassWithPrivateFields();

            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPrivateFields.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPrivateFields.NonMaskedPropertyName), Is.EqualTo(2));
            Assert.That(propertyBag.GetPropertyValue(ref container, DerivedClassWithPrivateFields.StringPropertyName), Is.EqualTo("Hello the World!"));
        }
    }
}
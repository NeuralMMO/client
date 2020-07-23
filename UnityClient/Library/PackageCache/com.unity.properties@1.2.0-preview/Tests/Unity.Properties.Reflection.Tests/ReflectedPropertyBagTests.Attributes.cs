using System;
using NUnit.Framework;
using Unity.Properties.Reflection.Internal;
using UnityEngine;

namespace Unity.Properties.Reflection.Tests
{
#pragma warning disable 649
    partial class ReflectedPropertyBagTests
    {
        class ClassWithMemberAttributes
        {
            public static string PublicFieldWithNoAttributesName => nameof(PublicFieldWithNoAttributes);
            public static string PrivateFieldWithNoAttributesName => nameof(PrivateFieldWithNoAttributes);
            public static string PublicPropertyWithNoAttributesName => nameof(PublicPropertyWithNoAttributes);
            public static string PrivatePropertyWithNoAttributesName => nameof(PrivatePropertyWithNoAttributes);
            public static string PublicFieldWithCreatePropertyAttributeName => nameof(PublicFieldWithCreatePropertyAttribute);
            public static string PrivateFieldWithCreatePropertyAttributeName => nameof(PrivateFieldWithCreatePropertyAttribute);
            public static string PublicPropertyWithCreatePropertyAttributeName => nameof(PublicPropertyWithCreatePropertyAttribute);
            public static string PrivatePropertyWithCreatePropertyAttributeName => nameof(PrivatePropertyWithCreatePropertyAttribute);
            public static string PublicFieldWithSerializeFieldAttributeName => nameof(PublicFieldWithSerializeFieldAttribute);
            public static string PrivateFieldWithSerializeFieldAttributeName => nameof(PrivateFieldWithSerializeFieldAttribute);
            public static string PublicPropertyWithSerializeFieldAttributeName => nameof(PublicPropertyWithSerializeFieldAttribute);
            public static string PrivatePropertyWithSerializeFieldAttributeName => nameof(PrivatePropertyWithSerializeFieldAttribute);
            public static string PublicFieldWithCreatePropertyAndNonSerializedAttributesName => nameof(PublicFieldWithCreatePropertyAndNonSerializedAttributes);
            public static string PrivateFieldWithCreatePropertyAndNonSerializedAttributesName => nameof(PrivateFieldWithCreatePropertyAndNonSerializedAttributes);
            public static string PublicFieldWithSerializeFieldAndNonSerializedAttributesName => nameof(PublicFieldWithSerializeFieldAndNonSerializedAttributes);
            public static string PrivateFieldWithSerializeFieldAndNonSerializedAttributesName => nameof(PrivateFieldWithSerializeFieldAndNonSerializedAttributes);

            public int PublicFieldWithNoAttributes;
            int PrivateFieldWithNoAttributes;
            public int PublicPropertyWithNoAttributes { get; set; }
            int PrivatePropertyWithNoAttributes { get; set; }
            
            [CreateProperty] public int PublicFieldWithCreatePropertyAttribute;
            [CreateProperty] int PrivateFieldWithCreatePropertyAttribute;
            [CreateProperty] public int PublicPropertyWithCreatePropertyAttribute { get; set; }
            [CreateProperty] int PrivatePropertyWithCreatePropertyAttribute { get; set; }
            
            [SerializeField] public int PublicFieldWithSerializeFieldAttribute;
            [SerializeField] int PrivateFieldWithSerializeFieldAttribute;
            [SerializeField] public int PublicPropertyWithSerializeFieldAttribute { get; set; }
            [SerializeField] int PrivatePropertyWithSerializeFieldAttribute { get; set; }
            
            [CreateProperty, NonSerialized] public int PublicFieldWithCreatePropertyAndNonSerializedAttributes;
            [CreateProperty, NonSerialized] int PrivateFieldWithCreatePropertyAndNonSerializedAttributes;
            
            [SerializeField, NonSerialized] public int PublicFieldWithSerializeFieldAndNonSerializedAttributes;
            [SerializeField, NonSerialized] int PrivateFieldWithSerializeFieldAndNonSerializedAttributes;
        }
        
        [Test]
        public void CreatePropertyBag_ClassWithMemberAttributes_PropertiesAreGeneratedCorrectly()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<ClassWithMemberAttributes>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PublicFieldWithNoAttributesName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PrivateFieldWithNoAttributesName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PublicPropertyWithNoAttributesName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PrivatePropertyWithNoAttributesName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PublicFieldWithCreatePropertyAttributeName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PrivateFieldWithCreatePropertyAttributeName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PublicPropertyWithCreatePropertyAttributeName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PrivatePropertyWithCreatePropertyAttributeName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PublicFieldWithSerializeFieldAttributeName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PrivateFieldWithSerializeFieldAttributeName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PublicPropertyWithSerializeFieldAttributeName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PrivatePropertyWithSerializeFieldAttributeName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PublicFieldWithCreatePropertyAndNonSerializedAttributesName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PrivateFieldWithCreatePropertyAndNonSerializedAttributesName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PublicFieldWithSerializeFieldAndNonSerializedAttributesName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithMemberAttributes.PrivateFieldWithSerializeFieldAndNonSerializedAttributesName), Is.False);
        }
    }
#pragma warning restore 649
}
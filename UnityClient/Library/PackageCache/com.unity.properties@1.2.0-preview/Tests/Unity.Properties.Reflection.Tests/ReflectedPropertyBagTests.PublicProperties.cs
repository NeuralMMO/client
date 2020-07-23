using NUnit.Framework;
using Unity.Properties.Reflection.Internal;

namespace Unity.Properties.Reflection.Tests
{
    partial class ReflectedPropertyBagTests
    {
        class ClassWithPublicProperties
        {
            public static string IntPropertyName => nameof(IntProperty);
            public static string FloatPropertyName => nameof(FloatProperty);
            public static string MaskedPropertyName => nameof(MaskedProperty);
            public static string VirtualPropertyName => nameof(VirtualProperty);

            public int IntProperty { get; set; } = 42;
            [CreateProperty] public float FloatProperty { get; set; } = 123.456f;
            [CreateProperty] public int MaskedProperty { get; set; } = 1;
            [CreateProperty] public virtual short VirtualProperty { get; set; } = -12345;
        }

        class DerivedClassWithPublicProperties : ClassWithPublicProperties
        {
            public static string BoolPropertyName => nameof(BoolProperty);
            public static string StringPropertyName => nameof(StringProperty);

            public bool BoolProperty { get; set; } = true;
            [CreateProperty] public string StringProperty { get; set; } = "Hello the World!";
            [CreateProperty] public new int MaskedProperty { get; set; } = 2;
            [CreateProperty] public override short VirtualProperty { get; set; } = 12345;
        }

        abstract class AbstractClassWithPublicProperties
        {
            public static string IntPropertyName => nameof(IntProperty);
            public static string FloatPropertyName => nameof(FloatProperty);

            public abstract int IntProperty { get; set; }
            [CreateProperty] public abstract float FloatProperty { get; set; }
        }

        class ImplementedAbstractClassWithPublicProperties : AbstractClassWithPublicProperties
        {
            public override int IntProperty { get; set; } = 13;
            [CreateProperty] public override float FloatProperty { get; set; } = 3.1416f;
        }
        
        [Test]
        public void CreatePropertyBag_ClassWithPublicProperties_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<ClassWithPublicProperties>();
            
            Assert.That(propertyBag, Is.Not.Null);
            
            Assert.That(propertyBag.HasProperty(ClassWithPublicProperties.IntPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithPublicProperties.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicProperties.MaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicProperties.VirtualPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPublicProperties.BoolPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPublicProperties.StringPropertyName), Is.False);
            
            var container = new ClassWithPublicProperties();
            
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicProperties.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicProperties.MaskedPropertyName), Is.EqualTo(1));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicProperties.VirtualPropertyName), Is.EqualTo((short)-12345));
        }
        
        [Test]
        public void CreatePropertyBag_DerivedClassWithPublicProperties_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<DerivedClassWithPublicProperties>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(ClassWithPublicProperties.IntPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(ClassWithPublicProperties.FloatPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicProperties.MaskedPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(ClassWithPublicProperties.VirtualPropertyName), Is.True);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPublicProperties.BoolPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(DerivedClassWithPublicProperties.StringPropertyName), Is.True);
            
            var container = new DerivedClassWithPublicProperties();
            
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicProperties.FloatPropertyName), Is.EqualTo(123.456f));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicProperties.MaskedPropertyName), Is.EqualTo(2));
            Assert.That(propertyBag.GetPropertyValue(ref container, ClassWithPublicProperties.VirtualPropertyName), Is.EqualTo((short)12345));
            Assert.That(propertyBag.GetPropertyValue(ref container, DerivedClassWithPublicProperties.StringPropertyName), Is.EqualTo("Hello the World!"));
        }
        
        [Test]
        public void CreatePropertyBag_ImplementedAbstractClassWithPublicProperties_PropertiesAreGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<ImplementedAbstractClassWithPublicProperties>();
            
            Assert.That(propertyBag, Is.Not.Null);

            Assert.That(propertyBag.HasProperty(AbstractClassWithPublicProperties.IntPropertyName), Is.False);
            Assert.That(propertyBag.HasProperty(AbstractClassWithPublicProperties.FloatPropertyName), Is.True);
            
            var container = new ImplementedAbstractClassWithPublicProperties();
            
            Assert.That(propertyBag.GetPropertyValue(ref container, AbstractClassWithPublicProperties.FloatPropertyName), Is.EqualTo(3.1416f));
        }
    }
}
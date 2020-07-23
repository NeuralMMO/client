using System;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;

namespace Unity.Properties.Tests
{
    [TestFixture]
    class PropertyAttributeTests
    {
        class AttributeA : Attribute
        {
            public int A;
        }

        class AttributeB : Attribute
        {
            public int B;
        }

        class AttributeC : Attribute
        {
            public int C;
        }

        struct TestContainer
        {
        }

        class TestProperty : Property<TestContainer, int>
        {
            public override string Name => "test";
            public override bool IsReadOnly => false;
            public override int GetValue(ref TestContainer container) => throw new NotImplementedException();
            public override void SetValue(ref TestContainer container, int value) => throw new NotImplementedException();
        }
        
        [Test]
        public void CreatingAProperty_WithAttributes_AttributesCanBeQueried()
        {
            var property = new TestProperty()
                           .WithAttribute(new AttributeA {A = 1})
                           .WithAttribute(new AttributeB {B = 2})
                           .WithAttribute(new AttributeC {C = 3})
                           .WithAttribute(new AttributeA {A = 4});

            Assert.That(property.HasAttribute<AttributeA>(), Is.True);
            Assert.That(property.HasAttribute<AttributeB>(), Is.True);
            Assert.That(property.HasAttribute<AttributeC>(), Is.True);
            Assert.That(property.HasAttribute<ReadOnlyAttribute>(), Is.False);

            Assert.That(property.GetAttribute<AttributeA>().A, Is.EqualTo(1));

            Assert.That(property.GetAttributes<AttributeA>().Count(), Is.EqualTo(2));
        }
    }
}
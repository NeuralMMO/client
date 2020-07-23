using System;
using System.Collections.Generic;
using Unity.Properties.UI.Internal;
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    [UI]
    class NullElementTests
    {
        class SomeOtherType
        {
        }
        
        class SomeType
        {
            public SomeOtherType NestedField;
        }
        
        class MultipleDerivedTypes{}
        class Derived1 : MultipleDerivedTypes{}
        class Derived2 : MultipleDerivedTypes{}
        
        class Container
        {
            public SomeType NullField;
            public SomeType NonNullFieldWithNestedNullField = new SomeType();
            public SomeType NonNullFieldWithNestedNonNullField = new SomeType{ NestedField = new SomeOtherType()};
        }

        class TypeWithString
        {
#pragma warning disable 649
            public string Value;
#pragma warning restore 649
        }

        class TypeWithDerivableField
        {
#pragma warning disable 649
            public MultipleDerivedTypes Value;
#pragma warning restore 649
        }

        [Test]
        public void NullElement_WithStringField_CreatesAButton()
        {
            var container = new TypeWithString(); 
            var element = new PropertyElement(); 
            element.SetTarget(container);

            var stringField = element.Q<NullElement<string>>();
            Assert.That(stringField, Is.Not.Null);
            Assert.That(stringField.Q<Button>(), Is.Not.Null);
            Assert.That(stringField.Q<PopupField<Type>>(), Is.Null);
        }
        
        [Test]
        public void NullElement_WithSingleConstructableType_CreatesAButton()
        {
            var container = new SomeType(); 
            var element = new PropertyElement(); 
            element.SetTarget(container);

            var stringField = element.Q<NullElement<SomeOtherType>>();
            Assert.That(stringField, Is.Not.Null);
            Assert.That(stringField.Q<Button>(), Is.Not.Null);
            Assert.That(stringField.Q<PopupField<Type>>(), Is.Null);
        }
        
        [Test]
        public void NullElement_WithMultipleConstructableTypes_CreatesAPopup()
        {
            var container = new TypeWithDerivableField(); 
            var element = new PropertyElement(); 
            element.SetTarget(container);

            var stringField = element.Q<NullElement<MultipleDerivedTypes>>();
            Assert.That(stringField, Is.Not.Null);
            Assert.That(stringField.Q<Button>(), Is.Null);
            Assert.That(stringField.Q<PopupField<Type>>(), Is.Not.Null);
        }

        [Test]
        public void PropertyElement_WithNullFields_CreatesNullElement()
        {
            var container = new Container(); 
            var element = new PropertyElement();
            element.SetTarget(container);
            
            Assert.That(element.Query<NullElement<SomeType>>().ToList().Count, Is.EqualTo(1));
            Assert.That(element.Query<NullElement<SomeOtherType>>().ToList().Count, Is.EqualTo(1));
        }
        
        [Test]
        public void NullElement_WithUnderlyingDataNotNullAnymore_UpdatesCorrectly()
        {
            var container = new Container(); 
            var element = new PropertyElement();
            element.SetTarget(container);

            var someTypes = new List<NullElement<SomeType>>();
            element.Query<NullElement<SomeType>>().ToList(someTypes);
            Assert.That(someTypes.Count, Is.EqualTo(1));

            var someOtherTypes = new List<NullElement<SomeOtherType>>();
            element.Query<NullElement<SomeOtherType>>().ToList(someOtherTypes);
            Assert.That(someOtherTypes.Count, Is.EqualTo(1));
            
            container.NonNullFieldWithNestedNullField.NestedField = new SomeOtherType();
            foreach (IBinding binding in someTypes)
            {
                binding.Update();
            }
            
            foreach (IBinding binding in someOtherTypes)
            {
                binding.Update();
            }
            someTypes.Clear();
            someOtherTypes.Clear();
            
            element.Query<NullElement<SomeType>>().ToList(someTypes);
            Assert.That(someTypes.Count, Is.EqualTo(1));

            element.Query<NullElement<SomeOtherType>>().ToList(someOtherTypes);
            Assert.That(someOtherTypes.Count, Is.EqualTo(0));
            
            container.NullField = new SomeType{ NestedField = new SomeOtherType()};
            foreach (IBinding binding in someTypes)
            {
                binding.Update();
            }
            
            foreach (IBinding binding in someOtherTypes)
            {
                binding.Update();
            }
            someTypes.Clear();
            someOtherTypes.Clear();
            
            element.Query<NullElement<SomeType>>().ToList(someTypes);
            Assert.That(someTypes.Count, Is.EqualTo(0));

            element.Query<NullElement<SomeOtherType>>().ToList(someOtherTypes);
            Assert.That(someOtherTypes.Count, Is.EqualTo(0));
        }
    }
}

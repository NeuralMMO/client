#pragma warning disable 649
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Properties.UI.Internal;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Properties.UI.Tests.Attributes
{
    [UI]
    class UIAttributesTests : WindowTestsFixtureBase
    {
        interface IType {}
        
        interface INoDerivedType {}

        abstract class BaseType : IType {}
        
        class TypeA : BaseType {}

        class TypeB : BaseType {}

        class TypeC : BaseType
        {
            public TypeA ConcreteTypeA;
        }
        
        class InvalidType : BaseType
        {
            public InvalidType(int value){}
        }

        class Unrelated
        {
        }
        
        class AllFine
        {
            [CreateInstanceOnInspection] public TypeA ConcreteTypeA;
            [CreateInstanceOnInspection] public TypeB ConcreteTypeB;
            [CreateInstanceOnInspection(typeof(TypeA))] public IType InterfaceToConcreteTypeA;
        }

        class Collection
        {
            [CreateInstanceOnInspection] public List<TypeA> ConcreteTypeAs;
        }

        class SubFields
        {
            [CreateInstanceOnInspection] public TypeC ConcreteTypeC;
        }

        class Warnings
        {
            [CreateInstanceOnInspection] public INoDerivedType NoDerivedType;
            [CreateInstanceOnInspection(typeof(InvalidType))] public IType InvalidType;
            [CreateInstanceOnInspection(typeof(Unrelated))] public IType NotAssignable;
        }
        
        [Test]
        public void NullValue_WithCreateElementOnInspectionAttribute_InstantiateValue()
        {
            var instance = new AllFine();
            
            // Null before
            Assert.That(instance.ConcreteTypeA, Is.Null);
            Assert.That(instance.ConcreteTypeB, Is.Null);
            Assert.That(instance.InterfaceToConcreteTypeA, Is.Null);
            
            Element.SetTarget(instance);
            
            // Not null after
            Assert.That(instance.ConcreteTypeA, Is.Not.Null);
            Assert.That(instance.ConcreteTypeB, Is.Not.Null);
            Assert.That(instance.InterfaceToConcreteTypeA, Is.Not.Null);
            
            // Instances have the expected types
            Assert.That(instance.ConcreteTypeA.GetType(), Is.EqualTo(typeof(TypeA)));
            Assert.That(instance.ConcreteTypeB.GetType(), Is.EqualTo(typeof(TypeB)));
            Assert.That(instance.InterfaceToConcreteTypeA.GetType(), Is.EqualTo(typeof(TypeA)));
        }

        [Test]
        public void CreateElementOnInspectionAttribute_OnCollectionTypes_AffectsCollectionOnly()
        {
            var instance = new Collection
            {
                ConcreteTypeAs = new List<TypeA>{ null }
            };
            Assert.That(instance.ConcreteTypeAs[0], Is.Null);
            Element.SetTarget(instance);
            Assert.That(instance.ConcreteTypeAs[0], Is.Null);
        }
        
        [Test]
        public void NullValue_WithCreateElementOnInspectionAttribute_IsNotPropagatedToSubFields()
        {
            var instance = new SubFields();
            Assert.That(instance.ConcreteTypeC, Is.Null);
            Element.SetTarget(instance);
            Assert.That(instance.ConcreteTypeC, Is.Not.Null);
            Assert.That(instance.ConcreteTypeC.ConcreteTypeA, Is.Null);
        }
        
        [Test]
        public void NonNullValue_WithCreateElementOnInspectionAttribute_DoesNothing()
        {
            var instance = new AllFine
            {
                ConcreteTypeA = new TypeA(),
                ConcreteTypeB = new TypeB(),
                InterfaceToConcreteTypeA = new TypeA()
            };

            var typeA = instance.ConcreteTypeA;
            var typeB = instance.ConcreteTypeB;
            var interfaceToTypeA = instance.InterfaceToConcreteTypeA;
            
            // Not null before
            Assert.That(instance.ConcreteTypeA, Is.Not.Null);
            Assert.That(instance.ConcreteTypeB, Is.Not.Null);
            Assert.That(instance.InterfaceToConcreteTypeA, Is.Not.Null);
            Element.SetTarget(instance);
            
            // Not null after
            Assert.That(instance.ConcreteTypeA, Is.Not.Null);
            Assert.That(instance.ConcreteTypeB, Is.Not.Null);
            Assert.That(instance.InterfaceToConcreteTypeA, Is.Not.Null);
            
            // Instance were not recreated
            Assert.That(instance.ConcreteTypeA, Is.EqualTo(typeA));
            Assert.That(instance.ConcreteTypeB, Is.EqualTo(typeB));
            Assert.That(instance.InterfaceToConcreteTypeA, Is.EqualTo(interfaceToTypeA));
        }
        
        [Test]
        public void NullValue_WithInvalidCreateElementOnInspectionAttribute_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, PropertyChecks.GetNotConstructableWarningMessage(typeof(INoDerivedType)));
            LogAssert.Expect(LogType.Warning, PropertyChecks.GetNotConstructableWarningMessage(typeof(InvalidType)));
            LogAssert.Expect(LogType.Warning, PropertyChecks.GetNotAssignableWarningMessage(typeof(Unrelated), typeof(IType)));
            Element.SetTarget(new Warnings());
        }
    }
}
#pragma warning restore 649

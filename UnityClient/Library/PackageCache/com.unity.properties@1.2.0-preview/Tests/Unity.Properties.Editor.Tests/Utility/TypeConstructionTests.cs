using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Unity.Properties.Editor;
using UnityEngine;

namespace Unity.Properties.Tests
{
    class TypeConstructionTests
    {
        static class Types
        {
            public class NoConstructorClass{}

            public class DefaultConstructorClass
            {
                public DefaultConstructorClass() {}
            }
            
            public class InternalDefaultConstructorClass
            {
                internal InternalDefaultConstructorClass() {}
            }
            
            public class PrivateDefaultConstructorClass
            {
                PrivateDefaultConstructorClass() {}
            }

            public class CustomConstructorClass
            {
                public CustomConstructorClass(int a) {}
            }
            
            public class DefaultAndCustomConstructorClass
            {
                public DefaultAndCustomConstructorClass() {}
                public DefaultAndCustomConstructorClass(int a) {}
            }
            
            public struct NoConstructorStruct{}

            public struct CustomConstructorStruct
            {
                public CustomConstructorStruct(int a) {}
            }

            public interface IInterface {}
            public abstract class AbstractClassWithNoConstructor : IInterface {}

            public class ChildOfAbstractClassWithNoConstructor : AbstractClassWithNoConstructor
            {
            }

            public abstract class AbstractClassWithDefaultConstructor : IInterface
            {
                public AbstractClassWithDefaultConstructor() {}
            }
            
            public class ChildOfAbstractClassWithDefaultConstructor : AbstractClassWithDefaultConstructor
            {
            }
            
            public abstract class AbstractClassWithInternalDefaultConstructor : IInterface
            {
                internal AbstractClassWithInternalDefaultConstructor() {}
            }
            
            public class ChildOfAbstractClassWithPrivateDefaultConstructor : AbstractClassWithInternalDefaultConstructor
            {
            }

            public class NotConstructableBaseClass
            {
                protected NotConstructableBaseClass() {}
            }

            public class NotConstructableDerivedClass : NotConstructableBaseClass
            {
                protected NotConstructableDerivedClass() {}
            }
            
            public class ConstructableDerivedClass : NotConstructableBaseClass {}
            public class A : ConstructableDerivedClass {}
            public class B : ConstructableDerivedClass {}
            public class C : ConstructableDerivedClass {}
        }
        
        [Test]
        public void CanBeConstructedFromGenericMethod_WithConstructableType_ReturnsTrue()
        {
            Assert.That(TypeConstruction.CanBeConstructed<ConstructibleBaseType>(), Is.True);
            Assert.That(TypeConstruction.CanBeConstructed<ConstructibleDerivedType>(), Is.True);
            Assert.That(TypeConstruction.CanBeConstructed<NoConstructorType>(), Is.True);
            Assert.That(TypeConstruction.CanBeConstructed<ParameterLessConstructorType>(), Is.True);
            Assert.That(TypeConstruction.CanBeConstructed<ScriptableObjectType>(), Is.True);
        }
        
        [Test]
        public void CanBeConstructedFromGenericMethod_WithNonConstructableType_ReturnsFalse()
        {
            Assert.That(TypeConstruction.CanBeConstructed<IConstructInterface>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructed<AbstractConstructibleBaseType>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructed<NonConstructibleDerivedType>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructed<ParameterConstructorType>(), Is.False);
        }

        [Test]
        [TestCase(typeof(Types.NoConstructorClass))]
        [TestCase(typeof(Types.NoConstructorStruct))]
        [TestCase(typeof(Types.DefaultConstructorClass))]
        [TestCase(typeof(Types.DefaultAndCustomConstructorClass))]
        [TestCase(typeof(Types.CustomConstructorStruct))]
        [TestCase(typeof(Types.ChildOfAbstractClassWithNoConstructor))]
        [TestCase(typeof(Types.ChildOfAbstractClassWithDefaultConstructor))]
        [TestCase(typeof(Types.ChildOfAbstractClassWithPrivateDefaultConstructor))]
        public void CanBeConstructedFromType_WithConstructableType_ReturnsTrue(Type type)
        {
            Assert.That(TypeConstruction.CanBeConstructed(type), Is.True);
        }
        
        [Test]
        [TestCase(typeof(Types.CustomConstructorClass))]
        [TestCase(typeof(Types.PrivateDefaultConstructorClass))]
        [TestCase(typeof(Types.InternalDefaultConstructorClass))]
        [TestCase(typeof(Types.IInterface))]
        [TestCase(typeof(Types.AbstractClassWithNoConstructor))]
        [TestCase(typeof(Types.AbstractClassWithDefaultConstructor))]
        [TestCase(typeof(Types.AbstractClassWithInternalDefaultConstructor))]
        public void CanBeConstructedFromType_WithNonConstructableType_ReturnsFalse(Type type)
        {
            Assert.That(TypeConstruction.CanBeConstructed(type), Is.False);
        }
        
        [Test]
        public void ConstructingAnInstance_WithAConstructableType_ReturnsAnActualInstance()
        {
            Assert.That(TypeConstruction.Construct<Types.NoConstructorClass>(), Is.Not.Null);
            Assert.That(TypeConstruction.Construct<Types.NoConstructorStruct>(), Is.Not.Null);
            Assert.That(TypeConstruction.Construct<Types.DefaultConstructorClass>(), Is.Not.Null);
            Assert.That(TypeConstruction.Construct<Types.DefaultAndCustomConstructorClass>(), Is.Not.Null);
            Assert.That(TypeConstruction.Construct<Types.CustomConstructorStruct>(), Is.Not.Null);
            Assert.That(TypeConstruction.Construct<Types.ChildOfAbstractClassWithNoConstructor>(), Is.Not.Null);
            Assert.That(TypeConstruction.Construct<Types.ChildOfAbstractClassWithDefaultConstructor>(), Is.Not.Null);
            Assert.That(TypeConstruction.Construct<Types.ChildOfAbstractClassWithPrivateDefaultConstructor>(), Is.Not.Null);
        }
        
        [Test]
        public void TryToConstructAnInstance_WithAConstructableType_ReturnsTrue()
        {
            Assert.That(TypeConstruction.TryConstruct<Types.NoConstructorClass>(out _), Is.True);
            Assert.That(TypeConstruction.TryConstruct<Types.NoConstructorStruct>(out _), Is.True);
            Assert.That(TypeConstruction.TryConstruct<Types.DefaultConstructorClass>(out _), Is.True);
            Assert.That(TypeConstruction.TryConstruct<Types.DefaultAndCustomConstructorClass>(out _), Is.True);
            Assert.That(TypeConstruction.TryConstruct<Types.CustomConstructorStruct>(out _), Is.True);
            Assert.That(TypeConstruction.TryConstruct<Types.ChildOfAbstractClassWithNoConstructor>(out _), Is.True);
            Assert.That(TypeConstruction.TryConstruct<Types.ChildOfAbstractClassWithDefaultConstructor>(out _), Is.True);
            Assert.That(TypeConstruction.TryConstruct<Types.ChildOfAbstractClassWithPrivateDefaultConstructor>(out _), Is.True);
        }
        
        [Test]
        public void SettingAndUnSettingAnExplicitConstructionMethod_ToCreateAnInstance_BehavesProperly()
        {
            Assert.That(TypeConstruction.CanBeConstructed<ParameterConstructorType>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructed(typeof(ParameterConstructorType)), Is.False);
            TypeConstruction.SetExplicitConstructionMethod(ExplicitConstruction);
            Assert.That(TypeConstruction.CanBeConstructed<ParameterConstructorType>(), Is.True);
            Assert.That(TypeConstruction.CanBeConstructed(typeof(ParameterConstructorType)), Is.True);
            {
                var instance = TypeConstruction.Construct<ParameterConstructorType>();
                Assert.That(instance, Is.Not.Null);
                Assert.That(instance.Value, Is.EqualTo(10.0f));
            }
            
            TypeConstruction.UnsetExplicitConstructionMethod(ExplicitConstruction);
            Assert.That(TypeConstruction.CanBeConstructed<ParameterConstructorType>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructed(typeof(ParameterConstructorType)), Is.False);
        }

        [Test]
        public void SettingAnExplicitConstructionMethod_WithAnotherExplicitConstructionMethodAlreadySet_Throws()
        {
            TypeConstruction.SetExplicitConstructionMethod(ExplicitConstruction);
            Assert.Throws<InvalidOperationException>(() =>TypeConstruction.SetExplicitConstructionMethod(OtherExplicitConstruction));
            TypeConstruction.UnsetExplicitConstructionMethod(ExplicitConstruction);
        }
        
        [Test]
        public void SettingAnExplicitConstructionMethod_WhenAlreadySet_Throws()
        {
            TypeConstruction.SetExplicitConstructionMethod(ExplicitConstruction);
            Assert.Throws<InvalidOperationException>(() =>TypeConstruction.SetExplicitConstructionMethod(ExplicitConstruction));
            TypeConstruction.UnsetExplicitConstructionMethod(ExplicitConstruction);
        }
        
        [Test]
        public void UnSettingAnExplicitConstructionMethod_WhenNoExplicitConstructionMethodIsRegistered_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>TypeConstruction.UnsetExplicitConstructionMethod(ExplicitConstruction));
        }
        
        [Test]
        public void UnSettingAnExplicitConstructionMethod_WithAnotherExplicitConstructionMethodSet_Throws()
        {
            TypeConstruction.SetExplicitConstructionMethod(ExplicitConstruction);
            Assert.Throws<InvalidOperationException>(() =>TypeConstruction.UnsetExplicitConstructionMethod(OtherExplicitConstruction));
            TypeConstruction.UnsetExplicitConstructionMethod(ExplicitConstruction);
        }
        
        [Test]
        public void TryingToSetAnExplicitConstructionMethod_WithAnotherExplicitConstructionMethodAlreadySet_ReturnsFalse()
        {
            TypeConstruction.SetExplicitConstructionMethod(ExplicitConstruction);
            Assert.That(TypeConstruction.TrySetExplicitConstructionMethod(OtherExplicitConstruction), Is.False);
            TypeConstruction.UnsetExplicitConstructionMethod(ExplicitConstruction);
        }
        
        [Test]
        public void TryingToSetAnExplicitConstructionMethod_WhenAlreadySet_ReturnsFalse()
        {
            TypeConstruction.SetExplicitConstructionMethod(ExplicitConstruction);
            Assert.That(TypeConstruction.TrySetExplicitConstructionMethod(ExplicitConstruction), Is.False);
            TypeConstruction.UnsetExplicitConstructionMethod(ExplicitConstruction);
        }
        
        [Test]
        public void TryingToUnsetAnExplicitConstructionMethod_WhenNoExplicitConstructionMethodIsRegistered_ReturnsFalse()
        {
            Assert.That(TypeConstruction.TryUnsetExplicitConstructionMethod(ExplicitConstruction), Is.False);
        }
        
        [Test]
        public void TryingToUnsetAnExplicitConstructionMethod_WithAnotherExplicitConstructionMethodSet_ReturnsFalse()
        {
            TypeConstruction.SetExplicitConstructionMethod(ExplicitConstruction);
            Assert.That(TypeConstruction.TryUnsetExplicitConstructionMethod(OtherExplicitConstruction), Is.False);
            TypeConstruction.UnsetExplicitConstructionMethod(ExplicitConstruction);
        }

        [Test]
        public void SettingAndUnSettingAnExplicitConstructionMethod_WhenNoneAlreadySet_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TypeConstruction.SetExplicitConstructionMethod(ExplicitConstruction));
            Assert.DoesNotThrow(() => TypeConstruction.UnsetExplicitConstructionMethod(ExplicitConstruction));
        }
        
        [Test]
        public void TryingToSetAndUnsetAnExplicitConstructionMethod_WhenNoneAlreadySet_ReturnsTrue()
        {
            Assert.That(TypeConstruction.TrySetExplicitConstructionMethod(ExplicitConstruction), Is.True);
            Assert.That(TypeConstruction.TryUnsetExplicitConstructionMethod(ExplicitConstruction), Is.True);
        }

        [Test]
        public void GettingAllConstructableTypes_FromGenericType_ReturnsProperCount()
        {
            var types = new List<Type>();
            TypeConstruction.GetAllConstructableTypes<Types.NotConstructableBaseClass>(types);
            Assert.That(types.Count, Is.EqualTo(4));
            
            types.Clear();
            TypeConstruction.GetAllConstructableTypes<Types.A>(types);
            Assert.That(types.Count, Is.EqualTo(1));
            
            types.Clear();
            TypeConstruction.GetAllConstructableTypes<Types.NotConstructableDerivedClass>(types);
            Assert.That(types.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void GettingAllConstructableTypes_FromType_ReturnsProperCount()
        {
            var types = new List<Type>();
            TypeConstruction.GetAllConstructableTypes(typeof(Types.NotConstructableBaseClass), types);
            Assert.That(types.Count, Is.EqualTo(4));
            
            types.Clear();
            TypeConstruction.GetAllConstructableTypes(typeof(Types.A), types);
            Assert.That(types.Count, Is.EqualTo(1));
            
            types.Clear();
            TypeConstruction.GetAllConstructableTypes(typeof(Types.NotConstructableDerivedClass), types);
            Assert.That(types.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void CanBeConstructedFromDerivedType_FromConstructableDerivedType_ReturnsTrue()
        {
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<IConstructInterface>(), Is.True);
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<AbstractConstructibleBaseType>(), Is.True);
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<ConstructibleBaseType>(), Is.True);
        }
        
        [Test]
        public void CanBeConstructedFromDerivedType_FromNonConstructableDerivedType_ReturnsFalse()
        {
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<ConstructibleDerivedType>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<NonConstructibleDerivedType>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<NoConstructorType>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<ParameterLessConstructorType>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<ParameterConstructorType>(), Is.False);
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<ScriptableObjectType>(), Is.False);
        }
        
        [Test]
        public void ConstructingAndInstance_FromADerivedType_ReturnsAnInstance()
        {
            {
                var instance = TypeConstruction.Construct<ConstructibleBaseType>(typeof(ConstructibleDerivedType));
                Assert.That(instance, Is.Not.Null);
                Assert.That(instance, Is.TypeOf<ConstructibleDerivedType>());
                Assert.That(instance.Value, Is.EqualTo(25.0f));
                Assert.That((instance as ConstructibleDerivedType).SubValue, Is.EqualTo(50.0f));
            }

            {
                var instance = TypeConstruction.Construct<ConstructibleDerivedType>();
                Assert.That(instance, Is.Not.Null);
                Assert.That(instance.Value, Is.EqualTo(25.0f));
                Assert.That(instance.SubValue, Is.EqualTo(50.0f));
            }
            
            {
                var instance = TypeConstruction.Construct<ConstructibleDerivedType>();
                Assert.That(instance, Is.Not.Null);
                Assert.That(instance.Value, Is.EqualTo(25.0f));
                Assert.That(instance.SubValue, Is.EqualTo(50.0f));
            }
        }
        
        [Test]
        public void ConstructingAndInstance_FromANonConstructableDerivedType_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => TypeConstruction.Construct<IConstructInterface>(typeof(NonConstructibleDerivedType)));
        }

        [Test]
        public void ConstructingAndInstance_FromANonAssignableDerivedType_Throws()
        {
            Assert.Throws<ArgumentException>(() => TypeConstruction.Construct<IConstructInterface>(typeof(ParameterLessConstructorType)));
        }
        
        [Test]
        public void ConstructingAnInstance_DerivedFromObject_IsAlwaysPossible()
        {
            Assert.That(TypeConstruction.CanBeConstructedFromDerivedType<object>(), Is.True);
            Assert.That(TypeConstruction.Construct<object>(typeof(Types.A)), Is.Not.Null);
        }
        
        static ParameterConstructorType ExplicitConstruction()
        {
            return new ParameterConstructorType(10.0f);
        }
        
        static ParameterConstructorType OtherExplicitConstruction()
        {
            return new ParameterConstructorType(10.0f);
        }
    }
}

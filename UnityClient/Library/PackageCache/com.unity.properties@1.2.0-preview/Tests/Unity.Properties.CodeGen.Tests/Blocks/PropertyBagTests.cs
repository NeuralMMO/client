using System;
using System.Reflection;
using NUnit.Framework;
using Unity.Properties.CodeGen.Tests.Types;

namespace Unity.Properties.CodeGen.Tests
{
    [TestFixture]
    sealed class PropertyBagTests : PostProcessTestBase
    {
        [Test]
        [Ignore("Test is not stable for CI.")]
        public void StructWithPrimitives()
        {
            TestPropertyBag(MethodBase.GetCurrentMethod(), typeof(StructWithPrimitives));
        }
        
        [Test]
        [Ignore("Test is not stable for CI.")]
        public void ClassWithGenericParameter_WhenGenericIsInt32()
        {
            TestPropertyBag(MethodBase.GetCurrentMethod(), typeof(ClassWithGenericParameter<int>));
        }
        
        [Test]
        [Ignore("Test is not stable for CI.")]
        public void NestedClassWithOuterGenericParameter_WhenGenericIsInt32()
        {
            TestPropertyBag(MethodBase.GetCurrentMethod(), typeof(ClassWithGenericParameter<int>.Nested));
        }
        
        [Test]
        [Ignore("Test is not stable for CI.")]
        public void ClassWithCollections()
        {
            TestPropertyBag(MethodBase.GetCurrentMethod(), typeof(ClassWithCollections));
        }
        
        [Test]
        [Ignore("Test is not stable for CI.")]
        public void StructWithPrimitiveFromAnotherAssembly()
        {
            TestPropertyBag(MethodBase.GetCurrentMethod(), typeof(StructWithPrimitiveFromAnotherAssembly));
        }
        
        [Test]
        [Ignore("Test is not stable for CI.")]
        public void ClassFromAnotherAssemblyWithHiddenField()
        {
            TestPropertyBag(MethodBase.GetCurrentMethod(), typeof(ClassFromAnotherAssemblyWithHiddenField));
        }
        
        static void TestPropertyBag(MethodBase test, Type type, bool overwriteExpectationWithReality = false)
        {
            var source = GetAssemblyDefinition(type.Assembly);
            var name = $".expectations/{test.DeclaringType.Name}.{test.Name}";

            Test
            (
                name: name,
                source: source,
                action: context =>
                {
                    var generated = Blocks.PropertyBag.Generate(context, context.Module.ImportReference(type));
                    context.Module.Types.Add(generated);
                },
                overwriteExpectationWithReality
            );
        }
    }
}
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;

namespace Unity.Properties.CodeGen.Tests
{
    [TestFixture]
    sealed class PropertyTests : PostProcessTestBase
    {
        [Test]
        [Ignore("Test is not stable for CI.")]
        public void StructWithInt32Field_WhenFieldIsPublic()
        {
            TestProperty(MethodBase.GetCurrentMethod(), typeof(StructWithPrimitives).GetField(nameof(StructWithPrimitives.Int32Field)));
        }

        [Test]
        [Ignore("Test is not stable for CI.")]
        public void StructWithInt32Field_WhenFieldIsPrivate()
        {
            TestProperty(MethodBase.GetCurrentMethod(), typeof(StructWithPrimitives).GetField("HiddenInt32Field", BindingFlags.NonPublic | BindingFlags.Instance));
        }

        [Test]
        [Ignore("Test is not stable for CI.")]
        public void StructWithInt32Property_WhenPropertyIsPublicReadWrite()
        {
            TestProperty(MethodBase.GetCurrentMethod(), typeof(StructWithPrimitives).GetProperty(nameof(StructWithPrimitives.Int32Property)));
        }

        [Test]
        [Ignore("Test is not stable for CI.")]
        public void StructWithInt32Property_WhenPropertyIsPublicReadOnly()
        {
            TestProperty(MethodBase.GetCurrentMethod(), typeof(StructWithPrimitives).GetProperty(nameof(StructWithPrimitives.Int32PropertyReadOnly)));
        }

        [Test]
        [Ignore("Test is not stable for CI.")]
        public void ClassWithInt32ListField_WhenFieldIsPublic()
        {
            TestProperty(MethodBase.GetCurrentMethod(), typeof(ClassWithCollections).GetField(nameof(ClassWithCollections.Int32List)));
        }
        
        [Test]
        [Ignore("Test is not stable for CI.")]
        public void StructWithInt32Field_WhenFieldHasCustomAttribute()
        {
            TestProperty(MethodBase.GetCurrentMethod(), typeof(StructWithPrimitives).GetField(nameof(StructWithPrimitives.Int32FieldWithCustomAttribute)));
        }
        
        static void TestProperty(MethodBase test, MemberInfo member, bool overwriteExpectationWithReality = false)
        {
            var type = member.DeclaringType;
            var source = GetAssemblyDefinition(type.Assembly);
            var name = $".expectations/{test.DeclaringType.Name}.{test.Name}";
            
            Test
            (
                name: name, 
                source: source, 
                action: context =>
                {
                    var typeDefinition = source.MainModule.GetType(type.FullName);
                    var memberDefinition = (IMemberDefinition) null;

                    switch (member)
                    {
                        case FieldInfo _:
                            memberDefinition = typeDefinition.Fields.First(f => f.Name == member.Name);
                            break;
                        case PropertyInfo _:
                            memberDefinition = typeDefinition.Properties.First(p => p.Name == member.Name);
                            break;
                    }
                    
                    var generated = Blocks.Property.Generate(context, context.Module.ImportReference(type), memberDefinition);
                    context.Module.Types.Add(generated);
                }, 
                overwriteExpectationWithReality
            );
        }
    }
}
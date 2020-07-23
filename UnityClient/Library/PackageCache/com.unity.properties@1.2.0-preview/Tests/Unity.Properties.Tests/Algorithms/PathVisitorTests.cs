using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    class TestPathVisitor : Internal.PathVisitor
    {
        public TestPathVisitor(PropertyPath path)
        {
            Path = path;
        }

        protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            
        }
    }
    
    [TestFixture]
    class PathVisitorTests : PropertiesTestFixture
    {
        [Test]
        public void PathVisitor_VisitArrayElement_ReturnVisitErrorCodeOk()
        {
            var container = new ClassWithLists
            {
                Int32List = new List<int> {1, 2, 3}
            };

            var visitor = new TestPathVisitor(new PropertyPath($"{nameof(ClassWithLists.Int32List)}[1]"));
            
            PropertyContainer.Visit(ref container, visitor);
            
            Assert.That(visitor.ErrorCode, Is.EqualTo(Internal.VisitErrorCode.Ok));
        }
        
        [Test]
        public void PathVisitor_VisitNestedContainer_ReturnVisitErrorCodeOk()
        {
            var container = new StructWithNestedStruct
            {
                Container = new StructWithPrimitives()
            };

            var visitor = new TestPathVisitor(new PropertyPath($"{nameof(StructWithNestedStruct.Container)}.{nameof(StructWithPrimitives.Float64Value)}"));
            
            PropertyContainer.Visit(ref container, visitor);
            
            Assert.That(visitor.ErrorCode, Is.EqualTo(Internal.VisitErrorCode.Ok));
        }
    }
}
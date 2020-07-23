using NUnit.Framework;

namespace Unity.Properties.Tests
{
    [TestFixture]
    class PropertyPathTests
    {
        [Test]
        public void CanConstructPropertyPathManually()
        {
            var propertyPath = new PropertyPath();
            Assert.That(propertyPath.PartsCount, Is.EqualTo(0));
            propertyPath.PushName("Foo");
            Assert.That(propertyPath.PartsCount, Is.EqualTo(1));
            Assert.That(propertyPath[0].Type, Is.EqualTo(PropertyPath.PartType.Name));
            Assert.That(propertyPath[0].Name, Is.EqualTo("Foo"));
            
            propertyPath.PushName("Bar");
            Assert.That(propertyPath.PartsCount, Is.EqualTo(2));
            Assert.That(propertyPath[1].Type, Is.EqualTo(PropertyPath.PartType.Name));
            Assert.That(propertyPath[1].Name, Is.EqualTo("Bar"));
            
            propertyPath.PushIndex(5);
            Assert.That(propertyPath.PartsCount, Is.EqualTo(3));
            Assert.That(propertyPath[2].Type, Is.EqualTo(PropertyPath.PartType.Index));
            Assert.That(propertyPath[2].Index, Is.EqualTo(5));
            
            propertyPath.PushName("Bee");
            Assert.That(propertyPath.PartsCount, Is.EqualTo(4));
            Assert.That(propertyPath[3].Type, Is.EqualTo(PropertyPath.PartType.Name));
            Assert.That(propertyPath[3].Name, Is.EqualTo("Bee"));
            
            Assert.That(propertyPath.ToString(), Is.EqualTo("Foo.Bar[5].Bee"));
            
            propertyPath.Pop();
            
            Assert.That(propertyPath.PartsCount, Is.EqualTo(3));
            Assert.That(propertyPath.ToString(), Is.EqualTo("Foo.Bar[5]"));
            
            propertyPath.Clear();
            
            Assert.That(propertyPath.PartsCount, Is.EqualTo(0));
            Assert.That(propertyPath.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        [TestCase("")]
        [TestCase("Foo")]
        [TestCase("[0]")]
        [TestCase("Foo[0]")]
        [TestCase("Foo[0].Bar")]
        [TestCase("Foo[0].Bar[1]")]
        [TestCase("Foo.Bar")]
        [TestCase("Foo.Bar[0]")]
        [TestCase("Foo.Bar[\"one\"]")]
        public void CanConstructAPropertyPathFromAString(string path)
        {
            Assert.That(() => CreateFromString(path), Throws.Nothing);
        }

        [Test]
        [TestCase("", 0)]
        [TestCase("Foo", 1)]
        [TestCase("Foo[0]", 2)]
        [TestCase("Foo[0].Bar", 3)]
        [TestCase("Foo[0].Bar[1]", 4)]
        [TestCase("Foo.Bar", 2)]
        [TestCase("Foo.Bar[0]", 3)]
        [TestCase("Foo.Foo.Foo.Foo.Foo", 5)]
        public void PropertyPathHasTheRightAmountOfParts(string path, int partCount)
        {
            var propertyPath = new PropertyPath(path);
            Assert.That(propertyPath.PartsCount, Is.EqualTo(partCount));
        }

        [Test]
        [TestCase("Foo[0]", 0)]
        [TestCase("Foo[1]", 1)]
        [TestCase("Foo.Bar[2]", 2)]
        [TestCase("Foo.Bar[12]", 12)]
        [TestCase("Foo[0].Foo[1].Foo[2].Foo[3].Foo[4]", 0, 1, 2, 3, 4)]
        public void PropertyPathMapsListIndicesCorrectly(string path, params int[] indices)
        {
            var propertyPath = new PropertyPath(path);
            var listIndex = 0;
            for (var i = 0; i < propertyPath.PartsCount; ++i)
            {
                var part = propertyPath[i];
                if (part.IsIndex)
                {
                    Assert.That(part.Index, Is.EqualTo(indices[listIndex]));
                    ++listIndex;
                }
            }
        }
        
        [Test]
        [TestCase("Foo[-1]")]
        [TestCase("Foo.Bar[-20]")]
        public void ThrowsWhenUsingNegativeIndices(string path)
        {
            Assert.That(() => CreateFromString(path), Throws.ArgumentException);
        }
        
        [Test]
        [TestCase("Foo[lol]")]
        public void ThrowsWhenUsingNonNumericIndices(string path)
        {
            Assert.That(() => CreateFromString(path), Throws.ArgumentException);
        }
        
        static PropertyPath CreateFromString(string path)
        {
            return new PropertyPath(path);
        }
    }
}
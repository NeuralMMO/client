using NUnit.Framework;

namespace Unity.Properties.Tests
{
    [TestFixture]
    class PropertyBagStoreTests
    {
        struct Foo
        {
            
        }

        [Test]
        [TestRequires_ENABLE_IL2CPP("This test is to ensure reflected bags are disabled on IL2CPP platforms.")]
        public void GetPropertyBag_WithUnregisteredType_ReturnsNull()
        {
            Assert.That(Internal.PropertyBagStore.GetPropertyBag<Foo>(), Is.Null);
        }
    }
}
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    [TestFixture]
    class PropertyBagTests
    {
        class TestPropertyBag<T> : ContainerPropertyBag<T>
        {
            
        }
        
        [Test]
        public void CreateInstance_WithNonContainerType_Throws()
        {
            Assert.Throws<TypeInitializationException>(() =>
            {
                new TestPropertyBag<int>();
            });
        }
    }
}
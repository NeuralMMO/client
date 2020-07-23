using System.Collections.Generic;
using NUnit.Framework;
using Unity.Properties.Internal;
using Unity.Properties.Reflection.Internal;

namespace Unity.Properties.Reflection.Tests
{
    partial class ReflectedPropertyBagTests
    {
        [Test]
        public void CreatePropertyBag_ListOfInt_ListPropertyBagIsGenerated()
        {
            var propertyBag = new ReflectedPropertyBagProvider().CreatePropertyBag<List<int>>();

            Assert.That(propertyBag.GetType(), Is.EqualTo(typeof(ListPropertyBag<List<int>, int>)));
        }
    }
}
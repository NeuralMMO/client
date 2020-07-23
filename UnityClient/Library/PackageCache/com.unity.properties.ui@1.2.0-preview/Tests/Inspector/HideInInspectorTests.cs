#pragma warning disable 649
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties.UI.Internal;

namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    class HideInInspectorTests
    {
        public static class Types
        {
            public class FieldWithHideInInspector
            {
                public float Float;
                [HideInInspector]
                public int Int;
                public string String;
            }    
        }

        [Test]
        public void PropertyElement_FieldWithHideInInspector_AreNotShown()
        {
            var withHideInInspectorField = new PropertyElement();
            withHideInInspectorField.SetTarget(new Types.FieldWithHideInInspector());
            var customInspectorElements = withHideInInspectorField.Query<CustomInspectorElement>().ToList(); 
            Assert.That(customInspectorElements.Count, Is.EqualTo(0));
            Assert.That(withHideInInspectorField.childCount, Is.EqualTo(2));
            Assert.That(withHideInInspectorField.Query<IntegerField>().First(), Is.Null);
        }
    }
}
#pragma warning restore 649
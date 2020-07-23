#pragma warning disable 649
using System.Linq;
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    class VisualElementExtensionsTests : WindowTestsFixtureBase
    {
        struct SomeType
        {
            public Texture2D Texture;
            public Sprite Sprite;
            public float Value1;
            public float Value2;
            public float Value3;
        }
        
        VisualElement m_Hidden;
        VisualElement m_Visible;
        
        protected override void OnSetupReady()
        {
            m_Hidden = new VisualElement();
            m_Hidden.Hide();
            
            m_Visible = new VisualElement();
            m_Visible.Show();
            
            Element.Add(m_Hidden);
            Element.Add(m_Visible);
        }

        [Test]
        public void CallingShow_OnAVisibleElement_DoesNothing()
        {
            Assert.That(m_Visible.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            m_Visible.Show();
            Assert.That(m_Visible.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        }
        
        [Test]
        public void CallingShow_OnAnInvisibleElement_ShowsIt()
        {
            Assert.That(m_Hidden.style.display.value, Is.EqualTo(DisplayStyle.None));
            m_Hidden.Show();
            Assert.That(m_Hidden.style.display.value, Is.EqualTo(DisplayStyle.Flex));
        }
        
        [Test]
        public void CallingHide_OnAVisibleElement_HidesIt()
        {
            Assert.That(m_Visible.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            m_Visible.Hide();
            Assert.That(m_Visible.style.display.value, Is.EqualTo(DisplayStyle.None));
        }
        
        [Test]
        public void CallingHide_OnAnInvisibleElement_DoesNothing()
        {
            Assert.That(m_Hidden.style.display.value, Is.EqualTo(DisplayStyle.None));
            m_Hidden.Hide();
            Assert.That(m_Hidden.style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        [Test]
        public void Querying_ChildrenOfType_ReturnsCorrectCount()
        {
            Element.SetTarget(new SomeType());
            Assert.That(Element.ChildrenOfType<ObjectField>().Count(), Is.EqualTo(2));
            Assert.That(Element.ChildrenOfType<FloatField>().Count(), Is.EqualTo(3));
        }
    }
}
#pragma warning restore 649

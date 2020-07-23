using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Properties.UI.Internal;
using Unity.Properties.Tests;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Resources = Unity.Properties.UI.Internal.Resources;

#pragma warning disable 649
namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    partial class PropertyElementTests : WindowTestsFixtureBase
    {
        class HideAttribute : Attribute
        {
        }
        
        class ShowAttribute : Attribute
        {
        }

        class FilterByAttribute
        {       

            [Hide] public int Int;
            [Hide] public string String = "";
            [Hide] public double Double;
            [Show] public float Float;

        }

        class ContainsATexture
        {
            public Texture2D Texture;
        }
        
        [Test]
        public void PropertyElement_Target_CanBeSetAndGet()
        {
            var element = new PropertyElement();
            Assert.That(element.TryGetTarget(out UIContainer _), Is.False);
            Assert.Throws<InvalidOperationException>(() => element.GetTarget<UIContainer>());

            var container = new UIContainer();
            Assert.DoesNotThrow(() => element.SetTarget(container));
            Assert.That(element.TryGetTarget(out UIContainer _), Is.True);
            Assert.DoesNotThrow(() => element.GetTarget<UIContainer>());

            Assert.DoesNotThrow(() => element.SetTarget((object)container));
            Assert.That(element.TryGetTarget(out UIContainer _), Is.True);
            Assert.DoesNotThrow(() => element.GetTarget<UIContainer>());
            
            Assert.DoesNotThrow(() => element.SetTarget(container));
            Assert.That(element.TryGetTarget(out object _), Is.True);
            Assert.DoesNotThrow(() => element.GetTarget<object>());
            
            var container2 = new ComplexUIContainer();
            Assert.That(element.TryGetTarget(out ComplexUIContainer _), Is.False);
            Assert.Throws<InvalidCastException>(() => element.GetTarget<ComplexUIContainer>());
            Assert.DoesNotThrow(() => element.SetTarget(container2));
            Assert.That(element.TryGetTarget(out ComplexUIContainer _), Is.True);
            Assert.DoesNotThrow(() => element.GetTarget<ComplexUIContainer>());

            var container3 = new ComplexUIChildContainer();
            Assert.DoesNotThrow(() => element.SetTarget(container3));
            Assert.That(element.TryGetTarget(out ComplexUIContainer _), Is.True);
            Assert.DoesNotThrow(() => element.GetTarget<ComplexUIContainer>());
        }

        [Test]
        public void PropertyElement_Changes_ArePropagated()
        {
            Element.SetTarget(GetContainer());

            var expectedPath = new PropertyPath();
            Element.OnChanged += (propertyElement, path) =>
            {
                Assert.That(path.ToString(), Is.EqualTo(expectedPath.ToString()));
            };

            {
                var field = Element.Query<FloatField>()
                    .Where(f => f.bindingPath == nameof(ComplexUIContainer.FloatField)).First();
                expectedPath.Clear();
                expectedPath.PushName(nameof(ComplexUIContainer.FloatField));
                field.value = 25.0f;
            }

            {
                var field = Element.Query<IntegerField>()
                    .Where(f => f.bindingPath == nameof(ComplexUIContainer.IntField)).First();
                expectedPath.Clear();
                expectedPath.PushName(nameof(ComplexUIContainer.IntField));
                field.value = 50;
            }

            {
                var field = Element.Query<TextField>()
                    .Where(f => f.bindingPath == nameof(ComplexUIContainer.StringField)).First();
                expectedPath.Clear();
                expectedPath.PushName(nameof(ComplexUIContainer.StringField));
                field.value = "Mueueue";
            }

            {
                var field = Element.Query<IListElement<List<int>, int>>()
                    .Where(f => f.bindingPath == nameof(ComplexUIContainer.IntListField)).First()
                    .Query<IntegerField>().Where(f => f.bindingPath == "2").First();
                expectedPath.Clear();
                expectedPath.PushName(nameof(ComplexUIContainer.IntListField));
                expectedPath.PushIndex(2);
                field.value = 4;
            }

            {
                var kvps = Element.Query<DictionaryElement<Dictionary<int, int>, int, int>>()
                    .Where(f => f.bindingPath == nameof(ComplexUIContainer.IntIntDictionary)).First()
                    .Query<IntegerField>().Where(f => f.bindingPath == "Value").ToList();
                for (var i = 0; i < kvps.Count; ++i)
                {
                    var field = kvps[i];
                    expectedPath.Clear();
                    expectedPath.PushName(nameof(ComplexUIContainer.IntIntDictionary));
                    expectedPath.PushKey(i);
                    expectedPath.PushName("Value");
                    field.value = 20;
                }
            }
        }

        [Test]
        public void PropertyElement_SettingSameValue_AreNotPropagated()
        {
            Element.SetTarget(GetContainer());

            Element.OnChanged += (propertyElement, path) => throw new Exception();

            Element.Query<FloatField>().Where(f => f.bindingPath == nameof(ComplexUIContainer.FloatField)).First()
                .value = 50.0f;
            Element.Query<IntegerField>().Where(f => f.bindingPath == nameof(ComplexUIContainer.IntField)).First()
                .value = 25;
            Element.Query<TextField>().Where(f => f.bindingPath == nameof(ComplexUIContainer.StringField)).First()
                .value = "Hey";

            var items = Element.Query<IListElement<List<int>, int>>()
                .Where(f => f.bindingPath == nameof(ComplexUIContainer.IntListField)).First()
                .m_ContentRoot.Query<IntegerField>().ToList();
            for (var i = 0; i < items.Count; ++i)
            {
                items[i].value = i;
            }

            var kvps = Element.Query<DictionaryElement<Dictionary<int, int>, int, int>>()
                .Where(f => f.bindingPath == nameof(ComplexUIContainer.IntIntDictionary)).First()
                .Query<IntegerField>().Where(f => f.bindingPath == "Value").ToList();
            for (var i = 0; i < kvps.Count; ++i)
            {
                kvps[i].value = i;
            }
        }

        [Test]
        public void PropertyElement_IsPathValid_ReturnsTrueForValidPath()
        {
            var element = new PropertyElement();
            var container = GetContainer();
            element.SetTarget(container);

            var path = new PropertyPath();
            {
                path.Clear();
                path.PushName(nameof(ComplexUIContainer.FloatField));
                Assert.That(element.IsPathValid(path), Is.True);
            }

            {
                path.Clear();
                path.PushName(nameof(ComplexUIContainer.IntField));
                Assert.That(element.IsPathValid(path), Is.True);
            }

            {
                path.Clear();
                path.PushName(nameof(ComplexUIContainer.StringField));
                Assert.That(element.IsPathValid(path), Is.True);
            }

            {
                path.Clear();
                path.PushName(nameof(ComplexUIContainer.IntListField));
                Assert.That(element.IsPathValid(path), Is.True);
                for (var i = 0; i < container.IntListField.Count; ++i)
                {
                    path.PushIndex(i);
                    Assert.That(element.IsPathValid(path), Is.True);
                    path.Pop();
                }
            }

            {
                path.Clear();
                path.PushName(nameof(ComplexUIContainer.IntIntDictionary));
                Assert.That(element.IsPathValid(path), Is.True);
                foreach (var kvp in container.IntIntDictionary.ToList())
                {
                    path.PushKey(kvp.Key);
                    Assert.That(element.IsPathValid(path), Is.True);

                    path.PushName("Key");
                    Assert.That(element.IsPathValid(path), Is.True);
                    path.Pop();

                    path.PushName("Value");
                    Assert.That(element.IsPathValid(path), Is.True);
                    path.Pop();

                    path.Pop();
                }
            }
        }

        [Test]
        public void PropertyElement_IsPathValid_ReturnsFalseForInvalidPath()
        {
            var element = new PropertyElement();
            var container = GetContainer();
            element.SetTarget(container);

            var path = new PropertyPath();
            {
                path.Clear();
                path.PushName("jdhj");
                Assert.That(element.IsPathValid(path), Is.False);
            }

            {
                path.Clear();
                path.PushName(nameof(ComplexUIContainer.IntListField));
                Assert.That(element.IsPathValid(path), Is.True);
                for (var i = container.IntListField.Count; i < container.IntListField.Count + 10; ++i)
                {
                    path.PushIndex(i);
                    Assert.That(element.IsPathValid(path), Is.False);
                    path.Pop();
                }
            }

            {
                path.Clear();
                path.PushName(nameof(ComplexUIContainer.IntIntDictionary));
                Assert.That(element.IsPathValid(path), Is.True);
                foreach (var kvp in container.IntIntDictionary.ToList())
                {
                    path.PushName(kvp.Key.ToString());
                    Assert.That(element.IsPathValid(path), Is.False);
                }
            }
        }

        [Test]
        public void PropertyElement_WithAttributeFilter_ExcludeFromHierarchyGeneration()
        {
            var element = new PropertyElement();
            element.SetTarget(new FilterByAttribute());
            Assert.That(element.contentContainer.childCount, Is.EqualTo(4));
            Assert.That(element[0], Is.InstanceOf<IntegerField>());
            Assert.That(element[1], Is.InstanceOf<TextField>());
            Assert.That(element[2], Is.InstanceOf<DoubleField>());
            Assert.That(element[3], Is.InstanceOf<FloatField>());
            
            element.SetAttributeFilter(attributes => null == attributes.OfType<HideAttribute>().FirstOrDefault());
            
            Assert.That(element.contentContainer.childCount, Is.EqualTo(1));
            Assert.That(element[0], Is.InstanceOf<FloatField>());
        }
        
        [Test]
        public void PropertyElement_WithAttributeFilter_IncludeInHierarchyGeneration()
        {
            var element = new PropertyElement();
            element.SetTarget(new FilterByAttribute());
            Assert.That(element.contentContainer.childCount, Is.EqualTo(4));
            Assert.That(element[0], Is.InstanceOf<IntegerField>());
            Assert.That(element[1], Is.InstanceOf<TextField>());
            Assert.That(element[2], Is.InstanceOf<DoubleField>());
            Assert.That(element[3], Is.InstanceOf<FloatField>());
            
            element.SetAttributeFilter(attributes => null != attributes.OfType<ShowAttribute>().FirstOrDefault());
            
            Assert.That(element.contentContainer.childCount, Is.EqualTo(1));
            Assert.That(element[0], Is.InstanceOf<FloatField>());
        }

        [Test]
        public void ObjectField_WhenSettingBackNullValue_DoesNotThrowAnException()
        {
            var instance = new ContainsATexture();
            Assert.DoesNotThrow(() =>Element.SetTarget(instance));
            var textureField = Element.Q<ObjectField>();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Resources.Icons + "dark/Add.png");
            Assert.That(texture, Is.Not.Null);
            Assert.DoesNotThrow(() => textureField.value = texture);
            Assert.DoesNotThrow(() => textureField.value = null);
        }

        
        static ComplexUIContainer GetContainer()
        {
            return new ComplexUIContainer
            {
                FloatField = 50.0f,
                IntField = 25,
                StringField = "Hey",
                IntListField = new List<int> {0, 1, 2, 3, 4},
                IntIntDictionary = new Dictionary<int, int>
                {
                    {0, 0},
                    {1, 1},
                    {2, 2},
                    {3, 3},
                    {4, 4},
                }
            };
        }
    }
}
#pragma warning restore 649
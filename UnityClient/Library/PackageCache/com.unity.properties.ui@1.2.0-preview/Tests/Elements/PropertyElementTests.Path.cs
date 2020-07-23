using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Unity.Properties.Tests;

namespace Unity.Properties.UI.Tests
{
    partial class PropertyElementTests
    {
        void AssertGetValueAtValidPath<TValue>(PropertyPath path, TValue value)
        {
            Assert.That(Element.GetValue<TValue>(path), Is.EqualTo(value));
            Assert.That(Element.TryGetValue<TValue>(path, out var v), Is.True);    
            Assert.That(v, Is.EqualTo(value));
        }
        
        void AssertGetValueAtInvalidPath<TValue>(PropertyPath path)
        {
            Assert.Throws<InvalidPathException>(() => Element.GetValue<TValue>(path));
            Assert.That(Element.TryGetValue<TValue>(path, out _), Is.False);
        }

        void AssertSetValueAtValidPath<TValue>(PropertyPath path, TValue value, TValue value2)
        {
            Element.SetValue(path, value);
            Assert.That(Element.GetValue<TValue>(path), Is.EqualTo(value));
            Assert.That(Element.TrySetValue(path, value2), Is.True);
            Assert.That(Element.GetValue<TValue>(path), Is.EqualTo(value2));
        }

        void AssertCannotSetAtInvalidPath<TValue>(PropertyPath path, TValue value)
        {
            Assert.Throws<InvalidPathException>(() => Element.SetValue(path, value));
            Assert.That(Element.TrySetValue(path, value), Is.False);
        }
        
        [Test]
        public void Value_CanGet_AtValidPath()
        {
            var container = GetContainer();
            Element.SetTarget(container);
            
            AssertGetValueAtValidPath(GetPath(nameof(ComplexUIContainer.FloatField)), 50.0f);
            AssertGetValueAtValidPath(GetPath(nameof(ComplexUIContainer.IntField)), 25);
            AssertGetValueAtValidPath(GetPath(nameof(ComplexUIContainer.StringField)), "Hey");
            AssertGetValueAtValidPath(GetPath(nameof(ComplexUIContainer.IntListField)), new List<int>
            {
                0, 1, 2, 3, 4
            });
            AssertGetValueAtValidPath(GetPath(nameof(ComplexUIContainer.IntIntDictionary)), new Dictionary<int, int>
            {
                {0, 0}, {1, 1}, {2, 2}, {3, 3}, {4, 4},
            });
        }
        
        [Test]
        public void Value_CannotGet_AtInvalidPath()
        {
            var container = GetContainer();
            Element.SetTarget(container);

            AssertGetValueAtInvalidPath<float>(GetPath("Hoy"));
            AssertGetValueAtInvalidPath<int>(GetListPath(nameof(ComplexUIContainer.IntListField), 14));
            AssertGetValueAtInvalidPath<int>(GetDictionaryPath(nameof(ComplexUIContainer.IntIntDictionary), 5));
        }
        
        [Test]
        public void Value_CannotGet_WithInvalidCast()
        {
            Element.SetTarget(GetContainer());
            Assert.Throws<InvalidCastException>(() => Element.GetValue<Vector3>(new PropertyPath(nameof(ComplexUIContainer.FloatField))));
            Assert.That(Element.TryGetValue<Vector3>(new PropertyPath(nameof(ComplexUIContainer.FloatField)), out _), Is.False);
        }

        [Test]
        public void Value_CanSet_AtValidPath()
        {
            var container = GetContainer();
            Element.SetTarget(container);

            AssertSetValueAtValidPath(GetPath(nameof(ComplexUIContainer.FloatField)), 150.0f, 250.0f);
            AssertSetValueAtValidPath(GetPath(nameof(ComplexUIContainer.IntField)), 213, 45);
            AssertSetValueAtValidPath(GetPath(nameof(ComplexUIContainer.StringField)), "Bloc", "Wut");
            AssertSetValueAtValidPath(GetPath(nameof(ComplexUIContainer.IntListField)),
                new List<int> {2, 4, 6, 8, 10},
                new List<int> {2, 4, 6, 8, 10, 12});

            AssertSetValueAtValidPath(GetPath(nameof(ComplexUIContainer.IntIntDictionary)),
                new Dictionary<int, int> { {0, 0}, {2, 6}, {4, 12}, {6, 18}, {8, 24} },
                new Dictionary<int, int> {{0, 0}, {2, 6}, {4, 12} });
        }

        [Test]
        public void Value_CannotSet_AtInvalidPath()
        {
            Element.SetTarget(GetContainer());
            
            AssertCannotSetAtInvalidPath(GetPath("Hoy"), 45);
            AssertCannotSetAtInvalidPath(GetListPath(nameof(ComplexUIContainer.IntListField), 14), 234);
            AssertCannotSetAtInvalidPath(GetDictionaryPath(nameof(ComplexUIContainer.IntIntDictionary), 5), 123);
        }
        
        [Test]
        public void Value_CannotSet_WithInvalidCast()
        {
            Element.SetTarget(GetContainer());
            Assert.Throws<InvalidCastException>(() => Element.SetValue(new PropertyPath(nameof(ComplexUIContainer.FloatField)), new PropertyPath("Hey")));
            Assert.That(Element.TrySetValue(new PropertyPath(nameof(ComplexUIContainer.FloatField)), new PropertyPath("Hey")), Is.False);
        }
    }
}
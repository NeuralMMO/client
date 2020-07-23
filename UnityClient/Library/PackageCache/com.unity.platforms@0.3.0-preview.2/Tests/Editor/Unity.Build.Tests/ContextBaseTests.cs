using NUnit.Framework;
using System;
using System.Linq;

namespace Unity.Build.Tests
{
    class ContextBaseTests : BuildTestsBase
    {
        class TestContextBase : ContextBase
        {
            public TestContextBase() : base() { }
            public TestContextBase(BuildPipelineBase pipeline, BuildConfiguration config) : base(pipeline, config) { }
        }

        class TestValueA { }
        class TestValueB { }

        [Test]
        public void HasValue()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            Assert.That(context.HasValue<TestValueA>(), Is.True);
            Assert.That(context.HasValue<TestValueB>(), Is.False);
        }

        [Test]
        public void GetValue()
        {
            var context = new TestContextBase();
            var value = new TestValueA();
            context.SetValue(value);
            Assert.That(context.GetValue<TestValueA>(), Is.EqualTo(value));
        }

        [Test]
        public void GetValue_WhenValueDoesNotExist_IsNull()
        {
            var context = new TestContextBase();
            Assert.That(context.GetValue<TestValueA>(), Is.Null);
        }

        [Test]
        public void GetOrCreateValue()
        {
            var context = new TestContextBase();
            Assert.That(context.GetOrCreateValue<TestValueA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestValueA>(), Is.True);
            Assert.That(context.GetValue<TestValueA>(), Is.Not.Null);
            Assert.That(context.Values.Length, Is.EqualTo(1));
        }

        [Test]
        public void GetOrCreateValue_WhenValueExist_DoesNotThrow()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            Assert.DoesNotThrow(() => context.GetOrCreateValue<TestValueA>());
        }

        [Test]
        public void GetValueOrDefault()
        {
            var context = new TestContextBase();
            Assert.That(context.GetValueOrDefault<TestValueA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestValueA>(), Is.False);
        }

        [Test]
        public void SetValue()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            context.SetValue<TestValueB>();
            Assert.That(context.HasValue<TestValueA>(), Is.True);
            Assert.That(context.GetValue<TestValueA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestValueB>(), Is.True);
            Assert.That(context.GetValue<TestValueB>(), Is.Not.Null);
            Assert.That(context.Values.Length, Is.EqualTo(2));
        }

        [Test]
        public void SetValue_SkipObjectType()
        {
            var context = new TestContextBase();
            Assert.DoesNotThrow(() => context.SetValue(new object()));
            Assert.That(context.Values.Length, Is.Zero);
        }

        [Test]
        public void SetValue_SkipNullValues()
        {
            var context = new TestContextBase();
            Assert.DoesNotThrow(() => context.SetValue<object>(null));
            Assert.That(context.Values.Length, Is.Zero);
        }

        [Test]
        public void SetValue_WhenValueExist_OverrideValue()
        {
            var context = new TestContextBase();
            var instance1 = new TestValueA();
            var instance2 = new TestValueA();

            context.SetValue(instance1);
            Assert.That(context.Values, Is.EqualTo(new[] { instance1 }));

            context.SetValue(instance2);
            Assert.That(context.Values, Is.EqualTo(new[] { instance2 }));
        }

        [Test]
        public void RemoveValue()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            Assert.That(context.Values.Length, Is.EqualTo(1));
            Assert.That(context.RemoveValue<TestValueA>(), Is.True);
            Assert.That(context.Values.Length, Is.Zero);
        }

        [Test]
        public void HasComponent()
        {
            var pipeline = new TestBuildPipelineWithComponents();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentA>());
            var context = new TestContextBase(pipeline, config);
            Assert.That(context.HasComponent<TestBuildComponentA>(), Is.True);
            Assert.That(context.HasComponent<TestBuildComponentB>(), Is.False);
            Assert.Throws<InvalidOperationException>(() => context.HasComponent<TestBuildComponentC>());
            Assert.Throws<ArgumentNullException>(() => context.HasComponent(null));
            Assert.Throws<InvalidOperationException>(() => context.HasComponent(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.HasComponent(typeof(TestBuildComponentInvalid)));
        }

        [Test]
        public void IsComponentInherited()
        {
            var pipeline = new TestBuildPipelineWithComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentB>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);

            Assert.That(context.IsComponentInherited<TestBuildComponentA>(), Is.False);
            Assert.That(context.IsComponentInherited<TestBuildComponentB>(), Is.True);
            Assert.Throws<InvalidOperationException>(() => context.IsComponentInherited<TestBuildComponentC>());

            Assert.Throws<ArgumentNullException>(() => context.IsComponentInherited(null));
            Assert.Throws<InvalidOperationException>(() => context.IsComponentInherited(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.IsComponentInherited(typeof(TestBuildComponentInvalid)));
        }

        [Test]
        public void IsComponentOverridden()
        {
            var pipeline = new TestBuildPipelineWithComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentA>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.SetComponent<TestBuildComponentB>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);

            Assert.That(context.IsComponentOverridden<TestBuildComponentA>(), Is.True);
            Assert.That(context.IsComponentOverridden<TestBuildComponentB>(), Is.False);
            Assert.Throws<InvalidOperationException>(() => context.IsComponentOverridden<TestBuildComponentC>());

            Assert.Throws<ArgumentNullException>(() => context.IsComponentOverridden(null));
            Assert.Throws<InvalidOperationException>(() => context.IsComponentOverridden(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.IsComponentOverridden(typeof(TestBuildComponentInvalid)));
        }

        [Test]
        public void TryGetComponent()
        {
            var pipeline = new TestBuildPipelineWithComponents();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentA>());
            var context = new TestContextBase(pipeline, config);
            Assert.That(context.TryGetComponent<TestBuildComponentA>(out _), Is.True);
            Assert.That(context.TryGetComponent<TestBuildComponentB>(out _), Is.False);
            Assert.Throws<InvalidOperationException>(() => context.TryGetComponent<TestBuildComponentC>(out _));
            Assert.Throws<ArgumentNullException>(() => context.TryGetComponent(null, out _));
            Assert.Throws<InvalidOperationException>(() => context.TryGetComponent(typeof(object), out _));
            Assert.Throws<InvalidOperationException>(() => context.TryGetComponent(typeof(TestBuildComponentInvalid), out _));
        }

        [Test]
        public void GetComponentOrDefault()
        {
            var pipeline = new TestBuildPipelineWithComponents();
            var config = BuildConfiguration.CreateInstance();
            var context = new TestContextBase(pipeline, config);
            Assert.That(context.HasComponent<TestBuildComponentA>(), Is.False);
            Assert.That(context.GetComponentOrDefault<TestBuildComponentA>(), Is.Not.Null);
            Assert.Throws<InvalidOperationException>(() => context.GetComponentOrDefault<TestBuildComponentC>());
            Assert.Throws<ArgumentNullException>(() => context.GetComponentOrDefault(null));
            Assert.Throws<InvalidOperationException>(() => context.GetComponentOrDefault(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.GetComponentOrDefault(typeof(TestBuildComponentInvalid)));
        }

        [Test]
        public void GetComponents()
        {
            var pipeline = new TestBuildPipelineWithComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentB>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);

            var components = context.GetComponents();
            Assert.That(components.Count, Is.EqualTo(2));
            Assert.That(components.Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestBuildComponentA), typeof(TestBuildComponentB) }));

            configA.SetComponent<TestBuildComponentC>();
            Assert.Throws<InvalidOperationException>(() => context.GetComponents());
        }

        [Test]
        public void GetComponents_WithType()
        {
            var pipeline = new TestBuildPipelineWithComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentB>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);

            var components = context.GetComponents<TestBuildComponentA>();
            Assert.That(components.Count, Is.EqualTo(1));
            Assert.That(components.Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestBuildComponentA) }));
            Assert.Throws<InvalidOperationException>(() => context.GetComponents<TestBuildComponentC>());

            configA.SetComponent<TestBuildComponentC>();
            Assert.Throws<InvalidOperationException>(() => context.GetComponents<TestBuildComponentC>());
        }

        [Test]
        public void GetComponentTypes()
        {
            var pipeline = new TestBuildPipelineWithComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentB>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);
            Assert.That(context.GetComponentTypes(), Is.EquivalentTo(new[] { typeof(TestBuildComponentA), typeof(TestBuildComponentB) }));

            configA.SetComponent<TestBuildComponentC>();
            Assert.Throws<InvalidOperationException>(() => context.GetComponentTypes());
        }
    }
}

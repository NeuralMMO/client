using NUnit.Framework;
using System;
using System.Linq;

namespace Unity.Build.Tests
{
    [TestFixture]
    class BuildStepCollectionTests : BuildTestsBase
    {
        [Test]
        public void Constructor()
        {
            var steps = new BuildStepCollection(typeof(TestBuildStepA), typeof(TestBuildStepB));
            Assert.That(steps.Select(step => step.GetType()), Is.EqualTo(new[] { typeof(TestBuildStepA), typeof(TestBuildStepB) }));
        }

        [Test]
        public void Constructor_FromTypeArrayImplicitConversion()
        {
            BuildStepCollection steps = new[]
            {
                typeof(TestBuildStepA),
                typeof(TestBuildStepB)
            };
            Assert.That(steps.Select(step => step.GetType()), Is.EqualTo(new[] { typeof(TestBuildStepA), typeof(TestBuildStepB) }));
        }

        [Test]
        public void Constructor_WithInvalidTypes_Throws()
        {
            Assert.Throws<ArgumentException>(() => new BuildStepCollection(typeof(TestBuildStepInvalid)));
        }

        [Test]
        public void Run_Succeeds()
        {
            BuildStepCollection steps = new[]
            {
                typeof(TestBuildStepA),
                typeof(TestBuildStepB)
            };

            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance();
            using (var context = new BuildContext(pipeline, config))
            {
                Assert.That(steps.Run(context).Succeeded, Is.True);
            }
        }

        [Test]
        public void Run_WithBuildStepFailure_Fails()
        {
            BuildStepCollection steps = new[]
            {
                typeof(TestBuildStepA),
                typeof(TestBuildStepFails),
                typeof(TestBuildStepB)
            };

            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance();
            using (var context = new BuildContext(pipeline, config))
            {
                Assert.That(steps.Run(context).Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithBuildStepException_Fails()
        {
            BuildStepCollection steps = new[]
            {
                typeof(TestBuildStepA),
                typeof(TestBuildStepThrows),
                typeof(TestBuildStepB)
            };

            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance();
            using (var context = new BuildContext(pipeline, config))
            {
                Assert.That(steps.Run(context).Succeeded, Is.False);
            }
        }
    }
}

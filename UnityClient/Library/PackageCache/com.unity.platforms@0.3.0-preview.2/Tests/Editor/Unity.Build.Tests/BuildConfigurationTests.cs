using NUnit.Framework;

namespace Unity.Build.Tests
{
    class BuildConfigurationTests : BuildTestsBase
    {
        [Test]
        public void CreateAsset()
        {
            const string assetPath = "Assets/" + nameof(BuildConfigurationTests) + BuildConfiguration.AssetExtension;
            Assert.That(BuildConfiguration.CreateAsset(assetPath), Is.Not.Null);
            UnityEditor.AssetDatabase.DeleteAsset(assetPath);
        }

        [Test]
        public void GetBuildPipeline_IsEqualToPipeline()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.GetBuildPipeline(), Is.EqualTo(pipeline));
        }

        [Test]
        public void GetBuildPipeline_WithoutPipeline_IsNull()
        {
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.GetBuildPipeline(), Is.Null);
        }

        [Test]
        public void CanBuild_IsTrue()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.CanBuild().Result, Is.True);
        }

        [Test]
        public void CanBuild_WithoutPipeline_IsFalse()
        {
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.CanBuild().Result, Is.False);
        }

        [Test]
        public void CanBuild_WhenCannotBuild_IsFalse()
        {
            var pipeline = new TestBuildPipelineCantBuild();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.CanBuild().Result, Is.False);
        }

        [Test]
        public void Build_Succeeds()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);
        }

        [Test]
        public void Build_WithoutPipeline_Fails()
        {
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.Build().Succeeded, Is.False);
        }

        [Test]
        public void Build_WhenCannotBuild_IsFalse()
        {
            var pipeline = new TestBuildPipelineCantBuild();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.False);
        }

        [Test]
        public void Build_WhenBuildFails_Fails()
        {
            var pipeline = new TestBuildPipelineBuildFails();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.False);
        }

        [Test]
        public void Build_WhenBuildThrows_Fails()
        {
            var pipeline = new TestBuildPipelineBuildThrows();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.False);
        }

        [Test]
        public void CanRun_IsTrue()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);
            Assert.That(config.CanRun().Result, Is.True);
        }

        [Test]
        public void CanRun_WithoutBuild_IsFalse()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.CanRun().Result, Is.False);
        }

        [Test]
        public void CanRun_WithFailedBuild_IsFalse()
        {
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.Build().Succeeded, Is.False);
            Assert.That(config.CanRun().Result, Is.False);
        }

        [Test]
        public void CanRun_WithoutPipeline_IsFalse()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            config.RemoveComponent<TestBuildPipelineComponent>();
            Assert.That(config.CanRun().Result, Is.False);
        }

        [Test]
        public void CanRun_WhenCannotRun_IsFalse()
        {
            var pipeline = new TestBuildPipelineCantRun();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);
            Assert.That(config.CanRun().Result, Is.False);
        }

        [Test]
        public void Run_Succeeds()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.True);
            }
        }

        [Test]
        public void Run_WithoutBuild_Fails()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithFailedBuild_Fails()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.Build().Succeeded, Is.False);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithoutPipeline_Fails()
        {
            var pipeline = new TestBuildPipeline();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            config.RemoveComponent<TestBuildPipelineComponent>();
            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenCannotRun_IsFalse()
        {
            var pipeline = new TestBuildPipelineCantRun();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunFails_Fails()
        {
            var pipeline = new TestBuildPipelineRunFails();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunThrows_Fails()
        {
            var pipeline = new TestBuildPipelineRunThrows();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }
    }
}

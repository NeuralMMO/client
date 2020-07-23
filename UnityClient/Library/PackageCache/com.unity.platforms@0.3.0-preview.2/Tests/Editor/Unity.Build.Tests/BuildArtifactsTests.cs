using NUnit.Framework;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.TestTools;

namespace Unity.Build.Tests
{
    class BuildArtifactsTests : BuildTestsBase
    {
        BuildPipelineBase m_BuildPipeline;
        BuildConfiguration m_BuildConfiguration;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_BuildPipeline = new TestBuildPipeline();
            m_BuildConfiguration = BuildConfiguration.CreateInstance(config => config.name = "TestConfiguration");
        }

        [Test]
        public void GetBuildArtifact_IsValid()
        {
            var result = BuildResult.Success(m_BuildPipeline, m_BuildConfiguration);
            BuildArtifacts.Store(result, new[] { new TestBuildArtifactA() });
            Assert.That(File.Exists(BuildArtifacts.GetArtifactPath(m_BuildConfiguration)), Is.True);
            Assert.That(BuildArtifacts.GetBuildArtifact<TestBuildArtifactA>(m_BuildConfiguration), Is.Not.Null);
        }

        [Test]
        public void GetBuildArtifact_WithoutBuildArtifacts_IsNull()
        {
            var result = BuildResult.Success(m_BuildPipeline, m_BuildConfiguration);
            BuildArtifacts.Store(result, new IBuildArtifact[] { });
            Assert.That(File.Exists(BuildArtifacts.GetArtifactPath(m_BuildConfiguration)), Is.True);
            Assert.That(BuildArtifacts.GetBuildArtifact<TestBuildArtifactA>(m_BuildConfiguration), Is.Null);
        }

        [Test]
        public void GetBuildArtifact_WithBuildArtifactTypeNotFound_IsNull()
        {
            var result = BuildResult.Success(m_BuildPipeline, m_BuildConfiguration);
            BuildArtifacts.Store(result, new[] { new TestBuildArtifactA() });
            Assert.That(File.Exists(BuildArtifacts.GetArtifactPath(m_BuildConfiguration)), Is.True);
            Assert.That(BuildArtifacts.GetBuildArtifact<TestBuildArtifactB>(m_BuildConfiguration), Is.Null);
        }

        [Test]
        public void GetBuildArtifact_WithoutBuildConfiguration_IsNull()
        {
            Assert.That(BuildArtifacts.GetBuildArtifact<IBuildArtifact>(null), Is.Null);
        }

        [Test]
        public void GetBuildArtifact_WithNullType_IsNull()
        {
            Assert.That(BuildArtifacts.GetBuildArtifact(m_BuildConfiguration, null), Is.Null);
        }

        [Test]
        public void GetBuildArtifact_WithInvalidType_IsNull()
        {
            Assert.That(BuildArtifacts.GetBuildArtifact(m_BuildConfiguration, typeof(TestBuildArtifactInvalid)), Is.Null);
        }

        [Test]
        public void GetBuildArtifact_FileDeleted_IsNull()
        {
            var result = BuildResult.Success(m_BuildPipeline, m_BuildConfiguration);
            BuildArtifacts.Store(result, new[] { new TestBuildArtifactA() });

            var artifactPath = BuildArtifacts.GetArtifactPath(m_BuildConfiguration);
            Assert.That(File.Exists(artifactPath), Is.True);

            File.Delete(artifactPath);
            Assert.That(BuildArtifacts.GetBuildArtifact<TestBuildArtifactA>(m_BuildConfiguration), Is.Null);
        }

        [Test]
        public void GetBuildArtifact_NoCache_IsValid()
        {
            var file = new DirectoryInfo(BuildArtifacts.BaseDirectory).GetFile($"{m_BuildConfiguration.name}.json");
            file.WriteAllText($"{{ \"Result\": null, \"Artifacts\": [{{ \"$type\": {typeof(TestBuildArtifactB).GetQualifedAssemblyTypeName().DoubleQuotes()} }}] }}");
            Assert.That(BuildArtifacts.GetBuildArtifact<TestBuildArtifactB>(m_BuildConfiguration), Is.Not.Null);
        }

        [Test]
        public void GetBuildArtifact_NoCache_WithDeserializeError_IsNull()
        {
            var file = new DirectoryInfo(BuildArtifacts.BaseDirectory).GetFile($"{m_BuildConfiguration.name}.json");
            file.WriteAllText("{ \"Result\": null, \"Artifacts\": [{ \"$type\": \"Some.Unknown.Assembly.Type, Some.Unknown.Assembly\" }] }");
            LogAssert.Expect(UnityEngine.LogType.Error, new Regex(@"Failed to deserialize.*:\n.*"));
            Assert.That(BuildArtifacts.GetBuildArtifact<TestBuildArtifactB>(m_BuildConfiguration), Is.Null);
        }

        [Test]
        public void GetBuildArtifact_NoCache_WithDeserializeException_IsNull()
        {
            new DirectoryInfo(BuildArtifacts.BaseDirectory).GetFile($"{m_BuildConfiguration.name}.json").WriteAllText("12345");
            LogAssert.Expect(UnityEngine.LogType.Error, new Regex(@"Failed to deserialize.*:\n.*"));
            Assert.That(BuildArtifacts.GetBuildArtifact<TestBuildArtifactB>(m_BuildConfiguration), Is.Null);
        }

        [Test]
        public void GetBuildResult_IsValid()
        {
            BuildArtifacts.Store(BuildResult.Success(m_BuildPipeline, m_BuildConfiguration), new IBuildArtifact[] { });
            var result = BuildArtifacts.GetBuildResult(m_BuildConfiguration);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Succeeded, Is.True);
        }

        [Test]
        public void GetBuildResult_WithoutBuildConfiguration_IsNull()
        {
            BuildArtifacts.Store(BuildResult.Success(m_BuildPipeline, m_BuildConfiguration), new IBuildArtifact[] { });
            var result = BuildArtifacts.GetBuildResult(null);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Store_WithoutBuildResult_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => BuildArtifacts.Store(null, new IBuildArtifact[] { }));
        }

        [Test]
        public void Store_WithoutBuildConfiguration_Throws()
        {
            var result = BuildResult.Success(m_BuildPipeline, null);
            Assert.Throws<ArgumentNullException>(() => BuildArtifacts.Store(result, new IBuildArtifact[] { }));
        }

        [Test]
        public void Store_WithoutBuildArtifactsArray_Throws()
        {
            var result = BuildResult.Success(m_BuildPipeline, m_BuildConfiguration);
            Assert.Throws<ArgumentNullException>(() => BuildArtifacts.Store(result, null));
        }
    }
}

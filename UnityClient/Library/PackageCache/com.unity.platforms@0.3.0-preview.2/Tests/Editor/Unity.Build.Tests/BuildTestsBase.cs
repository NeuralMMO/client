using NUnit.Framework;
using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Build.Tests
{
    class BuildTestsBase
    {
        [HideInInspector]
        protected class TestBuildPipelineComponent : IBuildPipelineComponent
        {
            [CreateProperty] public BuildPipelineBase Pipeline { get; set; }
            public int SortingIndex => 0;
            public bool SetupEnvironment() => false;
        }

        [HideInInspector]
        protected class TestBuildComponentA : IBuildComponent { }

        [HideInInspector]
        protected class TestBuildComponentB : IBuildComponent { }

        [HideInInspector]
        protected class TestBuildComponentC : IBuildComponent { }

        protected class TestBuildComponentInvalid { }

        [HideInInspector]
        protected class TestBuildPipeline : BuildPipelineBase
        {
            protected override BuildResult OnBuild(BuildContext context) => context.Success();
            protected override RunResult OnRun(RunContext context) => context.Success(new TestRunInstance());
        }

        [HideInInspector]
        protected class TestBuildPipelineCantBuild : BuildPipelineBase
        {
            protected override BoolResult OnCanBuild(BuildContext context) => BoolResult.False(nameof(TestBuildPipelineCantBuild));
            protected override BuildResult OnBuild(BuildContext context) => context.Success();
            protected override RunResult OnRun(RunContext context) => context.Success(new TestRunInstance());
        }

        [HideInInspector]
        protected class TestBuildPipelineBuildFails : BuildPipelineBase
        {
            protected override BuildResult OnBuild(BuildContext context) => context.Failure(nameof(TestBuildPipelineBuildFails));
            protected override RunResult OnRun(RunContext context) => context.Success(new TestRunInstance());
        }

        [HideInInspector]
        protected class TestBuildPipelineBuildThrows : BuildPipelineBase
        {
            protected override BuildResult OnBuild(BuildContext context) => throw new InvalidOperationException(nameof(TestBuildPipelineBuildThrows));
            protected override RunResult OnRun(RunContext context) => context.Success(new TestRunInstance());
        }

        [HideInInspector]
        protected class TestBuildPipelineNullBuildResult : BuildPipelineBase
        {
            protected override BuildResult OnBuild(BuildContext context) => null;
            protected override RunResult OnRun(RunContext context) => context.Success(new TestRunInstance());
        }

        [HideInInspector]
        protected class TestBuildPipelineCantRun : BuildPipelineBase
        {
            protected override BuildResult OnBuild(BuildContext context) => context.Success();
            protected override BoolResult OnCanRun(RunContext context) => BoolResult.False(nameof(TestBuildPipelineCantRun));
            protected override RunResult OnRun(RunContext context) => context.Success(new TestRunInstance());
        }

        [HideInInspector]
        protected class TestBuildPipelineRunFails : BuildPipelineBase
        {
            protected override BuildResult OnBuild(BuildContext context) => context.Success();
            protected override RunResult OnRun(RunContext context) => context.Failure(nameof(TestBuildPipelineCantRun));
        }

        [HideInInspector]
        protected class TestBuildPipelineRunThrows : BuildPipelineBase
        {
            protected override BuildResult OnBuild(BuildContext context) => context.Success();
            protected override RunResult OnRun(RunContext context) => throw new InvalidOperationException(nameof(TestBuildPipelineRunThrows));
        }

        [HideInInspector]
        protected class TestBuildPipelineNullRunResult : BuildPipelineBase
        {
            protected override BuildResult OnBuild(BuildContext context) => context.Success();
            protected override RunResult OnRun(RunContext context) => null;
        }

        [HideInInspector]
        protected class TestBuildPipelineWithComponents : BuildPipelineBase
        {
            public override Type[] UsedComponents { get; } =
            {
                typeof(TestBuildComponentA),
                typeof(TestBuildComponentB)
            };

            protected override BuildResult OnBuild(BuildContext context)
            {
                context.GetComponentOrDefault<TestBuildComponentA>();
                context.GetComponentOrDefault<TestBuildComponentB>();
                return context.Success();
            }

            protected override RunResult OnRun(RunContext context)
            {
                context.GetComponentOrDefault<TestBuildComponentA>();
                context.GetComponentOrDefault<TestBuildComponentB>();
                return context.Success(new TestRunInstance());
            }
        }

        [HideInInspector]
        protected class TestBuildPipelineWithMissingComponents : BuildPipelineBase
        {
            protected override BuildResult OnBuild(BuildContext context)
            {
                context.GetComponentOrDefault<TestBuildComponentA>();
                context.GetComponentOrDefault<TestBuildComponentB>();
                return context.Success();
            }

            protected override RunResult OnRun(RunContext context)
            {
                context.GetComponentOrDefault<TestBuildComponentA>();
                context.GetComponentOrDefault<TestBuildComponentB>();
                return context.Success(new TestRunInstance());
            }
        }

        [HideInInspector]
        protected class TestBuildPipelineWithInvalidComponents : BuildPipelineBase
        {
            public override Type[] UsedComponents { get; } = { typeof(TestBuildComponentInvalid) };
            protected override BuildResult OnBuild(BuildContext context) => context.Success();
            protected override RunResult OnRun(RunContext context) => context.Success(new TestRunInstance());
        }

        protected class TestBuildStepA : BuildStepBase
        {
            public override BuildResult Run(BuildContext context) => context.Success();
        }

        protected class TestBuildStepB : BuildStepBase
        {
            public override BuildResult Run(BuildContext context) => context.Success();
        }

        protected class TestBuildStepFails : BuildStepBase
        {
            public override BuildResult Run(BuildContext context) => context.Failure(nameof(TestBuildStepFails));
        }

        protected class TestBuildStepThrows : BuildStepBase
        {
            public override BuildResult Run(BuildContext context) => throw new InvalidOperationException(nameof(TestBuildStepThrows));
        }

        protected class TestBuildStepInvalid { }

        protected class TestRunInstance : IRunInstance
        {
            public TestRunInstance()
            {
                IsRunning = true;
            }

            public bool IsRunning { get; private set; }
            public void Dispose() { IsRunning = false; }
        }

        protected class TestBuildArtifactA : IBuildArtifact { }

        protected class TestBuildArtifactB : IBuildArtifact { }

        protected class TestBuildArtifactInvalid { }

        [SetUp]
        public void SetUp()
        {
            BuildArtifacts.Clean();
        }

        [TearDown]
        public void TearDown()
        {
            BuildArtifacts.Clean();
        }
    }
}

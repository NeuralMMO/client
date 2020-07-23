using System.IO;

namespace Unity.Build.Classic.Private
{
    public abstract class ClassicNonIncrementalPipelineBase : ClassicPipelineBase
    {
        protected override BuildResult OnBuild(BuildContext context)
        {
            PrepareContext(context);
            return BuildSteps.Run(context);
        }

        protected override void PrepareContext(BuildContext context)
        {
            base.PrepareContext(context);

            var nonIncrementalClassicData = context.GetOrCreateValue<NonIncrementalClassicSharedData>();
            nonIncrementalClassicData.TemporaryDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Temp", context.BuildConfigurationName);
            // Cleanup temporary directory
            if (Directory.Exists(nonIncrementalClassicData.TemporaryDirectory))
                Directory.Delete(nonIncrementalClassicData.TemporaryDirectory, true);
            Directory.CreateDirectory(nonIncrementalClassicData.TemporaryDirectory);
        }
    }
}

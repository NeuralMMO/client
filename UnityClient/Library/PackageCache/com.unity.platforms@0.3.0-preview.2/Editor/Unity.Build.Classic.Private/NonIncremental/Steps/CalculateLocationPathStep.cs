using System.IO;
using Unity.Build.Common;

namespace Unity.Build.Classic.Private
{
    internal class LocationInfo
    {
        internal string Path { get; set; }
    }

    abstract class CalculateLocationPathStep : BuildStepBase
    {
        protected string CalculateDefaultLocationPath(BuildContext context)
        {
            var productName = context.GetComponentOrDefault<GeneralSettings>().ProductName;
            var target = context.GetValue<ClassicSharedData>().BuildTarget;
            var extension = ClassicBuildProfile.GetExecutableExtension(target);
            var outputPath = context.GetOutputBuildDirectory();
            return Path.Combine(outputPath, productName + extension);
        }

        protected abstract string CalculatePath(BuildContext context);

        public override BuildResult Run(BuildContext context)
        {
            context.SetValue(new LocationInfo() { Path = CalculatePath(context) });
            return context.Success();
        }
    }
}

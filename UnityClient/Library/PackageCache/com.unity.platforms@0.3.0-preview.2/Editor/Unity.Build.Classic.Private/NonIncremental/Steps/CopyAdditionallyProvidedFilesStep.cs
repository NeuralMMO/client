using NiceIO;

namespace Unity.Build.Classic.Private
{
    sealed class CopyAdditionallyProvidedFilesStep : BuildStepBase
    {
        public override BuildResult Run(BuildContext context)
        {
            var classicSharedData = context.GetValue<ClassicSharedData>();

            foreach (var customizer in classicSharedData.Customizers)
            {
                customizer.RegisterAdditionalFilesToDeploy((from, to) =>
                {
                    new NPath(from).MakeAbsolute().Copy(new NPath(to).MakeAbsolute().EnsureParentDirectoryExists());
                });
            }
            return context.Success();
        }
    }
}

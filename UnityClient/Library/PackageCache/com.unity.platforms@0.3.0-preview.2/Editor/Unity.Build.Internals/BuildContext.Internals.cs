namespace Unity.Build.Internals
{
    internal static class BuildContextInternals
    {
        internal static BuildConfiguration GetBuildConfiguration(BuildContext context)
        {
            return context.BuildConfiguration;
        }
    }
}

using UnityEditor;

namespace Unity.Build.Classic.Private
{
    sealed class SwitchPlatfomStep : BuildStepBase
    {
        public override BuildResult Run(BuildContext context)
        {
            var target = context.GetValue<ClassicSharedData>().BuildTarget;
            if (target == BuildTarget.NoTarget)
            {
                return context.Failure($"Invalid build target '{target.ToString()}'.");
            }

            if (EditorUserBuildSettings.activeBuildTarget == target)
            {
                return context.Success();
            }

            if (EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(target), target))
            {
                return context.Failure("Editor's active Build Target needed to be switched. Please wait for switch to complete and then build again.");
            }
            else
            {
                return context.Failure($"Editor's active Build Target could not be switched. Look in the console or the editor log for additional errors.");
            }
        }
    }
}

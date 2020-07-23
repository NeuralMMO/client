using UnityEditor.SceneManagement;

namespace Unity.Build.Classic.Private
{
    sealed class SaveScenesAndAssetsStep : BuildStepBase
    {
        public override BuildResult Run(BuildContext context)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return context.Failure($"All Scenes and Assets must be saved before a build can be started.");
            }
            return context.Success();
        }
    }
}

using System;
using Unity.Build.Common;
using UnityEditor;

namespace Unity.Build.Classic.Private
{
    sealed class ApplyUnitySettingsStep : BuildStepBase
    {
        public override Type[] UsedComponents { get; } =
        {
            typeof(GeneralSettings),
            typeof(ClassicScriptingSettings)
        };

        public override BuildResult Run(BuildContext context)
        {
            var backups = new UnitySettingsState[]
            {
                new UnitySettingsState(UnitySettingsState.PlayerSettingsAsset),
                new UnitySettingsState(UnitySettingsState.EditorUserBuildSettingsAsset)
            };
            context.SetValue(backups);

            var serializedObject = new SerializedObject(UnitySettingsState.PlayerSettingsAsset);
            var generalSettings = context.GetComponentOrDefault<GeneralSettings>();
            var scriptingSettings = context.GetComponentOrDefault<ClassicScriptingSettings>();
            var targetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(context.GetValue<ClassicSharedData>().BuildTarget);

            // Get serialized properties for things which don't have API exposed
            SerializedProperty gcIncremental;
            var result = FindProperty(context, serializedObject, nameof(gcIncremental), out gcIncremental);
            if (result.Failed)
                return result;

            PlayerSettings.productName = generalSettings.ProductName;
            PlayerSettings.companyName = generalSettings.CompanyName;

            // Scripting Settings
            PlayerSettings.SetScriptingBackend(targetGroup, scriptingSettings.ScriptingBackend);
            PlayerSettings.SetIl2CppCompilerConfiguration(targetGroup, scriptingSettings.Il2CppCompilerConfiguration);
            gcIncremental.boolValue = scriptingSettings.UseIncrementalGC;

            foreach (var b in backups)
            {
                EditorUtility.ClearDirty(b.Target);
            }

            return context.Success();
        }

        public override BuildResult Cleanup(BuildContext context)
        {
            var backups = context.GetValue<UnitySettingsState[]>();
            foreach (var b in backups)
            {
                b.Restore();
            }
            return context.Success();
        }

        BuildResult FindProperty(BuildContext context, SerializedObject serializedObject, string name, out SerializedProperty serializedProperty)
        {
            serializedProperty = serializedObject.FindProperty(name);
            if (serializedProperty == null)
            {
                return context.Failure($"Failed to find: {name}");
            }
            return context.Success();
        }
    }
}

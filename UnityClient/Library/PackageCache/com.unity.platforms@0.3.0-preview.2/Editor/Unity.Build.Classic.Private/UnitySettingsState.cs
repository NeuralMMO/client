using System;
using UnityEditor;

namespace Unity.Build.Classic.Private
{
    sealed class UnitySettingsState
    {
        public UnityEngine.Object Target { private set; get; }
        public string Contents { private set; get; }
        public bool IsDirty { private set; get; }
        public UnitySettingsState(UnityEngine.Object _target)
        {
            if (_target == null)
                throw new NullReferenceException(nameof(_target));
            Target = _target;
            Contents = EditorJsonUtility.ToJson(_target);
            IsDirty = EditorUtility.GetDirtyCount(_target) > 0;
        }

        public void Restore()
        {
            // Note: EditorJsonUtility.FromJsonOverwrite doesn't dirty settings
            EditorJsonUtility.FromJsonOverwrite(Contents, Target);
            if (IsDirty)
                EditorUtility.SetDirty(Target);
        }

        public static PlayerSettings PlayerSettingsAsset => AssetDatabase.LoadAssetAtPath<PlayerSettings>("ProjectSettings/ProjectSettings.asset");
        // TODO: AssetDatabase.LoadAssetAtPath can't load from Library?
        public static EditorUserBuildSettings EditorUserBuildSettingsAsset => UnityEngine.Resources.FindObjectsOfTypeAll<EditorUserBuildSettings>()[0];// AssetDatabase.LoadAssetAtPath<EditorUserBuildSettings>("Library/EditorUserBuildSettings.asset");
    }
}

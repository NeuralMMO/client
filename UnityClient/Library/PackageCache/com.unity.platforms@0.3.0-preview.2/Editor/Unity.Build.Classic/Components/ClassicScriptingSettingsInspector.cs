using Unity.Properties.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Build.Classic
{
    sealed class ClassicScriptingSettingsInspector : Inspector<ClassicScriptingSettings>
    {
        EnumField m_ScriptingBackend;
        VisualElement m_Il2CppCompilerConfiguration;

        public override VisualElement Build()
        {
            var root = new VisualElement();
            DoDefaultGui(root, nameof(ClassicScriptingSettings.ScriptingBackend));
            DoDefaultGui(root, nameof(ClassicScriptingSettings.Il2CppCompilerConfiguration));
            DoDefaultGui(root, nameof(ClassicScriptingSettings.UseIncrementalGC));

            m_ScriptingBackend = root.Q<EnumField>(nameof(ClassicScriptingSettings.ScriptingBackend));
            m_Il2CppCompilerConfiguration = root.Q<VisualElement>(nameof(ClassicScriptingSettings.Il2CppCompilerConfiguration));

            return root;
        }

        public override void Update()
        {
            m_Il2CppCompilerConfiguration.SetEnabled((ScriptingImplementation)m_ScriptingBackend.value == ScriptingImplementation.IL2CPP);
        }
    }
}

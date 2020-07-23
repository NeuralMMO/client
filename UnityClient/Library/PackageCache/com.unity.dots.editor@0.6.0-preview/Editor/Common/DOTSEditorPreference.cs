using JetBrains.Annotations;
using Unity.Properties;

namespace Unity.Entities.Editor
{
    [DOTSEditorPreferencesSetting(Constants.Settings.AdvancedSettings), InternalSetting, UsedImplicitly]
    class AdvancedSettings : ISetting
    {
        public bool ShowAdvancedWorlds;

        public void OnSettingChanged(PropertyPath path)
        {
        }
    }
}

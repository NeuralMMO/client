using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    internal static class Assets
    {
        public static StyleSheet LoadStyleSheet(string name)
        {
            return Package.LoadAsset<StyleSheet>($"Editor/{typeof(StylesUtility).Namespace}/uss/{name}.uss");
        }

        public static VisualTreeAsset LoadVisualTreeAsset(string name)
        {
            return Package.LoadAsset<VisualTreeAsset>($"Editor/{typeof(StylesUtility).Namespace}/uxml/{name}.uxml");
        }
    }
}

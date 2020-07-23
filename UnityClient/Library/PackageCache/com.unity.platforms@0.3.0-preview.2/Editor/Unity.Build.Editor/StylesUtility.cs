using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    internal static class StylesUtility
    {
        public static void AddStyleSheetAndVariant(this VisualElement ve, string styleSheetName)
        {
            ve.styleSheets.Add(Assets.LoadStyleSheet(styleSheetName));
            ve.styleSheets.Add(Assets.LoadStyleSheet($"{styleSheetName}_{(EditorGUIUtility.isProSkin ? "dark" : "light")}"));
        }
    }
}

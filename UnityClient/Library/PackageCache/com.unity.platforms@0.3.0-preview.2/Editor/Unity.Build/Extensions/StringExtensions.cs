using System.IO;
using Unity.Build.Bridge;
using UnityEditor;

namespace Unity.Build
{
    internal static class StringExtensions
    {
        const string k_FilePath = "filePath";

        [InitializeOnLoadMethod]
        static void Register()
        {
            EditorGUIBridge.HyperLinkClicked += (args) =>
            {
                if (args.TryGetValue(k_FilePath, out var assetPath) && !string.IsNullOrEmpty(assetPath))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    if (obj != null && obj)
                    {
                        Selection.objects = new[] { obj };
                    }
                }
            };
        }

        public static string TrimStart(this string str, string value)
        {
            var index = str.IndexOf(value);
            return index >= 0 ? str.Substring(index + value.Length) : str;
        }

        public static string ToForwardSlash(this string value)
        {
            return value.Replace('\\', '/');
        }

        public static string SingleQuotes(this string value)
        {
            return "'" + value.Trim('\'') + "'";
        }

        public static string DoubleQuotes(this string value)
        {
            return '"' + value.Trim('"') + '"';
        }

        public static string ToHyperLink(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (File.Exists(value))
            {
                return $"<a {k_FilePath}={value.DoubleQuotes()}>{value}</a>";
            }
            else
            {
                return $"<a>{value}</a>";
            }
        }
    }
}

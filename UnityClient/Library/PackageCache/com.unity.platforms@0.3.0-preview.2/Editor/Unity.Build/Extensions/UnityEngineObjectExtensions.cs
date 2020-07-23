using Unity.Build.Bridge;
using UnityEditor;

namespace Unity.Build
{
    internal static class UnityEngineObjectExtensions
    {
        const string k_InstanceID = "instanceID";

        [InitializeOnLoadMethod]
        static void Register()
        {
            EditorGUIBridge.HyperLinkClicked += (args) =>
            {
                if (args.TryGetValue(k_InstanceID, out var value) && int.TryParse(value, out var instanceID) && instanceID != 0)
                {
                    var obj = EditorUtility.InstanceIDToObject(instanceID);
                    if (obj != null && obj)
                    {
                        Selection.objects = new[] { obj };
                    }
                }
            };
        }

        public static string ToHyperLink(this UnityEngine.Object obj)
        {
            if (string.IsNullOrEmpty(obj.name))
            {
                return string.Empty;
            }
            return $"<a {k_InstanceID}=\"{obj.GetInstanceID()}\">{obj.name}</a>";
        }
    }
}

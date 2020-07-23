using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    class SearchExpressionAsset : ScriptableObject
    {
        [MenuItem("Assets/Create/Quick Search/Expression")]
        internal static void CreateIndexProject()
        {
            var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (File.Exists(folderPath))
                folderPath = Path.GetDirectoryName(folderPath);

            var expressionPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, "expression.qse"));
            var newExpression = new SearchExpression(SearchFlags.Default);
            newExpression.Save(expressionPath);
            AssetDatabase.ImportAsset(expressionPath);
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(expressionPath);
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Unity.QuickSearch
{
    [CustomEditor(typeof(SearchExpressionImporter), editorForChildClasses: false, isFallback = false)]
    class SearchExpressionEditor : Editor
    {
        private SearchVariable[] m_Variables = new SearchVariable[0];
        private SearchExpression m_Expression;
        private ExpressionResultView m_ExpressionResultView;
        private GUIContent m_ExpressionTitle;
        private VisualElement m_ContentViewport;
        private string m_ExpressionName;
        private string m_ExpressionPath;

        private void InitializeExpression()
        {
            if (m_Expression != null)
                return;

            m_ExpressionPath = AssetDatabase.GetAssetPath(target);
            m_ExpressionName = Path.GetFileNameWithoutExtension(m_ExpressionPath);
            m_ExpressionTitle = new GUIContent(m_ExpressionName, Icons.quicksearch, m_ExpressionPath);

            m_Expression = new SearchExpression(SearchSettings.GetContextOptions());
            m_Expression.Load(m_ExpressionPath);

            m_Variables = SearchVariable.UpdateVariables(m_Expression, m_Variables);
            SearchVariable.Evaluate(m_Variables, m_Expression, null);
        }

        public void Evaluate()
        {
            m_Expression.Evaluate();
        }

        public void OnEnable()
        {
            InitializeExpression();
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (m_ExpressionResultView == null)
                m_ExpressionResultView = new ExpressionResultView(m_Expression);

            m_ExpressionResultView.RegisterCallback<GeometryChangedEvent>(OnSizeChange);
            EditorApplication.delayCall += () => m_ExpressionResultView.style.height = 500;

            return m_ExpressionResultView;
        }

        private void OnSizeChange(GeometryChangedEvent evt)
        {
            if (m_ExpressionResultView == null || m_ExpressionResultView.panel == null || m_ExpressionResultView.panel.visualTree == null)
                return;

            if (m_ContentViewport == null)
            {
                m_ContentViewport = m_ExpressionResultView.panel.visualTree.Query("unity-content-viewport").First();
                if (m_ContentViewport != null)
                    m_ExpressionResultView.panel.visualTree.RegisterCallback<GeometryChangedEvent>(OnSizeChange);
            }
            if (m_ContentViewport != null)
            {
                m_ExpressionResultView.style.height = m_ContentViewport.resolvedStyle.height - 29f - (m_Variables.Length * (EditorGUIUtility.singleLineHeight+2));

                var editorsList = m_ExpressionResultView.panel.visualTree.Query(className: "unity-inspector-editors-list").First();
                if (editorsList != null)
                {
                    foreach (var c in editorsList.Children())
                    {
                        if (c.name.StartsWith("TextAssetInspector"))
                        {
                            c.style.height = 0;
                            c.style.display = DisplayStyle.None;
                        }
                    }
                }
            }
        }

        public override bool HasPreviewGUI()
        {
            return false;
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            return Icons.quicksearch;
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        protected override void OnHeaderGUI()
        {
            Rect position = new Rect(0, 0, m_ContentViewport?.resolvedStyle.width ?? 1, m_ContentViewport?.resolvedStyle.height ?? 1);

            GUILayout.BeginHorizontal();
            using (new EditorGUIUtility.IconSizeScope(new Vector2(16, 16)))
                GUILayout.Label(m_ExpressionTitle);
            GUILayout.FlexibleSpace();
            if (Utils.isDeveloperBuild)
            {
                if (GUILayout.Button("Refresh"))
                    m_Expression.Evaluate();
            }
            if (GUILayout.Button("Edit"))
                ExpressionBuilder.Open(m_ExpressionPath);
            GUILayout.EndHorizontal();

            if (SearchVariable.DrawVariables(m_Variables, position.width))
                SearchVariable.Evaluate(m_Variables, m_Expression, Repaint);
        }

        public override void OnInspectorGUI()
        {
            // Using UIElements
        }
    }

    [ExcludeFromPreset, ScriptedImporter(version: 2, ext: "qse")]
    class SearchExpressionImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var so = ScriptableObject.CreateInstance<SearchExpressionAsset>();
            so.hideFlags |= HideFlags.HideInInspector;
            ctx.AddObjectToAsset("expression", so, Icons.quicksearch);
            ctx.SetMainObject(so);
        }

        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OpenSearchExpression(int instanceID, int line)
        {
            var path = AssetDatabase.GetAssetPath(instanceID);
            if (!path.EndsWith(".qse", System.StringComparison.OrdinalIgnoreCase))
                return false;

            ExpressionBuilder.Open(path);
            return true;
        }
    }
}

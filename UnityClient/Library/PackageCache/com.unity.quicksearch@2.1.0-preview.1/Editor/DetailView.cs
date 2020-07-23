using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    class DetailView
    {
        private readonly ISearchView m_SearchView;
        private string m_LastPreviewItemId;
        private Editor[] m_Editors;
        private int m_EditorsHash = 0;
        private Vector2 m_ScrollPosition;
        private double m_LastPreviewStamp = 0;
        private Texture2D m_PreviewTexture;
        private Dictionary<string, bool> m_EditorTypeFoldout = new Dictionary<string, bool>();

        public DetailView(ISearchView searchView)
        {
            m_SearchView = searchView;
        }

        public bool HasDetails(SearchContext context)
        {
            var selection = context.searchView.selection;
            var selectionCount = selection.Count;
            if (selectionCount == 0)
                return false;

            var showDetails = true;
            string sameType = null;
            foreach (var s in selection)
            {
                if (!s.provider.showDetails)
                {
                    showDetails = false;
                    break;
                }

                if (sameType == null)
                    sameType = s.provider.name.id;
                else if (sameType != s.provider.name.id)
                {
                    showDetails = false;
                    break;
                }
            }

            if (!showDetails)
                return false;

            return true;
        }

        public void Draw(SearchContext context, float width)
        {
            var selection = context.searchView.selection;
            var selectionCount = selection.Count;
            if (selectionCount == 0)
                return;

            var lastItem = selection.Last();
            using (var scrollView = new EditorGUILayout.ScrollViewScope(m_ScrollPosition, GUILayout.Width(width), GUILayout.ExpandHeight(true)))
            {
                var showOptions = lastItem.provider.showDetailsOptions;

                if (showOptions.HasFlag(ShowDetailsOptions.Inspector) && Event.current.type == EventType.Layout)
                    SetupEditors(selection);

                if (selectionCount > 1)
                {
                    GUILayout.Label($"Selected {selectionCount} items", Styles.previewDescription);
                }
                else
                {
                    if (showOptions.HasFlag(ShowDetailsOptions.Preview))
                    {
                        if (showOptions.HasFlag(ShowDetailsOptions.Inspector))
                        {
                            if (m_Editors != null && m_Editors.Length == 1 && m_Editors[0].HasPreviewGUI())
                            {
                                var e = m_Editors[0];
                                var previewRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(width), GUILayout.MaxHeight(width));
                                if (previewRect.width > 0 && previewRect.height > 0)
                                    e.OnPreviewGUI(previewRect, Styles.largePreview);
                            }
                            else
                                DrawPreview(context, lastItem, width);
                        }
                        else
                            DrawPreview(context, lastItem, width);
                    }

                    if (showOptions.HasFlag(ShowDetailsOptions.Description))
                        DrawDescription(context, lastItem);
                }

                if (showOptions.HasFlag(ShowDetailsOptions.Inspector))
                {
                    DrawInspector(selection, width);
                }

                if (showOptions.HasFlag(ShowDetailsOptions.Actions))
                    DrawActions(context);

                m_ScrollPosition = scrollView.scrollPosition;
            }
        }

        private void DrawActions(SearchContext context)
        {
            var selection = context.searchView.selection;
            var firstItem = selection.First();
            GUILayout.Space(10);

            foreach (var action in firstItem.provider.actions.Where(a => a.enabled(selection)))
            {
                if (action == null || action.content == null)
                    continue;

                if (selection.Count > 1 && action.execute == null)
                    continue;

                if (GUILayout.Button(new GUIContent(action.displayName, action.content.image, action.content.tooltip), GUILayout.ExpandWidth(true)))
                {
                    m_SearchView.ExecuteAction(action, selection.ToArray(), true);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void ResetEditors()
        {
            if (m_Editors != null)
            {
                foreach (var e in m_Editors)
                    UnityEngine.Object.DestroyImmediate(e);
            }
            m_Editors = null;
            m_EditorsHash = 0;
        }

        private void DrawInspector(SearchSelection selection, float width)
        {
            if (m_Editors == null)
                return;

            for (int i = 0; i < m_Editors.Length; ++i)
            {
                var e = m_Editors[i];
                if (!e)
                    continue;

                EditorGUIUtility.labelWidth = 0.4f * width;
                bool foldout = false;
                if (!m_EditorTypeFoldout.TryGetValue(e.GetType().Name, out foldout))
                    foldout = true;
                using (new EditorGUIUtility.IconSizeScope(new Vector2(16, 16)))
                {
                    var sectionContent = selection.Count == 1 ? EditorGUIUtility.ObjectContent(e.target, e.GetType()) : e.GetPreviewTitle();
                    if (selection.Count == 1)
                        sectionContent.tooltip = sectionContent.text;
                    else
                        sectionContent.tooltip = String.Join("\r\n", e.targets.Select(t => $"{SearchUtils.GetObjectPath(t)} ({t.GetInstanceID()})"));
                    foldout = EditorGUILayout.BeginToggleGroup(sectionContent, foldout);
                    if (foldout)
                    {
                        try
                        {
                            if (e.target is Transform)
                                e.DrawDefaultInspector();
                            else
                                e.OnInspectorGUI();
                            m_EditorTypeFoldout[e.GetType().Name] = foldout;
                        }
                        catch
                        {
                            // Ignore
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
            }
        }

        private void SetupEditors(SearchSelection selection)
        {
            int selectionHash = 0;
            foreach (var s in selection)
                selectionHash ^= s.id.GetHashCode();

            if (selectionHash == m_EditorsHash)
                return;

            ResetEditors();

            var targets = new List<UnityEngine.Object>();
            foreach (var s in selection)
            {
                var item = s;
                var itemObject = item.provider.toObject?.Invoke(item, typeof(UnityEngine.Object));
                if (!itemObject)
                    continue;

                if (itemObject is GameObject go)
                {
                    var components = go.GetComponents<Component>();
                    foreach (var c in components.Skip(components.Length > 1 ? 1 : 0))
                    {
                        if (!c || c.hideFlags.HasFlag(HideFlags.HideInInspector))
                            continue;

                        targets.Add(c);
                    }
                }
                else
                    targets.Add(itemObject);
            }

            m_Editors = targets.GroupBy(t => t.GetType()).Select(g => Editor.CreateEditor(g.ToArray())).ToArray();
            m_EditorsHash = selectionHash;
        }

        private static void DrawDescription(SearchContext context, SearchItem item)
        {
            var description = SearchContent.FormatDescription(item, context, 2048);
            GUILayout.Label(description, Styles.previewDescription);
        }

        private void DrawPreview(SearchContext context, SearchItem item, float size)
        {
            if (item.provider.fetchPreview == null)
                return;

            if (m_Editors != null && m_Editors.Length == 1 && m_Editors[0].HasPreviewGUI())
            {
                var e = m_Editors[0];
                var previewRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(size), GUILayout.MaxHeight(size));
                if (previewRect.width > 0 && previewRect.height > 0)
                    e.OnPreviewGUI(previewRect, Styles.largePreview);
            }
            else
            {
                var now = EditorApplication.timeSinceStartup;
                if (now - m_LastPreviewStamp > 2.5)
                    m_PreviewTexture = null;

                if (!m_PreviewTexture || m_LastPreviewItemId != item.id)
                {
                    m_LastPreviewStamp = now;
                    m_PreviewTexture = item.provider.fetchPreview(item, context, Styles.previewSize, FetchPreviewOptions.Preview2D | FetchPreviewOptions.Large);
                    m_LastPreviewItemId = item.id;
                }

                if (m_PreviewTexture == null)
                    m_SearchView.Repaint();

                size -= (Styles.largePreview.margin.left + Styles.largePreview.margin.right);
                GUILayout.Space(10);
                GUILayout.Label(m_PreviewTexture, Styles.largePreview, GUILayout.MaxWidth(size), GUILayout.MaxHeight(size));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    internal class FilterWindow : EditorWindow
    {
        private static class Styles
        {
            public const float kItemHeight = 18f;
            public const float kMinWindowHeight = 150f;
            public const float kMaxWindowHeight = 400f;
            public static Vector2 windowSize = new Vector2(240f, 80f);
            public static readonly GUIStyle filterHeader = new GUIStyle(EditorStyles.boldLabel)
            {
                name = "quick-search-filter-header",
                margin = new RectOffset(4, 4, 3, 2)
            };

            public static readonly GUIContent prefButtonContent = new GUIContent(Icons.settings, "Open quick search preferences...");
            public static readonly GUIStyle prefButton = new GUIStyle("IconButton")
            {
                fixedWidth = 16, fixedHeight = 16,
                margin = new RectOffset(2, 2, 2, 2)
            };

            public static readonly GUIStyle filterTimeLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                name = "quick-search-filter-time-label",
                fixedWidth = 50,
                alignment = TextAnchor.MiddleRight,
                margin = new RectOffset(0, 0, 1, 1),
                fontSize = Math.Max(filterHeader.fontSize - 2, 9),
                fontStyle = FontStyle.Italic,
                normal = new GUIStyleState { textColor = EditorStyles.helpBox.normal.textColor }
            };

            public static readonly GUIStyle filterTimeLongLabel = new GUIStyle(filterTimeLabel)
            {
                name = "quick-search-filter-time-long-label",
                normal = new GUIStyleState() { textColor = Color.red }
            };

            public static readonly GUIStyle filterToggle = new GUIStyle("Toggle") { margin = new RectOffset(4, 4, 2, 1) };
            public static readonly GUIStyle headerFilterToggle = new GUIStyle(filterToggle) { margin = new RectOffset(4, 4, 3, 1) };

            public static readonly GUIStyle filterEntry = new GUIStyle(EditorStyles.label) { name = "quick-search-filter-entry" };
            public static readonly GUIStyle panelBorder = new GUIStyle("grey_border") { name = "quick-search-filter-panel-border" };
            public static readonly GUIStyle filterExpanded = new GUIStyle("IN Foldout") { margin = new RectOffset(2, 1, 2, 0) };
            public static readonly GUIStyle separator = new GUIStyle("sv_iconselector_sep") { margin = new RectOffset(1, 1, 4, 0) };

            public static readonly GUIContent showAllResults = new GUIContent("Show all results...",
                "If this option is checked, Quick Search will try to find as many results as possible even if it means the search can take more time to resolve." +
                "\r\n\r\nIf you find searches to be too slow in large project try to uncheck this option.");

            public static float foldoutIndent = filterExpanded.fixedWidth + 6;
        }
        static SearchContext s_SearchContext;

        private ISearchView m_SearchView;
        private Vector2 m_ScrollPos;
        private int m_ToggleFilterFocusIndex = 1;
        private int m_ToggleFilterNextIndex = 0;
        private int m_ToggleFilterCount = 0;
        private SearchContext m_Context;
        private SearchContext.FilterDesc[] m_ExplicitProviders;
        private SearchContext.FilterDesc[] m_FilterableProviders;

        internal static double s_CloseTime;
        internal static bool canShow
        {
            get
            {
                if (EditorApplication.timeSinceStartup - s_CloseTime < 0.250)
                    return false;
                return true;
            }
        }

        public static bool ShowAtPosition(ISearchView quickSearchTool, SearchContext context, Rect rect)
        {
            var screenPos = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
            var screenRect = new Rect(screenPos, rect.size);

            s_SearchContext = context;
            var filterWindow = ScriptableObject.CreateInstance<FilterWindow>();
            filterWindow.m_SearchView = quickSearchTool;

            var minHeight = Mathf.Max(Styles.kMinWindowHeight, Styles.windowSize.y + Styles.kItemHeight * context.filters.Count());
            var height = Math.Min(minHeight, Styles.kMaxWindowHeight);
            var size = new Vector2(Styles.windowSize.x, height);
            filterWindow.ShowAsDropDown(screenRect, size);
            s_SearchContext = null;
            return true;
        }

        [UsedImplicitly]
        internal void OnEnable()
        {
            if (s_SearchContext != null)
            {
                m_Context = s_SearchContext;
                m_FilterableProviders = m_Context.filters.Where(d => !d.provider.isExplicitProvider).OrderBy(d => d.provider.priority).ToArray();
                m_ExplicitProviders = m_Context.filters.Where(d => d.provider.isExplicitProvider).OrderBy(d => d.provider.priority).ToArray();
            }
            else
            {
                throw new Exception("Opening Filter Window with not filter");
            }
        }

        [UsedImplicitly]
        internal void OnDestroy()
        {
            s_CloseTime = EditorApplication.timeSinceStartup;
            if (m_FilterableProviders.All(desc => !desc.enabled))
                Debug.LogWarning("All filters are disabled");
        }

        [UsedImplicitly]
        internal void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
                if (m_SearchView != null)
                    m_SearchView.Focus();
                return;
            }

            HandleKeyboardNavigation();

            m_ToggleFilterNextIndex = 0;

            GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, Styles.panelBorder);
            DrawHeader();
            GUILayout.Label(GUIContent.none, Styles.separator);

            m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);

            foreach (var desc in m_FilterableProviders)
            {
                DrawSectionHeader(desc);
            }

            m_ToggleFilterCount = m_ToggleFilterNextIndex;

            GUILayout.Space(10);
            DrawWantsMore();

            GUILayout.Space(10);
            DrawExplicitProviders();

            GUILayout.EndScrollView();
            GUILayout.Space(1);
        }

        private void HandleKeyboardNavigation()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.UpArrow)
            {
                m_ToggleFilterFocusIndex = Math.Max(0, m_ToggleFilterFocusIndex-1);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.DownArrow)
            {
                m_ToggleFilterFocusIndex = Math.Min(m_ToggleFilterFocusIndex+1, m_ToggleFilterCount-1);
                Event.current.Use();
            }

            GUI.FocusControl($"Box_{m_ToggleFilterFocusIndex}");
        }

        private void DrawExplicitProviders()
        {
            if (m_ExplicitProviders.Length == 0)
                return;

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Special Search Providers", null, "Providers only available if specified explicitly"), Styles.filterHeader);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label(GUIContent.none, Styles.separator);

            foreach (var desc in m_ExplicitProviders)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(Styles.foldoutIndent);
                GUILayout.Label(GetProviderLabelContent(desc.provider), Styles.filterHeader);
                GUILayout.EndHorizontal();
            }
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Regular Search Providers", Styles.filterHeader);
            if (GUILayout.Button(Styles.prefButtonContent, Styles.prefButton))
                SettingsService.OpenUserPreferences(SearchSettings.settingsPreferencesKey);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName($"Box_{m_ToggleFilterNextIndex++}");
            bool isEnabled = GUILayout.Toggle(m_FilterableProviders.All(d => d.enabled), "", Styles.headerFilterToggle, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
                m_Context.ResetFilter(isEnabled);

            GUILayout.EndHorizontal();
        }

        private void DrawSectionHeader(SearchContext.FilterDesc desc)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(Styles.foldoutIndent);

            GUILayout.Label(GetProviderLabelContent(desc.provider), Styles.filterHeader);
            GUILayout.FlexibleSpace();
            if (desc.provider != null)
            {
                var fetchTime = desc.provider.fetchTime;
                var loadTime = desc.provider.loadTime;
                var enableTime = desc.provider.enableTime;
                if (fetchTime > 0.99 || loadTime > 9.99 || enableTime > 9.99)
                {
                    GUIContent content = new GUIContent(fetchTime.ToString("0.#") + " ms",
                                                        $"Initialization took {loadTime.ToString("0.#")} ms\r\n" +
                                                        $"Activation took {enableTime.ToString("0.#")} ms");
                    GUILayout.Label(content, fetchTime < 25.0 ? Styles.filterTimeLabel : Styles.filterTimeLongLabel);
                }
            }

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName($"Box_{m_ToggleFilterNextIndex++}");
            bool isEnabled = GUILayout.Toggle(desc.enabled, "", Styles.filterToggle, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                m_Context.SetFilter(desc.provider.name.id, isEnabled);
                m_SearchView.Refresh();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawWantsMore()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(Styles.foldoutIndent);

            GUILayout.Label(Styles.showAllResults, Styles.filterHeader);
            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName($"Box_{m_ToggleFilterNextIndex++}");
            SearchSettings.wantsMore = GUILayout.Toggle(SearchSettings.wantsMore, "", Styles.filterToggle, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                if (SearchSettings.wantsMore)
                    m_SearchView.context.options |= SearchFlags.WantsMore;
                else
                    m_SearchView.context.options &= ~SearchFlags.WantsMore;
                m_SearchView.Refresh();
            }

            GUILayout.EndHorizontal();
        }

        private static GUIContent GetProviderLabelContent(SearchProvider provider)
        {
            var displayName = string.IsNullOrEmpty(provider.filterId) ? provider.name.displayName : provider.name.displayName + " (" + provider.filterId + ")";

            string tooltip = null;
            if (provider.filterId != null)
            {
                tooltip = $"Type \"{provider.filterId}\" to search ONLY for {provider.name.displayName}";
            }
            return new GUIContent(displayName, null, tooltip);
        }
    }
}
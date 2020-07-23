//#define QUICKSEARCH_DEBUG
#if (UNITY_2020_2_OR_NEWER)
//#define USE_SEARCH_ENGINE_API
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.QuickSearch
{
    /// <summary>
    /// Quick Search Editor Window
    /// </summary>
    /// <example>
    /// using Unity.QuickSearch;
    /// [MenuItem("Tools/Quick Search %space", priority = 42)]
    /// private static void OpenQuickSearch()
    /// {
    ///     QuickSearch.Open();
    /// }
    /// </example>
    public class QuickSearch : EditorWindow, ISearchView, IDisposable
    {
        const int k_ResetSelectionIndex = -1;
        const string k_LastSearchPrefKey = "last_search";
        private static readonly string k_CheckWindowKeyName = $"{typeof(QuickSearch).FullName}h";

        private static readonly string[] k_Dots = { ".", "..", "..." };
        private static readonly bool isDeveloperMode = Utils.isDeveloperBuild;
        private static EditorWindow s_FocusedWindow;
        private static SearchContext s_GlobalContext = null;

        // Selection state
        private SortedSearchList m_FilteredItems;
        private readonly List<int> m_Selection = new List<int>();
        private int m_DelayedCurrentSelection = k_ResetSelectionIndex;
        private SearchSelection m_SearchItemSelection;

        private bool m_Disposed = false;
        [SerializeField] private EditorWindow lastFocusedWindow;
        [SerializeField] private bool m_SearchBoxFocus;
        [SerializeField] private bool m_ShowFilterWindow = false;
        [SerializeField] private bool m_ShowCreateWindow = false;
        private SearchAnalytics.SearchEvent m_CurrentSearchEvent;
        internal double m_DebounceTime = 0.0;
        [SerializeField] private float m_ItemSize = 1;
        private DetailView m_DetailView;
        private IResultView m_ResultView;
        [SerializeField] private bool m_DetailsViewSplitterResize = false;

        const float m_DetailsViewShowMinSize = 425f;

        [SerializeField] private Vector3 m_WindowSize;
        [SerializeField] private float m_DetailsViewSplitterPos = -1f;

        [SerializeField] private  string searchTopic;
        [SerializeField] internal  bool sendAnalyticsEvent;
        [SerializeField] internal bool saveFilters;
        [SerializeField] private string[] providerIds;

        internal event Action nextFrame;
        internal event Action<Vector2, Vector2> resized;

        /// <summary>
        /// When QuickSearch is used in Object Picker mode, this callback is triggered when an object is selected.
        /// </summary>
        public Action<SearchItem, bool> selectCallback { get; set; }

        /// <summary>
        /// When QuickSearch is used in Object Picker mode, this callback is triggered when the search item list is
        /// computed from the query allowing a user to apply further filtering on the object set.
        /// </summary>
        public Func<SearchItem, bool> filterCallback { get; set; }

        /// <summary>
        /// Callback used to override the tracking behavior.
        /// </summary>
        public Action<SearchItem> trackingCallback { get; set; }

        /// <summary>
        /// Returns the selected item in the view
        /// </summary>
        public SearchSelection selection
        {
            get
            {
                if (m_SearchItemSelection == null)
                    m_SearchItemSelection = new SearchSelection(m_Selection, m_FilteredItems);
                return m_SearchItemSelection;
            }
        }

        /// <summary>
        /// Returns the current view search context
        /// </summary>
        public SearchContext context { get; private set; }

        /// <summary>
        /// Return the list of all search results.
        /// </summary>
        public ISearchList results => m_FilteredItems;

        /// <summary>
        /// Indicates how the data is displayed in the UI.
        /// </summary>
        public DisplayMode displayMode { get => IsDisplayGrid() ? DisplayMode.Grid : DisplayMode.List; }

        /// <summary>
        /// Defines the size of items in the search view.
        /// </summary>
        public float itemIconSize { get => m_ItemSize; set => UpdateItemSize(value); }

        /// <summary>
        /// Allow multi-selection or not.
        /// </summary>
        public bool multiselect { get; set; }

        /// <summary>
        /// Sets the search query text.
        /// </summary>
        /// <param name="searchText">Text to be displayed in the search view.</param>
        /// <param name="moveCursor">Where to place the cursor after having set the search text</param>
        public void SetSearchText(string searchText, TextCursorPlacement moveCursor = TextCursorPlacement.Default)
        {
            context.searchText = searchText ?? String.Empty;
            DebouncedRefresh();
            nextFrame += () =>
            {
                var te = SearchField.GetTextEditor();
                te.text = searchText;
                SearchField.MoveCursor(moveCursor);
            };
        }

        /// <summary>
        /// Open the Quick Search filter window to edit active filters.
        /// </summary>
        public void PopFilterWindow()
        {
            m_CurrentSearchEvent.useFilterMenuShortcut = true;
            nextFrame += () => m_ShowFilterWindow = true;
        }

        /// <summary>
        /// Open the Search Query creation window.
        /// </summary>
        public void PopSearchQueryCreateWindow()
        {
            nextFrame += () => m_ShowCreateWindow = true;
        }

        /// <summary>
        /// Re-fetch the search results and refresh the UI.
        /// </summary>
        public void Refresh()
        {
            SearchSettings.ApplyContextOptions(context);

            DebugInfo.refreshCount++;

            #if QUICKSEARCH_DEBUG
            Debug.LogWarning($"Searching {context.searchQuery} with {context.options}");
            #endif

            var foundItems = SearchService.GetItems(context);
            if (selectCallback != null)
                foundItems.Add(SearchItem.none);
            else if (String.IsNullOrEmpty(context.searchText))
                foundItems = SearchQuery.GetAllSearchQueryItems(context);

            SetItems(filterCallback == null ? foundItems : foundItems.Where(item => filterCallback(item)));

            EditorApplication.update -= UpdateAsyncResults;
            EditorApplication.update += UpdateAsyncResults;
        }

        /// <summary>
        /// Creates a new instance of a Quick Search window but does not show it immediately allowing a user to setup filter before showing the window.
        /// </summary>
        /// <param name="reuseExisting">If true, try to reuse an already existing instance of QuickSearch. If false will create a new QuickSearch window.</param>
        /// <returns>Returns the Quick Search editor window instance.</returns>
        public static QuickSearch Create(bool reuseExisting = false)
        {
            return Create(null, topic: "anything", saveFilters: true, reuseExisting: reuseExisting, multiselect: true);
        }

        /// <summary>
        /// Create a new quick search window that will be initialized with a specific context.
        /// </summary>
        /// <param name="context">Search context to start with</param>
        /// <param name="topic">Topic to seached</param>
        /// <param name="saveFilters">True if user provider filters should be saved for next search session</param>
        /// <param name="reuseExisting">True if we should reuse an opened window</param>
        /// <param name="multiselect">True if the search support multi-selection or not.</param>
        /// <returns></returns>
        public static QuickSearch Create(SearchContext context, string topic = "anything", bool saveFilters = true, bool reuseExisting = false, bool multiselect = true)
        {
            s_GlobalContext = context;
            s_FocusedWindow = focusedWindow;

            var qsWindow = reuseExisting && HasOpenInstances<QuickSearch>() ? GetWindow<QuickSearch>() : CreateInstance<QuickSearch>();
            qsWindow.multiselect = multiselect;
            qsWindow.saveFilters = saveFilters;
            qsWindow.searchTopic = topic;

            // Ensure we won't send events while doing a domain reload.
            qsWindow.sendAnalyticsEvent = true;
            return qsWindow;
        }

        /// <summary>
        /// Creates and open a new instance of Quick Search
        /// </summary>
        /// <param name="defaultWidth">Initial width of the window.</param>
        /// <param name="defaultHeight">Initial height of the window.</param>
        /// <param name="dockable">If true, creates a dockable QuickSearch Window (that will be closed when an item is activated). If false, it will create a DropDown (borderless, undockable and unmovable) version of QuickSearch.</param>
        /// <param name="reuseExisting">If true, try to reuse an already existing instance of QuickSearch. If false will create a new QuickSearch window.</param>
        /// <returns>Returns the Quick Search editor window instance.</returns>
        public static QuickSearch Open(float defaultWidth = 850, float defaultHeight = 539, bool dockable = false, bool reuseExisting = false)
        {
            return Create(reuseExisting: reuseExisting).ShowWindow(defaultWidth, defaultHeight, dockable);
        }

        /// <summary>
        /// Open the quick search window using a specific context (activating specific filters and such).
        /// </summary>
        /// <param name="providerIds">Unique ids of providers to enable when popping QuickSearch.</param>
        /// <returns>Returns a new shown instance of QuickSearch.</returns>
        /// <example>
        /// [MenuItem("Tools/Search Menus _F1")]
        /// public static void SearchMenuItems()
        /// {
        ///     QuickSearch.OpenWithContextualProvider("menu");
        /// }
        /// </example>
        public static QuickSearch OpenWithContextualProvider(params string[] providerIds)
        {
            var providers = providerIds.Select(id => SearchService.Providers.Find(p => p.name.id == id)).Where(p=>p!=null);
            if (providers.Any(p => p == null))
            {
                Debug.LogWarning($"Quick Search cannot find one of these search providers {String.Join(", ", providers)}");
                return OpenDefaultQuickSearch();
            }

            if (providerIds.Length == 0)
                return Open(dockable: SearchSettings.dockable || Utils.IsRunningTests());

            var context = SearchService.CreateContext(providers);
            var qsWindow = Create(context, saveFilters: false, topic: String.Join(", ", providers.Select(p => p.name.displayName.ToLower())));
            qsWindow.SetSearchText(SearchSettings.GetScopeValue(k_LastSearchPrefKey, qsWindow.context.scopeHash, ""));
            return qsWindow.ShowWindow(dockable: SearchSettings.dockable || Utils.IsRunningTests());
        }

        /// <summary>
        /// Open the default Quick Search window using default settings.
        /// </summary>
        /// <param name="defaultWidth">Initial width of the window.</param>
        /// <param name="defaultHeight">Initial height of the window.</param>
        /// <param name="dockable">If true, creates a dockable QuickSearch Window (that will be closed when an item is activated). If false, it will create a DropDown (borderless, undockable and unmovable) version of QuickSearch.</param>
        /// <returns>Returns the Quick Search editor window instance.</returns>
        public QuickSearch ShowWindow(float defaultWidth = 850, float defaultHeight = 538, bool dockable = false)
        {
            var windowSize = new Vector2(defaultWidth, defaultHeight);
            if (dockable)
            {
                if (!EditorPrefs.HasKey(k_CheckWindowKeyName))
                    position = Utils.GetMainWindowCenteredPosition(windowSize);
                Show();
            }
            else
            {
                this.ShowDropDown(windowSize);
            }
            Focus();
            return this;
        }

        /// <summary>
        /// Use Quick Search to as an object picker to select any object based on the specified filter type.
        /// </summary>
        internal static QuickSearch ShowObjectPicker(
            Action<UnityEngine.Object, bool> selectHandler,
            Action<UnityEngine.Object> trackingHandler,
            string searchText, string typeName, Type filterType,
            float defaultWidth = 850, float defaultHeight = 539, bool dockable = false)
        {
            return ShowObjectPicker(selectHandler, trackingHandler, (Func<UnityEngine.Object, bool>)null,
                searchText, typeName, filterType, defaultWidth, defaultHeight, dockable);
        }

        #if USE_SEARCH_ENGINE_API
        internal static QuickSearch ShowObjectPicker(
            Action<UnityEngine.Object, bool> selectHandler,
            Action<UnityEngine.Object> trackingHandler,
            #pragma warning disable IDE0060 // Remove unused parameter
            Func<UnityEditor.SearchService.ObjectSelectorTargetInfo, bool> pickerConstraintHandler,
            #pragma warning restore IDE0060 // Remove unused parameter
            string searchText, string typeName, Type filterType,
            float defaultWidth = 850, float defaultHeight = 539, bool dockable  = false)
        {
            return ShowObjectPicker(selectHandler, trackingHandler, searchText, typeName, filterType, defaultWidth, defaultHeight, dockable);
        }
        #endif

        /// <summary>
        /// Use Quick Search to as an object picker to select any object based on the specified filter type.
        /// </summary>
        /// <param name="selectHandler">Callback to trigger when a user selects an item.</param>
        /// <param name="trackingHandler">Callback to trigger when the user is modifying QuickSearch selection (i.e. tracking the currently selected item)</param>
        /// <param name="pickerConstraintHandler">Callback that is called to provide additionnal filtering when QuickSearch is populating its object list.</param>
        /// <param name="searchText">Initial search text for QuickSearch.</param>
        /// <param name="typeName">Type name of the object to select. Can be used to replace filterType.</param>
        /// <param name="filterType">Type of the object to select.</param>
        /// <param name="defaultWidth">Initial width of the window.</param>
        /// <param name="defaultHeight">Initial height of the window.</param>
        /// <param name="dockable">If true, creates a dockable QuickSearch Window (that will be closed when an item is activated). If false, it will create a DropDown (borderless, undockable and unmovable) version of QuickSearch.</param>
        /// <returns></returns>
        public static QuickSearch ShowObjectPicker(
            Action<UnityEngine.Object, bool> selectHandler,
            Action<UnityEngine.Object> trackingHandler,
            Func<UnityEngine.Object, bool> pickerConstraintHandler,
            string searchText, string typeName, Type filterType,
            float defaultWidth = 850, float defaultHeight = 539, bool dockable  = false)
        {
            if (selectHandler == null || typeName == null)
                return null;

            if (filterType == null)
                filterType = TypeCache.GetTypesDerivedFrom<UnityEngine.Object>()
                    .FirstOrDefault(t => t.Name == typeName) ?? typeof(UnityEngine.Object);

            var qs = Create();
            qs.saveFilters = false;
            qs.searchTopic = "object";
            qs.sendAnalyticsEvent = true;
            qs.titleContent.text = $"Select {filterType?.Name ?? typeName}...";
            qs.itemIconSize = 64;
            qs.multiselect = false;
            #if USE_SEARCH_ENGINE_API
            qs.filterCallback = (item) => item == SearchItem.none || (IsObjectMatchingType(item ?? SearchItem.none, filterType) /*&& (pickerConstraintHandler?.Invoke(Utils.ToObject(item, filterType)) ?? true)*/);
            #else
            qs.filterCallback = (item) => IsObjectMatchingType(item ?? SearchItem.none, filterType);
            #endif
            qs.selectCallback = (item, canceled) => selectHandler?.Invoke(Utils.ToObject(item, filterType), canceled);
            qs.trackingCallback = (item) => trackingHandler?.Invoke(Utils.ToObject(item, filterType));
            qs.context.wantsMore = true;
            qs.context.filterType = filterType;
            qs.SetSearchText(searchText, TextCursorPlacement.MoveToStartOfNextWord);

            if (!dockable)
                qs.ShowAuxWindow();
            else
                qs.Show();
            qs.position = Utils.GetMainWindowCenteredPosition(new Vector2(defaultWidth, defaultHeight));
            qs.Focus();

            return qs;
        }

        /// <summary>
        /// Set which items are selected in the view.
        /// </summary>
        /// <param name="selection">List containing indices of items to select.</param>
        public void SetSelection(params int[] selection)
        {
            if (!multiselect && selection.Length > 1)
                throw new Exception("Multi selection is not allowed.");

            var lastIndexAdded = k_ResetSelectionIndex;

            m_Selection.Clear();
            m_SearchItemSelection = null;
            foreach (var idx in selection)
            {
                if (!IsItemValid(idx))
                    continue;

                m_Selection.Add(idx);
                lastIndexAdded = idx;
            }

            if (lastIndexAdded != k_ResetSelectionIndex)
            {
                m_SearchItemSelection = null;
                TrackSelection(lastIndexAdded);
            }
        }
        /// <summary>
        /// Add items to the current selection.
        /// </summary>
        /// <param name="selection">List containing indices of items to add to current selection.</param>
        public void AddSelection(params int[] selection)
        {
            if (!multiselect && m_Selection.Count == 1)
                throw new Exception("Multi selection is not allowed.");

            var lastIndexAdded = k_ResetSelectionIndex;

            foreach (var idx in selection)
            {
                if (!IsItemValid(idx))
                    continue;

                if (m_Selection.Contains(idx))
                {
                    m_Selection.Remove(idx);
                }
                else
                {
                    m_Selection.Add(idx);
                    lastIndexAdded = idx;
                }
            }

            if (lastIndexAdded != k_ResetSelectionIndex)
            {
                m_SearchItemSelection = null;
                TrackSelection(lastIndexAdded);
            }
        }

        /// <summary>
        /// Execute a Search Action on a given list of items.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="items">Items to apply the action on.</param>
        /// <param name="endSearch">If true, executing this action will close the Quicksearch window.</param>
        public void ExecuteAction(SearchAction action, SearchItem[] items, bool endSearch = true)
        {
            var item = items.LastOrDefault();
            if (item == null)
                return;

            SendSearchEvent(item, action);
            EditorApplication.delayCall -= DelayTrackSelection;

            if (selectCallback != null)
            {
                selectCallback(item, false);
                selectCallback = null;
            }
            else
            {
                if (endSearch)
                    SearchField.UpdateLastSearchText(context.searchText);

                if (action.execute != null)
                    action.execute(items);
                else action.handler?.Invoke(item);
            }

            if (endSearch && action.closeWindowAfterExecution)
                CloseSearchWindow();
        }

        /// <summary>
        /// Show a contextual menu for the specified item.
        /// </summary>
        /// <param name="item">Item affected by the contextual menu.</param>
        /// <param name="position">Where the menu should be drawn on screen (generally item position)</param>
        public void ShowItemContextualMenu(SearchItem item, Rect position)
        {
            m_CurrentSearchEvent.useActionMenuShortcut = true;

            var menu = new GenericMenu();
            var shortcutIndex = 0;
            var currentSelection = new[] { item };
            foreach (var action in item.provider.actions.Where(a => a.enabled(currentSelection)))
            {
                var itemName = action.content.tooltip;
                if (shortcutIndex == 0)
                {
                    itemName += " _enter";
                }
                else if (shortcutIndex == 1)
                {
                    itemName += " _&enter";
                }
                else if (shortcutIndex == 2)
                {
                    itemName += " _&%enter";
                }
                else if (shortcutIndex == 3)
                {
                    itemName += " _&%#enter";
                }
                menu.AddItem(new GUIContent(itemName, action.content.image), false, () => ExecuteAction(action, currentSelection));
                ++shortcutIndex;
            }

            if (position == default)
                menu.ShowAsContext();
            else
                menu.DropDown(position);
        }

        [UsedImplicitly]
        internal void OnEnable()
        {
            hideFlags |= HideFlags.DontSaveInEditor;
            itemIconSize = SearchSettings.itemIconSize;
            lastFocusedWindow = lastFocusedWindow ?? s_FocusedWindow;

            #if UNITY_2020_2_OR_NEWER
            wantsLessLayoutEvents = true;
            #endif

            SearchSettings.SortActionsPriority();

            // Create search view context
            if (s_GlobalContext == null)
            {
                if (providerIds == null || providerIds.Length == 0)
                    context = new SearchContext(SearchService.Providers.Where(p => p.active));
                else
                    context = new SearchContext(providerIds.Select(id => SearchService.GetProvider(id)).Where(p => p != null));
            }
            else
            {
                context = s_GlobalContext;
                s_GlobalContext = null;

                // Save provider ids to restore search window state after a domain reload.
                providerIds = context.filters.Select(f => f.provider.name.id).ToArray();
            }
            context.searchView = this;
            context.focusedWindow = lastFocusedWindow;
            context.asyncItemReceived -= OnAsyncItemsReceived;
            context.asyncItemReceived += OnAsyncItemsReceived;
            m_FilteredItems = new SortedSearchList(context);

            LoadSessionSettings();

            // Create search view state objects
            m_SearchBoxFocus = true;
            m_CurrentSearchEvent = new SearchAnalytics.SearchEvent();
            m_DetailView = new DetailView(this);
            m_DebounceTime = 1f;

            resized += OnWindowResized;

            DebugInfo.Enable(this);
        }

        [UsedImplicitly]
        internal void OnDisable()
        {
            DebugInfo.Disable();

            s_FocusedWindow = null;
            AutoComplete.Clear();

            resized = null;
            nextFrame = null;
            EditorApplication.update -= UpdateAsyncResults;
            EditorApplication.update -= DebouncedRefresh;
            EditorApplication.delayCall -= DelayTrackSelection;

            selectCallback?.Invoke(null, true);

            if (!isDeveloperMode)
                SendSearchEvent(null); // Track canceled searches

            SaveSessionSettings();

            // End search session
            context.asyncItemReceived -= OnAsyncItemsReceived;
            context.Dispose();
            context = null;

            if (!Utils.IsRunningTests())
                Resources.UnloadUnusedAssets();
        }

        [UsedImplicitly]
        internal void OnGUI()
        {
            if (context == null)
                return;

            var evt = Event.current;
            var eventType = evt.rawType;
            if (eventType == EventType.Repaint)
            {
                DebugInfo.repaintCount++;

                var newWindowSize = position.size;
                if (!newWindowSize.Equals(m_WindowSize))
                {
                    if (m_WindowSize.x > 0)
                        resized?.Invoke(m_WindowSize, newWindowSize);
                    m_WindowSize = newWindowSize;
                }

                nextFrame?.Invoke();
                nextFrame = null;
            }

            HandleKeyboardNavigation(evt);
            #if QUICKSEARCH_DEBUG
            if (evt.isKey && evt.type != EventType.Used)
                Debug.Log($"KeyEvent({evt.type}, {evt.keyCode}, {(int)evt.character}, {evt.character})");
            #endif

            var windowBorder = SearchSettings.dockable ? GUIStyle.none : Styles.panelBorder;
            using (new EditorGUILayout.VerticalScope(windowBorder))
            {
                if (eventType == EventType.Repaint)
                    DebugInfo.gcDraw = GC.GetTotalMemory(false);
                DrawToolbar(evt);
                if (context == null)
                    return;
                using (new EditorGUILayout.HorizontalScope())
                {
                    var showDetails = position.width > m_DetailsViewShowMinSize && selectCallback == null && m_DetailView.HasDetails(context);
                    if (m_DetailsViewSplitterPos < 0f)
                        SetDetailsViewSplitterPosition(position.width - Styles.previewSize.x);

                    DrawItems(showDetails ? m_DetailsViewSplitterPos-2f : position.width);

                    if (showDetails)
                    {
                        m_DetailView.Draw(context, position.width - m_DetailsViewSplitterPos + 2f);
                        DrawDetailsViewSplitter(evt);
                    }
                }

                if (eventType == EventType.Repaint)
                    DebugInfo.gcDraw = GC.GetTotalMemory(false) - DebugInfo.gcDraw;
                DebugInfo.Draw();
                DrawStatusBar(evt);
                AutoComplete.Draw(context, this);
            }

            UpdateFocusControlState(evt);
        }

        [UsedImplicitly]
        internal void OnLostFocus()
        {
            AutoComplete.Clear();
        }

        [UsedImplicitly]
        internal void Update()
        {
            if (focusedWindow != this)
                return;

            var time = EditorApplication.timeSinceStartup;
            var repaintRequested = SearchField.UpdateBlinkCursorState(time);
            if (repaintRequested)
                Repaint();
        }

        internal void SetItems(IEnumerable<SearchItem> items)
        {
            m_SearchItemSelection = null;
            m_FilteredItems.Clear();
            m_FilteredItems.AddItems(items);
            SetSelection();
            UpdateWindowTitle();
        }

        private void OnAsyncItemsReceived(SearchContext context, IEnumerable<SearchItem> items)
        {
            var filteredItems = items;
            if (filterCallback != null)
                filteredItems = filteredItems.Where(item => filterCallback(item));
            m_FilteredItems.AddItems(filteredItems);
            EditorApplication.update -= UpdateAsyncResults;
            EditorApplication.update += UpdateAsyncResults;
        }

        private void UpdateAsyncResults()
        {
            EditorApplication.update -= UpdateAsyncResults;

            UpdateWindowTitle();
            Repaint();
        }

        private void SendSearchEvent(SearchItem item, SearchAction action = null)
        {
            if (item != null)
                m_CurrentSearchEvent.Success(item, action);

            if (m_CurrentSearchEvent.success || m_CurrentSearchEvent.elapsedTimeMs > 7000)
            {
                m_CurrentSearchEvent.Done();
                if (item != null)
                    m_CurrentSearchEvent.searchText = $"{context.searchText} => {item.id}";
                else
                    m_CurrentSearchEvent.searchText = context.searchText;
                if (sendAnalyticsEvent)
                    SearchAnalytics.SendSearchEvent(m_CurrentSearchEvent, context);
            }

            // Prepare next search event
            m_CurrentSearchEvent = new SearchAnalytics.SearchEvent();
        }

        private void UpdateWindowTitle()
        {
            if (!titleContent.image)
                titleContent.image = Icons.quicksearch;
            if (m_FilteredItems.Count == 0)
                titleContent.text = $"Quick Search";
            else
                titleContent.text = $"Quick Search ({m_FilteredItems.Count - (selectCallback != null ? 1 : 0)})";
        }

        private static string FormatStatusMessage(SearchContext context, ICollection<SearchItem> items)
        {
            var providers = context.providers.ToList();
            if (providers.Count == 0)
                return "There is no activated search provider";

            var msg = context.actionId != null ? $"Executing action for {context.actionId} " : "Searching ";
            if (providers.Count > 1)
                msg += Utils.FormatProviderList(providers.Where(p => !p.isExplicitProvider), showFetchTime: !context.searchInProgress);
            else
                msg += Utils.FormatProviderList(providers);

            if (items != null && items.Count > 0)
            {
                msg += $" and found <b>{items.Count}</b> result";
                if (items.Count > 1)
                    msg += "s";
                if (!context.searchInProgress)
                {
                    if (context.searchElapsedTime > 1.0)
                        msg += $" in {PrintTime(context.searchElapsedTime)}";
                }
                else
                    msg += " so far";
            }
            else if (!string.IsNullOrEmpty(context.searchQuery))
            {
                if (!context.searchInProgress)
                    msg += " and found nothing";
            }

            if (context.searchInProgress)
                msg += k_Dots[(int)EditorApplication.timeSinceStartup % k_Dots.Length];

            return msg;
        }

        private static string PrintTime(double timeMs)
        {
            if (timeMs >= 1000)
                return $"{Math.Round(timeMs / 1000.0)} seconds";
            return $"{Math.Round(timeMs)} ms";
        }

        private void DrawStatusBar(Event evt)
        {
            using (new GUILayout.HorizontalScope(Styles.statusBarBackground))
            {
                var hasProgress = context.searchInProgress;
                var title = FormatStatusMessage(context, m_FilteredItems);
                var tooltip = Utils.FormatProviderList(context.providers, fullTimingInfo: true);
                var statusLabelContent = EditorGUIUtility.TrTextContent(title, tooltip);
                GUILayout.Label(statusLabelContent, Styles.statusLabel, GUILayout.MaxWidth(position.width - 100));
                GUILayout.FlexibleSpace();

                #if !UNITY_2020_1_OR_NEWER
                // This is used to show some form of async progress for the indexing in 19.4
                if (Progress.Any())
                {
                    GUILayout.Label(Progress.Current(), Styles.statusLabel);
                    hasProgress = true;
                }
                #endif

                EditorGUI.BeginChangeCheck();
                var newItemIconSize = GUILayout.HorizontalSlider(itemIconSize, 0f, 165f,
                    Styles.itemIconSizeSlider, Styles.itemIconSizeSliderThumb, GUILayout.Width(100f));
                if (EditorGUI.EndChangeCheck())
                {
                    itemIconSize = newItemIconSize;
                    SearchSettings.itemIconSize = newItemIconSize;
                    m_ResultView.focusSelectedItem = true;
                }

                if (GUILayout.Button(SearchAnalytics.Version, Styles.versionLabel))
                    Utils.OpenDocumentationUrl();
                if (evt.type == EventType.Repaint)
                {
                    var helpButtonRect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(helpButtonRect, MouseCursor.Link);
                }

                if (hasProgress)
                {
                    var searchInProgressRect = EditorGUILayout.GetControlRect(false,
                        Styles.searchInProgressButton.fixedHeight, Styles.searchInProgressButton, Styles.searchInProgressLayoutOptions);

                    int frame = (int)Mathf.Repeat(Time.realtimeSinceStartup * 5, 11.99f);
                    GUI.Button(searchInProgressRect, Styles.statusWheel[frame], Styles.searchInProgressButton);

                    if (evt.type == EventType.MouseDown && searchInProgressRect.Contains(evt.mousePosition))
                        SettingsService.OpenUserPreferences(SearchSettings.settingsPreferencesKey);
                }
                else
                {
                    if (GUILayout.Button(Styles.prefButtonContent, Styles.prefButton))
                        SettingsService.OpenUserPreferences(SearchSettings.settingsPreferencesKey);
                }
            }
        }

        private bool IsItemValid(int index)
        {
            if (index < 0 || index >= m_FilteredItems.Count)
                return false;
            return true;
        }

        private bool IsSelectedItemValid()
        {
            var selectionIndex = m_Selection.Count == 0 ? k_ResetSelectionIndex : m_Selection.Last();
            return IsItemValid(selectionIndex);
        }

        private void DelayTrackSelection()
        {
            if (m_FilteredItems.Count == 0)
                return;

            if (!IsItemValid(m_DelayedCurrentSelection))
                return;

            var selectedItem = m_FilteredItems[m_DelayedCurrentSelection];
            if (trackingCallback == null)
                selectedItem.provider?.trackSelection?.Invoke(selectedItem, context);
            else
                trackingCallback(selectedItem);

            m_DelayedCurrentSelection = k_ResetSelectionIndex;
        }

        private void TrackSelection(int currentSelection)
        {
            if (!SearchSettings.trackSelection)
                return;

            m_DelayedCurrentSelection = currentSelection;
            EditorApplication.delayCall -= DelayTrackSelection;
            EditorApplication.delayCall += DelayTrackSelection;
        }

        private void UpdateFocusControlState(Event evt)
        {
            if (evt.type != EventType.Repaint)
                return;

            if (m_SearchBoxFocus)
            {
                SearchField.Focus();
                m_SearchBoxFocus = false;
            }
        }

        private int GetItemCount()
        {
            return m_FilteredItems.Count;
        }

        private bool HandleDefaultPressEnter(Event evt)
        {
            if (evt.type != EventType.KeyDown)
                return false;

            if (AutoComplete.enabled)
                return false;

            if (m_Selection.Count != 0 || results.Count == 0)
                return false;

            if (evt.keyCode != KeyCode.KeypadEnter && evt.keyCode != KeyCode.Return)
                return false;

            var item = results.ElementAt(0);
            if (item.provider.actions.Count == 0)
                return false;

            SearchAction action = item.provider.actions[0];
            if (context.actionId != null)
                action = SearchService.GetAction(item.provider, context.actionId);

            if (action == null)
                return false;

            evt.Use();
            ExecuteAction(action, new [] { item });
            GUIUtility.ExitGUI();
            return true;
        }

        private void HandleKeyboardNavigation(Event evt)
        {
            if (!evt.isKey)
                return;

            // Ignore tabbing and line return in quicksearch
            if (evt.keyCode == KeyCode.None && (evt.character == '\t' || (int)evt.character == 10))
                evt.Use();

            if (AutoComplete.HandleKeyEvent(evt))
                return;

            if (HandleDefaultPressEnter(evt))
                return;

            if (SearchField.HandleKeyEvent(evt))
                return;

            if (evt.type == EventType.KeyDown)
            {
                var ctrl = evt.control || evt.command;
                if (evt.keyCode == KeyCode.Escape)
                {
                    m_CurrentSearchEvent.endSearchWithKeyboard = true;
                    selectCallback?.Invoke(null, true);
                    selectCallback = null;
                    evt.Use();
                    CloseSearchWindow();
                }
                else if (evt.keyCode == KeyCode.F1)
                {
                    SetSearchText("?");
                    evt.Use();
                }
                else if (ctrl && evt.keyCode == KeyCode.S)
                {
                    PopSearchQueryCreateWindow();
                    evt.Use();
                }
                else if (evt.keyCode == KeyCode.LeftArrow && evt.modifiers.HasFlag(EventModifiers.Alt))
                {
                    PopFilterWindow();
                    evt.Use();
                }
                else if (evt.keyCode == KeyCode.Tab && evt.modifiers == EventModifiers.None)
                {
                    if (AutoComplete.Show(context, position))
                        evt.Use();
                }
            }

            if (evt.type != EventType.Used)
                m_ResultView.HandleInputEvent(evt, m_Selection);

            if (m_FilteredItems.Count == 0)
                SelectSearch();
        }

        /// <summary>
        /// Request to focus and select the search field.
        /// </summary>
        public void SelectSearch()
        {
            m_SearchBoxFocus = true;
        }

        private void CloseSearchWindow()
        {
            if (s_FocusedWindow)
                s_FocusedWindow.Focus();
            Close();
        }

        private bool IsDisplayGrid()
        {
            return m_ItemSize >= 32;
        }

        private void DrawHelpText()
        {
            const string help = "Search {0}!\r\n\r\n" +
                "- <b>Alt + Up/Down Arrow</b> \u2192 Search history\r\n" +
                "- <b>Alt + Left</b> \u2192 Filter\r\n" +
                "- <b>Alt + Right</b> \u2192 Actions menu\r\n" +
                "- <b>Enter</b> \u2192 Default action\r\n" +
                "- <b>Alt + Enter</b> \u2192 Secondary action\r\n" +
                "- Drag items around\r\n" +
                "- Type <b>?</b> to get help\r\n";

            if (String.IsNullOrEmpty(context.searchText.Trim()))
            {
                GUILayout.Box(string.Format(help, searchTopic), Styles.noResult, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            else if (context.actionId != null)
            {
                GUILayout.Box("Waiting for a command...", Styles.noResult, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            else
            {
                GUILayout.Box("No result for query \"" + context.searchText + "\"\n" + "Try something else?",
                              Styles.noResult, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
        }

        private void DrawItems(float sliderPos)
        {
            if (m_FilteredItems.Count > 0)
            {
                if (m_ResultView != null)
                    m_ResultView.Draw(m_Selection, sliderPos);
            }
            else
            {
                DrawHelpText();
            }
        }

        private void SetDetailsViewSplitterPosition(float newPos)
        {
            var minSize = Mathf.Max(200f, position.width * 0.2f);
            var maxSize = position.width * 0.9f;
            var previousSize = m_DetailsViewSplitterPos;
            m_DetailsViewSplitterPos = Mathf.Max(minSize, Mathf.Min(newPos, maxSize));
            if (previousSize != m_DetailsViewSplitterPos)
                Repaint();
        }

        private void OnWindowResized(Vector2 oldSize, Vector2 newSize)
        {
            var widthDiff = newSize.x - oldSize.x;
            if (position.width > m_DetailsViewShowMinSize && m_DetailsViewSplitterPos > 0f)
                SetDetailsViewSplitterPosition(m_DetailsViewSplitterPos + widthDiff);
        }

        private void DrawDetailsViewSplitter(Event evt)
        {
            var sliderRect = new Rect(m_DetailsViewSplitterPos - 2f, m_ResultView.rect.y, 3f, m_ResultView.rect.height);
            EditorGUIUtility.AddCursorRect(sliderRect, MouseCursor.ResizeHorizontal);

            if (evt.type == EventType.MouseDown && sliderRect.Contains(evt.mousePosition))
                m_DetailsViewSplitterResize = true;

            if (m_DetailsViewSplitterResize)
                SetDetailsViewSplitterPosition(evt.mousePosition.x);

            if (evt.type == EventType.MouseUp)
                m_DetailsViewSplitterResize = false;

            if (evt.type == EventType.Repaint && !m_ResultView.scrollbarVisible)
            {
                var sliderDrawRect = new Rect(m_DetailsViewSplitterPos - 4f, m_ResultView.rect.y, 1f, m_ResultView.rect.height);
                GUI.DrawTexture(sliderDrawRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, false, 0f, Styles.sliderColor, 0f, 0f);
            }
        }

        private Rect DrawToolbar(Event evt)
        {
            if (context == null)
                return Rect.zero;

            var searchTextRect = Rect.zero;
            using (new GUILayout.HorizontalScope(Styles.toolbar))
            {
                var rightRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(32f), GUILayout.ExpandHeight(true));
                if (EditorGUI.DropdownButton(rightRect, Styles.filterButtonContent, FocusType.Passive, Styles.filterButton) || m_ShowFilterWindow)
                {
                    if (FilterWindow.canShow)
                    {
                        m_ShowFilterWindow = false;
                        rightRect.x += 12f; rightRect.y -= 3f;
                        if (FilterWindow.ShowAtPosition(this, context, rightRect))
                            GUIUtility.ExitGUI();
                    }
                }

                searchTextRect = GUILayoutUtility.GetRect(position.width, Styles.searchField.fixedHeight, Styles.searchField,
                    GUILayout.MaxWidth(position.width - Styles.kSearchFieldWidthOffset), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                var previousSearchText = context.searchText;
                if (evt.type != EventType.KeyDown || evt.keyCode != KeyCode.None || evt.character != '\r')
                    context.searchText = SearchField.Draw(searchTextRect, context.searchText, Styles.searchField);

                if (String.IsNullOrEmpty(context.searchText))
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.TextArea(searchTextRect, $"search {searchTopic}...", Styles.placeholderTextStyle);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    DrawSearchQueryToolbar();

                    if (GUILayout.Button(Icons.clear, Styles.searchFieldBtn, GUILayout.Width(Styles.kSearchBoxBtnSize), GUILayout.Height(Styles.kSearchBoxBtnSize)))
                    {
                        AutoComplete.Clear();
                        context.searchText = "";
                        GUI.changed = true;
                        GUI.FocusControl(null);
                    }
                }

                if (String.Compare(previousSearchText, context.searchText, StringComparison.Ordinal) != 0)
                {
                    SetSelection();
                    DebouncedRefresh();
                }
            }

            return searchTextRect;
        }

        private void DrawSearchQueryToolbar()
        {
            var searchQueryRect = EditorGUILayout.GetControlRect(false, Styles.kSearchBoxBtnSize, Styles.searchFieldBtn, GUILayout.Width(Styles.kSearchBoxBtnSize));
            if (EditorGUI.DropdownButton(searchQueryRect, Styles.createSearchQueryContent, FocusType.Passive, Styles.searchFieldBtn) || m_ShowCreateWindow)
            {
                if (SearchQueryCreateWindow.canShow)
                {
                    searchQueryRect.x = searchQueryRect.x - SearchQueryCreateWindow.Styles.windowSize.x + searchQueryRect.width;
                    searchQueryRect.y += 8f;
                    m_ShowCreateWindow = false;
                    var screenRect = new Rect(GUIUtility.GUIToScreenPoint(searchQueryRect.position), searchQueryRect.size);
                    SearchQueryCreateWindow.ShowAtPosition(this, context, screenRect);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void DebouncedRefresh()
        {
            EditorApplication.update -= DebouncedRefresh;
            if (!this)
                return;

            if (SearchSettings.debounceMs == 0)
            {
                Refresh();
                return;
            }

            var currentTime = EditorApplication.timeSinceStartup;
            if (m_DebounceTime != 0 && currentTime - m_DebounceTime > (SearchSettings.debounceMs / 1000.0f))
            {
                Refresh();
                m_DebounceTime = 0;
            }
            else
            {
                if (m_DebounceTime == 0)
                    m_DebounceTime = currentTime;
                EditorApplication.update += DebouncedRefresh;
            }
        }

        private static bool IsObjectMatchingType(SearchItem item, Type filterType)
        {
            if (item == SearchItem.none)
                return true;
            var obj = Utils.ToObject(item, filterType);
            if (!obj)
                return false;
            var objType = obj.GetType();
            return objType == filterType || obj.GetType().IsSubclassOf(filterType);
        }

        private bool LoadSessionSettings()
        {
            if (Utils.IsRunningTests())
                return false;
            context.ResetFilter(true);
            foreach (var f in SearchSettings.filters)
                context.SetFilter(f.Key, f.Value);
            SetSearchText(SearchSettings.GetScopeValue(k_LastSearchPrefKey, context.scopeHash, ""));
            return true;
        }

        private void SaveSessionSettings()
        {
            if (Utils.IsRunningTests())
                return;

            SearchSettings.SetScopeValue(k_LastSearchPrefKey, context.scopeHash, context.searchText);
            if (saveFilters)
            {
                foreach (var p in SearchService.Providers.Where(p => p.active && !p.isExplicitProvider))
                    SearchSettings.filters[p.name.id] = context.IsEnabled(p.name.id);
            }
            SearchSettings.Save();
        }

        private void UpdateItemSize(float value)
        {
            var oldMode = displayMode;
            m_ItemSize = value;
            var newMode = displayMode;
            if (m_ResultView == null || oldMode != newMode)
            {
                if (newMode == DisplayMode.List)
                    m_ResultView = new ListView(this);
                else if (newMode == DisplayMode.Grid)
                    m_ResultView = new GridView(this);
            }
        }

        [InitializeOnLoadMethod]
        private static void OpenQuickSearchFirstUse()
        {
            if (Utils.IsRunningTests())
                return;

            var quickSearchFirstUseTokenPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", "Library", "~quicksearch.new"));
            var hasQuickSearchFirstUseToken = System.IO.File.Exists(quickSearchFirstUseTokenPath);

            if (!SearchSettings.onBoardingDoNotAskAgain)
            {
                EditorApplication.delayCall += () =>
                {
                    if (AssetDatabase.FindAssets($"t:{nameof(SearchDatabase)} a:assets").Length == 0)
                        EditorApplication.delayCall += () => OnBoardingWindow.OpenWindow();

                    SearchSettings.onBoardingDoNotAskAgain = true;
                    SearchSettings.Save();
                };
            }
            else if (System.IO.File.Exists(quickSearchFirstUseTokenPath))
                EditorApplication.delayCall += () => OpenQuickSearch();

            if (hasQuickSearchFirstUseToken)
                System.IO.File.Delete(quickSearchFirstUseTokenPath);
        }

        [UsedImplicitly, CommandHandler(nameof(OpenQuickSearch))]
        private static void OpenQuickSearchCommand(CommandExecuteContext c)
        {
            OpenDefaultQuickSearch();
        }

        [UsedImplicitly, Shortcut("Help/Quick Search", KeyCode.O, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        private static void OpenQuickSearch()
        {
            OpenDefaultQuickSearch();
        }

        private static QuickSearch OpenDefaultQuickSearch()
        {
            return Open(dockable: SearchSettings.dockable, reuseExisting: SearchSettings.dockable);
        }

        [Shortcut("Help/Quick Search Contextual", KeyCode.C, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        private static void OpenContextual()
        {
            var contextualProviders = SearchService.Providers.Where(p => p.active && (p.isEnabledForContextualSearch?.Invoke() ?? false));
            OpenWithContextualProvider(contextualProviders.Select(p => p.name.id).ToArray());
        }

        /// <summary>
        /// Dispose of the QuickSearch window and context.
        /// </summary>
        /// <param name="disposing">Is the Window already being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if (disposing)
                Close();

            m_Disposed = true;
        }

        /// <summary>
        /// Dispose of the QuickSearch window and context.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}

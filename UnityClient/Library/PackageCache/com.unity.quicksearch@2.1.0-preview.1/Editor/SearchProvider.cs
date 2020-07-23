using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Unity.QuickSearch
{
    /// <summary>
    /// Defines at set of options that indicates to the search provider how the preview should be fetched.
    /// </summary>
    [Flags]
    public enum FetchPreviewOptions
    {
        /// <summary>
        /// No options are defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the provider should generate a 2D preview.
        /// </summary>
        Preview2D = 1,

        /// <summary>
        /// Indicates that the provider should generate a 3D preview.
        /// </summary>
        Preview3D = 1 << 1,

        /// <summary>
        /// Indicate that the preview size should be around 128x128.
        /// </summary>
        Normal    = 1 << 10,

        /// <summary>
        /// Indicates that the preview resolution should be higher than 256x256.
        /// </summary>
        Large     = 1 << 11
    }

    /// <summary>
    /// Defines what details are shown in the details panel for the search view.
    /// </summary>
    [Flags]
    public enum ShowDetailsOptions
    {
        /// <summary>
        /// No options are defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// Show a large preview.
        /// </summary>
        Preview = 1,

        /// <summary>
        /// Show an embedded inspector for the selected object.
        /// </summary>
        Inspector = 1 << 1,

        /// <summary>
        /// Show selected item possible actions
        /// </summary>
        Actions = 1 << 2,

        /// <summary>
        /// Show an extended item description
        /// </summary>
        Description = 1 << 3,

        /// <summary>
        /// Default set of options used when [[showDetails]] is set to true.
        /// </summary>
        Default = Preview | Actions | Description
    }

    /// <summary>
    /// The name entry holds a name and an identifier at once.
    /// </summary>
    [DebuggerDisplay("{displayName} ({id})")]
    public class NameEntry
    {
        /// <summary>
        /// Construct a new name identifier
        /// </summary>
        /// <param name="id">Unique id of the Entry.</param>
        /// <param name="displayName">Name to display in UI.</param>
        /// <param name="enabled">Is the component enabled.</param>
        public NameEntry(string id, string displayName = null, bool enabled = true)
        {
            this.id = id;
            this.displayName = displayName ?? id;
            this.isEnabled = enabled;
        }

        /// <summary> Unique name for an object </summary>
        public string id;

        /// <summary> Display name (use by UI) </summary>
        public string displayName;

        /// <summary>Indicates if the entry is enabled</summary>
        public bool isEnabled;
    }

    /// <summary>
    /// SearchProvider manages search for specific type of items and manages thumbnails, description and subfilters, etc.
    /// </summary>
    [DebuggerDisplay("{name.id}")]
    public class SearchProvider
    {
        internal const int k_RecentUserScore = -99;

        internal SearchContext defaultContext;

        /// <summary>
        /// Create a new SearchProvider
        /// </summary>
        /// <param name="id">Search Provider unique id.</param>
        public SearchProvider(string id)
            : this(id, null, (Func<SearchContext, List<SearchItem>, SearchProvider, object>)null)
        {
        }

        /// <summary>
        /// Create a new SearchProvider
        /// </summary>
        /// <param name="id">Search Provider unique id.</param>
        /// <param name="displayName">Provider pretty name, use to display in UI.</param>
        public SearchProvider(string id, string displayName)
            : this(id, displayName, (Func<SearchContext, List<SearchItem>, SearchProvider, object>)null)
        {
        }

        /// <summary>
        /// Create a new SearchProvider
        /// </summary>
        /// <param name="id">Search Provider unique id.</param>
        /// <param name="fetchItemsHandler">Handler responsible to populate a list of SearchItems according to a query.</param>
        public SearchProvider(string id, Func<SearchContext, List<SearchItem>, SearchProvider, object> fetchItemsHandler)
            : this(id, null, fetchItemsHandler)
        {
        }

        /// <summary>
        /// Create a new SearchProvider
        /// </summary>
        /// <param name="id">Search Provider unique id.</param>
        /// <param name="fetchItemsHandler">Handler responsible to populate a list of SearchItems according to a query.</param>
        public SearchProvider(string id, Func<SearchContext, SearchProvider, object> fetchItemsHandler)
            : this(id, null, (context, items, provider) => fetchItemsHandler(context, provider))
        {
        }

        /// <summary>
        /// Create a new SearchProvider
        /// </summary>
        /// <param name="id">Search Provider unique id.</param>
        /// <param name="displayName">Provider pretty name, use to display in UI.</param>
        /// <param name="fetchItemsHandler">Handler responsible to populate a list of SearchItems according to a query.</param>
        public SearchProvider(string id, string displayName, Func<SearchContext, SearchProvider, object> fetchItemsHandler)
            : this(id, displayName, (context, items, provider) => fetchItemsHandler(context, provider))
        {
        }

        /// <summary>
        /// Create a new SearchProvider
        /// </summary>
        /// <param name="id">Search Provider unique id.</param>
        /// <param name="displayName">Provider pretty name, use to display in UI.</param>
        /// <param name="fetchItemsHandler">Handler responsible to populate a list of SearchItems according to a query.</param>
        public SearchProvider(string id, string displayName, Func<SearchContext, List<SearchItem>, SearchProvider, object> fetchItemsHandler)
        {
            if (String.IsNullOrEmpty(id))
                throw new ArgumentException("provider id must be non-empty", nameof(id));

            active = true;
            name = new NameEntry(id, displayName);
            actions = new List<SearchAction>();
            fetchItems = fetchItemsHandler ?? ((context, items, provider) => null);
            fetchThumbnail = (item, context) => item.thumbnail ?? Icons.quicksearch;
            fetchPreview = null;
            fetchLabel = (item, context) => item.label ?? item.id ?? String.Empty;
            fetchDescription = (item, context) => item.description ?? String.Empty;
            priority = 100;
            showDetails = false;
            showDetailsOptions = ShowDetailsOptions.Default;
            filterId = $"{id}:";

            defaultContext = new SearchContext(this);
        }

        /// <summary>
        /// Helper function to create a new search item for the current provider.
        /// </summary>
        /// <param name="id">Unique id of the search item. This is used to remove duplicates to the user view.</param>
        /// <param name="context">Search context from the query that generates this item.</param>
        /// <param name="score">Score of the search item. The score is used to sort all the result per provider. Lower score are shown first.</param>
        /// <param name="label">The search item label is displayed on the first line of the search item UI widget.</param>
        /// <param name="description">The search item description is displayed on the second line of the search item UI widget.</param>
        /// <param name="thumbnail">The search item thumbnail is displayed left to the item label and description as a preview.</param>
        /// <param name="data">User data used to recover more information about a search item. Generally used in fetchLabel, fetchDescription, etc.</param>
        /// <returns>The newly created search item attached to the current search provider.</returns>
        public SearchItem CreateItem(SearchContext context, string id, int score, string label, string description, Texture2D thumbnail, object data)
        {
            #if false // Debug sorting
            description = $"DEBUG: id={id} - label={label} - description={description} - thumbnail={thumbnail} - data={data}";
            label = $"{label ?? id} ({score})";
            #endif

            return new SearchItem(id)
            {
                score = score,
                label = label,
                description = description,
                options = SearchItemOptions.Highlight | SearchItemOptions.Ellipsis,
                thumbnail = thumbnail,
                data = data,
                provider = this,
                context = context
            };
        }

        /// <summary>
        /// Helper function to create a new search item for the current provider.
        /// </summary>
        /// <param name="id">Unique id of the search item. This is used to remove duplicates to the user view.</param>
        /// <param name="score">Score of the search item. The score is used to sort all the result per provider. Lower score are shown first.</param>
        /// <param name="label">The search item label is displayed on the first line of the search item UI widget.</param>
        /// <param name="description">The search item description is displayed on the second line of the search item UI widget.</param>
        /// <param name="thumbnail">The search item thumbnail is displayed left to the item label and description as a preview.</param>
        /// <param name="data">User data used to recover more information about a search item. Generally used in fetchLabel, fetchDescription, etc.</param>
        /// <returns>The newly created search item attached to the current search provider.</returns>
        public SearchItem CreateItem(string id, int score, string label, string description, Texture2D thumbnail, object data)
        {
            return CreateItem(defaultContext, id, score, label, description, thumbnail, data);
        }

        /// <summary>
        /// Helper function to create a new search item for the current provider.
        /// </summary>
        /// <param name="context">Search context from the query that generates this item.</param>
        /// <param name="id">Unique id of the search item. This is used to remove duplicates to the user view.</param>
        /// <returns>The newly created search item attached to the current search provider.</returns>
        public SearchItem CreateItem(SearchContext context, string id)
        {
            return CreateItem(context, id, 0, null, null, null, null);
        }

        /// <summary>
        /// Helper function to create a new search item for the current provider.
        /// </summary>
        /// <param name="id">Unique id of the search item. This is used to remove duplicates to the user view.</param>
        /// <returns>The newly created search item attached to the current search provider.</returns>
        public SearchItem CreateItem(string id)
        {
            return CreateItem(defaultContext, id, 0, null, null, null, null);
        }

        /// <summary>
        /// Helper function to create a new search item for the current provider.
        /// </summary>
        /// <param name="id">Unique id of the search item. This is used to remove duplicates to the user view.</param>
        /// <param name="label">The search item label is displayed on the first line of the search item UI widget.</param>
        /// <returns>The newly created search item attached to the current search provider.</returns>
        public SearchItem CreateItem(string id, string label)
        {
            return CreateItem(defaultContext, id, 0, label, null, null, null);
        }

        /// <summary>
        /// Helper function to create a new search item for the current provider.
        /// </summary>
        /// <param name="id">Unique id of the search item. This is used to remove duplicates to the user view.</param>
        /// <param name="label">The search item label is displayed on the first line of the search item UI widget.</param>
        /// <param name="description">The search item description is displayed on the second line of the search item UI widget.</param>
        /// <param name="thumbnail">The search item thumbnail is displayed left to the item label and description as a preview.</param>
        /// <param name="data">User data used to recover more information about a search item. Generally used in fetchLabel, fetchDescription, etc.</param>
        /// <returns>The newly created search item attached to the current search provider.</returns>
        public SearchItem CreateItem(string id, string label, string description, Texture2D thumbnail, object data)
        {
            return CreateItem(defaultContext, id, 0, label, description, thumbnail, data);
        }

        /// <summary>
        /// Create a Search item that will be bound to the SeaechProvider.
        /// </summary>
        /// <param name="context">Search context from the query that generates this item.</param>
        /// <param name="id">Unique id of the search item. This is used to remove duplicates to the user view.</param>
        /// <param name="label">The search item label is displayed on the first line of the search item UI widget.</param>
        /// <param name="description">The search item description is displayed on the second line of the search item UI widget.</param>
        /// <param name="thumbnail">The search item thumbnail is displayed left to the item label and description as a preview.</param>
        /// <param name="data">User data used to recover more information about a search item. Generally used in fetchLabel, fetchDescription, etc.</param>
        /// <returns>New SearchItem</returns>
        public SearchItem CreateItem(SearchContext context, string id, string label, string description, Texture2D thumbnail, object data)
        {
            return CreateItem(context, id, 0, label, description, thumbnail, data);
        }

        internal void RecordFetchTime(double t)
        {
            fetchTime  = t;
        }

        /// <summary> Unique id of the provider.</summary>
        public NameEntry name;

        /// <summary>
        /// Indicates if the provider is active or not. Inactive providers are completely ignored by the search service. The active state can be toggled in the search settings.
        /// </summary>
        public bool active;

        /// <summary> Text token use to "filter" a provider (ex:  "me:", "p:", "s:")</summary>
        public string filterId;

        /// <summary> This provider is only active when specified explicitly using his filterId</summary>
        public bool isExplicitProvider;

        /// <summary> Indicates if the provider can show additional details or not.</summary>
        public bool showDetails;

        /// <summary> Explicitly define details options to be shown</summary>
        /// TODO: Move these options to the item options
        public ShowDetailsOptions showDetailsOptions = ShowDetailsOptions.Default;

        /// <summary> Handler used to fetch and format the label of a search item.</summary>
        public Func<SearchItem, SearchContext, string> fetchLabel;

        /// <summary>
        /// Handler to provider an async description for an item. Will be called when the item is about to be displayed.
        /// Allows a plugin provider to only fetch long description when they are needed.
        /// </summary>
        public Func<SearchItem, SearchContext, string> fetchDescription;

        /// <summary>
        /// Handler to provider an async thumbnail for an item. Will be called when the item is about to be displayed.
        /// Compared to preview a thumbnail should be small and returned as fast as possible. Use fetchPreview if you want to generate a preview that is bigger and slower to return.
        /// Allows a plugin provider to only fetch/generate preview when they are needed.
        /// </summary>
        public Func<SearchItem, SearchContext, Texture2D> fetchThumbnail;

        /// <summary>
        /// Similar to fetchThumbnail, fetchPreview usually returns a bigger preview. The QuickSearch UI will progressively show one preview each frame,
        /// preventing the UI to block if many preview needs to be generated at the same time.
        /// </summary>
        public Func<SearchItem, SearchContext, Vector2, FetchPreviewOptions, Texture2D> fetchPreview;

        /// <summary> If implemented, it means the item supports drag. It is up to the SearchProvider to properly setup the DragAndDrop manager.</summary>
        public Action<SearchItem, SearchContext> startDrag;

        /// <summary> Called when the selection changed and can be tracked.</summary>
        public Action<SearchItem, SearchContext> trackSelection;

        /// <summary> MANDATORY: Handler to get items for a given search context.
        /// The return value is an object that can be of type IEnumerable or IEnumerator.
        /// The enumeration of those objects should return SearchItems.
        /// </summary>
        public Func<SearchContext, List<SearchItem>, SearchProvider, object> fetchItems;

        /// <summary> Returns any valid Unity object held by the search item.</summary>
        public Func<SearchItem, Type, UnityEngine.Object> toObject;

        /// <summary>
        /// This callback is used to open additional context for a given item.
        /// </summary>
        public Func<SearchSelection, Rect, bool> openContextual;

        /// <summary>
        /// Provider can return a list of words that will help the user complete his search query
        /// </summary>
        [Obsolete("GetKeywords is deprecated. Define fetchPropositions on your provider instead.")]
        public Action<SearchContext, string, List<string>> fetchKeywords;

        /// <summary>
        /// Provider can return a list of words that will help the user complete his search query.
        /// </summary>
        internal Func<SearchContext, SearchPropositionOptions, IEnumerable<SearchProposition>> fetchPropositions;

        /// <summary>
        /// Called when the QuickSearchWindow is opened. Allow the Provider to perform some caching.
        /// </summary>
        public Action onEnable;

        /// <summary>
        /// Called when the QuickSearchWindow is closed. Allow the Provider to release cached resources.
        /// </summary>
        public Action onDisable;

        /// <summary>
        /// Hint to sort the Provider. Affect the order of search results and the order in which provider are shown in the FilterWindow.
        /// </summary>
        public int priority;

        /// <summary>
        /// Called when quick search is invoked in "contextual mode". If you return true it means the provider is enabled for this search context.
        /// </summary>
        public Func<bool> isEnabledForContextualSearch;

        // INTERNAL
        internal List<SearchAction> actions;
        internal double fetchTime;
        internal double loadTime;
        internal double enableTime;
        internal int m_EnableLockCounter;
        internal static int sessionCounter;

        internal void OnEnable(double enableTimeMs)
        {
            try
            {
                if (m_EnableLockCounter == 0)
                    onEnable?.Invoke();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                enableTime = enableTimeMs;
                ++m_EnableLockCounter;
                ++sessionCounter;
            }
        }

        internal void OnDisable()
        {
            --m_EnableLockCounter;
            --sessionCounter;
            if (m_EnableLockCounter == 0)
            {
                try
                {
                    onDisable?.Invoke();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
        }
    }
}
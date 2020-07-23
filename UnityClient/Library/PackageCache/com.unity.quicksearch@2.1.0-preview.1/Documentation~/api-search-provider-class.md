# The SearchProvider class

 The **[SearchProvider](../api/Unity.QuickSearch.SearchProvider.html)** class executes searches for specific types of items, and manages thumbnails, descriptions, and sub-filters.

 It has the following basic API:

```CSharp
public class SearchProvider
{
    public SearchProvider(string id, string displayName = null);

    // Creates an Item bound to this provider.
    public SearchItem CreateItem(string id, string label = null, string description = null, Texture2D thumbnail = null);

    // Utility functions to check whether the search text matches a string.
    public static bool MatchSearchGroups(string searchContext, string content);
    public static bool MatchSearchGroups(string searchContext, string content,
                                        out int startIndex, out int endIndex);

    // The provider's unique ID.
    public NameId name;
    // Text token to "filter" a provider (for example, "me:", "p:", and "s:").
    public string filterId;
    // This provider is only active when a search explicitly specifies it with
    // its filterId.
    public bool isExplicitProvider;
    // Handler to fetch and format the label of a search item.
    public FetchStringHandler fetchLabel;
    // Handler to provide an async description for an item. Called just before
    // Quick Search displays the item.
    // Allows a plug-in provider to fetch long descriptions only when Quick
    // Search needs them.
    public FetchStringHandler fetchDescription;
    // Handler to provider an async thumbnail for an item. Called just before
    // Quick Search displays the item.
    // Allows a plug-in provider to fetch/generate previews only when Quick
    // Search needs them.
    public PreviewHandler fetchThumbnail;
    // Handler to support drag interactions. It is up to the SearchProvider
    // to properly set up the DragAndDrop manager.
    public StartDragHandler startDrag;
    // Called when the selection changes and Quick Search can track it.
    public TrackSelectionHandler trackSelection;
    // MANDATORY: Handler to get items for a search context.
    public GetItemsHandler fetchItems;
    // A Search Provider can return a list of words that help the user complete
    // their search query.
    public GetKeywordsHandler fetchKeywords;
    // List of sub-filters that are visible in the FilterWindow for a
    // SearchProvider (see AssetProvider for an example).
    public List<NameId> subCategories;
    // Called when the Quick Search window opens. Allows the Provider to perform
    // some caching.
    public Action onEnable;
    // Called when the Quick Search window closes. Allows the Provider to release
    // cached resources.
    public Action onDisable;
    // Int to sort the Provider. Affects the order of search results and the
    // order in which providers are shown in the FilterWindow.
    public int priority;
    // Called when Quick Search opens in "contextual mode". If you return true
    // it means the provider is enabled for this search context.
    public IsEnabledForContextualSearch isEnabledForContextualSearch;
}
```

## Caching and releasing resources

When you launch the Quick Search window, it calls [`onEnable`](../api/Unity.QuickSearch.SearchProvider.html?q=onenable#Unity_QuickSearch_SearchProvider_onEnable), which you can use to cache resources.

When you close the Quick Search window, it calls [`onDisable`](../api/Unity.QuickSearch.SearchProvider.html?q=onenable#Unity_QuickSearch_SearchProvider_onDisable), which you can use to release resources.

## Initialization

Because the Quick Search item list uses a virtual scrolling algorithm, some [`SearchItem`](../api/Unity.QuickSearch.SearchItem.html) fields (for example, [`label`](../api/Unity.QuickSearch.SearchItem.html#Unity_QuickSearch_SearchItem_label), [`thumbnail`](../api/Unity.QuickSearch.SearchItem.html#Unity_QuickSearch_SearchItem_thumbnail), and  [`description`](../api/Unity.QuickSearch.SearchItem.html#Unity_QuickSearch_SearchItem_description)) are fetched on demand, if they are not already provided.

To populate those fields after the items are created, you need to initialize the [`SearchProvider`](../api/Unity.QuickSearch.SearchProvider.html) with specific handlers ([`fetchLabel`](../api/Unity.QuickSearch.SearchProvider.html#Unity_QuickSearch_SearchProvider_fetchLabel), [`fetchDescription`](../api/Unity.QuickSearch.SearchProvider.html#Unity_QuickSearch_SearchProvider_fetchDescription), [`fetchThumbnail`](../api/Unity.QuickSearch.SearchProvider.html#Unity_QuickSearch_SearchProvider_fetchThumbnail)).

## Tracking item selection

You can register a callback on [`trackSelection`](../api/Unity.QuickSearch.SearchProvider.html#Unity_QuickSearch_SearchProvider_trackSelection) to have Quick Search do something whenever you select an item in the search results using the mouse or the keyboard. For example, the [Asset](search-assets.md) and [Scene](search-scene.md) providers use the [`trackSelection`](../api/Unity.QuickSearch.SearchProvider.html#Unity_QuickSearch_SearchProvider_trackSelection) callback to ping the selected item in QuickSearch.

## Enabling drag and drop

Some Search Providers return items that you can drag and drop into the Scene. If you are creating a custom provider whose items support drag and drop, implement [`startDrag`](../api/Unity.QuickSearch.SearchProvider.html#Unity_QuickSearch_SearchProvider_startDrag).

For example, the [Asset](search-assets.md) and [Scene](search-scene.md) providers populate the `DragAndDrop` structure with the relevant item UIDs to allow proper drag and drop interactions.

## Including a provider in a contextual search

When you open the Quick Search window with the **Alt Shift + C** shortcut, it starts a contextual search, meaning Quick Search searches the window that has focus.

When you launch a contextual search, providers that override [`isEnabledForContextualSearch`](../api/Unity.QuickSearch.SearchProvider.html#Unity_QuickSearch_SearchProvider_isEnabledForContextualSearch) check to see if they should be enabled, as in the following example:

```CSharp
// Taken from Scene hierarchy provider:
// Makes the provider part of the contextual search if the Scene view or the
// Hierarchy window has focus.
isEnabledForContextualSearch = () =>
                QuickSearchTool.IsFocusedWindowTypeName("SceneView") ||
                QuickSearchTool.IsFocusedWindowTypeName("SceneHierarchyWindow");
```
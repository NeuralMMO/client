# Performing a search

Search Providers use the [`fetchItems`](../api/Unity.QuickSearch.SearchProvider.html) function to search for items and filter the results. The [`fetchItems`](../api/Unity.QuickSearch.SearchProvider.html) function has the following signature:

```CSharp
// context: the necessary search context (for example, tokenized search and
// sub-filters).
// items: list of items to populate (if not using the asynchronous api)
// provider: the Search Provider itself
public delegate IEnumerable<SearchItem> GetItemsHandler(SearchContext context,
                                    List<SearchItem> items,
                                    SearchProvider provider);
```

The [`SearchProvider`](../api/Unity.QuickSearch.SearchProvider.html) must add new [`SearchItem`](../api/Unity.QuickSearch.SearchItem.html)s to the `items` list or return an `IEnumerable<SearchItem>`.

> [!NOTE]
> If you do not use the asynchronous `fetchItems` api, you must return `null` in your `fetchItems` function.

A `SearchItem` is a simple struct:

```CSharp
public struct SearchItem
{
    public readonly string id;
    // The item score affects how Quick Search sorts the item within the results from the Search Provider.
    public int score;
    // Optional: Display name of the item. If the item does not have one,
    // SearchProvider.fetchLabel is called).
    public string label;
    // If the item does not have a description SearchProvider.fetchDescription
    // is called when Quick Search first displays the item.
    public string description;
    // If true, the description already has rich text formatting.
    public SearchItemDescriptionFormat descriptionFormat;
    // If the item does not have a thumbnail, SearchProvider.fetchThumbnail
    // is called when Quick Search first displays the item.
    public Texture2D thumbnail;
    // Search Provider user-customizable content
    public object data;
}
```
A `SearchItem` only requires the `id`.

> [!TIP]
> When you filter according to [`SearchContext.searchText`](../api/Unity.QuickSearch.SearchContext.html#Unity_QuickSearch_SearchContext_searchText) use the static function [`SearchProvider.MatchSearchGroup`](/api/Unity.QuickSearch.SearchProvider.html#Unity_QuickSearch_SearchProvider_MatchSearchGroups_Unity_QuickSearch_SearchContext_System_String_System_Boolean_) which makes a partial search.

## Using fuzzy search

To use fuzzy search on an item, you can use [`FuzzySearch.FuzzyMatch`](../api/Unity.QuickSearch.FuzzySearch.html#methods), as in the following example:

```CSharp
if (FuzzySearch.FuzzyMatch(sq, CleanString(item.label), ref score, matches))
    item.label = RichTextFormatter.FormatSuggestionTitle(item.label, matches);
```

All search items are sorted against items of the same provider with their `score`. The **lower score** appears at the top of the item list (**ascending sorting**).

## Asynchronous search API

You can use the asynchronous `fetchItems` API when a Search Provider takes a long time to compute its results, or relies on an asynchronous search engine such as WebRequests.

To use the asynchronous API, have the `fetchItems` function return an `IEnumerable<SearchItem>`. The `IEnumerable<SearchItem>` should be a function that yields results, so that the API can fetch one item at a time.

When an `IEnumerable<SearchItem>` is returned, the enumerator is stored and iterated over during an application update. Enumeration continues over multiple application updates until it is finished.

The iterating time is constrained to ensure the UI is not blocked. However, because the call is in the main thread, you should make sure to yield as soon as possible if the results are not ready.

The following example demonstrates how to use the asynchronous `fetchItems` API:

```CSharp
public class AsyncSearchProvider : SearchProvider
{
    public AsyncSearchProvider(string id, string displayName = null)
        : base(id, displayName)
    {
        fetchItems = (context, items, provider) => FetchItems(context, provider);
    }

    private IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
    {
        while(ResultsNotReady())
        {
            yield return null;
        }

        var oneItem = // Get an item
        yield return oneItem;

        var anotherItem = // Get another item
        yield return anotherItem;

        if(SomeConditionThatBreaksTheSearch())
        {
            // Search must be terminated
            yield break;
        }

        // You can iterate over an enumerable. The enumeration
        // continues where it left.
        foreach(var item in someItems)
        {
            yield return item;
        }
    }
}
```

You can find additional examples in the Quick Search package. Use the Project view to navigate to: `Packages/Quick Search/Editor/Providers/Examples/`.

- `AssetStoreProvider.cs`: queries the Asset Store using WebRequest.
- `ESS.cs`: creates a process to start the Entrian Source search indexer, which provides full text search for Assets in your Project.

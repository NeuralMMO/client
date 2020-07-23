# Registering a Search Provider

To add a new Search Provider, create a function and tag it with the [`[SearchItemProvider]`](../api/Unity.QuickSearch.SearchItemProviderAttribute.html) attribute, as in the following example:

```CSharp
[SearchItemProvider]
internal static SearchProvider CreateProvider()
{
    return new SearchProvider(type, displayName)
    {
        filterId = "me:",
        fetchItems = (context, items, provider) =>
        {
            var itemNames = new List<string>();
            var shortcuts = new List<string>();
            GetMenuInfo(itemNames, shortcuts);

            items.AddRange(itemNames.Where(menuName =>
                    SearchProvider.MatchSearchGroups(context.searchText, menuName))
                .Select(menuName => provider.CreateItem(menuName,
                                            Path.GetFileName(menuName), menuName)));
        },

        fetchThumbnail = (item, context) => Icons.shortcut
    };
}
```

- The function must return a new  [`SearchProvider`] instance.
- The `SearchProvider` instance must have the following:
  - A unique `type`. For example, **Asset**, **Menu**, or **Scene**.
  - A `displayName` to use in the [Filters pane](search-filters.md#persistent-search-filters).
- The optional `filterId` provides a search token for [text-based filtering](search-filters.md#search-tokens). For example, `p:` is the filter ID for [Asset searches](search-assets.md).

## Registering a Search Provider shortcut

To register a shortcut for a new provider use:

```CSharp
[UsedImplicitly, Shortcut("Help/Quick Search/Assets")]
private static void PopQuickSearch()
{
    // Open Quick Search with only the "Asset" provider enabled.
    QuickSearchTool.OpenWithContextualProvider("asset");
}
```
You can map shortcuts to keys or key combinations using the [shortcuts manager](https://docs.unity3d.com/Manual/ShortcutsManager.html).
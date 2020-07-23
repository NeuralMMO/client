# Using Quick Search

To use Quick Search, do the following:

1. **[Launch Quick Search](#launch-quick-search)**
1. **[Search](#searching)**
1. **[Perform actions on items in the results](#performing-actions)**

## Launch Quick Search

There are several ways to open the Quick Search window:

|Shortcut:|Function:|
|-|-|
|**Alt + \'**   | Open Quick Search in the state it was in the last time you used it. <ul><li>The last search term you used appears in the search field.</li><li>The last changes you made to the [filter configuration](search-filters.md#persistent-search-filters) are still in effect.</li></ul> |
|**Alt + Shift + M**   | Start a search for menu items only. |
|**Alt + Shift + C**   | Start a contextual search. The scope depends on what has focus when you open the Quick Search window.<ul><li>If the Hierarchy window has focus, Quick Search searches Scene items.</li><li>If the Project window has focus, Quick Search searches Project Assets.</li><li>If any other window has focus, Quick Search opens as though you used the **Alt + \'** shortcut.</li></ul>  |

> [!TIP]
> You can change the keyboard shortcuts used to launch Quick Search from the [Shortcuts Manager](https://docs.unity3d.com/Manual/ShortcutsManager.html).

## Searching

To perform regular or special searches, use the search field.

### Regular searches

A [regular search](regular-searches.md) uses all regular Search Providers unless you exclude them.

- To perform a regular search using all [active Search Providers](search-filters.md#persistent-search-filters), enter the search terms in the search field. Results appear as you type.

- To only display results for a specific Search Provider, prefix the search terms with the Provider's [search token](regular-searches.md#providers).<br/><br/>A search token is a text string that you can use in the search field to search using only a specific Search Provider.

The following table lists regular Search Providers and their search tokens:

[!include[](incl-search-providers.md)]

### Special searches

A [special search](special-searches.md) is opt-in: Quick Search only uses special Search Providers when you perform a special search.    

To perform a special search, prefix the search terms with the Provider's search token.

The following table lists special Search Providers and their search tokens:

[!include[](incl-special-search-providers.md)]

### Navigating search results

Use **Alt + &uarr;** (up arrow) and **Alt + &darr;** (down arrow) cycle through the search history.

## Performing actions

After you search, you can perform actions on the items Quick Search returns. The actions you can perform depend on the type of item.

For example if Quick Search returns a package, you can install/uninstall it. If Quick Search returns an Asset, you can select, open, or highlight it in the Hierarchy window.

- Every type of item has a [default action](#default-actions).
- Some items support [additional actions](#additional-actions) via a context menu.
- Some items also support [drag and drop actions](#drag-and-drop-actions).


To find out which actions you can perform on different types of items, see the pages for individual search filters in the [Regular searches](regular-searches.md) and [Special searches](special-searches.md) sections.

### Default actions

Every type of item has a default action.

To perform the default action for an item do one of the following:

- Double-click the item.
- Select the item and use **Enter**.

### Additional actions

Some items support additional actions that you access from a context menu.

To access the additional actions context menu for an item, do one of the following:

- Right-click the item.
- Select the item and use **Alt + &rarr;** (right-arrow).
- In the item entry, select **More Options** (**&vellip;**).

You can also use the following shortcuts to perform additional actions on a selected item without opening the contextual menu:

|Shortcut:|Function:|
|-|-|
|**Alt + Enter**|Second action|
|**Alt + Ctrl + Enter**|Third action|
|**Alt + Ctrl + Shift + Enter**|Fourth action|

### Drag and Drop actions

Some Search Providers (for example, the [Asset](search-assets.md) and [Scene](search-scene.md) providers) support drag and drop actions. You can drag items from the results area to any part of Unity that supports them, for example, the Hierarchy window, the Game view, or the Inspector.







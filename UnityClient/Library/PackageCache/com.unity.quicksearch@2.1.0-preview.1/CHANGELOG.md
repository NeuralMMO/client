# Changelog

## [2.1.0-preview.1] - 2020-07-17
- [UX] Improve search database import pipeline by producing index artifacts per asset.
- [UX] Add support to index asset import settings (using the extended index setting options).
- [UX] Add a auto-complete dropdown to find properties using TAB.
- [FIX] Fix corrupted SJSON settings parsing (case 1260242)
- [DOC] Add a cheatsheet that covers all filters for all providers.
- [API] Support dots in QueryEngine filter ids.
- [API] Move query engine support to SearchIndexer.

## [2.0.2] - 2020-07-01
- [UX] Remove the package provider browse action.
- [UX] Add search expression Map node to output X/Y value pairs.
- [UX] Add many new asset and scene filters for material and prefabs.
- [FIX] Fix various search expression issues.
- [FIX] Fix searching saved queried within a contextual search.
- [FIX] Execute first item when pressing enter even if no selection (case 1258382).
- [API] Add support for doubles with the Query Engine default operators.
- [API] Add API to get the query node located at a specified position in a query string.

## [2.0.1] - 2020-06-30
- [FIX] Fix AssetImporters experimental issues with 2020.2

## [2.0.0] - 2020-06-11
- [UX] Save Quick Search settings per project instead of globally for all projects.
- [UX] Remove support for 2018.4.
- [UX] Remove search provider sub categories. It simplifies the search view filter window.
- [UX] Remove basic file indexer support. Now indexing only works with .index files and fallback is using the AssetDatabase.FindAsset API.
- [UX] Improve the saved search query workflow (less field to fulfill).
- [UX] Change Reset priorities button in Preferences to Reset to providers Defaults (which reset priority, active and default actions).
- [UX] Add the total asynchronous time a query took for all provider sessions in the Quick Search status bar.
- [UX] Add the ability to fetch items on a specific provider.
- [UX] Add support to override the default object picker using Quick Search.
- [UX] Add support for regular selection as well as multi selection using end and home keys.
- [UX] Add support for multiple asset indexes.
- [UX] Add Search Engines for Unity's Search API.
- [UX] Add scene property filtering support (i.e. `t:light2d p(intensity)>=0.5`)
- [UX] Add prefab asset indexing support.
- [UX] Add onboarding workflow the first time you launch Quick Search
- [UX] Add new scene provider filters (i.e. id:<string>, path:<string/string> size:<number>, layer:<number>, tag:<string>, t:<type>, is:[visible|hidden|leaf|root|child])
- [UX] Add new create Search Query Button. If search queries exist in the project, this is what we show instead of hardcoded help string.
- [UX] Add nested search expression nodes support in the Expression Builder.
- [UX] Add multi selection support
- [UX] Add more error reporting for invalid queries with the `QueryEngine`.
- [UX] Add index manager to manage your project asset, prefab and scene indexes.
- [UX] Add grid view support to display search results in a grid of thumbnails.
- [UX] Add Creation Window to for Search Query.
- [UX] Add background scene asset indexing.
- [UX] Add an embedded inspector for objects returned by the resource and scene search providers.
- [UX] Add a search expression builder to create complex queries.
- [UX] Add a compact list view.
- [UX] Add `dir:DIR_NAME` to asset indexing to filter assets by their direct parent folder name.
- [UX] Add "Show all results..." checkbox to run per search provider more queries to find even more results. In example for the AssetProvider, if this is checked we try to find more assets by using AssetDatabase.FindAssets. This can be unchecked in large project where the asset database can be very slow.
- [FIX] Remove the asset store provider for Unity version before 2019.3.
- [FIX] Optimize the search menu and scene providers (about 4-5x faster).
- [FIX] Fix Unity crash when dragging and dropping from quick search (case 1215420)
- [FIX] Fix the search field initial styling and position when opening Quick Search.
- [FIX] Fix scrollbar overflow (more visible in the light theme).
- [FIX] Fix Quick Search fails to find assets when more than 16 characters are entered into the search field (case 1225947)
- [FIX] Fix Progress API usage.
- [FIX] Fix one letter word query that breaks searching the index.
- [FIX] Fix NullReferenceException thrown When "Disabled" option is toggle from "Search Index Manager" window (case 1252291)
- [FIX] Fix filter override application.
- [FIX] Fix drag and drop paths for the asset search provider.
- [FIX] Fix details view min and max size.
- [FIX] Fix complete file name indexing (case 1214270)
- [FIX] Fix an issue tracking selection of item at index 1.
- [FIX] Fix actions sorting on SearchService init and in SearchSettings window.
- [FIX] Add support for any characters in word searches.
- [FIX] Add better support for startup incremental update.
- [FIX] Add better sorting for assets based on file path matches.
- [DOC] Update API documentation.
- [API] Remove the `SearchFilter` class.
- [API] Optimize call to operator handlers when in fallback mode.
- [API] Improve the `SearchContext` API in order to keep track of filtered providers.
- [API] Improve support for simultaneous calls to `SearchService.GetItems` with different search contexts.
- [API] Improve build time of a `QueryEngine` search query.
- [API] Fix calling onEnable/onDisable multiple time when doing multiple simultaneous searches with a provider.
- [API] Allow skipping words when parsing a query with the QueryEngine.
- [API] Allow retrieval of tokens used to generate a query.
- [API] Add the ability to customize a query engine with filters using method attributes. Used by the Scene Provider.
- [API] Add the ability for the QueryEngine to skip unknown filters in a query.
- [API] Add support to remove filters on the `QueryEngine`.
- [API] Add support to override the string comparison options for word/phrase matching with the `QueryEngine`.
- [API] Add support for spaces inside nested queries.
- [API] Add support for nested queries with the `QueryEngine`.
- [API] Add support for custom object indexers.
- [API] Add support for concurrent calls to the SearchApi engines with different SearchApi contexts.
- [API] Add support for a search word transformer with the `QueryEngine`.
- [API] Add a websocket client called SearchChannel to do search from a web application (20.3 or 21.1 required).

## [1.5.5] - 2020-06-01
- [FIX] Fix one letter word search (one letter word should be ignored when searching the index)

## [1.5.4] - 2020-05-22
- [UX] Remove experimental indexing in 1.5 as it will be officially released in version 2.0
- [FIX] Remove usage of Progress.RunTask

## [1.5.3] - 2020-02-23
- [FIX] Increase word character variation indexing to 32.
- [FIX] Ensure package and store search providers are not enabled while runnign tests.

## [1.5.2] - 2020-02-20
- [FIX] The asset store provider will only be available for 2020.1 and newer.
- [FIX] Improve scene provider performances
- [FIX] Fix Unity crash when dragging and dropping from quick search (1215420)
- [Fix] Fix complete file name indexing (case 1214270)

## [1.5.1] - 2020-01-24
- [FIX] Fix Progress API usage.

## [1.5.0] - 2020-01-22
- [UX] You can now search scene objects with a given component using c:<component name>.
- [UX] We've removed the dockable window mode of Quick Search since it wasn't playing nice with some loading and refreshing workflows and optimizations.
- [UX] Update the quick search spinning wheel when async results are still being fetched.
- [UX] Select search item on mouse up instead of mouse down.
- [UX] fetchPreview of AssetStoreProvider uses the PurchaseInfo to get a bigger/more detailed preview.
- [UX] Change the Resources Provider to use the QueryEngine. Some behaviors may have changed.
- [UX] Asset Store provider fetches multiple screenshots and populates the preview panel carousel with those.
- [UX] Add UMPE quick search indexing to build the search index in another process.
- [UX] Add selected search item preview panel.
- [UX] Add Resource provider, which lets you search all resources loaded by Unity.
- [UX] Add new Unity 2020.1 property editor support.
- [UX] Add drag and drop support to the resource search provider.
- [UX] Add documentation link to Help provider and version label.
- [UX] Add Asset Store provider populating items with asset store package.
- [UX] Add a new settings to enable the new asset indexer in the user preferences.
- [UX] Add a new asset indexer that indexes many asset properties, such as dependencies, size, serialized properties, etc.
- [UX] Add a carrousel to display images of asset store search results.
- [FIX] Only enable the search asset watcher once the quick search tool is used the first time.
- [FIX] Do not load the LogProvider if the application console log path is not valid.
- [FIX] Add support for digits when splitting camel cases of file names.
- [FIX] Prevent search callback errors when not validating queries.
- [DOC] Quick Search Manual has been reviewed and edited.
- [DOC] Document more APIs.
- [DOC] Add some sample packages to Quick Search to distribute more search provider and query engine examples.
- [API] Make Unity.QuickSearch.QuickSearch public to allow user to open quick search explicitly with specific context data.
- [API] Improved the SearchIndexer API and performances
- [API] Change the signature of `fetchItems` to return an object instead of an `IEnumerable<SearchItem>`. This item can be an `IEnumerable<SearchItem>` as before, or an `IEnumerator` to allow yield returns of `IEnumerator` or `IEnumerable`.
- [API] Add the ability to configure string comparisons with the QueryEngine.
- [API] Add the `QueryEngine` API.
- [API] Add `QuickSearch.ShowWindow(float width, float height)` to allow opening Quick Search at any size.

## [1.4.1] - 2019-09-03
- Quick Search is now a verified package.
- [UX] Add UIElements experimental search provider.
- [FIX] Add to the asset search provider type filter all ScriptableObject types.
- [FIX] Fix Asset store URL.
- [DOC] Document more public APIs.
- [API] Add programming hooks to the scene search provider.

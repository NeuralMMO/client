using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.QuickSearch.Providers
{
    [UsedImplicitly]
    static class AssetProvider
    {
        internal const string type = "asset";
        private const string displayName = "Asset";

        private static readonly string[] k_NonSimpleSearchTerms = new string[] {"(", ")", "-", "=", "<", ">"};

        private static bool reloadAssetIndexes = true;
        private static List<SearchDatabase> m_AssetIndexes= null;
        private static List<SearchDatabase> assetIndexes
        {
            get
            {
                if (reloadAssetIndexes || m_AssetIndexes == null)
                {
                    AssetPostprocessorIndexer.contentRefreshed -= TrackAssetIndexChanges;
                    m_AssetIndexes = SearchDatabase.Enumerate("asset").ToList();
                    reloadAssetIndexes = false;
                    AssetPostprocessorIndexer.contentRefreshed += TrackAssetIndexChanges;
                }
                return m_AssetIndexes;
            }
        }

        [UsedImplicitly, SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SearchProvider(type, displayName)
            {
                priority = 25,
                filterId = "p:",
                showDetails = true,
                showDetailsOptions = ShowDetailsOptions.Default | ShowDetailsOptions.Inspector,

                isEnabledForContextualSearch = () => Utils.IsFocusedWindowTypeName("ProjectBrowser"),
                toObject = (item, type) => AssetDatabase.LoadAssetAtPath(item.id, type),
                fetchItems = (context, items, provider) => SearchAssets(context, provider),
                fetchDescription = (item, context) => (item.description = GetAssetDescription(item.id)),
                fetchThumbnail = (item, context) => Utils.GetAssetThumbnailFromPath(item.id),
                fetchPreview = (item, context, size, options) => Utils.GetAssetPreviewFromPath(item.id, options),
                openContextual = (selection, rect) => OpenContextualMenu(selection, rect),
                startDrag = (item, context) => StartDrag(item, context),
                trackSelection = (item, context) => Utils.PingAsset(item.id),
                fetchPropositions = (context, options) => FetchPropositions(context, options)
            };
        }

        private static IEnumerable<SearchProposition> FetchPropositions(SearchContext context, SearchPropositionOptions options)
        {
            if (context.options.HasFlag(SearchFlags.NoIndexing))
                return null;

            return assetIndexes.SelectMany(db => db.index.GetKeywords().Select(kw => new SearchProposition(kw)));
        }

        private static IEnumerable<string> FilterIndexes(IEnumerable<string> paths)
        {
            return paths.Where(u => u.EndsWith(".index", StringComparison.OrdinalIgnoreCase));
        }

        private static void TrackAssetIndexChanges(string[] updated, string[] deleted, string[] moved)
        {
            var loaded = assetIndexes?.Where(db=>db).Select(db => AssetDatabase.GetAssetPath(db)).ToArray() ?? new string[0];
            if (FilterIndexes(updated).Except(loaded).Count() > 0 || loaded.Intersect(FilterIndexes(deleted)).Count() > 0)
                reloadAssetIndexes = true;
        }

        private static bool OpenContextualMenu(SearchSelection selection, Rect contextRect)
        {
            var old = Selection.instanceIDs;
            SearchUtils.SelectMultipleItems(selection);
            EditorUtility.DisplayPopupMenu(contextRect, "Assets/", null);
            EditorApplication.delayCall += () => EditorApplication.delayCall += () => Selection.instanceIDs = old;
            return true;
        }

        private static void StartDrag(SearchItem item, SearchContext context)
        {
            if (context.selection.Count > 1)
            {
                var selectedObjects = context.selection.Select(i => AssetDatabase.LoadAssetAtPath<Object>(i.id));
                var paths = context.selection.Select(i => i.id).ToArray();
                Utils.StartDrag(selectedObjects.ToArray(), paths, item.GetLabel(context, true));
            }
            else
                Utils.StartDrag(new [] { AssetDatabase.LoadAssetAtPath<Object>(item.id) }, new []{ item.id }, item.GetLabel(context, true));
        }

        private static IEnumerator SearchAssets(SearchContext context, SearchProvider provider)
        {
            var searchQuery = context.searchQuery;

            if (!String.IsNullOrEmpty(searchQuery))
            {
                bool indexesReady = false;
                if (!context.options.HasFlag(SearchFlags.NoIndexing))
                {
                    indexesReady = assetIndexes.All(db => db.index?.IsReady() ?? false);
                    if (indexesReady)
                        yield return assetIndexes.Select(db => SearchIndexes(context.searchQuery, context, provider, db));
                }

                // Search file system wild cards
                if (context.searchQuery.Contains('*'))
                {
                    var globSearch = context.searchQuery;
                    if (globSearch.IndexOf("glob:", StringComparison.OrdinalIgnoreCase) == -1 && context.searchWords.Length == 1)
                        globSearch = $"glob:\"{globSearch}\"";
                    yield return AssetDatabase.FindAssets(globSearch)
                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                        .Select(path => CreateItem(context, provider, "*", path, 999));
                }
                else
                {
                    // Search by GUID
                    var guidPath = AssetDatabase.GUIDToAssetPath(searchQuery);
                    if (!String.IsNullOrEmpty(guidPath))
                        yield return provider.CreateItem(context, guidPath, -1, $"{Path.GetFileName(guidPath)} ({searchQuery})", null, null, null);

                    // Finally search the default asset database for any remaining results.
                    if (context.options.HasFlag(SearchFlags.NoIndexing) ||
                        (context.wantsMore && !k_NonSimpleSearchTerms.Any(t => searchQuery.IndexOf(t, StringComparison.Ordinal) != -1)))
                    {
                        yield return AssetDatabase.FindAssets(searchQuery)
                            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                            .Select(path => CreateItem(context, provider, "ADB", path, 998));
                    }

                    if (!indexesReady)
                        yield return assetIndexes.Select(db => SearchIndexes(context.searchQuery, context, provider, db));
                }
            }

            if (context.wantsMore && context.filterType != null && String.IsNullOrEmpty(searchQuery))
            {
                yield return AssetDatabase.FindAssets($"t:{context.filterType.Name}")
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => CreateItem(context, provider, "More", path, 999));

                if (assetIndexes != null)
                    yield return assetIndexes.Select(db => SearchIndexes($"has={context.filterType.Name}", context, provider, db));
            }
        }

        private static IEnumerator SearchIndexes(string searchQuery, SearchContext context, SearchProvider provider, SearchDatabase db)
        {
            while (!db.ready)
            {
                if (!db)
                    yield break;
                yield return null;
            }

            // Search index
            var index = db.index;
            db.Report("Search", searchQuery);
            yield return index.Search(searchQuery.ToLowerInvariant()).Select(e => CreateItem(context, provider, db.name, e.id, e.score));
        }

        private static SearchItem CreateItem(SearchContext context, SearchProvider provider, string dbName, string assetPath, int itemScore)
        {
            var words = context.searchPhrase;
            var filenameNoExt = Path.GetFileNameWithoutExtension(assetPath);
            if (filenameNoExt.Equals(words, StringComparison.OrdinalIgnoreCase))
                itemScore = SearchProvider.k_RecentUserScore - 1;

            var filename = Path.GetFileName(assetPath);
            if (context.options.HasFlag(SearchFlags.Debug) && !String.IsNullOrEmpty(dbName))
                filename += $" ({dbName}, {itemScore})";
            return provider.CreateItem(context, assetPath, itemScore, filename, null, null, null);
        }

        internal static string GetAssetDescription(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
                return assetPath;
            var fi = new FileInfo(assetPath);
            if (!fi.Exists)
                return "File does not exist anymore.";
            var fileSize = new FileInfo(assetPath).Length;
            return $"{assetPath} ({EditorUtility.FormatBytes(fileSize)})";
        }

        [UsedImplicitly, SearchActionsProvider]
        internal static IEnumerable<SearchAction> CreateActionHandlers()
        {
            #if UNITY_EDITOR_OSX
            const string k_RevealActionLabel = "Reveal in Finder...";
            #else
            const string k_RevealActionLabel = "Show in Explorer...";
            #endif

            return new[]
            {
                new SearchAction(type, "select", null, "Select asset...")
                {
                    handler = (item) => Utils.FrameAssetFromPath(item.id),
                    execute = (items) => SearchUtils.SelectMultipleItems(items, focusProjectBrowser: true)
                },
                new SearchAction(type, "open", null, "Open asset...")
                {
                    handler = (item) =>
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Object>(item.id);
                        if (asset != null) AssetDatabase.OpenAsset(asset);
                    }
                },
                new SearchAction(type, "add_scene", null, "Add scene...")
                {
                    // Only works in single selection and adds a scene to the current hierarchy.
                    enabled = (items) => items.Count == 1 && items.Last().id.EndsWith(".unity", StringComparison.OrdinalIgnoreCase),
                    handler = (item) => UnityEditor.SceneManagement.EditorSceneManager.OpenScene(item.id, UnityEditor.SceneManagement.OpenSceneMode.Additive)
                },
                new SearchAction(type, "reveal", null, k_RevealActionLabel)
                {
                    handler = (item) => EditorUtility.RevealInFinder(item.id)
                }
            };
        }

        [UsedImplicitly, Shortcut("Help/Quick Search/Assets")]
        internal static void PopQuickSearch()
        {
            QuickSearch.OpenWithContextualProvider(type, Query.type);
        }
    }
}

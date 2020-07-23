//#define QUICKSEARCH_DEBUG

#if UNITY_2020_1_OR_NEWER
//#define SHOW_SEARCH_PROGRESS
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Unity.QuickSearch
{
    /// <summary>
    /// Attribute used to declare a static method that will create a new search provider at load time.
    /// </summary>
    public class SearchItemProviderAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute used to declare a static method that define new actions for specific search providers.
    /// </summary>
    public class SearchActionsProviderAttribute : Attribute
    {
    }

    /// <summary>
    /// Principal Quick Search API to initiate searches and fetch results.
    /// </summary>
    public static class SearchService
    {
        internal const string prefKey = "quicksearch";

        const string k_ActionQueryToken = ">";

        private const int k_MaxFetchTimeMs = 50;

        internal static Dictionary<string, List<string>> ActionIdToProviders { get; private set; }

        /// <summary>
        /// Returns the list of all providers (active or not)
        /// </summary>
        public static List<SearchProvider> Providers { get; private set; }

        /// <summary>
        /// Returns the list of providers sorted by priority.
        /// </summary>
        public static IEnumerable<SearchProvider> OrderedProviders
        {
            get
            {
                return Providers.OrderBy(p => p.priority + (p.isExplicitProvider ? 100000 : 0));
            }
        }

        static SearchService()
        {
            Refresh();
        }

        /// <summary>
        /// Returns the data of a search provider given its ID.
        /// </summary>
        /// <param name="providerId">Unique ID of the provider</param>
        /// <returns>The matching provider</returns>
        public static SearchProvider GetProvider(string providerId)
        {
            return Providers.Find(p => p.name.id == providerId);
        }

        /// <summary>
        /// Returns the search action data for a given provider and search action id.
        /// </summary>
        /// <param name="provider">Provider to lookup</param>
        /// <param name="actionId">Unique action ID within the provider.</param>
        /// <returns>The matching action</returns>
        public static SearchAction GetAction(SearchProvider provider, string actionId)
        {
            if (provider == null)
                return null;
            return provider.actions.Find(a => a.id == actionId);
        }

        /// <summary>
        /// Activate or deactivate a search provider.
        /// Call Refresh after this to take effect on the next search.
        /// </summary>
        /// <param name="providerId">Provider id to activate or deactivate</param>
        /// <param name="active">Activation state</param>
        public static void SetActive(string providerId, bool active = true)
        {
            var provider = Providers.FirstOrDefault(p => p.name.id == providerId);
            if (provider == null)
                return;
            SearchSettings.GetProviderSettings(providerId).active = active;
            provider.active = active;
        }

        /// <summary>
        /// Clears everything and reloads all search providers.
        /// </summary>
        /// <remarks>Use with care. Useful for unit tests.</remarks>
        public static void Refresh()
        {
            RefreshProviders();
            RefreshProviderActions();
        }

        /// <summary>
        /// Returns a list of keywords used by auto-completion for the active providers.
        /// </summary>
        /// <param name="context">Current search context</param>
        /// <param name="lastToken">Search token currently being typed.</param>
        /// <returns>A list of keywords that can be shown in an auto-complete dropdown.</returns>
        [Obsolete("GetKeywords is deprecated. Define fetchPropositions on your provider instead.")]
        public static string[] GetKeywords(SearchContext context, string lastToken)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Create context from a list of provider id.
        /// </summary>
        /// <param name="providerIds">List of provider id</param>
        /// <param name="searchText">seach Query</param>
        /// <param name="flags">Options defining how the query will be performed</param>
        /// <returns>New SearchContext</returns>
        public static SearchContext CreateContext(IEnumerable<string> providerIds, string searchText = "", SearchFlags flags = SearchFlags.Default)
        {
            return new SearchContext(providerIds.Select(id => GetProvider(id)).Where(p => p != null), searchText, flags);
        }

        /// <summary>
        /// Create context from a list of providers.
        /// </summary>
        /// <param name="providers">List of providers</param>
        /// <param name="searchText">seach Query</param>
        /// <param name="flags">Options defining how the query will be performed</param>
        /// <returns>New SearchContext</returns>
        public static SearchContext CreateContext(IEnumerable<SearchProvider> providers, string searchText = "", SearchFlags flags = SearchFlags.Default)
        {
            return new SearchContext(providers, searchText, flags);
        }

        /// <summary>
        /// Initiate a search and return all search items matching the search context. Other items can be found later using the asynchronous searches.
        /// </summary>
        /// <param name="context">The current search context</param>
        /// <param name="options">Options defining how the query will be performed</param>
        /// <returns>A list of search items matching the search query.</returns>
        public static List<SearchItem> GetItems(SearchContext context, SearchFlags options = SearchFlags.Default)
        {
            DebugInfo.gcFetch = GC.GetTotalMemory(false);

            // Stop all search sessions every time there is a new search.
            context.sessions.StopAllAsyncSearchSessions();
            context.searchFinishTime = context.searchStartTime = EditorApplication.timeSinceStartup;
            context.sessionEnded -= OnSearchEnded;
            context.sessionEnded += OnSearchEnded;

            #if SHOW_SEARCH_PROGRESS
            if (Progress.Exists(context.progressId))
                Progress.Finish(context.progressId, Progress.Status.Succeeded);
            context.progressId = Progress.Start($"Searching...", options: Progress.Options.Indefinite);
            #endif

            if (options.HasFlag(SearchFlags.WantsMore))
                context.wantsMore = true;

            int fetchProviderCount = 0;
            var allItems = new List<SearchItem>(3);
            #if QUICKSEARCH_DEBUG
            var debugProviderList = context.providers.ToList();
            using (new DebugTimer($"Search get items {String.Join(", ", debugProviderList.Select(p=>p.name.id))} -> {context.searchQuery}"));
            #endif
            foreach (var provider in context.providers)
            {
                try
                {
                    var watch = new System.Diagnostics.Stopwatch();
                    watch.Start();
                    fetchProviderCount++;
                    var iterator = provider.fetchItems(context, allItems, provider);
                    if (iterator != null && options.HasFlag(SearchFlags.Synchronous))
                    {
                        var stackedEnumerator = new StackedEnumerator<SearchItem>(iterator);
                        while (stackedEnumerator.MoveNext())
                        {
                            if (stackedEnumerator.Current != null)
                                allItems.Add(stackedEnumerator.Current);
                        }
                    }
                    else
                    {
                        var session = context.sessions.GetProviderSession(context, provider.name.id);
                        session.Reset(context, iterator, k_MaxFetchTimeMs);
                        session.Start();
                        var sessionEnded = !session.FetchSome(allItems, k_MaxFetchTimeMs);
                        if (options.HasFlag(SearchFlags.FirstBatchAsync))
                            session.SendItems(allItems);
                        if (sessionEnded)
                            session.Stop();
                    }
                    provider.RecordFetchTime(watch.Elapsed.TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(new Exception($"Failed to get fetch {provider.name.displayName} provider items.", ex));
                }
            }

            if (fetchProviderCount == 0)
            {
                OnSearchEnded(context);
                context.sessions.StopAllAsyncSearchSessions();
            }

            DebugInfo.gcFetch = GC.GetTotalMemory(false) - DebugInfo.gcFetch;

            if (!options.HasFlag(SearchFlags.Sorted))
                return allItems;

            allItems.Sort(SortItemComparer);
            return allItems.GroupBy(i => i.id).Select(i => i.First()).ToList();
        }

        /// <summary>
        /// Execute a search request that will fetch search results asynchronously.
        /// </summary>
        /// <param name="context">Search context used to track asynchronous request.</param>
        /// <param name="options">Options defining how the query will be performed</param>
        /// <returns>Asynchronous list of search items.</returns>
        public static ISearchList Request(SearchContext context, SearchFlags options = SearchFlags.None)
        {
            if (options.HasFlag(SearchFlags.Synchronous))
            {
                throw new NotSupportedException($"Use {nameof(SearchService)}.{nameof(GetItems)}(context, " +
                                    $"{nameof(SearchFlags)}.{nameof(SearchFlags.Synchronous)}) to fetch items synchronously.");
            }

            ISearchList results = null;
            if (options.HasFlag(SearchFlags.Sorted))
                results = new SortedSearchList(context);
            else
                results = new AsyncSearchList(context);

            results.AddItems(GetItems(context, options));
            return results;
        }

        /// <summary>
        /// Load a search expression asset.
        /// </summary>
        /// <param name="expressionPath">Asset path of the search expression</param>
        /// <param name="options">Options defining how the query will be performed</param>
        /// <returns>Returns a SearchExpression ready to be evaluated.</returns>
        public static ISearchExpression LoadExpression(string expressionPath, SearchFlags options = SearchFlags.Default)
        {
            if (!File.Exists(expressionPath))
                throw new ArgumentException($"Cannot find expression {expressionPath}", nameof(expressionPath));

            var se = new SearchExpression(options);
            se.Load(expressionPath);
            return se;
        }

        /// <summary>
        /// Parse a simple json document string as a SearchExpression.
        /// </summary>
        /// <param name="sjson">Simple Json string defining a SearchExpression</param>
        /// <param name="options">Options defining how the query will be performed</param>
        /// <returns>Returns a SearchExpression ready to be evaluated.</returns>
        public static ISearchExpression ParseExpression(string sjson, SearchFlags options = SearchFlags.Default)
        {
            var se = new SearchExpression(options);
            se.Parse(sjson);
            return se;
        }

        internal static SearchContext CreateContext(SearchProvider provider, string searchText = "")
        {
            return CreateContext(new[] { provider }, searchText);
        }

        internal static SearchContext CreateContext(string providerId, string searchText = "", SearchFlags flags = SearchFlags.Default)
        {
            return CreateContext(new[] { providerId }, searchText, flags);
        }

        internal static SearchContext CreateContext(string searchText)
        {
            return CreateContext(Providers.Where(p => p.active), searchText);
        }

        private static void OnSearchEnded(SearchContext context)
        {
            context.searchFinishTime = EditorApplication.timeSinceStartup;

            #if SHOW_SEARCH_PROGRESS
            if (context.progressId != -1 && Progress.Exists(context.progressId))
                Progress.Finish(context.progressId, Progress.Status.Succeeded);
            context.progressId = -1;
            #endif
        }

        internal static void ReportProgress(SearchContext context, float progress = 0f, string status = null)
        {
            #if SHOW_SEARCH_PROGRESS
            if (context.progressId != -1 && Progress.Exists(context.progressId))
                Progress.Report(context.progressId, progress, status);
            #endif
        }

        private static int SortItemComparer(SearchItem item1, SearchItem item2)
        {
            var po = item1.provider.priority.CompareTo(item2.provider.priority);
            if (po != 0)
                return po;
            po = item1.score.CompareTo(item2.score);
            if (po != 0)
                return po;
            return String.Compare(item1.id, item2.id, StringComparison.Ordinal);
        }

        private static void RefreshProviders()
        {
            Providers = Utils.GetAllMethodsWithAttribute<SearchItemProviderAttribute>().Select(methodInfo =>
            {
                try
                {
                    SearchProvider fetchedProvider = null;
                    using (var fetchLoadTimer = new DebugTimer(null))
                    {
                        fetchedProvider = methodInfo.Invoke(null, null) as SearchProvider;
                        if (fetchedProvider == null)
                            return null;

                        fetchedProvider.loadTime = fetchLoadTimer.timeMs;

                        // Load per provider user settings
                        if (SearchSettings.TryGetProviderSettings(fetchedProvider.name.id, out var providerSettings))
                        {
                            fetchedProvider.active = providerSettings.active;
                            fetchedProvider.priority = providerSettings.priority;
                        }
                    }
                    return fetchedProvider;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                    return null;
                }
            }).Where(provider => provider != null).ToList();
        }

        private static void RefreshProviderActions()
        {
            ActionIdToProviders = new Dictionary<string, List<string>>();
            foreach (var action in Utils.GetAllMethodsWithAttribute<SearchActionsProviderAttribute>()
                                        .SelectMany(methodInfo => methodInfo.Invoke(null, null) as IEnumerable<object>)
                                        .Where(a => a != null).Cast<SearchAction>())
            {
                var provider = Providers.Find(p => p.name.id == action.providerId);
                if (provider == null)
                    continue;
                provider.actions.Add(action);
                if (!ActionIdToProviders.TryGetValue(action.id, out var providerIds))
                {
                    providerIds = new List<string>();
                    ActionIdToProviders[action.id] = providerIds;
                }
                providerIds.Add(provider.name.id);
            }
            SearchSettings.SortActionsPriority();
        }
    }
}

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Unity.QuickSearch.Providers
{
    [UsedImplicitly]
    static class Query
    {
        internal const string type = "query";
        private const string displayName = "Queries";

        [UsedImplicitly, SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SearchProvider(type, displayName)
            {
                filterId = "q:",
                isExplicitProvider = true,
                isEnabledForContextualSearch = () => true,
                fetchItems = (context, items, provider) =>
                {
                    var queryItems = SearchQuery.GetAllSearchQueryItems(context);
                    if (string.IsNullOrEmpty(context.searchQuery))
                    {
                        items.AddRange(queryItems);
                    }
                    else
                    {
                        foreach (var qi in queryItems)
                        {
                            if (SearchUtils.MatchSearchGroups(context, qi.label, true) ||
                                SearchUtils.MatchSearchGroups(context, ((SearchQuery)qi.data).searchQuery, true))
                            {
                                items.Add(qi);
                            }
                        }
                    }
                    return null;
                }
            };
        }

        [UsedImplicitly, SearchActionsProvider]
        internal static IEnumerable<SearchAction> ActionHandlers()
        {
            return new[]
            {
                new SearchAction(type, "exec", null, "Execute search query")
                {
                    closeWindowAfterExecution = false,
                    handler = (item) => SearchQuery.ExecuteQuery(item?.context.searchView, (SearchQuery)item.data)
                },
                new SearchAction(type, "select", null, "Select search query", (item) => Utils.FrameAssetFromPath(item.id))
            };
        }
    }
}

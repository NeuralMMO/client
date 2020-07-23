//#define QUICKSEARCH_DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch.Providers
{
    [UsedImplicitly]
    static class StaticMethodProvider
    {
        private const string type = "static_methods";
        private const string displayName = "Static API";

        private static MethodInfo[] methods;

        [UsedImplicitly, SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SearchProvider(type, displayName)
            {
                priority = 85,
                filterId = "#",
                isExplicitProvider = true,
                fetchItems = (context, items, provider) =>
                {
                    if (!context.searchText.StartsWith(provider.filterId))
                        return null;

                    // Cache all available static APIs
                    if (methods == null)
                        methods = FetchStaticAPIMethodInfo();

                    foreach (var m in methods)
                    {
                        if (!SearchUtils.MatchSearchGroups(context, m.Name))
                            continue;

                        var visibilityString = !m.IsPublic ? "<i>Internal</i> - " : String.Empty;
                        items.Add(provider.CreateItem(context, m.Name, m.IsPublic ? 0 : 1, m.Name, $"{visibilityString}{m.DeclaringType} - {m}" , null, m));
                    }

                    return null;
                },

                fetchThumbnail = (item, context) => Icons.shortcut
            };
        }

        [Pure]
        private static MethodInfo[] FetchStaticAPIMethodInfo()
        {
            #if QUICKSEARCH_DEBUG
            using (new DebugTimer("GetAllStaticMethods"))
            #endif
            {
                bool isDevBuild = UnityEditor.Unsupported.IsDeveloperBuild();
                var staticMethods = AppDomain.CurrentDomain.GetAllStaticMethods(isDevBuild);
                #if QUICKSEARCH_DEBUG
                Debug.Log($"Fetched {staticMethods.Length} APIs");
                #endif

                return staticMethods;
            }
        }

        private static void LogResult(object result)
        {
            if (result == null)
                return;

            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, result as UnityEngine.Object, result.ToString());
        }

        [UsedImplicitly, SearchActionsProvider]
        private static IEnumerable<SearchAction> ActionHandlers()
        {
            return new[]
            {
                new SearchAction(type, "exec", null, "Execute method...", (items) =>
                {
                    foreach (var item in items)
                    {
                        var m = item.data as MethodInfo;
                        if (m == null)
                            return;
                        var result = m.Invoke(null, null);
                        if (result == null)
                            return;
                        var list = result as IEnumerable;
                        if (result is string || list == null)
                        {
                            LogResult(result);
                            EditorGUIUtility.systemCopyBuffer = result.ToString();
                        }
                        else
                        {
                            foreach (var e in list)
                                LogResult(e);
                        }
                    }
                })
            };
        }
    }
}

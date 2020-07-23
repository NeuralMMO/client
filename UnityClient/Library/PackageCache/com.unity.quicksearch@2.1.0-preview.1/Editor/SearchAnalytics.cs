// #define QUICKSEARCH_DEBUG
// #define QUICKSEARCH_ANALYTICS_LOGGING
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace Unity.QuickSearch
{
    internal static class SearchAnalytics
    {
        [Serializable]
        internal class ProviderData
        {
            // Was the provider enabled for the search
            public bool isEnabled;
            // Id of the provider
            public string id;
            // Average time of the last 10 search performed by this provider (in ms).
            public long avgTime;
            // Custom provider data
            public string custom;
        }

        [Serializable]
        internal class PreferenceData
        {
            // BEGIN- Not used anymore
            public bool useDockableWindow;
            public bool closeWindowByDefault;
            // END - Not used anymore

            public bool trackSelection;
        }

        [Serializable]
        internal class SearchEvent
        {
            public SearchEvent()
            {
                startTime = DateTime.Now;
            }

            public void Success(SearchItem item, SearchAction action = null)
            {
                Done();
                success = true;
                providerId = item.provider.name.id;
                if (action != null)
                    actionId = action.content.text;
            }

            public void Done()
            {
                if (duration == 0)
                    duration = elapsedTimeMs;
            }

            public long elapsedTimeMs => (long) (DateTime.Now - startTime).TotalMilliseconds;

            // Start time when the SearchWindow opened
            private DateTime startTime;
            // Duration (in ms) for which the search window was opened
            public long duration;
            // Was the search result a success: did an item gets activated.
            public bool success;
            // If search successful: Provider of the item activated.
            public string providerId;
            // If search successful: ActionId of the item activated.
            public string actionId;
            // What was in the search box when the tool window was closed.
            public string searchText;
            // UI Usage
            // Was the item activated using the Enter key (false: user clicked in the item)
            public bool endSearchWithKeyboard;
            // Was the tool opened in general mode (where we record the search state)
            [Obsolete] public bool saveSearchStateOnExit = true;
            // Was the history shortcut used.
            public bool useHistoryShortcut;
            // Was the FilterMenu shortcut used.
            public bool useFilterMenuShortcut;
            // Was the Action Menu shortcut used.
            public bool useActionMenuShortcut;
            // Was drag and drop used.
            public bool useDragAndDrop;
            // Provider specific data
            public ProviderData[] providerDatas;

            public bool useOverrideFilter;
            public bool isDeveloperMode;
            public PreferenceData preferences;

            // Future:
            // useFilterId
            // useActionQuery
            // nbCharacterInSearch
            // useActionMenu
            // useFilterWindow
            // useRightClickOnItem
            // useRightClickContextAction
        }

        [Serializable]
        internal struct GenericEvent
        {
            // Message category
            public string category;
            // Enum id of the message category
            public int categoryId;
            // Message name
            public string name;
            // Message type
            public string message;
            // Message data
            public string description;
            // Event duration
            public long duration;
        }

        enum EventName
        {
            quickSearchGeneric,
            quickSearch
        }

        public enum EventCategory
        {
            Custom = 0,
            Information = 1,
            Warning = 2,
            Error = 3,
            Usage = 4
        }

        public static string Version;
        private static bool s_Registered;
        private static HashSet<int> s_OnceHashCodes = new HashSet<int>();

        static SearchAnalytics()
        {
            Version = Utils.GetQuickSearchVersion();
            EditorApplication.delayCall += () =>
            {
                Application.logMessageReceived += (condition, trace, type) =>
                {
                    if (type == LogType.Exception &&
                        !string.IsNullOrEmpty(trace) &&
                        trace.Contains("quicksearch"))
                    {
                        if (s_OnceHashCodes.Add(trace.GetHashCode()))
                        {
                            SendErrorEvent("__uncaught__", condition, trace);
                        }
                    }
                };
            };
        }

        public static void SendCustomEvent(string category, string name, string message = null, string description = null)
        {
            SendEvent(EventCategory.Custom, category, name, message, description, TimeSpan.Zero);
        }

        public static void SendCustomEvent(string category, string name, TimeSpan duration, string message = null, string description = null)
        {
            SendEvent(EventCategory.Custom, category, name, message, description, duration);
        }

        public static void SendExceptionOnce(string name, Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            var hashCode = ex.StackTrace.GetHashCode();
            if (s_OnceHashCodes.Add(hashCode))
            {
                SendException(name, ex);
            }
        }

        public static void SendException(string name, Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            SendErrorEvent(name, ex.Message, ex.ToString());
        }

        public static void SendErrorEvent(string name, string message = null, string description = null)
        {
            SendEvent(EventCategory.Error, name, TimeSpan.Zero, message, description);
        }

        public static void SendEvent(EventCategory category, string name, string message = null, string description = null)
        {
            SendEvent(category, category.ToString(), name, message, description, TimeSpan.Zero);
        }

        public static void SendEvent(EventCategory category, string name, TimeSpan duration, string message = null, string description = null)
        {
            SendEvent(category, category.ToString(), name, message, description, duration);
        }

        public static void SendSearchEvent(SearchEvent evt, SearchContext searchContext)
        {
            evt.useOverrideFilter = searchContext.filterId != null;
            evt.isDeveloperMode = Utils.isDeveloperBuild;
            evt.preferences = new PreferenceData()
            {
                closeWindowByDefault = true,
                useDockableWindow = false,
                trackSelection = SearchSettings.trackSelection
            };

            var providers = searchContext.providers;
            evt.providerDatas = providers.Select(provider => new ProviderData()
            {
                id = provider.name.id,
                avgTime = (long)searchContext.searchElapsedTime,
                isEnabled = evt.useOverrideFilter ? true : searchContext.IsEnabled(provider.name.id),
                custom = ""
            }).ToArray();

            Send(EventName.quickSearch, evt);
        }

        private static bool RegisterEvents()
        {
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                return false;
            }

            if (!EditorAnalytics.enabled)
            {
                Console.WriteLine("[QS] Editor analytics are disabled");
                return false;
            }

            if (s_Registered)
            {
                return true;
            }

            var allNames = Enum.GetNames(typeof(EventName));
            if (allNames.Any(eventName => !RegisterEvent(eventName)))
            {
                return false;
            }

            s_Registered = true;
            return true;
        }

        private static bool RegisterEvent(string eventName)
        {
            const string vendorKey = "unity.quicksearch";
            var result = EditorAnalytics.RegisterEventWithLimit(eventName, 100, 1000, vendorKey);
            switch (result)
            {
                case AnalyticsResult.Ok:
                    {
                        #if QUICKSEARCH_ANALYTICS_LOGGING
                        Debug.Log($"QuickSearch: Registered event: {eventName}");
                        #endif
                        return true;
                    }
                case AnalyticsResult.TooManyRequests:

                    // this is fine - event registration survives domain reload (native)
                    return true;
                default:
                    {
                        Console.WriteLine($"[QS] Failed to register analytics event '{eventName}'. Result: '{result}'");
                        return false;
                    }
            }
        }

        private static void SendEvent(EventCategory category, string categoryName, string name, string message, string description,
            TimeSpan duration)
        {
            if (string.IsNullOrEmpty(categoryName) || string.IsNullOrEmpty(name))
            {
                Console.WriteLine(new ArgumentNullException().ToString());
                return;
            }

            var e = new GenericEvent()
            {
                category = categoryName,
                categoryId = (int)category,
                name = name,
                message = message,
                description = description,
                duration = (long)duration.TotalMilliseconds
            };

            Send(EventName.quickSearchGeneric, e);
        }

        private static void Send(EventName eventName, object eventData)
        {
            if (!RegisterEvents())
            {
                #if QUICKSEARCH_ANALYTICS_LOGGING
                Console.WriteLine($"[QS] Analytics disabled: event='{eventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(eventData, true)}");
                #endif
                return;
            }
            try
            {
                var result = EditorAnalytics.SendEventWithLimit(eventName.ToString(), eventData);
                if (result == AnalyticsResult.Ok)
                {
                    #if QUICKSEARCH_ANALYTICS_LOGGING
                    Console.WriteLine($"[QS] Event='{eventName}', time='{DateTime.Now:HH:mm:ss}', payload={EditorJsonUtility.ToJson(eventData, true)}");
                    #endif
                }
                else
                {
                    Console.WriteLine($"[QS] Failed to send event {eventName}. Result: {result}");
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
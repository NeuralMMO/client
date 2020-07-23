using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;

namespace Unity.QuickSearch.Providers
{
    [UsedImplicitly]
    static class Settings
    {
        internal const string type = "settings";
        private const string displayName = "Settings";

        static class SettingsPaths
        {
            public readonly static string[] value;

            static SettingsPaths()
            {
                value = FetchSettingsProviders().Select(provider => provider.settingsPath).ToArray();
            }

            private static SettingsProvider[] FetchSettingsProviders()
            {
                var type = typeof(SettingsService);
                var method = type.GetMethod("FetchSettingsProviders", BindingFlags.NonPublic | BindingFlags.Static);
                return (SettingsProvider[])method.Invoke(null, null);
            }
        }

        [UsedImplicitly, SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SearchProvider(type, displayName)
            {
                filterId = "se:",
                fetchItems = (context, items, provider) =>
                {
                    if (string.IsNullOrEmpty(context.searchQuery))
                        return null;

                    items.AddRange(SettingsPaths.value
                                    .Where(path => SearchUtils.MatchSearchGroups(context, path, true))
                                    .Select(path => provider.CreateItem(context, path, null, path, null, null)));
                    return null;
                },

                fetchLabel = (item, context) => item.label ?? (item.label = Utils.GetNameFromPath(item.id)),

                fetchThumbnail = (item, context) => Icons.settings
            };
        }

        [UsedImplicitly, SearchActionsProvider]
        internal static IEnumerable<SearchAction> ActionHandlers()
        {
            return new[]
            {
                new SearchAction(type, "open", null, "Open project settings...", (items) =>
                {
                    var item = items.Last();
                    if (item.id.StartsWith("Project/"))
                        SettingsService.OpenProjectSettings(item.id);
                    else
                        SettingsService.OpenUserPreferences(item.id);
                })
            };
        }
    }
}

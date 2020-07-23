//#define DEBUG_TIMING
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Unity.QuickSearch.Providers
{
    /// <summary>
    /// Scene provider. Can be used as a base class if you want to enhance the scene searching capabilities of QuickSearch.
    /// </summary>
    [UsedImplicitly]
    public class SceneProvider : SearchProvider
    {
        /// <summary>
        /// Fetch all the scene GameObjects.
        /// </summary>
        protected Func<GameObject[]> fetchGameObjects { get; set; }
        /// <summary>
        /// Build a list of keywords for all of the different components found in the scene.
        /// </summary>
        protected Func<GameObject, string[]> buildKeywordComponents { get; set; }
        /// <summary>
        /// Has the hierarchy since last search.
        /// </summary>
        protected bool m_HierarchyChanged = true;

        private GameObject[] m_GameObjects = null;
        private SceneQueryEngine m_SceneQueryEngine;

        /// <summary>
        /// Create a new SceneProvider.
        /// </summary>
        /// <param name="providerId">Unique Id for the scene provider.</param>
        /// <param name="filterId">Filter token id use to search only with this provider.</param>
        /// <param name="displayName">Provider display name used in UI.</param>
        public SceneProvider(string providerId, string filterId, string displayName)
            : base(providerId, displayName)
        {
            priority = 50;
            this.filterId = filterId;
            showDetails = true;
            showDetailsOptions = ShowDetailsOptions.Inspector | ShowDetailsOptions.Actions;

            isEnabledForContextualSearch = () =>
                Utils.IsFocusedWindowTypeName("SceneView") ||
                Utils.IsFocusedWindowTypeName("SceneHierarchyWindow");

            EditorApplication.hierarchyChanged += () => m_HierarchyChanged = true;

            toObject = (item, type) => ObjectFromItem(item, type);

            fetchItems = (context, items, provider) => SearchItems(context, provider);

            fetchLabel = (item, context) =>
            {
                if (item.label != null)
                    return item.label;

                var go = ObjectFromItem(item);
                if (!go)
                    return item.id;

                if (context == null || context.searchView == null || context.searchView.displayMode == DisplayMode.List)
                {
                    var transformPath = SearchUtils.GetTransformPath(go.transform);
                    var components = go.GetComponents<Component>();
                    if (components.Length > 2 && components[1] && components[components.Length - 1])
                        item.label = $"{transformPath} ({components[1].GetType().Name}..{components[components.Length - 1].GetType().Name})";
                    else if (components.Length > 1 && components[1])
                        item.label = $"{transformPath} ({components[1].GetType().Name})";
                    else
                        item.label = $"{transformPath} ({item.id})";

                    if (context != null)
                    {
                        long score = 1;
                        List<int> matches = new List<int>();
                        var sq = Utils.CleanString(context.searchQuery);
                        if (FuzzySearch.FuzzyMatch(sq, Utils.CleanString(item.label), ref score, matches))
                            item.label = RichTextFormatter.FormatSuggestionTitle(item.label, matches);
                    }
                }
                else
                {
                    item.label = go.name;
                }

                return item.label;
            };

            fetchDescription = (item, context) =>
            {
                var go = ObjectFromItem(item);
                return (item.description = SearchUtils.GetHierarchyPath(go));
            };

            fetchThumbnail = (item, context) =>
            {
                var obj = ObjectFromItem(item);
                if (obj == null)
                    return null;

                return (item.thumbnail = Utils.GetThumbnailForGameObject(obj));
            };

            fetchPreview = (item, context, size, options) =>
            {
                var obj = ObjectFromItem(item);
                if (obj == null)
                    return item.thumbnail;
                return Utils.GetSceneObjectPreview(obj, options, item.thumbnail);
            };

            startDrag = (item, context) =>
            {
                if (context.selection.Count > 1)
                    Utils.StartDrag(context.selection.Select(i => ObjectFromItem(i)).ToArray(), item.GetLabel(context, true));
                else
                    Utils.StartDrag(new [] { ObjectFromItem(item) }, item.GetLabel(context, true));
            };

            fetchPropositions = (context, options) =>
            {
                return m_SceneQueryEngine?.FindPropositions(context, options);
            };

            trackSelection = (item, context) => PingItem(item);

            fetchGameObjects = SearchUtils.FetchGameObjects;
            buildKeywordComponents = SceneQueryEngine.BuildKeywordComponents;
        }

        /// <summary>
        /// Create default action handles for scene SearchItem. See <see cref="SearchAction"/>.
        /// </summary>
        /// <param name="providerId">Provider Id registered for the action.</param>
        /// <returns>A collection of SearchActions working for a Scene SearchItem.</returns>
        public static IEnumerable<SearchAction> CreateActionHandlers(string providerId)
        {
            return new SearchAction[]
            {
                new SearchAction(providerId, "select", null, "Select object(s) in scene...")
                {
                    execute = (items) =>
                    {
                        FrameObjects(items.Select(i => i.provider.toObject(i, typeof(GameObject))).Where(i=>i).ToArray());
                    }
                },

                new SearchAction(providerId, "open", null, "Select containing asset...")
                {
                    handler = (item) =>
                    {
                        var pingedObject = PingItem(item);
                        if (pingedObject != null)
                        {
                            var go = pingedObject as GameObject;
                            var assetPath = SearchUtils.GetHierarchyAssetPath(go);
                            if (!String.IsNullOrEmpty(assetPath))
                                Utils.FrameAssetFromPath(assetPath);
                            else
                                FrameObject(go);
                        }
                    }
                }
            };
        }

        private IEnumerator SearchItems(SearchContext context, SearchProvider provider)
        {
            if (!String.IsNullOrEmpty(context.searchQuery))
            {
                if (m_HierarchyChanged)
                {
                    m_GameObjects = fetchGameObjects();
                    m_SceneQueryEngine = new SceneQueryEngine(m_GameObjects);
                    m_HierarchyChanged = false;
                }

                yield return m_SceneQueryEngine.Search(context).Select(gameObject =>
                {
                    if (!gameObject)
                        return null;
                    return AddResult(context, provider, gameObject.GetInstanceID().ToString(), 0, false);
                });
            }
            else if (context.wantsMore && context.filterType != null && String.IsNullOrEmpty(context.searchQuery))
            {
                yield return GameObject.FindObjectsOfType(context.filterType)
                    .Select(obj =>
                    {
                        if (obj is Component c)
                            return c.gameObject;
                        return obj as GameObject;
                    })
                    .Where(go => go)
                    .Select(go => AddResult(context, provider, go.GetInstanceID().ToString(), 999, false));
            }
        }

        private static SearchItem AddResult(SearchContext context, SearchProvider provider, string id, int score, bool useFuzzySearch)
        {
            string description = null;
            #if false
            description = $"F:{useFuzzySearch} {id} ({score})";
            #endif
            var item = provider.CreateItem(context, id, score, null, description, null, null);
            return SetItemDescriptionFormat(item, useFuzzySearch);
        }

        private static SearchItem SetItemDescriptionFormat(SearchItem item, bool useFuzzySearch)
        {
            item.options = SearchItemOptions.Ellipsis
                | SearchItemOptions.RightToLeft
                | (useFuzzySearch ? SearchItemOptions.FuzzyHighlight : SearchItemOptions.Highlight);
            return item;
        }

        private static UnityEngine.Object PingItem(SearchItem item)
        {
            var obj = ObjectFromItem(item);
            if (obj == null)
                return null;
            EditorGUIUtility.PingObject(obj);
            return obj;
        }

        private static void FrameObject(object obj)
        {
            Selection.activeGameObject = obj as GameObject ?? Selection.activeGameObject;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();
        }

        private static void FrameObjects(UnityEngine.Object[] objects)
        {
            Selection.instanceIDs = objects.Select(o => o.GetInstanceID()).ToArray();
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();
        }

        private static GameObject ObjectFromItem(SearchItem item)
        {
            var instanceID = Convert.ToInt32(item.id);
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            return obj;
        }

        private static UnityEngine.Object ObjectFromItem(SearchItem item, Type type)
        {
            var go = ObjectFromItem(item);
            if (!go)
                return null;

            if (typeof(Component).IsAssignableFrom(type))
                return go.GetComponent(type);

            return ObjectFromItem(item);
        }
    }

    static class BuiltInSceneObjectsProvider
    {
        const string k_DefaultProviderId = "scene";

        [UsedImplicitly, SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SceneProvider(k_DefaultProviderId, "h:", "Scene");
        }

        [UsedImplicitly, SearchActionsProvider]
        internal static IEnumerable<SearchAction> ActionHandlers()
        {
            return SceneProvider.CreateActionHandlers(k_DefaultProviderId);
        }

        [UsedImplicitly, Shortcut("Help/Quick Search/Scene")]
        internal static void OpenQuickSearch()
        {
            QuickSearch.OpenWithContextualProvider(k_DefaultProviderId, Query.type);
        }
    }
}

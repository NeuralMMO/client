//#define DEBUG_EXPRESSION_SEARCH

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    class SearchRequest : IDisposable
    {
        public string id { get; private set; }
        public bool resolving { get; private set; }
        public ExpressionType type { get; private set; }
        public List<SearchRequest> dependencies { get; private set; }
        public SearchContext context => runningQueries.LastOrDefault() ?? pendingQueries.FirstOrDefault();

        public Queue<SearchContext> pendingQueries { get; private set; }
        public List<SearchContext> runningQueries { get; private set; }

        private HashSet<SearchItem> pendingItems;

        public static readonly SearchRequest empty = new SearchRequest(ExpressionType.Undefined);
        private static Dictionary<string, Type> s_TypeTable = new Dictionary<string, Type>();

        /// <summary>
        /// Called every time new results are available.
        /// </summary>
        public event Action<IEnumerable<SearchItem>> resultsReceived;

        /// <summary>
        /// Called when the search expression has resolved.
        /// </summary>
        public event Action<SearchRequest> resolved;

        public SearchRequest(ExpressionType type)
        {
            id = Guid.NewGuid().ToString();
            this.type = type;
            pendingQueries = new Queue<SearchContext>();
            runningQueries = new List<SearchContext>();
            dependencies = new List<SearchRequest>();
            resolving = false;
        }

        public SearchRequest(ExpressionType type, SearchContext searchContext)
            : this(type)
        {
            pendingQueries.Enqueue(searchContext);
        }

        public void Dispose()
        {
            foreach (var r in runningQueries.ToArray())
                OnSearchEnded(r);
            resultsReceived = null;
        }

        public SearchRequest Join(Func<string, SearchRequest> handler)
        {
            SearchRequest joinedRequest = new SearchRequest(type);

            Resolve(results =>
            {
                foreach (var item in results)
                {
                    if (item == null)
                        continue;

                    var request = handler(item.id);
                    request.TransferTo(joinedRequest);
                }
            }, null);

            DependsOn(joinedRequest);
            return joinedRequest;
        }

        internal IEnumerable<SearchItem> SelectPath(SearchItem item)
        {

            var obj = item.provider?.toObject?.Invoke(item, typeof(UnityEngine.Object));
            if (!obj)
                return new [] { item };

            var path = AssetDatabase.GetAssetPath(obj);
            if (!String.IsNullOrEmpty(path))
                return new [] { new SearchItem(path) };

            if (obj is GameObject go)
                return new [] { new SearchItem(SearchUtils.GetHierarchyPath(go)) };

            return new [] { item };
        }

        internal ISet<SearchItem> SelectTypes(SearchItem item)
        {
            var results = new HashSet<SearchItem>();
            var obj = item.provider.toObject?.Invoke(item, typeof(UnityEngine.Object));
            if (!obj)
                return results;

            if (obj is GameObject go)
                results.UnionWith(go.GetComponents<Component>().Select(c => new SearchItem(c.GetType().Name)));
            else
                results.Add(new SearchItem(obj.GetType().Name));

            return results;
        }

        internal ISet<SearchItem> SelectObject(SearchItem item, string objectTypeName, string objectPropertyName, bool mapped, bool overrides)
        {
            var results = new HashSet<SearchItem>();

            if (objectTypeName == null || objectPropertyName == null)
                return results;

            if (!s_TypeTable.TryGetValue(objectTypeName ?? "", out var objectType))
            {
                objectType = TypeCache.GetTypesDerivedFrom(typeof(UnityEngine.Object)).FirstOrDefault(t => t.Name == objectTypeName);
                s_TypeTable[objectTypeName] = objectType;
            }

            if (objectType == null)
                return results;

            if (typeof(Component).IsAssignableFrom(objectType))
                return SelectComponent(item, objectTypeName, objectPropertyName, mapped, overrides);

            var assetObject = item.provider?.toObject(item, objectType);
            if (!assetObject)
                return results;

            SelectProperty(assetObject, objectPropertyName, results, mapped ? item : null);

            return results;
        }

        internal IEnumerable<SearchItem> SelectReferences(SearchItem item, string type, int depth)
        {
            var obj = item.provider?.toObject?.Invoke(item, typeof(UnityEngine.Object));
            if (!obj)
                return Enumerable.Empty<SearchItem>();

            var assetProvider = SearchService.GetProvider("asset");
            return SearchUtils.GetReferences(obj, depth)
                .Where(path => string.IsNullOrEmpty(type) || AssetDatabase.GetMainAssetTypeAtPath(path)?.Name == type)
                .Select(path => assetProvider.CreateItem(path));
        }

        internal ISet<SearchItem> SelectComponent(SearchItem item, string objectTypeName, string objectPropertyName, bool mapped, bool overrides)
        {
            var results = new HashSet<SearchItem>();

            if (objectTypeName == null || objectPropertyName == null)
                return results;

            var go = item.provider?.toObject(item, typeof(GameObject)) as GameObject;
            if (!go)
                return results;

            var correspondingObject = overrides ? go : (PrefabUtility.GetCorrespondingObjectFromOriginalSource(go) ?? go);
            if (!correspondingObject)
                return results;

            if (!s_TypeTable.TryGetValue(objectTypeName ?? "", out var objectType))
            {
                objectType = TypeCache.GetTypesDerivedFrom<Component>().FirstOrDefault(t => t.Name == objectTypeName);
                s_TypeTable[objectTypeName] = objectType;
            }

            if (objectType == null)
                return results;

            var components = correspondingObject.GetComponentsInChildren(objectType);
            foreach (var c in components)
            {
                if (!c)
                    continue;
                SelectProperty(c, objectPropertyName, results, mapped ? item : null);
            }

            return results;
        }

        private void SelectProperty(UnityEngine.Object obj, string objectPropertyName, ISet<SearchItem> results, SearchItem source)
        {
            using (var so = new SerializedObject(obj))
            {
                var property = so.FindProperty(objectPropertyName);
                if (property == null || property.isArray)
                    return;
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer: SelectValue(results, property.intValue, source); break;
                    case SerializedPropertyType.Enum: SelectValue(results, property.enumNames[property.enumValueIndex], source); break;
                    case SerializedPropertyType.Boolean: SelectValue(results, property.boolValue.ToString().ToLowerInvariant(), source); break;
                    case SerializedPropertyType.String: SelectValue(results, property.stringValue, source); break;
                    case SerializedPropertyType.Float: SelectValue(results, property.floatValue, source); break;
                    case SerializedPropertyType.FixedBufferSize: SelectValue(results, property.fixedBufferSize, source); break;
                    case SerializedPropertyType.Color: SelectValue(results, ColorUtility.ToHtmlStringRGB(property.colorValue), source); break;
                    case SerializedPropertyType.AnimationCurve: SelectValue(results, property.animationCurveValue, source); break;

                    case SerializedPropertyType.Vector2: SelectVector(results, property.vector2Value, source); break;
                    case SerializedPropertyType.Vector3: SelectVector(results, property.vector3Value, source); break;
                    case SerializedPropertyType.Vector4: SelectVector(results, property.vector4Value, source); break;
                    case SerializedPropertyType.Rect: SelectVector(results, property.rectValue, source); break;
                    case SerializedPropertyType.Bounds: SelectVector(results, property.boundsValue, source); break;
                    case SerializedPropertyType.Quaternion: SelectVector(results, property.quaternionValue, source); break;
                    case SerializedPropertyType.Vector2Int: SelectVector(results, property.vector2IntValue, source); break;
                    case SerializedPropertyType.Vector3Int: SelectVector(results, property.vector3IntValue, source); break;
                    case SerializedPropertyType.RectInt: SelectVector(results, property.rectIntValue, source); break;
                    case SerializedPropertyType.BoundsInt: SelectVector(results, property.boundsIntValue, source); break;

                    case SerializedPropertyType.ManagedReference: SelectValue(results, property.managedReferenceFullTypename, source); break;
                    case SerializedPropertyType.ObjectReference: SelectAssetPath(results, property.objectReferenceValue, source); break;
                    case SerializedPropertyType.ExposedReference: SelectAssetPath(results, property.exposedReferenceValue, source); break;

                    case SerializedPropertyType.Generic:
                        break;

                    case SerializedPropertyType.Gradient:
                    case SerializedPropertyType.Character:
                    case SerializedPropertyType.LayerMask:
                    case SerializedPropertyType.ArraySize:
                    default:
                        Debug.LogWarning($"Cannot select {property.propertyType} {objectPropertyName} with {id}");
                        break;
                }
            }
        }

        private void SelectVector<T>(ISet<SearchItem> results, T value, SearchItem source) where T : struct
        {
            results.Add(MapItem(source, Convert.ToString(value).Replace("(", "").Replace(")", "").Replace(" ", "")));
        }

        private void SelectValue(ISet<SearchItem> results, object value, SearchItem source)
        {
            results.Add(MapItem(source, value));
        }

        private void SelectAssetPath(ISet<SearchItem> results, UnityEngine.Object obj, SearchItem source)
        {
            var assetPath = AssetDatabase.GetAssetPath(obj);
            if (source == null && String.IsNullOrEmpty(assetPath))
                return;
            results.Add(MapItem(source, assetPath));
        }

        private SearchItem MapItem(SearchItem source, object value)
        {
            if (source == null)
                source = new SearchItem(Convert.ToString(value));
            else
            {
                source.value = value;
                source.description = value.ToString();
            }
            return source;
        }

        public void Resolve(Action<IEnumerable<SearchItem>> onSearchItemReceived, Action<SearchRequest> finishedHandler)
        {
            if (onSearchItemReceived != null)
                resultsReceived += onSearchItemReceived;

            if (finishedHandler != null)
                resolved += finishedHandler;

            DebugLog($"Resolving <b>{type}</b>");
            resolving = true;
            while (pendingQueries.Count > 0)
            {
                var r = pendingQueries.Dequeue();
                runningQueries.Add(r);

                r.sessionEnded += OnSearchEnded;
                r.asyncItemReceived += OnSearchItemsReceived;

                DebugLog($"Fetching items with <a>{r.searchQuery}</a>");
                SearchService.GetItems(r, SearchFlags.FirstBatchAsync);

                if (!r.searchInProgress)
                    runningQueries.Remove(context);
            }

            UpdateState();

            // Check if we still have some pending queries to resolve.
            if (resolving)
            {
                EditorApplication.update -= DeferredResolve;
                EditorApplication.update += DeferredResolve;
            }
        }

        private void DeferredResolve()
        {
            EditorApplication.update -= DeferredResolve;
            Resolve(null, null);
        }

        [System.Diagnostics.Conditional("DEBUG_EXPRESSION_SEARCH")]
        private void DebugLog(string msg)
        {
            UnityEngine.Debug.Log($"<i>[{id}]</i> {msg}");
        }

        public void ProcessItems(IEnumerable<SearchItem> items)
        {
            ProcessItems(context, items);
        }

        public void ProcessItems(SearchContext context, IEnumerable<SearchItem> items)
        {
            #if DEBUG_EXPRESSION_SEARCH
            if (items is ICollection list)
            {
                if (list.Count > 0)
                    DebugLog($"Received {list.Count} items for {context?.searchQuery ?? type.ToString()}");
            }
            else
                DebugLog($"Received more items for {context?.searchQuery ?? type.ToString()}");
            #endif

            if (resultsReceived == null)
            {
                if (pendingItems == null)
                    pendingItems = new HashSet<SearchItem>(items);
                else
                    pendingItems.UnionWith(items);
            }
            else
            {
                resultsReceived(items);
            }
        }

        private void OnSearchEnded(SearchContext context)
        {
            context.sessionEnded -= OnSearchEnded;
            context.asyncItemReceived -= OnSearchItemsReceived;
            runningQueries.Remove(context);

            UpdateState();
        }

        private void UpdateState()
        {
            if (!resolving)
                return;

            if (pendingItems?.Count > 0)
            {
                Debug.Assert(resultsReceived != null);
                resultsReceived?.Invoke(pendingItems);
                pendingItems = null;
            }

            if (dependencies.Count == 0 && runningQueries.Count == 0 && pendingQueries.Count == 0)
            {
                resolving = false;
                resolved?.Invoke(this);
                DebugLog($"Resolved <b>{type}</b>");

                resolved = null;
                resultsReceived = null;
            }
        }

        private void OnSearchItemsReceived(SearchContext context, IEnumerable<SearchItem> items)
        {
            ProcessItems(context, items);
        }

        public void DependsOn(SearchRequest exs)
        {
            dependencies.Add(exs);
            exs.resolved += OnDependencyFinished;
        }

        private void OnDependencyFinished(SearchRequest exs)
        {
            exs.resolved -= OnDependencyFinished;
            dependencies.Remove(exs);
        }

        public void TransferTo(SearchRequest exSearch)
        {
            while (pendingQueries.Count > 0)
                exSearch.pendingQueries.Enqueue(pendingQueries.Dequeue());
        }
    }
}

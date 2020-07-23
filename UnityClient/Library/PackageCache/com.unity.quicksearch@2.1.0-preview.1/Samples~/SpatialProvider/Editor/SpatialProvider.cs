using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Unity.QuickSearch;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Custom provider showing how to implement a custom Query Engine supporting a Spatial search filter.
/// </summary>
public static class SpatialProvider
{
    internal static string type = "spl";
    internal static string displayName = "Spatial";

    static GameObject[] s_GameObjects;
    static QueryEngine<GameObject> s_QueryEngine;

    static readonly Stack<StringBuilder> _SbPool = new Stack<StringBuilder>();

    [SearchItemProvider]
    internal static SearchProvider CreateProvider()
    {
        return new SearchProvider(type, displayName)
        {
            active = false,
            filterId = "spl:",
            onEnable = OnEnable,
            fetchItems = (context, items, provider) => SearchItems(context, provider),
            fetchLabel = FetchLabel,
            fetchDescription = FetchDescription,
            fetchThumbnail = FetchThumbnail,
            fetchPreview = FetchPreview,
            trackSelection = TrackSelection,
            isExplicitProvider = false,
        };
    }

    static void OnEnable()
    {
        s_GameObjects = SearchUtils.FetchGameObjects();
        s_QueryEngine = new QueryEngine<GameObject>();

        // Id supports all operators
        s_QueryEngine.AddFilter("id", go => go.GetInstanceID());
        // Name supports only :, = and !=
        s_QueryEngine.AddFilter("n", go => go.name, new []{":", "=", "!="});

        // Add distance filtering. Does not support :.
        s_QueryEngine.AddFilter("dist", DistanceHandler, DistanceParamHandler, new []{"=", "!=", "<", ">", "<=", ">="});
    }

    static IEnumerator SearchItems(SearchContext context, SearchProvider provider)
    {
        var query = s_QueryEngine.Parse(context.searchQuery);
        if (!query.valid)
            yield break;

        var filteredObjects = query.Apply(s_GameObjects);
        foreach (var filteredObject in filteredObjects)
        {
            yield return provider.CreateItem(filteredObject.GetInstanceID().ToString(), null, null, null, filteredObject.GetInstanceID());
        }
    }

    static string FetchLabel(SearchItem item, SearchContext context)
    {
        if (item.label != null)
            return item.label;

        var go = ObjectFromItem(item);
        if (!go)
            return item.id;

        var transformPath = GetTransformPath(go.transform);
        var components = go.GetComponents<Component>();
        if (components.Length > 2 && components[1] && components[components.Length - 1])
            item.label = $"{transformPath} ({components[1].GetType().Name}..{components[components.Length - 1].GetType().Name})";
        else if (components.Length > 1 && components[1])
            item.label = $"{transformPath} ({components[1].GetType().Name})";
        else
            item.label = $"{transformPath} ({item.id})";

        return item.label;
    }

    static string FetchDescription(SearchItem item, SearchContext context)
    {
        var go = ObjectFromItem(item);
        return (item.description = SearchUtils.GetHierarchyPath(go));
    }

    static Texture2D FetchThumbnail(SearchItem item, SearchContext context)
    {
        var obj = ObjectFromItem(item);
        if (obj == null)
            return null;

        return item.thumbnail = GetThumbnailForGameObject(obj);
    }

    static Texture2D FetchPreview(SearchItem item, SearchContext context, Vector2 size, FetchPreviewOptions options)
    {
        var obj = ObjectFromItem(item);
        if (obj == null)
            return item.thumbnail;

        var assetPath = SearchUtils.GetHierarchyAssetPath(obj, true);
        if (String.IsNullOrEmpty(assetPath))
            return item.thumbnail;
        return AssetPreview.GetAssetPreview(obj) ?? GetAssetPreviewFromPath(assetPath, size, options);
    }

    static void TrackSelection(SearchItem item, SearchContext context)
    {
        var obj = ObjectFromItem(item);
        Selection.activeGameObject = obj ?? Selection.activeGameObject;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();
    }

    static float DistanceHandler(GameObject go, Vector3 p)
    {
        return (go.transform.position - p).magnitude;
    }

    static Vector3 DistanceParamHandler(string param)
    {
        if (param == "selection")
        {
            var centerPoint = Selection.gameObjects.Select(go => go.transform.position).Aggregate((v1, v2) => v1 + v2);
            centerPoint /= Selection.gameObjects.Length;
            return centerPoint;
        }

        if (param.StartsWith("[") && param.EndsWith("]"))
        {
            param = param.Trim('[', ']');
            var vectorTokens = param.Split(',');
            var vectorValues = vectorTokens.Select(token => float.Parse(token, CultureInfo.InvariantCulture.NumberFormat)).ToList();
            while (vectorValues.Count < 3)
                vectorValues.Add(0f);
            return new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]);
        }

        var obj = s_GameObjects.FirstOrDefault(go => go.name == param);
        if (!obj)
            return Vector3.zero;
        return obj.transform.position;
    }

    static GameObject ObjectFromItem(SearchItem item)
    {
        var instanceID = Convert.ToInt32(item.id);
        var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        return obj;
    }

    private static string GetTransformPath(Transform tform)
    {
        if (tform.parent == null)
            return "/" + tform.name;
        return GetTransformPath(tform.parent) + "/" + tform.name;
    }

    static Texture2D GetThumbnailForGameObject(GameObject go)
    {
        var thumbnail = PrefabUtility.GetIconForGameObject(go);
        if (thumbnail)
            return thumbnail;
        return EditorGUIUtility.ObjectContent(go, go.GetType()).image as Texture2D;
    }

    static Texture2D GetAssetPreviewFromPath(string path, Vector2 previewSize, FetchPreviewOptions previewOptions)
    {
        var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (obj == null)
            return null;
        var preview = AssetPreview.GetAssetPreview(obj);
        if (preview == null || previewOptions.HasFlag(FetchPreviewOptions.Large))
        {
            var largePreview = AssetPreview.GetMiniThumbnail(obj);
            if (preview == null || (largePreview != null && largePreview.width > preview.width))
                preview = largePreview;
        }
        return preview;
    }
}

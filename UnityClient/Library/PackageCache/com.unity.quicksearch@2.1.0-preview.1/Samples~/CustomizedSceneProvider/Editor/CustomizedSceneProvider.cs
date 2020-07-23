// -----------------------------------------------------------------------------
//
// Simple Scene Provider customization example. This sample shows how
// you can use the SceneQueryEngineFilter and SceneQueryEngineParameterTransformer
// attributes to customize the search engine used by the Scene Provider.
//
// -----------------------------------------------------------------------------
using System.Globalization;
using System.Linq;
using Unity.QuickSearch.Providers;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CustomizedSceneProvider use to showcase <see cref="SceneQueryEngineFilter"/> and <see cref="SceneQueryEngineParameterTransformer"/>
/// </summary>
public static class CustomizedSceneProvider
{
    // Add a new filter function "dist" that returns the distance between an object and a point. This filter
    // needs a parameter transformer to transform the text into a point. Also, it doesn't support the operator "contains" (:).
    [SceneQueryEngineFilter("dist", "DistanceParamHandler", new []{"=", "!=", "<", ">", "<=", ">="})]
    static float DistanceHandler(GameObject go, Vector3 p)
    {
        return (go.transform.position - p).magnitude;
    }

    // Identify this function as a parameter transformer with the attribute SceneQueryEngineParameterTransformer.
    // This transformer handles the text inside the parenthesis of the filter function, i.e. "dist([10, 15])<10" this function will receive
    // "[10,15]" as parameter.
    [SceneQueryEngineParameterTransformer]
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

        var obj = GameObject.Find(param);
        if (!obj)
            return Vector3.zero;
        return obj.transform.position;
    }

    /// <summary>
    /// Computes how many lights are affecting a given mesh
    /// </summary>
    /// <param name="go"></param>
    /// <returns>If valid, we return how many lights affects a mesh</returns>
    [SceneQueryEngineFilter("lights", new[] { "=", "!=", "<", ">", "<=", ">=" })]
    internal static int? MeshRendererAffectedByLightsSceneFilter(GameObject go)
    {
        if (!go.TryGetComponent<MeshRenderer>(out var meshRenderer))
            return null;

        if (!meshRenderer.isVisible)
            return null;

        var lightEffectCount = 0;
        var gp = go.transform.position;
        foreach (var light in Object.FindObjectsOfType<Light>())
        {
            if (!light.isActiveAndEnabled)
                continue;

            var lp = light.transform.position;

            var distance = Vector3.Distance(gp, lp);
            if (distance > light.range)
                continue;

            if (light.type == UnityEngine.LightType.Spot)
            {
                var da = Vector3.Dot(light.transform.forward, lp - gp);
                if (da > Mathf.Deg2Rad * light.spotAngle)
                    continue;
            }

            lightEffectCount++;
        }

        return lightEffectCount;
    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.QuickSearch;
using UnityEditor;
using UnityEngine;

static class TypeProvider
{
    private static SearchIndexer index;
    private static QueryEngine<Type> QE;
    private static Type[] allTypes = TypeCache.GetTypesDerivedFrom<UnityEngine.Object>().ToArray();

    [MenuItem("Window/Quick Search/Rebuild type index")]
    private static void Build()
    {
        allTypes = TypeCache.GetTypesDerivedFrom<UnityEngine.Object>().ToArray();

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        QE = new QueryEngine<Type>();
        QE.AddFilter("m", t => t.GetMethods().Length);
        QE.AddFilter("id", t => t.FullName);
        QE.AddFilter("asm", t => Path.GetFileNameWithoutExtension(t.Assembly.Location).ToLowerInvariant());
        QE.SetSearchDataCallback(t => new[] { t.Name });
        Debug.Log($"QuerEngine initialization took {sw.Elapsed.TotalMilliseconds,3:0.##} ms");

        sw.Restart();
        index = new SearchIndexer();
        index.Start();
        foreach (var t in allTypes)
        {
            var di = index.AddDocument(t.AssemblyQualifiedName, false);
            index.AddWord(t.Name.ToLowerInvariant(), 0, di);
            index.AddNumber("m", t.GetMethods().Length, 0, di);
            index.AddProperty("id", t.FullName.ToLowerInvariant(),
                minVariations: t.FullName.Length, maxVariations: t.FullName.Length,
                score: 0, documentIndex: di,
                saveKeyword: false, exact: true);
            index.AddProperty("asm", Path.GetFileNameWithoutExtension(t.Assembly.Location).ToLowerInvariant(), di);
        }
        index.Finish(() => Debug.Log($"Type indexing took {sw.Elapsed.TotalMilliseconds,3:0.##} ms"));
    }

    private static IEnumerable<Type> IDX_FetchItems(string searchQuery)
    {
        while (!index.IsReady())
            yield return null;
        foreach (var e in index.Search(searchQuery))
            yield return Type.GetType(e.id);
    }    

    private static IEnumerable<Type> QE_FetchItems(string searchQuery)
    {
        var query = QE.Parse(searchQuery);
        if (!query.valid)
            return Enumerable.Empty<Type>();
        return query.Apply(allTypes);
    }

    private static SearchProvider CreateProvider(string id, string label, Func<string, IEnumerable<Type>> fetchHandler)
    {
        return new SearchProvider(id, label)
        {
            active = false,
            filterId = $"{id}:",
            isExplicitProvider = true,
            onEnable = OnEnable,
            fetchItems = (context, items, provider) => FetchItems(context, provider, fetchHandler)
        };
    }

    private static void OnEnable()
    {
        if (allTypes == null)
            Build();
    }

    private static IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider, Func<string, IEnumerable<Type>> fetchHandler)
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        foreach(var t in fetchHandler(context.searchQuery).Where(t => t != null))
            yield return provider.CreateItem(context, t.AssemblyQualifiedName, 0, t.FullName, t.AssemblyQualifiedName, null, t);
        Debug.Log($"{provider.name.displayName} Searching {allTypes.Length} types with <b><i>{context.searchQuery}</i></b> took {sw.Elapsed.TotalMilliseconds,2:0.0} ms");
    }

    [SearchItemProvider] internal static SearchProvider QE_CreateProvider() => CreateProvider("tqe", "Types (QE)", QE_FetchItems);
    [SearchItemProvider] internal static SearchProvider IDX_CreateProvider() => CreateProvider("tidx", "Types (IDX)", IDX_FetchItems);
}

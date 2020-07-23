using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.QuickSearch;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

#if UNITY_2020_1_OR_NEWER
static class DependencyProvider
{
    const string providerId = "dep";
    const string dependencyIndexLibraryPath = "Library/dependencies.index";
    private static SearchIndexer index;

    private readonly static Regex guidRx = new Regex(@"guid:\s+([a-z0-9]{32})");

    private readonly static ConcurrentDictionary<string, string> guidToPathMap = new ConcurrentDictionary<string, string>();
    private readonly static ConcurrentDictionary<string, string> pathToGuidMap = new ConcurrentDictionary<string, string>();
    private readonly static ConcurrentDictionary<string,  ConcurrentDictionary<string, byte>> guidToRefsMap = new ConcurrentDictionary<string,  ConcurrentDictionary<string, byte>>();
    private readonly static ConcurrentDictionary<string,  ConcurrentDictionary<string, byte>> guidFromRefsMap = new ConcurrentDictionary<string,  ConcurrentDictionary<string, byte>>();
    private readonly static Dictionary<string, int> guidToDocMap = new Dictionary<string, int>();

    private readonly static string[] builtinGuids = new string[]
    {
         "0000000000000000d000000000000000",
         "0000000000000000e000000000000000",
         "0000000000000000f000000000000000"
    };

    [MenuItem("Window/Quick Search/Rebuild dependency index")]
    private static void Build()
    {
        pathToGuidMap.Clear();
        guidToPathMap.Clear();
        guidToRefsMap.Clear();
        guidFromRefsMap.Clear();
        guidToDocMap.Clear();

        var allGuids = AssetDatabase.FindAssets("a:all");
        foreach (var guid in allGuids.Concat(builtinGuids))
        {
            TrackGuid(guid);
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            pathToGuidMap.TryAdd(assetPath, guid);
            guidToPathMap.TryAdd(guid, assetPath);
        }

        Task.Run(RunThreadIndexing);
    }

    private static void Load(string indexPath)
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        var indexBytes = File.ReadAllBytes(indexPath);
        index = new SearchIndexer();
        index.LoadBytes(indexBytes, (success) =>
        {
            if (!success)
                Debug.LogError($"Failed to load dependency index at {indexPath}");
            else
                Debug.Log($"Loading dependency index took {sw.Elapsed.TotalMilliseconds,3:0.##} ms ({EditorUtility.FormatBytes(indexBytes.Length)} bytes)");
        });
    }

    private static void RunThreadIndexing()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        index = new SearchIndexer();
        index.Start();
        int completed = 0;
        var metaFiles = Directory.GetFiles("Assets", "*.meta", SearchOption.AllDirectories);
        var progressId = Progress.Start($"Scanning dependencies ({metaFiles.Length} assets)");

        var exclude = new[] { ".unity", ".prefab" };

        Parallel.ForEach(metaFiles, mf =>
        {
            Progress.Report(progressId, completed / (float)metaFiles.Length, mf);
            var assetPath = mf.Replace("\\", "/").Substring(0, mf.Length - 5).ToLowerInvariant();
            if (!File.Exists(assetPath))
                return;

            var extension = Path.GetExtension(assetPath);
            if (exclude.Contains(extension))
                return;

            var guid = ToGuid(assetPath);
            Progress.Report(progressId, completed / (float)metaFiles.Length, assetPath);

            TrackGuid(guid);
            pathToGuidMap.TryAdd(assetPath, guid);
            guidToPathMap.TryAdd(guid, assetPath);

            var mfc = File.ReadAllText(mf);
            ScanDependencies(guid, mfc);

            using (var file = new StreamReader(assetPath))
            {
                var header = new char[5];
                if (file.ReadBlock(header, 0, header.Length) == header.Length &&
                    header[0] == '%' && header[1] == 'Y' && header[2] == 'A' && header[3] == 'M' && header[4] == 'L')
                {
                    var ac = file.ReadToEnd();
                    ScanDependencies(guid, ac);
                }
            }

            Progress.Report(progressId, ++completed / (float)metaFiles.Length);
        });
        Progress.Finish(progressId, Progress.Status.Succeeded);

        completed = 0;
        var total = pathToGuidMap.Count + guidToRefsMap.Count + guidFromRefsMap.Count;
        progressId = Progress.Start($"Indexing {total} dependencies");
        foreach (var kvp in pathToGuidMap)
        {
            var guid = kvp.Value;
            var path = kvp.Key;

            var ext = Path.GetExtension(path);
            if (ext.Length > 0 && ext[0] == '.')
                ext = ext.Substring(1);

            Progress.Report(progressId, completed++ / (float)total, path);

            var di = AddGuid(guid);

            index.AddExactWord("all", 0, di);
            AddStaticProperty("id", guid, di);
            AddStaticProperty("path", path, di);
            AddStaticProperty("t", GetExtension(path), di);
            index.AddWord(guid, guid.Length, 0, di);
            index.AddWord(Path.GetFileNameWithoutExtension(path), 0, di);
        }

        foreach (var kvp in guidToRefsMap)
        {
            var guid = kvp.Key;
            var refs = kvp.Value.Keys;
            var di = AddGuid(guid);

            Progress.Report(progressId, completed++ / (float)total, guid);

            index.AddNumber("count", refs.Count, 0, di);
            foreach (var r in refs)
                AddStaticProperty("to", r, di);
        }

        foreach (var kvp in guidFromRefsMap)
        {
            var guid = kvp.Key;
            var refs = kvp.Value.Keys;
            var di = AddGuid(guid);

            Progress.Report(progressId, completed++ / (float)total, guid);

            index.AddNumber("in", refs.Count, 0, di);
            foreach (var r in refs)
            {
                AddStaticProperty("from", r, di);

                if (guidToPathMap.TryGetValue(r, out var rp))
                    AddStaticProperty("t", GetExtension(rp), di);
            }

            if (guidToPathMap.TryGetValue(guid, out var path))
                AddStaticProperty("is", "valid", di);
            else
            {
                AddStaticProperty("is", "broken", di);

                var refString = string.Join(", ", refs.Select(r =>
                {
                    if (guidToPathMap.TryGetValue(r, out var rp))
                        return rp;
                    return r;
                }));
                index.GetDocument(di).metadata = $"Refered by {refString}";
            }
        }

        Progress.SetDescription(progressId, $"Saving dependency index at {dependencyIndexLibraryPath}");

        index.Finish((bytes) =>
        {
            File.WriteAllBytes(dependencyIndexLibraryPath, bytes);
            Progress.Finish(progressId, Progress.Status.Succeeded);

            Debug.Log($"Dependency indexing took {sw.Elapsed.TotalMilliseconds,3:0.##} ms " +
                $"and was saved at {dependencyIndexLibraryPath} ({EditorUtility.FormatBytes(bytes.Length)} bytes)");
        }, removedDocuments: null);
    }

    private static string GetExtension(string path)
    {
        var ext = Path.GetExtension(path);
        if (ext.Length > 0 && ext[0] == '.')
            ext = ext.Substring(1);
        return ext;
    }

    private static void AddStaticProperty(string key, string value, int di)
    {
        index.AddProperty(key, value, value.Length, value.Length, 0, di, false, false);
    }

    private static void ScanDependencies(string guid, string content)
    {
        foreach (Match match in guidRx.Matches(content))
        {
            if (match.Groups.Count < 2)
                continue;
            var rg = match.Groups[1].Value;
            if (rg == guid)
                continue;

            TrackGuid(rg);

            guidToRefsMap[guid].TryAdd(rg, 0);
            guidFromRefsMap[rg].TryAdd(guid, 0);
        }
    }

    private static void TrackGuid(string guid)
    {
        if (!guidToRefsMap.ContainsKey(guid))
            guidToRefsMap.TryAdd(guid, new ConcurrentDictionary<string, byte>());

        if (!guidFromRefsMap.ContainsKey(guid))
            guidFromRefsMap.TryAdd(guid, new ConcurrentDictionary<string, byte>());
    }

    private static int AddGuid(string guid)
    {
        if (guidToDocMap.TryGetValue(guid, out var di))
            return di;

        di = index.AddDocument(guid);
        guidToDocMap.Add(guid, di);
        return di;
    }

    private static string ToGuid(string assetPath)
    {
        string metaFile = $"{assetPath}.meta";
        if (!File.Exists(metaFile))
            return null;

        string line;
        using (var file = new StreamReader(metaFile))
        {
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("guid:", StringComparison.Ordinal))
                    continue;
                return line.Substring(6);
            }
        }

        return null;
    }

    [SearchItemProvider]
    internal static SearchProvider CreateProvider()
    {
        return new SearchProvider(providerId, "Dependencies")
        {
            active = false,
            filterId = $"dep:",
            isExplicitProvider = true,
            onEnable = OnEnable,
            fetchItems = (context, items, provider) => FetchItems(context, provider),
            fetchLabel = FetchLabel,
            fetchDescription = FetchDescription,
            trackSelection = TrackSelection
        };
    }

    private static void OnEnable()
    {
        if (index == null)
        {
            if (File.Exists(dependencyIndexLibraryPath))
                Load(dependencyIndexLibraryPath);
            else
                Build();
        }
    }

    [SearchActionsProvider]
    internal static IEnumerable<SearchAction> ActionHandlers()
    {
        return new[]
        {
            Goto("to", "Show references...", "to"),
            Goto("from", "Show usage...", "from")
        };
    }

    private static SearchAction Goto(string action, string title, string filter)
    {
        return new SearchAction("dep", action, null, title, (SearchItem item) => item.context?.searchView?.SetSearchText($"dep: {filter}:{item.id}"))
        {
            closeWindowAfterExecution = false
        };
    }

    private static string FetchLabel(SearchItem item, SearchContext context)
    {
        if (guidToPathMap.ContainsKey(item.id))
            return item.id;

        var assetPath = AssetDatabase.GUIDToAssetPath(item.id);
        if (!string.IsNullOrEmpty(assetPath))
            return item.id;

        if (guidFromRefsMap.ContainsKey(item.id))
            return $"<color=#EE6666>{item.id}</color>";

        return $"<color=red>{item.id}</color>";
    }

    private static string GetDescrition(SearchItem item)
    {
        var assetPath = AssetDatabase.GUIDToAssetPath(item.id);
        if (!string.IsNullOrEmpty(assetPath))
            return assetPath;

        var metaString = index.GetDocument((int)item.data).metadata;
        if (!string.IsNullOrEmpty(metaString))
            return metaString;

        return "<invalid>";
    }

    private static string FetchDescription(SearchItem item, SearchContext context)
    {
        var description = GetDescrition(item);
        if (item.options.HasFlag(SearchItemOptions.Compacted))
            description = $"{FetchLabel(item, context)} ({description})";
        return (item.description = description);
    }

    private static void TrackSelection(SearchItem item, SearchContext context)
    {
        EditorGUIUtility.systemCopyBuffer = item.id;
    }

    private static IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        while (!index.IsReady())
            yield return null;
        foreach (var e in index.Search(context.searchQuery))
            yield return provider.CreateItem(context, e.id, e.score, null, null, null, e.index);
        Debug.Log($"{provider.name.displayName} Searching dependencies with <b><i>{context.searchQuery}</i></b> took {sw.Elapsed.TotalMilliseconds,2:0.0} ms");
    }

    [Shortcut("Help/Quick Search/Dependencies")]
    internal static void OpenShortcut()
    {
        var qs = QuickSearch.OpenWithContextualProvider(providerId);
        qs.itemIconSize = 1; // Open in list view by default.
    }
}
#endif
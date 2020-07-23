//#define DEBUG_INDEXING
//#define DEBUG_TIMERS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

namespace Unity.QuickSearch
{
    using Task = SearchTask<TaskData>;

    class TaskData
    {
        public TaskData(byte[] bytes, SearchIndexer combinedIndex)
        {
            this.bytes = bytes;
            this.combinedIndex = combinedIndex;
        }

        public readonly byte[] bytes;
        public readonly SearchIndexer combinedIndex;
    }

    [ExcludeFromPreset]
    class SearchDatabase : ScriptableObject, ITaskReporter
    {
        // 1- First version
        // 2- Rename ADBIndex for SearchDatabase
        // 3- Add db name and type
        // 4- Add better ref: property indexing
        // 5- Fix asset has= property indexing.
        // 6- Use produce artifacts async
        // 7- Update how keywords are encoded
        public const int version = (7 << 8) ^ SearchIndexEntryImporter.version;

        public enum IndexType
        {
            asset,
            scene,
            prefab
        }

        enum ImportMode
        {
            Synchronous,
            Asynchronous,
            NoImport
        }

        [Flags]
        enum ChangeStatus
        {
            Deleted = 1 << 1,
            Missing = 1 << 2,
            Changed = 1 << 3,

            All = Deleted | Missing | Changed,
            Any = All
        }

        [Serializable]
        public class Options // TODO: replace this class with a enum flags IndexingOptions
        {
            public bool disabled = false;           // Disables the index

            public bool types = true;               // Index type information about objects
            public bool properties = false;         // Index serialized properties of objects
            public bool extended = false;           // Index as many properties as possible (i.e. asset import settings)
            public bool dependencies = false;       // Index object dependencies (i.e. ref:<name>)

            public override int GetHashCode()
            {
                return (       types ? (int)IndexingOptions.Types        : 0) |
                       (  properties ? (int)IndexingOptions.Properties   : 0) |
                       (    extended ? (int)IndexingOptions.Extended     : 0) |
                       (dependencies ? (int)IndexingOptions.Dependencies : 0);
            }
        }

        [Serializable]
        public class Settings
        {
            [NonSerialized] public string root;
            [NonSerialized] public string source;
            [NonSerialized] public string guid;

            public string name;
            public string type = nameof(IndexType.asset);
            public string[] roots;
            public string[] includes;
            public string[] excludes;

            public Options options;
            public int baseScore = 100;
        }

        [System.Diagnostics.DebuggerDisplay("{guid} > {path}")]
        class IndexArtifact
        {
            public IndexArtifact(string guid, Hash128 key)
            {
                this.guid = guid;
                this.key = key;
                path = null;
            }

            public bool valid => key.isValid;

            public readonly string guid;
            public Hash128 key;
            public string path;
        }

        [SerializeField] public long timestamp;
        [SerializeField] public Settings settings;
        [SerializeField, HideInInspector] public byte[] bytes;

        [NonSerialized] private int m_InstanceID = 0;
        [NonSerialized] private Task m_CurrentTask;
        [NonSerialized] private static readonly Dictionary<string, Type> IndexerFactory = new Dictionary<string, Type>();
        [NonSerialized] private volatile int m_UpdateTasks = 0;

        public ObjectIndexer index { get; internal set; }
        public bool loaded { get; private set; }
        public bool ready => this && loaded && index != null && index.IsReady();
        public bool updating => m_UpdateTasks > 0 || !loaded || m_CurrentTask != null || !ready;
        public string path => AssetDatabase.GetAssetPath(this);

        internal static event Action<SearchDatabase> indexLoaded;

        static SearchDatabase()
        {
            IndexerFactory[nameof(IndexType.asset)] = typeof(AssetIndexer);
            IndexerFactory[nameof(IndexType.scene)] = typeof(SceneIndexer);
            IndexerFactory[nameof(IndexType.prefab)] = typeof(SceneIndexer);
        }

        private static bool IsValidType(string path, string[] types)
        {
            var settings = LoadSettings(path);
            if (settings.options.disabled)
                return false;
            return types.Length ==0 || types.Contains(settings.type);
        }

        public static IEnumerable<SearchDatabase> Enumerate(params string[] types)
        {
            const string k_SearchDataFindAssetQuery = "t:SearchDatabase a:all";
            return AssetDatabase.FindAssets(k_SearchDataFindAssetQuery).Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => IsValidType(path, types))
                .Select(path => AssetDatabase.LoadAssetAtPath<SearchDatabase>(path)).Where(db => db != null)
                .Select(db => { db.Log("Enumerate"); return db; });
        }

        public static Settings LoadSettings(string settingsPath)
        {
            var settingsJSON = File.ReadAllText(settingsPath);
            var indexSettings = JsonUtility.FromJson<Settings>(settingsJSON);

            indexSettings.source = settingsPath;
            indexSettings.guid = AssetDatabase.AssetPathToGUID(settingsPath);
            indexSettings.root = Path.GetDirectoryName(settingsPath).Replace("\\", "/");
            if (String.IsNullOrEmpty(indexSettings.name))
                indexSettings.name = Path.GetFileNameWithoutExtension(settingsPath);

            return indexSettings;
        }

        public static ObjectIndexer CreateIndexer(Settings settings, string settingsPath = null)
        {
            // Fix the settings root if needed
            if (!String.IsNullOrEmpty(settingsPath))
            {
                if (String.IsNullOrEmpty(settings.source))
                    settings.source = settingsPath;

                if (String.IsNullOrEmpty(settings.root))
                    settings.root = Path.GetDirectoryName(settingsPath).Replace("\\", "/");

                if (String.IsNullOrEmpty(settings.guid))
                    settings.guid = AssetDatabase.AssetPathToGUID(settingsPath);
            }

            if (!IndexerFactory.TryGetValue(settings.type, out var indexerType))
                throw new ArgumentException($"{settings.type} indexer does not exist", nameof(settings.type));
            return (ObjectIndexer)Activator.CreateInstance(indexerType, new object[] {settings});
        }

        public void Import(string settingsPath)
        {
            settings = LoadSettings(settingsPath);
            index = CreateIndexer(settings);
            name = settings.name;

            var paths = index.GetDependencies();
            var guids = paths.Select(AssetDatabase.AssetPathToGUID).Where(g => !string.IsNullOrEmpty(g)).ToList();
            using (var importTask = new Task("Import", $"Importing {name} index", guids.Count, this))
            {
                var completed = 0;
                var artifacts = ProduceArtifacts(guids);
                ResolveArtifactPaths(artifacts, out var _, importTask, ref completed);
                timestamp = DateTime.Now.ToBinary();
            }
        }

        internal void OnEnable()
        {
            if (settings == null)
                return;

            var indexPath = AssetDatabase.GetAssetPath(this);
            index = CreateIndexer(settings, indexPath);

            if (settings.source == null)
                return;

            m_InstanceID = GetInstanceID();

            Log("OnEnable");

            if (bytes?.Length > 0)
            {
                EditorApplication.update -= Load;
                EditorApplication.update += Load;
            }
            else
            {
                EditorApplication.update -= Build;
                EditorApplication.update += Build;
            }
        }

        internal void OnDisable()
        {
            Log("OnDisable");
            AssetPostprocessorIndexer.contentRefreshed -= OnContentRefreshed;
        }

        [System.Diagnostics.Conditional("DEBUG_INDEXING")]
        private void Log(string callName, params string[] args)
        {
            Log(LogType.Log, callName, args);
        }

        [System.Diagnostics.Conditional("DEBUG_INDEXING")]
        private void Log(LogType logType, string callName, params string[] args)
        {
            if (!this || index == null || index.settings.options.disabled)
                return;
            var status = "";
            if (ready) status += "R";
            if (index != null && index.IsReady()) status += "I";
            if (loaded) status += "L";
            if (updating) status += "~";
            Debug.LogFormat(logType, LogOption.None, this, $"({m_InstanceID}, {status}) <b>{settings.name}</b>.<b>{callName}</b> {string.Join(", ", args)}" +
                $" {Utils.FormatBytes(bytes?.Length ?? 0)}, {index?.documentCount ?? 0} documents, {index?.indexCount ?? 0} elements");
        }

        public void Report(string status, params string[] args)
        {
            Log(status, args);
        }

        private void Load()
        {
            EditorApplication.update -= Load;

            if (!this)
                return;

            var loadTask = new Task("Load", $"Loading {name} index", (task, data) => Setup(), this);
            loadTask.RunThread(() =>
            {
                var step = 0;
                loadTask.Report($"Loading {bytes.Length} bytes...");
                if (!index.LoadBytes(bytes))
                    Debug.LogError($"Failed to load {name} index. Please re-import it.", this);

                loadTask.Report(++step, step+1);

                Dispatcher.Enqueue(() => loadTask.Resolve(new TaskData(bytes, index)));
            });
        }

        private bool ResolveArtifactPaths(IList<IndexArtifact> artifacts, out List<IndexArtifact> unresolvedArtifacts, Task task, ref int completed)
        {
            task.Report($"Resolving {artifacts.Count} artifacts...");

            var artifactIndexSuffix = $"{settings.type}.{settings.options.GetHashCode():X}.index".ToLowerInvariant();
            var indexImporterType = SearchIndexEntryImporter.GetIndexImporterType(settings.type, settings.options.GetHashCode());

            unresolvedArtifacts = new List<IndexArtifact>();
            foreach (var a in artifacts)
            {
                if (a == null || !string.IsNullOrEmpty(a.path) || string.IsNullOrWhiteSpace(a.guid))
                {
                    ++completed;
                    continue;
                }

                // Make sure the asset is still valid at this stage, otherwise ignore it
                // It could be a temporary file that was created since import.
                var invalidAsset = String.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(a.guid));
                if (invalidAsset)
                {
                    Debug.LogWarning($"Cannot resolve index artifact for {a.guid}");
                    continue;
                }

                if (!a.valid)
                {
                    a.key = ProduceArtifact(a.guid, indexImporterType, ImportMode.NoImport);
                    if (!a.valid)
                    {
                        unresolvedArtifacts.Add(a);
                        continue;
                    }
                }

                if (GetArtifactPaths(a.key, out var paths))
                {
                    a.path = paths.LastOrDefault(p => p.EndsWith("."+artifactIndexSuffix, StringComparison.Ordinal));
                    if (a.path == null)
                    {
                        Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, this,
                            $"Cannot find index artifact {artifactIndexSuffix} for {AssetDatabase.GUIDToAssetPath(a.guid)} ({a.guid})\n\t- {String.Join("\n\t- ", paths)}");
                    }
                    ++completed;
                }
            }

            task.Report(++completed);
            return unresolvedArtifacts.Count == 0;
        }

        private void ResolveArtifacts(string taskName, string title, Task.ResolveHandler finished)
        {
            m_CurrentTask?.Cancel();
            var resolveTask = new Task(taskName, title, finished, 1, this);

            m_CurrentTask = resolveTask;
            List<string> paths = null;
            resolveTask.RunThread(() =>
            {
                resolveTask.Report("Resolving GUIDs...");
                paths = index.GetDependencies();
            }, () =>
            {
                if (resolveTask?.Canceled() ?? false)
                    return;

                resolveTask.Report("Producing artifacts...");
                var guids = paths.Select(AssetDatabase.AssetPathToGUID).Where(g => !string.IsNullOrEmpty(g)).ToList();
                resolveTask.total = guids.Count;
                var artifacts = ProduceArtifacts(guids);

                if (resolveTask?.Canceled() ?? false)
                    return;

                ResolveArtifacts(artifacts, null, resolveTask);
            });
        }

        private bool ResolveArtifacts(IndexArtifact[] artifacts, IList<IndexArtifact> partialSet, Task task)
        {
            try
            {
                #if DEBUG_TIMERS
                using (new DebugTimer($"Resolving {partialSet?.Count ?? artifacts.Length} artifacts for {settings.source}"))
                #endif
                {
                    partialSet = partialSet ?? artifacts;

                    if (task.Canceled())
                        return false;

                    int completed = artifacts.Length - partialSet.Count;
                    if (ResolveArtifactPaths(partialSet, out var remainingArtifacts, task, ref completed))
                    {
                        if (task.Canceled())
                            return false;
                        return task.RunThread(() => CombineIndexes(settings, artifacts, task));
                    }

                    // Retry
                    EditorApplication.delayCall += () => ResolveArtifacts(artifacts, remainingArtifacts, task);
                }
            }
            catch (Exception err)
            {
                task.Resolve(err);
            }

            return false;
        }

        private byte[] CombineIndexes(Settings settings, IndexArtifact[] artifacts, Task task)
        {
            var completed = 0;
            var combineIndexer = new SearchIndexer();
            var indexName = settings.name.ToLowerInvariant();

            task.Report("Combining indexes...");

            combineIndexer.Start();
            foreach (var a in artifacts)
            {
                if (task.Canceled())
                    return null;

                task.Report(completed++);

                if (a == null || a.path == null)
                    continue;

                var si = new SearchIndexer();
                if (!si.ReadIndexFromDisk(a.path))
                    continue;

                if (task.Canceled())
                    return null;

                combineIndexer.CombineIndexes(si, baseScore: settings.baseScore,
                    (di, indexer) => indexer.AddProperty("a", indexName, di, saveKeyword: true, exact: true));
            }

            if (task.Canceled())
                return null;

            task.Report($"Sorting {combineIndexer.indexCount} indexes...");

            if (task.async)
            {
                combineIndexer.Finish((bytes) => task.Resolve(new TaskData(bytes, combineIndexer)), null, saveBytes: true);
            }
            else
            {
                combineIndexer.Finish(removedDocuments: null);
                return combineIndexer.SaveBytes();
            }

            return null;
        }

        private void Build()
        {
            EditorApplication.update -= Build;

            if (!this)
                return;

            ResolveArtifacts("Build", $"Building {name} index", OnArtifactsResolved);
        }

        private void Setup()
        {
            loaded = true;
            Log("Setup");
            indexLoaded?.Invoke(this);
            AssetPostprocessorIndexer.contentRefreshed -= OnContentRefreshed;
            AssetPostprocessorIndexer.contentRefreshed += OnContentRefreshed;
        }

        private void OnContentRefreshed(string[] updated, string[] removed, string[] moved)
        {
            if (!this || settings.options.disabled)
                return;
            var changeset = new AssetIndexChangeSet(updated, removed, moved, p => HasDocumentChanged(p));
            if (changeset.empty)
                return;
            IncrementalUpdate(changeset);
        }

        private bool HasDocumentChanged(string path, ChangeStatus check = ChangeStatus.All)
        {
            if (index.SkipEntry(path, true))
                return false;

            if (HasChanges(path, check))
                return true;

            return false;
        }

        private bool HasChanges(string path, ChangeStatus check = ChangeStatus.All)
        {
            if (check.HasFlag(ChangeStatus.Deleted) && !File.Exists(path))
                return true;

            if (!index.TryGetHash(path, out var hash) && check.HasFlag(ChangeStatus.Missing))
                return true;

            if (check.HasFlag(ChangeStatus.Changed))
            {
                var changeHash = index.GetDocumentHash(path);
                if (hash.isValid && changeHash != hash)
                    return true;
            }

            return false;
        }

        private void IncrementalUpdate(AssetIndexChangeSet changeset)
        {
            if (changeset.updated.Length > 0) Log("Change", string.Join(", ", changeset.updated));
            if (changeset.removed.Length > 0) Log("Remove", string.Join(", ", changeset.removed));

            var baseScore = settings.baseScore;
            var indexName = settings.name.ToLowerInvariant();
            var updates = changeset.updated.Select(p => AssetDatabase.AssetPathToGUID(p)).Where(g => !string.IsNullOrEmpty(g)).ToList();

            ++m_UpdateTasks;
            ResolveArtifacts(ProduceArtifacts(updates), null, new Task("Update", $"Updating {name} index ({updates.Count})", (Task task, TaskData data) =>
            {
                if (task.canceled || task.error != null)
                {
                    if (task.error != null)
                        Debug.LogException(task.error);
                    --m_UpdateTasks;
                    return;
                }

                byte[] mergedBytes = null;
                task.Report("Merging changes to index...");
                task.RunThread(() =>
                {
                    index.Merge(changeset.removed, data.combinedIndex, baseScore, (di, indexer) => indexer.AddProperty("a", indexName, di, true, true));
                    mergedBytes = index.SaveBytes();
                }, () =>
                {
                    --m_UpdateTasks;
                    bytes = mergedBytes;
                    timestamp = DateTime.Now.ToBinary();
                    Setup();
                });
            }, updates.Count, this));
        }

        private void OnArtifactsResolved(Task task, TaskData data)
        {
            m_CurrentTask = null;
            if (task.canceled || task.error != null)
                return;
            index.ApplyFrom(data.combinedIndex);
            bytes = data.bytes;
            Setup();
        }

        private IndexArtifact[] ProduceArtifacts(IList<string> guids)
        {
            #if DEBUG_TIMERS
            using (new DebugTimer($"Producing {guids.Count} artifacts for {settings.source}"))
            #endif
            {
                var artifactCount = guids.Count;
                var artifacts = new IndexArtifact[artifactCount];
                var indexImporterType = SearchIndexEntryImporter.GetIndexImporterType(settings.type, settings.options.GetHashCode());

                #if UNITY_2020_2_OR_NEWER
                var artifactIds = AssetDatabaseExperimental.ProduceArtifactsAsync(guids.Select(g => new GUID(g)).ToArray(), indexImporterType);
                for (int i = 0; i < artifactIds.Length; ++i)
                    artifacts[i] = new IndexArtifact(guids[i], artifactIds[i].value);
                #else
                for (int i = 0; i < artifactCount; ++i)
                {
                    if (String.IsNullOrEmpty(guids[i]))
                        continue;
                    artifacts[i] = new IndexArtifact(guids[i], ProduceArtifact(guids[i], indexImporterType, ImportMode.Asynchronous));
                }
                #endif

                return artifacts;
            }
        }

        private static Hash128 ProduceArtifact(GUID guid, Type importerType, ImportMode mode)
        {
            switch (mode)
            {
                #if UNITY_2020_2_OR_NEWER
                case ImportMode.Asynchronous:
                    return AssetDatabaseExperimental.ProduceArtifactAsync(new ArtifactKey(guid, importerType)).value;
                case ImportMode.Synchronous:
                    return AssetDatabaseExperimental.ProduceArtifact(new ArtifactKey(guid, importerType)).value;
                case ImportMode.NoImport:
                    return AssetDatabaseExperimental.LookupArtifact(new ArtifactKey(guid, importerType)).value;
                #else
                case ImportMode.Asynchronous:
                    return AssetDatabaseExperimental.GetArtifactHash(guid.ToString(), importerType, AssetDatabaseExperimental.ImportSyncMode.Queue);
                case ImportMode.Synchronous:
                    return AssetDatabaseExperimental.GetArtifactHash(guid.ToString(), importerType);
                case ImportMode.NoImport:
                    return AssetDatabaseExperimental.GetArtifactHash(guid.ToString(), importerType, AssetDatabaseExperimental.ImportSyncMode.Poll);
                #endif
            }

            return default;
        }

        private static Hash128 ProduceArtifact(string guid, Type importerType, ImportMode mode)
        {
            return ProduceArtifact(new GUID(guid), importerType, mode);
        }

        private static bool GetArtifactPaths(Hash128 artifactHash, out string[] paths)
        {
            #if UNITY_2020_2_OR_NEWER
            return AssetDatabaseExperimental.GetArtifactPaths(new ArtifactID { value = artifactHash }, out paths);
            #else
            return AssetDatabaseExperimental.GetArtifactPaths(artifactHash, out paths);
            #endif
        }
    }
}

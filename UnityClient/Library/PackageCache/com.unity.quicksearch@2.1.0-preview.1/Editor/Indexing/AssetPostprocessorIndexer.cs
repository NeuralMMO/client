using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Experimental;

namespace Unity.QuickSearch
{
    readonly struct AssetIndexChangeSet
    {
        public readonly string[] updated;
        public readonly string[] removed;

        public AssetIndexChangeSet(string[] updated, string[] removed)
        {
            this.removed = removed;
            this.updated = updated;
        }

        public AssetIndexChangeSet(IEnumerable<string> updated, IEnumerable<string> removed, IEnumerable<string> moved, Func<string, bool> predicate)
        {
            this.removed = removed.Where(predicate).ToArray();
            this.updated = updated.Concat(moved).Distinct().Where(predicate).ToArray();
        }

        public bool empty => updated?.Length == 0 && removed?.Length == 0;
        public IEnumerable<string> all => updated.Concat(removed).Distinct();
    }

    class AssetPostprocessorIndexer : AssetPostprocessor
    {
        private static bool s_Enabled;
        private static double s_BatchStartTime;

        private static readonly HashSet<string> s_UpdatedItems = new HashSet<string>();
        private static readonly HashSet<string> s_RemovedItems = new HashSet<string>();
        private static readonly HashSet<string> s_MovedItems = new HashSet<string>();

        const string k_TransactionDatabasePath = "Library/transactions.db";
        static readonly TransactionManager transactionManager;

        private static readonly object s_ContentRefreshedLock = new object();

        private static event Action<string[], string[], string[]> s_ContentRefreshed;
        public static event Action<string[], string[], string[]> contentRefreshed
        {
            add
            {
                lock (s_ContentRefreshedLock)
                {
                    Enable();
                    s_ContentRefreshed -= value;
                    s_ContentRefreshed += value;
                }
            }

            remove
            {
                lock (s_ContentRefreshedLock)
                {
                    s_ContentRefreshed -= value;
                    if (s_ContentRefreshed == null || s_ContentRefreshed.GetInvocationList().Length == 0)
                        Disable();
                }
            }
        }

        public static bool pending => s_UpdatedItems.Count > 0 || s_RemovedItems.Count > 0 || s_MovedItems.Count > 0;

        static AssetPostprocessorIndexer()
        {
            if (AssetDatabaseExperimental.IsAssetImportWorkerProcess())
                return;
            transactionManager = new TransactionManager(k_TransactionDatabasePath);
            transactionManager.Init();
            EditorApplication.quitting += OnQuitting;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        static void OnAfterAssemblyReload()
        {
            transactionManager.Init();
        }

        static void OnBeforeAssemblyReload()
        {
            transactionManager.Shutdown();
        }

        public static void Enable()
        {
            if (AssetDatabaseExperimental.IsAssetImportWorkerProcess())
                return;
            s_Enabled = true;
        }

        public static void Disable()
        {
            s_Enabled = false;
        }

        private static void OnQuitting()
        {
            transactionManager?.Shutdown();
            s_Enabled = false;
        }

        [UsedImplicitly]
        internal static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] movedTo, string[] movedFrom)
        {
            if (AssetDatabaseExperimental.IsAssetImportWorkerProcess())
                return;

            RaiseContentRefreshed(imported, deleted.Concat(movedFrom).Distinct().ToArray(), movedTo);
        }

        private static void RaiseContentRefreshed(IEnumerable<string> updated, IEnumerable<string> removed, IEnumerable<string> moved)
        {
            if (transactionManager.Initialized)
            {
                var transactions = updated.Select(path => new Transaction(AssetDatabase.AssetPathToGUID(path), AssetModification.Updated))
                    .Concat(removed.Select(path => new Transaction(AssetDatabase.AssetPathToGUID(path), AssetModification.Removed)))
                    .Concat(moved.Select(path => new Transaction(AssetDatabase.AssetPathToGUID(path), AssetModification.Moved)));
                transactionManager.Write(transactions);
            }

            if (!s_Enabled)
                return;

            s_UpdatedItems.UnionWith(updated);
            s_RemovedItems.UnionWith(removed);
            s_MovedItems.UnionWith(moved);

            if (s_UpdatedItems.Count > 0 || s_RemovedItems.Count > 0 || s_MovedItems.Count > 0)
            {
                s_BatchStartTime = EditorApplication.timeSinceStartup;
                EditorApplication.delayCall -= RaiseContentRefreshed;
                EditorApplication.delayCall += RaiseContentRefreshed;
            }
        }

        private static void RaiseContentRefreshed()
        {
            var currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - s_BatchStartTime > 0.5)
            {
                if (s_UpdatedItems.Count == 0 && s_RemovedItems.Count == 0 && s_MovedItems.Count == 0)
                    return;

                s_ContentRefreshed?.Invoke(s_UpdatedItems.ToArray(), s_RemovedItems.ToArray(), s_MovedItems.ToArray());
                s_UpdatedItems.Clear();
                s_RemovedItems.Clear();
                s_MovedItems.Clear();
            }
            else
            {
                EditorApplication.delayCall -= RaiseContentRefreshed;
                EditorApplication.delayCall += RaiseContentRefreshed;
            }
        }
    }
}

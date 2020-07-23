using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Unity.QuickSearch
{
    class DebugInfo
    {
        private readonly StringBuilder m_StatsBuilder = new StringBuilder();

        private static DebugInfo s_InitialStats;
        private static DebugInfo s_CurrentStats;

        private static ISearchView searchView;
        private static SearchContext searchContext;

        // Scan leaks
        private static UnityEngine.Object[] s_Objects = null;

        public static int repaintCount;
        public static int refreshCount;
        public static long gcFetch;
        public static long gcDraw;

        public int objectCount;
        public int textureCount;
        public long gcTotalMemory;
        public long totalMemory;

        public DebugInfo()
        {
            Update();
        }

        private void Update()
        {
            objectCount = Resources.FindObjectsOfTypeAll<UnityEngine.Object>().Length;
            textureCount = Resources.FindObjectsOfTypeAll<Texture>().Length;
            gcTotalMemory = GC.GetTotalMemory(false);
            totalMemory = Profiler.GetTotalAllocatedMemoryLong();
        }

        public string ToString(DebugInfo d)
        {
            if (d == null)
                return "No initial stats";

            m_StatsBuilder.Clear();
            m_StatsBuilder.Append($"<b>#</b> {SearchProvider.sessionCounter}  ");
            m_StatsBuilder.Append($"<b>Repaint</b>: {repaintCount} ({EditorUtility.FormatBytes(gcDraw)})  ");
            m_StatsBuilder.Append($"<b>Refresh</b>: {refreshCount} ({EditorUtility.FormatBytes(gcFetch)})  ");
            m_StatsBuilder.Append($"<b>Mem.</b>: {DiffBytes(totalMemory, d.totalMemory)}  ");
            m_StatsBuilder.Append($"<b>Objects</b>: {objectCount}/{d.objectCount} ({Diff(objectCount, d.objectCount)})  ");
            m_StatsBuilder.Append($"<b>Textures</b>: {textureCount}/{d.textureCount} ({Diff(textureCount, d.textureCount)})  ");

            return m_StatsBuilder.ToString();
        }

        private static string Diff(int now, int initial)
        {
            if (now >= initial)
                return $"+{now-initial}";
            return $"{now-initial}";
        }

        private static string DiffBytes(long now, long initial)
        {
            var diff = EditorUtility.FormatBytes(Math.Abs(now - initial));
            if (now >= initial)
                return $"+{diff}";
            return $"-{diff}";
        }

        private static void Refresh()
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();

            s_InitialStats = new DebugInfo();

            repaintCount = 0;
            refreshCount = 0;
            gcDraw = 0;
            gcFetch = 0;
        }

        public static void Enable(ISearchView view)
        {
            if (!SearchSettings.debug)
                return;

            searchView = view;
            searchContext = view.context;

            Refresh();
        }

        public static void Disable()
        {
            searchView = null;
            searchContext = null;
        }

        public static void Draw()
        {
            if (!SearchSettings.debug || searchContext == null)
                return;

            Debug.Assert(searchContext != null && searchView != null);

            if (s_CurrentStats == null)
                s_CurrentStats = new DebugInfo();
            s_CurrentStats.Update();

            using (new GUILayout.HorizontalScope(Styles.debugToolbar))
            {
                GUILayout.Label(s_CurrentStats.ToString(s_InitialStats), Styles.debugLabel);

                GUILayout.FlexibleSpace();

                string elapsedTimeString;
                if (searchContext.searchInProgress)
                    elapsedTimeString = "<i>" + Math.Round(searchContext.searchElapsedTime) + " ms</i>";
                else
                    elapsedTimeString = "<b>" + Math.Round(searchContext.searchElapsedTime) + " ms</b>";
                if (GUILayout.Button(elapsedTimeString, Styles.debugToolbarButton))
                    searchView.Refresh();

                if (GUILayout.Button($"Scan ({s_Objects?.Length ?? 0})", Styles.debugToolbarButton))
                {
                    Scan();
                }

                if (GUILayout.Button("Refresh", Styles.debugToolbarButton))
                {
                    Refresh();
                    searchView.Refresh();
                }
            }
        }

        private static void Scan()
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();

            var previousObjects = s_Objects;
            s_Objects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();

            if (previousObjects != null)
            {
                var ids = new HashSet<int>(previousObjects.Select(o => o.GetInstanceID()));
                var leaks = new List<UnityEngine.Object>();
                foreach (var o in s_Objects)
                {
                    if (!ids.Contains(o.GetInstanceID()))
                        leaks.Add(o);
                }

                if (leaks.Count == 0)
                {
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "No leak!");
                }
                else
                {
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"Found {leaks.Count} for {s_Objects.Length} objects");

                    foreach (var leak in leaks)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(leak);
                        if (leak is Texture tex)
                            Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, tex, $"({leak.GetInstanceID()}) {leak.name} [{leak.GetType().Name}] <{assetPath}>");
                        else
                            Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, leak, $"({leak.GetInstanceID()}) {leak.name} [{leak.GetType().Name}] <{assetPath}>");
                    }
                }

                GC.Collect();
                Resources.UnloadUnusedAssets();
                s_Objects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
            }
        }
    }
}

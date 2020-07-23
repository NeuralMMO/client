using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if !UNITY_2020_1_OR_NEWER
namespace Unity.QuickSearch
{
    static class Progress
    {
        public enum Status
        {
            Invalid = -1,
            Running = 0,
            Succeeded,
            Failed,
            Canceled,
            Paused
        }

        [Flags]
        public enum Options
        {
            None = 0 << 0,
            Sticky = 1 << 0,
            Indefinite = 1 << 1,
            Synchronous = 1 << 2,
            Managed = 1 << 3,
            Unmanaged = 1 << 4
        }

        private class ProgressItem
        {
            public int id;
            public string title;
            public string description;
            public float progress;
            public Func<bool> cancelCallback;
        }

        private static int s_NextProgressId = 0;
        private static readonly Dictionary<int, ProgressItem> s_ProgressItems = new Dictionary<int, ProgressItem>();
        private static readonly GUIContent s_NoStatus = new GUIContent();
        private static readonly GUIContent s_CurrentStatus = new GUIContent();

        public static bool Any()
        {
            lock (s_ProgressItems)
                return s_ProgressItems.Count != 0;
        }

        public static GUIContent Current()
        {
            lock (s_ProgressItems)
            {
                if (s_ProgressItems.Count == 0)
                    return s_NoStatus;
                var item = s_ProgressItems.ElementAt((int)EditorApplication.timeSinceStartup % s_ProgressItems.Count).Value;
                s_CurrentStatus.text = s_ProgressItems.Count > 1 ? $"({s_ProgressItems.Count}) " : "";
                s_CurrentStatus.text += $"{item.title} ({item.progress * 100.0f,2:0.0}%)";
                s_CurrentStatus.tooltip = item.description;
                return s_CurrentStatus;
            }
        }

        public static int Start(string title)
        {
            lock (s_ProgressItems)
            {
                var progressId = s_NextProgressId++;
                var progressItem = new ProgressItem() { id = progressId, title = title };
                s_ProgressItems[progressId] = progressItem;
                return progressId;
            }
        }

        public static void RegisterCancelCallback(int progressId, Func<bool> cancelCallback)
        {
            lock (s_ProgressItems)
            {
                if (s_ProgressItems.TryGetValue(progressId, out var item))
                    item.cancelCallback = cancelCallback;
            }
        }

        public static void SetDescription(int progressId, string description)
        {
            lock (s_ProgressItems)
            {
                if (s_ProgressItems.TryGetValue(progressId, out var item))
                    item.description = description;
            }
        }

        public static bool Exists(int progressId)
        {
            lock (s_ProgressItems)
                return s_ProgressItems.ContainsKey(progressId);
        }

        public static void Finish(int progressId, Status status)
        {
            lock (s_ProgressItems)
            {
                if (s_ProgressItems.TryGetValue(progressId, out var item))
                {
                    if (status == Status.Failed)
                        Debug.LogError(item.description);
                    s_ProgressItems.Remove(progressId);
                }
            }
        }

        public static Status GetStatus(int progressId)
        {
            lock (s_ProgressItems)
            {
                if (s_ProgressItems.TryGetValue(progressId, out var item))
                    return Status.Running;
                return Status.Invalid;
            }
        }

        public static void Remove(int progressId)
        {
            lock (s_ProgressItems)
                s_ProgressItems.Remove(progressId);
        }

        public static void Report(int progressId, float progress)
        {
            lock (s_ProgressItems)
            {
                if (s_ProgressItems.TryGetValue(progressId, out var item))
                    item.progress = progress;
            }
        }
    }
}
#endif

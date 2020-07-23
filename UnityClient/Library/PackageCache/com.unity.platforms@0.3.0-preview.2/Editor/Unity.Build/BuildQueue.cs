using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Build
{
    internal class BuildQueue : ScriptableSingleton<BuildQueue>
    {
        [Serializable]
        internal class OnAllBuildsCompletedEvent : UnityEvent<BuildResult[]>
        {
        }

        [Serializable]
        internal class QueuedBuild
        {
            [SerializeField]
            internal int sortingIndex;

            [SerializeField]
            internal string buildConfigurationGuid;

            [SerializeField]
            internal bool buildFinished;

            [SerializeField]
            internal string buildPipelineResult;
        }

        /// <summary>
        /// Sort builds, so less active target switching would occur.
        /// Build targets matching NoTarget (for ex., Dots) or active target, will come first. The others will follow
        /// </summary>
        internal class BuildStorter
        {
            readonly BuildTarget m_CurrentActiveTarget;

            internal BuildStorter(BuildTarget currentActiveTarget)
            {
                m_CurrentActiveTarget = currentActiveTarget;
            }
            internal int Compare(QueuedBuild x, QueuedBuild y)
            {
                if (x.sortingIndex == y.sortingIndex)
                    return 0;

                if (x.sortingIndex == (int)m_CurrentActiveTarget || x.sortingIndex == (int)BuildTarget.NoTarget)
                    return -1;
                if (y.sortingIndex == (int)m_CurrentActiveTarget || y.sortingIndex == (int)BuildTarget.NoTarget)
                    return 1;

                return x.sortingIndex.CompareTo(y.sortingIndex);
            }
        }

        [SerializeField]
        List<QueuedBuild> m_QueueBuilds;

        [SerializeField]
        BuildTarget m_OriginalBuildTarget;

        [SerializeField]
        OnAllBuildsCompletedEvent m_OnAllBuildsCompletedEvent;

        List<QueuedBuild> m_PrepareQueueBuilds;

        public void OnEnable()
        {
            if (m_QueueBuilds == null)
                m_QueueBuilds = new List<QueuedBuild>();

            if (m_QueueBuilds.Count == 0)
            {
                Clear();
                return;
            }

            // Can't start builds right away, because BuildConfiguration might not be loaded yet, meaning OnEnable in HierarchicalComponentContainer might not be called yet
            EditorApplication.delayCall += ProcessBuilds;
        }

        public void Clear()
        {
            m_OriginalBuildTarget = BuildTarget.NoTarget;
            if (m_PrepareQueueBuilds != null)
                m_PrepareQueueBuilds.Clear();
            if (m_QueueBuilds != null)
                m_QueueBuilds.Clear();
            m_OnAllBuildsCompletedEvent = null;
        }

        private static BuildConfiguration ToBuildConfiguration(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                throw new Exception("No valid build configuration provided");

            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<BuildConfiguration>(path);
        }

        public void QueueBuild(BuildConfiguration config, BuildResult buildResult)
        {
            if (m_PrepareQueueBuilds == null)
                m_PrepareQueueBuilds = new List<QueuedBuild>();

            if (config == null)
                throw new ArgumentNullException(nameof(config));
            var b = new QueuedBuild();
            b.sortingIndex = config.GetComponent<IBuildPipelineComponent>().SortingIndex;
            b.buildConfigurationGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(config));

            if (m_QueueBuilds.Count > 0)
                buildResult = BuildResult.Failure(config.GetBuildPipeline(), config, "Can't queue builds while executing build.");

            // If the build failed in previous step, don't execute it
            if (buildResult != null && buildResult.Failed)
                b.buildFinished = true;
            else
                b.buildFinished = false;

            b.buildPipelineResult = buildResult != null ? JsonSerialization.ToJson(buildResult, new JsonSerializationParameters { DisableRootAdapters = true, SerializedType = typeof(BuildResult) }) : string.Empty;

            m_PrepareQueueBuilds.Add(b);
        }

        public void FlushBuilds(UnityAction<BuildResult[]> onAllBuildsCompleted)
        {
            if (m_PrepareQueueBuilds == null || m_PrepareQueueBuilds.Count == 0)
                return;
            m_QueueBuilds.Clear();
            m_QueueBuilds.AddRange(m_PrepareQueueBuilds);
            m_PrepareQueueBuilds = null;

            m_OriginalBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            m_OnAllBuildsCompletedEvent = new OnAllBuildsCompletedEvent();
            if (onAllBuildsCompleted != null)
            {
                UnityEventTools.AddPersistentListener(m_OnAllBuildsCompletedEvent, onAllBuildsCompleted);
                m_OnAllBuildsCompletedEvent.SetPersistentListenerState(m_OnAllBuildsCompletedEvent.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime);
            }
            var sorter = new BuildStorter(m_OriginalBuildTarget);
            m_QueueBuilds.Sort(sorter.Compare);
            ProcessBuilds();
        }

        private QueuedBuild GetNextUnfinishedBuild()
        {
            foreach (var b in m_QueueBuilds)
            {
                if (!b.buildFinished)
                    return b;
            }
            return null;
        }

        private void ProcessBuilds()
        {
            EditorApplication.delayCall -= ProcessBuilds;

            if (m_OriginalBuildTarget <= 0)
            {
                var invalidTarget = m_OriginalBuildTarget;
                Clear();
                throw new Exception($"Original build target is invalid: {invalidTarget}");
            }

            // Editor is compiling, wait until other frame
            if (EditorApplication.isCompiling)
            {
                EditorApplication.delayCall += ProcessBuilds;
                return;
            }

            QueuedBuild currentBuild = GetNextUnfinishedBuild();

            while (currentBuild != null)
            {
                var t = (BuildTarget)currentBuild.sortingIndex;
                var b = ToBuildConfiguration(currentBuild.buildConfigurationGuid);

                if (t == BuildTarget.NoTarget || t == EditorUserBuildSettings.activeBuildTarget)
                {
                    currentBuild.buildPipelineResult = JsonSerialization.ToJson(b.Build(), new JsonSerializationParameters { DisableRootAdapters = true, SerializedType = typeof(BuildResult) });
                    currentBuild.buildFinished = true;
                }
                else
                {
                    try
                    {
                        if (b.GetComponent<IBuildPipelineComponent>().SetupEnvironment())
                        {
                            // Show dialog before actual build dialog, this way it's clear what's happening
                            EditorUtility.DisplayProgressBar("Hold on...", $"Switching to {t}", 0.0f);
                            return;
                        }
                    }
                    catch
                    {
                        m_QueueBuilds.Clear();
                        throw;
                    }

                }

                currentBuild = GetNextUnfinishedBuild();
            }


            // No more builds to run?
            if (currentBuild == null)
            {
                // We're done
                if (m_OriginalBuildTarget == EditorUserBuildSettings.activeBuildTarget)
                {
                    EditorUtility.ClearProgressBar();
                    m_OnAllBuildsCompletedEvent.Invoke(m_QueueBuilds.Select(m =>
                    {
                        var buildResult = TypeConstruction.Construct<BuildResult>();
                        JsonSerialization.TryFromJsonOverride(m.buildPipelineResult, ref buildResult, out _, new JsonSerializationParameters { DisableRootAdapters = true, SerializedType = typeof(BuildResult) });
                        return buildResult;
                    }).ToArray());
                    Clear();
                }
                else
                {
                    EditorUtility.DisplayProgressBar("Hold on...", $"Switching to original build target {m_OriginalBuildTarget}", 0.0f);
                    // Restore original build target
                    EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(m_OriginalBuildTarget), m_OriginalBuildTarget);
                }
                return;
            }
        }
    }
}

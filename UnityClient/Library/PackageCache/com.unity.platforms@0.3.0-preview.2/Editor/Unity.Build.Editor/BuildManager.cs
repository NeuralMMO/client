using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Editor
{
    [Serializable]
    internal class BuildInstructions
    {
        [SerializeField]
        bool m_Build;
        [SerializeField]
        bool m_Run;
        [SerializeField]
        GUID m_BuildConfigurationGuid;

        internal bool Build
        {
            get => m_Build;
            set => m_Build = value;
        }

        internal bool Run
        {
            get => m_Run;
            set => m_Run = value;
        }

        internal BuildConfiguration BuildConfiguration
        {
            get => BuildConfiguration.LoadAsset(m_BuildConfigurationGuid);
            set => m_BuildConfigurationGuid = new GUID(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value)));
        }

        internal bool Valid
        {
            get => BuildConfiguration.LoadAsset(m_BuildConfigurationGuid) != null;
        }
    }

    [Serializable]
    internal class BuildManager : EditorWindow
    {
        static readonly string kSettingsPath = "UserSettings/BuildManagerSettings.asset";

        [SerializeField]
        private BuildManagerTreeState m_TreeState;
        private BuildManagerTreeView m_TreeView;
        [SerializeField]
        private List<BuildInstructions> m_BuildInstructions;


        [MenuItem("Window/Build/Manager")]
        static void Init()
        {
            BuildManager window = (BuildManager)EditorWindow.GetWindow(typeof(BuildManager));
            window.titleContent = new GUIContent("Build Manager");
            window.Show();
        }

        private void OnEnable()
        {
            if (File.Exists(kSettingsPath))
            {
                EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(kSettingsPath), this);
            }

            if (m_BuildInstructions == null)
            {
                m_BuildInstructions = new List<BuildInstructions>();
            }

            // Remove configs which are no longer in the project
            m_BuildInstructions = new List<BuildInstructions>(m_BuildInstructions.Where(m => m.Valid));

            m_TreeState = BuildManagerTreeState.CreateOrInitializeTreeState(m_TreeState);
            m_TreeView = new BuildManagerTreeView(m_TreeState, RegenerateBuildItems);
        }

        private void OnDisable()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(kSettingsPath));
            File.WriteAllText(kSettingsPath, EditorJsonUtility.ToJson(this));
        }

        private BuildInstructions GetOrCreateBuildConfigurationProperties(BuildConfiguration config)
        {
            var props = m_BuildInstructions.FirstOrDefault(m => m.BuildConfiguration == config);
            if (props != null)
                return props;
            props = new BuildInstructions() { BuildConfiguration = config, Build = true, Run = true };
            m_BuildInstructions.Add(props);
            return props;
        }

        private void DeleteBuildConfigurationProperties(BuildConfiguration config)
        {
            for (int i = 0; i < m_BuildInstructions.Count; i++)
            {
                if (m_BuildInstructions[i].BuildConfiguration == config)
                {
                    m_BuildInstructions.RemoveAt(i);
                    return;
                }
            }
        }

        private void RefreshProperties()
        {
            var paths = AssetDatabase.FindAssets($"t:{typeof(BuildConfiguration).FullName}");
            var allSettings = paths.Select(p => (BuildConfiguration)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(p), typeof(BuildConfiguration))).ToArray();
            foreach (var s in allSettings)
            {
                if (s.GetBuildPipeline() == null)
                {
                    DeleteBuildConfigurationProperties(s);
                    continue;
                }
                GetOrCreateBuildConfigurationProperties(s);
            }
        }

        List<BuildTreeViewItem> RegenerateBuildItems()
        {
            RefreshProperties();
            var settings = new List<BuildTreeViewItem>();
            foreach (var p in m_BuildInstructions)
            {
                settings.Add(new BuildTreeViewItem(0, p));
            }

            return settings;
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            var rc = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniLabel, GUILayout.ExpandHeight(true), GUILayout.MinWidth(450));
            m_TreeView.OnGUI(rc);
            if (GUILayout.Button("Batch Build"))
            {
                BuildPipelineBase.BuildAsync(new BuildBatchDescription()
                {
                    BuildItems = m_BuildInstructions.Where(m => m.Build).Select(m => new BuildBatchItem() { BuildConfiguration = m.BuildConfiguration }).ToArray(),
                    OnBuildCompleted = OnBuildCompleted
                });
            }
            GUILayout.EndHorizontal();
        }

        void OnBuildCompleted(BuildResult[] results)
        {
            foreach (var r in results)
            {
                var props = GetOrCreateBuildConfigurationProperties(r.BuildConfiguration);
                if (props.Run)
                {
                    var runResult = r.BuildConfiguration.Run();
                    if (runResult.Failed)
                        Debug.LogError(runResult.Message);
                }
            }
        }
    }
}

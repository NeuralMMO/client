using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Build.Editor
{
    [Serializable]
    internal class BuildManagerTreeState
    {
        internal enum Columns
        {
            BuildConfiguration,
            Build,
            Run
        }

        [SerializeField]
        internal TreeViewState treeViewState;

        [SerializeField]
        internal MultiColumnHeaderState columnHeaderState;

        [NonSerialized]
        internal MultiColumnHeader columnHeader;

        internal static BuildManagerTreeState CreateOrInitializeTreeState(BuildManagerTreeState state)
        {
            if (state == null)
                state = new BuildManagerTreeState();

            if (state.treeViewState == null)
                state.treeViewState = new TreeViewState();

            bool firstInit = state.columnHeaderState == null;
            var headerState = CreateMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(state.columnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(state.columnHeaderState, headerState);
            state.columnHeaderState = headerState;

            state.columnHeader = new MultiColumnHeader(headerState);
            if (firstInit)
                state.columnHeader.ResizeToFit();

            return state;
        }

        private static MultiColumnHeaderState CreateMultiColumnHeaderState()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Build Settings"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 200,
                    minWidth = 200,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Build"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 100,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Run"),
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 100,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false
                }
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(Columns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var header = new MultiColumnHeaderState(columns);
            return header;
        }
    }

    internal class BuildTreeViewItem : TreeViewItem
    {
        readonly BuildInstructions m_BuildInstructions;

        internal BuildInstructions BuildInstructions { get { return m_BuildInstructions; } }
        internal BuildTreeViewItem(int depth) : base(-1, depth)
        {

        }

        internal BuildTreeViewItem(int depth, BuildInstructions buildInstructions) : base(buildInstructions.BuildConfiguration.GetInstanceID(), depth)
        {
            m_BuildInstructions = buildInstructions;
        }
    }

    internal class BuildManagerTreeView : TreeView
    {
        internal static class Styles
        {
            internal static GUIStyle buildConfigurationButton = new GUIStyle(EditorStyles.miniButton) { alignment = TextAnchor.MiddleLeft };
        }

        readonly Func<List<BuildTreeViewItem>> m_DataCallback;

        private readonly Dictionary<int, BuildTreeViewItem> m_CachedRowMap = new Dictionary<int, BuildTreeViewItem>();

        public BuildManagerTreeView(BuildManagerTreeState state, Func<List<BuildTreeViewItem>> getData)
            : base(state.treeViewState, state.columnHeader)
        {
            m_DataCallback = getData;

            rowHeight = 20f;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (rowHeight - EditorGUIUtility.singleLineHeight) * 0.5f;
            extraSpaceBeforeIconAndLabel = 18f;

            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem(0, -1);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var tempRoot = new BuildTreeViewItem(-1);

            var data = m_DataCallback();
            foreach (var d in data)
            {
                tempRoot.AddChild(d);
            }

            var items = new List<TreeViewItem>();
            AddChildrenRecursive(tempRoot, -1, items);

            SetupParentsAndChildrenFromDepths(root, items);
            return items;
        }

        void AddChildrenRecursive(TreeViewItem parent, int depth, IList<TreeViewItem> newRows)
        {
            if (parent == null || !parent.hasChildren)
                return;
            foreach (BuildTreeViewItem child in parent.children)
            {
                var item = new BuildTreeViewItem(child.depth, child.BuildInstructions);
                newRows.Add(child);

                if (child.hasChildren)
                {
                    if (IsExpanded(child.id))
                    {
                        AddChildrenRecursive(child, depth + 1, newRows);
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (BuildTreeViewItem)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                Rect rc = args.GetCellRect(i);
                if (i == columnIndexForTreeFoldouts)
                {
                    var indent = GetContentIndent(item);
                    rc.x += indent;
                    rc.width -= indent;
                }
                DisplayItem(item, i, rc);
            }
        }

        private void DisplayItem(BuildTreeViewItem item, int c, Rect rc)
        {
            BuildManagerTreeState.Columns column = (BuildManagerTreeState.Columns)c;

            var props = item.BuildInstructions;
            EditorGUI.BeginChangeCheck();
            switch (column)
            {
                case BuildManagerTreeState.Columns.BuildConfiguration:
                    var pipeline = props.BuildConfiguration.GetBuildPipeline();
                    if (GUI.Button(rc, props.BuildConfiguration.name + " with " + pipeline.GetType().Name, Styles.buildConfigurationButton))
                    {
                        EditorGUIUtility.PingObject(props.BuildConfiguration);
                    }

                    break;
                case BuildManagerTreeState.Columns.Build:
                    props.Build = EditorGUI.Toggle(rc, props.Build);
                    break;
                case BuildManagerTreeState.Columns.Run:
                    props.Run = EditorGUI.Toggle(rc, props.Run);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
                this.SetSelection(new List<int>(new[] { props.BuildConfiguration.GetInstanceID() }));
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void ExpandedStateChanged()
        {
            // When an asset is expanded while being selected also select it's children:
            var toSelect = new HashSet<int>();
            var selectedRows = GetSelection();

            foreach (var i in selectedRows)
            {
                foreach (var row in GetRows())
                {
                    if (row.id == i)
                    {
                        if (row.children != null && row.children.Count > 0)
                        {
                            var children = row.children.Where(c => c != null).Select(c => c.id).ToArray();
                            if (children != null)
                                toSelect.UnionWith(children);
                        }
                    }
                }
            }
            toSelect.UnionWith(selectedRows);
            SetSelection(toSelect.ToList());
        }

        public BuildTreeViewItem this[int key]
        {
            get
            {
                if (!m_CachedRowMap.ContainsKey(key))
                {
                    var row = GetRows().ToList().Find(r => r.id == key);
                    if (row != null)
                        m_CachedRowMap[key] = (BuildTreeViewItem)row;
                }
                return m_CachedRowMap[key];
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var toSelect = new HashSet<int>();

            foreach (var i in selectedIds)
            {
                foreach (var row in GetRows())
                {
                    if (row.id == i)
                    {
                        if (row.parent.depth >= 0)
                        {
                            toSelect.Add(row.parent.id);
                            toSelect.UnionWith(row.parent.children.Select(c => c.id).ToArray());
                        }
                    }
                }
            }

            toSelect.UnionWith(selectedIds);
            selectedIds = toSelect.ToList();

            //Select all children of all currently selected root elements
            var selectedChildren = new HashSet<int>(selectedIds);
            foreach (var i in selectedIds)
            {
                foreach (var row in GetRows())
                {
                    if (row.id == i)
                    {
                        if (row.children != null && row.children.Count > 0)
                        {
                            var children = row.children.Where(c => c != null).Select(c => c.id).ToArray();
                            if (children != null)
                                selectedChildren.UnionWith(children);
                        }
                    }
                }
            }
            SetSelection(selectedChildren.ToList());
        }
    }
}

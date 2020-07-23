using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unity.QuickSearch
{
    class StringListView : TreeView
    {
        private string[] m_Models;
        private string m_InitialValue;

        public event Action<int> elementActivated;

        static class Styles
        {
            public static readonly GUIStyle label = new GUIStyle(EditorStyles.label)
            {
                richText = true
            };
        }

        public StringListView(string initialValue, string[] models, TreeViewState treeViewState = null)
            : base(treeViewState ?? new TreeViewState())
        {
            m_Models = models;
            m_InitialValue = initialValue;
            showAlternatingRowBackgrounds = true;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();
            var selectionIds = new List<int>();
            for (var i = 0; i < m_Models.Length; i++)
            {
                if (m_Models[i] == m_InitialValue)
                    selectionIds.Add(i + 1);
                allItems.Add(new TreeViewItem { id = i + 1, depth = 0, displayName = m_Models[i]});
            }
            SetupParentsAndChildrenFromDepths(root, allItems);
            EditorApplication.delayCall += () => 
            {
                SetSelection(selectionIds);
                if (selectionIds.Count > 0)
                    FrameItem(selectionIds.Last());
                Repaint();
            };
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            GUI.Label(args.rowRect, args.item.displayName, Styles.label);
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            elementActivated?.Invoke(item.id - 1);
        }

        protected override void KeyEvent()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                var selection = GetSelection();
                if (selection.Count == 0)
                    return;

                var item = FindItem(selection[0], rootItem);
                elementActivated?.Invoke(item.id - 1);
            }
        }

        public bool IsFirstItemSelected()
        {
            var selection = GetSelection();
            if (selection.Count == 0)
                return false;

            var allRows = GetRows();
            if (allRows.Count == 0)
                return false;
            var selectedItems = FindRows(selection);
            return allRows[0] == selectedItems[0];
        }
    }
}
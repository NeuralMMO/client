using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unity.QuickSearch
{
    class CustomItemListView : TreeView
    {
        private IList m_Models;
        private Action<TreeView, TreeViewItem, object, string, Rect, int, bool, bool> m_DrawRow;

        public event Action<int> elementActivated;
        public Func<TreeView, TreeViewItem, object, string, bool> doesItemMatch;

        public CustomItemListView(IList models, float rowHeight, Action<TreeView, TreeViewItem, object, string, Rect, int, bool, bool> drawRow, TreeViewState treeViewState = null)
            : base(treeViewState ?? new TreeViewState())
        {
            m_Models = models;
            m_DrawRow = drawRow;
            showAlternatingRowBackgrounds = true;
            if (rowHeight > 0)
                this.rowHeight = rowHeight;

            Reload();
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
        }

        protected override void SingleClickedItem(int id)
        {
            elementActivated?.Invoke(id - 1);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (m_DrawRow != null)
            {
                m_DrawRow(this, args.item, m_Models[args.item.id - 1], args.label, args.rowRect, args.row, args.selected, args.focused);
            }
            else
            {
                base.RowGUI(args);
            }
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (doesItemMatch != null)
                return doesItemMatch.Invoke(this, item, m_Models[item.id - 1], search);
            return item.displayName != null && base.DoesItemMatchSearch(item, search);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();
            for (var i = 0; i < m_Models.Count; i++)
            {
                allItems.Add(new TreeViewItem { id = i + 1, depth = 0, displayName = m_Models[i].ToString()});
            }
            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
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
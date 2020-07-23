using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Editor.Bridge;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Unity.Entities.Editor
{
    class EntityHierarchyTreeView : VisualElement, IDisposable
    {
        const int k_ItemHeight = 20;

        readonly List<ITreeViewItem> m_RootItems = new List<ITreeViewItem>();

        TreeView m_TreeView;
        EntitySelectionProxy m_SelectionProxy;
        IEntityHierarchyGroupingStrategy m_Strategy;

        public EntityHierarchyTreeView()
        {
            style.flexGrow = 1.0f;

            CreateTreeView();
            CreateEntitySelectionProxy();
        }

        public void Dispose()
        {
            if (m_SelectionProxy != null)
                UnityObject.DestroyImmediate(m_SelectionProxy);
        }

        public void Refresh(IEntityHierarchyGroupingStrategy strategy)
        {
            if (m_Strategy == strategy)
                return;

            m_Strategy = strategy;

            RecreateRootItems();

            EntityHierarchyPool.ReturnAllVisualElements(this);
            m_TreeView.Refresh();
        }

        void RecreateRootItems()
        {
            foreach (var child in m_RootItems)
                ((IPoolable)child).ReturnToPool();

            m_RootItems.Clear();

            if (m_Strategy == null)
                return;

            using (var rootNodes = m_Strategy.GetChildren(EntityHierarchyNodeId.Root, Allocator.TempJob))
            {
                foreach (var node in rootNodes)
                    m_RootItems.Add(EntityHierarchyPool.GetTreeViewItem(null, node, m_Strategy));
            }
        }

        void CreateTreeView()
        {
            m_TreeView = new TreeView(m_RootItems, k_ItemHeight, OnMakeItem, OnBindItem)
            {
                Filter = OnFilter,
            };
            m_TreeView.onSelectionChange += OnSelectionChange;
            m_TreeView.style.flexGrow = 1.0f;

            Add(m_TreeView);
        }

        void CreateEntitySelectionProxy()
        {
            m_SelectionProxy = ScriptableObject.CreateInstance<EntitySelectionProxy>();
            m_SelectionProxy.hideFlags = HideFlags.HideAndDontSave;
            m_SelectionProxy.EntityControlSelectButton += OnSelectionChangedByInspector;
        }

        void OnSelectionChange(IEnumerable<ITreeViewItem> selection)
        {
            var selectedItem = (EntityHierarchyTreeViewItem)selection.FirstOrDefault();
            if (selectedItem == null)
                return;

            // TODO: Support undo/redo (see: Hierarchy window)

            if (selectedItem.NodeId.Kind == NodeKind.Entity)
            {
                EntityTreeNode entityNode;
                unsafe
                {
                    selectedItem.Strategy.GetNode(selectedItem.NodeId, &entityNode);
                }

                if (entityNode.Entity != Entity.Null)
                {
                    m_SelectionProxy.SetEntity(m_Strategy.World, entityNode.Entity);
                    Selection.activeObject = m_SelectionProxy;
                }
            }
            else
            {
                // TODO: Deal with non-Entity selections
                Selection.activeObject = null;
            }
        }

        void OnSelectionChangedByInspector(World world, Entity entity)
        {
            if (world != m_Strategy.World)
                return;

            m_TreeView.Select(new EntityHierarchyNodeId(NodeKind.Entity, entity.Index).GetHashCode());
        }

        VisualElement OnMakeItem() => EntityHierarchyPool.GetVisualElement(this);

        void OnBindItem(VisualElement element, ITreeViewItem item) => ((EntityHierarchyVisualElement)element).SetSource((EntityHierarchyTreeViewItem)item);

        bool OnFilter(ITreeViewItem item) => true;
    }
}

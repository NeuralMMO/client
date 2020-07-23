using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Editor.Bridge;
using UnityEditor;

namespace Unity.Entities.Editor
{
    class EntityHierarchyTreeViewItem : ITreeViewItem, IPoolable
    {
        static readonly string k_ChildrenListModificationExceptionMessage =
            L10n.Tr($"{nameof(EntityHierarchyTreeViewItem)} does not allow external modifications to its list of children.");

        readonly List<ITreeViewItem> m_Children = new List<ITreeViewItem>();
        bool m_ChildrenInitialized;
        ITreeViewItem m_Parent;

        public void Initialize(ITreeViewItem parentItem, EntityHierarchyNodeId nodeId, IEntityHierarchyGroupingStrategy strategy)
        {
            m_Parent = parentItem;
            NodeId = nodeId;
            Strategy = strategy;
        }

        public EntityHierarchyNodeId NodeId { get; private set; }
        public IEntityHierarchyGroupingStrategy Strategy { get; private set; }

        int ITreeViewItem.id => NodeId.GetHashCode();

        ITreeViewItem ITreeViewItem.parent => m_Parent;

        IEnumerable<ITreeViewItem> ITreeViewItem.children
        {
            get
            {
                if (!m_ChildrenInitialized)
                {
                    PopulateChildren();
                    m_ChildrenInitialized = true;
                }
                return m_Children;
            }
        }

        bool ITreeViewItem.hasChildren => Strategy.HasChildren(NodeId);

        void ITreeViewItem.AddChild(ITreeViewItem _) => throw new NotSupportedException(k_ChildrenListModificationExceptionMessage);

        void ITreeViewItem.AddChildren(IList<ITreeViewItem> _) => throw new NotSupportedException(k_ChildrenListModificationExceptionMessage);

        void ITreeViewItem.RemoveChild(ITreeViewItem _) => throw new NotSupportedException(k_ChildrenListModificationExceptionMessage);

        void IPoolable.Reset()
        {
            NodeId = default;

            Strategy = null;

            m_Parent = null;
            m_Children.Clear();
            m_ChildrenInitialized = false;
        }

        void IPoolable.ReturnToPool()
        {
            foreach (var child in m_Children)
                ((IPoolable)child).ReturnToPool();

            EntityHierarchyPool.ReturnTreeViewItem(this);
        }

        void PopulateChildren()
        {
            using (var childNodes = Strategy.GetChildren(NodeId, Allocator.TempJob))
            {
                foreach (var node in childNodes)
                {
                    var item = EntityHierarchyPool.GetTreeViewItem(this, node, Strategy);
                    m_Children.Add(item);
                }
            }
        }
    }
}

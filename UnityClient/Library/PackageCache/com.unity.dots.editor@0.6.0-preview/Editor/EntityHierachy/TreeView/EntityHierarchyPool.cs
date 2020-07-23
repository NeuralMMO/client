using System.Collections.Generic;
using Unity.Editor.Bridge;

namespace Unity.Entities.Editor
{
    static class EntityHierarchyPool
    {
        static readonly Dictionary<EntityHierarchyTreeView, HashSet<EntityHierarchyVisualElement>> k_PerTreeViewElements =
            new Dictionary<EntityHierarchyTreeView, HashSet<EntityHierarchyVisualElement>>();

        public static EntityHierarchyVisualElement GetVisualElement(EntityHierarchyTreeView treeView)
        {
            var item = Pool<EntityHierarchyVisualElement>.GetPooled();
            if (!k_PerTreeViewElements.TryGetValue(treeView, out var list))
                k_PerTreeViewElements[treeView] = list = new HashSet<EntityHierarchyVisualElement>();

            list.Add(item);
            item.Owner = treeView;
            return item;
        }

        public static void ReturnAllVisualElements(EntityHierarchyTreeView treeView)
        {
            if (!k_PerTreeViewElements.TryGetValue(treeView, out var list))
                return;

            foreach (var item in list)
                Pool<EntityHierarchyVisualElement>.Release(item);

            list.Clear();
        }

        public static void ReturnVisualElement(EntityHierarchyVisualElement item)
        {
            if (!k_PerTreeViewElements.TryGetValue(item.Owner, out var list))
                return;

            if (list.Remove(item))
                Pool<EntityHierarchyVisualElement>.Release(item);
        }

        public static EntityHierarchyTreeViewItem GetTreeViewItem(ITreeViewItem parent, EntityHierarchyNodeId nodeId, IEntityHierarchyGroupingStrategy strategy)
        {
            var item = Pool<EntityHierarchyTreeViewItem>.GetPooled();
            item.Initialize(parent, nodeId, strategy);
            return item;
        }

        public static void ReturnTreeViewItem(EntityHierarchyTreeViewItem item)
        {
            Pool<EntityHierarchyTreeViewItem>.Release(item);
        }
    }
}

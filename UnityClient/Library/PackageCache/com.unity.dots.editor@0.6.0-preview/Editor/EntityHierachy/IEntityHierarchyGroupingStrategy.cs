using JetBrains.Annotations;
using System;
using Unity.Collections;

namespace Unity.Entities.Editor
{
    interface IEntityHierarchyGroupingStrategy : IDisposable
    {
        World World { get; }

        ComponentType[] ComponentsToWatch { get; }

        void ApplyEntityChanges(NativeArray<Entity> newEntities, NativeArray<Entity> removedEntities);
        void ApplyComponentDataChanges(in ComponentDataDiffer.ComponentChanges componentChanges);
        void ApplySharedComponentDataChanges(in SharedComponentDataDiffer.ComponentChanges componentChanges);

        bool HasChildren(in EntityHierarchyNodeId nodeId);

        NativeArray<EntityHierarchyNodeId> GetChildren(in EntityHierarchyNodeId nodeId, Allocator allocator);

        bool Exists(in EntityHierarchyNodeId nodeId);

        unsafe void GetNode<T>(in EntityHierarchyNodeId nodeId, T* node) where T : unmanaged;

        uint GetNodeVersion(in EntityHierarchyNodeId nodeId);

        string GetNodeName(in EntityHierarchyNodeId nodeId);
    }

    struct EntityTreeNode
    {
        [UsedImplicitly]
        public Entity Entity;
    }

    struct VirtualTreeNode
    {
        [UsedImplicitly]
        public Hash128 SceneId;
    }

    // Stub
    struct EntityHierarchyDifferResult {}
}

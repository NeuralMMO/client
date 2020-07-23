using System;
using Unity.Collections;
using Unity.Transforms;

namespace Unity.Entities.Editor
{
    class EntityHierarchyDefaultGroupingStrategy : IEntityHierarchyGroupingStrategy
    {
        public EntityHierarchyDefaultGroupingStrategy(World world)
            => World = world;

        public World World { get; }

        public void Dispose()
        {
            // TODO: Implement
        }

        public ComponentType[] ComponentsToWatch { get; } = { typeof(Parent) };

        public void ApplyEntityChanges(NativeArray<Entity> newEntities, NativeArray<Entity> removedEntities)
        {
        }

        public void ApplyComponentDataChanges(in ComponentDataDiffer.ComponentChanges componentChanges)
        {
        }

        public void ApplySharedComponentDataChanges(in SharedComponentDataDiffer.ComponentChanges componentChanges)
        {
        }

        public bool HasChildren(in EntityHierarchyNodeId nodeId)
        {
            // TODO: Implement
            return false;
        }

        public NativeArray<EntityHierarchyNodeId> GetChildren(in EntityHierarchyNodeId nodeId, Allocator allocator)
        {
            // TODO: Implement
            return new NativeArray<EntityHierarchyNodeId>(0, allocator);
        }

        public bool Exists(in EntityHierarchyNodeId nodeId)
        {
            // TODO: Implement
            return false;
        }

        public unsafe void GetNode<T>(in EntityHierarchyNodeId nodeId, T* node) where T : unmanaged
        {
            // TODO: Implement
        }

        public uint GetNodeVersion(in EntityHierarchyNodeId nodeId)
        {
            // TODO: Implement
            return 0;
        }

        public unsafe string GetNodeName(in EntityHierarchyNodeId nodeId)
        {
            EntityTreeNode entityNode;
            GetNode(nodeId, &entityNode);
            var label = World.EntityManager.GetName(entityNode.Entity);
            return string.IsNullOrEmpty(label) ? entityNode.Entity.ToString() : label;
        }
    }
}

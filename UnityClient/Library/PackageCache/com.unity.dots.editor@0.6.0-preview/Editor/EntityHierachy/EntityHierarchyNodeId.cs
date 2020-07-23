using System;
using System.Globalization;

namespace Unity.Entities.Editor
{
    readonly struct EntityHierarchyNodeId : IEquatable<EntityHierarchyNodeId>
    {
        public readonly NodeKind Kind;
        public readonly int Id;

        public EntityHierarchyNodeId(NodeKind kind, int id)
            => (Kind, Id) = (kind, id);

        public static readonly EntityHierarchyNodeId Root = new EntityHierarchyNodeId(NodeKind.Root, 0);

        public bool Equals(EntityHierarchyNodeId other)
            => Kind == other.Kind && Id == other.Id;

        public override int GetHashCode()
        {
            unchecked
            {
                return (((byte)Kind).GetHashCode() * 397) ^ Id;
            }
        }

        public override string ToString() => Equals(EntityHierarchyNodeId.Root) ? "Root" : $"{Kind} - {Id.ToString(NumberFormatInfo.InvariantInfo)}";
    }

    enum NodeKind : byte
    {
        None = 0,
        Root = 1,
        Entity = 2,
        Scene = 3,
    }
}

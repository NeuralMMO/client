using Unity.Entities;
using UnityEngine;

namespace Unity.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    [ExecuteAlways]
    public class StructuralChangePresentationSystemGroup : ComponentSystemGroup
    {
    }
}

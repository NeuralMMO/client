using Unity.Entities;
using UnityEngine;

namespace Unity.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(StructuralChangePresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    [ExecuteAlways]
    public class UpdatePresentationSystemGroup : ComponentSystemGroup
    {
    }
}

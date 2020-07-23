using System;
using Unity.Entities;

namespace Unity.Transforms
{
    /// <summary>
    /// Copy Transform from GameObject associated with Entity to TransformMatrix.
    /// </summary>
    [WriteGroup(typeof(LocalToWorld))]
    public struct CopyTransformFromGameObject : IComponentData {}

    [UnityEngine.DisallowMultipleComponent]
    [Obsolete("CopyTransformFromGameObjectProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class CopyTransformFromGameObjectProxy : ComponentDataProxy<CopyTransformFromGameObject> {}
}

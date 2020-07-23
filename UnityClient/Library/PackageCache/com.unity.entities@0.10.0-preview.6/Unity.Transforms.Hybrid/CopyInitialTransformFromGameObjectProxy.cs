using System;
using Unity.Entities;

namespace Unity.Transforms
{
    /// <summary>
    /// Copy Transform from GameObject associated with Entity to TransformMatrix.
    /// Once only. Component is removed after copy.
    /// </summary>
    public struct CopyInitialTransformFromGameObject : IComponentData {}

    [UnityEngine.DisallowMultipleComponent]
    [Obsolete("CopyInitialTransformFromGameObjectProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class CopyInitialTransformFromGameObjectProxy : ComponentDataProxy<CopyInitialTransformFromGameObject> {}
}

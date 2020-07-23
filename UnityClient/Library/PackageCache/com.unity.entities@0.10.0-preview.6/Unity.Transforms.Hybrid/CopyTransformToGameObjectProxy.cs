using System;
using Unity.Entities;

namespace Unity.Transforms
{
    /// <summary>
    /// Copy Transform to GameObject associated with Entity from TransformMatrix.
    /// </summary>
    public struct CopyTransformToGameObject : IComponentData {}

    [UnityEngine.DisallowMultipleComponent]
    [Obsolete("CopyTransformToGameObjectProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class CopyTransformToGameObjectProxy : ComponentDataProxy<CopyTransformToGameObject> {}
}

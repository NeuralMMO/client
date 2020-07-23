using System;
using Unity.Entities;

namespace Unity.Transforms
{
    [UnityEngine.DisallowMultipleComponent]
    [UnityEngine.AddComponentMenu("DOTS/Deprecated/LocalToWorldProxy-Deprecated")]
    [Obsolete("LocalToWorldProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class LocalToWorldProxy : ComponentDataProxy<LocalToWorld>
    {
    }
}

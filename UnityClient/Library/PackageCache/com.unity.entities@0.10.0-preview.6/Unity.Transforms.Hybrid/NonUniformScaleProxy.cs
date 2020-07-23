using System;
using Unity.Entities;

namespace Unity.Transforms
{
    [UnityEngine.DisallowMultipleComponent]
    [UnityEngine.AddComponentMenu("DOTS/Deprecated/NonUniformScaleProxy-Deprecated")]
    [Obsolete("NonUniformScaleProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class NonUniformScaleProxy : ComponentDataProxy<NonUniformScale>
    {
    }
}

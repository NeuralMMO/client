using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Transforms
{
    [UnityEngine.DisallowMultipleComponent]
    [UnityEngine.AddComponentMenu("DOTS/Deprecated/Rotation-Deprecated")]
    [Obsolete("RotationProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class RotationProxy : ComponentDataProxy<Rotation>
    {
        protected override void ValidateSerializedData(ref Rotation serializedData)
        {
            serializedData.Value = math.normalizesafe(serializedData.Value);
        }
    }
}

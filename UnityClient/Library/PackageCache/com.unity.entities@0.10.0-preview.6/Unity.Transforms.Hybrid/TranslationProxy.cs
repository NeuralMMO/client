using System;
using Unity.Entities;

namespace Unity.Transforms
{
    [UnityEngine.DisallowMultipleComponent]
    [UnityEngine.AddComponentMenu("DOTS/Deprecated/Translation-Deprecated")]
    [Obsolete("TranslationProxy has been deprecated. Please use the new GameObject-to-entity conversion workflows instead. (RemovedAfter 2020-07-03).")]
    public class TranslationProxy : ComponentDataProxy<Translation>
    {
    }
}

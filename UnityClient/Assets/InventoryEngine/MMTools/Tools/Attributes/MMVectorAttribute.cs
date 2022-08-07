using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    public class MMVectorAttribute : PropertyAttribute
    {
        public readonly string[] Labels;

        public MMVectorAttribute(params string[] labels)
        {
            Labels = labels;
        }
    }
}

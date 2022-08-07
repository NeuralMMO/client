using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class MMConditionAttribute : PropertyAttribute
    {
        public string ConditionBoolean = "";
        public bool Hidden = false;

        public MMConditionAttribute(string conditionBoolean)
        {
            this.ConditionBoolean = conditionBoolean;
            this.Hidden = false;
        }

        public MMConditionAttribute(string conditionBoolean, bool hideInInspector)
        {
            this.ConditionBoolean = conditionBoolean;
            this.Hidden = hideInInspector;
        }

    }
}

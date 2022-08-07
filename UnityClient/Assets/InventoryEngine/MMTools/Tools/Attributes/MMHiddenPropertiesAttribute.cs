using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class MMHiddenPropertiesAttribute : Attribute
    {
        public string[] PropertiesNames;

        public MMHiddenPropertiesAttribute(params string[] propertiesNames)
        {
            PropertiesNames = propertiesNames;
        }
    }
}

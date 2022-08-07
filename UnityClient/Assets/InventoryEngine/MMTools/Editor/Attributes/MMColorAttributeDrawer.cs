using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    [CustomPropertyDrawer(typeof(MMColorAttribute))]
    public class MMColorAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Color color = (attribute as MMColorAttribute).color;
            Color prev = GUI.color;
            GUI.color = color;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.color = prev;
        }
    }
}

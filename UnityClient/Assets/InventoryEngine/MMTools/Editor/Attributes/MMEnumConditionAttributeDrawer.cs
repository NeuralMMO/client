using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    // original implementation by http://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5/
    [CustomPropertyDrawer(typeof(MMEnumConditionAttribute))]
    public class MMEnumConditionAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MMEnumConditionAttribute enumConditionAttribute = (MMEnumConditionAttribute)attribute;
            bool enabled = GetConditionAttributeResult(enumConditionAttribute, property);
            bool previouslyEnabled = GUI.enabled;
            GUI.enabled = enabled;
            if (!enumConditionAttribute.Hidden || enabled)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            GUI.enabled = previouslyEnabled;
        }

        private bool GetConditionAttributeResult(MMEnumConditionAttribute enumConditionAttribute, SerializedProperty property)
        {
            bool enabled = true;
            string propertyPath = property.propertyPath;
            string conditionPath = propertyPath.Replace(property.name, enumConditionAttribute.ConditionEnum);
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            if (sourcePropertyValue != null)
            {
                int currentEnum = sourcePropertyValue.enumValueIndex;

                enabled = enumConditionAttribute.ContainsBitFlag(currentEnum);
            }
            else
            {
                Debug.LogWarning("No matching boolean found for ConditionAttribute in object: " + enumConditionAttribute.ConditionEnum);
            }

            return enabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            MMEnumConditionAttribute enumConditionAttribute = (MMEnumConditionAttribute)attribute;
            bool enabled = GetConditionAttributeResult(enumConditionAttribute, property);

            if (!enumConditionAttribute.Hidden || enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }
}
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
    [CustomPropertyDrawer(typeof(MMConditionAttribute))]
    public class MMConditionAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MMConditionAttribute conditionAttribute = (MMConditionAttribute)attribute;
            bool enabled = GetConditionAttributeResult(conditionAttribute, property);
            bool previouslyEnabled = GUI.enabled;
            GUI.enabled = enabled;
            if (!conditionAttribute.Hidden || enabled)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            GUI.enabled = previouslyEnabled;
        }

        private bool GetConditionAttributeResult(MMConditionAttribute condHAtt, SerializedProperty property)
        {
            bool enabled = true;
            string propertyPath = property.propertyPath; 
            string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionBoolean); 
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            if (sourcePropertyValue != null)
            {
                enabled = sourcePropertyValue.boolValue;
            }
            else
            {
                Debug.LogWarning("No matching boolean found for ConditionAttribute in object: " + condHAtt.ConditionBoolean);
            }

            return enabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            MMConditionAttribute conditionAttribute = (MMConditionAttribute)attribute;
            bool enabled = GetConditionAttributeResult(conditionAttribute, property);

            if (!conditionAttribute.Hidden || enabled)
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

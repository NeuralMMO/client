using UnityEngine;
using UnityEditor;
 using System.Collections;

namespace MoreMountains.Tools
{
    [CustomPropertyDrawer(typeof(AIAction))]
    public class AIActionPropertyInspector : PropertyDrawer
    {
        const float LineHeight = 16f;

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            var height = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(prop));

            Rect position = rect;

            position.height = height;
            EditorGUI.PropertyField(position, prop); //, new GUIContent("Script"));
            position.y += height;

            AIAction @typedObject = prop.objectReferenceValue as AIAction;
            if (@typedObject != null && !string.IsNullOrEmpty(@typedObject.Label))
            {
                position.height = height;
                EditorGUI.LabelField(position, "Label", @typedObject.Label);
                position.y += height;
            }
            else
            {
                EditorGUIUtility.GetControlID(FocusType.Passive);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var h = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(property));
            float height = h;

            AIAction @typedObject = property.objectReferenceValue as AIAction;
            if (@typedObject != null && !string.IsNullOrEmpty(@typedObject.Label))
            {
                height += h;
            }
            return height;
        }
    }
}
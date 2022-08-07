using UnityEngine;
using UnityEditor;
 using System.Collections;

namespace MoreMountains.Tools
{
    [CustomPropertyDrawer(typeof(AITransition))]
    public class AITransitionPropertyInspector : PropertyDrawer
    {
        const float LineHeight = 16f;

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            Rect position = rect;
            foreach (SerializedProperty a in prop)
            {
                var height = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(a));
                position.height = height;

                if(a.name == "Decision")
                {
                    EditorGUI.PropertyField(position, a, new GUIContent(a.name));
                    position.y += height;

                    var @object = a.objectReferenceValue;
                    AIDecision @typedObject = @object as AIDecision;
                    if (@typedObject != null && !string.IsNullOrEmpty(@typedObject.Label))
                    {
                        EditorGUI.LabelField(position, "Label", @typedObject.Label);
                        position.y += height;
                    }
                    else
                    {
                        EditorGUIUtility.GetControlID(FocusType.Passive);
                    }
                }
                else
                {
                    EditorGUI.PropertyField(position, a, new GUIContent(a.name));
                    position.y += height;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            foreach (SerializedProperty a in property)
            {
                var h = Mathf.Max(LineHeight, EditorGUI.GetPropertyHeight(a));
                if(a.name == "Decision")
                {
                    height += h;

                    var @object = a.objectReferenceValue;
                    AIDecision @typedObject = @object as AIDecision;
                    if (@typedObject != null && !string.IsNullOrEmpty(@typedObject.Label))
                    {
                        height += h;
                    }
                }
                else
                {
                    height += h;
                }
            }
            return height;
        }
    }
}
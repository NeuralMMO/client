using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    [CustomPropertyDrawer(typeof(MMBackgroundColorAttribute))]
    public class MMBackgroundColorAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var backgroundColorAttribute = attribute as MMBackgroundColorAttribute;

            bool doHighlight = true;
            
            if (doHighlight)
            {
                var color = GetColor(backgroundColorAttribute.Color);
                var padding = EditorGUIUtility.standardVerticalSpacing;
                var highlightRect = new Rect(position.x - padding, position.y - padding,
                    position.width + (padding * 2), position.height + (padding * 2));
                EditorGUI.DrawRect(highlightRect, color);
                var cc = GUI.contentColor;
                GUI.contentColor = Color.black;
                EditorGUI.PropertyField(position, property, label);

                GUI.contentColor = cc;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private Color GetColor(MMBackgroundAttributeColor color)
        {
            switch (color)
            {
                case MMBackgroundAttributeColor.Red:
                    return new Color32(255, 0, 63, 255);
                case MMBackgroundAttributeColor.Pink:
                    return new Color32(255, 66, 160, 255);
                case MMBackgroundAttributeColor.Orange:
                    return new Color32(255, 128, 0, 255);
                case MMBackgroundAttributeColor.Yellow:
                    return new Color32(255, 211, 0, 255);
                case MMBackgroundAttributeColor.Green:
                    return new Color32(102, 255, 0, 255);
                case MMBackgroundAttributeColor.Blue:
                    return new Color32(0, 135, 189, 255);
                case MMBackgroundAttributeColor.Violet:
                    return new Color32(127, 0, 255, 255);
                default:
                    return Color.white;
            }
        }
    }
}

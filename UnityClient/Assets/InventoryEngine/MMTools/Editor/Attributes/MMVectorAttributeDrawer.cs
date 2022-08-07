using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    [CustomPropertyDrawer(typeof(MMVectorAttribute))]
    public class MMVectorLabelsAttributeDrawer : PropertyDrawer
    {
        protected static readonly GUIContent[] originalLabels = new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z"), new GUIContent("W") };
        protected const int padding = 375;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent guiContent)
        {
            int ratio = (padding > Screen.width) ? 2 : 1;
            return ratio * base.GetPropertyHeight(property, guiContent);
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent guiContent)
        {
            MMVectorAttribute vector = (MMVectorAttribute)attribute;
            
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                float[] fieldArray = new float[] { property.vector2Value.x, property.vector2Value.y };
                fieldArray = DrawFields(rect, fieldArray, ObjectNames.NicifyVariableName(property.name), EditorGUI.FloatField, vector);
                property.vector2Value = new Vector2(fieldArray[0], fieldArray[1]);
            }
            else if (property.propertyType == SerializedPropertyType.Vector3)
            {
                float[] fieldArray = new float[] { property.vector3Value.x, property.vector3Value.y, property.vector3Value.z };
                fieldArray = DrawFields(rect, fieldArray, ObjectNames.NicifyVariableName(property.name), EditorGUI.FloatField, vector);
                property.vector3Value = new Vector3(fieldArray[0], fieldArray[1], fieldArray[2]);
            }
            else if (property.propertyType == SerializedPropertyType.Vector4)
            {
                float[] fieldArray = new float[] { property.vector4Value.x, property.vector4Value.y, property.vector4Value.z, property.vector4Value.w };
                fieldArray = DrawFields(rect, fieldArray, ObjectNames.NicifyVariableName(property.name), EditorGUI.FloatField, vector);
                property.vector4Value = new Vector4(fieldArray[0], fieldArray[1], fieldArray[2]);
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                int[] fieldArray = new int[] { property.vector2IntValue.x, property.vector2IntValue.y };
                fieldArray = DrawFields(rect, fieldArray, ObjectNames.NicifyVariableName(property.name), EditorGUI.IntField, vector);
                property.vector2IntValue = new Vector2Int(fieldArray[0], fieldArray[1]);
            }
            else if (property.propertyType == SerializedPropertyType.Vector3Int)
            {
                int[] array = new int[] { property.vector3IntValue.x, property.vector3IntValue.y, property.vector3IntValue.z };
                array = DrawFields(rect, array, ObjectNames.NicifyVariableName(property.name), EditorGUI.IntField, vector);
                property.vector3IntValue = new Vector3Int(array[0], array[1], array[2]);
            }
        }

        protected T[] DrawFields<T>(Rect rect, T[] vector, string mainLabel, System.Func<Rect, GUIContent, T, T> fieldDrawer, MMVectorAttribute vectors)
        {
            T[] result = vector;

            bool shortSpace = (Screen.width < padding);

            Rect mainLabelRect = rect;
            mainLabelRect.width = EditorGUIUtility.labelWidth;
            if (shortSpace)
            {
                mainLabelRect.height *= 0.5f;
            }                

            Rect fieldRect = rect;
            if (shortSpace)
            {
                fieldRect.height *= 0.5f;
                fieldRect.y += fieldRect.height;
                fieldRect.width = rect.width / vector.Length;
            }
            else
            {
                fieldRect.x += mainLabelRect.width;
                fieldRect.width = (rect.width - mainLabelRect.width) / vector.Length;
            }

            EditorGUI.LabelField(mainLabelRect, mainLabel);

            for (int i = 0; i < vector.Length; i++)
            {
                GUIContent label = vectors.Labels.Length > i ? new GUIContent(vectors.Labels[i]) : originalLabels[i];
                Vector2 labelSize = EditorStyles.label.CalcSize(label);
                EditorGUIUtility.labelWidth = Mathf.Max(labelSize.x + 5, 0.3f * fieldRect.width);
                result[i] = fieldDrawer(fieldRect, label, vector[i]);
                fieldRect.x += fieldRect.width;
            }

            EditorGUIUtility.labelWidth = 0;
            return result;
        }
    }
}
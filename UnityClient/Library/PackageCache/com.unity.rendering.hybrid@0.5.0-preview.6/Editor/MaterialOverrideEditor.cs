using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(MaterialOverride))]
public class MaterialOverrideEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Font defaultFont = EditorStyles.label.font;

        serializedObject.Update();
        SerializedProperty assetProp = serializedObject.FindProperty("overrideAsset");
        EditorGUILayout.PropertyField(assetProp, new GUIContent("Override Asset"));

        MaterialOverride overrideComponent = (target as MaterialOverride);
        if (overrideComponent != null)
        {
            MaterialOverrideAsset overrideAsset = overrideComponent.overrideAsset;
            if (overrideAsset != null)
            {
                SerializedProperty overrideListProp = serializedObject.FindProperty("overrideList");
                for (int i = 0; i < overrideListProp.arraySize; i++)
                {
                    SerializedProperty overrideProp = overrideListProp.GetArrayElementAtIndex(i);
                    string displayName = overrideProp.FindPropertyRelative("displayName").stringValue;
                    ShaderPropertyType type = (ShaderPropertyType)overrideProp.FindPropertyRelative("type").intValue;
                    SerializedProperty instanceProp = overrideProp.FindPropertyRelative("instanceOverride");

                    Rect fieldRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                    GUI.skin.font = defaultFont;
                    if (instanceProp.boolValue)
                    {
                        DrawOverrideMargin(fieldRect);
                        GUI.skin.font = EditorStyles.boldFont;
                    }

                    EditorGUI.BeginChangeCheck();
                    if (type == ShaderPropertyType.Color)
                    {
                        SerializedProperty colorProp = overrideProp.FindPropertyRelative("value");

                        Color color = new Color(colorProp.vector4Value.x, colorProp.vector4Value.y, colorProp.vector4Value.z, colorProp.vector4Value.w);
                        Color newColor = EditorGUI.ColorField(fieldRect, new GUIContent(displayName), color);
                        Vector4 vec4 = new Vector4(newColor.r, newColor.g, newColor.b, newColor.a);
                        colorProp.vector4Value = vec4;
                    }
                    else if (type == ShaderPropertyType.Vector)
                    {
                        SerializedProperty vector4Prop = overrideProp.FindPropertyRelative("value");

                        Vector4 vec4 = vector4Prop.vector4Value;
                        Vector4 newVec4 = EditorGUI.Vector4Field(fieldRect, new GUIContent(displayName), vec4);
                        vector4Prop.vector4Value = newVec4;
                    }
                    else if (type == ShaderPropertyType.Float || type == ShaderPropertyType.Range)
                    {
                        SerializedProperty floatProp = overrideProp.FindPropertyRelative("value");

                        float f = floatProp.vector4Value.x;
                        float newF = EditorGUI.FloatField(fieldRect, new GUIContent(displayName), f);
                        floatProp.vector4Value = new Vector4(newF, 0.0f, 0.0f, 0.0f);
                    }
                    else
                    {
                        Debug.Log("Property " + displayName + " is of unsupported type " + type + " for material override.");
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        instanceProp.boolValue = true;
                    }


                    if (instanceProp.boolValue)
                    {
                        if (fieldRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ContextClick)
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Apply to MaterialOverride '" + overrideComponent.overrideAsset.name + "'"),
                                false, ApplyToOverrideAsset, i);
                            menu.AddItem(new GUIContent("Revert"), false, RevertGameobjectOverride, i);
                            menu.ShowAsContext();
                            Event.current.Use();
                        }
                    }
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    //TODO(andrew.theisen): can we avoid this SO update?
    private void ApplyToOverrideAsset(object index)
    {
        serializedObject.Update();

        int intIndex = (int)index;
        SerializedProperty overrideListProp = serializedObject.FindProperty("overrideList");
        SerializedProperty overrideProp = overrideListProp.GetArrayElementAtIndex(intIndex);
        SerializedProperty value = overrideProp.FindPropertyRelative("value");
        SerializedProperty instanceProp = overrideProp.FindPropertyRelative("instanceOverride");
        instanceProp.boolValue = false;

        serializedObject.ApplyModifiedProperties();

        MaterialOverride overrideComponent = (target as MaterialOverride);
        SerializedObject assetSerializedObj = new SerializedObject(overrideComponent.overrideAsset);
        assetSerializedObj.Update();
        SerializedProperty assetOverrideProp = assetSerializedObj.FindProperty("overrideList").GetArrayElementAtIndex(intIndex);
        assetOverrideProp.FindPropertyRelative("value").vector4Value = value.vector4Value;
        assetOverrideProp.FindPropertyRelative("instanceOverride").boolValue = false;
        assetSerializedObj.ApplyModifiedProperties();
    }

    //TODO(andrew.theisen): can we avoid this SO update?
    private void RevertGameobjectOverride(object index)
    {
        serializedObject.Update();

        SerializedProperty overrideListProp = serializedObject.FindProperty("overrideList");
        SerializedProperty overrideProp = overrideListProp.GetArrayElementAtIndex((int)index);
        SerializedProperty instanceProp = overrideProp.FindPropertyRelative("instanceOverride");
        instanceProp.boolValue = false;

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawOverrideMargin(Rect controlRect)
    {
        controlRect.yMin += 2;
        controlRect.yMax += 1;

        if (Event.current.type == EventType.Repaint)
        {
            Color oldColor = GUI.backgroundColor;
            bool oldEnabled = GUI.enabled;
            GUI.enabled = true;

            Color k_OverrideMarginColor = new Color(1f / 255f, 153f / 255f, 235f / 255f, 0.75f);

            GUI.backgroundColor = k_OverrideMarginColor;
            controlRect.x = 0;
            controlRect.width = 2;
            GUI.skin.GetStyle("OverrideMargin").Draw(controlRect, false, false, false, false);

            GUI.enabled = oldEnabled;
            GUI.backgroundColor = oldColor;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

internal class MaterialPropPopup : PopupWindowContent
{
    public bool propertyListChanged;

    private Vector2 _scrollViewVector;
    private MaterialOverrideAsset _overrideAsset;
    private SerializedObject _serializedObject;
    private List<string> _generatedScriptPaths;

    //TODO(andrew.theisen): we need a way to support float2 and float3. can't do this until we can get the DOTS property type or byte size
    private readonly ShaderPropertyType[] _supportedTypes =
    {
        ShaderPropertyType.Color,
        ShaderPropertyType.Vector,
        ShaderPropertyType.Float,
        ShaderPropertyType.Range,
    };

    public MaterialPropPopup(MaterialOverrideAsset overrideAsset, SerializedObject serializedObject)
    {
        _overrideAsset = overrideAsset;
        _serializedObject = serializedObject;
        _generatedScriptPaths = new List<string>();
        propertyListChanged = false;
    }

    public override void OnGUI(Rect rect)
    {
        _scrollViewVector = GUILayout.BeginScrollView(_scrollViewVector);
        if (_overrideAsset.material != null)
        {
            Shader shader = _overrideAsset.material.shader;
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                ShaderPropertyType propertyType = shader.GetPropertyType(i);
                if (_supportedTypes.Any(item => item == propertyType))
                {
                    string propertyName = shader.GetPropertyName(i);
                    string displayName = propertyName;

                    //TODO(andrew.theisen): review if this UI code is too coupled with behavior?
                    int index = _overrideAsset.overrideList.FindIndex(d => d.name == displayName);
                    bool overriden = index != -1;
                    bool toggle = GUILayout.Toggle(overriden, displayName);
                    if (overriden != toggle)
                    {
                        _serializedObject.Update();
                        SerializedProperty overrideListProp = _serializedObject.FindProperty("overrideList");
                        int arraySize = overrideListProp.arraySize;

                        string shaderName = AssetDatabase.GetAssetPath(shader);
                        if (toggle)
                        {
                            overrideListProp.InsertArrayElementAtIndex(arraySize);
                            SerializedProperty overrideProp = overrideListProp.GetArrayElementAtIndex(arraySize);
                            overrideProp.FindPropertyRelative("name").stringValue = propertyName;
                            overrideProp.FindPropertyRelative("displayName").stringValue = displayName;

                            overrideProp.FindPropertyRelative("shaderName").stringValue = shaderName;
                            overrideProp.FindPropertyRelative("materialName").stringValue = _overrideAsset.material.name;
                            overrideProp.FindPropertyRelative("type").intValue = (int)propertyType;
                            overrideProp.FindPropertyRelative("instanceOverride").boolValue = false;
                            if (propertyType == ShaderPropertyType.Vector || propertyType == ShaderPropertyType.Color)
                            {
                                overrideProp.FindPropertyRelative("value").vector4Value = _overrideAsset.material.GetVector(propertyName);
                            }
                            else if (propertyType == ShaderPropertyType.Float || propertyType == ShaderPropertyType.Range)
                            {
                                Vector4 vec4 = new Vector4(_overrideAsset.material.GetFloat(propertyName), 0.0f, 0.0f, 0.0f);
                                overrideProp.FindPropertyRelative("value").vector4Value = vec4;
                            }
                        }
                        else
                        {
                            overrideListProp.DeleteArrayElementAtIndex(index);
                        }
                        _serializedObject.ApplyModifiedProperties();

                        string scriptPath = GenerateIComponentData();
                        if (scriptPath != null)
                        {
                            _generatedScriptPaths.Add(scriptPath);
                        }

                        propertyListChanged = true;
                    }
                }
            }
        }

        GUILayout.EndScrollView();
    }

    private string GenerateIComponentData()
    {
        string filepath = null;
        string preamble =
@"using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
";
        for (int i = 0; i < _overrideAsset.overrideList.Count; i++)
        {
            MaterialOverrideAsset.OverrideData overrideData = _overrideAsset.overrideList[i];


            if (_overrideAsset.GetTypeFromAttrs(overrideData) != null)
            {
                continue;
            }

            string generatedStruct = "";

            string @fieldName = overrideData.name.Replace("_", ""); //TODO(andrew.theisen): properly sanitize type names to follow c# class name rules
            string @typeName = "";

            if (overrideData.type == ShaderPropertyType.Color || overrideData.type == ShaderPropertyType.Vector)
            {
                @typeName = "Vector4Override";
                generatedStruct =
                    $@"    [MaterialProperty(""{@overrideData.name}"", MaterialPropertyFormat.Float4)]
    struct {@fieldName}{@typeName} : IComponentData
    {{
        public float4 Value;
    }}
}}
";
            }
            else if (overrideData.type == ShaderPropertyType.Float || overrideData.type == ShaderPropertyType.Range)
            {
                @typeName = "FloatOverride";
                generatedStruct =
                    $@"    [MaterialProperty(""{@overrideData.name}"", MaterialPropertyFormat.Float)]
    struct {@fieldName}{@typeName} : IComponentData
    {{
        public float Value;
    }}
}}
";
            }

            if (generatedStruct != "")
            {
                //TODO(andrew.theisen): writeall text 260 char limit for filepath
                filepath = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(_overrideAsset.material)),
                    $@"{@fieldName}{@typeName}OverrideGenerated.cs");
                File.WriteAllText(filepath, preamble + generatedStruct);

                _overrideAsset.overrideList[i] = overrideData;
            }
        }

        return filepath;
    }

    public override void OnClose()
    {
        if (_generatedScriptPaths.Count != 0)
        {
            AssetDatabase.StartAssetEditing();
            foreach (var scriptPath in _generatedScriptPaths)
            {
                AssetDatabase.ImportAsset(scriptPath);
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
            //Type overrideType = GetTypeFromAttrs(overrideData); //TODO(andrew.theisen): how do we get type manager to update so we can get the type right away?
        }
    }
}

[CustomEditor(typeof(MaterialOverrideAsset))]
public class MaterialOverrideAssetEditor : Editor
{
    private Rect _buttonRect;
    private MaterialPropPopup _popupWindow;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MaterialOverrideAsset overrideAsset = (target as MaterialOverrideAsset);
        Material currentMat = overrideAsset.material;

        bool dirtyGameObjects = false;
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("material"), new GUIContent("Material"));

        SerializedProperty overrideListProp = serializedObject.FindProperty("overrideList");
        for (int i = 0; i < overrideListProp.arraySize; i++)
        {
            SerializedProperty overrideProp = overrideListProp.GetArrayElementAtIndex(i);
            string displayName = overrideProp.FindPropertyRelative("displayName").stringValue;
            ShaderPropertyType type = (ShaderPropertyType)overrideProp.FindPropertyRelative("type").intValue;

            if (type == ShaderPropertyType.Color)
            {
                SerializedProperty colorProp = overrideProp.FindPropertyRelative("value");
                Color color = new Color(colorProp.vector4Value.x, colorProp.vector4Value.y, colorProp.vector4Value.z, colorProp.vector4Value.w);
                Color newColor = EditorGUILayout.ColorField(new GUIContent(displayName), color);
                Vector4 vec4 = new Vector4(newColor.r, newColor.g, newColor.b, newColor.a);
                colorProp.vector4Value = vec4;
            }
            else if (type == ShaderPropertyType.Vector)
            {
                SerializedProperty vector4Prop = overrideProp.FindPropertyRelative("value");
                EditorGUILayout.PropertyField(vector4Prop, new GUIContent(displayName));
            }
            else if (type == ShaderPropertyType.Float || type == ShaderPropertyType.Range)
            {
                SerializedProperty floatProp = overrideProp.FindPropertyRelative("value");
                float f = floatProp.vector4Value.x;
                float newF = EditorGUILayout.FloatField(new GUIContent(displayName), f);
                floatProp.vector4Value = new Vector4(newF, 0.0f, 0.0f, 0.0f);
            }
            else
            {
                Debug.Log("Property " + displayName + " is of unsupported type " + type + " for material override.");
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            dirtyGameObjects = true;
        }

        string buttonTxt = overrideAsset.material == null ? "Select a Material" : "Add Property Overrride";

        if (overrideAsset.material == null)
        {
            GUI.enabled = false;
        }


        if (GUILayout.Button(buttonTxt))
        {
            if (Event.current.type == EventType.Repaint)
            {
                _buttonRect = GUILayoutUtility.GetLastRect();
            }
            _buttonRect.x = Event.current.mousePosition.x;
            _buttonRect.y = Event.current.mousePosition.y;
            _popupWindow = new MaterialPropPopup(overrideAsset, serializedObject);
            PopupWindow.Show(_buttonRect, _popupWindow);
        }
        GUI.enabled = true;

        if (_popupWindow != null)
        {
            dirtyGameObjects = _popupWindow.propertyListChanged ? true : dirtyGameObjects;
        }
        if (dirtyGameObjects)
        {
            foreach (var overrideComponent in FindObjectsOfType<MaterialOverride>())
            {
                if (overrideComponent.overrideAsset == target)
                {
                    EditorUtility.SetDirty(overrideComponent);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
        if (currentMat != overrideAsset.material)
        {
            overrideAsset.overrideList = new List<MaterialOverrideAsset.OverrideData>();
        }
    }
}

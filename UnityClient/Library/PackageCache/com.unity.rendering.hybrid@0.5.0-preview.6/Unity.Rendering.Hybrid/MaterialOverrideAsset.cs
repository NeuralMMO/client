using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequiresEntityConversion]
[CreateAssetMenu(fileName = "MaterialOverrideAsset", menuName = "Shader/Material Override Asset", order = 1)] //TODO(andrew.theisen): where should this live in the menu?
public class MaterialOverrideAsset : ScriptableObject
{
    [Serializable]
    public struct OverrideData
    {
        public string name;
        public string displayName;
        public string shaderName;
        public string materialName;
        public ShaderPropertyType type;
        public Vector4 value;
        public bool instanceOverride;
    }

    public List<OverrideData> overrideList = new List<OverrideData>();

    public Material material;


    public Type GetTypeFromAttrs(OverrideData overrideData)
    {
        Type overrideType = null;
        bool componentExists = false;
        foreach (var t in TypeManager.GetAllTypes())
        {
            if (t.Type != null)
            {
                //TODO(andrew.theisen): this grabs the first IComponentData that matches these attributes but multiple matches can exist such as URPMaterialPropertyBaseColor
                //                and HDRPMaterialPropertyBaseColor. It actually shouldn't matter which one is used can they can work either shader.
                foreach (var attr in t.Type.GetCustomAttributes(typeof(MaterialPropertyAttribute), false))
                {
                    var propAttr = (MaterialPropertyAttribute)attr;
                    MaterialPropertyFormat propFormat = 0;
                    //TODO(andrew.theisen): So this won't use exisiting IComponentDatas always. for example:
                    //                HDRPMaterialPropertyEmissiveColor is Float3, but the ShaderPropertyType
                    //                is Color but without alpha. can fix this when we can get the DOTS
                    //                type or byte size of the property
                    if (overrideData.type == ShaderPropertyType.Vector || overrideData.type == ShaderPropertyType.Color)
                    {
                        propFormat = MaterialPropertyFormat.Float4;
                    }
                    else if (overrideData.type == ShaderPropertyType.Float || overrideData.type == ShaderPropertyType.Range)
                    {
                        propFormat = MaterialPropertyFormat.Float;
                    }
                    else
                    {
                        break;
                    }

                    if (propAttr.Name == overrideData.name && propAttr.Format == propFormat)
                    {
                        overrideType = t.Type;
                        componentExists = true;
                        break;
                    }
                }
            }
            if (componentExists)
            {
                break;
            }
        }
        return overrideType;
    }

    public void OnValidate()
    {
        foreach (var overrideComponent in FindObjectsOfType<MaterialOverride>())
        {
            if (overrideComponent.overrideAsset == this)
            {
                if (material != null)
                {
                    var newList = new List<OverrideData>();
                    foreach (var overrideData in overrideList)
                    {
                        int index = overrideComponent.overrideList.FindIndex(d => d.name == overrideData.name);
                        if (index != -1)
                        {
                            if (overrideComponent.overrideList[index].instanceOverride)
                            {
                                newList.Add(overrideComponent.overrideList[index]);
                                continue;
                            }
                        }
                        newList.Add(overrideData);
                    }
                    overrideComponent.overrideList = newList;
                    overrideComponent.ApplyMaterialProperties();
                }
                else
                {
                    overrideComponent.overrideList = new List<OverrideData>();
                    overrideComponent.ClearOverrides();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

interface IHelper
{
    void AddComponentData(EntityManager dstManager, Entity entity, IComponentData iComponentData);
}

class Helper<T> : IHelper where T : struct, IComponentData
{
    public void AddComponentData(EntityManager dstManager, Entity entity, IComponentData iComponentData)
    {
        dstManager.AddComponentData(entity, (T)iComponentData);
    }
}

[DisallowMultipleComponent]
[RequiresEntityConversion]
[ExecuteInEditMode]
[ConverterVersion("joe", 1)]
public class MaterialOverride : MonoBehaviour
{
    public MaterialOverrideAsset overrideAsset;

    public List<MaterialOverrideAsset.OverrideData> overrideList = new List<MaterialOverrideAsset.OverrideData>();


    public void ApplyMaterialProperties()
    {
        if (overrideAsset != null)
        {
            if (overrideAsset.material != null)
            {
                //TODO(andrew.theisen): needs support for multiple renderers
                var renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.SetPropertyBlock(null);
                    var propertyBlock = new MaterialPropertyBlock();
                    foreach (var overrideData in overrideList)
                    {
                        if (overrideData.type == ShaderPropertyType.Color)
                        {
                            propertyBlock.SetColor(overrideData.name, overrideData.value);
                        }
                        else if (overrideData.type == ShaderPropertyType.Vector)
                        {
                            propertyBlock.SetVector(overrideData.name, overrideData.value);
                        }
                        else if (overrideData.type == ShaderPropertyType.Float || overrideData.type == ShaderPropertyType.Range)
                        {
                            propertyBlock.SetFloat(overrideData.name, overrideData.value.x);
                        }
                    }

                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }
    }

    public void OnValidate()
    {
        if (overrideAsset != null)
        {
            var newList = new List<MaterialOverrideAsset.OverrideData>();
            foreach (var overrideData in overrideAsset.overrideList)
            {
                int index = overrideList.FindIndex(d => d.name == overrideData.name);
                if (index != -1)
                {
                    if (overrideList[index].instanceOverride)
                    {
                        newList.Add(overrideList[index]);
                        continue;
                    }
                }
                newList.Add(overrideData);
            }
            overrideList = newList;
            ApplyMaterialProperties();
        }
        else
        {
            overrideList = new List<MaterialOverrideAsset.OverrideData>();
            ClearOverrides();
        }
    }

    public void ClearOverrides()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.SetPropertyBlock(null);
        }
    }

    public void OnDisable()
    {
        ClearOverrides();
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
public class MaterialOverrideSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((MaterialOverride uMaterialOverride) =>
        {
            if (uMaterialOverride.overrideAsset != null)
            {
                if (uMaterialOverride.overrideAsset.material != null)
                {
                    var entity = GetPrimaryEntity(uMaterialOverride);
                    foreach (var overrideData in uMaterialOverride.overrideList)
                    {
                        //TODO(andrew.theisen): this loops over types in TypeManager which is slow. But running GetTypes on generated IComponentData doesn't work
                        //                unless we find the correct assembly for it. This could be slow as we might need to loop through all assemblies to find it.
                        //                Another possible solution is to use asmdef so we know exactly where the generated IComponentData lives. Of course
                        //                this means more generation and forces an organization of these generated IComponentData structs
                        Type overrideType = uMaterialOverride.overrideAsset.GetTypeFromAttrs(overrideData);
                        if (overrideType != null)
                        {
                            //TODO(andrew.theisen): if the IComponentData doesn't exactly have the field Value to store it's data this will fail. We should warn at least
                            if (overrideData.type == ShaderPropertyType.Vector || overrideData.type == ShaderPropertyType.Color)
                            {
                                var component = (IComponentData)Activator.CreateInstance(overrideType);
                                System.Reflection.FieldInfo fInfo = overrideType.GetField("Value");
                                fInfo.SetValue(component,
                                    new float4(overrideData.value.x, overrideData.value.y, overrideData.value.z, overrideData.value.w));
                                var helperType = typeof(Helper<>).MakeGenericType(component.GetType());
                                var helper = (IHelper)Activator.CreateInstance(helperType);
                                helper.AddComponentData(DstEntityManager, entity, component);
                            }
                            else if (overrideData.type == ShaderPropertyType.Float || overrideData.type == ShaderPropertyType.Range)
                            {
                                var component = (IComponentData)Activator.CreateInstance(overrideType);
                                System.Reflection.FieldInfo fInfo = overrideType.GetField("Value");
                                fInfo.SetValue(component, overrideData.value.x);
                                var helperType = typeof(Helper<>).MakeGenericType(component.GetType());
                                var helper = (IHelper)Activator.CreateInstance(helperType);
                                helper.AddComponentData(DstEntityManager, entity, component);
                            }
                        }
                    }
                }
            }
        });
    }
}

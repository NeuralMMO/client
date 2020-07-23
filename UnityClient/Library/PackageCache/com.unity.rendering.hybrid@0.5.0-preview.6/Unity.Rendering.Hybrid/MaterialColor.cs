using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Rendering
{
    // NOTE: This is a code example material override component for setting RGBA color to hybrid renderer.
    //       You should implement your own material property override components inside your own project.

    [Serializable]
    [MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
    public struct MaterialColor : IComponentData
    {
        public float4 Value;
    }

    namespace Authoring
    {
        [DisallowMultipleComponent]
        [RequiresEntityConversion]
        [ConverterVersion("joe", 1)]
        public class MaterialColor : MonoBehaviour
        {
            public Color color;
        }

        [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
        public class MaterialColorSystem : GameObjectConversionSystem
        {
            protected override void OnUpdate()
            {
                Entities.ForEach((MaterialColor uMaterialColor) =>
                {
                    var entity = GetPrimaryEntity(uMaterialColor);
                    var data = new Unity.Rendering.MaterialColor { Value = new float4(uMaterialColor.color.r, uMaterialColor.color.g, uMaterialColor.color.b, uMaterialColor.color.a) };
                    DstEntityManager.AddComponentData(entity, data);
                });
            }
        }
    }
}

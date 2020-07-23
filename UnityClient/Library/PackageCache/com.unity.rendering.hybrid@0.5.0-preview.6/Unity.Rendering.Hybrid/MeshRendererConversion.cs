using System.Collections.Generic;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Unity.Rendering
{
    [ConverterVersion("unity", 7)]
    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    class MeshRendererConversion : GameObjectConversionSystem
    {
        const bool AttachToPrimaryEntityForSingleMaterial = true;

        protected override void OnUpdate()
        {
            var materials = new List<Material>(10);
            Entities.WithNone<TextMesh>().ForEach((MeshRenderer meshRenderer, MeshFilter meshFilter) =>
            {
                var entity = GetPrimaryEntity(meshRenderer);
                var mesh = meshFilter.sharedMesh;
                meshRenderer.GetSharedMaterials(materials);

                Convert(entity, DstEntityManager, this, meshRenderer, mesh, materials);
            });
        }

#if ENABLE_HYBRID_RENDERER_V2
        private static BuiltinMaterialPropertyUnity_MotionVectorsParams CreateMotionVectorsParams(ref RenderMesh mesh, ref Renderer meshRenderer)
        {
            float s_bias = -0.001f;
            float hasLastPositionStream = mesh.needMotionVectorPass ? 1.0f : 0.0f;
            var motionVectorGenerationMode = meshRenderer.motionVectorGenerationMode;
            float forceNoMotion = (motionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion) ? 0.0f : 1.0f;
            float cameraVelocity = (motionVectorGenerationMode == MotionVectorGenerationMode.Camera) ? 0.0f : 1.0f;
            return new BuiltinMaterialPropertyUnity_MotionVectorsParams { Value = new float4(hasLastPositionStream, forceNoMotion, s_bias, cameraVelocity) };
        }

#endif

        private static void AddComponentsToEntity(
            RenderMesh renderMesh,
            Entity entity,
            EntityManager dstEntityManager,
            GameObjectConversionSystem conversionSystem,
            Renderer meshRenderer,
            Mesh mesh,
            List<Material> materials,
            bool flipWinding,
            int id)
        {
            renderMesh.material = materials[id];
            renderMesh.subMesh = id;

            dstEntityManager.AddSharedComponentData(entity, renderMesh);

            dstEntityManager.AddComponentData(entity, new PerInstanceCullingTag());
            dstEntityManager.AddComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

            if (flipWinding)
                dstEntityManager.AddComponent(entity, ComponentType.ReadWrite<RenderMeshFlippedWindingTag>());

            conversionSystem.ConfigureEditorRenderData(entity, meshRenderer.gameObject, true);

#if ENABLE_HYBRID_RENDERER_V2
            dstEntityManager.AddComponent(entity, ComponentType.ReadOnly<WorldToLocal_Tag>());

#if HDRP_9_0_0_OR_NEWER
            // HDRP previous frame matrices (for motion vectors)
            if (renderMesh.needMotionVectorPass)
            {
                dstEntityManager.AddComponent(entity, ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_MatrixPreviousM>());
                dstEntityManager.AddComponent(entity, ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_MatrixPreviousMI_Tag>());
            }
            dstEntityManager.AddComponentData(entity, CreateMotionVectorsParams(ref renderMesh, ref meshRenderer));
#endif

            dstEntityManager.AddComponentData(entity, new BuiltinMaterialPropertyUnity_RenderingLayer
            {
                Value = new uint4(meshRenderer.renderingLayerMask, 0, 0, 0)
            });

            dstEntityManager.AddComponentData(entity, new BuiltinMaterialPropertyUnity_WorldTransformParams
            {
                Value = flipWinding ? new float4(0, 0, 0, -1) : new float4(0, 0, 0, 1)
            });

#if URP_9_0_0_OR_NEWER
            // Default initialized light data for URP
            dstEntityManager.AddComponentData(entity, new BuiltinMaterialPropertyUnity_LightData
            {
                Value = new float4(0, 0, 1, 0)
            });
#endif
#endif
        }

        public static void Convert(
            Entity entity,
            EntityManager dstEntityManager,
            GameObjectConversionSystem conversionSystem,
            Renderer meshRenderer,
            Mesh mesh,
            List<Material> materials)
        {
            var materialCount = materials.Count;

            // Don't add RenderMesh (and other required components) unless both mesh and material assigned.
            if ((mesh != null) && (materialCount > 0))
            {
                var renderMesh = new RenderMesh
                {
                    mesh = mesh,
                    castShadows = meshRenderer.shadowCastingMode,
                    receiveShadows = meshRenderer.receiveShadows,
                    layer = meshRenderer.gameObject.layer
                };

                renderMesh.needMotionVectorPass = (meshRenderer.motionVectorGenerationMode == MotionVectorGenerationMode.Object) ||
                    (meshRenderer.motionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion);

                //@TODO: Transform system should handle RenderMeshFlippedWindingTag automatically. This should not be the responsibility of the conversion system.
                float4x4 localToWorld = meshRenderer.transform.localToWorldMatrix;
                var flipWinding = math.determinant(localToWorld) < 0.0;

                if (materialCount == 1 && AttachToPrimaryEntityForSingleMaterial)
                {
                    AddComponentsToEntity(
                        renderMesh,
                        entity,
                        dstEntityManager,
                        conversionSystem,
                        meshRenderer,
                        mesh,
                        materials,
                        flipWinding,
                        0);
                }
                else
                {
                    for (var m = 0; m != materialCount; m++)
                    {
                        var meshEntity = conversionSystem.CreateAdditionalEntity(meshRenderer);

                        dstEntityManager.AddComponentData(meshEntity, new LocalToWorld { Value = localToWorld });
                        if (!dstEntityManager.HasComponent<Static>(meshEntity))
                        {
                            dstEntityManager.AddComponentData(meshEntity, new Parent { Value = entity });
                            dstEntityManager.AddComponentData(meshEntity, new LocalToParent { Value = float4x4.identity });
                        }

                        AddComponentsToEntity(
                            renderMesh,
                            meshEntity,
                            dstEntityManager,
                            conversionSystem,
                            meshRenderer,
                            mesh,
                            materials,
                            flipWinding,
                            m);
                    }
                }
            }
        }
    }
}

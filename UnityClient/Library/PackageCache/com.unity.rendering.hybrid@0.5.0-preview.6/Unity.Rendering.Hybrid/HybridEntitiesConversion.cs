using System.Collections.Generic;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
#if HDRP_7_0_0_OR_NEWER
using UnityEngine.Rendering.HighDefinition;
#endif
#if URP_7_0_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace Unity.Rendering
{
#if !TINY_0_22_0_OR_NEWER
    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class LightConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
#if HDRP_7_0_0_OR_NEWER || !HDRP_7_0_0_OR_NEWER
            Entities.ForEach((Light light) =>
            {
                AddHybridComponent(light);
            });
#endif

#if HDRP_7_0_0_OR_NEWER
            Entities.ForEach((HDAdditionalLightData light) =>
            {
                AddHybridComponent(light);
            });
#endif

#if URP_7_0_0_OR_NEWER
            Entities.ForEach((UniversalAdditionalLightData vfx) =>
            {
                AddHybridComponent(vfx);
            });
#endif
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class LightProbeProxyVolumeConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((LightProbeProxyVolume group) =>
            {
                AddHybridComponent(group);
            });
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class ReflectionProbeConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ReflectionProbe probe) =>
            {
                AddHybridComponent(probe);
            });

#if HDRP_7_0_0_OR_NEWER
            Entities.ForEach((HDAdditionalReflectionData reflectionData) =>
            {
                AddHybridComponent(reflectionData);
            });
#endif
        }
    }

#if HYBRID_ENTITIES_CAMERA_CONVERSION
    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class CameraConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Camera camera) =>
            {
                AddHybridComponent(camera);
            });

#if HDRP_7_0_0_OR_NEWER
            Entities.ForEach((HDAdditionalCameraData data) =>
            {
                AddHybridComponent(data);
            });
#endif

#if URP_7_0_0_OR_NEWER
            Entities.ForEach((UniversalAdditionalCameraData data) =>
            {
                AddHybridComponent(data);
            });
#endif
        }
    }
#endif

    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class TextMeshConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((TextMesh mesh, MeshRenderer renderer) =>
            {
                AddHybridComponent(mesh);
                AddHybridComponent(renderer);
            });
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class SpriteRendererConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((SpriteRenderer sprite) =>
            {
                AddHybridComponent(sprite);
            });
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class VisualEffectConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((VisualEffect vfx) =>
            {
                AddHybridComponent(vfx);
            });
        }
    }

#if HDRP_7_0_0_OR_NEWER
    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class DecalProjectorConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((DecalProjector projector) =>
            {
                AddHybridComponent(projector);
            });
        }
    }
#endif

#if HDRP_7_0_0_OR_NEWER
    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class DensityVolumeConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((DensityVolume volume) =>
            {
                AddHybridComponent(volume);
            });
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class PlanarReflectionProbeConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((PlanarReflectionProbe probe) =>
            {
                AddHybridComponent(probe);
            });
        }
    }
#endif

#if SRP_7_0_0_OR_NEWER
    [WorldSystemFilter(WorldSystemFilterFlags.HybridGameObjectConversion)]
    public class VolumeConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Volume volume) =>
            {
                AddHybridComponent(volume);
            });
        }
    }
#endif
#endif
}

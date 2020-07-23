using Unity.Build.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Hybrid;

namespace Unity.Scenes.Editor
{
    static class ResourceCatalogBuildCode
    {
        public static void WriteCatalogFile(SceneList sceneList, string sceneInfoPath)
        {
            var sceneInfos = sceneList.GetSceneInfosForBuild();
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<ResourceCatalogData>();
            var metas = builder.Allocate(ref root.resources, sceneInfos.Length);
            for (int i = 0; i < sceneInfos.Length; i++)
            {
                metas[i] = new ResourceMetaData()
                {
                    ResourceId = sceneInfos[i].Scene.assetGUID,
                    ResourceFlags = sceneInfos[i].AutoLoad ? ResourceMetaData.Flags.AutoLoad : ResourceMetaData.Flags.None,
                    ResourceType = ResourceMetaData.Type.Scene,
                };
            }

            var strings = builder.Allocate(ref root.paths, sceneInfos.Length);
            for (int i = 0; i < sceneInfos.Length; i++)
                builder.AllocateString(ref strings[i], sceneInfos[i].Path);

            BlobAssetReference<ResourceCatalogData>.Write(builder, sceneInfoPath, ResourceCatalogData.CurrentFileFormatVersion);
            builder.Dispose();
        }
    }
}

using Unity.Entities;
using UnityEngine;
#if UNITY_EDITOR
using Unity.Entities.Tests;
#endif

public struct TestMetadata : IComponentData
{
    public int SectionIndex;
    public int Value;
}

public struct TestMetadataTag : IComponentData
{
}

public struct TestMetadataWithEntity : IComponentData
{
    public Entity Entity;
}

public struct TestMetadataWithBlobAsset : IComponentData
{
    public BlobAssetReference<int> BlobAsset;
}
#if UNITY_EDITOR
[ConverterVersion("simonm", 5)]
public class SectionMetadataTestAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int Value;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var sectionEntity = conversionSystem.GetSceneSectionEntity(entity);
        //Second call to GetSceneSectionEntity should return same entity, else bail out making the test fail
        if (conversionSystem.GetSceneSectionEntity(entity) != sectionEntity)
            return;

        int sectionIndex = dstManager.GetSharedComponentData<SceneSection>(entity).Section;
        dstManager.AddComponentData(sectionEntity, new TestMetadata {SectionIndex = sectionIndex, Value = Value});

        dstManager.AddComponentData(sectionEntity, new TestMetadataWithEntity());
        dstManager.AddComponentData(sectionEntity, new TestMetadataWithBlobAsset());
        dstManager.AddSharedComponentData(sectionEntity, new EcsTestSharedComp());
        dstManager.AddBuffer<EcsIntElement>(sectionEntity);
        dstManager.AddComponentData(sectionEntity, new EcsState1());
        dstManager.AddComponent<TestMetadataTag>(sectionEntity);
#if !UNITY_DISABLE_MANAGED_COMPONENTS
        dstManager.AddComponentData(sectionEntity, new EcsTestManagedComponent());
#endif
    }
}
#endif

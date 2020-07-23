using NUnit.Framework;
using Unity.Entities.Tests;
using Unity.Scenes.Editor;
using UnityEditor;
using UnityEngine;
using Unity.Entities;
using World = Unity.Entities.World;

namespace Unity.Scenes.Tests
{
#if !UNITY_DISABLE_MANAGED_COMPONENTS
    public class EditorEntityScenesTests : ECSTestsFixture
    {
        public class MaterialRefComponent : IComponentData
        {
            public Material Value;
        }

        [Test]
        public void TestReadAndWriteWithObjectRef()
        {
            string binPath = "Temp/test.bin";
            string binRefPath = "Temp/test.bin.ref";

            var dstWorld = new World("");
            var dstEntitymanager = dstWorld.EntityManager;
            var material = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.entities/Unity.Scenes.Hybrid.Tests/Test.mat");

            var entity = m_Manager.CreateEntity();
            m_Manager.AddComponentData(entity, new MaterialRefComponent { Value = material });
            m_Manager.AddComponentData(entity, new EcsTestData() { value = 5});

            EditorEntityScenes.Write(m_Manager, binPath, binRefPath);
            EditorEntityScenes.Read(dstEntitymanager, binPath, binRefPath);

            var dstEntity = dstEntitymanager.UniversalQuery.GetSingletonEntity();

            Assert.AreEqual(material, m_Manager.GetComponentData<MaterialRefComponent>(entity).Value);
            Assert.AreEqual(material, dstEntitymanager.GetComponentData<MaterialRefComponent>(dstEntity).Value);

            Assert.AreEqual(5, m_Manager.GetComponentData<EcsTestData>(entity).value);
            Assert.AreEqual(5, dstEntitymanager.GetComponentData<EcsTestData>(dstEntity).value);
        }
    }
#endif
}

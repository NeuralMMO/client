using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Entities.Tests.Conversion
{
    class GeneratedAuthoringComponentConversionTests : ConversionTestFixtureBase
    {
        static Type GetAuthoringComponentType<T>()
        {
            var typeName = $"Unity.Entities.Tests.{typeof(T).Name}Authoring, Unity.Entities.TestComponents";
            var authoringType = Type.GetType(typeName);
            Assert.IsNotNull(authoringType, $"Could not find generated authoring type for {typeof(T).Name}, looked for {typeName}.");
            return authoringType;
        }

        [Test]
        public void GeneratedAuthoringComponent_ConvertsPrimitiveTypes()
        {
            var go = CreateGameObject();
            var authoringType = GetAuthoringComponentType<CodeGenTestComponent>();
            var c = go.AddComponent(authoringType);
            authoringType.GetField(nameof(CodeGenTestComponent.Bool)).SetValue(c, true);
            authoringType.GetField(nameof(CodeGenTestComponent.Int)).SetValue(c, 16);
            authoringType.GetField(nameof(CodeGenTestComponent.Char)).SetValue(c, 'x');
            authoringType.GetField(nameof(CodeGenTestComponent.Enum)).SetValue(c, WorldFlags.Conversion | WorldFlags.Shadow);

            const float floatValue = 42.2f;
            authoringType.GetField(nameof(CodeGenTestComponent.Float)).SetValue(c, floatValue);


            Entity goEntity = default;
            Assert.DoesNotThrow(() => goEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, MakeDefaultSettings()));

            EntitiesAssert.ContainsOnly(m_Manager,
                // gameobject created above
                EntityMatch.Exact<CodeGenTestComponent>(goEntity, k_RootComponents)
            );

            var component = m_Manager.GetComponentData<CodeGenTestComponent>(goEntity);
            Assert.AreEqual(true, component.Bool);
            Assert.AreEqual(16, component.Int);
            Assert.AreEqual(floatValue, component.Float);
            Assert.AreEqual('x', component.Char);
            Assert.AreEqual(WorldFlags.Conversion | WorldFlags.Shadow, component.Enum);
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void GeneratedAuthoringComponent_ConvertsString()
        {
            var go = CreateGameObject();
            var authoringType = GetAuthoringComponentType<CodeGenManagedTestComponent>();
            var c = go.AddComponent(authoringType);
            authoringType.GetField(nameof(CodeGenManagedTestComponent.String)).SetValue(c, "test");
            // MonoBehaviours never have null arrays on them, hence the code isn't checking for that.
            authoringType.GetField(nameof(CodeGenManagedTestComponent.Entities)).SetValue(c, Array.Empty<GameObject>());

            Entity goEntity = default;
            Assert.DoesNotThrow(() => goEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, MakeDefaultSettings()));

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<CodeGenManagedTestComponent>(goEntity, k_RootComponents)
            );

            var component = m_Manager.GetComponentData<CodeGenManagedTestComponent>(goEntity);
            Assert.AreEqual("test", component.String);
        }

#endif

        [Test]
        public void GeneratedAuthoringComponent_WithNullReference_DoesNotThrow()
        {
            var go = CreateGameObject();
            go.AddComponent(GetAuthoringComponentType<CodeGenTestComponent>());

            Entity goEntity = default;
            Assert.DoesNotThrow(() => goEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, MakeDefaultSettings()));

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<CodeGenTestComponent>(goEntity, k_RootComponents)
            );

            Assert.AreEqual(Entity.Null, m_Manager.GetComponentData<CodeGenTestComponent>(goEntity).Entity);
        }

        [Test]
        public void GeneratedAuthoringComponent_WithNonPrefabReference_IsNotConverted()
        {
            var go = CreateGameObject();

            var authoringType = GetAuthoringComponentType<CodeGenTestComponent>();
            var c = go.AddComponent(authoringType);
            authoringType.GetField(nameof(CodeGenTestComponent.Entity)).SetValue(c, CreateGameObject());

            Entity goEntity = default;
            LogAssert.Expect(LogType.Warning, new Regex(@"GameObject that was not included in the conversion"));
            Assert.DoesNotThrow(() => goEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, MakeDefaultSettings()));

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<CodeGenTestComponent>(goEntity, k_RootComponents)
            );
            Assert.AreEqual(Entity.Null, m_Manager.GetComponentData<CodeGenTestComponent>(goEntity).Entity);
        }

        [Test]
        public void GeneratedAuthoringComponent_WithReferencedPrefab_IsConverted()
        {
            var go = CreateGameObject();

            var authoringType = GetAuthoringComponentType<CodeGenTestComponent>();
            var c = go.AddComponent(authoringType);
            authoringType.GetField(nameof(CodeGenTestComponent.Entity)).SetValue(c, LoadPrefab("Prefab"));

            Entity goEntity = default;
            Assert.DoesNotThrow(() => goEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, MakeDefaultSettings()));

            EntitiesAssert.Contains(m_Manager, EntityMatch.Exact<CodeGenTestComponent>(goEntity, k_RootComponents));
            var prefabEntity = m_Manager.GetComponentData<CodeGenTestComponent>(goEntity).Entity;
            EntitiesAssert.ContainsOnly(m_Manager,
                // gameobject created above
                EntityMatch.Exact<CodeGenTestComponent>(goEntity, k_RootComponents),
                // referenced prefab
                EntityMatch.Exact<Prefab>(prefabEntity, new MockData(), k_RootComponents)
            );
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void GeneratedAuthoringComponent_WithMultipleReferences_ConvertsOnlyPrefab()
        {
            var go = CreateGameObject();

            var authoringType = GetAuthoringComponentType<CodeGenManagedTestComponent>();
            var c = go.AddComponent(authoringType);
            authoringType.GetField(nameof(CodeGenManagedTestComponent.Entities)).SetValue(c, new[]
            {
                LoadPrefab("Prefab"),
                null,
                CreateGameObject()
            });

            LogAssert.Expect(LogType.Warning, new Regex(@"GameObject that was not included in the conversion"));
            Entity goEntity = default;
            Assert.DoesNotThrow(() => goEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, MakeDefaultSettings()));
            EntitiesAssert.Contains(m_Manager, EntityMatch.Exact<CodeGenManagedTestComponent>(goEntity, k_RootComponents));

            var prefabEntity = m_Manager.GetComponentData<CodeGenManagedTestComponent>(goEntity).Entities[0];
            EntitiesAssert.ContainsOnly(m_Manager,
                // gameobject created above
                EntityMatch.Exact<CodeGenManagedTestComponent>(goEntity, k_RootComponents),
                // referenced prefab
                EntityMatch.Exact<Prefab>(prefabEntity, new MockData(), k_RootComponents)
            );
        }

#endif
    }
}

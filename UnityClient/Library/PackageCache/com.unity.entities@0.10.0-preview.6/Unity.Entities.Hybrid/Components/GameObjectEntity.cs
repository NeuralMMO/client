using System;
using System.Collections.Generic;
using Unity.Entities.Conversion;
using UnityEngine;
using UnityEngine.Assertions;
using MonoBehaviour = UnityEngine.MonoBehaviour;
using GameObject = UnityEngine.GameObject;
using Component = UnityEngine.Component;

namespace Unity.Entities
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [AddComponentMenu("")]
    public class GameObjectEntity : MonoBehaviour
    {
        public EntityManager EntityManager
        {
            get
            {
                if (enabled && gameObject.activeInHierarchy)
                    ReInitializeEntityManagerAndEntityIfNecessary();
                return m_EntityManager;
            }
        }
        EntityManager m_EntityManager;

        public Entity Entity
        {
            get
            {
                if (enabled && gameObject.activeInHierarchy)
                    ReInitializeEntityManagerAndEntityIfNecessary();
                return m_Entity;
            }
        }
        Entity m_Entity;

        void ReInitializeEntityManagerAndEntityIfNecessary()
        {
            // in case e.g., on a prefab that was open for edit when domain was unloaded
            // existing m_EntityManager lost all its data, so simply create a new one
            if (!m_EntityManager.IsCreated && !m_Entity.Equals(default))
                Initialize();
        }

        static List<Component> s_ComponentsCache = new List<Component>();

        // TODO: Very wrong error messages when creating entity with empty ComponentType array?
        public static Entity AddToEntityManager(EntityManager entityManager, GameObject gameObject)
        {
            var entity = GameObjectConversionMappingSystem.CreateGameObjectEntity(entityManager, gameObject, s_ComponentsCache);
            s_ComponentsCache.Clear();
            return entity;
        }

        //@TODO: is this used? deprecate?
        public static void AddToEntity(EntityManager entityManager, GameObject gameObject, Entity entity)
        {
            var components = gameObject.GetComponents<Component>();

#pragma warning disable 618 // remove once ComponentDataProxyBase is removed
            for (var i = 0; i != components.Length; i++)
            {
                var component = components[i];
                if (component == null || component is ComponentDataProxyBase || component is GameObjectEntity || component.IsComponentDisabled())
                    continue;

                entityManager.AddComponentObject(entity, component);
            }
#pragma warning restore 618
        }

        void Initialize()
        {
            DefaultWorldInitialization.DefaultLazyEditModeInitialize();
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                m_Entity = AddToEntityManager(m_EntityManager, gameObject);
            }
        }

        protected virtual void OnEnable()
        {
            Initialize();
        }

        protected virtual void OnDisable()
        {
            if (EntityManager.IsCreated && EntityManager.Exists(Entity))
                EntityManager.DestroyEntity(Entity);

            m_EntityManager = default;
            m_Entity = Entity.Null;
        }

        public static void CopyAllComponentsToEntity(GameObject gameObject, EntityManager entityManager, Entity entity)
        {
#pragma warning disable 618 // remove once ComponentDataProxyBase is removed
            foreach (var proxy in gameObject.GetComponents<ComponentDataProxyBase>())
            {
                var type = proxy.GetComponentType();
                entityManager.AddComponent(entity, type);
                proxy.UpdateComponentData(entityManager, entity);
            }
#pragma warning restore 618
        }
    }
}

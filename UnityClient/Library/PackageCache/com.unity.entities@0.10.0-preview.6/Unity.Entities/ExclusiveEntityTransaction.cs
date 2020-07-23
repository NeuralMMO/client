using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Entities
{
    public unsafe struct ExclusiveEntityTransaction
    {
        private EntityManager m_Manager;

        /// <summary>
        /// Return the entity manager this transaction operates upon
        /// </summary>
        public EntityManager EntityManager => m_Manager;

        internal ExclusiveEntityTransaction(EntityManager manager)
        {
            m_Manager = manager;
        }

        internal void OnDestroy()
        {
        }

        internal EntityArchetype CreateArchetype(ComponentType* types, int count)
        {
            return m_Manager.CreateArchetype(types, count);
        }

        public EntityArchetype CreateArchetype(params ComponentType[] types)
        {
            return m_Manager.CreateArchetype(types);
        }

        public Entity CreateEntity(EntityArchetype archetype)
        {
            return m_Manager.CreateEntity(archetype);
        }

        public void CreateEntity(EntityArchetype archetype, NativeArray<Entity> entities)
        {
            m_Manager.CreateEntity(archetype, entities);
        }

        public Entity CreateEntity(params ComponentType[] types)
        {
            return m_Manager.CreateEntity(types);
        }

        public Entity Instantiate(Entity srcEntity)
        {
            return m_Manager.Instantiate(srcEntity);
        }

        public void Instantiate(Entity srcEntity, NativeArray<Entity> outputEntities)
        {
            m_Manager.Instantiate(srcEntity, outputEntities);
        }

        public void DestroyEntity(NativeArray<Entity> entities)
        {
            m_Manager.DestroyEntity(entities);
        }

        public void DestroyEntity(NativeSlice<Entity> entities)
        {
            m_Manager.DestroyEntity(entities);
        }

        public void DestroyEntity(Entity entity)
        {
            m_Manager.DestroyEntity(entity);
        }

        public void AddComponent(Entity entity, ComponentType componentType)
        {
            m_Manager.AddComponent(entity, componentType);
        }

        public DynamicBuffer<T> AddBuffer<T>(Entity entity) where T : struct, IBufferElementData
        {
            return m_Manager.AddBuffer<T>(entity);
        }

        public void RemoveComponent(Entity entity, ComponentType type)
        {
            m_Manager.RemoveComponent(entity, type);
        }

        public bool Exists(Entity entity)
        {
            return m_Manager.Exists(entity);
        }

        public bool HasComponent(Entity entity, ComponentType type)
        {
            return m_Manager.HasComponent(entity, type);
        }

        public T GetComponentData<T>(Entity entity) where T : struct, IComponentData
        {
            return m_Manager.GetComponentData<T>(entity);
        }

        public void SetComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData
        {
            m_Manager.SetComponentData(entity, componentData);
        }

        public T GetSharedComponentData<T>(Entity entity) where T : struct, ISharedComponentData
        {
            return m_Manager.GetSharedComponentData<T>(entity);
        }

        public void SetSharedComponentData<T>(Entity entity, T componentData) where T : struct, ISharedComponentData
        {
            m_Manager.SetSharedComponentData(entity, componentData);
        }

        internal void AddSharedComponent<T>(NativeArray<ArchetypeChunk> chunks, T componentData)
            where T : struct, ISharedComponentData
        {
            m_Manager.AddSharedComponent<T>(chunks, componentData);
        }

        public DynamicBuffer<T> GetBuffer<T>(Entity entity) where T : struct, IBufferElementData
        {
            return m_Manager.GetBuffer<T>(entity);
        }

        public void SwapComponents(ArchetypeChunk leftChunk, int leftIndex, ArchetypeChunk rightChunk, int rightIndex)
        {
            m_Manager.SwapComponents(leftChunk, leftIndex, rightChunk, rightIndex);
        }

        internal void AllocateConsecutiveEntitiesForLoading(int count)
        {
            m_Manager.AllocateConsecutiveEntitiesForLoading(count);
        }
    }
}

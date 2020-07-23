using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace Unity.Entities
{
    public unsafe partial struct EntityManager
    {
        // ----------------------------------------------------------------------------------------------------------
        // PUBLIC
        // ----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates an entity having the specified archetype.
        /// </summary>
        /// <remarks>
        /// The EntityManager creates the entity in the first available chunk with the matching archetype that has
        /// enough space.
        ///
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before creating the entity and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="archetype">The archetype for the new entity.</param>
        /// <returns>The Entity object that you can use to access the entity.</returns>
        [StructuralChangeMethod]
        public Entity CreateEntity(EntityArchetype archetype)
        {
            var access = GetCheckedEntityDataAccess();
            return access->CreateEntity(archetype);
        }

        /// <summary>
        /// Creates an entity having components of the specified types.
        /// </summary>
        /// <remarks>
        /// The EntityManager creates the entity in the first available chunk with the matching archetype that has
        /// enough space.
        ///
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before creating the entity and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="types">The types of components to add to the new entity.</param>
        /// <returns>The Entity object that you can use to access the entity.</returns>
        [StructuralChangeMethod]
        public Entity CreateEntity(params ComponentType[] types)
        {
            return CreateEntity(CreateArchetype(types));
        }

        [StructuralChangeMethod]
        public Entity CreateEntity()
        {
            BeforeStructuralChange();
            Entity entity;
            var access = GetCheckedEntityDataAccess();
            var ecs = access->EntityComponentStore;
            var mcs = access->ManagedComponentStore;

            ecs->CreateEntities(access->GetEntityOnlyArchetype().Archetype, &entity, 1);
            mcs.Playback(ref ecs->ManagedChangesTracker);
            return entity;
        }

        /// <summary>
        /// Creates a set of entities of the specified archetype.
        /// </summary>
        /// <remarks>Fills the [NativeArray](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html)
        /// object assigned to the `entities` parameter with the Entity objects of the created entities. Each entity
        /// has the components specified by the <see cref="EntityArchetype"/> object assigned
        /// to the `archetype` parameter. The EntityManager adds these entities to the <see cref="World"/> entity list. Use the
        /// Entity objects in the array for further processing, such as setting the component values.</remarks>
        /// <param name="archetype">The archetype defining the structure for the new entities.</param>
        /// <param name="entities">An array to hold the Entity objects needed to access the new entities.
        /// The length of the array determines how many entities are created.</param>
        [StructuralChangeMethod]
        public void CreateEntity(EntityArchetype archetype, NativeArray<Entity> entities)
        {
            GetCheckedEntityDataAccess()->CreateEntity(archetype, entities);
        }

        /// <summary>
        /// Creates a set of entities of the specified archetype.
        /// </summary>
        /// <remarks>Creates a [NativeArray](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html) of entities,
        /// each of which has the components specified by the <see cref="EntityArchetype"/> object assigned
        /// to the `archetype` parameter. The EntityManager adds these entities to the <see cref="World"/> entity list.</remarks>
        /// <param name="archetype">The archetype defining the structure for the new entities.</param>
        /// <param name="entityCount">The number of entities to create with the specified archetype.</param>
        /// <param name="allocator">How the created native array should be allocated.</param>
        /// <returns>
        /// A [NativeArray](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html) of entities
        /// with the given archetype.
        /// </returns>
        [StructuralChangeMethod]
        public NativeArray<Entity> CreateEntity(EntityArchetype archetype, int entityCount, Allocator allocator)
        {
            var entities = new NativeArray<Entity>(entityCount, allocator);
            GetCheckedEntityDataAccess()->CreateEntity(archetype, entities);

            return entities;
        }

        /// <summary>
        /// Destroy all entities having a common set of component types.
        /// </summary>
        /// <remarks>Since entities in the same chunk share the same component structure, this function effectively destroys
        /// the chunks holding any entities identified by the `entityQueryFilter` parameter.</remarks>
        /// <param name="entityQueryFilter">Defines the components an entity must have to qualify for destruction.</param>
        [StructuralChangeMethod]
        public void DestroyEntity(EntityQuery entityQuery)
        {
            var access = GetCheckedEntityDataAccess();
            var ecs = access->EntityComponentStore;
            var queryImpl = entityQuery._GetImpl();

            Unity.Entities.EntityComponentStore.AssertValidEntityQuery(entityQuery, ecs);
            DestroyEntity(queryImpl->_QueryData->MatchingArchetypes, queryImpl->_Filter);
        }

        /// <summary>
        /// Destroys all entities in an array.
        /// </summary>
        /// <remarks>
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before destroying the entity and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="entities">An array containing the Entity objects of the entities to destroy.</param>
        [StructuralChangeMethod]
        public void DestroyEntity(NativeArray<Entity> entities)
        {
            DestroyEntityInternal((Entity*)entities.GetUnsafeReadOnlyPtr(), entities.Length);
        }

        /// <summary>
        /// Destroys all entities in a slice of an array.
        /// </summary>
        /// <remarks>
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before destroying the entity and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="entities">The slice of an array containing the Entity objects of the entities to destroy.</param>
        [StructuralChangeMethod]
        public void DestroyEntity(NativeSlice<Entity> entities)
        {
            DestroyEntityInternal((Entity*)entities.GetUnsafeReadOnlyPtr(), entities.Length);
        }

        /// <summary>
        /// Destroys an entity.
        /// </summary>
        /// <remarks>
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before destroying the entity and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="entity">The Entity object of the entity to destroy.</param>
        [StructuralChangeMethod]
        public void DestroyEntity(Entity entity)
        {
            DestroyEntityInternal(&entity, 1);
        }

        /// <summary>
        /// Clones an entity.
        /// </summary>
        /// <remarks>
        /// The new entity has the same archetype and component values as the original, however system state & prefab tag components are removed from the clone.
        ///
        /// If the source entity was converted from a prefab and thus has a <see cref="LinkedEntityGroup"/> component,
        /// the entire group is cloned as a new set of entities. Entity references on components that are being cloned to entities inside the set are remapped to the instantiated entities.
        ///
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before creating the entity and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="srcEntity">The entity to clone</param>
        /// <returns>The Entity object for the new entity.</returns>
        [StructuralChangeMethod]
        public Entity Instantiate(Entity srcEntity)
        {
            var access = GetCheckedEntityDataAccess();
            Entity entity;
            access->InstantiateInternal(srcEntity, &entity, 1);
            return entity;
        }

        /// <summary>
        /// Makes multiple clones of an entity.
        /// </summary>
        /// <remarks>
        /// The new entity has the same archetype and component values as the original, however system state & prefab tag components are removed from the clone.
        ///
        /// If the source entity has a <see cref="LinkedEntityGroup"/> component, the entire group is cloned as a new
        /// set of entities. Entity references on components that are being cloned to entities inside the set are remapped to the instantiated entities.
        ///
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before creating these entities and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="srcEntity">The entity to clone.</param>
        /// <param name="outputEntities">An array to receive the Entity objects of the root entity in each clone.
        /// The length of this array determines the number of clones.</param>
        [StructuralChangeMethod]
        public void Instantiate(Entity srcEntity, NativeArray<Entity> outputEntities)
        {
            var access = GetCheckedEntityDataAccess();
            access->InstantiateInternal(srcEntity, (Entity*)outputEntities.GetUnsafePtr(), outputEntities.Length);
        }

        /// <summary>
        /// Makes multiple clones of an entity.
        /// </summary>
        /// <remarks>
        /// The new entity has the same archetype and component values as the original, however system state & prefab tag components are removed from the clone.
        ///
        /// If the source entity has a <see cref="LinkedEntityGroup"/> component, the entire group is cloned as a new
        /// set of entities. Entity references on components that are being cloned to entities inside the set are remapped to the instantiated entities.
        ///
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before creating these entities and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="srcEntity">The entity to clone.</param>
        /// <param name="instanceCount">The number of entities to instantiate with the same components as the source entity.</param>
        /// <param name="allocator">How the created native array should be allocated.</param>
        /// <returns>A [NativeArray](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html) of entities.</returns>
        [StructuralChangeMethod]
        public NativeArray<Entity> Instantiate(Entity srcEntity, int instanceCount, Allocator allocator)
        {
            var access = GetCheckedEntityDataAccess();
            var entities = new NativeArray<Entity>(instanceCount, allocator);
            access->InstantiateInternal(srcEntity, (Entity*)entities.GetUnsafePtr(), instanceCount);
            return entities;
        }

        /// <summary>
        /// Clones a set of entities.
        /// </summary>
        /// <remarks>
        /// The new entity has the same archetype and component values as the original, however system state & prefab tag components are removed from the clone.
        ///
        /// Entity references on components that are being cloned to entities inside the set are remapped to the instantiated entities.
        /// This method overload ignores the <see cref="LinkedEntityGroup"/> component,
        /// since the group of entities that will be cloned is passed explicitly.
        ///
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before creating the entity and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="srcEntities">The set of entities to clone</param>
        /// <param name="outputEntities">the set of entities that were cloned. outputEntities.Length must match srcEntities.Length</param>
        [StructuralChangeMethod]
        public void Instantiate(NativeArray<Entity> srcEntities, NativeArray<Entity> outputEntities)
        {
            var access = GetCheckedEntityDataAccess();
            access->InstantiateInternal((Entity*)srcEntities.GetUnsafeReadOnlyPtr(), (Entity*)outputEntities.GetUnsafePtr(), srcEntities.Length, outputEntities.Length, true);
        }

        /// <summary>
        /// Clones a set of entities, different from Instantiate because it does not remove the prefab tag component.
        /// </summary>
        /// <remarks>
        /// The new entity has the same archetype and component values as the original, however system state components are removed from the clone.
        ///
        /// Entity references on components that are being cloned to entities inside the set are remapped to the instantiated entities.
        /// This method overload ignores the <see cref="LinkedEntityGroup"/> component,
        /// since the group of entities that will be cloned is passed explicitly.
        ///
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before creating the entity and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="srcEntities">The set of entities to clone</param>
        /// <param name="outputEntities">the set of entities that were cloned. outputEntities.Length must match srcEntities.Length</param>
        [StructuralChangeMethod]
        public void CopyEntities(NativeArray<Entity> srcEntities, NativeArray<Entity> outputEntities)
        {
            var access = GetCheckedEntityDataAccess();
            access->InstantiateInternal((Entity*)srcEntities.GetUnsafeReadOnlyPtr(), (Entity*)outputEntities.GetUnsafePtr(), srcEntities.Length, outputEntities.Length, false);
        }

        /// <summary>
        /// Creates a set of chunks containing the specified number of entities having the specified archetype.
        /// </summary>
        /// <remarks>
        /// The EntityManager creates enough chunks to hold the required number of entities.
        ///
        /// **Important:** This function creates a sync point, which means that the EntityManager waits for all
        /// currently running Jobs to complete before creating these chunks and no additional Jobs can start before
        /// the function is finished. A sync point can cause a drop in performance because the ECS framework may not
        /// be able to make use of the processing power of all available cores.
        /// </remarks>
        /// <param name="archetype">The archetype for the chunk and entities.</param>
        /// <param name="chunks">An empty array to receive the created chunks.</param>
        /// <param name="entityCount">The number of entities to create.</param>
        [Obsolete("CreateChunk is deprecated. (RemovedAfter 2020-06-05)", false)]
        [StructuralChangeMethod]
        public void CreateChunk(EntityArchetype archetype, NativeArray<ArchetypeChunk> chunks, int entityCount)
        {
            var access = GetCheckedEntityDataAccess();
            var ecs = access->EntityComponentStore;
            var mcs = access->ManagedComponentStore;

            Unity.Entities.EntityComponentStore.AssertValidArchetype(ecs, archetype);
            BeforeStructuralChange();

            ecs->CreateChunks(archetype.Archetype, (ArchetypeChunk*)chunks.GetUnsafePtr(), chunks.Length, entityCount);
            mcs.Playback(ref ecs->ManagedChangesTracker);
        }

        /// <summary>
        /// Detects the created and destroyed entities compared to last time the method was called with the given state.
        /// </summary>
        /// <remarks>
        /// Entities must be fully destroyed, if system state components keep it alive it still counts as not yet destroyed.
        /// EntityCommandBuffers that have not been played back will have no effect on this until they are played back.
        /// </remarks>
        /// <param name="state">The same state list must be passed when you call this method, it remembers the entities that were already notified created & destroyed.</param>
        /// <param name="createdEntities">The Entities that were created</param>
        /// <param name="destroyedEntities">The Entities that were destroyed</param>
        public JobHandle GetCreatedAndDestroyedEntitiesAsync(NativeList<int> state, NativeList<Entity> createdEntities, NativeList<Entity> destroyedEntities)
        {
            var access = GetCheckedEntityDataAccess();

            var jobHandle = Entities.EntityComponentStore.GetCreatedAndDestroyedEntities(access->EntityComponentStore, state, createdEntities, destroyedEntities, true);
            access->DependencyManager->AddDependency(null, 0, null, 0, jobHandle);

            return jobHandle;
        }

        /// <summary>
        /// Detects the created and destroyed entities compared to last time the method was called with the given state.
        /// </summary>
        /// <remarks>
        /// Entities must be fully destroyed, if system state components keep it alive it still counts as not yet destroyed.
        /// EntityCommandBuffers that have not been played back will have no effect on this until they are played back.
        /// </remarks>
        /// <param name="state">The same state list must be passed when you call this method, it remembers the entities that were already notified created & destroyed.</param>
        /// <param name="createdEntities">The Entities that were created</param>
        /// <param name="destroyedEntities">The Entities that were destroyed</param>
        public void GetCreatedAndDestroyedEntities(NativeList<int> state, NativeList<Entity> createdEntities, NativeList<Entity> destroyedEntities)
        {
            var access = GetCheckedEntityDataAccess();
            Entities.EntityComponentStore.GetCreatedAndDestroyedEntities(access->EntityComponentStore, state, createdEntities, destroyedEntities, false);
        }

        // ----------------------------------------------------------------------------------------------------------
        // INTERNAL
        // ----------------------------------------------------------------------------------------------------------

        internal void DestroyEntityInternal(Entity* entities, int count)
        {
            var access = GetCheckedEntityDataAccess();
            access->DestroyEntityInternal(entities, count);
        }

        void DestroyEntity(UnsafeMatchingArchetypePtrList archetypeList, EntityQueryFilter filter)
        {
            var access = GetCheckedEntityDataAccess();
            access->DestroyEntity(archetypeList, filter);
        }
    }
}

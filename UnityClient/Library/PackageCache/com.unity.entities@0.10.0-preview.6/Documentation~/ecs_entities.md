---
uid: ecs-entities
---
# Entities
<!-- 
> Topics to add
> * Spawning Entities in Jobs -- Entity Command Buffers
> * Transferring Entities between worlds: EM.MoveEntity
-->

Entities are one of the three principle elements of an Entity Component System architecture. They represent the individual "things" in your game or application. An entity has neither behavior nor data; instead, it identifies which pieces of data belong together. [Systems](ecs_systems.md) provide the behavior, and [components](ecs_components.md) store the data.

An entity is essentially an ID. The easiest way to think of it is as a super lightweight [GameObject](https://docs.unity3d.com/Manual/class-GameObject.html) that does not even have a name by default. Entity IDs are stable; you can use them to store a reference to another component or entity. For example, a child entity in a hierarchy might need to reference its parent entity. 

An [EntityManager](xref:Unity.Entities.EntityManager) manages all of the entities in a [World](xref:Unity.Entities.World). An EntityManager maintains the list of entities and organizes the data associated with an entity for optimal performance.

Although an entity does not have a type, groups of entities can be categorized by the types of data components associated with them. As you create entities and add components to them, the EntityManager keeps track of the unique combinations of components on the existing entities. Such a unique combination is called an __Archetype__. The EntityManager creates an [EntityArchetype](xref:Unity.Entities.EntityArchetype) struct as you add components to an entity. You can use existing `EntityArchetype`s to create new entities that conform to that archetype. You can also create an `EntityArchetype` in advance and use that to create entities. 

## Creating entities

The easiest way to create an entity is in the Unity Editor. You can set ECS to convert  both GameObjects placed in a Scene and Prefabs into entities at runtime. For more dynamic parts of your game or application, you can create spawning systems that create multiple entities in a job. Finally, you can use one of the [EntityManager.CreateEntity](xref:Unity.Entities.EntityManager.CreateEntity) functions to create entities one at a time.

### Creating entities with an EntityManager

Use one of the [EntityManager.CreateEntity](xref:Unity.Entities.EntityManager.CreateEntity) functions to create an entity. ECS creates the entity in the same World as the EntityManager.

You can create entities one-by-one in the following ways:

* Create an entity with components that use an array of [ComponentType](xref:Unity.Entities.ComponentType) objects.
* Create an entity with components that use an [EntityArchetype](xref:Unity.Entities.EntityArchetype).
* Copy an existing entity, including its current data, with [Instantiate](xref:Unity.Entities.EntityManager.Instantiate%28Unity.Entities.Entity%29)
* Create an entity with no components and then add components to it. (You can add components immediately or when additional components are needed.)

You can also create multiple entities at a time:

* Fill a NativeArray with new entities with the same archetype using [CreateEntity](xref:Unity.Entities.EntityManager.CreateEntity).
* Fill a NativeArray with copies of an existing entity, including its current data, using [Instantiate](xref:Unity.Entities.EntityManager.Instantiate%28Unity.Entities.Entity%29).
* Explicitly create chunks populated with a specified number of entities with a given archetype with [CreateChunk](xref:Unity.Entities.EntityManager.CreateChunk*).
    
## Adding and removing components

After an entity has been created, you can add or remove components. When you do this, the archetype of the affected entities change and the EntityManager must move altered data to a new chunk of memory, as well as condense the component arrays in the original chunks. 

Changes to an entity that cause structural changes — that is, adding or removing components that change the values of `SharedComponentData`, and destroying the entity — cannot be done inside a job because these could invalidate the data that the job is working on. Instead, you add the commands to make these types of changes to an [EntityCommandBuffer](xref:Unity.Entities.EntityCommandBuffer) and execute this command buffer after the job is complete.  

The EntityManager provides functions to remove a component from a single entity as well as all of the entities in a NativeArray. For more information, see the documentation on [Components](ecs_components.md).

## Iterating entities

Iterating over all entities that have a matching set of components, is at the center of the ECS architecture. See [Accessing entity Data](chunk_iteration.md).
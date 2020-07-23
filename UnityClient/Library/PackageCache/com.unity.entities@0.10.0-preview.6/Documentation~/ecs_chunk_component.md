---
uid: ecs-chunk-component-data
---

# Chunk component data

Use chunk components to associate data with a specific [chunk](xref:Unity.Entities.ArchetypeChunk).

Chunk components contain data that applies to all entities in a specific chunk. For example, if you have chunks of entities that represent 3D objects that are organized by proximity, you can use a chunk component to store a collective bounding box for them. chunk components use the interface type [IComponentData](xref:Unity.Entities.IComponentData). 

## Add and set the values of a chunk component

Although chunk components can have values unique to an individual chunk, they are still part of the archetype of the entities in the chunk. Therefore, if you remove a chunk component from an entity, ECS moves that entity to a different chunk (possibly a new one). Likewise, if you add a chunk component to an entity, ECS moves that entity to a different chunk because its archetype changes; the addition of the chunk component does not affect the remaining entities in the original chunk. 

If you use an entity in a chunk to change the value of a chunk component, it changes the value of the chunk component that is common to all the entities in that chunk. If you change the archetype of an entity so that it moves into a new chunk that has the same type of chunk component, then the existing value in the destination chunk is unaffected. **Note:** If the entity is moved to a newly created chunk, then ECS creates a new chunk component for that chunk and assigns its default value.

The main differences between working with chunk components and general-purpose components is that you use different functions to add, set, and remove them. chunk components also have their own [ComponentType](xref:Unity.Entities.ComponentType.ChunkComponentType) functions that you use to define entity archetypes and queries. 

 
**Relevant APIs**

| **Purpose** | **Function** |
| :---------------  | :---------------- |
| Declaration | [IComponentData](xref:Unity.Entities.IComponentData) |
|&nbsp;|&nbsp;|
| **[ArchetypeChunk methods](xref:Unity.Entities.ArchetypeChunk)** | 
| Read | [GetChunkComponentData<T>(ArchetypeChunkComponentType<T>)](xref:Unity.Entities.ArchetypeChunk.GetChunkComponentData*) |
| Check | [HasChunkComponent<T>(ArchetypeChunkComponentType<T>)](xref:Unity.Entities.ArchetypeChunk.HasChunkComponent*) |
| Write | [SetChunkComponentData<T>(ArchetypeChunkComponentType<T>, T)](xref:Unity.Entities.ArchetypeChunk.SetChunkComponentData*) |
|&nbsp;|&nbsp;|
|  **[EntityManager methods](xref:Unity.Entities.EntityManager)** |&nbsp; |
| Create | [AddChunkComponentData<T>(Entity)](xref:Unity.Entities.EntityManager.AddChunkComponentData``1(Unity.Entities.Entity)) |
| Create | [AddChunkComponentData<T>(EntityQuery, T)](xref:Unity.Entities.EntityManager.AddChunkComponentData``1(Unity.Entities.EntityQuery,``0)) |
| Create | [AddComponents(Entity,ComponentTypes)](xref:Unity.Entities.EntityManager.AddComponents(Unity.Entities.Entity,Unity.Entities.ComponentTypes)) |
| Get type info | [GetArchetypeChunkComponentType<T>(Boolean)](xref:Unity.Entities.EntityManager.GetArchetypeChunkComponentType*) |
| Read | [GetChunkComponentData<T>(ArchetypeChunk)](xref:Unity.Entities.EntityManager.GetChunkComponentData``1(Unity.Entities.ArchetypeChunk)) |
| Read | [GetChunkComponentData<T>(Entity)](xref:Unity.Entities.EntityManager.GetChunkComponentData``1(Unity.Entities.Entity)) |
| Check | [HasChunkComponent<T>(Entity)](xref:Unity.Entities.EntityManager.HasChunkComponent*) |
| Delete | [RemoveChunkComponent<T>(Entity)](xref:Unity.Entities.EntityManager.RemoveChunkComponent``1(Unity.Entities.Entity)) |
| Delete | [RemoveChunkComponentData<T>(EntityQuery)](xref:Unity.Entities.EntityManager.RemoveChunkComponentData*) |
| Write | [EntityManager.SetChunkComponentData<T>(ArchetypeChunk, T)](xref:Unity.Entities.EntityManager.SetChunkComponentData*) |

<a name="declare"></a>

## Declaring a chunk component

Chunk components use the interface type [IComponentData](xref:Unity.Entities.IComponentData).

[!code-cs[declare-chunk-component](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#declare-chunk-component)]


<a name="create"></a>
## Creating a chunk component 

To add a chunk component directly, use an entity in the target chunk, or use an entity query that selects a group of target chunks. You cannot add chunk components inside a job, nor can they be added with an `EntityCommandBuffer`.
 
You can also include chunk components as part of  the [EntityArchetype](xref:Unity.Entities.EntityArchetype) or list of [ComponentType](xref:Unity.Entities.ComponentType) objects that ECS uses to create entities. ECS creates the chunk components for each chunk and stores entities with that archetype. 


Use [ComponentType.ChunkComponent&lt;T&gt;](xref:Unity.Entities.ComponentType``1) or [ComponentType.ChunkComponentReadOnly&lt;T&gt;](xref:Unity.Entities.ComponentTypeReadOnly``1) with these methods. Otherwise, ECS treats the component as a general-purpose component instead of a chunk component.
 
**With an entity in a chunk**

Given an entity in the target chunk, you can use the [EntityManager.AddChunkComponentData&lt;T&gt;()](xref:Unity.Entities.EntityManager.AddChunkComponentData``1) function to add a chunk component to the chunk:

[!code-cs[em-snippet](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#em-snippet)]

When you use this method, you cannot immediately set a value for the chunk component.

**With an [EntityQuery](xref:Unity.Entities.EntityQuery)**

Given an entity query that selects all the chunks that you want to add a chunk component to, you can use the [EntityManager.AddChunkComponentData&lt;T&gt;()](xref:Unity.Entities.EntityManager.AddChunkComponentData``1(Unity.Entities.EntityQuery,``0)) function to add and set the component:

[!code-cs[desc-chunk-component](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#desc-chunk-component)]

When you use this method, you can set the same initial value for all of the new chunk components.

**With an [EntityArchetype](xref:Unity.Entities.EntityArchetype)**

When you create entities with an archetype or a list of component types, include the chunk component types in the archetype:

[!code-cs[archetype-chunk-component](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#archetype-chunk-component)]

or list of component types:

[!code-cs[component-list-chunk-component](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#component-list-chunk-component)]

When you use these methods, the chunk components for new chunks that ECS creates as part of entity construction receive the default struct value. ECS does not change chunk components in existing chunks. See [Updating a chunk component](#update) for how to set the chunk component value given a reference to an entity.

<a name="read"></a>
## Reading a chunk component 

To read a chunk component, you can use the [ArchetypeChunk](xref:Unity.Entities.ArchetypeChunk) object that represents the chunk, or use an entity in the target chunk.

**With the ArchetypeChunk instance**

Given a chunk, you can use the [EntityManager.GetChunkComponentData&lt;T&gt;](xref:Unity.Entities.EntityManager.GetChunkComponentData``1(Unity.Entities.ArchetypeChunk)) function to read its chunk component. The following code iterates over all of the chunks that match a query and accesses a chunk component of type `ChunkComponentA`:

[!code-cs[read-chunk-component](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#read-chunk-component)]

**With an entity in a chunk**

Given an entity, you can access a chunk component in the chunk that contains the entity with [EntityManager.GetChunkComponentData&lt;T&gt;](xref:Unity.Entities.EntityManager.GetChunkComponentData``1(Unity.Entities.Entity)):

[!code-cs[read-entity-chunk-component](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#read-entity-chunk-component)]

<a name="update"></a>
## Updating a chunk component 

You can update a chunk component given a reference to the [chunk](xref:Unity.Entities.ArchetypeChunk) it belongs to. In an `IJobChunk` job, you can call [ArchetypeChunk.SetChunkComponentData](xref:Unity.Entities.ArchetypeChunk.SetChunkComponentData*). On the main thread, you can use the EntityManager version: [EntityManager.SetChunkComponentData](xref:Unity.Entities.EntityManager.SetChunkComponentData*). **Note:** You cannot access chunk components using SystemBase Entities.ForEach because you do not have access to the `ArchetypeChunk` object or the EntityManager.

**With the ArchetypeChunk instance**

To update a chunk component in a job, see [Reading and writing in a system](#read-and-write-jcs).

To update a chunk component on the main thread, use the EntityManager:

[!code-cs[set-chunk-component](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#set-chunk-component)]

**With an Entity instance**

If you have an entity in the chunk rather than the chunk reference itself, you can also use the EntityManger to get the chunk that contains the entity:

**Note:** If you only want to read a chunk component and not write to it, you should use [ComponentType.ChunkComponentReadOnly](xref:Unity.Entities.ComponentType.ChunkComponentReadOnly*) when you define the entity query to avoid creating unnecessary job scheduling constraints.

<a name="delete"></a>
## Deleting a chunk component 

Use the [EntityManager.RemoveChunkComponent](xref:Unity.Entities.EntityManager.RemoveChunkComponent*) functions to delete a chunk component. You can remove a chunk component given an entity in the target chunk or you can remove all of the chunk components of a given type from all chunks an entity query selects. 

If you remove a chunk component from an individual entity, that entity moves to a different chunk because the archetype of the entity changes. The chunk keeps the unchanged chunk component as long as there are other entities that remain in the chunk. 

<a name="in-query"></a>

## Using a chunk component in a query

To use a chunk component in an entity query, you must use either the [ComponentType.ChunkComponent&lt;T&gt;](xref:Unity.Entities.ComponentType``1) or [ComponentType.ChunkComponentReadOnly&lt;T&gt;](xref:Unity.Entities.ComponentTypeReadOnly``1) functions to specify the type. Otherwise, ECS treats the component as a general-purpose component instead of a Chunk component.

**In an [EntityQueryDesc](Unity.Entities.EntityQueryDesc)**

You can use the following query description to create an entity query that selects all chunks, and entities in those chunks, that have a chunk component of type, _ChunkComponentA_:

[!code-cs[use-chunk-component](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#use-chunk-component)]


<a name="chunk-by-chunk"></a>
## Iterating over chunks to set chunk components

To iterate over all chunks for which you want to set a chunk component, you can create an entity query that selects the correct chunks and then use the EntityQuery object to get a list of the ArchetypeChunk instances as a native array. The ArchetypeChunk object allows you to write a new value to the chunk component. 

[!code-cs[full-chunk-example](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#full-chunk-example)]

Note that if you need to read the components in a chunk to determine the proper value of a chunk component, you should use [IJobChunk](#read-and-write-jcs).  For example, the following code calculates the axis-aligned bounding box for all chunks containing entities that have LocalToWorld components:

[!code-cs[aabb-chunk-component](../package/DocCodeSamples.Tests/ChunkComponentExamples.cs#aabb-chunk-component)]


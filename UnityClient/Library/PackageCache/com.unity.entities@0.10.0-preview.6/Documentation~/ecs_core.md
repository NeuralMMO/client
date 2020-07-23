---
uid: ecs-concepts
---
# ECS concepts

An Entity Component System (ECS) architecture separates identity (**entities**), data (**components**), and behavior (**systems**). The architecture focuses on the data. Systems read streams of component data, and then transform the data from an input state to an output state, which entities then index.

The following diagram illustrates how these three basic parts work together:

![](images/ECSBlockDiagram.png)

In this diagram, a system reads `Translation` and `Rotation` components, multiplies them and then updates the corresponding `LocalToWorld` components (`L2W = T*R`).

The fact that entities A and B have a `Renderer` component and entity C does not, doesn't affect the system, because the system does not care about `Renderer` components. 

You can set up a system so that it requires a `Renderer` component, in which case, the system ignores the components of entity C; or, alternately, you can set up a system to exclude entities with `Renderer` components, which then ignores the components of entities A and B.

## Archetypes

A unique combination of component types is called an [Archetype](xref:Unity.Entities.Archetype). For example, a 3D object might have a component for its world transform, one for its linear movement, one for rotation, and one for its visual representation. Each instance of one of these 3D objects corresponds to a single entity, but because they share the same set of components, ECS classifies them as a single archetype: 

![](images/ArchetypeDiagram.png)

In this diagram, entities A and B share archetype M, while entity C has archetype N. 

To smoothly change the archetype of an entity, you can add or remove components at runtime. For example, if you remove the `Renderer` component from entity B, it then moves to archetype N.

## Memory Chunks

The archetype of an entity determines where ECS stores the components of that entity. ECS allocates memory in "chunks", each represented by an [ArchetypeChunk](xref:Unity.Entities.ArchetypeChunk) object. A chunk always contains entities of a single archetype. When a chunk of memory becomes full, ECS allocates a new chunk of memory for any new entities created with the same archetype. If you add or remove components, which then changes an entity archetype, ECS moves the components for that entity to a different chunk. 

![](images/ArchetypeChunkDiagram.png)

This organizational scheme provides a one-to-many relationship between archetypes and chunks. It also means that finding all the entities with a given set of components only requires searching through the existing archetypes, which are typically small in number, rather than all of the entities, which are typically much larger in number. 

ECS does not store the entities that are in a chunk in a specific order. When an entity is created or changed to a new archetype, ECS puts it into the first chunk that stores the archetype, and that has space. Chunks remain tightly packed, however; when an entity is removed from an archetype, ECS moves the components of the last entity in the chunk into the newly vacated slots in the component arrays.

**Note:** The values of shared components in an archetype also determine which entities are stored in which chunk. All of the entities in a given chunk have the exact same values for any shared components. If you change the value of any field in a shared component, the modified entity moves to a different chunk, just as it would if you changed that entity's archetype. A new chunk is allocated, if necessary. 

Use shared components to group entities within an archetype when it is more efficient to process them together. For example, the Hybrid Renderer defines its [RenderMesh component](https://docs.unity3d.com/Packages/com.unity.rendering.hybrid@latest?subfolder=/api/Unity.Rendering.RenderMesh.html) to achieve this.

## Entity queries

To identify which entities a system should process, use an [EntityQuery](xref:Unity.Entities.EntityQuery). An entity query searches the existing archetypes for those that have the components that match your requirements. You can specify the following component requirements with a query:

* **All** — the archetype must contain all of the component types in the **All** category.
* **Any** — the archetype must contain at least one of the component types in the **Any** category.
* **None** — the archetype must not contain any of the component types in the **None** category.

An entity query provides a list of the chunks that contain the types of components the query requires. You can then iterate over the components in those chunks directly with [IJobChunk](chunk_iteration_job.md). 

## Jobs

To take advantage of multiple threads, you can use the [C# Job system]. ECS provides the [SystemBase](xref:Unity.Entites.SystemBase) class, along with the `Entities.ForEach` and [IJobChunk](chunk_iteration_job.md) `Schedule()` and `ScheduleParallel()` methods, to transform data outside the main thread. `Entities.ForEach` is the simplest to use and typically requires fewer lines of code to implement. You can use IJobChunk for more complex situations that `Entities.ForEach` does not handle.

ECS schedules jobs on the main thread in the [order that your systems are arranged](#system-organization). As jobs are scheduled, ECS keeps track of which jobs read and write which components. A job that reads a component is dependent on any prior scheduled job that writes to the same component and vice versa. The job scheduler uses job dependencies to determine which jobs it can run in parallel and which must run in sequence.  

<a name="system-organization"></a>
## System organization

ECS organizes systems by [World](xref:Unity.Entities.World) and then by [group](xref:Unity.Enties.ComponentSystemGroup). By default, ECS creates a default World with a predefined set of groups. It finds all available systems, instantiates them, and adds them to the predefined [simulation group](xref:Unity.Entities.SimulationSystemGroup) in the default World.

You can specify the update order of systems within the same group. A group is a kind of system, so you can add a group to another group and specify its order just like any other system. All systems within a group update before the next system or group. If you do not specify an order, ECS inserts systems into the update order in a deterministic way that does not depend on creation order. In other words, the same set of systems always updates in the same order within their group even when you don't explicitly specify an order. [Entity component buffer systems](xref:Unity.Entities.EntityComponentBufferSystem) 

System updates happen on the main thread. However, systems can use jobs to offload work to other threads. [SystemBase](xref:Unity.entities.SystemBase) provide a straightforward way to create and schedule Jobs. 

For more information about system creation, update order, and the attributes you can use to organize your systems, see the documentation on [System Update Order](system_update_order.md) .

## ECS authoring

When you create your game or application in the Unity Editor, you can use GameObjects and MonoBehaviours to create a conversion system to map those UnityEngine objects and components to entities. For more information, see [Creating Gameplay](gp_overview.md).

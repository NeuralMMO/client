---
uid: ecs-components
---
# Components

Components are one of the three principle elements of an Entity Component System architecture. They represent the data of your game or application. [Entities](ecs_entities.md) are identifiers that index your collections of components, while [systems](ecs_systems.md) provide the behavior. 

A component in ECS is a struct that has one of the following "marker interfaces":

* [IComponentData](xref:Unity.Entities.IComponentData) — Use for [general purpose](xref:ecs-component-data) and [chunk components](xref:ecs-chunk-component-data).
* [IBufferElementData](xref:Unity.Entities.IBufferelementData) — Associates  [dynamic buffers](xref:ecs-dynamic-buffers) with an entity.
* [ISharedComponentData](xref:Unity.Entities.ISharedComponentData) — Categorizes or groups entities by value within an archetype. For more information, see [Shared Component Data](xref:ecs-shared-component-data).
* [ISystemStateComponentData](xref:Unity.Entities.ISystemStateComponentData) — Associates a system-specific state with an entity and detects when individual entities are created or destroyed. For more information, see [System State Components](xref:ecs-system-state-component-data).
* [ISharedSystemStateComponentData](xref:Unity.Entities.ISharedSystemStateComponentData) — a combination of shared and system state data. See [System State Components](xref:ecs-system-state-component-data).
* [Blob assets](xref:ecs-blob-asset-data) – While not technically a "component," you can use blob assets to store data. Blob assets can be referenced by one or more components using a [BlobAssetReference](xref:Unity.Entities.BlobAssetReference) and are immutable. You can use blob assets to share data between assets and access that data in C# jobs.
 
The EntityManager organizes unique combinations of components into **archetypes**. It stores the components of all entities with the same archetype together in blocks of memory called **chunks**. The entities in a given chunk all have the same component archetype.

![](images/ArchetypeChunkDiagram.png)

This diagram illustrates how ECS stores component data chunks by their archetypes. Shared components and chunk components are exceptions because ECS stores them outside of the chunk. A single instance of these component types apply to all of the entities in the applicable chunks. Additionally, you can optionally store dynamic buffers outside of the chunk. Even though ECS does not store these types of components inside of the chunk, you can generally treat them the same as other component types when querying for entities.
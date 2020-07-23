---
uid: ecs-iteration
---
# Accessing entity data

Iterating over your data is one of the most common tasks you need to perform when you implement ECS systems. The ECS systems typically process a set of entities, reads data from one or more components, performs a calculation, and then writes the result to another component.

The most efficient way to iterate over entities and components is in a parallelizable job that processes the components in order. This takes advantage of the processing power from all available cores and data locality to avoid CPU cache misses. 

The ECS API provides a number of ways to accomplish iteration, each with its own performance implications and restrictions. You can iterate over ECS data in the following ways:

* [SystemBase.Entities.ForEach] — the simplest efficient way to process component data entity by entity.

* [IJobChunk] — iterates over the eligible blocks of memory (called a **[chunk]**) that contain matching entities. The job `Execute()` function can use a for loop to iterate over the elements inside each chunk. You can use [IJobChunk] for more complex situations than [Entities.ForEach] supports, while maintaining maximum efficiency. 

* [Manual iteration] — if the previous methods are insufficient, you can manually iterate over entities or Chunks. For example, you can use a job such as `IJobParallelFor` to get a `NativeArray` that contains entities or the Chunks of the entities that you want to process and iterate over.

The [EntityQuery] class provides a way to construct a view of your data that contains only the specific data you need for a given algorithm or process. Many of the iteration methods in the list above use an [EntityQuery], either explicitly or internally.

**Important:** The following iteration types should not be used in new code: 

* IJobForEach
* IJobForEachWithEntity
* ComponentSystem
* JobComponentSystem

These types are being phased out in preference to [SystemBase] and will become obsolete once they have gone through a deprecation cycle. Use [SystemBase] and [SystemBase.Entities.ForEach] or [IJobChunk] to replace them.


[SystemBase]: entities_job_foreach.md
[Entities.ForEach]: entities_job_foreach.md
[SystemBase.Entities.ForEach]: entities_job_foreach.md
[IJobChunk]: chunk_iteration_job.md
[EntityQuery]: ecs_entity_query.md
[Chunk]: xref:Unity.Entities.ArchetypeChunk
---
uid: ecs-ijobchunk
---

# Using IJobChunk jobs

You can implement [IJobChunk](xref:Unity.Entities.IJobChunk) inside a system to iterate through your data by chunk. When you schedule an IJobChunk job in the `OnUpdate()` function of a system, the job invokes your `Execute()` function once for each chunk that matches the entity query passed to the job's `Schedule()` method. You can then iterate over the data inside each chunk, entity by entity.

Iterating with IJobChunk requires more code setup than does Entities.ForEach, but is also more explicit and represents the most direct access to the data, as it is actually stored. 

Another benefit of iterating by chunks is that you can check whether an optional component is present in each chunk with `Archetype.Has<T>()`, and then process all of the entities in the chunk accordingly.

To implement an IJobChunk job, use the following steps:

1. Create an `EntityQuery` to identify the entities that you want to process.
2. Define the job struct, and include fields for `ArchetypeChunkComponentType` objects that identify the types of components the job must directly access. Also, specify whether the job reads or writes to those components.
3. Instantiate the job struct and schedule the job in the system `OnUpdate()` function.
4. In the `Execute()` function, get the `NativeArray` instances for the components the job reads or writes and then iterate over the current chunk to perform the desired work.

For more information, the [ECS samples repository](https://github.com/Unity-Technologies/EntityComponentSystemSamples) contains a simple HelloCube example that demonstrates how to use `IJobChunk`.

<a name="query"></a>

## Query for data with a EntityQuery

An EntityQuery defines the set of component types that an archetype must contain for the system to process its associated chunks and entities. An archetype can have additional components, but it must have at least those that the EntityQuery defines. You can also exclude archetypes that contain specific types of components.  

For simple queries, you can use the `SystemBase.GetEntityQuery()` function and pass in the component types as follows:

[!code-cs[system](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#rotationspeedsystem)]

For more complex situations, you can use an `EntityQueryDesc`. An `EntityQueryDesc` provides a flexible query mechanism to specify the component types:

* `All`: All component types in this array must exist in the archetype
* `Any`: At least one of the component types in this array must exist in the archetype
* `None`: None of the component types in this array can exist in the archetype

For example, the following query includes archetypes that contain the `RotationQuaternion` and `RotationSpeed` components, but excludes any archetypes that contain the `Frozen` component:

[!code-cs[oncreate2](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#oncreate2)]

The query uses `ComponentType.ReadOnly<T>` instead of the simpler `typeof` expression to designate that the system does not write to `RotationSpeed`.

You can also combine multiple queries. To do this, pass an array of `EntityQueryDesc` objects rather than a single instance. ECS uses a logical OR operation to combine each query. The following example selects any archetypes that contain a `RotationQuaternion` component or a `RotationSpeed` component (or both):

[!code-cs[oncreate3](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#oncreate3)]

**Note:** Do not include completely optional components in the `EntityQueryDesc`. To handle optional components, use the `chunk.Has<T>()` method inside `IJobChunk.Execute()` to determine whether the current ArchetypeChunk has the optional component or not. Because all entities in the same chunk have the same components, you only need to check whether an optional component exists once per chunk: not once per entity.

For efficiency and to avoid needless creation of garbage-collected reference types, you should create the `EntityQueries` for a system in the system’s `OnCreate()` function and store the result in an instance variable. (In the above examples, the `m_Query` variable is used for this purpose.)

<a name="define-job-struct"></a>

## Define the IJobChunk struct

The IJobChunk struct defines fields for the data the job needs when it runs, as well as the job’s `Execute()` method.

To access the component arrays inside of the chunks that the system passes to your `Execute()` method, you must create an `ArchetypeChunkComponentType<T>` object for each type of component that the job reads or writes to. You can use these objects to get instances of the `NativeArray`s that provide access to the components of an entity. Include all of the components referenced in the job’s EntityQuery that the `Execute()` method reads or writes. You can also provide `ArchetypeChunkComponentType` variables for optional component types that you do not include in the EntityQuery. 

You must check to make sure that the current chunk has an optional component before you try to access it. For example, the HelloCube IJobChunk example declares a job struct that defines `ArchetypeChunkComponentType<T>` variables for two components; `RotationQuaternion` and `RotationSpeed`:

[!code-cs[speedjob](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#speedjob)]

The system assigns values to these variables in the `OnUpdate()` function. ECS uses the variables inside the `Execute()` method when it runs the job.

The job also uses the Unity delta time to animate the rotation of a 3D object. The example uses a struct field to pass this value to the `Execute()` method.  

<a name="execute"></a>

## Writing the Execute method

The signature of the IJobChunk `Execute()` method is:

[!code-cs[speedjob](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#execsignature)]

The `chunk` parameter is a handle to the block of memory that contains the entities and components that this iteration of the job has to process. Because a chunk can only contain a single archetype, all of the entities in a chunk have the same set of components. 

Use the `chunk` parameter to get the NativeArray instances for components:

[!code-cs[getcomponents](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#getcomponents)]

These arrays are aligned so that an entity has the same index in all of them. You can then use a normal for loop to iterate through the component arrays. Use `chunk.Count` to get the number of entities stored in the current chunk:

[!code-cs[chunkiteration](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#chunkiteration)]

If you have the `Any` filter in your EntityQueryDesc or have completely optional components that don’t appear in the query at all, you can use the `ArchetypeChunk.Has<T>()` function to test whether the current chunk contains one of those components before you use it:

    if (chunk.Has<OptionalComp>(OptionalCompType))
    {//...}

__Note:__ If you use a concurrent entity command buffer, pass the `chunkIndex` argument as the `jobIndex` parameter to the command buffer functions.

<a name="filtering"></a>

## Skipping chunks with unchanged entities

If you only need to update entities when a component value has changed, you can add that component type to the change filter of the EntityQuery that selects the entities and chunks for the job. For example, if you have a system that reads two components and only needs to update a third when one of the first two has changed, you can use a EntityQuery as follows:

[!code-cs[changefilter](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#changefilter)]

The EntityQuery change filter supports up to two components. If you want to check more or you aren't using a EntityQuery, you can make the check manually. To make this check, use the `ArchetypeChunk.DidChange()` function to compare the chunk’s change version for the component to the system's `LastSystemVersion`. If this function returns false, you can skip the current chunk altogether because none of the components of that type have changed since the last time the system ran. 

You must use a struct field to pass the `LastSystemVersion` from the system into the job, as follows:

[!code-cs[changefilterjobstruct](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#changefilterjobstruct)]

As with all the job struct fields, you must assign its value before you schedule the job:

[!code-cs[changefilteronupdate](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#changefilteronupdate)]

**Note:** For efficiency, the change version applies to whole chunks not individual entities. If another job which has the ability to write to that type of component accesses a chunk, then ECS increments the change version for that component and the `DidChange()` function returns true. ECS increments the change version even if the job that declares write access to a component does not actually change the component value. 

<a name="schedule"></a>
## Instantiate and schedule the job

To run an IJobChunk job, you must create an instance of your job struct, setting the struct fields, and then schedule the job. When you do this in the `OnUpdate()` function of a SystemBase implementation, the system schedules the job to run every frame.

[!code-cs[schedulequery](../package/DocCodeSamples.Tests/ChunkIterationJob.cs#schedulequery)]

When you call the `GetArchetypeChunkComponentType<T>()` function to set your component type variables, make sure that you set the `isReadOnly` parameter to true for components that the job reads, but doesn’t write. Setting these parameters correctly can have a significant impact on how efficiently the ECS framework can schedule your jobs. These access mode settings must match their equivalents in both the struct definition, and the EntityQuery. 

Do not cache the return value of `GetArchetypeChunkComponentType<T>()` in a system class variable. You must call the function every time the system runs, and pass the updated value to the job.

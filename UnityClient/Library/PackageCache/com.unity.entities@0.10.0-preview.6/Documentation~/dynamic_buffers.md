---
uid: ecs-dynamic-buffers
---

# Dynamic buffer components

Use dynamic buffer components to associate array-like data with an entity. Dynamic buffers are ECS components that can hold a variable number of elements, and automatically resize as necessary. 

To create a dynamic buffer, first declare a struct that implements [IBufferElementData](xref:Unity.Entities.IBufferElementData) and defines the elements stored in the buffer. For example, you can use the following struct for a buffer component that stores integers:

[!code-cs[declare-element](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#declare-element)]

To associate a dynamic buffer with an entity, add an [IBufferElementData](xref:Unity.Entities.IBufferElementData) component directly to the entity rather than adding the [dynamic buffer container](xref:Unity.Entities.DynamicBuffer`1) itself. 

ECS manages the container. For most purposes, you can use a declared `IBufferElementData` type to treat a dynamic buffer the same as any other ECS component. For example, you can use the `IBufferElementData` type in [entity queries](xref:Unity.Entities.EntityQuery) as well as when you add or remove the buffer component. However, you must use different functions to access a buffer component and those functions provide the [DynamicBuffer](xref:Unity.Entities.DynamicBuffer`1) instance, which gives an array-like interface to the buffer data.

To specify an “internal capacity" for a dynamic buffer component, use the [InternalBufferCapacity attribute](xref:Unity.Entities.InternalBufferCapacityAttribute). The internal capacity defines the number of elements the dynamic buffer stores in the [ArchetypeChunk](xref:Unity.Entities.ArchetypeChunk) along with the other components of an entity. If you increase the size of a buffer beyond the internal capacity, the buffer allocates a heap memory block outside the current chunk and moves all existing elements. ECS manages this external buffer memory automatically, and frees the memory when the buffer component is removed. 

**Note:** If the data in a buffer is not dynamic, you can use a [blob asset](xref:Unity.Entities.BlobBuilder) instead of a dynamic buffer. Blob assets can store structured data, including arrays. Multiple entities can share blob assets.
 
## Declaring buffer element types

To declare a buffer, declare a struct that defines the type of element that you want to put into the buffer. The struct must implement [IBufferElementData](xref:Unity.Entities.IBufferElementData), like so:

[!code-cs[declare-element-full](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#declare-element-full)]


## Adding buffer types to entities

To add a buffer to an entity, add the `IBufferElementData` struct that defines the data type of the buffer element, and then add that type directly to an entity or to an [archetype](xref:Unity.Entities.EntityArchetype):

### Using EntityManager.AddBuffer()

For more information, see the documentation on [EntityManager.AddBuffer()](xref:Unity.Enities.EntityManager.AddBuffer`1(Unity.Entities.Entity)).

[!code-cs[declare](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#add-with-manager)]

### Using an archetype

[!code-cs[declare](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#add-with-archetype)]

### Using the `[GenerateAuthoringComponent]` attribute

You can use `[GenerateAuthoringComponent]`to generate authoring components for simple IBufferElementData implementations that contain only one field. Setting this attribute allows you add an ECS IBufferElementData component to a GameObject so that you can set the buffer elements in the Editor.  

For example, if you declare the following type, you can add it directly to a GameObject in the Editor:

```
[GenerateAuthoringComponent]
public struct IntBufferElement: IBufferElementData
{
    public int Value;
}
```

In the background, Unity generates a class named `IntBufferElementAuthoring` (which inherits from `MonoBehaviour`), which exposes a public field of `List<int>` type. When the GameObject containing this generated authoring component is converted into an entity, the list is converted into `DynamicBuffer<IntBufferElement>`, and then added to the converted entity.

Note the following restrictions:
- Only one component in a single C# file can have a generated authoring component, and the C# file must not have another MonoBehaviour in it.
- `IBufferElementData` authoring components cannot be automatically generated for types that contain more than one field.
- `IBufferElementData` authoring components cannot be automatically generated for types that have an explicit layout.

### Using an [EntityCommandBuffer](xref:Unity.Entities.EntityCommandBuffer)

You can add or set a buffer component when you add commands to an entity command buffer. 

Use [AddBuffer](xref:Unity.Entities.EntityCommandBuffer.AddBuffer``1(Unity.Entities.Entity)) to create a new buffer for the entity, which changes the entity's archetype. Use [SetBuffer](xref:Unity.Entities.EntityCommandBuffer.SetBuffer``1(Unity.Entities.Entity)) to wipe out the existing buffer (which must exist) and create a new, empty buffer in its place. Both functions return a [DynamicBuffer](xref:Unity.Entities.DynamicBuffer`1) instance that you can use to populate the new buffer. You can add elements to the buffer immediately, but they are not otherwise accessible until the buffer is added to the entity when the command buffer is executed.

The following job creates a new entity using a command buffer and then adds a dynamic buffer component using [EntityCommandBuffer.AddBuffer](xref:Unity.Entities.EntityCommandBuffer.AddBuffer``1(Unity.Entities.Entity)). The job also adds a number of elements to the dynamic buffer. 

[!code-cs[declare](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#add-in-job)]

**Note:** You are not required to add data to the dynamic buffer immediately. However, you won't have access to the buffer again until after the entity command buffer you are using is executed.

## Accessing buffers

You can use [EntityManager](xref:Unity.Entities.EntityManager), [systems](ecs_systems.md), and jobs to access the [DynamicBuffer](xref:Unity.Entities.DynamicBuffer`1) instance in much the same way as you access other component types of entities. 

### EntityManager

You can use an instance of the [EntityManager](xref:Unity.Entities.EntityManager) to access a dynamic buffer:

[!code-cs[declare](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#access-manager)]

### Looking up buffers of another entity

When you need to look up the buffer data belonging to another entity in a job, you can pass a [BufferFromEntity](xref:Unity.Entities.BufferFromEntity`1) variable to the job.

[!code-cs[declare](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#lookup-snippet)]

### SystemBase Entities.ForEach

You can access dynamic buffers associated with the entities you process with Entities.ForEach by passing the buffer as one of your lambda function parameters. The following example adds all the values stored in the buffers of type, `MyBufferElement`:

[!code-cs[access-buffer-system](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#access-buffer-system)]

Note that we can write directly to the captured `sum` variable in this example because we execute the code with `Run()`. If we scheduled the function to run in a job, we could only write to a native container such as NativeArray, even though the result is a single value.

### IJobChunk

To access an individual buffer in an `IJobChunk` job, pass the buffer data type to the job and use that to get a [BufferAccessor](xref:Unity.Entities.BufferAccessor`1). A buffer accessor is an array-like structure that provides access to all of the dynamic buffers in the current chunk. 

Like the previous example, the following example adds up the contents of all dynamic buffers that contain elements of type, `MyBufferElement`. `IJobChunk` jobs can also run in parallel on each chunk, so in the example, it first stores the intermediate sum for each buffer in a native array and then uses a second job to calculate the final sum. In this case, the intermediate array holds one result for each chunk, rather than one result for each entity.

[!code-cs[declare](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#access-chunk-job)]

## Reinterpreting buffers

Buffers can be reinterpreted as a type of the same size. The intention is to
allow controlled type-punning and to get rid of the wrapper element types when
they get in the way. To reinterpret, call [Reinterpret&lt;T&gt;](xref:Unity.Entities.DynamicBuffer`1.Reinterpret*):

[!code-cs[declare](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#reinterpret-snippet)]

The reinterpreted buffer instance retains the safety handle of the original
buffer, and is safe to use. Reinterpreted buffers reference original data, so
modifications to one reinterpreted buffer are immediately reflected in
others.

**Note:** The reinterpret function only enforces that the types involved have the same length. For example, you can alias a `uint` and `float` buffer without raising an error because both types are 32-bits long. You must make sure that the reinterpretation makes sense logically.

## Buffer reference invalidation
Every [structural change](sync_points.md#structural-changes) invalidates all references to dynamic buffers. Structural changes generally cause entities to move from one chunk to another. Small dynamic buffers can reference memory within a chunk (as opposed to from main memory) and therefore, they need to be reacquired after a structural change.

[!code-cs[declare](../package/DocCodeSamples.Tests/DynamicBufferExamples.cs#invalidation)]

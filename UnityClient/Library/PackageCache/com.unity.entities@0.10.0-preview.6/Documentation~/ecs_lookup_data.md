---
uid: ecs-data-lookup
---

# Looking up data

The most efficient way to access and modify your ECS data is to use a system with an entity query and job. This provides the best utilization of CPU resources with the fewest memory cache misses. In fact, one of the goals of your data design should be to perform the bulk of your data transformation using the most efficient, fastest path. However, sometimes you need to access an arbitrary component of an arbitrary entity at an arbitrary point in your program.

Given an Entity object, you can look up data in its [IComponentData] and [dynamic buffers]. The method varies depending on whether your code executes in a system using [Entities.ForEach] or using an [IJobChunk] job, or elsewhere on the main thread.

## Looking up entity data in a system

Use [GetComponent&lt;T&gt;(Entity)] to look up data stored in a component of an arbitrary entity from inside a system's [Entities.ForEach] or [Job.WithCode] function.

For example, if you have Target components with an Entity field identifying the entity to target, you can use the following code to rotate the tracking entities toward their target:

[!code-cs[lookup-foreach](../package/DocCodeSamples.Tests/LookupDataExamples.cs#lookup-foreach)]

Accessing data stored in [dynamic buffers] requires an extra step. You must declare a local variable of type [BufferFromEntity] in your [OnUpdate()] method. You can then "capture" the local variable in your lambda function. 

[!code-cs[lookup-foreach-buffer](../package/DocCodeSamples.Tests/LookupDataExamples.cs#lookup-foreach-buffer)]


## Looking up entity data in IJobChunk

To randomly access component data in an IJobChunk or other job struct, use one of the following types to get an array-like interface to component, indexed by [Entity] object:

* [ComponentDataFromEntity]
* [BufferFromEntity]

Declare a field of type [ComponentDataFromEntity] or [BufferFromEntity], and set the value of the field before scheduling the job.

For example, if you had Target components with an Entity field identifying the entities to target, you could add the following field to your job struct to look up the world position of the targets:

[!code-cs[lookup-ijobchunk-declare](../package/DocCodeSamples.Tests/LookupDataExamples.cs#lookup-ijobchunk-declare)]

Note that this declaration uses the [ReadOnly] attribute. You should always declare ComponentDataFromEntity<T> objects as read-only unless you do write to the components you access.
    
You can set this field when scheduling the job as follows:

[!code-cs[lookup-ijobchunk-set](../package/DocCodeSamples.Tests/LookupDataExamples.cs#lookup-ijobchunk-set)]

Inside the job's `Execute()` function, you can lookup the value of a component using an Entity object:

[!code-cs[lookup-ijobchunk-read](../package/DocCodeSamples.Tests/LookupDataExamples.cs#lookup-ijobchunk-read)]
  
 The following, full example shows a system that moves entities that have a Target field containing the Entity object of their target towards the current location of the target:
 
[!code-cs[lookup-ijobchunk](../package/DocCodeSamples.Tests/LookupDataExamples.cs#lookup-ijobchunk)]

## Data access errors

If the data you are looking up overlaps the data you are directly reading and writing in the job, then random access can lead to race conditions and subtle bugs. When you are sure that there is no overlap between the specific entity data you are reading or writing directly in the job and the specific entity data you are reading or writing randomly, then you can mark the accessor object with the [NativeDisableParallelForRestriction] attribute. 

[dynamic buffers]: xref:ecs-dynamic-buffers
[GetComponent&lt;T&gt;(Entity)]: xref:Unity.Entities.SystemBase.GetComponent``1(Unity.Entities.Entity)
[Entity]: xref:Unity.Entities.Entity
[ComponentDataFromEntity]: xref:Unity.Entities.ComponentDataFromEntity`1
[BufferFromEntity]: xref:Unity.Entities.BufferFromEntity`1 
[Why ECS]: http://www.example.com#need-to-add-this-page
[IComponentData]: xref:ecs-component-data
[dynamic buffers]: xref:ecs-dynamic-buffers
[Entities.ForEach]: xref:Unity.Entities.SystemBase.Entities
[OnUpdate()]: xref:Unity.Entities.SystemBase.OnUpdate*
[IJobChunk]: xref:Unity.Entities.IJobChunk
[ReadOnly]: https://docs.unity3d.com/ScriptReference/Unity.Collections.ReadOnlyAttribute.html
[NativeDisableParallelForRestriction]: https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeDisableParallelForRestrictionAttribute.html
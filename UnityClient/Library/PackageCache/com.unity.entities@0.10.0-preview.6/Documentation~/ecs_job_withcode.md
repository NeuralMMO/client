---
uid: ecs-job-withcode
---

# Using Job.WithCode

The [Job.WithCode] construction provided by the [SystemBase] class is an easy way to run a function as a single background job. You can also run [Job.WithCode] on the main thread and still take advantage of [Burst] compilation to speed up execution.

The following example  uses one [Job.WithCode] lambda function to fill a [native array] with random numbers and another job to add those numbers together:

[!code-cs[job-with-code-example](../package/DocCodeSamples.Tests/LambdaJobExamples.cs#job-with-code-example)]

**Note:** To run a parallel job, implement [IJobFor], which you can schedule using [ScheduleParallel()] in the system [OnUpdate()] function.

## Variables

You cannot pass parameters to the [Job.WithCode] lambda function or return a value. Instead, you can capture local variables in your [OnUpdate()] function. 

When you schedule your job to run in the [C# Job System] using `Schedule()`, there are additional restrictions:

* Captured variables must be declared as  [NativeArray] -- or other [native container] -- or a [blittable] type.  
* To return data,  you must write the return value to a captured [native array], even if the data is a single value. (Note that you can write to any captured variable when executing with `Run()`.)

[Job.WithCode] provides a set of functions to apply read-only and safety attributes to your captured [native container] variables. For example, you can use `WithReadOnly` to designate that you don't update the container and `WithDeallocateOnJobCompletion` to automatically dispose a container after the job finshes. ([Entities.ForEach] provides the same functions.)

See [Job.WithCode] for more information about these modifiers and attributes.
 
## Executing the function

You have two options to execute your lambda function:
* `Schedule()` -- executes the function as a single, non-parallel job.
   Scheduling a job runs the code on a background thread and thus can take better advantage of available CPU resources. 
* `Run()` -- executes the function immediately on the main thread.
   In most cases the [Job.WithCode] can be [Burst] compiled so executing code can be faster inside [Job.WithCode] even though it is still run on the main thread.

Note that calling `Run()` automatically completes all the dependencies of the [Job.WithCode] construction. If you do not explicitly pass a [JobHandle] object to `Run()` the system assumes that the current [Dependency] property represents the function's dependencies. (Pass in a new [JobHandle] if the function has no dependencies.)

## Dependencies

By default, a system manages its ECS-related dependencies using its [Dependency] property. The system adds each job created with [Entities.ForEach] and [Job.WithCode] to the [Dependency] job handle in the order that they appear in the [OnUpdate()] function. You can also manage job dependencies manually by passing a [JobHandle] to your `Schedule` functions, which then return the resulting dependency. See [Dependency] for more information.
 
See [Job dependencies] for more general information about job dependencies.

 [C# Job System]: https://docs.unity3d.com/Manual/JobSystem.html
[Burst]: https://docs.unity3d.com/Packages/com.unity.burst@latest/index.html
[Dependency]: xref:Unity.Entities.SystemBase.Dependency
[race condition]: https://en.wikipedia.org/wiki/Race_condition
[Job dependencies]: xref:ecs-job-dependencies
[IJobFor]: https://docs.unity3d.com/Manual/JobSystemCreatingJobs.html
[ScheduleParallel()]: https://docs.unity3d.com/ScriptReference/Unity.Jobs.IJobForExtensions.ScheduleParallel.html
[OnUpdate()]: xref:Unity.Entities.SystemBase.OnUpdate*
[blittable]: https://docs.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types
[sync point]: xref:sync-point
[JobHandle]: https://docs.unity3d.com/ScriptReference/Unity.Jobs.JobHandle.html
[Job.WithCode]: xref:Unity.Entities.SystemBase.Job
[Entities.ForEach]: xref:Unity.Entities.SystemBase.Entities
[SystemBase.Entities]: xref:Unity.Entities.SystemBase.Entities
[SystemBase]: xref:Unity.Entities.SystemBase
[NativeArray]: https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html
[native array]: https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html
[native container]: https://docs.unity3d.com/Manual/JobSystemNativeContainer.html
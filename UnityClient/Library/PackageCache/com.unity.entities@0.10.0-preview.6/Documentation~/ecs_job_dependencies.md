---
uid: ecs-job-dependencies
---

# Job dependencies

Unity analyzes the data dependencies of each system based on the ECS components that the system reads and writes. If a system that updates earlier in the frame reads data that a later system writes, or writes data that a later system reads, then the second system depends on the first. To prevent [race conditions], the job scheduler makes sure that all the jobs a system depends on have finished before it runs that system's jobs. 

A system's [Dependency] property is a [JobHandle] that represents the ECS-related dependencies of the system. Before [OnUpdate()], the [Dependency] property reflects the incoming dependencies that the system has on prior jobs. By default, the system updates the [Dependency] property based on the components each job reads and writes as you schedule jobs in a system. 

To override this default behavior, use the overloaded versions of [Entities.ForEach] and [Job.WithCode] that take job dependencies as an parameter and return the updated dependencies as a [JobHandle]. When you use the explicit versions of these constructions, ECS does not automatically combine the job handles with the system's [Dependency] property. You must combine them manually when required. 

Note that the system [Dependency] property does not track the dependencies that a job might have on data passed through [NativeArrays] or other similar containers. If you write a NativeArray in one job, and read that array in another, you must manually add the JobHandle of the first job as a dependency of the second (typically by using [JobHandle.CombineDependencies]).

When you call [Entities.ForEach.Run()] the job scheduler completes all scheduled jobs that the system depends on before starting the ForEach iteration. If you also use [WithStructuralChanges()] as part of the construction, then the job scheduler completes all running and scheduled jobs. Structural changes also invalidate any direct references to component data. See [sync points] for more information.

See [JobHandle and dependencies] for more information.

[Dependency]: xref:Unity.Entities.SystemBase.Dependency
[race conditions]: https://en.wikipedia.org/wiki/Race_condition
[IJobParallelFor]: https://docs.unity3d.com/Manual/JobSystemParallelForJobs.html
[OnUpdate()]: xref:Unity.Entities.SystemBase.OnUpdate*
[JobHandle]: https://docs.unity3d.com/ScriptReference/Unity.Jobs.JobHandle.html
[NativeArrays]: https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html
[C# Job System]: https://docs.unity3d.com/Manual/JobSystem.html
[WithStructuralChanges()]: xref:Unity.Entities.SystemBase.Entities
[Entities.ForEach.Run()]: xref:Unity.Entities.SystemBase.Entities
[Entities.ForEach]: xref:Unity.Entities.SystemBase.Entities
[JobHandle.CombineDependencies]: https://docs.unity3d.com/ScriptReference/Unity.Jobs.JobHandle.CombineDependencies.html
[Job.WithCode]: xref:Unity.Entities.SystemBase.Job
[sync points]: xref:ecs-sync-points
[JobHandle and dependencies]: https://docs.unity3d.com/Manual/JobSystemJobDependencies.html





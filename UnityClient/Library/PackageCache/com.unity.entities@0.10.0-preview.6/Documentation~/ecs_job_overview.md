---
uid: ecs-jobs
---
# Jobs in ECS

ECS uses the [C# Job System] extensively. Whenever possible, you should use the jobs in your system code. The [SystemBase] class provides [Entities.ForEach] and [Job.WithCode] to help implement your game logic as multithreaded code. In more complex situations, you can use [IJobChunk].

For example, the following system updates positions:

    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Transforms;
    
    public class MovementSpeedSystem : SystemBase
    {
        // OnUpdate runs on the main thread.
        protected override void OnUpdate()
        {
            Entities
                .ForEach((ref Translation position, in MovementSpeed speed) =>
                    {
                        float3 displacement = speed.Value * dt;
                        position = new Translation(){
                                Value = position.Value + displacement
                            };
                    })
                .ScheduleParallel();
        }
    }


For more information about systems, see [ECS Systems](ecs_systems.md).

[C# Job System]: https://docs.unity3d.com/Manual/JobSystem.html
[SystemBase]: xref:Unity.Entities.SystemBase 
[Entities.ForEach]: ecs_entities_foreach.md
[Job.WithCode]: ecs_job_withcode.md
[IJobChunk]: chunk_iteration_job.md
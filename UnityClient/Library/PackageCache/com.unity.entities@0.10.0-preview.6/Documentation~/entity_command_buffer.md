---
uid: ecs-entity-command-buffer
---
# Entity Command Buffers

The [`EntityCommandBuffer`](xref:Unity.Entities.EntityCommandBuffer) (ECB) class solves two important problems:

1. When you're in a job, you can't access the [`EntityManager`](xref:Unity.Entities.EntityManager).
2. When you perform a [structural change](sync_points.md) (like creating an entity), you create a [sync point](sync_points.md) and must wait for all jobs to complete.

The `EntityCommandBuffer` abstraction allows you to queue up changes (from either a job or from the main thread) so that they can take effect later on the main thread.

## Entity command buffer systems
[Entity command buffer systems](xref:Unity.Entities.EntityCommandBufferSystem) allow you to play back the commands queued up in ECBs at a clearly defined point in a frame. These systems are usually the best way to use ECBs. You can acquire multiple ECBs from the same entity command buffer system and the system will play back all of them in the order they were created when it is updated. This creates a single sync point when the system is updated instead of one sync point per ECB and ensures determinism.

The default World initialization provides three system groups, for initialization, simulation, and presentation, that are updated in order each frame. Within a group, there is an entity command buffer system that runs before any other system in the group and another that runs after all other systems in the group. Preferably, you should use one of the existing command buffer systems rather than creating your own in order to minimize synchronization points. See [Default System Groups](system_update_order.md) for a list of the default groups and command buffer systems.

If you want to use an ECB from a parallel job (e.g. in an `Entities.ForEach`), you must ensure that you convert it to a concurrent ECB first by calling `ToConcurrent` on it. To ensure that the sequence of the commands in the ECB does not depend on how the work is distributed across jobs, you must also pass the index of the entity in the current query to each operation.

You can acquire and use an ECB like this:

[!code-cs[ecb_concurrent](../package/DocCodeSamples.Tests/EntityCommandBuffers.cs#ecb_concurrent)]
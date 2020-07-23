---
uid: sync-points
---

# Sync points

A synchronization point (sync point) is a point in program execution that waits for the completion of all jobs that have been scheduled so far. Sync points limit your ability to use all worker threads available in the job system for a period of time. As such, you should generally aim to avoid sync points.

## Structural changes

Sync points are caused by operations that you cannot safely perform when there are any other jobs that operate on components. Structural changes to the data in ECS are the primary cause of sync points. All of the following are structural changes:
 
 * Creating entities
 * Deleting entities
 * Adding components to an entity
 * Removing components from an entity
 * Changing the value of shared components

Broadly speaking, any operation that changes the archetype of an entity or causes the order of entities within a chunk to change is a structural change. These structural changes can only be performed on the main thread.

Structural changes not only require a sync point, but they also invalidate all direct references to any component data. This includes instances of [DynamicBuffer] and the result of methods that provide direct access to the components such as [ComponentSystemBase.GetComponentDataFromEntity].

## Avoiding sync points

You can use [entity command buffers](xref:ecs-entity-command-buffer) (ECBs) to queue up structural changes instead of immediately performing them. Commands stored in an ECB can be played back at a later point during the frame. This reduces multiple sync points spread across the frame to a single sync point when the ECB is played back.

 Each of the standard [ComponentSystemGroup] instances provides a [EntityCommandBufferSystem] as the first and last systems
 updated in the group. By getting an [ECB] object from one of these standard ECB systems, all structural changes within the group occur at the
 same point in the frame, resulting in one sync point rather than several. ECBs also allow you to record structural changes within a job.
 Without an ECB, you can only make structural changes on the main thread. (Even on the main thread, it is typically faster to record
 commands in an ECB and then play back those commands, than it is to make the structural changes one-by-one using the [EntityManager] class itself.)

 If you cannot use an [EntityCommandBufferSystem] for a task, try to group any systems that make structural changes together in the
 system execution order. Two systems that both make structural changes only incur one sync point if they update sequentially.
 
 See [Entity Command Buffers] for more information about using command buffers and command buffer systems.

[EntityManager]: xref:Unity.Entites.EntityManager
[ECB]: xref:Unity.Entities.EntityCommandBuffer
[EntityCommandBufferSystem]: xref:Unity.Entities.EntityCommandBuffer
[ComponentSystemGroup]: xref:Unity.Entities.ComponentSystemGroup
[entity command buffers]: entity_command_buffer.md
[DynamicBuffer]: xref:Unity.Entities.DynamicBuffer``1
[ComponentSystemBase.GetComponentDataFromEntity]: xref:Unity.Entities.ComponentSystemBase.GetComponentDataFromEntity*

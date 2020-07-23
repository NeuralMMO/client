---
uid: ecs-version-numbers
---
# Version numbers

Version numbers (also known as generations) detect potential changes. You can use them to implement efficient optimization strategies, such as to skip processing when data hasn't changed since the last frame of the application. It's useful to perform quick version checks on entities to improve the performance of your application.

This page outlines all of the different version numbers ECS uses, and the conditions that causes them to change.

All version numbers are 32-bit signed integers. They always increase unless they wrap around: signed integer overflow is defined behavior in C#. This means that to compare version numbers, you should use the (in)equality operator, not relational operators.

For example, the correct way to check if VersionB is more recent than VersionA is to use the following:

    bool VersionBIsMoreRecent = (VersionB - VersionA) > 0;

There is usually no guarantee how much a version number increases by.

## EntityId.Version

An `EntityId` is made of an index and a version number. Because ECS recycles indices, the version number is increased in `EntityManager` every time the entity is destroyed. If there is a mismatch in the version numbers when an `EntityId` is looked up in `EntityManager`, it means that the entity referred to doesn’t exist anymore.

For example, before you fetch the position of the enemy that a unit is tracking via an `EntityId`, you can call `ComponentDataFromEntity.Exists`. This uses the version number to check if the entity still exists.

## World.Version

ECS increases the version number of a World every time it creates or destroys a manager (i.e. system).

## EntityDataManager.GlobalVersion

`EntityDataManager.GlobalVersion` is increased before every job component system update.

You should use this version number in conjunction with `System.LastSystemVersion`.

## System.LastSystemVersion

`System.LastSystemVersion` takes the value of `EntityDataManager.GlobalVersion` after every job component system update.

You should use this version number in conjunction with `Chunk.ChangeVersion[]`.

## Chunk.ChangeVersion

For each component type in the archetype, this array contains the value of `EntityDataManager.GlobalVersion` at the time the component array was last accessed as writeable within this chunk. This does not guarantee that anything has changed, only that it might have changed.

You can never access shared components as writeable, even if there is a version number stored for those too: it serves no purpose.

When you use the `WithChangeFilter()` function in an `Entities.ForEach` construction, ECS compares the `Chunk.ChangeVersion` for that specific component to `System.LastSystemVersion`, and it only processes chunks whose component arrays have been accessed as writeable after the system last started running.

For example, if the amount of health points of a group of units is guaranteed not to have changed since the previous frame, you can skip checking if those units should update their damage model.

## EntityManager.m_ComponentTypeOrderVersion[]

For each non-shared component type, ECS increases the version number every time an iterator involving that type becomes invalid. In other words, anything that might modify arrays of that type (not instances).

For example, if you have static objects that a particular component identifies, and a per-chunk bounding box, you only need to update those bounding boxes if the type order version changes for that component.

## SharedComponentDataManager.m_SharedComponentVersion[]

These version numbers increase when any structural change happens to the entities stored in a chunk that reference the shared component.

For example, if you keep a count of entities per shared component, you can rely on that version number to only redo each count if the corresponding version number changes.

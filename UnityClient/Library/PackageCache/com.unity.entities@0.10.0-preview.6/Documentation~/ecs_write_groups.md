---
uid: ecs-writegroups
---

# Write groups

A common ECS pattern is for a system to read one set of **input** components and write to another component as its **output**. However, in some cases, you might want to override the output of a system, and use a different system based on a different set of inputs to update the output component. Write groups provide a mechanism for one system to override another, even when you cannot change the other system.

The write group of a target component type consists of all other component types that ECS applies the [`WriteGroup` attribute](xref:Unity.Entities.WriteGroupAttributes) to, with that target component type as the argument. As a system creator, you can use write groups so that your system's users can exclude entities that your system would otherwise select and process. This filtering mechanism lets system users update components for the excluded entities based on their own logic, while letting your system operate normally on the rest.

To make use of write groups, you must use the [write group filter option](xref:Unity.Entities.EntityQueryOptions) on the queries in your system. This excludes all entities from the query that have a component from a write group of any of the components that are marked as writable in the query.

To override a system that uses write groups, mark your own component types as part of the write group of the output component type of that system. The original system ignores any entities that have your components and you can update the data of those entities with your own systems. 

## Write groups example
In this example, you use an external package to color all characters in your game depending on their state of health. For this, there are two components in the package: `HealthComponent` and `ColorComponent`.

```csharp
public struct HealthComponent : IComponentData
{
   public int Value;
}

public struct ColorComponent : IComponentData
{
   public float4 Value;
}
```

Additionally, there are two systems in the package:
 1. The `ComputeColorFromHealthSystem`, which reads from `HealthComponent` and writes to `ColorComponent`
 1. The `RenderWithColorComponent`, which reads from `ColorComponent`

To represent when a player uses a power-up and their character becomes invincible, you attach an `InvincibleTagComponent` to the character's entity. In this case, the character's color should change to a separate, different color, which the above example does not accommodate. 

You can create your own system to override the `ColorComponent` value, but ideally `ComputeColorFromHealthSystem` would not compute the color for your entity to begin with. It should ignore any entity that has `InvincibleTagComponent`. This becomes more relevant when there are thousands of players on the screen. Unfortunately, the system is from another package which does not know about the `InvincibleTagComponent`. This is when a write group is useful. It allows a system to ignore entities in a query when you know that the values it computes would be overridden anyway. There are two things you need to support this:

 1. The `InvincibleTagComponent` must marked as part of the write group of `ColorComponent`:

    ```csharp
    [WriteGroup(typeof(ColorComponent))]
    struct InvincibleTagComponent : IComponentData {}
    ```

    The write group of `ColorComponent` consists of all component types that have the `WriteGroup` attribute with `typeof(ColorComponent)` as the argument.
 1. The `ComputeColorFromHealthSystem` must explicitly support write groups. To achieve this, the system needs to specify the `EntityQueryOptions.FilterWriteGroup` option for all its queries.

You could implement the `ComputeColorFromHealthSystem` like this:

```csharp
...
protected override void OnUpdate() {
   Entities
      .WithName("ComputeColor")
      .WithEntityQueryOptions(EntityQueryOptions.FilterWriteGroup) // support write groups
      .ForEach((ref ColorComponent color, in HealthComponent health) => {
         // compute color here
      }).ScheduleParallel();
}
...
```
When this executes, the following happens:
 1. The system detects that you write to `ColorComponent` because it is a by-reference parameter
 1. It looks up the write group of `ColorComponent` and finds the `InvincibleTagComponent` in it
 1. It excludes all entities that have an `InvincibleTagComponent`

The benefit is that this allows the system to exclude entities based on a type that is unknown to the system and might live in a different package.

**Note:** For more examples, see the `Unity.Transforms` code, which uses write groups for every component it updates, including `LocalToWorld`.

## Creating write groups
To create write groups, add the `WriteGroup` attribute to the declarations of each component type in the write group. The `WriteGroup` attribute takes one parameter, which is the type of component that the components in the group uses to update. A single component can be a member of more than one write group.

For example, if you have a system that writes to component `W` whenever there are components `A` or `B` on an entity, then you can define a write group for `W` as follows:

```csharp
public struct W : IComponentData
{
   public int Value;
}

[WriteGroup(typeof(W))]
public struct A : IComponentData
{
   public int Value;
}

[WriteGroup(typeof(W))]
public struct B : IComponentData
{
   public int Value;
}
```

**Note:** You do not add the target of the write group (component `W` in the example above) to its own write group.

## Enabling write group filtering

To enable write group filtering, set the `FilterWriteGroups` flag on your job:

```csharp
public class AddingSystem : SystemBase
{
   protected override void OnUpdate() {
      Entities
          // support write groups by setting EntityQueryOptions
         .WithEntityQueryOptions(EntityQueryOptions.FilterWriteGroup) 
         .ForEach((ref W w, in B b) => {
            // perform computation here
         }).ScheduleParallel();}
}
```

For query description objects, set the flag when you create the query:

```csharp
public class AddingSystem : SystemBase
{
   private EntityQuery m_Query;

   protected override void OnCreate()
   {
       var queryDescription = new EntityQueryDesc
       {
           All = new ComponentType[] {
              ComponentType.ReadWrite<W>(),
              ComponentType.ReadOnly<B>()
           },
           Options = EntityQueryOptions.FilterWriteGroup
       };
       m_Query = GetEntityQuery(queryDescription);
   }
   // Define IJobChunk struct and schedule...
}
```

When you enable write group filtering in a query, the query adds all components in a write group of a writable component to the `None` list of the query unless you explicitly add them to the `All` or `Any` lists. As a result, the query only selects an entity if it explicitly requires every component on that entity from a particular write group. If an entity has one or more additional components from that write group, the query rejects it.

In the example code above, the query:
 * Excludes any entity that has component `A`, because `W` is writable and `A` is part of the write group of `W`.
 * Does not exclude any entity that has component `B`. Even though `B` is part of the write group of `W`, it is also explicitly specified in the `All` list.

## Overriding another system that uses write groups
If a system uses write group filtering in its queries, you use your own system to override that system and write to those components. To override the system, add your own components to the write groups of the components to which the other system writes. Because write group filtering excludes any components in the write group that the query doesn't explicitly required, the other system ignores any entities that have your components.

For example, if you want to set the orientation of your entities by specifying the angle and axis of rotation, you can create a component and a system to convert the angle and axis values into a quaternion and write that to the `Unity.Transforms.Rotation` component. To prevent the `Unity.Transforms` systems from updating `Rotation`, no matter what other components besides yours are present, you can put your component in the write group of `Rotation`:

```csharp
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[Serializable]
[WriteGroup(typeof(Rotation))]
public struct RotationAngleAxis : IComponentData
{
   public float Angle;
   public float3 Axis;
}
```

You can then update any entities with the `RotationAngleAxis` component without contention:

```csharp
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

public class RotationAngleAxisSystem : SystemBase
{
   protected override void OnUpdate()
   {
      Entities.ForEach((ref Rotation destination, in RotationAngleAxis source) =>
      {
         destination.Value 
             = quaternion.AxisAngle(math.normalize(source.Axis), source.Angle);
      }).ScheduleParallel();
   }
}
```

## Extending another system that uses write groups

If you want to extend another system rather than override it, or if you want to allow future systems to override or extend your system, then you can enable write group filtering on your own system. However, when you do this, neither system handles no combinations of components by default. You must explicitly query for and process each combination.

In the previous example, it defined a write group that contains components `A` and `B` that targets component `W`. If you add a new component, called `C`, to the write group, then the new system that knows about `C` can query for entities that contain `C` and it does not matter if those entities also have components `A` or `B`. However, if the new system also enables write group filtering, that is no longer true. If you only require component `C`, then write group filtering excludes any entities with either `A` or `B`. Instead, you must explicitly query for each combination of components that make sense. **Note:** You can use the `Any` clause of the query when appropriate.

```csharp
var query = new EntityQueryDesc
{
    All = new ComponentType[] {
       ComponentType.ReadOnly<C>(), 
       ComponentType.ReadWrite<W>()
    },
    Any = new ComponentType[] {
       ComponentType.ReadOnly<A>(), 
       ComponentType.ReadOnly<B>()
    },
    Options = EntityQueryOptions.FilterWriteGroup
};
```

If you have any entities that contain combinations of components in the write group that are not explicitly mentioned, then the system that writes to the target of the write group, and its filters, does not handle them. However, if you have any if these type of entities, it is most likely a logical error in the program, and they should not exist.

---
uid: gameplay-common-patterns
---

# Common patterns in gameplay code

## Structuring code with Entities.ForEach

[Entities.ForEach] allows you to write inline jobified code that deals with a set of entities. When organizing your code, it can help to encapsulate functionality into methods and structures. The following patterns provide ways to do this:

* [Static methods]
* [Encapsulate data and methods]

<a name="static-methods"></a>
### Call static methods from an Entities.ForEach

This pattern helps you to re-use functionality in multiple places. It can also help simplify the structure of complex systems and make your code more readable.

You can use a static method as the ForEach lambda function, as illustrated in the following example. A static function called this way is [Burst] compiled (if the function is not Burst-compatible, add [.WithoutBurst()] to the [Entities.ForEach] construction).

```csharp
public class RotationSpeedSystem_ForEach : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        Entities
            .WithName("RotationSpeedSystem_ForEach")
            .ForEach((ref Rotation rotation, in RotationSpeed_ForEach rotationSpeed) 
                => DoRotation(ref rotation, rotationSpeed.RadiansPerSecond * deltaTime))
            .ScheduleParallel();
    }

    static void DoRotation(ref Rotation rotation, float amount)
    {
        rotation.Value = math.mul(
            math.normalize(rotation.Value), 
            quaternion.AxisAngle(math.up(), amount));
    }
}
```

For more information about creating ECS systems, see:

* [Systems]
* [SystemBase]

<a name="encapsulation"></a>
### Encapsulate data and method in a captured value-type:

This pattern helps you organize data and work together into a single unit.

You can define a struct that declares local fields for the data together with the method called by [Entities.ForEach]. In the system [OnUpdate()] function, you can create an instance of the struct as a local variable and then call the function as illustrated in the following examle:

```csharp
public class RotationSpeedSystem_ForEach : SystemBase
{
    struct RotateData
    {
        float3 m_Direction;
        float m_DeltaTime;
        float m_Speed;

        public RotateData(float3 direction, float deltaTime, float speed) 
            => (m_Direction, m_DeltaTime, m_Speed) = (direction, deltaTime, speed);
        public void DoWork(ref Rotation rotation) 
            => rotation.Value = math.mul(math.normalize(rotation.Value), 
                quaternion.AxisAngle(m_Direction, m_Speed * m_DeltaTime));
    }
    
    protected override void OnUpdate()
    {
        var rotateUp = new RotateData(math.up(), Time.DeltaTime, 3.0f);
        Entities.ForEach((ref Rotation rotation) 
            => rotateUp.DoWork(ref rotation))
            .ScheduleParallel();
    }
}
```

**Note:** this pattern copies the data into your job struct (and back out if used with `.Run`). If you do this with very large job structs it can have some performance overhead due to struct copying. In this case it might be a sign that your job should be split up into multiple smaller jobs.

For more information about creating ECS systems, see:

* [Systems]
* [SystemBase]


[Burst]: https://docs.unity3d.com/Packages/com.unity.burst@latest/index.html
[Entities.ForEach]: xref:Unity.Entities.SystemBase.Entities
[.WithoutBurst()]: xref:Unity.Entities.SystemBase.Entities
[OnUpdate()]: xref:Unity.Entities.SystemBase.OnUpdate*
[SystemBase]: xref:Unity.Entities.SystemBase
[Systems]: ecs_systems.md
[Static methods]: #static-methods
[Encapsulate data and methods]: #encapsulation

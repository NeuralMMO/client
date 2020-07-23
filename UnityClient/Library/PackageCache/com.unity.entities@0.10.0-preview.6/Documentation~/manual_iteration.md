# Manual iteration

You can request all of the chunks explicitly in a NativeArray and process them with a job such as `IJobParallelFor`. You should use this method if you need to manage chunks in a way that is not appropriate for the simplified model of  iterating over all of the chunks in a EntityQuery. The following is an example of this:

```c#
public class RotationSpeedSystem : SystemBase
{
   [BurstCompile]
   struct RotationSpeedJob : IJobParallelFor
   {
       [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;
       public ArchetypeChunkComponentType<RotationQuaternion> RotationType;
       [ReadOnly] public ArchetypeChunkComponentType<RotationSpeed> RotationSpeedType;
       public float DeltaTime;

       public void Execute(int chunkIndex)
       {
           var chunk = Chunks[chunkIndex];
           var chunkRotation = chunk.GetNativeArray(RotationType);
           var chunkSpeed = chunk.GetNativeArray(RotationSpeedType);
           var instanceCount = chunk.Count;

           for (int i = 0; i < instanceCount; i++)
           {
               var rotation = chunkRotation[i];
               var speed = chunkSpeed[i];
               rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), speed.RadiansPerSecond * DeltaTime));
               chunkRotation[i] = rotation;
           }
       }
   }
   
   EntityQuery m_Query;   

   protected override void OnCreate()
   {
       var queryDesc = new EntityQueryDesc
       {
           All = new ComponentType[]{ typeof(RotationQuaternion), ComponentType.ReadOnly<RotationSpeed>() }
       };

       m_Query = GetEntityQuery(queryDesc);
   }

   protected override void OnUpdate()
   {
       var rotationType = GetArchetypeChunkComponentType<RotationQuaternion>();
       var rotationSpeedType = GetArchetypeChunkComponentType<RotationSpeed>(true);
       var chunks = m_Query.CreateArchetypeChunkArray(Allocator.TempJob);
       
       var rotationsSpeedJob = new RotationSpeedJob
       {
           Chunks = chunks,
           RotationType = rotationType,
           RotationSpeedType = rotationSpeedType,
           DeltaTime = Time.deltaTime
       };
       this.Dependency rotationsSpeedJob.Schedule(chunks.Length,32, this.Dependency);
   }
}
```

## Iterating manually

You can use the EntityManager class to manually iterate through the entities or chunks, though this is not best practice. You should only use these iteration methods in test or debugging code (or when you are just experimenting), or in an isolated World where you have a perfectly controlled set of entities.

For example, the following snippet iterates through all of the entities in the active World:

``` c#
var entityManager = World.Active.EntityManager;
var allEntities = entityManager.GetAllEntities();
foreach (var entity in allEntities)
{
   //...
}
allEntities.Dispose();
```

 This snippet iterates through all of the chunks in the active World:

``` c#
var entityManager = World.Active.EntityManager;
var allChunks = entityManager.GetAllChunks();
foreach (var chunk in allChunks)
{
   //...
}
allChunks.Dispose();
```

---
uid: ecs-entities-foreach
---

# Using Entities.ForEach

Use the [Entities.ForEach] construction provided by the [SystemBase] class as a concise way to define and execute your algorithms over entities and their components. [Entities.ForEach] executes a lambda function you define over all the entities selected by an [entity query]. 

To execute a job lambda function, you either schedule the job using `Schedule()` and `ScheduleParallel()`, or execute it immediately (on the main thread) with `Run()`. You can use additional methods defined on [Entities.ForEach] to set the entity query as well as various job options.

The following example illustrates a simple [SystemBase] implementation that uses [Entities.ForEach] to read one component (Velocity in this case) and write to another (Translation):

[!code-cs[entities-foreach-example](../package/DocCodeSamples.Tests/LambdaJobExamples.cs#entities-foreach-example)]

Note the use of the keywords `ref` and `in` on the parameters of the ForEach lambda function. Use `ref` for components that you write to, and `in` for components that you only read. Marking components as read-only helps the job scheduler execute your jobs more efficiently.

## Selecting entities

[Entities.ForEach] provides its own mechanism for defining the entity query used to select the entites to process. The query automatically includes any components you use as parameters of your lambda function. You can also use the `WithAll`, `WithAny`, and `WithNone` clauses to further refine which entities are selected. See [SystemBase.Entities] for the complete list of query options. 

The following example selects entities that have the components, Destination, Source, and LocalToWorld; and have at least one of the components, Rotation, Translation, or Scale; but which do not have a LocalToParent component.

[!code-cs[entity-query](../package/DocCodeSamples.Tests/LambdaJobExamples.cs#entity-query)]

In this example, only the Destination and Source components can be accessed inside the lambda function since they are the only components in the parameter list.

### Accessing the EntityQuery object 

To access the [EntityQuery] object created by [Entities.ForEach], use [WithStoreEntityQueryInField(ref query)] with the ref parameter modifier. This function assigns a reference to the query to the field you provide. 

The following example illustrates how to access the EntityQuery object implicitly created for an [Entities.ForEach] construction. In this case, the example uses the EntityQuery object to invoke the [CalculateEntityCount()] method. The example uses this count to create a native array with enough space to store one value per entity selected by the query:

[!code-cs[store-query](../package/DocCodeSamples.Tests/LambdaJobExamples.cs#store-query)]

<a name="optional-components"></a>
### Optional components

You cannot create a query specifying optional components (using WithAny&lt;T,U&gt;) and also access those components in the lambda function. If you need to read or write to a component that is optional, you can split the Entities.ForEach construction into multiple jobs, one for each combination of the optional components. For example, if you had two optional components, you would need three ForEach constructions: one including the first optional component, one including the second, and one including both components. Another alternative is to iterate by chunk using IJobChunk.

<a name="change-filtering"></a>
### Change filtering

In cases where you only want to process an entity component when another entity of that component has changed since the last time the current [SystemBase] instance has run, you can enable change filtering using WithChangeFilter&lt;T&gt;. The component type used in the change filter must either be in the lambda function parameter list or part of a WithAll&lt;T&gt; statement.

[!code-cs[with-change-filter](../package/DocCodeSamples.Tests/LambdaJobExamples.cs#with-change-filter)]

An entity query supports change filtering on up to two component types.

Note that change filtering is applied at the chunk level. If any code accesses a component in a chunk with write access, then that component type in that chunk is marked as changed -- even if the code didn’t actually change any data. 

<a name="shared-component-filtering"></a>
### Shared component filtering

Entities with shared components are grouped into chunks with other entities having the same value for their shared components. You can select groups of entities that have specific shared component values using the WithSharedComponentFilter() function.

The following example selects entities grouped by a Cohort ISharedComponentData. The lambda function in this example sets a DisplayColor IComponentData component based on the entity’s cohort:

[!code-cs[with-shared-component](../package/DocCodeSamples.Tests/LambdaJobExamples.cs#with-shared-component)]

The example uses the EntityManager to get all the unique cohort values. It then schedules a lambda job for each cohort, passing the new color to the lambda function as a captured variable. 

<a name="lambda-function"></a>
## Defining the ForEach function

When you define the lambda function to use with [Entities.ForEach], you can declare parameters that the [SystemBase] class uses to pass in information about the current entity when it executes the function.

A typical lambda function looks like:

[!code-cs[lambda-params](../package/DocCodeSamples.Tests/LambdaJobExamples.cs#lambda-params)]

You can pass up to eight parameters to an Entities.ForEach lambda function. The parameters must be grouped in the following order:

    1. Parameters passed-by-value first (no parameter modifiers)
    2. Writable parameters second (`ref` parameter modifier)
    3. Read-only parameters last (`in` parameter modifier)

All components should use either the `ref` or the `in` parameter modifier keywords. Otherwise, the component struct passed to your function is a copy instead of a reference. This means an extra memory copy for read-only parameters and means that any changes to components you intended to update are silently thrown when the copied struct goes out of scope after the function returns.

If your function does not obey these rules, the compiler provides an error similar to:

`error CS1593: Delegate 'Invalid_ForEach_Signature_See_ForEach_Documentation_For_Rules_And_Restrictions' does not take N arguments`

(Note that the error message cites the number of arguments as the issue even when the problem is the parameter order.)

<a name="component-parameters"></a>
### Component parameters

To access a component associated with an entity, you must pass a parameter of that component type to the lambda function. The compiler automatically adds all components passed to the function to the entity query as required components. 

To update a component value, you must pass it to the lambda function by reference using the `ref` keyword in the parameter list. (Without the `ref` keyword, any modifications would be made to a temporary copy of the component since it would be passed by value.) 

To designate a component passed to the lambda function as read-only, use the `in` keyword in the parameter list.

**Note:** Using `ref` means that the components in the current chunk are marked as changed, even if the lambda function does not actually modify them. For efficiency, always designate components that your lambda function does not modify as read only using the `in` keyword.

The following example passes a Source component parameter to the job as read-only, and a Destination component parameter as writable: 

[!code-cs[read-write-modifiers](../package/DocCodeSamples.Tests/LambdaJobExamples.cs#read-write-modifiers)]

**Note:** Currently, you cannot pass chunk components to the Entities.ForEach lambda function.

For dynamic buffers, use DynamicBuffer&lt;T&gt; rather than the Component type stored in the buffer:

[!code-cs[dynamicbuffer](../package/DocCodeSamples.Tests/LambdaJobExamples.cs#dynamicbuffer)]

<a name="named-parameters"></a>
### Special, named parameters

In addition to components, you can pass the following special, named parameters to the Entities.ForEach lambda function, which are assigned values based on the entity the job is currently processing:

* **`Entity entity`** — the Entity instance of the current entity. (The parameter can be named anything as long as the type is Entity.)
* **`int entityInQueryIndex`** — the index of the entity in the list of all entities selected by the query. Use the entity index value when you have a [native array] that you need to fill with a unique value for each entity. You can use the entityInQueryIndex as the index in that array. The entityInQueryIndex should also be used as the jobIndex for adding commands to a concurrent [EntityCommandBuffer].
* **`int nativeThreadIndex`** — a unique index for the thread executing the current iteration of the lambda function. When you execute the lambda function using Run(), nativeThreadIndex is always zero. (Do not use `nativeThreadIndex` as the `jobIndex` of a concurrent [EntityCommandBuffer]; use `entityInQueryIndex`instead.)

<a name="capturing-variables"></a>
## Capturing variables

You can capture local variables for Entities.ForEach lambda functions. When you execute the function using a job (by calling one of the Schedule functions instead of Run) there are some restrictions on the captured variables and how you use them:

* Only native containers and blittable types can be captured.
* A job can only write to captured variables that are native containers. (To “return” a single value, create a [native array] with one element.)

If you read a [native container], but don't write to it, always specify read-only access using `WithReadOnly(variable)`. 
See [SystemBase.Entities] for more information about setting attributes for captured variables. The attributes you can specify include, `DeallocateOnJobCompletion`, `NativeDisableParallelForRestriction`, and others. [Entities.ForEach] provides these as functions because the C# language doesn't allow attibutes on local variables.

**Note:** When executing the function with `Run()` you can write to captured variables that are not native containers. However, you should still use blittable types where possible so that the function can be compiled with [Burst].

## Supported Features

You can execute the lambda function on the main thread using `Run()`, as a single job using `Schedule()`, or as a parallel job using `ScheduleParallel()`. These different execution methods have different constraints on how you access data. In addition, [Burst] uses a restricted subset of the C# language, so you  need to specify `WithoutBurst()` when using C# features outside this subset (including accessing managed types). 

The following table shows which features are currently supported in [Entities.ForEach] for the different methods of scheduling available in [SystemBase]:

| Supported Feature             | Run                                             | Schedule | ScheduleParallel     |
|-------------------------------|-------------------------------------------------|----------|----------------------|
| Capture local value type      | x                                               | x        | x                    |
| Capture local reference type  | x (only WithoutBurst)                           |          |                      |
| Writing to captured variables | x                                               |          |                      |
| Use field on the system class     | x (only WithoutBurst)                           |          |                      |
| Methods on reference types    | x (only WithoutBurst)                           |          |                      |
| Shared Components             | x (only WithoutBurst)                           |          |                      |
| Managed Components            | x (only WithoutBurst)                           |          |                      |
| Structural changes            | x (only WithoutBurst and WithStructuralChanges) |          |                      |
| SystemBase.GetComponent       | x                                               | x        | x                    |
| SystemBase.SetComponent       | x                                               | x        |                      |
| GetComponentDataFromEntity    | x                                               | x        | x (only as ReadOnly) |
| HasComponent                  | x                                               | x        | x                    |
| WithDeallocateOnJobCompletion | x                                               | x        | x                    |

An [Entities.ForEach] construction uses specialized intermediate language (IL) compilation post-processing to translate the code you write for the construction into correct ECS code. This translation allows you to express the intent of your algorithm without having to include complex, boilerplate code. However, it can mean that some common ways of writing code are not allowed.

The following features are not currently supported:

| Unsupported Feature                                                             |
|---------------------------------------------------------------------------------|
| Dynamic code in .With invocations                                               |
| SharedComponent parameters by ref                                               |
| Nested Entities.ForEach lambda expressions                                      |
| Entities.ForEach in systems marked with [ExecuteAlways] (currently being fixed) |
| Calling with delegate stored in variable, field or by method                    |
| SetComponent with lambda parameter type                                         |
| GetComponent with writable lambda parameter                                     |
| Generic parameters in lambdas                                                   |
| In systems with generic parameters                                              |


## Dependencies

By default, a system manages its ECS-related dependencies using its [Dependency] property. By default, the system adds each job created with [Entities.ForEach] and [Job.WithCode] to the [Dependency] job handle in the order that they appear in the [OnUpdate()] function. You can also manage job dependencies manually by passing a [JobHandle] to your `Schedule` functions, which then return the resulting dependency. See [Dependency] for more information.
 
See [Job dependencies] for more general information about job dependencies.

[SystemBase]: xref:Unity.Entities.SystemBase
[Dependency]: xref:Unity.Entities.SystemBase.Dependency
[race condition]: https://en.wikipedia.org/wiki/Race_condition
[Job dependencies]: xref:ecs-job-dependencies
[IJobParallelFor]: https://docs.unity3d.com/Manual/JobSystemParallelForJobs.html
[OnUpdate()]: xref:Unity.Entities.SystemBase.OnUpdate*
[Entities.ForEach]: xref:Unity.Entities.SystemBase.Entities
[SystemBase.Entities]: xref:Unity.Entities.SystemBase.Entities
[EntityCommandBuffer]:xref:Unity.Entities.EntityCommandBuffer
[native array]: https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html
[EntityQuery]: xref:Unity.Entities.EntityQuery
[entity query]: xref:Unity.Entities.EntityQuery
[WithStoreEntityQueryInField(out query)]: xref:Unity.Entities.SystemBase.Entities
[CalculateEntityCount()]: xref:Unity.Entities.EntityQuery.CalculateEntityCount*
[Burst]: https://docs.unity3d.com/Packages/com.unity.burst@latest/index.html
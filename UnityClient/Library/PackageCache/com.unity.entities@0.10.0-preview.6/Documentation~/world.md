---
uid: ecs-world
---
# World

A World owns both an [EntityManager](xref:Unity.Entities.EntityManager) and a set of [ComponentSystems](component_system.md). You can create as many World objects as you like. Commonly you can create a simulation World and a rendering or presentation World.

By default you create a single World when you enter __Play Mode__ and populate it with all available `ComponentSystem` objects in the Project. However, you can disable the default World creation and replace it with your own code via global defines as follows:

* `#UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_RUNTIME_WORLD` disables generation of the default runtime World.
* `#UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_EDITOR_WORLD` disables generation of the default Editor World.
* `#UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP` disables generation of both default Worlds.

## Further information

- **Default World creation code** (see file: _Packages/com.unity.entities/Unity.Entities.Hybrid/Injection/DefaultWorldInitialization.cs_)
- **Automatic bootstrap entry point** (see file:  _Packages/com.unity.entities/Unity.Entities.Hybrid/Injection/AutomaticWorldBootstrap.cs_) 


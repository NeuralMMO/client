# Change log


## [0.5.0] - 2020-04-24

### Changed

Changes that only affect *Hybrid Renderer V2*:
* V2 now computes accurate AABBs for batches.
* V2 now longer adds WorldToLocal component to renderable entities.

Changes that affect both versions:
* Updated dependencies of this package.

### Deprecated

* Deprecated `FrozenRenderSceneTagProxy` and `RenderMeshProxy`. Please use the GameObject-to-Entity conversion workflow instead.

### Fixed

* Improved precision of camera frustum plane calculation in FrustumPlanes.FromCamera.
* Improved upload performance by uploading matrices as 4x3 instead of 4x4 as well as calculating inverses on the GPU
* Fixed default color properties being in the wrong color space


## [0.4.2] - 2020-04-15

### Changes

* Updated dependencies of this package.


## [0.4.1] - 2020-04-08

### Added (Hybrid V2)

* DisableRendering tag component for disabling rendering of entities

### Changed

* Improved hybrid.renderer landing document. Lots of new information.

### Fixed

* Fixed shadow mapping issues, especially when using the built-in renderer.

### Misc

* Highlighting additional changes introduced in `0.3.4-preview.24` which were not part of the previous changelogs, see below.


## [0.4.0] - 2020-03-13

### Added (All Versions)

* HeapAllocator: Offset allocator for sub-allocating resources such as NativeArrays or ComputeBuffers.

### Added (Hybrid V2)

Hybrid Renderer V2 is a new experimental renderer. It has a significantly higher performance and better feature set compared to the existing hybrid renderer. However, it is not yet confirmed to work on all platforms. To enable Hybrid Renderer V2, use the `ENABLE_HYBRID_RENDERER_V2` define in the Project Settings.

* HybridHDRPSamples Project for sample Scenes, unit tests and graphics tests.
* HybridURPSamples Project for sample Scenes, unit tests and graphics tests.
* MaterialOverride component: User friendly way to configure material overrides for shader properties.
* MaterialOverrideAsset: MaterialOverride asset for configuring general material overrides tied to a shader.
* SparseUploader: Delta update ECS data on GPU ComputeBuffer.
* Support for Unity built-in material properties: See BuiltinMaterialProperties directory for all IComponentData structs.
* Support for HDRP material properties: See HDRPMaterialProperties directory for all IComponentData structs.
* Support for URP material properties: See URPMaterialProperties directory for all IComponentData structs.
* New API (2020.1) to directly write to ComputeBuffer from parallel Burst jobs.
* New API (2020.1) to render Hybrid V2 batches though optimized SRP Batcher backend.

### Changes (Hybrid V2)

* Full rewrite of RenderMeshSystemV2 and InstancedRenderMeshBatchGroup. New code is located at `HybridV2RenderSystem.cs`.
* Partial rewrite of culling. Now all culling code is located at `HybridV2Culling.cs`.
* Hybrid Renderer and culling no longer use hash maps or IJobNativeMultiHashMapVisitKeyMutableValue jobs. Chunk components and chunk/forEach jobs are used instead.
* Batch setup and update now runs in parallel Burst jobs. Huge performance benefit.
* GPU persistent data model. ComputeBuffer to store persistent data on GPU side. Use `chunk.DidChange<T>` to delta update only changed data. Huge performance benefit.
* Per-instance shader constants are no longer setup to constant buffers for each viewport. This makes HDRP script main thread cost significantly smaller and saves significant amount of CPU time in render thread.

### Fixed

* Fixed culling issues (disappearing entities) 8000+ meters away from origin.
* Fixes to solve chunk fragmentation issues with ChunkWorldRenderBounds and other chunk components. Some changes were already included in 0.3.4 package, but not documented.
* Removed unnecessary reference to Unity.RenderPipelines.HighDefinition.Runtime from asmdef.
* Fixed uninitialized data issues causing flickering on some graphics backends (2020.1).

### Misc

* Highlighting `RenderBounds` component change introduced in `0.3.4-preview.24` which was not part of the previous changelogs, see below.


## [0.3.5] - 2020-03-03

### Changed

* Updated dependencies of this package.


## [0.3.4] - 2020-02-17

### Changed

* Updated dependencies of this package.
* When creating entities from scratch with code, user now needs to manually add `RenderBounds` component. Instantiating prefab works as before.
* Inactive GameObjects and Prefabs with `StaticOptimizeEntity` are now correctly treated as static
* `RenderBoundsUpdateSystem` is no longer `public` (breaking)
* deleted public `CreateMissingRenderBoundsFromMeshRenderer` system (breaking)


## [0.3.3] - 2020-01-28

### Changed

* Updated dependencies of this package.


## [0.3.2] - 2020-01-16

### Changed

* Updated dependencies of this package.


## [0.3.1] - 2019-12-16

**This version requires Unity 2019.3.0f1+**

### Changes

* Updated dependencies of this package.


## [0.3.0] - 2019-12-03

### Changes

* Updated dependencies of this package.


## [0.2.0] - 2019-11-22

**This version requires Unity 2019.3 0b11+**

### New Features

* Added support for vertex skinning.

### Fixes

* Fixed an issue where disabled UnityEngine Components were not getting ignored when converted via `ConvertToEntity` (it only was working for subscenes).

### Changes

* Removed `LightSystem` and light conversion.
* Updated dependencies for this package.

### Upgrade guide

  * `Lightsystem` was not performance by default and the concept of driving a game object from a component turned out to be not performance by default. It was also not maintainable because every property added to lights has to be reflected in this package.
  * `LightSystem` will be replaced with hybrid entities in the future. This will be a more clean uniform API for graphics related functionalities.


## [0.1.1] - 2019-08-06

### Fixes

* Adding a disabled tag component, now correctly disables the light.

### Changes

* Updated dependencies for this package.


## [0.1.0] - 2019-07-30

### New Features

* New `GameObjectConversionSettings` class that we are using to help manage the various and growing settings that can tune a GameObject conversion.
* New ability to convert and export Assets, which is initially needed for Tiny.
  * Assets are discovered via `DeclareReferencedAsset` in the `GameObjectConversionDeclareObjectsGroup` phase and can then be converted by a System during normal conversion phases.
  * Assets can be marked for export and assigned a guid via `GameObjectConversionSystem.GetGuidForAssetExport`. During the System `GameObjectExportGroup` phase, the converted assets can be exported via `TryCreateAssetExportWriter`.
* `GetPrimaryEntity`, `HasPrimaryEntity`, and the new `TryGetPrimaryEntity` all now work on `UnityEngine.Object` instead of `GameObject` so that they can also query against Unity Assets.

### Upgrade guide

* Various GameObject conversion-related methods now receive a `GameObjectConversionSettings` object rather than a set of misc config params.
  * `GameObjectConversionSettings` has implicit constructors for common parameters such as `World`, so much existing code will likely just work.
  * Otherwise construct a `GameObjectConversionSettings`, configure it with the parameters you used previously, and send it in.
* `GameObjectConversionSystem`: `AddLinkedEntityGroup` is now `DeclareLinkedEntityGroup` (should auto-upgrade).
* The System group `GameObjectConversionDeclarePrefabsGroup` is now `GameObjectConversionDeclareObjectsGroup`. This cannot auto-upgrade but a global find&replace will fix it.
* `GameObjectConversionUtility.ConversionFlags.None` is gone, use 0 instead.

### Changes

* Changing `entities` dependency to latest version (`0.1.0-preview`).


## [0.0.1-preview.13] - 2019-05-24

### Changes

* Changing `entities` dependency to latest version (`0.0.12-preview.33`).


## [0.0.1-preview.12] - 2019-05-16

### Fixes

* Adding/fixing `Equals` and `GetHashCode` for proxy components.


## [0.0.1-preview.11] - 2019-05-01

Change tracking started with this version.

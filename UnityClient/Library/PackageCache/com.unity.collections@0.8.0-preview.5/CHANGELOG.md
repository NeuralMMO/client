# Change log


## [0.8.0] - 2020-04-24

### Added

 * Added `UnsafeAtomicCounter32/64` providing helper interface for atomic counter functionality.
 * Added `NativeBitArray` providing arbitrary sized bit array functionality with safety mechanism.

### Changed

 * Bumped Burst version to improve compile time and fix multiple bugs.

### Deprecated

 * Deprecated `IJobNativeMultiHashMapMergedSharedKeyIndices`, `JobNativeMultiHashMapUniqueHashExtensions`,
   `IJobNativeMultiHashMapVisitKeyValue`, `JobNativeMultiHashMapVisitKeyValue`, `IJobNativeMultiHashMapVisitKeyMutableValue`,
   `JobNativeMultiHashMapVisitKeyMutableValue`, and introduced `NativeHashMap.GetUnsafeBucketData` and
   `NativeMultiHashMap.GetUnsafeBucketData` to obtain internals to implement deprecated functionality
   inside user code. If this functionality is used, the best is to copy deprecated code into user code.

### Removed

* Removed expired API `class TerminatesProgramAttribute`


## [0.7.1] - 2020-04-08

### Deprecated

 * Deprecated `Length` property from `NativeHashMap`, `UnsafeHashMap`, `NativeMultiHashMap`,
   `UnsafeMultiHashMap`, `NativeQueue`, and replaced it with `Count()` to reflect that there
   is computation being done.

### Fixed

 * Fixed an issue where `FixedListDebugView<T>` only existed for IComparable types, which lead to a crash while debugging other types.
 * Removed code that made NativeStream incompatible with Burst.


## [0.7.0] - 2020-03-13

### Added

 * Added ability to dispose NativeKeyValueArrays from job (DisposeJob).
 * Added `NativeQueue<T>.ToArray` to copy a native queue to an array efficiently

### Changed

 * Upgraded Burst to fix multiple issues and introduced a native debugging feature.

### Deprecated

 * Deprecated `Length` property from `NativeHashMap`, `UnsafeHashMap`, `NativeMultiHashMap`,
   `UnsafeMultiHashMap`, `NativeQueue`, and replaced it with `Count()` to reflect that there
   is computation being done.

### Removed

* Removed expired API `CollectionHelper.CeilPow2()`
* Removed expired API `CollectionHelper.lzcnt()`
* Removed expired API `struct ResizableArray64Byte<T>`

### Fixed

* Removed code that made `NativeStream` incompatible with Burst.


## [0.6.0] - 2020-03-03

### Added

 * Added ability to dispose `UnsafeAppendBuffer` from a `DisposeJob`.

### Changed

 * `UnsafeAppendBuffer` field `Size` renamed to `Length`.
 * Removed `[BurstDiscard]` from all validation check functions. Validation is present in code compiled with Burst.

### Removed

* Removed expired overloads for `NativeStream.ScheduleConstruct` without explicit allocators.

### Fixed

 * Fixed `UnsafeBitArray` out-of-bounds access.


## [0.5.2] - 2020-02-17

### Changed

* Changed `NativeList<T>` parallel reader/writer to match functionality of `UnsafeList` parallel reader/writer.
* Updated dependencies of this package.

### Removed

* Removed expired API `UnsafeUtilityEx.RestrictNoAlias`

### Fixed

 * Fixed bug in `NativeList.CopyFrom`.


## [0.5.1] - 2020-01-28

### Changed

 * Updated dependencies of this package.


## [0.5.0] - 2020-01-16

### Added

 * Added `UnsafeRingQueue<T>` providing fixed-size circular buffer functionality.
 * Added missing `IDisposable` constraint to `UnsafeList` and `UnsafeBitArray`.
 * Added `ReadNextArray<T>` to access a raw array (pointer and length) from an `UnsafeAppendBuffer.Reader`.
 * Added FixedString types, guaranteed binary-layout identical to NativeString types, which they are intended to replace.
 * Added `FixedList<T>` generic self-contained List struct
 * Added `BitArray.SetBits` with arbitrary ulong value.
 * Added `BitArray.GetBits` to retrieve bits as ulong value.

### Changed

 * Changed `UnsafeBitArray` memory initialization option default to `NativeArrayOptions.ClearMemory`.
 * Changed `FixedList` structs to pad to natural alignment of item held in list

### Deprecated

 * `BlobAssetComputationContext.AssociateBlobAssetWithGameObject(int, GameObject)` replaced by its `UnityEngine.Object` counterpart `BlobAssetComputationContext.AssociateBlobAssetWithUnityObject(int, UnityEngine.Object)` to allow association of BlobAsset with any kind of `UnityEngine.Object` derived types.
 * Adding removal dates to the API that have been deprecated but did not have the date set.

### Removed

 * Removed `IEquatable` constraint from `UnsafeList<T>`.

### Fixed

 * Fixed `BitArray.SetBits`.


## [0.4.0] - 2019-12-16

**This version requires Unity 2019.3.0f1+**

### New Features

* Adding `FixedListTN` as a non-generic replacement for `ResizableArrayN<T>`.
* Added `UnsafeBitArray` providing arbitrary sized bit array functionality.

### Fixes

* Updated performance package dependency to 1.3.2 which fixes an obsoletion warning
* Adding `[NativeDisableUnsafePtrRestriction]` to `UnsafeList` to allow burst compilation.


## [0.3.0] - 2019-12-03

### New Features

* Added fixed-size `BitField32` and `BitField64` bit array.

### Changes

Removed the following deprecated API as announced in/before `0.1.1-preview`:

* Removed `struct Concurrent` and `ToConcurrent()` for `NativeHashMap`, `NativeMultiHashMap` and `NativeQueue` (replaced by the *ParallelWriter* API).
* From NativeStream.cs: `struct NativeStreamReader` and `struct NativeStreamWriter`, replaced by `struct NativeStream.Reader` and `struct NativeStream.Writer`.
* From NativeList.cs: `ToDeferredJobArray()` (replaced by `AsDeferredJobArray()` API).


## [0.2.0] - 2019-11-22

**This version requires Unity 2019.3 0b11+**

### New Features

* Added fixed-size UTF-8 NativeString in sizes of 32, 64, 128, 512, and 4096 bytes.
* Added HPC# functions for float-to-string and string-to-float.
* Added HPC# functions for int-to-string and string-to-int.
* Added HPC# functions for UTF16-to-UTF8 and UTF8-to-UTF16.
* New `Native(Multi)HashMap.GetKeyValueArrays` that will query keys and values
  at the same time into parallel arrays.
* Added `UnsafeStream`, `UnsafeHashMap`, and `UnsafeMultiHashMap`, providing
  functionality of `NativeStream` container but without any safety mechanism
  (intended for advanced users only).
* Added `AddNoResize` methods to `NativeList`. When it's known ahead of time that
  list won't grow, these methods won't try to resize. Rather exception will be
  thrown if capacity is insufficient.
* Added `ParallelWriter` support for `UnsafeList`.
* Added `UnsafeList.TrimExcess` to set capacity to actual number of elements in
  the container.
* Added convenience blittable `UnsafeList<T>` managed container with unmanaged T
  constraint.

### Changes

* `UnsafeList.Resize` now doesn't resize to lower capacity. User must call
  `UnsafeList.SetCapacity` to lower capacity of the list. This applies to all other
  containers based on `UnsafeList`.
* Updated dependencies for this package.

### Fixes

* Fixed NativeQueue pool leak.


## [0.1.1] - 2019-08-06

### Fixes

* `NativeHashMap.Remove(TKey key, TValueEQ value)` is now supported in bursted code.
* Adding deprecated `NativeList.ToDeferredJobArray()` back in - Use `AsDeferredJobArray()`
  instead. The deprecated function will be removed in 3 months. This can not be auto-upgraded
  prior to Unity `2019.3`.
* Fixing bug where `TryDequeue` on an empty `NativeQueue` that previously had enqueued elements could leave it in
  an invalid state where `Enqueue` would fail silently afterwards.

### Changes

* Updated dependencies for this package.


## [0.1.0] - 2019-07-30

### New Features

* NativeMultiHashMap.Remove(key, value) has been addded. It lets you remove
  all key & value pairs from the hashmap.
* Added ability to dispose containers from job (DisposeJob).
* Added UnsafeList.AddNoResize, and UnsafeList.AddRangeNoResize.
* BlobString for storing string data in a blob

### Upgrade guide

* `Native*.Concurrent` is renamed to `Native*.ParallelWriter`.
* `Native*.ToConcurrent()` function is renamed to `Native*.AsParallelWriter()`.
* `NativeStreamReader/Writer` structs are subclassed and renamed to
  `NativeStream.Reader/Writer` (note: changelot entry added retroactively).

### Changes

* Deprecated ToConcurrent, added AsParallelWriter instead.
* Allocator is not an optional argument anymore, user must always specify the allocator.
* Added Allocator to Unsafe\*List container, and removed per method allocator argument.
* Introduced memory intialization (NativeArrayOptions) argument to Unsafe\*List constructor and Resize.

### Fixes

* Fixed UnsafeList.RemoveRangeSwapBack when removing elements near the end of UnsafeList.
* Fixed safety handle use in NativeList.AddRange.


## [0.0.9-preview.20] - 2019-05-24

### Changes

* Updated dependencies for `Unity.Collections.Tests`


## [0.0.9-preview.19] - 2019-05-16

### New Features

* JobHandle NativeList.Dispose(JobHandle dependency) allows Disposing the container from a job.
* Exposed unsafe NativeSortExtension.Sort(T* array, int length) method for simpler sorting of unsafe arrays
* Imporoved documentation for `NativeList`
* Added `CollectionHelper.WriteLayout` debug utility

### Fixes

* Fixes a `NativeQueue` alignment issue.


## [0.0.9-preview.18] - 2019-05-01

Change tracking started with this version.

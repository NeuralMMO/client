# Changelog

## [1.3.3] - 2020-06-25


### Fixed
- Fixed compatibility issues between burst and older linux distros.
- Fixed an issue preventing player builds to succeed when burst compilation is disabled.

### Known Issues
- Output of `Debug.Log` is temporarily disabled when used in Burst Function Pointers/Jobs to avoid a deadlock on a domain reload. A fix for the Unity editor is being developed.

## [1.3.2] - 2020-06-16


### Added

### Removed

### Changed

### Fixed
- Burst package has been upgraded popup could fire erroneously under shutdown conditions.
- Debug information for anonymous structs could be created partially multiple times for the same type.
- IntPtr.Size now correctly returns int32 size (rather than UInt64) - fixes an assert.
- Fix safety checks in editor to not log a warning. Safety checks are now restored to true when restarting the editor and are no longer stored as an editor preference.

### Known Issues

## [1.3.1] - 2020-06-05


### Fixed
- Burst compilation is no longer cancelled when exiting play mode.
- Filter symbol warnings to prevent them reaching logs.
- Fixed handling of conversion from signed integer to pointer which caused issues as discovered by Zuntatos on the forums.
- Fix an issue where a function/job could run without being initialized.
- Fixed a bug with constant expressions that could cause a compile-time hang.

### Added

### Removed

### Changed
- To avoid users falling into the consistent trap of having `Safety Checks` set to `Off`, any reload of the Editor will issue a warning telling the user that `Safety Checks` have been reset to `On`.
- The command line option --burst-disable-compilation is now disabling entirely Burst, including the AppDomain.

### Known Issues

## [1.3.0] - 2020-05-23


### Changed
- Bump package version to 1.3.0 stable release.

## [1.3.0-preview.13] - 2020-05-12


### Fixed
- Fixed incorrect struct layout for certain configurations of explicit-layout structs with overlapping fields
- Fixed a bug where the `mm256_cvtepi32_ps` intrinsic would crash the compiler.

## [1.3.0-preview.12] - 2020-05-05


### Fixed
- Fix an issue when changing the base type of an enum that would not trigger a new compilation and would keep code previously compiled, leading to potential memory corruptions or crashes.
- Fixed a subtle AArch64 ABI bug with struct-return's (structs that are returned via a pointer argument) that was found by our partners at Arm.
- Fix an issue that was preventing Debug.Log to be used from a Job in Unity 2020.1

### Changed
- JIT cache is now cleared when changing Burst version

## [1.3.0-preview.11] - 2020-04-30


### Fixed
- Fix potentially different hashes returned from `BurstRuntime.GetHashCode32/64` if called from different assemblies.
- Fixed an issue where Burst was misidentifying F16C supporting CPUs as AVX2.
- SDK level bumped for MacOS to ensure notarization requests are compatable.
- Fixed a typo `m256_cvtsi256_si32` -> `mm256_cvtsi256_si32` and `m256_cvtsi256_si64` -> `mm256_cvtsi256_si64`.
- The compiler is now generating a proper compiler error if a managed type used directly or indirectly with SharedStatic<T>.
- Fixed a bug where implicitly stack allocated variables (`var foo = new Foo();`) in Burst were not being zero initialized, so any field of the variable that was not initialized during construction would have undefined values.
- Fix potential race condition when accessing on-disk library cache
- Fixed a bug where Burst was sometimes producing invalid code for iOS 11.0.3+.

### Added
- Added support for `System.Threading.Volatile` methods `Read` and `Write`, and for the `System.Threading.Thread.MemoryBarrier` method.
- New FMA X86 intrinsics. These are gated on AVX2 support, as our AVX2 detection requires the AVX2, FMA, and F16C features.
- `UnsafeUtility.MemCmp` now maps to a Burst optimal memory comparison path that uses vectorization.

### Removed

### Changed

### Known Issues

## [1.3.0-preview.10] - 2020-04-21


### Fixed
- Fix negation of integer types smaller than 32 bits.
- Fixed a bug where optimizer generated calls to `ldexp` would be incorrectly deduced when deterministic floating-point was enabled.
- Swapped private linkage for internal linkage on functions, this fixes duplicate symbol issues on some targets.
- variable scopes should now encompass the whole scope.
- variables in parent scopes should now be present in locals windows.
- Native plugin location for windows has changed in 2019.3.9f1. If you are on an older version of 2019.3 you will need to upgrade for burst to work in windows standalone players.
- Added an error if `Assert.AreEqual` or `Assert.AreNotEqual` were called with different typed arguments.
- Fixed a bug where doing an explicit cast to a `Unity.Mathematics` vector type where the source was a scalar would fail to compile.
- Fix issue when converting large unsigned integer values to double or float.
- Fix an invalid value returned from a conditional where one type is an int32 and the other type would be a byte extended to an int32.
- Button layout of disassembly toolbar tweaked.
- Copy to clipboard now copies exactly what is shown in the inspector window (including enhancements and colours if shown)
- AVX2 now generates the correct AVX2 256-bit wide SLEEF functions instead of the FMA-optimized 128-bit variants.

### Added
- Anonymous types are now named in debug information.
- XCode/LLDB debugging of burst compiled code is now possible on macOS.
- Added some extra documentation about how to enable `AVX`/`AVX2` in AOT builds, and how we gate some functionality on multiple instruction sets to reduce the combinations exposed underneath.
- Optimized external functions (like `UnsafeUtility.Malloc`) such that if they are called multiple times the function-pointer address is cached.
- Add support for string interpolation (e.g `$"This is a string with an {arg1} and {arg2}"`).
- Add support for Debug.Log(object) (e.g `Debug.Log("Hello Log!");`).
- Add support for string assignment to Unity.Collections.FixedString (e.g `"FixedString128 test = "Hello FixedString!"`).
- If burst detects a package update, it now prompts a restart of Unity (via dialog). The restart was always required, but could be missed/forgotten.
- Better error message for unsupported static readonly arrays.
- Link to native debugging video to Presentations section of docs.
- Fixes a bug where `in` parameters of interfaces could sometimes confuse the Burst inspector.

### Removed

### Changed
- iOS builds for latest xcode versions will now use LLVM version 9.
- Burst AOT Settings now lets you specify the exact targets you want to compile for - so you could create a player with SSE2, AVX, and AVX2 (EG. _without_ SSE4 support if you choose to).
- Improve speed of opening Burst Inspector by up to 2x.
- Provided a better error message when structs with static readonly fields were a mix of managed/unmanaged which Burst doesn't support.
- Tidied up the known issues section in the docs a little.
- Enhanced disassembly option has been expanded to allow better control of what is shown, and allow a reduction in the amount of debug metadata shown.
- Load Burst Inspector asynchronously to avoid locking-up Editor.
- Documented restrictions on argument and return types for DllImport, internal calls, and function pointers.

### Known Issues

## [1.3.0-preview.9] - 2020-04-01


### Changed
- Improved the compile time performance when doing `UnsafeUtility.ReadArrayElement` or `UnsafeUtility.WriteArrayElement` with large structs.
- Made some compile-time improvements when indirect arguments (those whose types are too big that they have to be passed by reference) that reduced our compile time by 3.61% on average.

### Fixed
- Fixed a bug where storing a `default` to a pointer that was generic would cause an LLVM verifier error.
- Fixed an obscure bug in how struct layouts that had dependencies on each other were resolved.
- Fixed a bug as found by [@iamarugin](https://forum.unity.com/members/iamarugin.737579/) where LLVM would introduce ldexp/ldexpf during optimizations that LLD would not be able to resolve.
- Fixed a bug where the compiler would not promote sub-integer types to integers when doing scalar-by-vector math (like multiplies).

### Added
- Variable scopes are now constructed for debug information.
- A new setting to Burst AOT Settings that allows debug symbols to be generated even in a non development standalone build.

### Removed

### Known Issues

## [1.3.0-preview.8] - 2020-03-24


### Added
- Double math builtins in `Unity.Mathematics` now use double vector implementations from SLEEF.
- Fixed a bug with `lzcnt`, `tzcnt`, and `countbits` which when called with `long` types could produce invalid codegen.
- New F16C X86 intrinsics. These are gated on AVX2 support, as our AVX2 detection requires the AVX2, FMA, and F16C features.
- Add user documentation about generic jobs and restrictions.
- Add new experimental compiler intrinsics `Loop.ExpectVectorized()` and `Loop.ExpectNotVectorized()` that let users express assumptions about loop vectorization, and have those assumptions validated at compile-time.Enabled with `UNITY_BURST_EXPERIMENTAL_LOOP_INTRINSICS`.

### Changed
- Changed how `Unity.Mathematics` functions behave during loop vectorization and constant folding to substantially improve code generation.
- Our SSE4.2 support was implicitly dependent on the POPCNT extended instruction set, but this was not reflected in our CPU identification code. This is now fixed so that SSE4.2 is gated on SSE4.2 and POPCNT support.
- The popcnt intrinsics now live in their own static class `Unity.Burst.Intrinsics.Popcnt` to match the new F16C intrinsics.
- Deferred when we load the SLEEF builtins to where they are actually used, decreasing compile time with Burst by 4.29% on average.

### Fixed
- Fix an issue where a generic job instance (e.g `MyGenericJob<int>`) when used through a generic argument of a method or type would not be detected by the Burst compiler when building a standalone player.
- `[DlIimport("__Internal")]` for iOS now handled correctly. Fixes crashes when using native plugins on iOS.

### Removed

### Known Issues

## [1.3.0-preview.7] - 2020-03-16


### Added
- Added additional diagnostic for tracking Visual Studio location failures.
- Added an override to bypass link.exe discovery under certain conditions.
- Added a ldloc -> stloc optimization which improves compile times.
- More documentation on function pointers, specifically some performance considerations to be aware of when using them.

### Removed

### Changed
- Updated tools used for determining Visual Studio locations.

### Fixed
- Embedded Portable PDB handling improved.
- Fixed a case where our load/store optimizer would inadvertently combine a load/store into a cpblk where there were intermediate memory operations that should have been considered.
- Fixed a bug where the no-alias analysis would, through chains of complicated pointer math, deduce that a no-alias return (like from `UnsafeUtility.Malloc`) would not alias with itself.
- No longer log missing MonoPInvokeCallbackAttribute when running tests.

### Known Issues

## [1.3.0-preview.6] - 2020-03-12


### Added
- Experimental support for `Prefetch`, allowing users to request from the memory subsystem pointer addresses they intend to hit next. This functionality is guarded by the `UNITY_BURST_EXPERIMENTAL_PREFETCH_INTRINSIC` preprocessor define.

### Fixed
- Fix SSE `maxps` intrinsic would emit `maxss`

## [1.3.0-preview.5] - 2020-03-11


### Fixed
- `MemCpy` and `MemSet` performance regression in Burst 1.3.0.preview.4 (as was spotted by [@tertle](https://forum.unity.com/members/33474/)) has been fixed.
- Fix a crash when loading assembly with PublicKeyToken starting with a digit.
- Better handling of MonoPInvokeCallbackAttribute: no check for the namespace, don't print message on Mono builds.

### Changed
- Improved error message for typeof usage.

## [1.3.0-preview.4] - 2020-03-02


### Added
- Debug information for types.
- Debug information for local variables.
- Debug information for function parameters.
- Support for `fixed` statements. These are useful when interacting with `fixed` buffers in structs, to get at the pointer data underneath.
- A fast-math optimization for comparisons that benefits the [BurstBenchmarks](https://github.com/nxrighthere/BurstBenchmarks) that [nxrightthere](https://forum.unity.com/members/nxrighthere.568489/) has put together.
- DOTS Runtime Jobs will now generate both `MarshalToBurst` and `MarshalFromBurst` functions when job structs in .Net builds are not blittable.
- DOTS Runtime Job Marshalling generation is now controllable via the commandline switch `--generate-job-marshalling-methods`.

### Removed

### Changed
- Made it clear that the Burst aliasing intrinsics are tied to optimizations being enabled for a compilation.
- Restore unwind information for all builds.
- Print a info message if compiling a function pointer with missing MonoPInvokeCallback attribute (this can lead to runtime issues on IL2CPP with Burst disabled). The message will be converted to a warning in future releases.

### Fixed
- Fixed an issue where DOTS Runtime generated job marshalling functiosn may throw a `FieldAccessException` when scheduling private and internal job structs.
- Fix a bug that prevented entry point method names (and their declaring type names) from having a leading underscore.
- vector/array/pointer debug data now utilizes the correct size information.
- DOTS Runtime will now only generate job marshaling functions on Windows, as all other platforms rely on Mono which does not require job marshalling.
- `ldobj` / `stobj` of large structs being copied to stack-allocated variables could cause compile-time explosions that appeared to the user like the compiler had hung. Worked around these by turning them into memcpy's underneath in LLVM.
- Don't always use latest tool chain on certain platforms.
- Fix a crash when compiling job or function pointer that was previously cached, then unloaded, then reloaded.
- Fixed compiler error in array element access when index type is not `Int32`.
- Fix `set1_xxx` style x86 intrinsics generated compile time errors.

### Known Issues
- Native debugger feature is only available on windows host platform at the moment.

## [1.3.0-preview.3] - 2020-02-12


### Changed
- Changed how the inliner chooses to inline functions to give the compiler much more say over inlining decisions based on heuristics.
- Updated AOT requirements to be clearer about cross platform support.

### Added
- 1.3.0-preview.1 added support for desktop cross compilation, but the changelog forgot to mention it.

### Removed

### Fixed
- Documentation for the command line options to unity contained extra -
- Burst now exclusively uses the `<project>/Temp/Burst` folder for any temporary files it requires during compilation.
- Fix a regression that could break usage of native plugins.

### Known Issues

## [1.3.0-preview.2] - 2020-02-10

### Fixed
- Fix the error `Burst failed to compile the function pointer Int32 DoGetCSRTrampoline()` that could happen when loading a project using Burst with Burst disabled.

## [1.3.0-preview.1] - 2020-02-04

### Added
- Enabled lower precision variants for `pow`, `sin`, `cos`, `log`, `log2`, `log10`, `exp`, `exp2`, and `exp10` when `BurstPrecision.Low` is specified.
- Add CPU minimum and maximum target for desktop platforms Standalone Player builds.
- Append a newline between IRPassDiagnostic messages, fixes pass diagnostics readability in the inspector.
- Add a new attribute `[AssumeRange]` that lets users tag function parameters and returns of an integer type with a constrained range that the value is allowed to inhabit. `NativeArray.Length` and `NativeSlice.Length` have automatic detection that the property is always positive. This assumption feeds into the optimizer and can produce better codegen.
- Enabled support for DOTS Runtime SharedStatics. Due to the nature of DOTS Runtime, only the generic versions of `SharedStatic.GetOrCreate<TContext>` are supported.
- Add a new intrinsic `Unity.Burst.Intrinsics.Common.Pause()` which causes a thread pause to occur for the current thread. This is useful for spin-locks to stop over contention on the lock.
- Add some new Burst aliasing deductions to substantially improve the aliasing detection in the compiler, resulting in better codegen.
- Add syntax colouring to WASM.
- Add `IsCreated` to the `FunctionPointer` class to allow checks on whether a given function pointer has a valid (non null) pointer within it.
- Add AVX2 intrinsics
- Add some missing intrinsics from SSE, SSE2 and AVX
- Added explicit X86 intrinsics from SSE-AVX2.
- AVX and AVX2 CPU targets are now available for x64 AOT builds.
- Allow handle structs (structs with a single pointer/integer in them) to be inside another struct as long as they are the single member, as these require no ABI pain.
- Added support for `Interlocked.Read`.
- Added a new intrinsic `Common.umul128` which lets you get the low and high components of a 64-bit multiplication. This is especially useful for things like large hash creation.
- Menu option to allow all burst jobs to be more easily debugged in a native debugger.

### Removed

### Changed
- Upgraded Burst to use LLVM Version 9.0.1 by default, bringing the latest optimization improvements from the LLVM project.
- Upgraded Burst to use SLEEF 3.4.1, bringing the latest performance improvements to mathematics functions as used in Burst.
- Improved Burst performance in the Editor by caching compiled libraries on-disk, meaning that in subsequent runs of the Editor, assemblies that haven't changed won't be recompiled.
- Update the documentation of `CompileSynchronously` to advise against any general use of setting `CompileSynchronously = true`.
- Take the `Unity.Burst.CompilerServices.Aliasing` intrinsics out of experimental. These intrinsics form part of our strategy to give users more insight into how the compiler understands their code, by producing compiler errors when user expectations are not met. Questions like _'Does A alias with B?'_ can now be definitively answered for developers. See the **Aliasing Checks** section of the Burst documentation for information.
- Align disassembly instruction output in Inspector (x86/x64 only).
- Renamed `m128` to `v128`.
- Renamed `m256` to `v256`.
- BurstCompile(Debug=true), now modifies the burst code generator (reducing some optimisations) in order to allow a better experience in debugging in a native debugger.

### Fixed
- Fix a bug where floating-point != comparisons were using a stricter NaN-aware comparison than was required.
- Fix inspector for ARMV7_NEON target.
- Fix some issues with Burst AOT Settings, including changing the settings to be Enable rather than Disable.
- Fix an issue where WASM was being incorrectly shown in the disassembly view.
- Fixed an issue where if the `Unity.Entities.StaticTypeRegistry` assembly wasn't present in a build, Burst would throw a `NullReferenceException`.
- Fix issue with type conversion in m128/m256 table initializers.
- Fix inspector source line information (and source debug information) from being lost depending on inlining.
- Fix occasional poor code generation for on stack AVX2 variables.
- Fix `xor_ps` was incorrectly downcoded.
- Fix reference version of AVX2 64-bit variable shifts intrinsics.
- Fix reference version of SSE4.2 `cmpestrz`.
- Fix bitwise correctness issue with SSE4.2/AVX explicit rounding in CEIL mode for negative numbers that round to zero (was not correctly computing negative zero like the h/w).
- Fix calls to `SHUFFLE`, `SHUFFLE_PS` and similar macro-like functions would not work in non-entrypoint functions.
- Source location information was offset by one on occasions.
- Debug metadata is now tracked on branch/switch instructions.
- Fix poor error reporting when intrinsic immediates were not specified as literals.
- Fix basic loads and stores (using explicit calls) were not unaligned and sometimes non-temporal when they shouldn't be.
- Removed the  `<>c__DisplayClass_` infix that was inserted into every `Entities.ForEach` in the Burst inspector to clean up the user experience when searching for Entities.ForEach jobs.
- Fix background compile errors accessing X86 `MXCSR` from job threads.
- Fix possible `ExecutionEngineException` when resolving external functions.
- Fix linker output not being propagated through to the Editor console.

### Known Issues

## [1.2.0-preview.9] - 2019-11-06

- Fix compilation requests being lost when using asynchronous compilation.
- Prevent Burst compilation being toggled on while in play mode, either via "Enable Compilation" menu item or programmatically - was previously technically possible but produced unpredictable results.

## [1.2.0-preview.8] - 2019-11-01

- Fix a `NullReferenceException` happening in a call stack involving `CecilExtensions.IsDelegate(...)`.

## [1.2.0-preview.7] - 2019-10-30

- Many improvements to the Inspector:
  - New assembly syntax colorization!
  - Fix issue with menu settings being modified when opening the Inspector.
  - Make compile targets left pane resizable.
  - Fix vertical scrollbar size.
  - Add automatic refresh when selecting a target to compile.
- Fix an issue where `ref readonly` of a struct type, returned from a function, would cause a compiler crash.
- Add support for `Interlocked.Exchange` and `Interlocked.CompareExchange` for float and double arguments.
- Fix bug preventing iOS builds from working, if burst is disabled in AOT Settings.

## [1.2.0-preview.6] - 2019-10-16

- New multi-threaded compilation support when building a standalone player.
- Improve `BurstCompiler.CompileFunctionPointer` to compile asynchronously function pointers in the Editor.
- Improve of error codes and messages infrastructure.
- Upgraded Burst to use LLVM Version 8.0.1 by default, bringing the latest optimization improvements from the LLVM project.
- Fix issue with libtinfo5 missing on Linux.
- Fix possible NullReferenceException when an entry point function is calling another empty function.
- Fix an exception occurring while calculating the size of a struct with indirect dependencies to itself.
- Fix potential failure when loading MDB debugging file.
- Fix linker issue with folder containing spaces.
- Fix issue with package validation by removing ifdef around namespaces.
- Fix issue with an internal compiler exception related to an empty stack.

## [1.2.0-preview.5] - 2019-09-23

- Fix crashing issue during the shutdown of the editor.

## [1.2.0-preview.4] - 2019-09-20

- Fix a logging issue on shutdown.

## [1.2.0-preview.3] - 2019-09-20

- Fix potential logging of an error while shutting down the editor.

## [1.2.0-preview.2] - 2019-09-20

- New multi-threaded compilation of jobs/function pointers in the editor.
- Improve caching of compiled jobs/function pointers.
- Fix a caching issue where some jobs/function pointers would not be updated in the editor when updating their code.
- Fix an issue where type initializers with interdependencies were not executed in the correct order.
- Fix an issue with `Failed to resolve assembly Windows, Version=255.255.255.255...` when building for Xbox One.
- Fix compilation error on ARM32 when calling an external function.
- Fix an issue with function pointers that would generate invalid code if a non-blittable type is used in a struct passed by ref.
- Fix an issue with function pointers that would generate invalid code in case containers/pointers passed to the function are memory aliased.
- Report a compiler error if a function pointer is trying to be compiled without having the `[BurstCompile]` attribute on the method and owning type.

## [1.2.0-preview.1] - 2019-09-09

- Fix assembly caching issue, cache usage now conservative (Deals with methods that require resolving multiple assemblies prior to starting the compilation - generics).
- Fix Mac OS compatibility of Burst (10.10 and up) - fixes undefined symbol _futimens_.

## [1.1.3-preview.3] - 2019-09-02

- Query android API target level from player settings when building android standalone players.
- Add calli opcode support to support bindings to native code.

## [1.1.3-preview.2] - 2019-08-29

- Fix to allow calling [BurstDiscard] functions from static constructors.
- Correctly error if a DLLImport function uses a struct passed by value, but allow handle structs (structs with a single pointer/integer in them) as these require no ABI pain.
- Upgraded Burst to use LLVM Version 8 by default, bringing the latest optimisation improvements from the LLVM project.
- Added support for multiple LLVM versions, this does increase the package size, however it allows us to retain compatability with platforms that still require older versions of LLVM.
- Fix bug in assembly caching, subsequent runs should now correctly use cached jit code as appropriate.
- Add support for Lumin platform

## [1.1.3-preview.1] - 2019-08-26

- Add support for use of the MethodImpl(MethodImplOptions.NoOptimization) on functions.
- Fix an issue whereby static readonly vector variables could not be constructed unless using the constructor whose number of elements matched the width of the vector.
- Fix an issue whereby static readonly vector variables could not be struct initialized.
- Improve codegen for structs with explicit layout and overlapping fields.
- Fix a bug causing SSE4 instructions to be run on unsupported processors.
- Fix an issue where storing a pointer would fail as our type normalizer would cast the pointer to an i8.
- Begin to add Burst-specific aliasing information by instructing LLVM on our stack-allocation and global variables rules.

## [1.1.2] - 2019-07-26

- Fix an issue where non-readonly static variable would not fail in Burst while they are not supported.
- Fix issue with char comparison against an integer. Add partial support for C# char type.
- Improve codegen for struct layout with simple explicit layout.
- Fix NullReferenceException when using a static variable with a generic declaring type.
- Fix issue with `stackalloc` not clearing the allocated stack memory as it is done in .NET CLR.

## [1.1.1] - 2019-07-11

- Fix a compiler error when using a vector type as a generic argument of a NativeHashMap container.
- Disable temporarily SharedStatic/Execution mode for current 2019.3 alpha8 and before.
- Fix detection of Android NDK for Unity 2019.3.
- Update documentation for known issues.

## [1.1.0] - 2019-07-09

- Fix detection of Android NDK for Unity 2019.3.
- Update documentation for known issues.

## [1.1.0-preview.4] - 2019-07-05

- Burst will now report a compilation error when writing to a `[ReadOnly]` container/variable.
- Fix regression with nested generics resolution for interface calls.
- Fix issue for UWP with Burst generating non appcert compliant binaries.
- Fix issue when reading/writing vector types to a field of an explicit layout.
- Fix build issue on iOS, use only hash names for platforms with clang toolchain to mitigate issues with long names in LLVM IR.
- Allow calls to intrinsic functions (e.g `System.Math.Log`) inside static constructors.
- Improve performance when detecting if a method needs to be recompiled at JIT time.
- Fix an issue with explicit struct layout and vector types.

## [1.1.0-preview.3] - 2019-06-28

- Fix issue with generic resolution that could fail.
- Add support for readonly static data through generic instances.
- Add internal support for `SharedStatic<T>` for TypeManager.
- Add intrinsic support for `math.bitmask`.

## [1.1.0-preview.2] - 2019-06-20

- Fix issue where uninitialized values would be loaded instead for native containers containing big structs.
- Fix issue where noalias analysis would fail for native containers containing big structs.
- Fix issue when calling "internal" methods that take bool parameters.
- Add support for `MethodImplOptions.AggressiveInlining` to force inlining.
- Fix issue in ABITransform that would cause compilation errors with certain explicit struct layouts.
- Disable debug information generation for PS4 due to IR compatability issue with latest SDK.
- Implemented an assembly level cache for JIT compilation to improve iteration times in the Editor.
- Implement a hard cap on the length of symbols to avoid problems for platforms that ingest IR for AOT.
- Add support for `FunctionPointer<T>` usable from Burst Jobs via `BurstCompiler.CompileFunctionPointer<T>`.
- Add `BurstCompiler.Options` to allow to control/enable/disable Burst jobs compilation/run at runtime.
- Add `BurstRuntime.GetHashCode32<T>` and `GetHashCode64<T>` to allow to generate a hash code for a specified time from a Burst job.

## [1.0.0] - 2019-04-16

- Release stable version.

## [1.0.0-preview.14] - 2019-04-15

- Bump to mathematics 1.0.1
- Fix android ndk check on windows when using the builtin toolchain.
- Fix crash when accessing a field of a struct with an explicit layout through an embedded struct.
- Fix null pointer exception on building for android if editor version is less than 2019.1.
- Workaround IR compatibility issue with AOT builds on IOS.

## [1.0.0-preview.13] - 2019-04-12

- Fix linker error on symbol `$___check_bounds already defined`.
- Fix StructLayout Explicit size calculation and backing storage.

## [1.0.0-preview.12] - 2019-04-09

- Fix crash when accessing a NativeArray and performing in-place operations (e.g `nativeArray[i] += 121;`).

## [1.0.0-preview.11] - 2019-04-08

- Improve error logging for builder player with Burst.
- Fix NullReferenceException when storing to a field which is a generic type.

## [1.0.0-preview.10] - 2019-04-05

- Update known issues in the user manual.
- Improve user manual documentation about debugging, `[BurstDiscard]` attribute, CPU architectures supported...
- Fix an issue where Burst callbacks could be sent to the editor during shutdowns, causing an editor crash.
- Improve error messages for external tool chains when building for AOT.

## [1.0.0-preview.9] - 2019-04-03

- Fix an auto-vectorizer issue not correctly detecting the safe usage of NativeArray access when performing in-place operations (e.g `nativeArray[i] += 121;`).
- Add support for dynamic dispatch of functions based on CPU features available at runtime.
  - Fix issue when running SSE4 instructions on a pre-SSE4 CPU.
- Fix write access to `NativeArray<bool>`.
- Remove dependencies to C runtime for Windows/Linux build players (for lib_burst_generated.so/.dll).
- Updated API documentation.
- Update User manual.
- Static link some libraries into the Burst llvm wrapper to allow better support for some linux distros.

## [1.0.0-preview.8] - 2019-03-28

- Fix for iOS symbol names growing too long, reduced footprint of function names via pretty printer and a hash.

## [1.0.0-preview.7] - 2019-03-28

- Burst will now only generate debug information for AOT when targeting a Development Build.
- Added support for locating the build tools (standalone) for generating AOT builds on windows, without having to install Visual Studio complete.
- Fix Log Timings was incorrectly being passed along to AOT builds, causing them to fail.
- Fix editor crash if Burst aborted compilation half way through (because editor was being closed).
- Fix issue with job compilation that could be disabled when using the Burst inspector.
- Fix issue with spaces in certain paths (e.g. ANDROID_NDK_ROOT) when building for AOT.
- Restore behavior of compiling ios projects from windows with Burst, (Burst does not support cross compiling for ios) - we still generate a valid output project, but with no Burst code.
- Add support for Android embedded NDK.
- Fix issue where certain control flow involving object construction would crash the compiler in release mode.

## [1.0.0-preview.6] - 2019-03-17

- Fix invalid codegen with deep nested conditionals.
- Fix issue with Burst menu "Enable Compilation" to also disable cache jobs.
- Improve handling of PS4 toolchain detection.

## [1.0.0-preview.5] - 2019-03-16

- Fix regression with JIT caching that was not properly recompiling changed methods.
- Remove NativeDumpFlags from public API.
- Remove usage of PropertyChangingEventHandler to avoid conflicts with custom Newtonsoft.Json.
- Fix issue when a job could implement multiple job interfaces (IJob, IJobParallelFor...) but only the first one would be compiled.

## [1.0.0-preview.4] - 2019-03-15

- Fix "Error while verifying module: Invalid bitcast" that could happen with return value in the context of deep nested conditionals.
- Fix support for AOT compilation with float precision/mode.
- Fix fast math for iOS/PS4.
- Fix issue with double not using optimized intrinsics for scalars.
- Fix issue when loading a MDB file was failing when building a standalone player.
- Fix no-alias analysis that would be disabled in a standalone player if only one of the method was failing.
- Fix bug with explicit layout struct returned as a pointer by a property but creating an invalid store.
- Change `FloatPrecision.Standard` defaulting from `FloatPrecision.High` (ULP1) to `FloatPrecision.Medium` (ULP3.5).

## [1.0.0-preview.3] - 2019-03-14

- Fix compilation issue with uTiny builds.

## [1.0.0-preview.2] - 2019-03-13

- Fix no-alias warning spamming when building a standalone player.
- Improve the layout of the options/buttons for the inspector so that they at least attempt to layout better when the width is too small for all the buttons.
- Fix formatting of error messages so the Unity Console can correctly parse the location as a clickable item (Note however it does not appear to allow double clicking on absolute paths).
- Change Burst menu to Jobs/Burst. Improve order of menu items.
- Fix for AOTSettings bug related to StandaloneWindows vs StandaloneWindows64.

## [1.0.0-preview.1] - 2019-03-11

- Fix regression when resolving the type of generic used in a field.
- Fix linker for XboxOne, UWP.
- Fix performance codegen when using large structs.
- Fix codegen when a recursive function is involved with platform dependent ABI transformations.

## [0.2.4-preview.50] - 2019-02-27

- Fix meta file conflict.
- Fix changelog format.

## [0.2.4-preview.49] - 2019-02-27

- Move back com.unity.burst.experimental for function pointers support, but use internal modifier for this API.
- Restructure package for validation.

## [0.2.4-preview.48] - 2019-02-26

- Move back com.unity.burst.experimental for function pointers support, but use internal modifier for this API.

## [0.2.4-preview.47] - 2019-02-26

- Fix an issue during publish stage which was preventing to release the binaries.

## [0.2.4-preview.46] - 2019-02-26

- iOS player builds now use static linkage (to support TestFlight)  - Minimum supported Unity versions are 2018.3.6f1 or 2019.1.0b4.
- Fix a warning in Burst AOT settings.
- Enable forcing synchronous job compilation from menu.

## [0.2.4-preview.45] - 2019-02-07

- Disable Burst AOT settings support for unity versions before 2019.1.

## [0.2.4-preview.44] - 2019-02-06

- Fix incorrect conversions when performing subtraction with enums and floats.
- Fix compatability issue with future unity versions.
- Fix bug with ldfld bitcast on structs with explicit layouts.
- Guard against an issue resolving debug locations if the scope is global.

## [0.2.4-preview.43] - 2019-02-01

- Add preliminary support for Burst AOT settings in the player settings.
- Move BurstCompile (delegate/function pointers support) from com.unity.burst package to com.unity.burst.experimental package.
- Fix issue with stackalloc allocating a pointer size for the element type resulting in possible StackOverflowException.
- Add support for disabling Burst compilation from Unity editor with the command line argument `--burst-disable-compilation` .
- Add support for forcing synchronous compilation from Unity editor with the command line argument `--burst-force-sync-compilation`.
- Fix a compiler crash when generating debugging information.
- Fix invalid codegen involving ternary operator

## [0.2.4-preview.42] - 2019-01-22

- Fix a compilation error when implicit/explicit operators are used returning different type for the same input type.

## [0.2.4-preview.41] - 2019-01-17

- Fix codegen issue with Interlocked.Decrement that was instead performing an increment.
- Fix codegen issue for an invalid layout of struct with nested recursive pointer references.
- Fix for Fogbugz case : https://fogbugz.unity3d.com/f/cases/1109514/.
- Fix codegen issue with ref bool on a method argument creating a compiler exception.

## [0.2.4-preview.40] - 2018-12-19

- Fix bug when a write to a pointer type of an argument of a generic function.
- Breaking change of API: `Accuracy` -> `FloatPrecision`, and `Support` => `FloatMode`.
- Add `FloatMode.Deterministic` mode with early preview of deterministic mathematical functions.
- Fix bug with fonts in inspector being incorrectly reloaded.

## [0.2.4-preview.39] - 2018-12-06

- Add preview support for readonly static arrays typically used for LUT.
- Fix an issue with generics incorrectly being resolved in certain situations.
- Fix ARM32/ARM64 compilation issues for some instructions.
- Fix ARM compilation issues on UWP.
- Fix issue with math.compress.
- Add support for `ldnull` for storing a managed null reference to a ref field (e.g for DisposeSentinel).

## [0.2.4-preview.38] - 2018-11-17

- Fix issue when converting an unsigned integer constant to a larger unsigned integer (e.g (ulong)uint.MaxValue).
- Fix crash in editor when IRAnalysis can return an empty string .
- Fix potential crash of Cecil when reading symbols from assembly definition.

## [0.2.4-preview.37] - 2018-11-08

- Fix a crash on Linux and MacOS in the editor with dlopen crashing when trying to load burst-llvm (linux).

## [0.2.4-preview.36] - 2018-11-08

- Fix a crash on Linux and MacOS in the editor with dlopen crashing when trying to load burst-llvm (mac).

## [0.2.4-preview.35] - 2018-10-31

- Try to fix a crash on macosx in the editor when a job is being compiled by Burst at startup time.
- Fix Burst accidentally resolving reference assemblies.
- Add support for Burst for ARM64 when building UWP player.

## [0.2.4-preview.34] - 2018-10-12

- Fix compiler exception with an invalid cast that could occur when using pinned variables (e.g `int32&` resolved to `int32**` instead of `int32*`).

## [0.2.4-preview.33] - 2018-10-10

- Fix a compiler crash with methods incorrectly being marked as external and throwing an exception related to ABI.

## [0.2.4-preview.32] - 2018-10-04

- Fix codegen and linking errors for ARM when using mathematical functions on plain floats.
- Add support for vector types GetHashCode.
- Add support for DllImport (only compatible with Unity `2018.2.12f1`+ and ` 2018.3.0b5`+).
- Fix codegen when converting uint to int when used in a binary operation.

## [0.2.4-preview.31] - 2018-09-24

- Fix codegen for fmodf to use inline functions instead.
- Add extended disassembly output to the Burst inspector.
- Fix generic resolution through de-virtualize methods.
- Fix bug when accessing float3.zero. Prevents static constructors being considered intrinsics.
- Fix NoAlias attribute checking when generics are used.

## [0.2.4-preview.30] - 2018-09-11

- Fix IsValueType throwing a NullReferenceException in case of using generics.
- Fix discovery for Burst inspector/AOT methods inheriting from IJobProcessComponentData or interfaces with generics.
- Add `[NoAlias]` attribute.
- Improved codegen for csum.
- Improved codegen for abs(int).
- Improved codegen for abs on floatN/doubleN.

## [0.2.4-preview.29] - 2018-09-07

- Fix issue when calling an explicit interface method not being matched through a generic constraint.
- Fix issue with or/and binary operation on a bool returned by a function.

## [0.2.4-preview.28] - 2018-09-05

- Fix a compilation issue when storing a bool returned from a function to a component of a bool vector.
- Fix AOT compilation issue with a duplicated dictionary key.
- Fix settings of ANDROID_NDK_ROOT if it is not setup in Unity Editor.

## [0.2.4-preview.27] - 2018-09-03

- Improve detection of jobs within nested generics for AOT/Burst inspector.
- Fix compiler bug of comparison of a pointer to null pointer.
- Fix crash compilation of sincos on ARM (neon/AARCH64).
- Fix issue when using a pointer to a VectorType resulting in an incorrect access of a vector type.
- Add support for doubles (preview).
- Improve AOT compiler error message/details if the compiler is failing before the linker.

## [0.2.4-preview.26] - 2018-08-21

- Added support for cosh, sinh and tanh.

## [0.2.4-preview.25] - 2018-08-16

- Fix warning in unity editor.

## [0.2.4-preview.24] - 2018-08-15

- Improve codegen of math.compress.
- Improve codegen of math.asfloat/asint/asuint.
- Improve codegen of math.csum for int4.
- Improve codegen of math.count_bits.
- Support for lzcnt and tzcnt intrinsics.
- Fix AOT compilation errors for PS4 and XboxOne.
- Fix an issue that could cause wrong code generation for some unsafe ptr operations.

## [0.2.4-preview.23] - 2018-07-31

- Fix bug with switch case to support not only int32.

## [0.2.4-preview.22] - 2018-07-31

- Fix issue with pointers comparison not supported.
- Fix a StackOverflow exception when calling an interface method through a generic constraint on a nested type where the declaring type is a generic.
- Fix an issue with EntityCommandBuffer.CreateEntity/AddComponent that could lead to ArgumentException/IndexOutOfRangeException.

## [0.2.4-preview.21] - 2018-07-25

- Correct issue with Android AOT compilation being unable to find the NDK.

## [0.2.4-preview.20] - 2018-07-05

- Prepare the user documentation for a public release.

## [0.2.4-preview.19] - 2018-07-02

- Fix compilation error with generics when types are coming from different assemblies.

## [0.2.4-preview.18] - 2018-06-26

- Add support for subtracting pointers.

## [0.2.4-preview.17] - 2018-06-25

- Bump only to force a new version pushed.

## [0.2.4-preview.16] - 2018-06-25

- Fix AOT compilation errors.

## [0.2.4-preview.15] - 2018-06-25

- Fix crash for certain access to readonly static variable.
- Fix StackOverflowException when using a generic parameter type into an interface method.

## [0.2.4-preview.14] - 2018-06-23

- Fix an issue with package structure that was preventing Burst to work in Unity.

## [0.2.4-preview.13] - 2018-06-22

- Add support for Burst timings menu.
- Improve codegen for sin/cos.
- Improve codegen when using swizzles on vector types.
- Add support for sincos intrinsic.
- Fix AOT deployment.

## [0.2.4-preview.12] - 2018-06-13

- Fix a bug in codegen that was collapsing methods overload of System.Threading.Interlocked to the same method.

## [0.2.4-preview.11] - 2018-06-05

- Fix exception in codegen when accessing readonly static fields from different control flow paths.

## [0.2.4-preview.10] - 2018-06-04

- Fix a potential stack overflow issue when a generic parameter constraint on a type is also referencing another generic parameter through a generic interface constraint
- Update to latest Unity.Mathematics:
  - Fix order of parameters and codegen for step functions.

## [0.2.4-preview.9] - 2018-05-29

- Fix bug when casting an IntPtr to an enum pointer that was causing an invalid codegen exception.

## [0.2.4-preview.8] - 2018-05-24

- Breaking change: Move Unity.Jobs.Accuracy/Support to Unity.Burst.
- Deprecate ComputeJobOptimizationAttribute in favor of BurstCompileAttribute.
- Fix bug when using enum with a different type than int.
- Fix bug with IL stind that could lead to a memory corruption.

## [0.2.4-preview.7] - 2018-05-22

- Add support for nested structs in SOA native arrays.
- Add support for arbitrary sized elements in full SOA native arrays.
- Fix bug with conversion from signed/unsigned integers to signed numbers (integers & floats).
- Add support for substracting pointers at IL level.
- Improve codegen with pointers arithmetic to avoid checking for overflows.

## [0.2.4-preview.6] - 2018-05-11

- Remove `bool1` from mathematics and add proper support in Burst.
- Add support for ARM platforms in the Burst inspector UI.

## [0.2.4-preview.5] - 2018-05-09

- Add support for readonly static fields.
- Add support for stackalloc.
- Fix potential crash on MacOSX when using memset is used indirectly.
- Fix crash when trying to write to a bool1*.
- Fix bug with EnableBurstCompilation checkbox not working in Unity Editor.

## [0.2.4-preview.4] - 2018-05-03

- Fix an issue on Windows with `DllNotFoundException` occurring when trying to load `burst-llvm.dll` from a user profile containing unicode characters in the folder path.
- Fix an internal compiler error occurring with IL dup instruction.

## [0.2.4-preview.3] - 2018-05-03

- Add support for struct with an explicit layout.
- Fix noalias regression (that was preventing the auto-vectorizer to work correctly on basic loops).

## 0.2.3 (21 March 2018)

- Improve error messages for static field access.
- Improve collecting of compilable job by trying to collect concrete job type instances (issue #23).

## 0.2.2 (19 March 2018)

- Improve error messages in case using `is` or `as` cast in C#.
- Improve error messages if a static delegate instance is used.
- Fix codegen error when converting a byte/ushort to a float.

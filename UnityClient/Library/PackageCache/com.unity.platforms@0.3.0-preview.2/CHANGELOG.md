# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.3.0] - 2020-04-14

Build pipeline major overhaul: build pipelines are no longer asset based, and instead must be implemented in code by deriving from `BuildPipelineBase` class. Build steps are no longer mandatory but can still be used by deriving from `BuildStepBase`.

### Added
- New class `BuildPipelineBase` which is a class based replacement for `BuildPipeline` assets. Build steps can be used to organize build code, but is not mandatory anymore.
- New class `BuildStepBase` which is an optional replacement for now obsolete `BuildStep`.
- New class `BuildStepCollection` which represent a list of build steps that can be enumerated and executed.
- New class `BuildResult` and `RunResult` that derives from new base class `ResultBase`.
- New class `BuildProcess` which describe the state of an incremental build process.
- New class `RunContext` which holds the context when a pipeline is ran.
- Methods for querying build component values have been added to `ContextBase`.
- Methods for setting build component values are now available on `ContextBase`. Note that those values are only stored in memory; the build configuration asset is unchanged.
- New method `GetComponentOrDefault` on `BuildConfiguration` which returns the component value if found, otherwise a default instance of the component type without modifying the configuration.
- New method `GetComponentTypes` on `BuildConfiguration` which returns the flatten list of all component types from the configuration and its dependencies.
- New method `SetComponent` on `BuildConfiguration` that only takes a type and sets the component value to a default instance of the component type.
- New method `BuildIncremental` on `BuildPipelineBase` which can be used to implement build pipelines that run in background.

### Changed
- Class `BuildContext` now derives from new base class `ContextBase`.
- The `RequiredComponents` and `OptionalComponents` lists previously available on `BuildStep` have been replaced with the merged list `UsedComponents` on `BuildStepBase`.
- Methods `CanBuild` and `CanRun` on `BuildConfiguration` no longer expect an out string parameter, and instead return a `BoolResult` that contains the result and the reason.

### Deprecated
- Class `BuildPipeline` is now obsolete. It has been replaced by `BuildPipelineBase` which is no longer asset based. All build pipeline assets must be converted into a corresponding build pipeline class that derives from `BuildPipelineBase`.
- Class `BuildPipelineResult` is now obsolete. It has been replaced by `BuildResult`.
- Class `BuildStep` and `RunStep` are now obsolete. Class based build pipelines no longer enforce the use of build/run steps. Most interfaces and attributes related to `BuildStep` and `RunStep` are also obsolete.
- Class `BuildStepResult` and `RunStepResult` are now obsolete. They have been replaced by `BuildResult` and `RunResult` respectively.
- Property `BuildPipelineStatus` on `BuildContext` is now obsolete. `BuildPipelineResult` and `BuildStepResult` have been combined into `BuildResult`, removing the need for this intermediate status.

### Removed
- Removed optional mutator parameter on `BuildContext` class.

## [0.2.2] - 2020-03-23

### Added
- Added `com.unity.properties.ui` package version to `1.1.1-preview`.
- Added support for `LazyLoadReference` for deserializing asset references without loading them (requires Unity 2020.1).
- DotsBuildTarget now has an overridable TargetFramework property, which can be used to change target .NET framework.

### Removed
- Removed `DotsConfig` as it now lives in `com.unity.dots.runtime-0.24.0`.
- Removed unused dependency on `com.unity.dots.runtime'.
- Removed dependency on newtonsoft json to use serialization package API instead.

### Changed
- Updated `com.unity.properties` package version to `1.1.1-preview`. This is a major overhaul, please refer to the package documentation.
- Updated `com.unity.serialization` package version to `1.1.1-preview`. This is a major overhaul, please refer to the package documentation.

### Fixed
- Show apply/revert/cancel dialog if build configuration is modified upon clicking Build and/or Run button.
- Fixed build configuration inspector when using Unity 2020.1 and above.
- Build progress bar will update after elapsed time even if no values changed.

## [0.2.1] - 2020-02-25

### Added
- Support for building testable players (`TestablePlayer` component) as a step towards integration with the Unity Test Framework.
- Add a UsesIL2CPP property to BuildTarget

### Changed
- Enable Burst for DotNet builds on Windows
- Revert namespace `Unity.Platforms.Build*` change back to `Unity.Build*`.

### Fixed
- Fix Build & Run fallback when build pipeline doesn't have a proper RunStep, BuildOption.AutoRunPlayer was being set too late, thus it didn't have any effect, this is now fixed.
- Build configuration/pipeline assets will now properly apply changes when clicking outside inspector focus.
- Fixed asset cannot be null exception when trying to store build result.

## [0.2.1-preview] - 2020-01-24

### Changed
- Modfied data format for SceneList to contain additional flags to support LiveLink.
- `BuildStepBuildClassicLiveLink` was moved into the `Unity.Scenes.Editor` assembly in `com.unity.entities` package due to dependencies on Entities.
- Refactored `BuildStepBuildClassicPlayer` since it no longer shares its implementation with `BuildStepBuildClassicLiveLink`
- `ClassicBuildProfile.GetExecutableExtension` made public so that it can be used from other packages.

## [0.2.0-preview.2] - 2020-01-17

### Fixed
- Fix `BuildStepBuildClassicLiveLink` build step to re-generate Live Link player required metadata file.

## [0.2.0-preview.1] - 2020-01-15

### Added
- Platform specific event processing support (new Unity.Platforms.Common assembly).

## [0.2.0-preview] - 2020-01-13

The package `com.unity.build` has been merged in the `com.unity.platforms` package, and includes the following changes since the release of `com.unity.build@0.1.0-preview`:

### Added
- New `BuildStepRunBefore` and `BuildStepRunAfter` attributes which can be optionally added to a `BuildStep` to declare which other steps must be run before or after that step.
- `BuildStep` attribute now support `Name`, `Description` and `Category` properties.
- Added new `RunStep` attribute to configure run step types various properties.

### Changed
- Updated `com.unity.properties` to version `0.10.4-preview`.
- Updated `com.unity.serialization` to version `0.6.4-preview`.
- All classes that should not be derived from are now properly marked as `sealed`.
- All UI related code has been moved into assembly `Unity.Build.Editor`.
- Added support for `[HideInInspector]` attribute for build components, build steps and run steps. Using that attribute will hide the corresponding type from the inspector view.
- Field `BuildStepAttribute.flags` is now obsolete. The attribute `[HideInInspector]` should now be used to hide build steps in inspector or searcher menu.
- Field `BuildStepAttribute.description` is now obsolete: it has been renamed to `BuildStepAttribute.Description`.
- Field `BuildStepAttribute.category` is now obsolete: it has been renamed to `BuildStepAttribute.Category`.
- Interface `IBuildSettingsComponent` is now obsolete: it has been renamed to `IBuildComponent`.
- Class `BuildSettings` is now obsolete: it has been renamed to `BuildConfiguration`.
- Asset extension `.buildsettings` is now obsolete: it has been renamed to `.buildconfiguration`.
- Because all build steps must derive from `BuildStep`, all methods and properties on `IBuildStep` are no longer necessary and have been removed.
- Property `BuildStep.Description` is no longer abstract, and can now be set from attribute `BuildStepAttribute(Description = "...")`.
- Enum `BuildConfiguration` is now obsolete: it has been renamed to `BuildType`.
- Interface `IRunStep` is now obsolete: run steps must derive from `RunStep`.
- Nested `BuildPipeline` build steps are now executed as a flat list from the main `BuildPipeline`, rather than calling `IBuildStep.RunBuildStep` recursively on them.
- Build step cleanup pass will only be executed if the default implementation is overridden, greatly reducing irrelevant logging in `BuildPipelineResult`.
- Class `ComponentContainer` should not be instantiated directly and thus has been properly marked as `abstract`.
- Class `ComponentContainer` is now obsolete: it has been renamed to `HierarchicalComponentContainer`.

### Fixed
- Empty dependencies in inspector are now properly supported again.
- Dependencies label in inspector will now as "Dependencies" again.

## [0.1.8-preview] - 2019-12-11

### Added
- Added Unity.Build.Common files, moved them from com.unity.entities.

## [0.1.7-preview.3] - 2019-12-09

### Changed
- Disabled burst for windows/dotnet/collections checks, because it was broken.

## [0.1.7-preview.2] - 2019-11-12

### Changed
- Changed the way platforms customize builds for dots runtime, in a way that makes buildsettings usage clearer and faster, and more reliable.

## [0.1.7-preview] - 2019-10-25

### Added
- Added `WriteBeeConfigFile` method to pass build target specifc configuration to Bee.

## [0.1.6-preview] - 2019-10-23

### Added
- Re-introduce the concept of "buildable" build targets with the `CanBuild` property.

### Changed
- `GetDisplayName` method changed for `DisplayName` property.
- `GetUnityPlatformName` method changed for `UnityPlatformName` property.
- `GetExecutableExtension` method changed for `ExecutableExtension` property.
- `GetBeeTargetName` method changed for `BeeTargetName` property.

## [0.1.5-preview] - 2019-10-22

### Added
- Added static method `GetBuildTargetFromUnityPlatformName` to find build target that match Unity platform name. If build target is not found, an `UnknownBuildTarget` will be returned.
- Added static method `GetBuildTargetFromBeeTargetName` to find build target that match Bee target name. If build target is not found, an `UnknownBuildTarget` will be returned.

### Changed
- `AvailableBuildTargets` will now contain all build targets regardless of `HideInBuildTargetPopup` value, as well as `UnknownBuildTarget` instances.

## [0.1.4-preview] - 2019-09-26
- Bug fixes  
- Add iOS platform support
- Add desktop platforms package

## [0.1.3-preview] - 2019-09-03

- Bug fixes  

## [0.1.2-preview] - 2019-08-13

### Added
- Added static `AvailableBuildTargets` property to `BuildTarget` class, which provides the list of available build targets for the running Unity editor platform.
- Added static `DefaultBuildTarget` property to `BuildTarget` class, which provides the default build target for the running Unity editor platform.

### Changed
- Support for Unity 2019.1.

## [0.1.1-preview] - 2019-06-10

- Initial release of *Unity.Platforms*.

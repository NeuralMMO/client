# Changelog
All notable changes to this package will be documented in this file.

## [1.2.0] - 2020-04-03
### Changed
* Update `com.unity.properties` to version `1.2.0-preview`.
* Update `com.unity.serialization` to version `1.2.0-preview`.

## [1.1.1] - 2020-03-20
### Fixed
* Fix `AttributeFilter` incorrectly being called on the internal property wrapper.

### Changed
* Update `com.unity.properties` to version `1.1.1-preview`.
* Update `com.unity.serialization` to version `1.1.1-preview`.

## [1.1.0] - 2020-03-11
### Fixed
* Fixed background color not being used when adding new collection items.
* Fixed readonly arrays being resizable from the inspector.

### Changed
* Update `com.unity.properties` to version `1.1.0-preview`.
* Update `com.unity.serialization` to version `1.1.0-preview`.

### Added
* Added the `InspectorAttribute`, allowing to put property attributes on both fields and properties.
* Added the `DelayedValueAttribute`, which works similarly to `UnityEngine.DelayedAttribute`, but can work with properties.
* Added the `DisplayNameAttribute`, which works similarly to `UnityEngine.InspectorNameAttribute`, but can work with properties.
* Added the `MinValueAttribute`, which works similarly to `UnityEngine.MinAttribute`, but can work with properties.
* Added built-in inspector for `LazyLoadReference`.

## [1.0.0] - 2020-03-02
### Changed
* ***Breaking change*** Complete API overhaul, see the package documentation for details.

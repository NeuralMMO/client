# Changelog
## [1.0.16] - 2019-06-20
- Fixed an issue where the Test Runner window popped out if it was docked, or if something else was docked next to it, when re-opened (case 1158961)
- Fixed a regression where the running standalone playmode tests from the ui would result in an error.

## [1.0.15] - 2019-06-18
- Added new `[TestMustExpectAllLogs]` attribute, which automatically does `LogAssert.NoUnexpectedReceived()` at the end of affected tests. See docs for this attribute for more info on usage.
- Fixed a regression where no tests would be run if multiple filters are specified. E.g. selecting both a whole assembly and an individual test in the ui.
- Fixed an issue where performing `Run Selected` on a selected assembly would run all assemblies.
- Introduced the capability to do a split build and run, when running playmode tests on standalone devices.
- Fixed an error in ConditionalIgnore, if the condition were not set.

## [1.0.14] - 2019-05-27
- Fixed issue preventing scene creation in IPrebuildSetup.Setup callback when running standalone playmode tests.
- Fixed an issue where test assemblies would sometimes not be ordered alphabetically.
- Added module references to the package for the required modules: imgui and jsonserialize.
- Added a ConditionalIgnore attribute to help ignoring tests only under specific conditions.
- Fixed a typo in the player test window (case 1148671).

## [1.0.13] - 2019-05-07
- Fixed a regression where results from the player would no longer update correctly in the UI (case 1151147).

## [1.0.12] - 2019-04-16
- Added specific unity release to the package information.

## [1.0.11] - 2019-04-10
- Fixed a regression from 1.0.10 where test-started events were triggered multiple times after a domain reload.

## [1.0.10] - 2019-04-08
- Fixed an issue where test-started events would not be fired correctly after a test performing a domain reload (case 1141530).
- The UI should correctly run tests inside a nested class, when that class is selected.
- All actions should now correctly display a prefix when reporting test result. E.g. "TearDown :".
- Errors logged with Debug.LogError in TearDowns now append the error, rather than overwriting the existing result (case 1114306).
- Incorrect implementations of IWrapTestMethod and IWrapSetUpTearDown now gives a meaningful error.
- Fixed a regression where the Test Framework would run TearDown in a base class before the inheriting class (case 1142553).
- Fixed a regression introduced in 1.0.9 where tests with the Explicit attribute could no longer be executed.

## [1.0.9] - 2019-03-27
- Fixed an issue where a corrupt instance of the test runner window would block for a new being opened.
- Added the required modules to the list of package requirements.
- Fixed an issue where errors would happen if the test filter ui was clicked before the ui is done loading.
- Fix selecting items with duplicate names in test hierarchy of Test Runner window (case 987587).
- Fixed RecompileScripts instruction which we use in tests (case 1128994).
- Fixed an issue where using multiple filters on tests would sometimes give an incorrect result.

## [1.0.7] - 2019-03-12
### This is the first release of *Unity Package com.unity.test-framework*.

- Migrated the test-framework from the current extension in unity.

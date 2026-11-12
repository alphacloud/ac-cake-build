# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0]

Replaces 1.0.0. The `#load`-able Cake scripts have been rewritten as a Cake Frosting library, distributed as the
NuGet package `Alphacloud.CakeBuild`. See README "Migrating from 1.x".

### Added
- `Alphacloud.CakeBuild` NuGet package (net10.0, net11.0; Cake.Frosting 7.0.0) with `CakeHost.UseAlphacloudCakeBuild(settings)`.
- Standardized repository layout, configurable through `ProjectSettings.Conventions` (`Src`, `Lib`, `Tests`, `Samples`, `Common`, `artifacts`).
- `ValidateConventions` task (runs first in `Build`), which reports layout problems with actionable messages.
- `ProjectSettings.Tests`: `ExcludedCategories`, `CategoryTrait`, `RunReleaseTests`, `SequentialTestModules`.
- `ProjectSettings.ReleaseTargetBranch` (previously hardcoded to `master`).
- TRX test results are uploaded to AppVeyor.
- Support for `.slnx` solutions (preferred over `.sln`).

### Changed
- **Breaking:** tests run on xunit.v3 v4 / Microsoft.Testing.Platform, and coverage is collected with dotnet-coverage
  into one merged Cobertura report covering all projects and target frameworks (replaces OpenCover).
- **Breaking:** `CodeCoverageSettings` uses dotnet-coverage regex filters: `IncludeModulePaths`, `ExcludeModulePaths`,
  `ExcludeAttributes`, `ExcludeSources`.
- **Breaking:** `BuildInfo` is replaced by `BuildContext`. The default source folder is `Src`.
- The NuGet version comes from GitVersion `SemVer` (GitVersion 6 removed `NuGetVersion`). Pull-request builds use `0.<PR>.<build>`.
- Coveralls upload uses the coverage-reporter binary (`COVERALLS_REPORTER_EXE`).
- Tools are pinned: GitVersion.Tool 6.8.2, GitReleaseManager.Tool 0.20.0, dotnet-coverage 18.12.0, ReportGenerator 5.6.0.

### Removed
- `src/definitions.cake` and `src/tasks.cake`, together with the Cake.Coveralls, Cake.Coverlet, Cake.FileHelpers and
  Cake.AppVeyor addins.

### Fixed
- Repository information is calculated once per build.
- The test pipeline fails after the coverage report and upload, so coverage is still published when tests fail.

## [1.0.0]

### Added
- Initial cake build scripts
- Basic project structure
- Core build definitions and tasks

[Unreleased]: https://github.com/alphacloud/ac-cake-build/compare/2.0.0...HEAD
[2.0.0]: https://github.com/alphacloud/ac-cake-build/compare/1.0.0...2.0.0
[1.0.0]: https://github.com/alphacloud/ac-cake-build/releases/tag/1.0.0

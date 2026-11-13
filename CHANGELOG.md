# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0]

Replaces 1.0.0. The `#load`-able Cake scripts have been rewritten as a Cake Frosting library, distributed as the
NuGet package `Alphacloud.CakeBuild`. To upgrade, follow [docs/migrating-from-submodule.md](docs/migrating-from-submodule.md).

### Added
- Migration runbook `docs/migrating-from-submodule.md` for moving consumer repositories from the 1.x submodule to the
  NuGet package. It is written so an AI coding agent can run it on its own: ordered steps with checks, stop-and-ask
  blockers, settings and coverage-filter translation tables, a failure-symptom table, and a report template.
- `Alphacloud.CakeBuild` NuGet package (net10.0, net11.0; Cake.Frosting 7.0.0) with `CakeHost.UseAlphacloudCakeBuild(settings)`.
- Standardized repository layout, configurable through `ProjectSettings.Conventions` (`Src`, `Lib`, `Tests`, `Samples`, `Common`, `artifacts`).
- `ValidateConventions` task (runs first in `Build`), which reports layout problems with actionable messages.
- `ProjectSettings.Tests`: `ExcludedCategories`, `CategoryTrait`, `RunReleaseTests`, `SequentialTestModules`
  (default `true`, which passes `--max-parallel-test-modules 1`).
- `ProjectSettings.ReleaseTargetBranch` (previously hardcoded to `master`).
- `UploadTestResults` task (AppVeyor only): publishes the TRX test results to the AppVeyor Tests tab.
- Support for `.slnx` solutions (preferred over `.sln`).
- `ToolResolver`, the public helper that locates the pinned tools. It handles the `.exe` suffix on Windows.
- Documented the `BuildContext` API for consumer tasks. `BuildVersion.NextMajor`, `BuildVersion.CommitHash`,
  `RepositoryInfo.IsDevelopBranch` and `BuildContext.Target` are not used by built-in tasks; they are kept for
  consumer tasks.

### Requirements
- The repository path and every path under it must not contain spaces. The test command is nested inside
  `dotnet-coverage collect` as one argument.
- Test projects must use xunit.v3 4.x on Microsoft.Testing.Platform, enabled through `global.json`.

### Changed
- **Breaking:** tests run on xunit.v3 v4 / Microsoft.Testing.Platform, and coverage is collected with dotnet-coverage
  into one merged Cobertura report covering all projects and target frameworks (replaces OpenCover).
- **Breaking:** `CodeCoverageSettings` uses dotnet-coverage regex filters: `IncludeModulePaths`, `ExcludeModulePaths`,
  `ExcludeAttributes`, `ExcludeSources`.
- **Breaking:** `BuildInfo` is replaced by `BuildContext`. The default source folder is `Src`.
- The NuGet version comes from GitVersion `SemVer` (GitVersion 6 removed `NuGetVersion`). Pull-request builds use `0.<PR>.<build>`.
- Coveralls upload uses the coverage-reporter binary (`COVERALLS_REPORTER_EXE`).
- `CreateRelease` and `CloseMilestone` fail fast with a clear message when `GITHUB_TOKEN` is missing.
- `ValidateConventions` reports letter-case mismatches in folder names on every OS, not only Linux.
- `Build` runs the Debug build used for coverage for every configuration other than `Debug`. Before, it ran only
  for `Release`.
- Release-config test failures are deferred like Debug failures. `RunUnitTests` re-throws them after coverage and
  test results are reported.
- A missing pinned tool (GitVersion, dotnet-coverage) now fails the build. Before, Cake fell back to whatever
  version it found.
- Tools are pinned: GitVersion.Tool 6.8.2, GitReleaseManager.Tool 0.20.0, dotnet-coverage 18.12.0, ReportGenerator 5.6.0.

### Removed
- `src/definitions.cake` and `src/tasks.cake`, together with the Cake.Coveralls, Cake.Coverlet, Cake.FileHelpers and
  Cake.AppVeyor addins.

### Fixed
- Release branches are detected by prefix. A branch counts only when its name starts with `release/`,
  `releases/`, `hotfix/` or `hotfixes/`. Before, the prefix could appear anywhere in the name, so a branch such as
  `feature/release/x` matched and could create a GitHub release.
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

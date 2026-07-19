# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Migrated code coverage collection from OpenCover to Coverlet (`Cake.Coverlet` addin); `CodeCoverageSettings.IncludeFilter`/`ExcludeFilter` are now `List<string>` using Coverlet's `[Assembly]Type` filter syntax instead of OpenCover's `+`/`-` prefixed string filters.
- `BuildInfo.Get` now synthesizes a `0.<PRNumber>.<BuildNumber>` version for pull-request builds instead of calling GitVersion, which fails on PR builds.
- Bumped `Cake.Coveralls` (4.0.0 → 6.0.0), `Cake.FileHelpers` (7.0.0 → 9.0.0), `Cake.AppVeyor` (6.0.0 → 10.0.0), and switched `GitVersion.CommandLine` 5.12.0 → `GitVersion.Tool` 6.0.0 and `GitReleaseManager` → `GitReleaseManager.Tool` (pinned 0.20.0).
- Removed the `OpenCover` tool dependency (superseded by Coverlet); `ReportGenerator` now merges per-target-framework Coverlet output via a glob pattern.

### Fixed
- `ProjectSettings`'s default `CodeCoverage.IncludeFilter` now correctly interpolates the `solutionName` constructor parameter (was emitting the literal string `"solutionName*"` instead of the actual solution name).

## [1.0.0]

### Added
- Initial cake build scripts
- Basic project structure
- Core build definitions and tasks

[Unreleased]: https://github.com/alphacloud/ac-cake-build/compare/1.0.0...HEAD
[1.0.0]: https://github.com/alphacloud/ac-cake-build/releases/tag/1.0.0
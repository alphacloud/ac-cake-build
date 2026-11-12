# Project Brief: ac-cake-build / Alphacloud.CakeBuild

## What it is
Source of the **NuGet package `Alphacloud.CakeBuild`**, a Cake Frosting class library (net10.0;net11.0,
Cake.Frosting 7.0.0). Repo: https://github.com/alphacloud/ac-cake-build (default branch `master`, MIT).

Consumers use it for **local builds and AppVeyor pipelines** that build, test, pack and release
**multi-TFM .NET libraries**. Every consumer follows a **standardized folder and naming convention**:
`Src/{<Solution>.slnx, Directory.Build.props, Common, Lib, Tests, Samples}`, plus `GitVersion.yml` at the root.
Folder names are configurable.

The pipeline: restore → build (Debug for coverage, plus Release) → xunit.v3 v4 tests on MTP under dotnet-coverage
(one merged Cobertura report) → NuGet pack → GitHub draft release on release/hotfix branches → close milestone
on tags.

## Core requirements
1. Consumers write very little code: `new CakeHost().UseAlphacloudCakeBuild(new ProjectSettings(owner, repo, solution)).Run(args)`.
2. The same entry point works locally (HTML coverage report) and on AppVeyor (build number, TRX upload, Coveralls,
   releases).
3. Versions come from GitVersion (`SemVer`). Pull-request builds get `0.<PR>.<build>`.
4. The GitFlow release workflow is followed (see the Sharp-Architecture `docs/release-workflow.md`).
5. Releases follow SemVer and are recorded in CHANGELOG (Keep a Changelog). Task names are public API.

## History
- 1.0.0: `#load`-able `.cake` scripts consumed as a git submodule `.build/`. Still available on the `1.0.0` tag.
- 2.0.0 (issue #2, branch `feature/cake-frosting`): rewritten as the Frosting NuGet package. The scripts were deleted.

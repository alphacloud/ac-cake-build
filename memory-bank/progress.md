# Progress

## Done
- 1.0.0 tag: the `.cake` scripts (OpenCover).
- Commit 62e312e: the Coverlet / Cake v6 script update. It was never tagged and is superseded by 2.0.0.
- **Issue #2, the Frosting NuGet package (2.0.0 content):**
  - [x] Library: settings, conventions, `BuildContext`, `UseAlphacloudCakeBuild`, 15 tasks (14 existing plus
        `ValidateConventions`)
  - [x] dotnet-coverage + MTP test run with one merged Cobertura file; Release-config test run; deferred failure
  - [x] 40 unit tests × 2 TFMs; 0 build warnings (`TreatWarningsAsErrors`)
  - [x] `pack-local.ps1` puts 0.1.0 into `W:\Data\nuget\temp`
  - [x] Sample consumer `Src/Samples/SampleSolution` (library: netstandard2.0, net8.0, net10.0; tests: net8.0,
        net10.0; one ManualTests test)
  - [x] Self-build `build/`, `appveyor.yml`, `GitVersion.yml`, `GitReleaseManager.yaml`, `.gitignore`, `global.json`
  - [x] Docs: README, CHANGELOG 2.0.0, AGENTS.md, blueprints, memory bank
  - [x] Migration runbook `docs/migrating-from-submodule.md` (written for agents; README links it, with a hand-off prompt)
  - [ ] Migrate the consumers with the runbook (Autofac.Extras.Quartz, Sharp-Architecture)

## Validation results (local, 2026-11-12)
- `--tree` shows the expected graph.
- Sample `Default` run: all non-CI tasks succeeded.
  - Coverage was 85.7% (the Multiply method, which no test calls, is uncovered as intended).
  - ManualTests was excluded: 3 tests per TFM.
  - The `.nupkg` was produced. `AssemblyVersion.cs` held GitVersion values.
- Renaming the Lib folder made `ValidateConventions` fail with "Library folder 'Src/Lib' not found. Set
  ProjectSettings.Conventions.LibDir...".
- A failing test: the coverage file and report were still produced, then `RunUnitTests` failed with exit code 1.
- Self-build of this repo succeeded: 80/80 tests in Debug and Release, and the package was produced. Library coverage
  is 43.5% because task wrappers aren't unit tested.

## Known issues / risks
- AppVeyor-only code paths are not exercised locally (see activeContext).
- Tasks have no unit tests (they are thin wrappers). The pure logic is at 95–100%.
- Cobertura uses absolute paths, and the Coveralls `--base-path` behaviour still needs confirming on CI.
- On Linux the tool resolver looks for `dotnet-coverage`/`dotnet-gitversion` without `.exe`. That is only tested on Windows.

## Decision history
- 2026-07: OpenCover → Coverlet (scripts).
- 2026-11: scripts → Frosting NuGet package. Coverlet → dotnet-coverage + MTP. Enforced folder conventions.

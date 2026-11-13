# Plan: Convert ac-cake-build into the Cake Frosting NuGet package `Alphacloud.CakeBuild`

GitHub issue: **#2**. Branch: `feature/cake-frosting`. Release: **2.0.0** (breaking).

## Goal

Replace the `#load`-able scripts (`src/definitions.cake`, `src/tasks.cake`) with a NuGet package,
`Alphacloud.CakeBuild` (a Cake Frosting class library). It drives local builds and AppVeyor pipelines that
build, test, pack and release multi-TFM .NET libraries laid out according to a standardized folder and
naming convention.

Tests and coverage use **dotnet-coverage** + **xunit.v3 v4** on **Microsoft.Testing.Platform (MTP)**:
one `dotnet-coverage collect` wraps the whole `dotnet test` run. The release flow follows
`W:\vk\Projects\SharpArch\Sharp-Architecture\docs\release-workflow.md`
(GitFlow, GitVersion, GitReleaseManager, AppVeyor).

## Decisions (agreed with the user, 2026-11-12)

| # | Topic | Decision |
|---|---|---|
| - | Distribution | NuGet package (not a submodule); the consumer's `build/Build.csproj` references it |
| - | Source folder | Settable via `Conventions.SrcDir`, default **`Src`** (PascalCase like `Lib`, `Tests`, `Samples`, `Common`) |
| Q1 | TFMs | `net10.0;net11.0` (Cake.Frosting 7.0.0 ships both) |
| Q2 | Release-config tests | Kept, behind `ProjectSettings.RunReleaseTests = true` (default) |
| Q3 | Old `.cake` scripts | Deleted in 2.0.0; submodule users stay on tag `1.0.0` |
| Q4 | `[Unreleased]` Coverlet entries | Folded into 2.0.0 (dropped); 2.0.0 directly replaces 1.0.0 |
| Q5 | Commit subject | `#2 Convert cake build scripts to Cake Frosting NuGet library` |
| - | Dev/validation | The sample solution under `Src/Samples/` consumes package **0.1.0**, which is packed into the local feed `W:\Data\nuget\temp` (user-level NuGet source `Temp`) |
| R1 | Paths | **No spaces** in the repository path or any path under it. This is a documented requirement, not handled in code: the test command is nested inside `dotnet-coverage collect` as one quoted argument. |
| R2 | Tool lookup | All platform executable naming (`.exe` on Windows) goes through `ToolResolver`. A missing pinned tool fails the build instead of falling back to another version. |
| R3 | Unused public API | `BuildVersion.NextMajor`, `BuildVersion.CommitHash`, `RepositoryInfo.IsDevelopBranch` and `BuildContext.Target` are kept and documented for consumer-defined tasks. |
| R4 | TRX upload | A separate `UploadTestResults` task, gated with `ShouldRun(IsAppVeyor)`. Previously it was a branch inside `RunXunitTests`. |

### Behaviour changes accepted at code review (2026-11-12)

These changes were not in the original plan. All were reviewed and accepted:

- `TestSettings.SequentialTestModules` (default `true`) controls `--max-parallel-test-modules 1`.
- `CreateRelease` and `CloseMilestone` fail fast with a clear message when `GITHUB_TOKEN` is missing. 1.x passed an
  empty token straight to GitReleaseManager.
- `ValidateConventions` reports letter-case mismatches in folder names on Windows and macOS too, so they are caught
  before a Linux CI build fails.
- `Build` runs the extra Debug build (used for coverage) for every configuration other than `Debug`. In 1.x it ran
  only when the configuration was `Release`.
- Release-config test failures are deferred together with the Debug failures and re-thrown by `RunUnitTests` after
  coverage is reported. In 1.x the Release test run failed immediately.

## 1. Architecture

- `Src/Lib/Alphacloud.CakeBuild` is the class library (`Cake.Frosting` 7.0.0) that gets packed as the NuGet package.
- Consumer entry point:

  ```csharp
  return new CakeHost()
      .UseAlphacloudCakeBuild(new ProjectSettings("owner", "Repo", "SolutionName"))
      .Run(args);
  ```

  `UseAlphacloudCakeBuild` does all of the setup:
  - registers `ProjectSettings` with DI
  - calls `UseContext<BuildContext>()` and adds the lifetime hook
  - calls `AddAssembly(library)` so the library's tasks are discovered
  - installs the pinned tools via `InstallTool(...)`
  - sets the working directory to the repo root (the parent of `build/`)

  Consumers can still add their own tasks.
- `BuildContext : FrostingContext` is built by DI from `(ICakeContext, ProjectSettings)`. It replaces `BuildInfo`
  and `Setup`. It holds:
  - target and config
  - `RepositoryInfo` (computed once)
  - the version
  - `GITHUB_TOKEN`
  - `Paths`

  The artifacts directory is prepared in `BuildLifetime.Setup`.
- Pure logic lives in small classes that can be unit tested without Cake: branch detection, the PR version,
  the coverage settings XML, convention checks, and test arguments.
- Dropped addins: `Cake.Coverlet`, `Cake.Coveralls`, `Cake.AppVeyor`, `Cake.FileHelpers`. Replacements:
  - `BuildSystem.AppVeyor` from Cake.Common
  - `coveralls-windows.exe` (path in `COVERALLS_REPORTER_EXE`)
  - `XmlPoke` from Cake.Common

## 2. Folder & naming conventions (enforced by `ValidateConventions`)

```
<repo>/
├── build/Build.csproj           # Frosting host -> PackageReference Alphacloud.CakeBuild
├── build.ps1 / build.sh
├── global.json                  # SDK pin + { "test": { "runner": "Microsoft.Testing.Platform" } }
├── GitVersion.yml, GitReleaseManager.yaml, appveyor.yml
└── Src/                         # Conventions.SrcDir
    ├── <SolutionName>.slnx|.sln # .slnx preferred
    ├── Directory.Build.props    # PropertyGroup Label="Package" -> PackageReleaseNotes
    ├── Common/AssemblyVersion.cs   # generated by SetVersion
    ├── Lib/<Package>/           # packable multi-TFM libraries
    ├── Tests/<*Tests*>/         # xunit.v3 v4 (MTP) test projects
    └── Samples/                 # optional, never packed
```

All folder names in `ConventionSettings` can be overridden:

| Setting | Default |
|---|---|
| `SrcDir` | `Src` |
| `LibDir` | `Lib` |
| `TestsDir` | `Tests` |
| `SamplesDir` | `Samples` |
| `CommonDir` | `Common` |
| `ArtifactsDir` | `artifacts` |

Case matters on Linux, so Autofac.Extras.Quartz (whose source folder is `src/`) sets `SrcDir = "src"`.

## 3. Versions

| Component | Version | Provider |
|---|---|---|
| Cake.Frosting | 7.0.0 | PackageReference |
| GitVersion.Tool | 6.8.2 | `dotnet:` (InstallTool) |
| GitReleaseManager.Tool | 0.20.0 | `dotnet:` |
| dotnet-coverage | 18.12.0 | `dotnet:` |
| ReportGenerator | 5.6.0 | `nuget:` |
| xunit.v3 | 4.0.3 (MTP v2) | PackageReference (tests) |
| Coveralls reporter | `coveralls-windows.exe` | CI download |

## 4. Task graph (all 14 names kept + `ValidateConventions`)

- `Default` -> `UpdateAppVeyorBuildNumber`, `Build`, `RunUnitTests`, `CreateNugetPackages`, `CreateRelease`, `CloseMilestone`
- `Build` -> `ValidateConventions`, `SetVersion`, `UpdateAppVeyorBuildNumber`, `UpdateReleaseNotesLink`, `Restore`
- `RunUnitTests` -> `Build`, `CleanPreviousTestResults`, `RunXunitTests`, `UploadTestResults`, `GenerateCoverageReport`, `UploadCoverage`
- `CleanAll` (standalone)

Changes compared to 1.x:

- **Test filter:** `--filter "Category!=ManualTests"` (VSTest) is replaced by the MTP form
  `--filter-not-trait Category=ManualTests`. The excluded categories come from `TestSettings.ExcludedCategories`.
  The test command also passes `--ignore-exit-code 8`.
- **Version:** GitVersion 6 removed `NuGetVersion`, so the version comes from `SemVer`. `ToolPath` is forced to
  `dotnet-gitversion`.
- **AppVeyor test results:** test runs write TRX files (`--report-xunit-trx`), uploaded with
  `AppVeyor.UploadTestResults(..., MSTest)`.
- **Coverage:** one `dotnet-coverage collect -s codecoverage.config -f cobertura` call wraps `dotnet test`. The settings
  XML is generated from `CodeCoverageSettings` (module path, attribute and source regexes). The result is a single
  merged Cobertura file.
- **Coveralls:** one `report` call with `--base-path=<repo root>` (the coverage-reporter flag name).
- **Release tests:** `RunReleaseTests` runs the tests a second time in the Release config, without coverage. As in
  1.x, a test failure is deferred until the coverage output is written.
- **Release target branch:** `TargetCommitish` is taken from `ProjectSettings.ReleaseTargetBranch` (default `master`).

## 5. Repository layout (this repo)

```
ac-cake-build/
├── build/Build.csproj, build.ps1, build.sh   # self-build (ProjectReference to the library)
├── pack-local.ps1                            # packs 0.1.0 -> W:\Data\nuget\temp, clears the cached copy
├── global.json, GitVersion.yml, GitReleaseManager.yaml, appveyor.yml, .gitignore
├── Src/
│   ├── Alphacloud.CakeBuild.slnx, Directory.Build.props, Common/AssemblyVersion.cs
│   ├── Lib/Alphacloud.CakeBuild/
│   ├── Tests/Alphacloud.CakeBuild.Tests/   # xunit.v3 4.0.3 unit tests
│   └── Samples/SampleSolution/             # standalone consumer of the 0.1.0 package (not in the slnx)
└── README.md, CHANGELOG.md, AGENTS.md, blueprints/, memory-bank/
```

## 6. Implementation steps

1. Delete `src/*.cake`, then add the config files.
2. Write the library: settings, `BuildContext`, `UseAlphacloudCakeBuild`, the helper classes, and the tasks.
3. Write the unit tests.
4. Write `pack-local.ps1` and the sample solution (a multi-TFM library and tests, including one ManualTests test).
5. Add this repo's own `build/` host.
6. Update the docs: README, CHANGELOG 2.0.0, AGENTS.md, blueprints, and the memory bank.
7. Run `codegraph init && codegraph index .`, then commit with the subject `#2 ...`.

## 7. Validation

1. `dotnet build` and the unit tests pass on both net10.0 and net11.0.
2. `pack-local.ps1` produces `Alphacloud.CakeBuild.0.1.0.nupkg` in `W:\Data\nuget\temp`.
3. The sample's `build/` restores 0.1.0 from the `Temp` source, and `--tree` shows the expected task graph.
4. A `Default` run on the sample produces:
   - a merged Cobertura file and an HTML report
   - TRX files, with the ManualTests test excluded
   - the Release-config test run
   - `.nupkg` files in `artifacts/packages`
5. CI-only tasks are skipped on a local run.
6. A broken folder layout makes `ValidateConventions` fail with a clear message.


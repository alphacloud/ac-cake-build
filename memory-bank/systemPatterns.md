# System Patterns

## Library structure (`Src/Lib/Alphacloud.CakeBuild`, namespace `Alphacloud.CakeBuild`)
| File | Role |
|---|---|
| `ProjectSettings` | ctor `(repoOwner, repoName, solutionName)`. Get-only sub-settings: `Conventions`, `CodeCoverage`, `Tests`. Also `ReleaseTargetBranch` (default `master`). The default coverage include regex is derived from the solution name |
| `ConventionSettings` | Folder names: `SrcDir=Src`, `LibDir=Lib`, `TestsDir=Tests`, `SamplesDir=Samples`, `CommonDir=Common`, `ArtifactsDir=artifacts` |
| `CodeCoverageSettings` | dotnet-coverage regex lists: `IncludeModulePaths`, `ExcludeModulePaths`, `ExcludeAttributes`, `ExcludeSources` |
| `TestSettings` | `CategoryTrait=Category`, `ExcludedCategories=[ManualTests]`, `RunReleaseTests=true`, `SequentialTestModules=true` |
| `Paths` | Absolute paths built from root + conventions. Prefers `.slnx` over `.sln`. Artifacts: `packages/`, `test-results/{Debug,Release}`, `coverage/{codecoverage.config,coverage.cobertura.xml}`, `CodeCoverageReport/` |
| `RepositoryInfo.Create(...)` | Pure GitFlow flags. `IsMain` = the AppVeyor repo name equals owner/repo (upstream, not a fork). `IsReleaseBranch` = the branch name **starts with** `release/`, `releases/`, `hotfix/` or `hotfixes/` (case-insensitive) |
| `BuildVersion` (record) | `ForPullRequest(pr, build, sha, title)` → `0.pr.build` |
| `ConventionValidator` | Returns a list of errors. `DirectoryExistsExact` checks case. Checks for `PropertyGroup[@Label=Package]/PackageReleaseNotes` |
| `CoverageSettingsXml` | Generates the `<Configuration><CodeCoverage>` XML for `dotnet-coverage collect -s` |
| `TestCommand` | MTP `dotnet test` args: `--solution`, `--no-build`, `--report-xunit-trx`, `--ignore-exit-code 8`, `--max-parallel-test-modules 1`, `--filter-not-trait Category=ManualTests` |
| `BuildContext : FrostingContext` | DI-constructed from `(ICakeContext, ProjectSettings)`. Holds Target, Config, IsRelease, IsLocal, IsAppVeyor, Repository, Version (GitVersion SemVer with forced dotnet-gitversion ToolPath), GitHubToken, Paths, and `TestFailure` (deferred) |
| `BuildLifetime` | Setup: ensure the artifacts dir exists and clean it |
| `CakeHostExtensions.UseAlphacloudCakeBuild(settings, workingDirectory="..")` | Registers settings in DI, then calls UseContext, UseLifetime, UseWorkingDirectory, AddAssembly and InstallTool(`Tools.All`) |
| `Tools` | Pinned tool URIs |
| `ToolResolver` | The **only** place that knows about `.exe`: `ExecutableName(name, isWindows?)` (unit tested). `Resolve(ctx, name)` throws `CakeException` when the tool is missing, so it never falls back to a stale or global copy |

## Task graph (`Tasks/*.cs`, `FrostingTask<BuildContext>`)
```
Default ─► UpdateAppVeyorBuildNumber, Build, RunUnitTests, CreateNugetPackages, CreateRelease, CloseMilestone
Build   ─► ValidateConventions, SetVersion, UpdateAppVeyorBuildNumber, UpdateReleaseNotesLink, Restore
RunUnitTests ─► Build, CleanPreviousTestResults, RunXunitTests, UploadTestResults, GenerateCoverageReport, UploadCoverage
CleanAll (standalone)
```
- Environment gating uses `ShouldRun`:
  - AppVeyor: `UpdateAppVeyorBuildNumber` (its `OnError` only logs a warning)
  - tagged builds: `UpdateReleaseNotesLink`
  - local builds: `GenerateCoverageReport`
  - non-local builds: `UploadCoverage`
  - `IsMain && IsReleaseBranch && !PR`: `CreateRelease`
  - `IsMain && IsTagged && !PR`: `CloseMilestone`
- **Deferred test failure:** `RunXunitTests` records failures in `context.TestFailure` and does not throw. Coverage
  report and upload still run, then `RunUnitTests` throws.
- `RunXunitTests` runs `dotnet-coverage collect --settings ... --output-format cobertura --output ... "dotnet test ..."`
  for Debug. When `RunReleaseTests && IsRelease`, it also runs plain `dotnet test` for Release.
- `UploadTestResults` is its own task, gated by `ShouldRun(IsAppVeyor)`. It uploads TRX files
  (`AppVeyorTestResultsType.MSTest`); upload errors only log a warning.
- **Requirement: no spaces in the repo path.** The `dotnet test` command line is nested inside `dotnet-coverage
  collect` as one quoted argument. This is documented (README conventions, runbook blocker 5, CHANGELOG), not handled
  in code.
- Unused public API, kept on purpose and documented for consumer tasks: `BuildVersion.NextMajor`,
  `BuildVersion.CommitHash`, `RepositoryInfo.IsDevelopBranch`, `BuildContext.Target`.
- `UploadCoverage` runs `<COVERALLS_REPORTER_EXE> report <file> --format=cobertura --base-path=<root> --no-logo`.

## Design rules
- Keep tasks thin. Logic lives in pure, unit-tested classes.
- Task names are public API.
- Every public member has an XML doc comment. `TreatWarningsAsErrors` is on.
- In task files, `Path` is ambiguous (Cake.Core.IO vs System.IO), so add `using Path = System.IO.Path;`.

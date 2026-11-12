# ac-cake-build

## Project Settings

Stack: dotnet

## What this repo is

The source of the **`Alphacloud.CakeBuild`** NuGet package. It is a Cake Frosting class library (net10.0;net11.0,
Cake.Frosting 7.0.0) that gives every alphacloud .NET library the same pipeline, run locally and on AppVeyor:
restore → build → test + coverage → pack → GitHub release → close milestone.
Consumers reference the package from a small `build/Build.csproj` host and call
`new CakeHost().UseAlphacloudCakeBuild(new ProjectSettings(owner, repo, solution)).Run(args)`.

Version 1.x was a git submodule of `#load`-able `.cake` scripts. Those scripts were removed in 2.0.0, and 1.x
lives only on the `1.0.0` tag.

## Layout

```
build/                                  # builds this repo with the library itself (ProjectReference)
pack-local.ps1                          # packs dev version 0.1.0 -> W:/Data/nuget/temp (NuGet source "Temp"), evicts cache
Src/
├── Alphacloud.CakeBuild.slnx
├── Directory.Build.props
├── Lib/Alphacloud.CakeBuild/
│   ├── *Settings.cs, BuildVersion.cs, RepositoryInfo.cs, Paths.cs     # settings/model
│   ├── ConventionValidator.cs, CoverageSettingsXml.cs, TestCommand.cs # pure logic, unit tested
│   ├── BuildContext.cs, BuildLifetime.cs, CakeHostExtensions.cs, Tools.cs
│   └── Tasks/                          # FrostingTask<BuildContext> classes (thin wrappers)
├── Tests/Alphacloud.CakeBuild.Tests/   # xunit.v3 4.x (MTP) + Shouldly
└── Samples/SampleSolution/             # standalone consumer of package 0.1.0, used for end-to-end validation
```

The pipeline: `Default` → `Build` → `RunUnitTests` → `CreateNugetPackages` → `CreateRelease` → `CloseMilestone`.
`Build` depends on `ValidateConventions` → `SetVersion` → `UpdateAppVeyorBuildNumber` → `UpdateReleaseNotesLink` → `Restore`.

## Conventions when editing

- Keep the tasks thin. Put logic in pure classes that don't depend on Cake (`RepositoryInfo`, `TestCommand`,
  `CoverageSettingsXml`, `ConventionValidator`) and unit test it.
- Task names are part of the public contract, because consumers call `--target <name>`. Don't rename tasks without
  a major version bump.
- Use `ShouldRun(BuildContext)` to gate tasks by environment instead of branching inside `Run`.
- Tasks read all state from `BuildContext`. Never use ambient globals.
- Tool pins live in `Tools.cs` and the Cake.Frosting pin lives in the csproj. Record every bump in `CHANGELOG.md`.
- The default consumer folder layout is `Src/{Lib,Tests,Samples,Common}`, configurable through
  `ProjectSettings.Conventions`. Update the README conventions section whenever the layout or settings change.
- Use `TreatWarningsAsErrors`, and give every public API an XML doc comment.
- Commit subjects use the GitHub issue prefix, e.g. `#2 Convert ...`.

## Build & validate

- Unit tests: `dotnet test --solution Src/Alphacloud.CakeBuild.slnx`. MTP mode is set in the root `global.json`.
- Self build: `./build.ps1`, which runs the full pipeline on this repo.
- End to end: `./pack-local.ps1`, then `cd Src/Samples/SampleSolution; ./build.ps1`.
- Builds take more than 30 s (restore + tool install). When the shell has a timeout, start them in the background and
  write the output to a log file.

## Where to look first

- `README.md` is the consumer-facing source of truth: conventions, getting started, configuration, tasks,
  AppVeyor, and migrating from 1.x.
- `CHANGELOG.md` uses the Keep a Changelog format; the current version is 2.0.0.
- `memory-bank/` holds the agent working memory. `plan.md` is the conversion plan (issue #2).

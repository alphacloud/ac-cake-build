# Product Context

## Why this exists
alphacloud maintains several .NET OSS libraries (Autofac.Extras.Quartz, Sharp-Architecture, ...). Each used to carry
its own copy of the build script, and the copies drifted apart. 1.x moved the script into a shared submodule. 2.0
makes it a versioned NuGet package with an enforced repository layout. A fix or tool bump is now made once, and each
repo adopts it by bumping a `PackageReference`.

## Problems it solves
- **Duplicated, drifting build logic** across repos.
- **Release ceremony:** GitVersion versioning, `Common/AssemblyVersion.cs` generation, rewriting
  `PackageReleaseNotes` on tags, pack, GitReleaseManager draft release and milestone close.
- **Multi-TFM coverage:** a single dotnet-coverage session covers every test project and every TFM and produces one
  Cobertura file. This replaces Coverlet's per-project/per-TFM files and SharpArch's path-normalization and
  parallel-upload workarounds.
- **PR builds:** GitVersion fails there, so a version is synthesized.
- **Layout mistakes:** `ValidateConventions` reports missing or misnamed folders up front, including letter-case
  mismatches that would only break on Linux.

## Consumer experience
1. Add `build/Build.csproj` (PackageReference `Alphacloud.CakeBuild`) and `build/Program.cs` (3 lines). Add
   `build.ps1`/`build.sh` (`dotnet run --project build/Build.csproj -- $args`).
2. Add a root `global.json` with `"test": { "runner": "Microsoft.Testing.Platform" }`. Test projects use xunit.v3 4.x
   with `OutputType=Exe`.
3. Run `./build.ps1`: the full pipeline. `--target X`, `--buildConfig Debug` and `--tree` are also available.
4. AppVeyor needs `./build.ps1`, plus `GITHUB_TOKEN`, `COVERALLS_REPO_TOKEN` and `COVERALLS_REPORTER_EXE`
   (the coveralls-windows.exe download).

The README is the consumer-facing source of truth: conventions, getting started, configuration table, task table,
AppVeyor template, and migrating from 1.x.

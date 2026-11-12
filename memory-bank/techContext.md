# Tech Context

## Stack
| Component | Version | Where |
|---|---|---|
| Cake.Frosting | 7.0.0 | library csproj |
| Target frameworks | net10.0;net11.0 | `Src/Directory.Build.props` |
| SDK | 11.0.100 (`global.json`, rollForward latestFeature) | root |
| GitVersion.Tool | 6.8.2 | `Tools.cs` (dotnet:) |
| GitReleaseManager.Tool | 0.20.0 | `Tools.cs` |
| dotnet-coverage | 18.12.0 | `Tools.cs` |
| ReportGenerator | 5.6.0 | `Tools.cs` (nuget:) |
| xunit.v3 / Shouldly | 4.0.3 / 4.3.0 | `Src/Tests/Directory.Build.props` |

## Verified external facts (no need to look them up again)
- `--filter "Category!=X"` is VSTest syntax. xunit.v3 on MTP uses `--filter-not-trait` or `--filter-query`.
  MTP returns exit code 8 when a filter leaves zero tests, so pass `--ignore-exit-code 8`.
- xunit.v3 4.x depends on `xunit.v3.mtp-v2`. With MTP opted in through `global.json`, every test project must be MTP.
- `dotnet-coverage collect` has no include/exclude flags, only the `-s` settings XML (`ModulePaths`, `Attributes`,
  `Sources`, `CompanyNames`, `PublicKeyTokens`). Cobertura output uses absolute file paths.
- The coveralls coverage-reporter flags are `report <file> --format=cobertura --base-path=PATH --no-logo
  [--parallel --job-flag=]`. `done` finalizes a parallel build.
- GitVersion 6 removed `NuGetVersion`, so use `SemVer`. Force `ToolPath` to `dotnet-gitversion(.exe)`.
- The Frosting template (`dotnet new cakefrosting`) creates `build/Build.csproj` with `RunWorkingDirectory`, so
  `UseWorkingDirectory("..")` points at the repo root.
- The `Alphacloud.CakeBuild` ID was free on nuget.org (checked 2026-11-12).

## Dev environment
- Windows. Use PowerShell with **forward-slash** paths (`W:/vk/...`) and `rg` (grep is not installed).
- Machine-level `CAKE_PATHS_TOOLS=C:\Users\vk\AppData\Local\Temp\CakeTools`.
- Local feed `W:\Data\nuget\temp` (`/mnt/w/Data/nuget/temp` in WSL) is the user NuGet source "Temp". The NuGet cache is
  `W:\Data\nuget\packages`. `pack-local.ps1` evicts `alphacloud.cakebuild/0.1.0` from the cache.
- Full builds take 60–70 s, longer than the 30 s shell timeout. Start them with
  `Start-Process powershell ... *> $env:TEMP\x.log` and poll the log.
- Reference consumers:
  - `W:\vk\Projects\SharpArch\Sharp-Architecture` (`Src/`, `.slnx`, AppVeyor with mssql)
  - `W:\vk\Projects\alphacloud\Autofac.Extras.Quartz` (lowercase `src/`, `.sln`, so it needs `SrcDir="src"`)

## Commit conventions
GitHub issue prefix: `#2 <imperative summary ≤100 chars, no trailing punctuation>`, blank line, then a "what" body.
Mention a version only for major dependency bumps.

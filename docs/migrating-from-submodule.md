# Migrating from the ac-cake-build submodule (1.x) to Alphacloud.CakeBuild (2.x)

A runbook for moving a consumer repository off the 1.x `.build/` scripts (`#load "./.build/..."` in `build.cake`) and
onto the `Alphacloud.CakeBuild` NuGet package. It is written for an AI coding agent working in the consumer
repository; a human can follow the same steps.

Work through the **Steps** in order. Each step ends with a **check**: get the check passing before you move on.
The **Reference** section holds the translation tables the steps point to.

## Definition of done

The migration is complete when all of these hold:

- `.build/`, `build.cake` and the Cake bootstrapper scripts are gone.
- `build/Build.csproj` references `Alphacloud.CakeBuild` and passes the repository's settings to it.
- The folder layout follows the conventions, or `ProjectSettings.Conventions` describes the deviations.
- Every test project is xunit.v3 4.x on Microsoft.Testing.Platform (MTP).
- `./build.ps1` runs the full pipeline locally and is **green**: it exits 0, and it produces the same packages and
  the same test count as the baseline.
- CI calls `./build.ps1`.
- You have reported back using the report template at the end of this document.

## Inputs

| Input | Default | Where it comes from |
|---|---|---|
| Package version | Latest stable on nuget.org: `dotnet package search Alphacloud.CakeBuild --exact-match` | Use the version (and the already-registered feed) the user names. For example, a local validation build `0.1.0` from a folder feed. |
| Layout strategy | **Rename to the conventions** (`Src`, `Lib`, `Tests`, `Samples`, `Common`) | If the user wants to keep the existing folder names, configure `ProjectSettings.Conventions` instead. |
| Commit format | The repository's own convention (see its AGENTS.md / CLAUDE.md / CONTRIBUTING) | Ask the user if it is unclear. |

Commands below are PowerShell (`pwsh` runs on Linux and macOS too). Run them from the repository root.

## Blockers: stop and ask the user

Stop and ask, presenting the choices listed, when any of these is true:

1. **A test project uses a framework other than xunit**, such as NUnit or MSTest. The pipeline passes
   xunit-specific MTP options (`--report-xunit-trx`, `--filter-not-trait`). Once `global.json` opts into MTP, every
   test project must be an MTP project. Offer these choices:
   - port the project to xunit.v3
   - take it out of the solution that the build runs
   - stay on 1.x for now
2. **`build.cake` defines custom tasks**, or calls `RunTarget` with anything other than `target`. List the tasks and
   offer to port them as `FrostingTask<BuildContext>` classes in `build/` (see [Custom tasks](#custom-tasks)).
3. **A coverage filter targets types or methods instead of whole assemblies**, for example `[MyLib]MyLib.Internal.*`.
   dotnet-coverage settings in 2.x filter by module, attribute and source path only. Offer to rewrite it as a
   source-path exclude or to drop it.
4. **The package version from [Inputs](#inputs) does not resolve** (`dotnet package search` finds nothing).
5. **The repository path contains a space**, for example `C:\My Projects\repo`, or a folder or project inside the
   repository has a space in its name. Paths with spaces are not supported. Offer these choices:
   - clone or move the repository to a path without spaces
   - rename the folders that contain spaces

## Steps

### 1. Inventory

Collect these facts. You will need them in later steps and in the report.

```powershell
git status --short                          # must be clean; otherwise ask the user how to proceed
git ls-files -s .build                      # 160000 = real submodule; 100644 = vendored copy of the scripts
Get-Content .gitmodules -ErrorAction Ignore
Get-Content build.cake
Get-ChildItem -Directory -Name              # top-level layout; note the casing of src/Src
Get-ChildItem -Recurse -Depth 1 -Include *.sln,*.slnx -Name
git ls-files '*.csproj' | Select-String -Pattern 'Tests'
Get-Content appveyor.yml, global.json, .config/dotnet-tools.json -ErrorAction Ignore
(Get-Location).Path -match ' '                                  # must be False (blocker 5)
git ls-files | Where-Object { $_ -match ' ' -and $_ -match '\.(csproj|slnx?|props)$' }   # must print nothing
```

From `build.cake`, capture:
- the `ProjectSettings(owner, repo, solution)` arguments
- every `CodeCoverage` customization
- any additional tasks or `Setup`/`Teardown` logic

Check the [Blockers](#blockers-stop-and-ask-the-user) now.

**Check:** you can fill in every row of the report's *Inventory* section.

### 2. Baseline

Run the existing build once and record:
- the exit code
- the test count per test project
- the list of `.nupkg` files in `artifacts/packages`

```powershell
dotnet cake          # or ./build.ps1 / ./build.sh, whichever the repository uses today
```

If the baseline already fails, record why and continue. The migration aims at the same outcome as the baseline,
or a better one.

**Check:** the baseline numbers are written down.

### 3. Remove the 1.x build

Run the commands that match what step 1 found:

```powershell
# .build tracked as a real submodule (mode 160000)
git submodule deinit -f .build
git rm -f .build
Remove-Item -Recurse -Force .git/modules/.build -ErrorAction Ignore

# .build vendored as plain files (mode 100644)
git rm -r -f .build
```

Then remove the old entry point and its tooling:

```powershell
git rm -f build.cake
git rm -f --ignore-unmatch build.sh build.ps1   # old Cake bootstrappers; new ones come in step 5
```

- In `.config/dotnet-tools.json`, remove the `cake.tool` and `coveralls.net` entries. Delete the file if no tools
  remain.
- In `.gitignore`:
  - Remove any rule that ignores `build/`. Visual Studio templates often have one, and it would hide the new build
    project.
  - Make sure `tools/`, `artifacts/` and `<SrcDir>/<CommonDir>/AssemblyVersion.cs` are ignored.

**Check:**
- `git ls-files .build build.cake` prints nothing.
- `git check-ignore build/Build.csproj` prints nothing.

### 4. Layout

Compare the inventory with the [conventions](../README.md#repository-conventions). The solution must sit directly
in the source folder, and its name must match the `solutionName` argument.

- **Rename** (default strategy). Use `git mv`.
  - A rename that only changes letter case needs two hops on case-insensitive file systems (Windows, macOS), for
    example `src` → `src_tmp` → `Src`.
  - Then update every path that pointed at the old folders: the solution file, `ProjectReference`s,
    `Directory.Build.props` imports, `appveyor.yml` and CI scripts, and README badges and links.
- **Keep** (only when the user asked for it). Record each deviation as a `ProjectSettings.Conventions` value in
  step 5.

The pipeline writes to two files, and both need to be set up:

- **`<SrcDir>/Directory.Build.props`** needs a `<PropertyGroup Label="Package">` that contains a
  `<PackageReleaseNotes>` element. Tagged builds rewrite that element to point at the GitHub release.
- **`<SrcDir>/<CommonDir>/AssemblyVersion.cs`** is regenerated by `SetVersion`. Projects must compile it in, and
  must stop generating the same attributes themselves:

  ```xml
  <PropertyGroup>
    <GenerateAssemblyVersionAttribute>false</GenerateAssemblyVersionAttribute>
    <GenerateAssemblyFileVersionAttribute>false</GenerateAssemblyFileVersionAttribute>
    <GenerateAssemblyInformationalVersionAttribute>false</GenerateAssemblyInformationalVersionAttribute>
  </PropertyGroup>
  <ItemGroup Condition="Exists('$(MSBuildThisFileDirectory)Common/AssemblyVersion.cs')">
    <Compile Include="$(MSBuildThisFileDirectory)Common/AssemblyVersion.cs" Link="Properties/AssemblyVersion.cs" />
  </ItemGroup>
  ```

  If the repository already links the file in a way that works, leave it. Add this block only when no link exists.

**Check:** `dotnet build <SrcDir>/<Solution>.slnx` (or `.sln`) succeeds.


### 5. Add the Frosting host

**`build/Build.csproj`**. Set `TargetFramework` to the newest SDK the repository's `global.json` allows: `net10.0` or `net11.0`.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <RunWorkingDirectory>$(MSBuildProjectDirectory)</RunWorkingDirectory>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Alphacloud.CakeBuild" Version="2.0.0" />
  </ItemGroup>
</Project>
```

`RunWorkingDirectory` is required. `UseAlphacloudCakeBuild` finds the repository root by going one level up (`..`) from `build/`.

**`build/Program.cs`**. Translate the settings from `build.cake` using the [settings table](#settings-translation).

```csharp
using Alphacloud.CakeBuild;
using Cake.Frosting;

var settings = new ProjectSettings("owner", "Repo-Name", "SolutionName");
// settings.Conventions.SrcDir = "src";   // only when you kept a non-conventional layout

return new CakeHost()
    .UseAlphacloudCakeBuild(settings)
    .Run(args);
```

**`build.ps1`**

```powershell
dotnet run --project "$PSScriptRoot/build/Build.csproj" -- $args
exit $LASTEXITCODE;
```

**`build.sh`**. After creating it, mark it executable with `git update-index --chmod=+x build.sh`.

```bash
#!/usr/bin/env bash
dotnet run --project "$(dirname "$0")/build/Build.csproj" -- "$@"
```

**Root `global.json`**. If the file already exists, add the `test` section to it.

```json
{
  "sdk": { "version": "10.0.100", "rollForward": "latestFeature" },
  "test": { "runner": "Microsoft.Testing.Platform" }
}
```

Leave `GitVersion.yml` and `GitReleaseManager.yaml` unchanged. If `GitVersion.yml` is missing, ask the user. It defines the versioning scheme, so there is no safe default.

**Check:** `./build.ps1 --tree` prints the task graph, and `ValidateConventions` appears under `Build`.

### 6. Port the test projects to xunit.v3 on MTP

Make these changes in each test project:

| Change | From | To |
|---|---|---|
| Framework package | `xunit` 2.x | `xunit.v3` 4.x (latest stable) |
| Output type | library | `<OutputType>Exe</OutputType>` (applies to .NET Framework targets too) |
| Remove | `coverlet.collector`, `coverlet.msbuild`, `Appveyor.TestLogger`, `xunit.abstractions` | |
| Keep only for VSTest-based IDEs | `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio` (3.x) | |

Common compile errors after the switch:

| Error | Fix |
|---|---|
| `Xunit.Abstractions` not found | Remove the `using`. `ITestOutputHelper` now lives in the `Xunit` namespace. |
| `IAsyncLifetime` signature mismatch | Change `InitializeAsync` and `DisposeAsync` to return `ValueTask`. `DisposeAsync` now comes from `IAsyncDisposable`. |
| Duplicate `Main` / entry point | Delete the hand-written `Program.cs` from the test project. |
| `SkippableFact` / `Xunit.SkippableFact` | Use `Assert.Skip(...)` or `Assert.SkipWhen(...)` instead, and remove the package. |

For any other errors, see the [xUnit v2 → v3 migration guide](https://xunit.net/docs/getting-started/v3/migration).

**Check:** run `dotnet test --solution <SrcDir>/<Solution>.slnx --filter-not-trait Category=ManualTests` (MTP mode, enabled by `global.json`). It must pass on every target framework, and the test count must match the baseline.


### 7. Run the pipeline

```powershell
./build.ps1
```

The first run installs the pinned tools and can take several minutes. If your shell has a command timeout, run the build in the background and send the output to a log file.

**Check:** the build is **green**, meaning all of the following hold:

- The exit code is 0.
- These tasks report `Skipped`: `UpdateAppVeyorBuildNumber`, `UpdateReleaseNotesLink`, `UploadCoverage`, `CreateRelease`, `CloseMilestone`.
- These files exist:
  - `artifacts/coverage/coverage.cobertura.xml`
  - `artifacts/CodeCoverageReport/index.html`
  - one `.nupkg` per packable project in `artifacts/packages`, with the same names as the baseline
- `artifacts/CodeCoverageReport/Summary.txt` lists every library assembly and no test assemblies. If it doesn't, adjust `settings.CodeCoverage` using the [coverage table](#coverage-filter-translation).

If the build fails, look up the symptom in the [failure table](#failure-symptoms).

### 8. Update CI

Make these changes in `appveyor.yml`:

- Replace `dotnet cake`, `dotnet tool install Cake.Tool ...` and `dotnet tool restore` with `./build.ps1`.
- Replace `dotnet tool install coveralls.net` with a download of the Coveralls reporter, and set `COVERALLS_REPORTER_EXE` (see the [README AppVeyor section](../README.md#appveyor)).
- Keep the existing `secure:` values for `GITHUB_TOKEN` and `COVERALLS_REPO_TOKEN`.
- Make sure the image has every SDK and runtime that the test projects target. If one is missing, add an install step.
- Remove `--test-adapter-path` and `--logger AppVeyor` from any custom `dotnet test` steps; MTP projects reject those switches.
- If a cache key references `build.cake`, change it to `build\*.csproj`.

If a Linux job runs `dotnet test` directly, its filter syntax changes:

- `--filter "Category!=X"` becomes `--filter-not-trait Category=X`.
- Each condition in an `&` chain becomes its own `--filter-not-trait` switch.

**Check:** `git grep -n -E "dotnet cake|build\.cake|csmacnz|--logger AppVeyor"` prints nothing.

### 9. Document and commit

- **README:** change the build instructions to `./build.ps1`. Remove references to the submodule and to `git clone --recurse-submodules`.
- **CHANGELOG:** if the repository keeps one, add a "Build" entry under Unreleased.
- **Commit:** make one commit, in the repository's commit format, that summarizes the migration.

**Check:** `git status --short` is clean, and the commit contains the changes from steps 3–8.

## Report

When you finish, or stop at a blocker, report back in this shape:

```
Inventory: .build = submodule|vendored; solution = <path>; layout = renamed|kept (<Conventions overrides>);
           test frameworks = <list>; custom build.cake logic = none|<list>
Baseline:  exit <code>; tests <n> per project; packages <list>
Result:    exit <code>; tests <n> per project; packages <list>; coverage <line %>
Changes:   <one line per step 3-8>
Open:      <blockers, CI items only verifiable on AppVeyor, follow-ups>
```

Items that can only be verified on AppVeyor stay under **Open** until a CI build has run them:

- build number update
- TRX upload
- Coveralls upload
- GitHub release and milestone
- NuGet deploy


## Reference

### Settings translation

| 1.x (`build.cake`) | 2.x (`build/Program.cs`) |
|---|---|
| `new ProjectSettings(owner, repo, solution)` | same constructor, same arguments |
| `Argument("target", "Default")` + `RunTarget(target)` | built in: `--target <task>` |
| `Setup<BuildInfo>(... BuildInfo.Get ...; CreateDirectory/CleanDirectory(artifacts))` | built in: `BuildContext` + `BuildLifetime`; delete it |
| `Teardown(...)` with no body | delete it |
| `--buildConfig` argument | same argument, same default `Release` |
| `BuildInfo` in custom tasks | `BuildContext`: `Paths`, `Version`, `Repository`, `Settings`, `IsLocal`, `Config` |
| hardcoded `TargetCommitish = "master"` | `settings.ReleaseTargetBranch` (default `master`) |
| test filter `Category!=ManualTests` | `settings.Tests.ExcludedCategories` (default `["ManualTests"]`) |
| Release-mode test run | `settings.Tests.RunReleaseTests` (default `true`) |
| `src/`, `src/tests`, `src/common` | `settings.Conventions.SrcDir` / `TestsDir` / `CommonDir`; defaults `Src`, `Tests`, `Common` |

### Coverage filter translation

In 1.x, filters are Coverlet `[Assembly]Type` globs. In 2.x they are .NET regexes, matched by dotnet-coverage against
the module's **file path**. That is why module patterns start with `.*[\\/]` and end with `\.dll$`.

| 1.x | 2.x |
|---|---|
| `IncludeFilter = "+[Autofac.Extras.Quartz]*"` | `IncludeModulePaths = [@".*[\\/]Autofac\.Extras\.Quartz\.dll$"]` |
| `IncludeFilter = { "[SharpArch.*]*" }` | default already matches `<SolutionName>*.dll`; delete the line when the solution name is the prefix |
| `ExcludeFilter.AddRange("[Suteki*]*", "[SharpArch.XunitTests*]*")` | `ExcludeModulePaths.AddRange([@".*[\\/]Suteki[^\\/]*\.dll$", @".*[\\/]SharpArch\.XunitTests[^\\/]*\.dll$"])` |
| default excludes `[Tests*]*`, `[*]Microsoft.CodeAnalysis*` | default excludes `*Tests.dll`, `*Tests.*.dll`; framework assemblies are never instrumented |
| `ExcludeByAttribute = "ExcludeFromCodeCoverage"` | default `ExcludeAttributes` already covers it (full name regex) |
| `ExcludeByFile = "**/*Designer.cs"` | default `ExcludeSources` already covers it |
| type or method filters `[Asm]Ns.Type*` | blocker 3: no direct equivalent |

`IncludeModulePaths` replaces the default when you assign a new list. To extend the default, use `.Add`.

### Custom tasks

A 1.x `Task("X").IsDependentOn("Build").Does<BuildInfo>(b => ...)` becomes a class in `build/`:

```csharp
using Alphacloud.CakeBuild;
using Alphacloud.CakeBuild.Tasks;
using Cake.Frosting;

[TaskName("X")]
[IsDependentOn(typeof(BuildTask))]
public sealed class XTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsLocal;   // was .WithCriteria(...)
    public override void Run(BuildContext context) { /* body; Cake aliases are extension methods on context */ }
}
```

Frosting discovers tasks in the host assembly automatically, so `XTask` needs no extra registration. It can depend
on any built-in task. Built-in task names are reserved.

### Failure symptoms

| Symptom | Cause | Fix |
|---|---|---|
| `ValidateConventions` fails: `Source folder 'Src' not found` | layout kept or case differs | step 4: rename the folder, or set `Conventions.SrcDir` |
| `ValidateConventions` fails: `... must contain <PropertyGroup Label="Package"> ...` | release-notes element missing | add `<PackageReleaseNotes>` (step 4) |
| `Unable to find package Alphacloud.CakeBuild` | version or feed | check the version from [Inputs](#inputs) and run `dotnet nuget list source` |
| `dotnet test` exits 5: unknown option `--report-xunit-trx` or `--filter-not-trait` | a test project is not xunit.v3 / not on MTP | step 6 for that project |
| `MSB1001: Unknown switch. Switch: --solution` or `Testing with VSTest target is no longer supported by Microsoft.Testing.Platform` | `dotnet test` runs in VSTest mode: `global.json` is missing `"test": { "runner": "Microsoft.Testing.Platform" }`, or the build runs from a folder that doesn't see it | step 5 `global.json` at the repository root |
| `error CS0579: Duplicate 'AssemblyVersion' attribute` | SDK still generates version attributes | step 4 `Generate*Attribute=false` |
| `RunXunitTests` succeeds but coverage report is empty or shows 0 assemblies | include regex matches nothing | print `artifacts/coverage/codecoverage.config` and fix `IncludeModulePaths` |
| test count lower than baseline | `ManualTests` exclusion, or a trait name other than `Category` | compare with baseline; set `Tests.CategoryTrait` / `ExcludedCategories` |
| `RunXunitTests` fails right away: `dotnet test` reports an unknown argument, or a path is cut off at a space | the repository path contains a space | blocker 5: move the repository to a path without spaces |
| `Tool 'dotnet-coverage' was not found` | tool install failed (offline / proxy) | rerun with network access; check `CAKE_PATHS_TOOLS` |
| `CreateRelease` fails locally | `GITHUB_TOKEN` set and running as upstream; normally skipped | expected only on CI; unset the variable locally |


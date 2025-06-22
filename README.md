# ac-cake-build
Cake build script

## Table of Contents

- [ac-cake-build](#ac-cake-build)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Script structure](#script-structure)
    - [Project structure conventions](#project-structure-conventions)
  - [Adding as Submodule](#adding-as-submodule)
  - [Using the Build Scripts](#using-the-build-scripts)
    - [Create Cake script](#create-cake-script)
    - [Install tools](#install-tools)
    - [Project structure and conventions](#project-structure-and-conventions)
  - [Cloning Parent Project with Submodules](#cloning-parent-project-with-submodules)
  - [Updating Build System](#updating-build-system)
  - [Removing Submodule](#removing-submodule)
  - [Versioning](#versioning)
  - [Changelog](#changelog)

## Overview

This repository contains reusable Cake build scripts for .NET projects. 
It provides standardized build tasks and definitions that can be shared across multiple projects as a Git submodule, ensuring consistent build processes and easy maintenance.

## Script structure

**Settings (`definitions.cake`)**

`ProjectSettings` - Main configuration class:
- `RepoOwner` - GitHub repository owner/organization
- `RepoName` - GitHub repository name  
- `SolutionName` - Solution file name
- `CodeCoverage` - Code coverage configuration

`CodeCoverageSettings` - Coverage filters:
- `IncludeFilter` - Assemblies to include (default: `+[SolutionName*]*`)
- `ExcludeFilter` - Assemblies to exclude (default: test assemblies, Microsoft libraries)
- `ExcludeByFile` - Files to exclude (default: Designer files)
- `ExcludeByAttribute` - Attributes to exclude (default: ExcludeFromCodeCoverage)

`BuildInfo` - Runtime build information:
- `Target` - Build target (default: "Default")
- `Config` - Build configuration (default: "Release")
- `Version` - Semantic version information
- `Repository` - Git repository state
- `Paths` - Standard project paths

**Available Tasks (`tasks.cake`)**

Core Tasks:
- `CleanAll` - Clean bin/obj directories and artifacts
- `SetVersion` - Generate AssemblyVersion.cs with version info
- `Restore` - Restore NuGet packages
- `Build` - Build solution (Debug + Release for coverage)

Testing Tasks:
- `RunXunitTests` - Execute unit tests with OpenCover
- `CleanPreviousTestResults` - Clean test artifacts
- `GenerateCoverageReport` - Generate HTML coverage report (local only)
- `UploadCoverage` - Upload coverage to Coveralls (CI only)
- `RunUnitTests` - Combined test execution pipeline

Release Tasks:
- `CreateNugetPackages` - Pack NuGet packages
- `UpdateReleaseNotesLink` - Update package release notes URL
- `CreateRelease` - Create GitHub release (release branches only)
- `CloseMilestone` - Close GitHub milestone (tagged builds only)

Default Pipeline:
`Default` → `Build` → `RunUnitTests` → `CreateNugetPackages` → `CreateRelease` → `CloseMilestone`

### Project structure conventions

**Required Directory Structure:**
```
project-root/
├── .build/                    # Build system submodule
├── src/
│   ├── common/
│   │   └── AssemblyVersion.cs # Generated version file
│   ├── lib/                   # Library projects
│   ├── tests/                 # Test projects
│   ├── samples/               # Sample projects (optional)
│   ├── [SolutionName].sln     # Solution file
│   └── Directory.Build.props  # Common MSBuild properties
├── artifacts/                 # Build output (auto-created)
├── build.cake                 # Main build script
├── GitVersion.yml         # Version calculation
└── GitReleaseManager.yaml # GitHub release configuration  

```

**Required Configuration Files:**

* `build.cake` - Main build script
* `GitVersion.yml` - Version calculation
* `GitReleaseManager.yaml` - GitHub release configuration  
* `src/Directory.Build.props` - MSBuild properties

`GitVersion.yml` example:
```yaml
branches: {}
ignore:
  sha: []
tag-prefix: 'releases?/'
```

`GitReleaseManager.yaml` example:
```yaml
create:
  include-footer: true
  footer-heading: Where to get it
  footer-content: You can download this release from [nuget.org](https://www.nuget.org/packages/package-name/{milestone})
  footer-includes-milestone: true
```

**Required Environment Variables:**

**CI/CD (AppVeyor):**
- `COVERALLS_REPO_TOKEN` - Coveralls.io API token
- `GITHUB_TOKEN` - GitHub API token for releases

**NuGet Publishing:**
- NuGet API key (encrypted in appveyor.yml)
- Deploy triggers for develop/release branches and tags

**Project Naming Conventions:**
- Solution name matches repository name
- Test projects contain "Tests" in name
- Sample projects under `Samples/` folder
- Library projects under `Lib/` folder

## Adding as Submodule

Always pin specific version when adding submodule. Assume `1.0.0` as a version to use.
To add this repository as a submodule to your parent project.

```bash
git submodule add https://github.com/your-org/ac-cake-build.git .build
cd .build
git checkout 1.0.0
cd ..
git add .
git commit -m "Add build system"
```

## Using the Build Scripts

### Create Cake script

Create a `build.cake` file in your project root:

```csharp
#load "./.build/src/definitions.cake"

var target = Argument("target", "Default");

ProjectSettings settings = new ProjectSettings("OrganizationName", "Repository-Name", "SolutionName")
{
    CodeCoverage =
    {
        IncludeFilter = "+[SolutionName.*]*"
    }
};

Setup<BuildInfo>(context =>
{
    var buildInfo = BuildInfo.Get(context, settings);
    CreateDirectory(buildInfo.Paths.ArtifactsDir);
    CleanDirectory(buildInfo.Paths.ArtifactsDir);
    return buildInfo;
});

#load "./.build/src/tasks.cake"

RunTarget(target);
```

Update following parameters:
* `OrganizationName` - GitHub organization name
* `Repository-Name` - GitHub repository name
* `SolutionName` - Solution name


### Install tools

Install following tools as part of build initization step
* [Cake](https://cakebuild.net/)
* [Coveralls](https://coveralls.io/)

```bash
dotnet tool install Cake.Tool --version 1.3.0 --global
dotnet tool install coveralls.net --version 4.0.1 --global
```

### Project structure and conventions



## Cloning Parent Project with Submodules

To clone a parent project that includes this submodule:

```bash
# Clone with submodules
git clone --recurse-submodules <repo-url>

# Or if already cloned, initialize submodules
git submodule update --init --recursive
```

## Updating Build System

To update the build system to a new tag version:

```bash
cd .build
git fetch --tags
git checkout 2.0.0  # Replace with new tag version
cd ..
git add .build
git commit -m "Update build system to version 2.0.0"
```

## Removing Submodule

To remove the build system submodule:

```bash
git rm -f .build
rm -f .git/modules/.build
git add .gitmodules
git commit -m "Remove build system submodule"
```

## Versioning

This project follows [Semantic Versioning 2.0.0](https://semver.org/).

Given a version number MAJOR.MINOR.PATCH:
- **MAJOR**: Incompatible API changes
- **MINOR**: Backwards-compatible functionality additions
- **PATCH**: Backwards-compatible bug fixes

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes.

We follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format, which provides a curated, chronologically ordered list of notable changes for each version.


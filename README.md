# ac-cake-build
Cake build script

## Overview

This repository contains reusable Cake build scripts for .NET projects. It provides standardized build tasks and definitions that can be shared across multiple projects as a Git submodule, ensuring consistent build processes and easy maintenance.

## Adding as Submodule

Always pin specific version when adding submodule. Assume `1.0.01` as a version to use.
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


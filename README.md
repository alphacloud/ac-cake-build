# ac-cake-build
Cake build script

## Adding as Submodule

To add this repository as a submodule to your parent project:

```bash
git submodule add -b 1.2.3 https://github.com/your-org/ac-cake-build.git .build
```

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

## Versioning

This project follows [Semantic Versioning 2.0.0](https://semver.org/).

Given a version number MAJOR.MINOR.PATCH:
- **MAJOR**: Incompatible API changes
- **MINOR**: Backwards-compatible functionality additions
- **PATCH**: Backwards-compatible bug fixes

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes.

We follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format, which provides a curated, chronologically ordered list of notable changes for each version.


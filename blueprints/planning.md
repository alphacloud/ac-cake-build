# Project Planning Log — ac-cake-build

Chronological log of notable changes to this repo. Newest entries at the top.

---

## 2026-11-12 — Convert to Cake Frosting NuGet package `Alphacloud.CakeBuild` (#2)

**Change:**
- Replaced `src/definitions.cake` and `src/tasks.cake` with a Cake Frosting class library packed as the NuGet package
  `Alphacloud.CakeBuild` (net10.0;net11.0, Cake.Frosting 7.0.0).
- Added unit tests, a self-build host (`build/`), the `pack-local.ps1` dev feed script, a standalone sample consumer
  (`Src/Samples/SampleSolution`), and this repo's own AppVeyor/GitVersion/GitReleaseManager configs.

**Why:** Consumers need one versioned package to build and release multi-TFM libraries locally and on
AppVeyor, against a standard folder layout. That replaces a git submodule of loose scripts.

**Decisions (agreed with the user):**
- Folder names are configurable, PascalCase by default (`Src`, `Lib`, `Tests`, `Samples`, `Common`).
  `ValidateConventions` enforces them, including letter case.
- Tests and coverage use xunit.v3 v4 on MTP with dotnet-coverage, which produces one merged Cobertura report.
  Coverlet and OpenCover are gone.
- `RunReleaseTests` defaults to true. The `.cake` scripts are deleted, and the unreleased Coverlet changelog entries
  are folded into 2.0.0.
- Commit subjects start with the GitHub issue number (`#2`).

**Verified:**
- Builds with 0 warnings, and 80/80 unit tests pass on net10.0 and net11.0.
- The sample consumer ran the full `Default` target against package 0.1.0 from the local feed:
  - coverage merged from net8.0 and net10.0
  - ManualTests excluded
  - Release tests ran
  - the `.nupkg` was produced
  - CI-only tasks were skipped
- Negative cases:
  - a broken folder layout fails `ValidateConventions` with a clear message
  - a failing test still produces the coverage report, then fails `RunUnitTests`
- This repo builds itself with `./build.ps1`.

**Not verified locally:** AppVeyor-only paths: build number update, TRX upload, Coveralls upload with
`--base-path`, GitReleaseManager create/close.

---

## 2026-07-18 — Migrate coverage collection from OpenCover to Coverlet

**Change:** Ported the coverage/build-tooling changes that had been applied locally (uncommitted) in
`Sharp-Architecture/.build/` — a consumer checkout of this repo — back into the generic template at
`src/definitions.cake` and `src/tasks.cake`. Also updated `README.md`'s coverage-filter docs and example
`build.cake` snippet, and added a `CHANGELOG.md [Unreleased]` entry.

**Why:** SharpArch's local checkout had migrated `RunXunitTests` from OpenCover to Coverlet for coverage
collection, added a PR-build GitVersion workaround, bumped several Cake addin/tool versions, and fixed a bug
where `ProjectSettings`'s default include filter used the literal string `"solutionName"` instead of
interpolating the constructor parameter. None of it was NUnit-specific — `RunXunitTests` only calls
`dotnet test`, which is test-framework-agnostic — so nothing needed to be excluded on that front.

**Decisions made (asked the user, since the SharpArch checkout mixed real changes with apparent local scratch
state):**
- Kept the `src/` subfolder layout (did not flatten `definitions.cake`/`tasks.cake` to repo root, even though
  SharpArch's checkout had them at `.build/` root — that flattening wasn't treated as an intentional change).
- Left `README.md` and `LICENSE` otherwise intact — SharpArch's checkout had reduced `README.md` to a one-line
  stub and deleted `LICENSE`, which looked like side effects of local experimentation, not a deliberate change
  to port.
- Pinned `GitReleaseManager.Tool` to `0.20.0` explicitly (SharpArch's checkout had left it unpinned) to avoid
  an unreproducible restore if a new major version ships.

**State at time of writing:** Still at version `1.0.0` released; `[Unreleased]` now has real entries pending
the next version bump.

---

## 2026-07-18 — Initialize CLAUDE.md

**Change:** Added `CLAUDE.md` at the repo root documenting the project for Claude Code sessions, and this
`blueprints/planning.md` log.

**Why:** Repo had no `CLAUDE.md` or planning log. `CLAUDE.md` covers: what the repo is (a shared Cake
build-script library, consumed as a Git submodule — not an application), the `src/definitions.cake` /
`src/tasks.cake` layout and pipeline order, editing conventions, and the `Stack: dotnet` project setting.

**State at time of writing:**
- Version `1.0.0` (per `CHANGELOG.md`), single `[Unreleased]` section with no pending entries.
- `src/` contains only `definitions.cake` (settings/model classes) and `tasks.cake` (task definitions).
- No build/test/lint of its own — this repo has no `.sln`/`.csproj`; verification happens by exercising the
  scripts from a consumer repo's `build.cake`.
- No CI config committed in this repo (AppVeyor is assumed/documented as the consumer-side CI system).

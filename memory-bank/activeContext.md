# Active Context

_Last updated: 2026-11-12. Issue #2 is implemented, reviewed (`/code-review low`) and validated locally. It is
committed on `feature/cake-frosting`, which is pushed to origin and tracks `origin/feature/cake-frosting`._

## Current state
- The Frosting NuGet conversion is implemented according to `/plan.md`. The `.cake` scripts are deleted.
  CHANGELOG has 2.0.0; README, AGENTS.md and blueprints are rewritten.
- Package 0.1.0 is in `W:\Data\nuget\temp`, and `Src/Samples/SampleSolution` consumes it.

## Approved decisions (2026-11-12)
- Distribution: NuGet package. Folder conventions are configurable, PascalCase by default (`Src`, `Lib`, `Tests`,
  `Samples`, `Common`).
- TFMs: net10.0;net11.0. `RunReleaseTests` defaults to true. The `.cake` scripts are deleted, and 1.x stays on its tag.
- The never-tagged Coverlet `[Unreleased]` entries are folded into 2.0.0.
- Commit prefix: `#2`.

## Code review (2026-11-12, `/code-review low`, range 62e312e..HEAD), outcome per the user's decisions
- Spaces in paths: documented as a requirement, not fixed in code.
- `.exe` handling: centralized in `ToolResolver`, with tests.
- Unused API: kept and documented.
- All behaviour changes outside the original plan: accepted, and recorded in `plan.md` ("Behaviour changes accepted at
  code review") and the CHANGELOG.
- TRX upload: now the separate `UploadTestResults` task.
- Release-branch matching: fixed (`StartsWith`, not `Contains`). Tests cover nested names such as
  `feature/release/x` and `bugfix/hotfixes/y`.
- Still not acted on, by choice:
  - `--target CreateNugetPackages` doesn't depend on `Build` (same as 1.x).
  - The Coveralls reporter is downloaded from `releases/latest` (unpinned).
  - `COVERALLS_REPORTER_EXE` is read in a task, not in `BuildContext`.

## Next steps / open items
1. **Not validated: AppVeyor-only paths.** These are: the build-number update, TRX upload, Coveralls upload with
   `--base-path`, GitReleaseManager create/close, and PR versioning. Validate them on the first real AppVeyor build.
2. `appveyor.yml` contains `REPLACE_WITH_ENCRYPTED_*` placeholders. Owner action: encrypt the GitHub, Coveralls and
   NuGet tokens.
3. Migrate the real consumers (SharpArch, Autofac.Extras.Quartz) by following the runbook
   **`docs/migrating-from-submodule.md`**, which an agent can execute on its own. Hand-off prompt: README
   "Migrating from 1.x". Known findings:
   - **Quartz:**
     - `.build` is a vendored copy (mode 100644), not a real submodule.
     - Its `.gitignore` line 14 (`build/`) hides `build/Build.csproj`.
     - It uses lowercase `src`.
     - Tests are xunit 2.9.3 with `Appveyor.TestLogger`.
     - `.config/dotnet-tools.json` contains cake.tool and coveralls.net.
   - **SharpArch:**
     - `.build` is also vendored.
     - Tests are xunit.v3 3.2.2 plus coverlet.msbuild.
     - It has a legacy mono `build.sh` bootstrapper.
     - Its Linux AppVeyor job runs `dotnet test --filter` directly.
4. Release: merge to `develop`, then `release/2.0.0`, then tag `2.0.0` on master. The milestone `2.0.0` must exist.

## Git state
- Branch commits on top of `62e312e`:
  - `721be8c` Frosting conversion
  - `41bdf38` migration runbook
  - `769cfeb` code-review fixes
  - then the release-branch fix
- History was rewritten once to remove a third-party reference before the branch was first pushed.
- **Rule:** committed files must not name the user's employer, its paths or its internal projects. Git-ignored files
  are fine.
  - Before each commit, scan every file in `git ls-files` plus untracked files for these terms.
  - Don't write the terms here, because this file is committed. Ask the user if unsure.
  - The only reference paths allowed are the alphacloud and SharpArch ones in `techContext.md`.

## Preferences and insights
- The user wants questions asked when something is unclear, and prefers decisions offered as options.
- Code review: findings get triaged by the user one by one, and the user may choose "document as a requirement"
  instead of a code fix (as with spaces in paths). Record each outcome in `plan.md` and the CHANGELOG.
- Use the sample under `Src/Samples/` for development/validation, packed as 0.1.0 into the local feed.
- `blueprints/planning.md` is the chronological change log (newest first).

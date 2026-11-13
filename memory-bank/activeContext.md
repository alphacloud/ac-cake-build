# Active Context

_Last updated: 2026-11-12. Issue #2 implementation complete and validated locally; committed on `feature/cake-frosting`._

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

## Preferences and insights
- The user wants questions asked when something is unclear, and prefers decisions offered as options.
- Use the sample under `Src/Samples/` for development/validation, packed as 0.1.0 into the local feed.
- `blueprints/planning.md` is the chronological change log (newest first).

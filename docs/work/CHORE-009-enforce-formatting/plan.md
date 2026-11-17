# Plan: CHORE-009 Enforce formatting

| | |
|---|---|
| **ID** | CHORE-009 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-17 |
| **Size** | M — 17 files, but 11 of them are tool output (`dotnet format`) or BOM removal; about 5 hand-edited files, ~70 lines |
| **Security-sensitive** | yes — touches CI (`deploy.yml`); one step in the secret-free `build_and_test` job, no new action or dependency |
| **UI-facing** | no — config, whitespace, CI and docs only |
| **Opened** | 2026-11-17 |
| **Plan round** | 1 |

## Summary
Complete the root `.editorconfig`, let `dotnet format` fix the existing drift, add a `dotnet format src/BlazorInvaders.sln --verify-no-changes` step to the `build_and_test` job, and correct A6/A7 and the coding guidelines. The solution contains all four projects, so a single command covers every project.

Choices:
- `charset = utf-8` (CG73, no BOM). Once a charset is set, `dotnet format` also checks it and reports 7 `src/core/GameObjects/*.cs` files that have a BOM. The fix removes those BOMs. The same item removes the BOMs from `MainLayout.razor`, `_Imports.razor` and `app.css`, which the tool does not check, so that later edits do not re-encode them as a side effect. `src/BlazorInvaders.sln` keeps its BOM (`[*.sln] charset = utf-8-bom`, which is how Visual Studio writes it). CG73 is updated to say so.
- No `end_of_line` setting: the repository stores LF and Windows checkouts use CRLF (`core.autocrlf`). With no setting, the formatter uses the OS newline, which matches both.
- All naming and style rules stay at severity `suggestion`, so CI fails on whitespace and charset only. The new `Async` rule flags each Function's `Run` (a CG2 exception) only as an IDE hint.

WP1 (single package): everything. Security: no secrets, no new dependency, `dotnet format` ships with the SDK.

## Context found
- `.editorconfig`: at the root since CHORE-006 (commit 4828a1a moved it from `src/`). It has no `root = true`, charset or indent settings. It defines the `end_in_async` style, but no naming rule uses it. Its section is `[*.{cs}]`.
- `dotnet format whitespace src/BlazorInvaders.sln --verify-no-changes` (SDK 10.0.404, the pin is `global.json` 10.0.200 rollForward) reports 32 WHITESPACE errors, all in `src/core`: `Enumerations.cs` (18), `SceneBuilder.cs` (5), `GameEngine.cs` (4), `Alien.cs` (2), `Bomb.cs` (2), `Sprite.cs` (1). Tried on a scratch copy with the proposed config: it also reports 7 CHARSET errors (the BOM files). After `dotnet format src/BlazorInvaders.sln`, the full `--verify-no-changes` exits clean. The diff covers 11 `src/core` files and, ignoring whitespace, changes only the BOM lines. No style or analyzer fixes appear at the default `warn` severity.
- Tried on the scratch copy: `dotnet format style src/api/BlazorInvaders.Api.csproj --severity info --verify-no-changes --diagnostics IDE1006` reports "Missing suffix: 'Async'" for `SaveHighScore.cs:49` (`TextResponse`) and for the three `Run` methods. This proves the rule is live.
- Indentation in the repository: C#, Razor, JS, CSS and most JSON use 4 spaces. `*.csproj` and the workflow YAML use 2. The `.sln` uses tabs.
- `.github/workflows/deploy.yml` `build_and_test`: checkout, Setup .NET (10.0.x), Build, Test. It holds no secrets, and `permissions: contents: read` (CHORE-007).
- Architecture: A6 (linting and formatting, wrong today: it says `src/.editorconfig` and claims the `Async` suffix is set), A7 (CI), and the A2-A7 Decisions line (CHORE-003 D2 recorded "no `dotnet format` in CI"). This item changes A6 and A7 at the user's request (CHORE-004 D7 follow-ups).
- Coding guidelines: CG72 (4 spaces, Allman), CG73 (UTF-8 without BOM, no re-encoding), CG2 (`Async` suffix and its exceptions), CG1 (cites `src/.editorconfig`), CG63 (README run and test steps), CG17 (no reformatting-only changes in general; this item is the explicit exception the request asks for). The Tools row and the CI sentence say nothing is checked. The Open question on A6 is resolved by this item.
- README.md "Running tests" (line 87) gets the format command (CG63).

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | editorconfig, format fixes, CI step, docs | M | `.editorconfig`; `src/core/GameEngine.cs`, `src/core/Rendering/SceneBuilder.cs`, `src/core/GameObjects/{Alien,Bomb,Enumerations,GameTime,MotherShip,Player,Shot,Sprite}.cs` (formatter output only); `src/Shared/MainLayout.razor`, `src/_Imports.razor`, `src/wwwroot/css/app.css` (BOM only); `.github/workflows/deploy.yml`; `docs/architecture.md`; `docs/coding-guidelines.md`; `README.md` | — |

## Steps
### WP1
1. **Complete `.editorconfig`** (AC1). At the top, add `root = true`. Add a `[*]` section with `charset = utf-8`, `indent_style = space` and `indent_size = 4`. Add `[*.{csproj,yml,yaml}]` with `indent_size = 2`, and `[*.sln]` with `charset = utf-8-bom` and `indent_style = tab`. Keep the existing C# section and its rules. Add the naming rule `async_methods_end_in_async`: symbols `async_methods` (`applicable_kinds = method`, `applicable_accessibilities = *`, `required_modifiers = async`), style `end_in_async` (add `capitalization = pascal_case`), severity `suggestion`. Add a one-line comment above it naming the CG2 exceptions (`Run`, `[JSInvokable]`), which therefore stay at `suggestion`. No `end_of_line`, `insert_final_newline` or `trim_trailing_whitespace`: they are not asked for, and they would widen the diff.
2. **Apply the formatter** (AC2). Run `dotnet format src/BlazorInvaders.sln`. `git diff --stat` must list only the 11 `src/core` files above. `git diff -w --ignore-cr-at-eol` must show only the removed BOMs. Never hand-edit those files.
3. **Remove the remaining BOMs.** Strip the 3 leading bytes `EF BB BF` from `src/Shared/MainLayout.razor`, `src/_Imports.razor` and `src/wwwroot/css/app.css`, for example with `sed -i '1s/^\xEF\xBB\xBF//'` in Git Bash. Change nothing else.
4. **CI step** (AC3). In `deploy.yml` `build_and_test`, add a step `Check formatting` between Setup .NET and Build: `run: dotnet format src/BlazorInvaders.sln --verify-no-changes`. The command exits non-zero on drift, which fails the job, and `build_and_deploy` needs this job. Leave `build_and_deploy` alone.
5. **Architecture** (AC4). Update `docs/architecture.md`:
   - A6: root `.editorconfig` (UTF-8, 4-space indents, `_camelCase` private fields, expression-bodied members, `Async` suffix, naming and style at severity suggestion). CI runs `dotnet format --verify-no-changes` on the solution, which fails on whitespace and charset drift. Nullable and implicit usings stay as they are, with no analyzers and no JS linter.
   - A7: the CI sentence adds "check formatting".
   - Add a Decisions line: 2026-11-17 A6, A7: format check added to CI, replacing the "no `dotnet format` in CI" of CHORE-003 D2; requested by the user (CHORE-004 D7, CHORE-009).
   - Update the "Last updated" line.
6. **Coding guidelines** (AC4). Update `docs/coding-guidelines.md`:
   - The Tools row: `dotnet format` (whitespace, charset; IDE style and naming at suggestion), config `.editorconfig` (root, covers `src/` and `tests/`), check command `dotnet format src/BlazorInvaders.sln --verify-no-changes`.
   - The CI sentence: CI also fails on formatting drift.
   - The CG1 source: `src/.editorconfig` becomes `.editorconfig`.
   - CG72 and CG73 add `.editorconfig` as a source.
   - CG73: the C# encoding is checked by `dotnet format`. CHORE-009 removed the BOMs once. The `.sln` keeps its BOM.
   - Remove the A6 Open question.
   - Add a Revisions line and update the "Last updated" line.
7. **README** (CG63). Under "Running tests", add `dotnet format src/BlazorInvaders.sln --verify-no-changes` (the CI check) and `dotnet format src/BlazorInvaders.sln` (to fix), with one line each.

## Tests
No unit tests: nothing changes behaviour. The checks are commands the Tester runs.

| AC | Test | Type | WP |
|----|------|------|----|
| AC1 | `.editorconfig` has `root = true`, `charset`, `indent_style`/`indent_size` and the `async_methods_end_in_async` rule. Run `dotnet format style src/api/BlazorInvaders.Api.csproj --severity info --verify-no-changes --diagnostics IDE1006`: it reports "Missing suffix: 'Async'" at `SaveHighScore.cs:49`. Before the change, it reports no suffix finding | manual command | WP1 |
| AC2 | `dotnet format src/BlazorInvaders.sln --verify-no-changes` exits 0 (before: 32 WHITESPACE). `git diff -w --ignore-cr-at-eol base_commit -- src tests` shows only BOM lines. `head -c3` of the 10 files is not `ef bb bf` | manual command | WP1 |
| AC3 | `deploy.yml` has the step in `build_and_test`, before Build. Negative check: add a trailing double space inside a method in `src/core/GameEngine.cs`. The same command then exits non-zero and names `GameEngine.cs`. Revert | manual command | WP1 |
| AC4 | A6, A7, the Tools row, the CI sentence, CG1, CG73 and the Open questions match steps 5-6. No `src/.editorconfig` remains in `docs/architecture.md` or `docs/coding-guidelines.md` | review | WP1 |
| AC5 | `dotnet build src/BlazorInvaders.csproj --nologo`, `dotnet build src/api/BlazorInvaders.Api.csproj --nologo` (no new warnings, CG70) and `dotnet test tests/BlazorInvaders.Tests --nologo` all pass | existing suite | WP1 |

## Security considerations
- TB7 (GitHub to Azure) and T11 (CI/CD supply chain): the new step runs in `build_and_test`. That job has no secrets and `contents: read`, and it also runs for `pull_request`. It uses `dotnet format` from the SDK that `actions/setup-dotnet` already installs: no new action, no new package, no network use beyond the restore that Build already does. It reads the repository and writes nothing.
- No change to the actors (AC1-AC4), boundaries or threats, so `docs/security-model.md` needs no update.
- No inputs, auth, secrets or data exposure are involved. `.editorconfig` only affects editors and the formatter.

## UX considerations
None: not UI-facing. Stripping the BOM from the Razor and CSS files does not change the rendered output.

## Risks and rollback
- Merge conflicts: branches open in parallel (CHORE-010, -011 and others) may edit the same `src/core` files. The changes are whitespace and line 1 only. Merge this item first (the request says it runs first). A branch that conflicts resolves the conflict by taking its own version and running `dotnet format src/BlazorInvaders.sln`.
- Loading the solution on Linux CI: the `.sln` uses `..\tests\...` paths. MSBuild accepts these on Linux, but the item cannot run CI itself (it only runs on PRs and pushes to `master`). If it fails, switch to one `dotnet format <csproj> --verify-no-changes` per project.
- Line endings: a Windows clone with `core.autocrlf=false` has LF files while the formatter writes CRLF. This could report drift locally only, and CI is not affected. Accepted. `.gitattributes` normalization is out of scope.
- Rollback: revert the commit. The CI step is independent and can be removed alone.

## Out of scope
- Raising any style or naming rule above `suggestion`. Renaming `SaveHighScore.TextResponse` to `TextResponseAsync`. Making the private-field rule stop flagging `static readonly` fields, which CG1 wants PascalCase (`HighScoreValidator.cs:18`). Possible follow-ups.
- A JS or CSS formatter or linter, `end_of_line` settings or `.gitattributes` normalization.
- Formatting Razor files (`dotnet format` does not format them).

## Revisions

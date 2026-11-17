VERDICT: APPROVED

# QA review: CHORE-009 Enforce formatting, round 1

**Reviewed:** diff vs base_commit 7c89747 + untracked files
**Reviewer:** Octopus (QA Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | nit | `.editorconfig:42` | `dotnet_naming_style.end_in_async.capitalization              = pascal_case` has 4 fewer spaces before `=` than every other line in the section, so it breaks the file's column alignment. | Pad it to the same column as line 41. Optional. |
| Q2 | nit | `docs/coding-guidelines.md:20` (CG73, Source column) | The Source column says "code: no BOM in `src/`". The rule text in the same row exempts `src/BlazorInvaders.sln`, and that file still has a BOM (`head -c3` gives `ef bb bf`). | Change it to "no BOM in `src/` except the `.sln`", or drop the clause. Optional. |

## QA evidence
Assurance: standard. `.devagents/config.json` has no `quality` section, so there is no configured mutation, coverage or complexity tool. No production logic changed: the C# changes are formatter whitespace and BOM removal only.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| Step 1: complete `.editorconfig` | `.editorconfig:1-14` (root, `[*]`, `[*.{csproj,yml,yaml}]`, `[*.sln]`), `:32-42` (Async rule, comment naming the CG2 exceptions) | done |
| Step 2: apply the formatter | 10 `src/core` files. `git diff -w --ignore-cr-at-eol` shows only the BOM lines on line 1 and the removed whitespace-only last line of `Alien.cs` (the plan-review P1 rewording) | done |
| Step 3: remove the remaining BOMs | `src/Shared/MainLayout.razor:1`, `src/_Imports.razor:1`, `src/wwwroot/css/app.css:1` (BOM only, nothing else) | done |
| Step 4: CI step | `.github/workflows/deploy.yml:27-28`, in `build_and_test` between Setup .NET and Build | done |
| Step 5: architecture | `docs/architecture.md:3` (Last updated), A6 and A7 (`:16-17`), Decisions line `:37` | done |
| Step 6: coding guidelines | `docs/coding-guidelines.md:3`, Tools row `:10`, CI sentence `:13`, CG72/CG73 `:19-20`, CG1 `:25`, Open question removed, Revisions `:95` | done |
| Step 7: README | `README.md:92-97` | done |
| Tests AC1: `.editorconfig` keys, IDE1006 at `SaveHighScore.cs:49` | Run in scratch: it reports `SaveHighScore.cs(49,49)` plus the three `Run` methods | pass |
| Tests AC2: `--verify-no-changes` exits 0, diff only BOM/whitespace, no BOM in the 10 files | Exit 0. `head -c3` of all listed files shows no `ef bb bf`. No BOM remains in `*.cs/razor/css/js/json/csproj` under `src` or `tests`. Only the `.sln` keeps one | pass |
| Tests AC3: step before Build, negative check | Step present. B1-B4 below each make the CI command exit 2 | pass |
| Tests AC4: docs match steps 5-6, no `src/.editorconfig` left | grep finds no `src/.editorconfig` in architecture, guidelines or README | pass |
| Tests AC5: builds without warnings, tests pass | Both builds: 0 warnings, 0 errors. `dotnet test`: 211 passed, 0 failed, 0 skipped | pass |

No unplanned hunks. Every changed file is in WP1's file list. The exception is `docs/work/TRACKER.md`, which the PM owns.

### Coverage and CRAP
No coverage tool is configured. Nothing was measured, because no production logic changed: the changes are whitespace and BOM only, and `git diff -w` confirms it.

### Mutation testing
Not run: no `quality.mutation` is set up, and no logic changed.

### Breaks
The "tests" here are the plan's check commands, so the breaks target what those commands must catch. All 5 breaks are listed in `C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-009-enforce-formatting\docs\work\CHORE-009-enforce-formatting\reviews\qa-r1-breaks.json`. B5 uses a helper script, `C:\Users\elici\AppData\Local\Temp\claude\abad4d2d2f6bcd2d\41501319-ac2a-40b7-9c4d-6a53fd059269\scratchpad\qa009\check-async.mjs`. It exits 0 only if the AC1 command reports `SaveHighScore.cs(49,` "Missing suffix: 'Async'". Its baseline run in scratch passed.

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | AC2/AC3, api project covered: double space inside a statement | src/api/SaveHighScore.cs:17 | `dotnet format src/BlazorInvaders.sln --verify-no-changes` | caught (exit 2) |
| B2 | AC2/AC3, tests project covered: spaces inside parentheses | tests/BlazorInvaders.Tests/ComboTrackerTests.cs:8 | same | caught (exit 2) |
| B3 | AC1/AC3 charset: BOM put back into Enumerations.cs | src/core/GameObjects/Enumerations.cs:1 | same | caught (exit 2) |
| B4 | AC1 indentation: tab indent | src/core/GameEngine.cs:12 | same | caught (exit 2) |
| B5 | AC1 Async rule disabled (severity `none`) | .editorconfig:35 | `node .../check-async.mjs` | caught (exit 1) |

5 breaks: 5 caught, 0 survived.

## Checked and fine
- The Alien.cs whitespace-only last line and the unchanged Razor/CSS line 1s fit the reworded P1 gate. The P2 wording in A6 matches what the full `dotnet format` actually enforces.
- The working copies of the three stripped files and of `coding-guidelines.md` are now LF (git warns "LF will be replaced by CRLF"). The index already stores LF, so the committed diff is BOM only (`--numstat` gives 1/1 for each). It has no effect on what gets committed.
- The CI step runs in the secret-free `build_and_test` job, adds no new action and no package, and `build_and_deploy` is unchanged.

## Notes for the PM
- `test-report.md` is missing from the item folder, although the status log has Squid reporting PASS (5/5 ACs, 211 tests). I re-ran every check myself.
- Not checked: whether the solution loads on Linux CI (`..\tests\...` paths). The plan's Risks section already covers this, with a per-csproj fallback.

Restored: yes

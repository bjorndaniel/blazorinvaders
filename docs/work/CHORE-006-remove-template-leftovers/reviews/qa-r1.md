VERDICT: APPROVED

# QA review: CHORE-006 Remove template leftovers, round 1

**Reviewed:** diff vs base_commit f2ce768 (staged rename + 3 deletions) + untracked files (only the item folder `docs/`)
**Reviewer:** Octopus (qa-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|

None.

## QA evidence
Assurance: standard. The change has no production code: 3 files deleted and 1 file moved without changes. `quality` in `.devagents/config.json` sets up no mutation, coverage or complexity tool.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| WP1 step 1: remove `SaveHighScore.cs` | `D SaveHighScore.cs` (staged) | done |
| WP1 step 2: remove `src/wwwroot/routes.json` | `D src/wwwroot/routes.json`. At base its only rule was `/*` serves `/index.html` with 200. `staticwebapp.config.json` `navigationFallback.rewrite: /index.html` covers it. | done |
| WP1 step 3: remove `sample-data/weather.json` and the folder | `D src/wwwroot/sample-data/weather.json`. `src/wwwroot/sample-data` no longer exists. | done |
| WP1 step 4: move `.editorconfig`, content unchanged | `R src/.editorconfig -> .editorconfig`, 0 insertions and 0 deletions, no `root = true` added | done |
| WP1 step 5: build, test, format baseline | I re-ran build and test myself (see below). For `dotnet format` I rely on the baseline in the implementation notes. | done |
| Tests: AC1-AC4 manual (git status, grep) | `git status --porcelain` matches the steps above. `git grep -I -E "routes\.json\|weather\.json\|sample-data\|SaveHighScore"` outside `docs/`, `.devagents/` and `.claude/` matches only `src/api/SaveHighScore.cs` and `src/Components/SetHighScore.razor`. Those belong to the live API function and its caller, not the deleted root file. `.github/`, the csproj files and `service-worker*.js` have no matches. | done |
| Tests: AC5 existing suite | In the scratch copy, `dotnet build src/BlazorInvaders.sln` reports 0 warnings and 0 errors and builds all 5 projects. `dotnet test src/BlazorInvaders.sln` passes 211/211, none skipped. | done |

Every changed file belongs to WP1. No hunk is unplanned. No `TreatWarningsAsErrors`, `EnforceCodeStyleInBuild` or `AnalysisLevel` setting exists in any csproj or Directory.Build file. The moved rules are `suggestion` or `silent` only, so bringing `tests/` under them cannot fail the build.

### Coverage and CRAP
Not run: `quality.coverage` is not set up, and no production lines changed, so there is nothing to cover.

### Mutation testing
Not run: `quality.mutation` is not set up, and no production code changed.

### Breaks
None run, and no breaks file was written. The diff has no function, condition or AC with logic to break: every AC is a file removal or a move, and the `breaks` tool can only replace text inside existing files. The ACs were checked directly instead: git status, the reference grep, the scratch copy layout (`.editorconfig` at the root and no `src/.editorconfig`), and a full build and test.

## Checked and fine
- The `routes.json` content at base is redundant with `navigationFallback`, so nothing needed moving into `staticwebapp.config.json` (AC2).
- The `.editorconfig` rename is byte-identical, with a single `[*.{cs}]` section and no `root = true` (AC4).
- Before deletion, no project could compile the root `SaveHighScore.cs`: there is no csproj at the root and every project is under `src/` or `tests/`.
- No build, CI, service worker or README reference to any removed file remains (AC3, request constraints).
- The full solution builds with 0 warnings and 0 errors, and the tests pass 211/211 (AC5).
- `git status --porcelain` was the same before and after my review.

Restored: yes

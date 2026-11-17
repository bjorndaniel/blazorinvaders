# Plan: CHORE-006 Remove template leftovers

| | |
|---|---|
| **ID** | CHORE-006 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-17 |
| **Size** | S — three file deletions and one file move |
| **Security-sensitive** | no — no code, input or access change; routing is unchanged (see Security) |
| **UI-facing** | no — no user-visible change |
| **Opened** | 2026-11-17 |
| **Plan round** | 1 |

## Summary
Delete the three dead files and move `src/.editorconfig` to the repository root with `git mv`, so it also covers `tests/`. `routes.json` holds one rule (`/*` serves `/index.html`), which `staticwebapp.config.json` already covers through `navigationFallback`, so nothing needs moving. The `.editorconfig` content stays byte-identical, with no `root = true` added. The rules are all `suggestion` or `silent`, so the build does not change and no code is reformatted.

## Context found
- A grep finds no reference to `routes.json`, `weather.json`, `sample-data` or `SaveHighScore.cs` outside request.md. Only `src/.editorconfig` (19 lines, `[*.{cs}]` section only) exists as an editorconfig; no other `.editorconfig` exists in the repo.
- `src/wwwroot/staticwebapp.config.json`: `navigationFallback` rewrites to `/index.html`, so the `routes.json` `/*` rule is redundant. Azure Static Web Apps ignores `routes.json` when `staticwebapp.config.json` exists.
- No `docs/coding-guidelines.md` was checked; the existing `.editorconfig` is the style source.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Delete leftovers, move editorconfig | S | `SaveHighScore.cs`, `src/wwwroot/routes.json`, `src/wwwroot/sample-data/weather.json`, `src/.editorconfig` -> `.editorconfig` | — |

## Steps
### WP1
1. **Remove the dead function** — `git rm SaveHighScore.cs`.
2. **Remove the routing leftover** — `git rm src/wwwroot/routes.json`. No rule moves: its only rule is covered by `navigationFallback`.
3. **Remove sample data** — `git rm src/wwwroot/sample-data/weather.json`; delete the folder if it remains.
4. **Move editorconfig** — `git mv src/.editorconfig .editorconfig`, content unchanged.
5. **Verify** — `dotnet build` of the solution (`src/BlazorInvaders.sln`) and `dotnet test`; both succeed. Run `dotnet format --verify-no-changes` only to confirm that the build is unaffected, not to reformat; do not apply any changes.

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC1-AC4 | No new test: file removal and move; verified by `git status` and a repeated grep for the removed names | manual | WP1 |
| AC5 | Existing suite: `dotnet build` and `dotnet test` pass with the same results as at base | existing | WP1 |

## Security considerations
No trust boundary, access rule or dependency is touched (no `docs/security-model.md` entry applies or needs updating). Routing behaviour on Static Web Apps is unchanged: the SPA fallback stays in `staticwebapp.config.json`. Removing the unused function file and the sample data removes dead code only.

## UX considerations
None: not UI-facing.

## Risks and rollback
- A root `.editorconfig` also applies to `src/api` and `src/core`; they were already under `src/`, so no change. The `tests/` code may now show new suggestion-level IDE hints only; the build is unaffected.
- Rollback: `git revert` the commit.

## Out of scope
Reformatting, adding `root = true`, adding other editorconfig sections, any other refactoring.

## Revisions

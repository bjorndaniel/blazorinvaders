# Request: CHORE-006 Remove template leftovers

**ID:** CHORE-006  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
"add clean up": after the architecture proposal (CHORE-003) listed leftovers, the user asked for a cleanup item.

Leftovers found by the Planner (CHORE-003 architecture proposal):
- `SaveHighScore.cs` at the repository root: an old in-process Functions template that no project compiles.
- `src/wwwroot/routes.json` and `src/wwwroot/sample-data/weather.json`: template leftovers (`src/wwwroot/staticwebapp.config.json` exists).
- `src/.editorconfig` does not cover `tests/`.

## Problem / motivation
Dead files mislead readers and agents; the test project is not covered by the formatting rules.

## Acceptance criteria
- [ ] AC1: `SaveHighScore.cs` at the repository root is removed.
- [ ] AC2: `src/wwwroot/routes.json` is removed; any rule in it that `staticwebapp.config.json` does not already cover is moved there first, so routing on Azure Static Web Apps is unchanged.
- [ ] AC3: `src/wwwroot/sample-data/weather.json` (and the folder, if then empty) is removed, and nothing references it.
- [ ] AC4: the `.editorconfig` rules also apply to `tests/` (for example by moving it to the repository root), without reformatting existing code.
- [ ] AC5: the solution builds and all tests pass; game behaviour is unchanged.

## Project context
- `README.md`: Blazor WebAssembly (.NET 10) game deployed to Azure Static Web Apps with an Azure Functions API.

## Constraints and non-goals
- Assumed: no reformatting of existing files; no other refactoring.
- Assumed: removed files are not referenced by the build, CI, service worker cache list or README; anything that does reference them is updated.

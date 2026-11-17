# Implementation: CHORE-006 Remove template leftovers — WP1

## Round 1
**Plan steps covered:** WP1 steps 1, 2, 3, 4, 5
**Addresses findings:** P1 (plan-r1, minor): addressed. The `dotnet format` check now has a baseline. It was run at `base_commit` before any change and again after the change, and the results were compared (see Commands run). Both report the same 32 errors, all pre-existing whitespace findings in `src/core`, so the move changes nothing.
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `SaveHighScore.cs` | deleted — root-level Functions template, compiled by no project |
| `src/wwwroot/routes.json` | deleted — its only rule (`/*` to `/index.html`) is covered by `navigationFallback` in `staticwebapp.config.json` |
| `src/wwwroot/sample-data/weather.json` | deleted — unreferenced sample data; the folder was empty afterwards and was removed |
| `src/.editorconfig` | moved to `.editorconfig` (`git mv`, content unchanged) so it also covers `tests/` |

### Deviations from plan
None.

### Findings with no change, and disputed findings
None.

### Minor findings not fixed
None.

### Commands run
- `git grep -I -E "routes\.json|weather\.json|sample-data|^/SaveHighScore"` outside `docs/` → no matches (exit 1).
- `dotnet format src/BlazorInvaders.sln --verify-no-changes` at base → exit 2, 32 pre-existing errors (WHITESPACE in `src/core`). Output kept in the scratchpad.
- `dotnet build src/BlazorInvaders.sln` → succeeded, 0 warnings, 0 errors.
- `dotnet test tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj` → passed, 211 of 211.
- `dotnet format src/BlazorInvaders.sln --verify-no-changes` after the change → exit 2, same 32 errors as at base (no new findings). No formatter was applied.

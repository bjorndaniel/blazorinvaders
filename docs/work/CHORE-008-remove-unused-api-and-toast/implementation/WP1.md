# Implementation: CHORE-008 Remove unused GetHighScore function and Blazored.Toast — WP1

## Round 1
**Plan steps covered:** WP1 steps 1, 2, 3, 4, 5, 6, 7
**Addresses findings:** n/a
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/api/GetHighScore.cs` | deleted: the `gethighscore` function, the only user of `RequestFailedException` |
| `src/BlazorInvaders.csproj` | modified: removed the `Blazored.Toast` PackageReference line |
| `docs/coding-guidelines.md` | modified: CG16 "two functions"; CG21 example and evidence moved to `HighScoreValidator.cs` and `SaveHighScore.cs`; header and Revisions |
| `docs/ux-guidelines.md` | modified: D4 reworded without the package name; UX20 drops the Blazored.Toast sentence; header and Revisions |
| `docs/security-model.md` | modified: AC2, R1, TB3 and T13 drop the removed endpoint and package; header and Revisions |

### Deviations from plan
- Revisions lines in the three docs describe the change without naming `GetHighScore`, `gethighscore` or `Blazored.Toast`, so the plan's step 7 grep prints nothing outside `docs/work/`.

### Findings with no change, and disputed findings
n/a

### Minor findings not fixed
n/a

### Commands run
- `dotnet build src/BlazorInvaders.sln` → succeeded, 0 warnings, 0 errors
- `dotnet test tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj` → passed, 211 of 211
- `dotnet format src/BlazorInvaders.sln --verify-no-changes --no-restore` → exit 0
- `git grep -n -i -E "gethighscore([^s]|$)|blazored\.toast" -- . ':!docs/work'` → no matches
- No Markdown formatter is configured in the repository (no prettier config or package.json), so none was run on the docs; the csproj change removes one line.

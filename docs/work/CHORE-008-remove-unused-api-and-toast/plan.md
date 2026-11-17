# Plan: CHORE-008 Remove unused GetHighScore function and Blazored.Toast

| | |
|---|---|
| **ID** | CHORE-008 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-17 |
| **Size** | S - delete one file, one package line, four doc edits |
| **Security-sensitive** | yes - removes a NuGet dependency and changes the security model's access model (AC2, R1) and a trust boundary (TB3) |
| **UI-facing** | no - Toast has no markup, CSS or registration; no visible change |
| **Opened** | 2026-11-17 |
| **Plan round** | 2 |

## Summary
Delete `src/api/GetHighScore.cs` and the `Blazored.Toast` PackageReference, then fix the docs that cite them. Verified: no code, test, CI, README, CSS or `index.html` references either, and `Program.cs` registers only `AddBlazoredModal`. Choices: CG21's example and evidence in `docs/coding-guidelines.md` move to `src/api/HighScoreValidator.cs` (`catch (JsonException)`) and `src/api/SaveHighScore.cs` (boundary catch-all), since the current evidence file is deleted. Departure: CG16 (should) asks the next item touching the API to create `FunctionResponses.cs`; this item defers that, because the request keeps the other endpoints unchanged, and only corrects CG16's count to two functions. The security model's AC2, R1, TB3 and T13 drop the removed endpoint and package. Historical files under `docs/work/` are left as they are. Security: shrinks the anonymous API surface and removes an unused dependency (T13).

## Context found
- `src/api/GetHighScore.cs` is the only user of `using Azure;` and `RequestFailedException`; `TableClient` use is also in `GetLeaderboard.cs` and `SaveHighScore.cs`, so no csproj change in `src/api`. No other helper is used only by it.
- `src/BlazorInvaders.csproj:30` holds the Toast reference; `_Imports.razor` and `Program.cs` already contain only Modal.
- Docs mentioning them: `docs/coding-guidelines.md` CG21 (example and evidence, line 48) and CG16 ("three functions", line 41); `docs/ux-guidelines.md` line 11 (D4) and UX20 (line 38); `docs/security-model.md` AC2 (line 23), R1 (line 30, "All three"), TB3 (line 42) and T13 (line 64). `docs/architecture.md` lists only Blazored.Modal and does not name the endpoint: no change.
- Guidelines: CG16 (should, deferred, see Summary), CG21 (must; example only). `.editorconfig` applies to edited text.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Remove function and package, update docs | S | `src/api/GetHighScore.cs`, `src/BlazorInvaders.csproj`, `docs/coding-guidelines.md`, `docs/ux-guidelines.md`, `docs/security-model.md` | - |

## Steps
### WP1
1. **Delete the function** - `src/api/GetHighScore.cs`.
2. **Drop the package** - `src/BlazorInvaders.csproj`: remove the `Blazored.Toast` PackageReference line.
3. **CG21 and CG16** - `docs/coding-guidelines.md`: CG21's example becomes `catch (JsonException)` in `HighScoreValidator`, rule otherwise unchanged; evidence becomes `code: src/api/HighScoreValidator.cs, src/api/SaveHighScore.cs`. CG16: "moves the three functions" becomes "moves the two functions".
4. **UX20 and D4** - `docs/ux-guidelines.md`: UX20 drops "Blazored.Toast is unused and will be removed by CHORE-008." and keeps "There are no toasts."; line 11 D4 reworded to "there is no toast or notification library; messages go on the canvas or in page text" with no package name.
5. **Security model** - `docs/security-model.md`: AC2 lists only `getleaderboard` and `savehighscore`; R1 "All three use" becomes "Both use"; TB3 becomes "Internet to `GET /api/getleaderboard`"; T13 drops "and an unused package (`Blazored.Toast`)".
6. **Revisions** - in each of the three edited docs, set the "Last updated" header to `2026-11-17, by CHORE-008` and add a dated CHORE-008 line to its **Revisions** section naming what changed.
7. **Verify** - `git grep -n -i -E "gethighscore([^s]|$)|blazored\.toast" -- . ':!docs/work'` prints nothing; then build and test.

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC1-AC3 | `git grep` in step 7 finds no `GetHighScore` or `Blazored.Toast` outside `docs/work/` history | manual check | WP1 |
| AC4 | `dotnet build` of the app, `src/api` and tests, then `dotnet test tests/BlazorInvaders.Tests` pass; existing `HighScoreValidatorTests` still cover the remaining API | existing tests | WP1 |

No new tests: pure removal, nothing tests `GetHighScore`. No real dependencies involved.

## Security considerations
Removes an anonymous GET endpoint and one NuGet package. Security model: actor AC2 and rule R1 lose the endpoint; trust boundary TB3 narrows to `getleaderboard`; T13 loses its unused-package gap; T4 (anonymous flood) surface shrinks. No new input, secrets or dependency. The endpoint read the table entity `HighScore/Current`, which nothing writes; no data exposure remains. `docs/security-model.md` is updated in steps 5-6. Other endpoints unchanged.

## UX considerations
None: not UI-facing.

## Risks and rollback
- An external caller of `/api/gethighscore` would get 404: no client in the repo calls it (grep). Revert the commit to roll back.
- The generated `bootstrap.min.css` contains `.toast` classes; these are Bootstrap's, not Blazored's, and stay.

## Out of scope
Updating the deployed Function App, T1-T4, UX gap fixes, and `docs/work/` historical files.

## Revisions
Round 1: initial plan.
Round 2:
- P1: step 5 also fixes AC2, R1 and TB3; Context and Security list them.
- P2: Security-sensitive set to yes.
- P3: CG16 deferral declared in Summary; step 3 changes "three" to "two".
- P4: step 3 replaces CG21's example and cites HighScoreValidator.cs and SaveHighScore.cs.
- P5: step 7 uses the reviewer's `git grep`.
- P6: new step 6 updates headers and Revisions of the three docs.

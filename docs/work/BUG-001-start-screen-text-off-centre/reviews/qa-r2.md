VERDICT: APPROVED

# QA review: BUG-001 Start screen text off-centre, round 2

**Reviewed:** changes since round 1 (`changes BUG-001-start-screen-text-off-centre --since 1`): only test code changed (`tests/BlazorInvaders.Tests/SceneBuilderTests.cs`, +19 -4), plus the PM's `docs/work/TRACKER.md` status row
**Reviewer:** Octopus (qa-reviewer)

## Earlier findings
| ID | Status | Evidence |
|----|--------|----------|
| Q1 | resolved | The new test `GameOverScreen_DailyHint_IsCentred` (870, 801) sets `TodaysChallenge` on the real `SceneBuilder`, plays a real game until `GameOver` (and asserts that it got there), then calls `AssertCentred(scene, width, "D for today's challenge")`. Round 1 break B1 was run again and is now caught by `GameOverScreen_DailyHint_IsCentred(width: 801)`. |
| Q2 | withdrawn | The implementer's dispute holds. I checked it myself. `Daily` has a private setter, and only `GameEngine.Start` sets it (`src/core/GameEngine.cs:116,182`). `Start` also sets `Started = true` (`:203`). `GameOver = false` is set nowhere except `Start` (`:204`). `EndGame` sets `GameOver = true, Started = false` (`:403-404`). The app makes a single `readonly` engine and never replaces it (`src/Pages/Index.razor:66`). The attract screen draws only when `!Started && !GameOver` (`src/core/Rendering/SceneBuilder.cs:94-97`). So it can only show before the first `Start`, when `Daily` is null. The `"TODAY'S CHALLENGE"` branch at `SceneBuilder.cs:312` cannot be reached, so no test through the public API can pin it. Break B2 surviving again is consistent with that. |

## Findings
None.

## QA evidence
Assurance: standard. No production code changed in this item or in this round. `quality` is declined in `.devagents/config.json`, so no coverage, complexity or mutation tool is set up. Round 2 changed only test code, so as my instructions require for that case I re-ran the round 1 survivors and read the new or changed tests. Round 1 covered the traceability and the other breaks.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| WP1 step 1: centring tests | `SceneBuilderTests.cs`; new in r2: `GameOverScreen_DailyHint_IsCentred` (~:471) | done (round 1 trace stands) |
| WP1 step 2: `dotnet test` passes, no production change | the `breaks` baseline run of the full suite passed; the change touches only the test file | done |
| Tests row AC1/AC2/AC3 | as in round 1, plus the game-over daily hint (Q1). The daily leaderboard title cannot be reached (Q2 withdrawn) | done |
| Tests row AC4 | `StartScreen_LinesAreCentred_WithUnchangedTextFontAndY` now asserts Y and font on the line `AssertCentred` returns (C2) | done |

Changed files: only `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` (WP1). The `TRACKER.md` change is the PM's status bookkeeping, not code.

New and changed tests, read as in 2a: they call the real `GameEngine` and `SceneBuilder`, and assert on decoded commands (`Assert.Single`, centre flag, `x == width / 2`). They have no mocks, no tautologies, no skip or focus, and no swallowed exceptions. The `AssertCentred` refactor (C2) keeps every earlier assertion: it now returns the same line it checked through `Assert.Single`, instead of looking it up a second time.

### Coverage and CRAP
Not applicable: no coverage tool is set up, and no production line changed.

### Mutation testing
Not run: no mutation framework is set up, and no production code changed.

### Breaks
Command: `node .devagents/devagents.mjs breaks BUG-001-start-screen-text-off-centre .work/BUG-001-start-screen-text-off-centre/docs/work/BUG-001-start-screen-text-off-centre/reviews/qa-r2-breaks.json`

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | r1 survivor, AC2 game-over daily hint off-centre | src/core/Rendering/SceneBuilder.cs:283 | `dotnet test tests/BlazorInvaders.Tests --nologo` | caught: GameOverScreen_DailyHint_IsCentred(width: 801) |
| B2 | r1 survivor, AC2 daily leaderboard title off-centre | src/core/Rendering/SceneBuilder.cs:312 | `dotnet test tests/BlazorInvaders.Tests --nologo` | SURVIVED: expected. The branch cannot be reached (Q2 withdrawn), so this is not a finding |

## Notes for the PM
- Outside this item's scope, and existing before it: `SceneBuilder.cs:311-312` (comment "The board of the mode last played", `engine.IsDaily ? "TODAY'S CHALLENGE" : ...`) is dead code under the current engine. After the first game, `GameOver` stays true and the attract screen never shows again, so the daily board title is never drawn. This is either dead code to remove, or a missing return-to-attract behaviour. It might be worth a follow-up item; the user decides.
- The brief asked me to look at the dispute in `implementation/WP1.md`. That is a file pointer, not a verdict. I reached my conclusion from the code myself.
- The only file I wrote is the breaks list: `C:\Projects\bjorndaniel\blazorinvaders\.work\BUG-001-start-screen-text-off-centre\docs\work\BUG-001-start-screen-text-off-centre\reviews\qa-r2-breaks.json`. I did not write `reviews/qa-r2.md`: my instructions say this response is the review document, so the PM saves it.

Restored: yes

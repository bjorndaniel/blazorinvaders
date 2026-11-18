VERDICT: CHANGES_REQUESTED

# QA review: CHORE-014 Fix the five UX gaps, round 1

**Reviewed:** diff vs base_commit d2bf333 + untracked files
**Reviewer:** Octopus (QA Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | major | `test-report.md` ACs table, rows AC1-AC4 (production code at `src/Pages/Leaderboard.razor:29-34,90-98`, `src/wwwroot/css/app.css:221-224,348-351`, `src/Components/SetHighScore.razor:6-18`, `src/wwwroot/index.html:7`) | The plan's Tests table covers AC1-AC4 with manual browser checks, because A5 rules out component tests. None of them was run. The test report says "verified by inspection" and "manual browser check not run" / "no browser". This leaves several things unchecked: the failed-load and `Try again` behaviour (AC1), whether the focus ring shows on a fresh load (the AC2 risk the plan asked to record), the computed accessible names (AC3), and pinch zoom on the leaderboard and dialog versus the playfield (AC4). This code has no automated tests by design, so these checks are its only tests, and a deliberate break in any of these files would survive. Where it matters: AC1-AC4 and error handling (AC1). | The Tester runs the four manual checks exactly as the plan's Tests rows describe them. The app can be started with `uxPreview.command` from `.devagents/config.json`. Record each result in `test-report.md`, including what was seen for the AC2 fresh-load focus ring. |

## QA evidence
Assurance: standard. `quality` is not configured (`.devagents/config.json` has `"declined": ["quality-tools"]`).

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| Step 1 UX24 load failure | `src/Pages/Leaderboard.razor:29-34` (alert text and `btn btn-secondary` Try again, before Loading/empty), `:74` `_loadFailed`, `:82` cleared, `:93-98` `catch (Exception ex)` that logs and sets the flag only when `_daily == daily`; `_scores` left null | done |
| Step 2 UX32 focus | `src/wwwroot/css/app.css:214-224` (`outline: none` removed, `.game-canvas:focus-visible` white), `:342-351` (`.bm-content .initials:focus` black) | done |
| Step 3 UX33 names | `src/Components/SetHighScore.razor:6` label id, `:9` `role="group" aria-labelledby`, `:11/14/17` `aria-label` "Initial n of 3" | done |
| Step 4 UX38 zoom | `src/wwwroot/index.html:7`; `touch-action: none` kept at `app.css:218,245` | done |
| Step 5 UX43 pause text | `src/core/Rendering/SceneBuilder.cs:124` | done |
| Step 6 test | `tests/BlazorInvaders.Tests/SceneBuilderTests.cs:131-144` | done |
| Step 7 guidelines | `docs/ux-guidelines.md`: UX24/32/33/38/43, Known gaps, Revisions, Last updated | done (wording as planned) |
| Step 8 verify | build and test re-run by me: 263 passed | done |
| Tests AC1 (manual) | not run | missing (Q1) |
| Tests AC2 (manual) | not run | missing (Q1) |
| Tests AC3 (manual) | not run | missing (Q1) |
| Tests AC4 (manual) | not run | missing (Q1) |
| Tests AC5 | `SceneBuilderTests.PausedScene_InstructionMatchesTheControls` (true/false cases; Contains expected, DoesNotContain other) | done, checks what the row says |
| Tests AC6 | Known gaps holds only "None open..." plus the UX29/UX14 line; suite passes | done |

Every production change belongs to WP1 and no hunk is unplanned. `docs/work/TRACKER.md` is the PM's record and is not a WP file.

### Coverage and CRAP
No coverage tool in this project (`quality` declined, no coverage script). The only changed logic in reach of tests is the ternary at `SceneBuilder.cs:124`, and both of its branches run under the AC5 theory. Razor, CSS and HTML are outside automated tests by A5.

### Mutation testing
Not run: no mutation framework is set up (`quality` declined).

### Breaks
Run with `node .devagents/devagents.mjs breaks CHORE-014-fix-ux-gaps .work/CHORE-014-fix-ux-gaps/docs/work/CHORE-014-fix-ux-gaps/reviews/qa-r1-breaks.json`. These are separate from the Tester's B1 (text reverted) and B2 (condition inverted). I made only 3 of the 5 allowed breaks: line 124 is the only changed logic that tests can reach, and a break in the Razor, CSS or HTML would survive by definition (see Q1).

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | AC5 touch: keyboard hint also drawn | src/core/Rendering/SceneBuilder.cs:124 | `dotnet test tests/BlazorInvaders.Tests --nologo -v q --filter FullyQualifiedName~SceneBuilderTests` | caught (1 failed) |
| B2 | AC5 keyboard: touch hint also drawn | src/core/Rendering/SceneBuilder.cs:124 | same | caught (1 failed) |
| B3 | AC5 touch copy: "resume" to "pause" | src/core/Rendering/SceneBuilder.cs:124 | same | caught (1 failed) |

Full suite in the scratch copy: 263 passed, 0 failed.

## Checked and fine
- The new test calls the real `SceneBuilder` with `NewEngine()`. It has real assertions, no mocks and no skips.
- The failed-load branch shows fixed text only; `ex.Message` goes to the console only (UX44, CG20/CG24).
- A stale response for the other tab cannot set `_loadFailed` (`_daily == daily` guard).
- Nothing else blocks zoom: the only `preventDefault` calls in `game.js` are on keydown and on the touch buttons' pointerdown, and `touch-action: none` stays only on `.game-canvas` and `.touch-btn`.
- The "II" in the pause copy matches the touch Pause button label (`Index.razor:44`).

## Notes for the PM
- `docs/work/TRACKER.md` lists CHORE-014 as size M, but the plan and `status.md` say S.

Restored: yes

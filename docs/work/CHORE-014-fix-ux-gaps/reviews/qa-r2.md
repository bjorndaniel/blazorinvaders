VERDICT: CHANGES_REQUESTED

# QA review: CHORE-014 Fix the five UX gaps, round 2

**Reviewed:** the changes since round 1 (`changes CHORE-014-fix-ux-gaps --since 1`): `src/Pages/Leaderboard.razor`, `src/wwwroot/css/app.css`, `docs/ux-guidelines.md`, `docs/work/TRACKER.md`. No test code changed.
**Reviewer:** Octopus (QA Reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| Q1 | resolved | `test-report.md` Round 2 records the AC1-AC4 checks, run in a real headless Chrome 157 against `dotnet run`. The AC2 fresh-load ring is recorded for desktop and for touch emulation. AC4 is checked by computed state: the served viewport meta, and the effective `touch-action` is `auto` on the leaderboard and dialog and `none` on the canvas and touch buttons. A control run showed that a synthesized pinch cannot tell pass from fail in headless Chrome. I accept this as the plan's check. A pinch on a real phone is still untried (see Notes). |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q2 | major | `src/Pages/Leaderboard.razor:93,100,107-108` | Nothing checks the new request counter (the C1 fix). My break B1 drops the guard in `catch`, so a stale failing load sets `_loadFailed` again. That is exactly the C1 race, and the full suite still passes (SURVIVED). No round 2 browser check overlaps two loads either: the Tester's AC1 run covers fail, retry and success one after another. Where it matters: this is error handling. The guard decides whether a caught failure replaces a board that loaded. Under A5 (no component tests), a browser check is the only possible test. | The Tester adds a browser check with the same CDP harness and records it in `test-report.md`. (a) Make the first `getleaderboard` call slow and failing. While it is pending, click the selected tab again (or `Try again`) with a request that succeeds. Expected: the board or "No scores yet" shows, never "Could not load". (b) Make the All-time request slow, then switch to Today's, which succeeds. Expected: Today's result stays. If the PM judges the race too unlikely to test, record it under **Accepted risks**. |
| Q3 | minor | `docs/ux-guidelines.md:54` | The new UX32 sentence cites "(D2)". In this document, D2 means CHORE-005's decision D2 (legend at line 11: the five gaps and pinch zoom). The touch-ring departure is CHORE-014's D2 ("Hide on touch play"), so the citation points at the wrong decision. | Cite it as "CHORE-014 D2", in the text and/or the Source column. |

## QA evidence
Assurance: standard. `quality` is not configured (`"declined": ["quality-tools"]`), so there was no `crap` or `mutate` run.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| Steps 1-8 | unchanged except below; traced in round 1 | done (r1) |
| D2 (Decisions): no playfield ring on touch | `src/wwwroot/css/app.css:291-294`, inside `@media (pointer: coarse)` (`:269`). This is the same query that shows the touch pads and that `game.js:46` uses, and it comes after the base rule at `:221` with the same specificity, so it wins. | done; recorded in WP1 Round 2 |
| D2 guideline update | `docs/ux-guidelines.md:54` | done (citation Q3) |
| U1 focus after retry | `Leaderboard.razor:16,18` refs, `:81-86` `RetryAsync` | done (not a plan step, from review findings; recorded in WP1 Round 2 deviations) |
| U2 `mt-2` | `Leaderboard.razor:32` | done |
| C1 request counter | `Leaderboard.razor:77,93,100,107` | done, untested (Q2) |
| Tests AC1 (manual) | test-report Round 2 AC1, including U1 focus on the selected tab | done |
| Tests AC2 (manual) | test-report Round 2 AC2: desktop Tab and click, fresh load, touch emulation with Tab (D2), initials black outline | done |
| Tests AC3 (manual) | test-report Round 2 AC3: accessibility tree names | done |
| Tests AC4 (manual) | test-report Round 2 AC4: computed viewport and `touch-action`; pinch not reproducible headless | done (by computed state) |
| Tests AC5, AC6 | unchanged since r1; suite passes | done (r1) |

Every round 2 change belongs to WP1 and serves D2, U1, U2 or C1. Each one is recorded in `implementation/WP1.md` Round 2, so there is no unrecorded deviation. `docs/work/TRACKER.md` is the PM's record; the size is now corrected to S.

### Coverage and CRAP
There is no coverage tool configured (`quality` declined). Round 2 changed only Razor, CSS and docs, which A5 places outside the unit tests. No changed line in round 2 is reachable by a test.

### Mutation testing
Not run: no mutation framework is set up (`quality` declined).

### Breaks
Run with `node .devagents/devagents.mjs breaks CHORE-014-fix-ux-gaps .work/CHORE-014-fix-ux-gaps/docs/work/CHORE-014-fix-ux-gaps/reviews/qa-r2-breaks.json`. I made one break per changed piece of logic. Round 1 had no survivors to re-run. `SceneBuilder.cs` and its test are unchanged; round 1 covered them (B1-B3 caught).

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | C1: stale failing load sets the failed state (request guard dropped in catch) | src/Pages/Leaderboard.razor:108 | `dotnet test tests/BlazorInvaders.Tests --nologo -v q` | **SURVIVED** |
| B2 | U1: retry no longer moves focus to the selected tab | src/Pages/Leaderboard.razor:85 | same | **SURVIVED** |
| B3 | D2: touch rule no longer matches, ring shows on touch | src/wwwroot/css/app.css:292 | same | **SURVIVED** |

All three survive the unit suite, as expected under A5. The Tester's round 2 browser checks would catch B2 (AC1 row: "focus lands on the selected tab") and B3 (AC2 row: touch emulation after Tab gives outline `none`), so I raise no finding for them. No check would catch B1, hence Q2.

## Checked and fine
- `RetryAsync`: the tab buttons are always rendered, so the refs are valid when `FocusAsync` runs. If the user switches tabs during a retry, focus goes to the tab that is now selected.
- The counter is incremented before the first `await` (Blazor WASM is single-threaded). A tab switch also bumps it, so the earlier stale-tab protection is kept.
- The D2 rule hides only the playfield ring. The initials inputs keep their black `:focus` outline on touch, which the Tester confirmed.

## Notes for the PM
- The brief had a `User decision:` line beyond the template fields. I did not rely on it: I took D2 from `status.md`, which says the same.
- AC4: a real pinch on a phone has not been tried by anyone. The computed state is correct, but the user may want a quick check on a device.
- AC2: a headed desktop Chrome may still show the script-focus ring on a fresh load (WP1 Round 2 notes). D2 covers only touch, so this is not a finding.

Restored: yes

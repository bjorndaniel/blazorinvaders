VERDICT: APPROVED

# QA review: CHORE-014 Fix the five UX gaps, round 3

**Reviewed:** the changes since round 2 (`changes CHORE-014-fix-ux-gaps --since 2`): `src/Pages/Leaderboard.razor` (+8 -2), `docs/ux-guidelines.md` (+1 -1), `docs/work/TRACKER.md` (+1 -1). No test code changed. I also read the Round 3 sections of `test-report.md` and `implementation/WP1.md`.
**Reviewer:** Octopus (QA Reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| Q1 | resolved | Resolved in round 2. |
| Q2 | resolved | `test-report.md` Round 3 adds the two browser checks I asked for, Q2a (a stale failure after a newer load succeeds) and Q2b (a stale All-time success after the user switches to Today's). Both pass. The Tester did not run them against a break, so I checked against the markup that they can catch one. In `Leaderboard.razor:30` the `_loadFailed` branch renders before the board and before "No scores yet". So if the `catch` guard is dropped (my B1), Q2a would show "Could not load". If the success guard is dropped, Q2b would show ALL. |
| Q3 | resolved | `docs/ux-guidelines.md:54` now cites "CHORE-014 D2" in the text and in the Source column. |

## Findings
None.

## QA evidence
Assurance: standard. `quality` is not configured (`"declined": ["quality-tools"]`), so there was no `crap` or `mutate` run.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| Steps 1-8 | unchanged since round 1 | done (r1) |
| D2, U1, U2, C1 | unchanged since round 2 | done (r2) |
| C2/U3: no focus call after the page is disposed | `src/Pages/Leaderboard.razor:4` (`@implements IDisposable`), `:79` (`_disposed`), `:87-88` (guard in `RetryAsync`), `:91` (`Dispose`) | done. It comes from review findings, not a plan step, and is recorded in WP1 Round 3. It follows the `Index.razor:257` guard pattern. |
| Q3 citation | `docs/ux-guidelines.md:54` | done |
| Tests AC1-AC4 (manual) | test-report Round 2; Round 3 adds the Q2a, Q2b and C2 browser checks | done |
| Tests AC5, AC6 | suite passes, 263/263 | done (r1) |

Every round 3 change belongs to WP1 and serves C2/U3 or Q3. `TRACKER.md` is the PM's phase update. C3 (a nit) was left unchanged, with the reason recorded in WP1 Round 3.

### Coverage and CRAP
No coverage tool is configured (`quality` declined). The only changed production code is Razor, which A5 places outside the unit tests, so no test can reach any changed line from round 3.

### Mutation testing
Not run: no mutation framework is set up (`quality` declined).

### Breaks
Run with `node .devagents/devagents.mjs breaks CHORE-014-fix-ux-gaps .work/CHORE-014-fix-ux-gaps/docs/work/CHORE-014-fix-ux-gaps/reviews/qa-r3-breaks.json`. B1 is the round 2 survivor, run again. B2 is the one new break, for the one changed function with logic.

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | C1 (r2 survivor): stale failing load sets the failed state (request guard dropped in catch) | src/Pages/Leaderboard.razor:114 | `dotnet test tests/BlazorInvaders.Tests --nologo -v q` | **SURVIVED** |
| B2 | C2: focus after retry runs even when the page was left (dispose guard dropped) | src/Pages/Leaderboard.razor:88 | same | **SURVIVED** |

Both breaks survive the unit suite, as expected under A5 (no component tests). The browser checks are the tests here, and each break has one:
- **B1** is the situation Q2a sets up.
- **B2** is caught by the C2 check. Without the guard, `FocusAsync` runs on an element that was removed when the user navigated away. Blazor's focus call throws on an element it cannot find, which shows `#blazor-error-ui`, and the C2 check asserts that it stays hidden.

So I raise no finding for either.

## Checked and fine
- `_disposed` is set synchronously in `Dispose`. It is checked after the only await that can outlive the component (`ShowAsync`), and before the JS interop call.
- CG34 asks for `IAsyncDisposable` on components that hold JS or .NET references to dispose. `Leaderboard` holds only `ElementReference` values, which need no disposal, so `IDisposable` with the guard meets the rule's intent. WP1 Round 3 records this choice.
- The `if` without braces matches the style of `Index.razor:257`, and `dotnet format --verify-no-changes` passed (WP1 Round 3).

## Notes for the PM
- The brief held only template fields and the task. There were no extra instructions.
- The Tester's browser checks (Q2a, Q2b, C2) were not run against the code with the break applied. My judgement that they would catch B1 and B2 comes from reading the markup and Blazor's focus behaviour, not from a run.
- Still open from round 2: nobody has tried a real pinch on a phone (AC4).

Restored: yes

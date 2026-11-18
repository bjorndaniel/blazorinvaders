VERDICT: APPROVED

# QA review: FEAT-002 Larger phone canvas text, round 2

**Reviewed:** the changes since round 1 (`changes FEAT-002-larger-phone-canvas-text --since 1`): SceneBuilder.cs, effects.js, game.js, SceneBuilderTests.cs, docs/ux-guidelines.md
**Reviewer:** Octopus (QA Reviewer)

I fixed nothing. The new tests close all five round 1 findings. My six breaks were all caught, and 337 tests pass. One nit remains.

## Earlier findings
| ID | Status | Evidence |
|----|--------|----------|
| Q1 | resolved | `Phone_PerfectSitsBelowTheEnlargedHitLine` (scale 0.41 and 0.445) builds a real scene with both "PERFECT!" and the hit line shown, and checks that PERFECT is below the hit line. B9 is now caught by it. |
| Q2 | resolved | test-report.md, Round 2, records the check in headless Chrome with the real effects.js. It covers the combo and boss labels at x=5 and x=865, with floor 30 (portrait), floor 27 (landscape) and floor 0 (desktop). The font is at least the floor, and the labels stay inside 0..870. That is the check Q2 asked for. The label code was driven directly, not through a full touch game, which is enough for the clamp. |
| Q3 | resolved | `Phone_FrenzyStaysBelowTheLoweredHeader` (scale 0.30) catches B7. |
| Q4 | resolved | `Phone_HitSizeDoesNotLeakIntoTheNextFrame` (same builder, two frames, PERFECT y = 80) catches B10. |
| Q5 | resolved | `ShrinkToFit_NeverGoesBelowTheDesignSize` catches B4. Both callers (`FitSize`, `HudSize`) now share `ShrinkToFit`, so one test covers the design-size floor for both. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q6 | nit | tests/BlazorInvaders.Tests/SceneBuilderTests.cs (`ShrinkToFit_NeverGoesBelowTheDesignSize`, `SetLostALife`) | The tests reach the private `ShrinkToFit` and the `LostALife` setter by reflection, using string names. If either is renamed, the code still compiles but these tests fail with a NullReferenceException. The tests already have InternalsVisibleTo access. | Optional: make `ShrinkToFit` `internal static`, like `PerfectBaseline` and `HitBaseline`, and call it directly. |

## QA evidence
Assurance: standard. No `quality` tools are set up. Coverage came from the project's coverlet.collector, and no mutation framework is available.

Production code changed this round, so I traced, measured and broke only what changed. Round 1 covered everything else.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| 3 Fit cap (refactored per C2) | SceneBuilder.cs:154-163 (`ShrinkToFit`, `FitSize`) | done. The arithmetic is the same as in round 1. |
| 4 HUD row shared fit (refactored per C2) | SceneBuilder.cs:166-176 (`HudSize`) | done. Same arithmetic. |
| 5 Floating labels (C1: named `MUST_READ_SIZE`) | effects.js:25, :164-168 | done. No change in behaviour. |
| 6 UX14 text (U1: 18 px labels) | docs/ux-guidelines.md:33 | done. Matches `MUST_READ_SIZE = 18`. |
| 7 Manual phone check, floating labels | test-report.md Round 2 | done (Q2) |
| T AC2 hit + PERFECT wiring | `Phone_PerfectSitsBelowTheEnlargedHitLine` | done |
| game.js:30, :86 (`gameCanvas` moved and cleared in `stop()`) | code review C3 | unplanned but harmless, recorded in WP1.md Round 2 |

All changed files are in WP1's list. `docs/work/TRACKER.md` is owned by the PM.

### Coverage and CRAP
Command: `scratch -- dotnet test tests/BlazorInvaders.Tests --collect:"XPlat Code Coverage"`. Result: 337 passed.

Every line changed this round in SceneBuilder.cs (154-176) runs in the tests, and both branches of `ShrinkToFit` are covered. The only partial branches the report shows are at :177-178 (`Describe`), which this item did not change. No complexity tool is set up, so there are no CRAP scores. effects.js and game.js have no test setup.

### Mutation testing
Not run: no mutation framework is set up. I broke the code by hand instead: the four round 1 survivors again, plus one break per changed function with logic.

### Breaks
Test command: `dotnet test tests/BlazorInvaders.Tests --nologo --filter FullyQualifiedName~SceneBuilderTests`. Result: 6 caught, 0 survived.

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B4 | r1 survivor: fit cap may shrink below the design size | SceneBuilder.cs:156 | SceneBuilderTests | caught: ShrinkToFit_NeverGoesBelowTheDesignSize |
| B7 | r1 survivor: FRENZY not shifted with the header | SceneBuilder.cs:337 | SceneBuilderTests | caught: Phone_FrenzyStaysBelowTheLoweredHeader |
| B9 | r1 survivor: PERFECT always at the design y | SceneBuilder.cs:243 | SceneBuilderTests | caught: Phone_PerfectSitsBelowTheEnlargedHitLine |
| B10 | r1 survivor: per-frame `_hitSize` reset dropped | SceneBuilder.cs:212 | SceneBuilderTests | caught: Phone_HitSizeDoesNotLeakIntoTheNextFrame |
| B11 | `FitSize` no longer calls `ShrinkToFit` | SceneBuilder.cs:162 | SceneBuilderTests | caught: Phone_FitCap_ShrinksLongLinesToTheCanvas |
| B12 | `HudSize` uses the full width as the space (no margins or gap) | SceneBuilder.cs:173 | SceneBuilderTests | caught: Phone_FitCap_ShrinksLongLinesToTheCanvas |

Equivalent change I did not try: `width > space` to `>=` in `ShrinkToFit`. When the two are equal, `floor(size × 1)` gives `size` either way.

Breaks file: C:\Projects\bjorndaniel\blazorinvaders\.work\FEAT-002-larger-phone-canvas-text\docs\work\FEAT-002-larger-phone-canvas-text\reviews\qa-r2-breaks.json

## Checked and fine
- The new tests call the production `SceneBuilder`, with no mocks, skips, focused tests or swallowed exceptions. `Phone_PerfectSitsBelowTheEnlargedHitLine` first checks that the hit line really is enlarged (`hit.Size > 20`), so it cannot pass with the hit line at its design size.
- The `CrowdedHudEngine()` refactor keeps the same state for `PhoneScenes` and the fit-cap test.
- `MUST_READ_SIZE` and `MustReadSize` are both 18, and each has a comment pointing to the other.

## Notes for the PM
- The brief held only its template fields and the task.
- My coverage command first wrote its results to `.devagents/runs/.devagents/runs/...`, in the main repository and outside the worktree. I deleted that folder after reading it. Nothing else outside the scratch copy and my breaks file was touched.

Restored: yes

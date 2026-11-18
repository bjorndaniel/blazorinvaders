VERDICT: CHANGES_REQUESTED

# QA review: FEAT-002 Larger phone canvas text, round 1

**Reviewed:** diff vs base_commit 840f5a4 + untracked files
**Reviewer:** Octopus (QA Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | major | src/core/Rendering/SceneBuilder.cs:246 | Nothing tests the call site that moves "PERFECT!" below an enlarged "You were hit!". Break B9 (always use `PerfectBaselineDesign`) survived, and coverlet shows the branch at :246 is only half covered (1/2): no built scene has both flags set. The helpers are tested, but the wiring is not. At scale 0.41 the broken version overlaps: the PERFECT top is about 57, the hit baseline 61. This matters: it serves AC2 and a row of the plan's Tests table. | Add a test that would fail with B9 at :246. Either a touch scene at 0.41 with `ShowPerfectBonus` and `LostALife` both true (the tests have InternalsVisibleTo, so the engine can be driven or seeded into that state), with PERFECT's box below the hit box. Or move the whole choice (hit shown or not, size, y) into the tested internal helper so the Build line has no logic left. |
| Q2 | major | src/wwwroot/js/effects.js:114-120, :164-166; plan step 7 | Nobody verified the floating-label changes of step 5 (raising labels to the floor, keeping them on the canvas). The plan names step 7 as the only check of the JS ("a combo with the player at each screen edge"). The implementer skipped step 7. The Tester's emulation hooked attract and HUD text only and lists "combo at edges: not driven". The other screens the Tester skipped are covered by the AC2 unit tests. The label clamp is not covered anywhere. | Do and record the step 7 check for floating labels: combo (20 px) and boss (24 px) labels at both screen edges, portrait and landscape, at about 375 px with touch. Confirm the font is at least `minTextSize` and the label stays inside the canvas. The UX reviewer's preview may serve, if it records this. |
| Q3 | minor | src/core/Rendering/SceneBuilder.cs:340 | Break B7 (FRENZY at 45, without `_hudShift`) survived. At scale 0.41/0.445 the unshifted line does not overlap, so no test pins the y that step 4 plans. It would overlap the header on smaller phones: at scale 0.30 with no combo, the header bottom is 35 and the FRENZY top about 33. The y is a layout position and does not break AC2 at the AC's 375 px. | Add a frenzy scene at a small scale (e.g. 0.30, no combo) to the overlap check, or assert FRENZY's y = 45 + shift. |
| Q4 | minor | src/core/Rendering/SceneBuilder.cs:215 | Break B10 (drop the per-frame `_hitSize = 0;`) survived. Every test builds a new `SceneBuilder`, but the page reuses one builder for every frame (`_scenes`). A stale hit size from an earlier frame would move PERFECT down when "You were hit!" no longer shows. This is presentation only, with no overlap. | Add a test that builds two frames with the same builder (enlarged hit, then PERFECT without the hit) and checks that PERFECT is at 80. |
| Q5 | minor | src/core/Rendering/SceneBuilder.cs:162 | Break B4 (the fit cap may shrink below the design size) survived. No current string needs shrinking below its design size, so the rule "never below its design size" from step 3 cannot fail. The test's `InRange(26, 39)` cannot detect it. | Add a unit case with a string too long for the canvas even at its design size (e.g. through a scene or an internal `FitSize` seam) and check that the size stays at the design size. |

## QA evidence
Assurance: standard. `quality` has no tools set up in `.devagents/config.json` ("quality-tools" declined). Coverage came from the project's own coverlet.collector. No mutation framework.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| 1 Scale plumbing | game.js:30, :69, :108-110; Index.razor:124-131 | done (CG13 comments on both sides) |
| 2 Size rule, `DisplayScale`, `MinTextSize` | SceneBuilder.cs:66-70, :80-82, :111-118, :151, :270; Scene.cs:33-37 | done |
| 3 Advance table, `EstimateWidth`, fit cap | SceneBuilder.cs:97-107, :120-127, :154-179, :511-515 | done |
| 4 HUD shift, right-aligned combo, hit/PERFECT, bar constants | SceneBuilder.cs:129-149, :203-215, :242-248, :332-340, :93-95, :396 | done; wiring at :246 untested (Q1) |
| 5 Floating labels | effects.js:24, :35-36, :101-120, :164-166; game.js:252 | done; not verified (Q2) |
| 6 UX14, Known gaps, open question, revision | docs/ux-guidelines.md:33, :82-85, :96 | done, matches the code |
| 7 Manual phone check | test-report.md (partial: attract, HUD, hit line) | partial (Q2) |
| T AC1 `PhoneFontSize` theory | `PhoneFontSize_RaisesMustReadTextToTheFloor` | done, all 10 cases |
| T AC1 floor in built scenes, `MinTextSize` | `Phone_MustReadTextReachesTheFloor_AndFloorIsReported` | done |
| T AC2 `EstimateWidth` anchors | `EstimateWidth_MatchesTheMeasuredFont` | done |
| T AC2 on canvas, no overlap, boss bar, daily backdrop, centre | `Phone_TextStaysOnCanvas_AndDoesNotOverlap`, `Phone_CentredLinesKeepTheCentre` | done (combo x8 as count 20, recorded) |
| T AC2 hit + PERFECT baselines | `HitAndPerfectBaselines_StackWhenEnlarged`, `..._KeepDesignPositionsAtDesignSize` | done (helpers only, Q1) |
| T AC2 fit cap at 0.30 | `Phone_FitCap_ShrinksLongLinesToTheCanvas` | done |
| T AC3 desktop unchanged | `Desktop_IgnoresTheScale` + the unchanged pinned BUG-001 tests | done |
| T AC4 | the tests above; UX14 text checked | done |

Work packages: every changed file except `docs/work/TRACKER.md` (PM-owned) is in WP1's list. No unplanned production hunks. Unplanned and harmless: in game.js, `gameCanvas` is declared between `viewWidth` and `viewHeight`.

### Coverage and CRAP
Tool: coverlet.collector, `scratch -- dotnet test tests/BlazorInvaders.Tests --collect:"XPlat Code Coverage"` (332 passed). Uncovered changed lines in SceneBuilder.cs/Scene.cs: none. Partial branches: SceneBuilder.cs:246 (1/2, see Q1). No complexity tool, so no CRAP scores. Index.razor and the JS have no test setup (the plan covers them with step 7).

### Mutation testing
Did not run: no mutation framework is set up. Breaks by hand at the limit without a framework (M: 10).

### Breaks
Test command for all breaks: `dotnet test tests/BlazorInvaders.Tests --nologo --filter FullyQualifiedName~SceneBuilderTests`

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | AC1 must-read boundary `<` to `<=` | SceneBuilder.cs:116 | SceneBuilderTests | caught: PhoneFontSize theory |
| B2 | scale guard `<= 0` to `< 0` | SceneBuilder.cs:116 | SceneBuilderTests | caught: PhoneFontSize theory |
| B3 | fit cap +3 px | SceneBuilder.cs:162 | SceneBuilderTests | caught: Phone_FitCap |
| B4 | fit cap below design size | SceneBuilder.cs:162 | SceneBuilderTests | **SURVIVED** (Q5) |
| B5 | combo width left out of the HUD fit | SceneBuilder.cs:173 | SceneBuilderTests | caught: Phone_FitCap |
| B6 | header baseline not lowered | SceneBuilder.cs:207 | SceneBuilderTests | caught: Phone_TextStaysOnCanvas |
| B7 | FRENZY not shifted | SceneBuilder.cs:340 | SceneBuilderTests | **SURVIVED** (Q3) |
| B8 | hit line ignores the boss wave | SceneBuilder.cs:335 | SceneBuilderTests | caught: Phone_TextStaysOnCanvas (boss bar) |
| B9 | PERFECT always at design y | SceneBuilder.cs:246 | SceneBuilderTests | **SURVIVED** (Q1) |
| B10 | per-frame `_hitSize` reset dropped | SceneBuilder.cs:215 | SceneBuilderTests | **SURVIVED** (Q4) |

Breaks file: C:\Projects\bjorndaniel\blazorinvaders\.work\FEAT-002-larger-phone-canvas-text\docs\work\FEAT-002-larger-phone-canvas-text\reviews\qa-r1-breaks.json

## Checked and fine
- Desktop path: `Size` returns the design size when not on touch, and `FitSize` only acts on enlarged sizes, so desktop output is identical. The pinned BUG-001 tests pass unchanged.
- The new tests call the production `SceneBuilder`. They have no mocks, skips, focused tests or caught exceptions, and the assertions are on real scene output.
- The overlap boxes use the production `EstimateWidth`, which `EstimateWidth_MatchesTheMeasuredFont` anchors to the measured font, as planned.
- effects.js: `textFloor` is set before events spawn labels, `ctx.font` is set before `measureText`, and `parseInt('20px Orbitron', 10)` gives 20. Only the combo (20) and boss (24) labels reach the floor.
- `Tick` has exactly one caller (game.js:110), and it passes the new argument.
- A non-finite or non-positive scale gives the design size, and the size is capped at 60.

Restored: yes

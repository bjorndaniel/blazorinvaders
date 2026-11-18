VERDICT: CHANGES_REQUESTED

# Plan review: BUG-001 Start screen text off-centre, round 1

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | blocker | Summary, Steps 1-3, Risks | The root cause is a guess, and the numbers rule it out. On HEAD every centred line goes out at `x = w/2` with the centre flag (`SceneBuilder.cs:382-383`), and `game.js:210` draws it with `textAlign = 'center'`. That leaves the plan's cause: the gap between advance-centring and ink-centring, which is (first glyph's left bearing minus last glyph's right bearing) / 2. I decoded `src/wwwroot/fonts/orbitron-latin.woff2` (upm 1000). At 28 px the lines are 590.8 px and 320.0 px wide, and the first-glyph left bearings are about 1.5 px. The gap is therefore under about 1 px, and the plan's own Risks section says "a pixel or two". That cannot cause an offset visible in a screenshot, and the fix would move the text by less than a pixel. The step 1 gate also tests the wrong thing. It asks whether ink-centring lands within 2 px, which it will. It never checks whether the current build is more than 2 px off. So the implementer passes the gate, ships the helper, and the tests pass on fake metrics while the reported bug stays. The reported symptom (both 28 px lines right of centre, the 18 px line exact) matches `master` exactly. There, `Game.cs:604-619` (`RenderAttractStart`) calls `MeasureTextAsync` inside a `BeginBatchAsync` batch (`Game.cs:233`), with Blazor.Extensions.Canvas 1.1.1. The measure call goes out at once, while the batched `SetFontAsync("28px")` has not been flushed, so it measures with the earlier 18 px font. The text comes out about 2/3 of its true width, so `x = w/2 - width/2` lands too far right: about 105 px for line 1 and 57 px for line 2. The 18 px line measures correctly. The screenshot is probably from the deployed `master` build, not `feature/arcade-upgrades`. | Make step 1 a real reproduction on the `feature/arcade-upgrades` build: measure where the current text is drawn (scan the canvas pixels for each line's ink, or use `measureText` with the current alignment) against `w/2`. Write no JS fix unless that offset is over 2 px and the fix provably brings it under. If HEAD is already within 2 px, stop with a QUESTION: the bug is in `master` and is already fixed by the refactor. The item then shrinks to AC3, the C# pinning test you already planned, with no JS change. |
| P2 | major | Summary "Choice", WP1, Tests | `node --test` with `tests/js/*.test.mjs` adds a new kind of test and test tool. `docs/architecture.md` A5 says xUnit only with no JS tests, A11 says `tests/BlazorInvaders.Tests` is "the one test project", and `docs/coding-guidelines.md` line 11 lists no JS tooling. The plan has no step updating these documents, and CI never runs the new tests (the plan's own Risks section says so). With a new pattern and 5 files, Size S is also understated. | If P1 removes the JS helper, drop the JS test and keep S. Otherwise declare the choice in the Summary, add a step updating architecture A5/A11, the coding-guidelines tool table and README test steps (CG63), and resize to M. |
| P3 | minor | Step 3 (`effects.js`) | The floating score and pickup labels are not among AC2's centred messages. Changing them goes beyond the request. | Leave `effects.js` alone unless step 1 shows the labels are off-centre too. |
| P4 | nit | whole plan | 5.4 KB is above the roughly 4 KB budget for an S plan. | Trim when revising. |

## Attacks the plan survived
- The C# side is correct: the OP_TEXT argument order in `Scene.cs:13` and `SceneBuilder.cs:371-380` matches `game.js:208-211`, and every listed message uses `CenteredText`.
- The `centredX` formula is right for `textAlign = 'left'`. The ink spans `[x-L, x+R]`, so `x = c - (R-L)/2`, and the planned fake-metric cases check out.
- No text is wider than the canvas (591 px of 870), so clipping is not the cause.
- With an odd width (801), `w/2 = 400` against a true centre of 400.5, which is within AC1's 2 px.
- A new JS file would still work offline: `service-worker.published.js:12` caches every `.js` file from the generated assets manifest.
- `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` exists with `Decode`, `Texts` and `NewEngine` (870x500), as the plan says. The AC1/AC2/AC4 C# tests are doable, and the attract states are reachable through the timestamp.
- Shake (`effects.js:90-95`) moves everything equally and is undone each frame, so it cannot cause the offset.

## Checked and fine
- Security-sensitive `no` and UI-facing `yes` are correct.
- No new package dependency; Node v22 is available locally.
- Leaving the left-aligned score table text (`SceneBuilder.cs:305`) alone is correct.

## Notes for the PM
- Asking the user one question now would save a planning round: was the screenshot taken on the deployed site (`master`) or on a local `feature/arcade-upgrades` build? See the escalation below.

## Escalation
- Type: QUESTION
- What happened: The current branch centres text with canvas `textAlign = 'center'` at `w/2` (`SceneBuilder.cs:382`, `game.js:210`). The font metrics allow under about 1 px of error there. The reported pattern (28 px lines right of centre, 18 px line centred) matches `master`'s `Game.cs:604-619`, which measures inside a Blazor.Extensions.Canvas batch with a stale 18 px font. That would put the 28 px lines about 105 px and 57 px right of centre.
- What I need: Which build was the screenshot taken on: the deployed site (`master`), or the `feature/arcade-upgrades` branch run locally?
- Options: (1) Deployed `master` (recommended assumption): the refactor already fixes the bug. BUG-001 only adds the C# test that pins centring (AC3), with no JS change, and closes when `feature/arcade-upgrades` merges. (2) Local feature build: the Planner reproduces it with a pixel measurement first and fixes the cause that proves out. (3) Not sure: the Planner checks the feature build in step 1 and stops if it does not reproduce.
- State: nothing

VERDICT: APPROVED

# Code review: FEAT-002 Larger phone canvas text, round 1

**Reviewed:** diff vs base_commit 840f5a4 + untracked files
**Reviewer:** Saucer (code-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | minor | src/wwwroot/js/effects.js:166, src/core/Rendering/SceneBuilder.cs:82 | The label threshold `18` in JS mirrors `MustReadSize` in C#, but it is a bare literal. The C# constant has no "must match effects.js" comment (CG13). The JS comment's "Must match Scene.cs" covers only `textFloor`. | Name the JS value (e.g. `MUST_READ_SIZE = 18`) and add "must match" comments on both sides. Or pass the threshold through `Scene` as well. |
| C2 | minor | src/core/Rendering/SceneBuilder.cs:168 | `HudSize` repeats the shrink-to-fit maths in `FitSize` (:155): it enlarges, compares the width with the space, then applies `floor(size × space / width)` with a floor at the design size. | Make `FitSize` take a width function or a precomputed width, so both callers share one shrink rule. Or justify keeping both in the notes. |
| C3 | nit | src/wwwroot/js/game.js:30, :79 | `gameCanvas` is declared between `viewWidth` and `viewHeight`, which split the pair. `stop()` clears `ctx` and `dotnet` but keeps the canvas reference. | Declare it after `viewHeight` and set `gameCanvas = null` in `stop()`. |
| C4 | nit | tests/BlazorInvaders.Tests/SceneBuilderTests.cs | The crowded-HUD setup (leaderboard `WWW 99999`, start, `ComboTracker.Count = 20`) appears twice: once in `PhoneScenes` and once in `Phone_FitCap_ShrinksLongLinesToTheCanvas`. | Optionally extract a local `CrowdedHudEngine()` helper. |

## Checked and fine
- Desktop path: `Size()` returns the design size when `TouchControls` is false, and `FitSize` and `HudSize` only change an enlarged size. The HUD baseline, the combo at `w - 170`, the hit line at 55 and PERFECT at 80 stay exactly as before (AC3). `Desktop_IgnoresTheScale` pins the output.
- At scale 0.41 and 0.445 I traced each screen's vertical stacking against the 0.77 and 0.243 metrics: the HUD, FRENZY, the hit line in normal and boss waves, PERFECT, game over, paused, the daily banner inside its backdrop, and attract states 0-3. No overlaps. The 14 px "SOUND OFF" box clears the right-aligned combo.
- Edge cases: `HitBaseline` and `PerfectBaseline` keep the design positions at 20 px. `_hitSize` is reset each `Build`, so a stale `_hitY` is never used. A scale of 0, NaN or a negative value (a hidden canvas gives `clientWidth` 0) falls back to the design size, and sizes are capped at 60.
- `Text` now computes `space` from `_canvasWidth`. It is set at the top of `Build` before any text call, and every caller of `Text` runs inside `Build`.
- The `Tick` signature change has one caller (game.js:110). Both sides carry "must match" comments (CG13). `Scene.MinTextSize` reaches JS as `minTextSize` through the same camelCase path as `muted`.
- effects.js: `textFloor` is set before this frame's labels are spawned. The clamp measures each label after setting its font, under `textAlign = 'center'`. Labels under 18 px (14 and 16) are left alone.
- CG60: the new public members (`DisplayScale`, `PhoneFontSize`, `MinTextSize`) have summaries. The new numbers are named constants (CG12).
- Performance: width estimates run only for enlarged text, a few short strings per frame.
- `dotnet test --filter SceneBuilderTests`: 81 passed.

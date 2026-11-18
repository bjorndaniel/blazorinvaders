# Test report: FEAT-002

## Round 1
RESULT: PASS

Suite: dotnet test 332 passed; dotnet build 0 errors; `dotnet format src/BlazorInvaders.sln --verify-no-changes` clean. Tests added by me: 0 (implementer's tests cover every plan row).

Manual phone emulation (Chrome headless via CDP, 375 px, touch, DPR 2, fillText hooked; app on a local server, stopped after): 
- Portrait (canvas 359 css px): attract lines at 30 px = 12.4 CSS px; widest line 748 of 870 fits; header 12.4.
- Landscape (667x375, canvas 387 css px): 27-28 px = 12.0-12.5 CSS px; "You were hit!" appeared, 27 px, at y 58. Screenshots show no overlap or cut-off.
- Not driven: boss wave, daily banners, game over, pause, combo at edges (covered by the AC2 unit tests).

Breaks: 6/6 caught (`test-breaks-r1.json`). Mutation: not run (breaks covered the changed rules).

## Round 2
RESULT: PASS

Suite: dotnet test 337 passed; dotnet build 0 errors; `dotnet format --verify-no-changes` clean.
Round 1 results for AC1, AC3, AC4 and the unchanged code are kept; AC2 re-verified.

Tests added (tests/BlazorInvaders.Tests/SceneBuilderTests.cs), 5:
- Q1 `Phone_PerfectSitsBelowTheEnlargedHitLine` (0.41, 0.445; PERFECT and hit both shown)
- Q4 `Phone_HitSizeDoesNotLeakIntoTheNextFrame` (same builder, PERFECT y = 80)
- Q3 `Phone_FrenzyStaysBelowTheLoweredHeader` (scale 0.30)
- Q5 `ShrinkToFit_NeverGoesBelowTheDesignSize` (reflection on the private helper; no scene string is too long at design size)

Q2 browser check (real Chrome headless, real effects.js served from a local http server, stopped after; Orbitron not loaded so widths use the fallback font): combo and boss labels at x=5 and x=865 of an 870 canvas.
- Floor 30 (portrait): font 30px, combo 158 wide, boss 260 wide; drawX clamped to 79/130 left and 791/740 right; all inside 0..870.
- Floor 27 (landscape): font 27px; same clamping; all inside.
- Floor 0 (desktop): 20px/24px, unclamped, as before (off-canvas at edges, unchanged behaviour).
Not run: a full touch game session at 375 px; the label code was driven directly.

## Breaks
`test-breaks-r2.json` (round 1 list, B3/B4 find text updated to the refactored ShrinkToFit): 10/10 caught. B4, B7, B9, B10 now caught by the new tests. Mutation: no framework.

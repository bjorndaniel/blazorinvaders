# Implementation: FEAT-002 Larger phone canvas text — WP1

## Round 1
**Plan steps covered:** WP1 steps 1-6 (step 7 not done, see Deviations)
**Addresses findings:** P7, P8 (plan r3, minor)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/core/Rendering/SceneBuilder.cs` | modified: `DisplayScale`, `PhoneFontSize`, advance table, `EstimateWidth`, fit cap, HUD/hit/PERFECT layout helpers, `MinTextSize` |
| `src/core/Rendering/Scene.cs` | modified: `MinTextSize` |
| `src/Pages/Index.razor` | modified: `Tick(..., float scale)` sets `DisplayScale` |
| `src/wwwroot/js/game.js` | modified: passes canvas scale to `Tick`, `viewWidth` to `effects.draw` |
| `src/wwwroot/js/effects.js` | modified: label floor and on-canvas clamp |
| `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` | modified: FEAT-002 tests |
| `docs/ux-guidelines.md` | modified: UX14 built, Known gaps, open question, revision line |

### Deviations from plan
- Step 7 (manual phone emulation in a browser) not done: no browser available to me. Needs a check by someone who can run the app.
- The "COMBO x8" test state uses combo count 20, since the multiplier caps at x4 (same character widths).
- The paused test scene is built at 4 s so the wave banner is gone (a banner under the pause overlay overlaps at design size, unrelated to this item).

### Findings with no change, and disputed findings
- P7: `HitBaseline` and `PerfectBaseline` return the design positions (55, 80) at the hit design size 20; tested in `HitAndPerfectBaselines_KeepDesignPositionsAtDesignSize`.
- P8: header baseline and shift derive from the final capped size (`HudSize`); `Phone_FitCap_ShrinksLongLinesToTheCanvas` is muted at scale 0.30 and checks combo vs "SOUND OFF".

### Commands run
- `dotnet test` (tests/BlazorInvaders.Tests) -> 332 passed
- `dotnet build src/BlazorInvaders.csproj` -> 0 errors; `dotnet format whitespace --verify-no-changes` on my C# files -> clean
- `node --check` on game.js, effects.js -> ok

## Round 2
**Plan steps covered:** none new (review fixes only)
**Addresses findings:** C1, C2 (code-r1, minor), C3, C4 (code-r1, nit), U1 (ux-r1, nit)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/core/Rendering/SceneBuilder.cs` | modified r2: C1 "must match" comment on `MustReadSize`; C2 `ShrinkToFit` shared by `FitSize` and `HudSize` |
| `src/wwwroot/js/effects.js` | modified r2: C1 named `MUST_READ_SIZE = 18` with "must match" comment, used in `addLabel` |
| `src/wwwroot/js/game.js` | modified r2: C3 `gameCanvas` declared after `viewHeight`, cleared in `stop()` |
| `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` | modified r2: C4 `CrowdedHudEngine()` helper used by `PhoneScenes` and the fit-cap test |
| `docs/ux-guidelines.md` | modified r2: U1 UX14 says 18 px and larger for floating labels, as the code does |
| `src/core/Rendering/Scene.cs` | unchanged in r2 (listed for the commit set, round 1 change) |
| `src/Pages/Index.razor` | unchanged in r2 (listed for the commit set, round 1 change) |

### Findings addressed
- C1: fixed. `MUST_READ_SIZE` in effects.js and `SceneBuilder.MustReadSize` now reference each other.
- C2: fixed. Both callers use `ShrinkToFit(size, designSize, width, space)`. Same arithmetic as before: `width > space ? max(designSize, floor(size × space / width)) : size`.
- C3: fixed. `gameCanvas` sits after `viewHeight`; `stop()` sets it to null. `frame()` already returns early when `dotnet` is null, so the scale fallback of 1 is not reached in practice.
- C4: fixed. `CrowdedHudEngine()` replaces the two copies (`PhoneScenes`, `Phone_FitCap_ShrinksLongLinesToTheCanvas`).
- U1: fixed. UX14 now reads "floating labels of 18 px and larger".

### Deviations from plan
- none in round 2. Round 1 deviations still apply (step 7 phone emulation not done).

### Commands run
- `dotnet build src/BlazorInvaders.csproj` -> 0 errors
- `dotnet test tests/BlazorInvaders.Tests` -> 332 passed
- `dotnet format whitespace <project> --verify-no-changes --include <file>` for SceneBuilder.cs and SceneBuilderTests.cs -> clean
- `node --check` on game.js and effects.js -> ok
- Prettier: not installed in the worktree (no node_modules), so no JS or Markdown format check was run.

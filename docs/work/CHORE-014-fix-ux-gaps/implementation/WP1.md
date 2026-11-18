# Implementation: CHORE-014 Fix the five UX gaps — WP1

## Round 1
**Plan steps covered:** WP1 steps 1, 2, 3, 4, 5, 6, 7, 8
**Addresses findings:** n/a
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/Pages/Leaderboard.razor` | modified: `_loadFailed` state, failure message with `Try again`, catch logs to console (UX24) |
| `src/wwwroot/css/app.css` | modified: `outline: none` removed from `.game-canvas` and `.bm-content .initials`; `:focus-visible` and `:focus` outlines added (UX32) |
| `src/Components/SetHighScore.razor` | modified: label id, `role="group"` on the input row, `aria-label` on each input (UX33) |
| `src/wwwroot/index.html` | modified: viewport without `maximum-scale` and `user-scalable` (UX38) |
| `src/core/Rendering/SceneBuilder.cs` | modified: pause text is "Tap II to resume" on touch (UX43) |
| `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` | modified: `PausedScene_InstructionMatchesTheControls` theory (AC5) |
| `docs/ux-guidelines.md` | modified: UX24, UX32, UX33, UX38, UX43 rows updated; Known gaps emptied; revision line; last updated |

### Deviations from plan
None.

### Findings with no change, and disputed findings
n/a

### Minor findings not fixed
n/a

### Commands run
- `dotnet build src/BlazorInvaders.sln` → 0 warnings, 0 errors
- `dotnet test tests/BlazorInvaders.Tests` → passed 263, failed 0
- `dotnet format src/BlazorInvaders.sln --verify-no-changes --include` (SceneBuilder.cs, Leaderboard.razor, SetHighScore.razor) → no changes
- `dotnet format tests/BlazorInvaders.Tests --verify-no-changes --include tests/BlazorInvaders.Tests/SceneBuilderTests.cs` → no changes
- No Prettier or other formatter config exists in the repo for `.css`, `.html` or `.md`, so none was run on them.

## Round 2
**Plan steps covered:** n/a (round 2 fixes only)
**Addresses findings:** D2 (user decision "Hide on touch play"), U1, U2, C1
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/wwwroot/css/app.css` | modified: `.game-canvas:focus-visible` ring hidden inside the existing `(pointer: coarse)` block, the same query that shows the touch pads (D2) |
| `src/Pages/Leaderboard.razor` | modified: `Try again` has `mt-2` (U2); retry moves focus to the selected tab, since the button leaves the page while loading (U1); tab buttons get `@ref`; request counter `_request` replaces the tab check in `ShowAsync`, so only the newest load applies (C1) |
| `docs/ux-guidelines.md` | modified: UX32 notes that touch screens show no playfield ring (D2, the guideline departure is recorded here) |

### Deviations from plan
- C1 is fixed with a request counter, as the reviewer suggested, instead of the tab check. A tab switch also bumps the counter, so a stale response from the old tab is still ignored.
- U1 is fixed by focusing the selected tab after the retry, not by keeping the button mounted.

### Findings with no change, and disputed findings
n/a

### Minor findings not fixed
n/a

### Notes
- The ring is hidden by the same `(pointer: coarse)` query the touch pads use (app.css), which is also the `matchMedia` test in `game.js` `start`. A desktop with a fresh load still gets the script-focus ring until the first mouse click (Chromium's `:focus-visible` heuristic). D2 covers touch only, so that is unchanged; the Tester's AC2 check should record it.

### Commands run
- `dotnet build src/BlazorInvaders.sln` → 0 warnings, 0 errors
- `dotnet test tests/BlazorInvaders.Tests` → passed 263, failed 0
- `dotnet format src/BlazorInvaders.sln --verify-no-changes --include src/Pages/Leaderboard.razor` → no changes
- No Prettier config for `.css`, `.md` or `.razor` in the repo, so no formatter was run on them.

## Round 3
**Plan steps covered:** n/a (round 3 fixes only)
**Addresses findings:** C2, U3 (same cause as C2), Q3, C3 (nit, not changed)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/Pages/Leaderboard.razor` | modified: `@implements IDisposable`, `_disposed` flag set in `Dispose`; `RetryAsync` moves focus to the selected tab only `if (!_disposed)`, as `Index.razor` does (C2, U3) |
| `docs/ux-guidelines.md` | modified: UX32 cites "CHORE-014 D2" for the touch-ring departure, not CHORE-005 D2 (Q3) |

### Findings addressed
- C2 and U3: the retry's focus call is skipped once the user has left the page. The element reference is gone then, and `FocusAsync` would throw. Fixed with the guard pattern from `Index.razor:257` (`IDisposable` rather than `IAsyncDisposable`, since nothing is awaited here).
- Q3: citation corrected in UX32 text and Source column.
- C3 (nit, no change): focusing `Try again` after a failed retry would need a ref to a button that is not rendered yet while the load runs, since render happens after the handler returns. Kept focus on the selected tab. Small, but not worth a second ref for a nit.

### Deviations from plan
None.

### Commands run
- `dotnet build src/BlazorInvaders.sln` → 0 warnings, 0 errors
- `dotnet test tests/BlazorInvaders.Tests --nologo -v q` → passed 263, failed 0
- `dotnet format src/BlazorInvaders.sln --verify-no-changes --include src/Pages/Leaderboard.razor` → exit 0, no changes
- No Prettier config for `.md` or `.razor` in the repo, so no formatter was run on `docs/ux-guidelines.md`.

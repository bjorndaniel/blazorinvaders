# Plan: CHORE-014 Fix the five UX gaps

| | |
|---|---|
| **ID** | CHORE-014 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-18 |
| **Size** | S — seven files but about 60 changed lines, each a small edit that follows an existing pattern; no new dependency |
| **Security-sensitive** | no — no new input, no auth, no dependency; the leaderboard error shows fixed text only |
| **UI-facing** | yes — leaderboard error state, focus rings, input names, zoom, pause overlay text |
| **Opened** | 2026-11-17 |
| **Plan round** | 2 |

## Summary
Five small fixes, one per gap in the UX guidelines' Known gaps section, then those gaps are removed from the guidelines.
- UX24: `Leaderboard.razor` tracks a failed load on its own and shows "Could not load the leaderboard. Check your connection and try again." with a `Try again` button (`btn btn-secondary`, like `Back to game`). An empty board still says "No scores yet. Be the first!". The silent catch now logs to the console (CG20).
- UX32: white focus outline on the playfield (`:focus-visible`, so a mouse click shows none) and a black one on the initials inputs, replacing `outline: none`.
- UX33: `aria-label="Initial 1 of 3"` (2, 3) on the inputs, which form a group named by the visible "Enter your initials" label.
- UX38: `maximum-scale=1.0, user-scalable=no` comes out of the viewport meta. The playfield and touch buttons keep their `touch-action: none`, so pinch zoom works everywhere else. Choice: the playfield blocks zoom on the attract screen too, because a tap there starts a game.
- UX43: on touch screens the pause overlay says "Tap II to resume", like "Tap D to play".
- AC1-AC4 are Razor, CSS and HTML. A5 rules out component tests, so the Tester checks them by hand. AC5 gets a unit test.
- One package, WP1. Security: nothing new crosses a trust boundary. UX: follows UX24, UX32-UX34, UX38, UX43, UX44, and changes nothing else visually.

## Context found
- `docs/ux-guidelines.md`: UX24, UX32, UX33, UX38 and UX43 (the targets), UX34 (3:1 for control boundaries), UX44 (errors say what to do, no exception text), UX21 (button labels), UX13 (no new inline `style=`). The Known gaps section (lines 79-87) and D2 (CHORE-005): fix all of them in one item, and block zoom only during play.
- `docs/architecture.md`: A3 (Bootstrap, Blazored.Modal), A5 (xUnit against `src/core` only; no component or end-to-end tests, CHORE-003 D2), A10/A12 (canvas text lives in `SceneBuilder`), A23 (the client degrades gracefully when the API fails).
- `docs/coding-guidelines.md`: CG20 (no silent catch; the current `catch` in `Leaderboard.razor` `ShowAsync` breaks it), CG21 (a catch-all is fine in a Razor handler), CG24/CG26 (`Console.WriteLine` with `ex.Message` for expected offline cases), CG6 (access modifiers in `@code`), CG50/CG52 (core change needs a test; `[Theory]` for variations), CG54 (use `TestPlay.cs` helpers), CG72. Also `src/.editorconfig`.
- Code:
  - `src/Pages/Leaderboard.razor` `ShowAsync`: the catch sets `_scores = new()`, so a failure looks like an empty board. This is the UX24 root cause.
  - `src/Pages/Index.razor` `LoadLeaderboardAsync`: the logging pattern to copy ("Leaderboard unavailable: ...").
  - `src/wwwroot/css/app.css`: `.game-canvas { outline: none; touch-action: none }` (line 218) and `.bm-content .initials { outline: none }` (line 342). `.touch-btn` has `touch-action: none`.
  - `src/Components/SetHighScore.razor`: three `.initials` inputs under one unassociated `<label>`.
  - `src/wwwroot/index.html`: the viewport meta.
  - `src/core/Rendering/SceneBuilder.cs:124`: the pause text. The `TouchControls ? "Tap D to play" : "Press D to play"` pattern is at line 336, and the touch attract card says "II to pause". While paused, a tap on the playfield does nothing: `Index.razor` `HandlePresses` ignores Start during a game. Only the II button resumes, which is why the text says "Tap II".
  - `tests/BlazorInvaders.Tests/SceneBuilderTests.cs`: `PausedScene_ShowsPauseOverlay` (line 86) and `AttractInstructions_MatchTheControls` (`[Theory]`, line 228), the patterns to follow.
- Blazored.Modal 7.3.1's CSS sets no `touch-action`, so the dialog can be zoomed once the meta allows it.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | All five fixes and the guidelines update | S | `src/Pages/Leaderboard.razor`, `src/Components/SetHighScore.razor`, `src/wwwroot/css/app.css`, `src/wwwroot/index.html`, `src/core/Rendering/SceneBuilder.cs`, `tests/BlazorInvaders.Tests/SceneBuilderTests.cs`, `docs/ux-guidelines.md` | — |

## Steps
### WP1
1. **UX24 load failure**, in `src/Pages/Leaderboard.razor`:
   - Add `private bool _loadFailed;`. `ShowAsync` clears it with `_scores`.
   - The catch becomes `catch (Exception ex)` and logs `Console.WriteLine($"Leaderboard unavailable: {ex.Message}")`. Then, only if `_daily == daily`, it sets `_loadFailed = true` and leaves `_scores` null.
   - Markup order: if `_loadFailed`, show `<p role="alert">Could not load the leaderboard. Check your connection and try again.</p>` and a `<button type="button" class="btn btn-secondary">Try again</button>` (the same style as `Back to game`, line 61; UX21) that calls `ShowAsync(_daily)`. Otherwise keep the existing Loading, empty and table branches.
   - Plain white text, not `text-danger`, which falls below 4.5:1 on the gradient (UX34). A response of `null` still counts as empty (`?? new()`), and a non-success status throws, so it counts as a failure.
2. **UX32 focus**, in `src/wwwroot/css/app.css`:
   - Remove `outline: none` from `.game-canvas` and add `.game-canvas:focus-visible { outline: 2px solid #fff; outline-offset: 2px; }`.
   - Remove `outline: none` from `.bm-content .initials` and add `.bm-content .initials:focus { outline: 2px solid #000; outline-offset: 2px; }`. Text inputs match focus-visible on every focus anyway, so `:focus` is simpler.
   - Neither outline takes layout space. White on the `#111`-`#555` gradient and black on the white dialog both pass 3:1.
3. **UX33 names**, in `src/Components/SetHighScore.razor`: give the `<label>` `id="initials-label"`, put `role="group" aria-labelledby="initials-label"` on the inputs' `row` div, and give each input `aria-label="Initial 1 of 3"`, `"Initial 2 of 3"` or `"Initial 3 of 3"`. Nothing visible changes.
4. **UX38 zoom**, in `src/wwwroot/index.html`: the viewport becomes `width=device-width, initial-scale=1.0`. Do not change `touch-action` in `app.css`: it already blocks pinch and double-tap zoom on `.game-canvas` and `.touch-btn`.
5. **UX43 pause text**, in `src/core/Rendering/SceneBuilder.cs` line 124: `TouchControls ? "Tap II to resume" : "Press P to resume"`. No public member changes, so no XML summary changes (CG60).
6. **Test**, in `tests/BlazorInvaders.Tests/SceneBuilderTests.cs`: a `[Theory]` `PausedScene_InstructionMatchesTheControls(bool touch, string expected)`. It takes (false, "Press P to resume") and (true, "Tap II to resume"), and also asserts that the other variant is absent. Build it like `PausedScene_ShowsPauseOverlay`, with `NewEngine()` from `TestPlay.cs`.
7. **Guidelines**, in `docs/ux-guidelines.md`:
   - Delete " See Known gaps." from UX32, UX33, UX38 and UX43.
   - In UX24, replace "If a board cannot be loaded, the page says so and does not look empty. See Known gaps." with: If a board cannot be loaded, it shows "Could not load the leaderboard. Check your connection and try again." and a `Try again` button (`btn-secondary`) that reloads the selected board; it never shows the empty text.
   - Replace the Known gaps intro and its five bullets with "None open. CHORE-014 fixed the five gaps D2 listed."
   - Keep the "Not built yet ... (UX29) ... (UX14)" line.
   - Add a `## Revisions` line "2026-11-17 CHORE-014: fixed the five known gaps (UX24, UX32, UX33, UX38, UX43)", and set **Last updated** to 2026-11-17, by CHORE-014.
8. **Verify**: `cd .work/CHORE-014-fix-ux-gaps && dotnet build src/BlazorInvaders.sln 2>&1 | tail -n 5` with no new warnings (CG70), then `dotnet test tests/BlazorInvaders.Tests 2>&1 | tail -n 5`.

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC1 | Manual: run the client with the API stopped (or DevTools offline), open `/leaderboard`. Both tabs show the "Could not load" text and `Try again`, never "No scores yet". With the API running and an empty board, the page shows "No scores yet. Be the first!". `Try again` reloads once the API is back. | manual (A5: no component tests) | WP1 |
| AC2 | Manual: on a fresh load (desktop, and DevTools touch emulation), record whether the ring shows before any interaction, since `Index.razor:100` focuses the playfield by script. Tab to the playfield and see the white outline. A mouse click shows no ring. Each initials input shows a black outline when focused (`/?wave=5` in development reaches a qualifying score quickly). | manual | WP1 |
| AC3 | Manual: in DevTools Accessibility, each initials input's computed name is "Initial n of 3", inside a group named "Enter your initials". | manual | WP1 |
| AC4 | Manual, with a touch device or DevTools touch emulation: pinch zoom works on `/leaderboard` and on the open initials dialog. Pinching or double-tapping on the playfield and the touch buttons does not zoom. The viewport meta has no `maximum-scale` or `user-scalable`. | manual | WP1 |
| AC5 | `SceneBuilderTests.PausedScene_InstructionMatchesTheControls`, with touch=true giving "Tap II to resume" and no "Press P", and touch=false the reverse | unit | WP1 |
| AC6 | `docs/ux-guidelines.md` Known gaps has no UX24/32/33/38/43 entries. `dotnet test` passes. | manual + unit | WP1 |

No real dependencies are needed, so no containers.

## Security considerations
- The change crosses no new trust boundary. TB3 (`GET /api/getleaderboard`) is used as before, and the failure branch shows fixed text without the exception message (T9, CG24, UX44). `ex.Message` goes to the browser console only, as `Index.razor` already does.
- TB1: `index.html` changes. The published service worker's asset manifest and integrity hashes are regenerated at build, so T16 is unchanged.
- No inputs, auth, secrets, personal data or dependencies change. The `security-model.md` needs no update.

## UX considerations
- The leaderboard gets a new error state: the message plus `Try again` (`btn-secondary` like `Back to game`, verb label, UX21), next to its cause (UX20, UX24, UX44). Loading, empty and success stay as they are. `role="alert"` announces the error.
- Focus: visible outlines meeting 3:1 (UX32, UX34), on keyboard focus for the playfield and on every focus for the inputs. Focus order and the dialog's focus return (UX23) are unchanged.
- Names: UX33. Zoom: UX38 and WCAG 1.4.4 for the leaderboard and dialog. Pause copy: UX43 and UX41 (sentence case), with "II" matching the touch button and the "II to pause" card.
- No other visual change: no layout, colour or copy changes beyond these.

## Risks and rollback
- With page zoom allowed, a player who zooms the leaderboard and goes back to the game sees a zoomed page. They can pinch out anywhere outside the playfield and pads. On a landscape phone, the hidden title leaves little space for that, but the gaps around the pads remain.
- `Index.razor:100` focuses the playfield on load. Chromium may match `:focus-visible` for script focus before any pointer input, so the white ring could show on a fresh load, and on touch stay all game (the touch buttons `preventDefault` on pointerdown, `game.js:166`). The AC2 check records this for the UX reviewer; no JS is added unless it shows a real problem.
- Mobile browsers differ on `touch-action` for multi-finger gestures that start on two elements. The Tester checks Chrome Android emulation. iOS Safari already ignores `user-scalable=no`, so for iOS nothing changes.
- Rollback: revert the commit. Each fix is independent, so reverting one file undoes one fix.

## Out of scope
- The mute control (UX29) and larger phone text (UX14), each its own later item (D3).
- Other instruction copy, and the inline `style=` cleanup (UX13).
- Toggling `touch-action` by game state.

## Revisions
- Round 1: first version.
- Round 2: P1 `Try again` is `btn btn-secondary` (Summary, step 1, UX considerations). P2 AC2 check covers a fresh load; risk added. P3 step 7 rewrites UX24 with the failed state and `Try again`.

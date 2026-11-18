VERDICT: CHANGES_REQUESTED

# Plan review: CHORE-014 Fix the five UX gaps, round 1

**Reviewed:** plan
**Reviewer:** Crab (Plan Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | major | Step 1 (markup), UX considerations | The `Try again` button is `btn btn-outline-light btn-sm`. UX21 (`docs/ux-guidelines.md:39`) says the primary action is `btn-primary` and the secondary one is `btn-secondary`. `btn-outline-light` is only used for unselected tabs (`Leaderboard.razor:17,19`), and no button anywhere uses `btn-sm`. The plan says this follows UX21, but it adds a third button style and a new size without declaring it. | Use `btn btn-secondary`, the same as `Back to game`, or `btn-primary` if Try again is the page's main action. Drop `btn-sm`. Alternatively, declare the departure and add a step that updates UX21. |
| P2 | minor | Step 2, Tests AC2 | `Index.razor:100` calls `_container.FocusAsync()` on load, before the user has done anything. In Chromium, focus moved by script with no earlier pointer interaction can match `:focus-visible`. If it does, the white ring shows on a fresh load. On touch it then stays for the whole game, because the touch buttons `preventDefault` on pointerdown (`game.js:166`) and tapping an element that already has focus does not re-check it. The AC2 test only checks a mouse click after load. | AC2's manual check should cover a fresh load (desktop, and touch emulation) and record whether the ring shows before the first interaction. Add this to Risks so the UX reviewer can judge it. Do not add JS to fix it unless the check shows a real problem. |
| P3 | minor | Step 7 | UX24 lists each leaderboard state with its exact text ("Loading...", "No scores yet. Be the first!"). The new failed state and its `Try again` retry are a new UI pattern. Removing "See Known gaps." alone leaves them undocumented. | In step 7, rewrite UX24's failure sentence to give the message and the `Try again` button. |

## Attacks the plan survived
- Tab race: a slow failing all-time request finishes after the user switches to daily. `ShowAsync` clears `_loadFailed`, and the catch is guarded by `_daily == daily`, so the failure cannot appear on the wrong tab.
- Markup order: the catch leaves `_scores` null, so the `_loadFailed` branch must come before `Loading...`. The plan puts it first.
- Non-success status, timeout and bad JSON all throw from `GetFromJsonAsync`, so they show the failed state. A `null` body still counts as empty.
- AC4: `<CascadingBlazoredModal>` wraps the router in `App.razor`, so the dialog is not inside `.game-canvas`. Its `touch-action` is not combined with the canvas's `none`, and pinch on the dialog zooms. Only `.game-canvas` and `.touch-btn` set `touch-action: none` (`app.css:219,241`), so the leaderboard is zoomable.
- AC5 root cause: `HandlePresses` (`Index.razor:164-175`) returns early while a game is running, so only the II button (`data-press="16"`) or P resumes. "Tap II to resume" is true, and it matches the "II to pause" card (`SceneBuilder.cs:328`).
- Test grounding: `NewEngine()`, `Texts()`, `PausedScene_ShowsPauseOverlay` (line 86) and the `[Theory]` at line 228 exist, and `engine.Start(0); engine.TogglePause();` produces the paused scene.
- CG20 itself requires the next change to `Leaderboard.razor` to log the failure and show a failed state. Step 1 does both, using `Console.WriteLine` as CG26 and `Index.razor` do.
- Outline clipping: `outline-offset: 2px` on a `width: 100%` playfield in portrait stays inside the column's 15 px padding.
- Size S with 7 files: the table's ~3-file limit is exceeded. But 2 of the files are a test and the docs, and each code edit is one or two lines following an existing pattern. Stronger models would not help, so I do not consider S understated.
- Classification: not security-sensitive (fixed text, no new input or dependency). TB1/T16 are handled correctly. UI-facing is correct.

## Checked and fine
- The Bootstrap 4.3.1 and Blazored.Modal 7.3.1 versions match the plan.
- `role="group"` with `aria-labelledby` on the inputs' row and a separate `aria-label` per input meets AC3. Nothing visible changes.
- AC6: step 7 empties Known gaps, keeps the UX29/UX14 line, and updates Last updated and Revisions.
- Step 8 commands are valid for this layout.
- The plan is about 11 KB against a 4 KB budget for S. That is over budget but readable, and it contains no code. Not raised.

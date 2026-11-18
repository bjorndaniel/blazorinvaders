VERDICT: APPROVED

# Plan review: FEAT-001 Mute control, round 2

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| P1 | resolved | Steps 4, 7 and 8 and the Tests section now cover it. I checked the figures against `src/wwwroot/css/app.css:228-304` and `src/Pages/Index.razor:42-45`. **Portrait:** move pad 140 + gap 12 + Fire 84 + gap 12 + grid 96 (44+8+44) = 344 px. That fits the 359 px content box of a 375 px phone. **Landscape:** the column is 192 px tall (196 today) and 96 px wide. With the move pad that gives 140 + 96 + 2×12 = 260 px, under the 280 px the `.game-canvas` rule reserves. `.touch-daily` has no CSS or JS hooks, so wrapping the button in a new `div` loses nothing. The plan adds UX51 updates and manual checks at 375×667 and 667×375. |
| P2 | resolved | In step 6, `setMuted` never calls `getCtx` and touches the gain only when the value changes. `getCtx` creates the gain at the current state. |
| P3 | resolved | `SceneBuilder.Sound` is now the single copy, and the page has no `_sound` field (steps 3 and 4). |
| P4 | resolved | Step 3 now has the right figures: bar at x 285-585, label at x 225. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P5 | nit | whole plan | The plan grew to about 13 KB, against roughly 10 KB for M. | None required. The implementer can skip the duplicated width arithmetic in the Summary. |

## Attacks the plan survived
- **Grid alignment:** `.touch-actions` inherits `align-items: center` from `.touch-pad`. In a portrait row the 96 px grid and the 84 px Fire centre on each other, and in a landscape column they centre horizontally. Nothing overflows.
- **Order in portrait:** `row-reverse` with children in the order Fire, grid puts Fire on the right as before, and the grid sits between the move pad and Fire.
- **Desktop:** `.touch-pad` is still `display: none` outside `(pointer: coarse)`, so the new grid has no effect there.
- **Nesting and JS:** `listen` binds `[data-press]` on descendants, so the nested buttons still work. The `active` class is unaffected.
- **March tempo:** `marchStep++` (`game.js:358-359`) runs after `playTone` even when muting makes it return early. Tempo is kept as step 6 claims.
- **Muted page:** a page loaded muted never creates an `AudioContext` (step 6). On unmute the first sound creates it after a user gesture, as it does today.
- **Narrower phones:** at 320 px the portrait row already wraps today (348 px needed). The change makes the row 4 px narrower, so nothing gets worse.
- **Parts unchanged since round 1:** I found no blocker in them.

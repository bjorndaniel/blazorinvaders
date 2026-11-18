VERDICT: CHANGES_REQUESTED

# Plan review: FEAT-001 Mute control, round 1

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | major | Steps 4 and 7; `src/wwwroot/css/app.css:228-304` | Built as written, the change breaks the current portrait touch layout on most phones. In portrait, `.game-area` wraps, and the move pad and `.touch-actions` share one row. The move pad is 64+12+64 = 140 px, the game-area gap is 12 px, and the actions are 84+44+44 plus 2×12 gaps = 196 px, so 348 px in total. The content box is 359 px on a 375 px phone: the viewport minus `.cover-container` padding of 0.5rem each side. Bootstrap 4's row margin and col padding cancel out. A fourth 44 px button with its 12 px gap makes 404 px. That wraps on 375, 390 and 414 px phones, so FIRE drops onto its own row below the move pad. This breaks the thumb layout UX51 describes. The landscape column also grows to 252 px, which the plan never checks against short landscape heights. | Add a layout step. For example, put the three small buttons in their own wrapper inside `.touch-actions`, as a column in portrait or a 2×2 grid with FIRE. `listen` uses `querySelectorAll` on descendants, so nesting needs no JS change. Alternatively, reduce the gaps so that move pad + actions ≤ 359 px. Add a manual Tester check at 375×667 portrait and about 667×375 landscape. Update UX51 and UX37 if the arrangement they describe changes. |
| P2 | minor | Step 6 | `playSounds` calls `audio.setMuted(muted)` every frame from the first frame. If `setMuted` reaches the master gain through `getCtx()`, an unmuted page creates its `AudioContext` on page load, before any user gesture. Today the context is created on the first sound. Under autoplay policies a context created that early can stay suspended, and the code never calls `resume()`. Setting the gain at `currentTime` each frame also adds an automation event on every frame. | State that `setMuted` only stores the flag and touches the gain when a context already exists and the value changed. `getCtx` creates the master gain at the current state. |
| P3 | nit | Step 4 | The mute state is kept twice, in `_sound.Muted` and `_scenes.Muted`, and kept in sync by hand. | Optionally let `SceneBuilder` hold the `SoundSetting`, or derive one copy from the other. |
| P4 | nit | Step 3 | "boss bar (ends x 735, y 36-44)" is wrong. The bar spans x 285-585 and the "BOSS" label sits at x 225 (`SceneBuilder.cs` `BossFrame`). The conclusion of no overlap still holds. | Correct the figure or drop it. |

## Attacks the plan survived
- Mute pressed while paused, at game over or on the attract screen: it is handled after `HandlePresses` in `Tick`, so it works in every state. With the dialog open, input is zeroed, which is acceptable.
- Key and button in the same frame: `pressed` ORs into one bit, so there is one toggle. `!e.repeat` stops auto-repeat from toggling again.
- Muting mid-`playerDeath` or `bossDefeated`: the `setTimeout` tones go through `playTone`, which returns early while muted. Tones already sounding are cut by the master gain, so the change takes effect at once.
- March tempo across mute: `marchStep++` runs even when `playTone` returns early, so the notes stay in step on unmute.
- Ctrl+M / Cmd+M: the modifier guard and `preventDefault` only when unmodified keep browser shortcuts working (UX31).
- `aria-pressed` rendering: the plan uses the string form, because a bool would render the attribute as present or absent. Re-rendering does not drop the JS `active` class, because Blazor diffs only attributes that changed.
- Reload: the stored value is read before `start`, and the `StateHasChanged()` already at `Index.razor:101` renders `aria-pressed`. Tampered values fall back to sound on.
- `Scene.Muted` reaches JS as `muted`: there is no source-generated JSON context (no `JsonSerializable` in `src`), and `paused` already arrives in camelCase.
- HUD position (x 750, y 45, 14 px): no overlap with the combo (y 22), FRENZY (x 10), "You were hit!" (centred), the mothership (y 80), the boss (y 90) or PERFECT (y 80). It is drawn after the pause overlay. `#888` on black meets UX34.
- The existing `AttractInstructions_MatchTheControls` test still passes, because it asserts the start line, not the pause hint.
- Alternative of `AudioContext.suspend()`/`resume()`: no simpler, because it is promise-based and would also stall the march timer's tones. The gain node is fine.

## Checked and fine
- Grounding: `PRESS_KEYS`/`data-press`, `getItem`/`setItem`, `Scene.Paused`, `SceneBuilder.TouchControls` and the `SceneBuilderTests` patterns all exist as described.
- CG13 must-match comments are covered on both sides. CG17 applies to the new file only, CG60 summaries are planned, and CG12 is not touched (layout literals, not gameplay numbers).
- The security considerations name TB6, T14 and AC1 correctly, and step 8 updates TB6. Security-sensitive `no` is right.
- Size M, UI-facing yes, and one WP that covers every touched file.
- AC1-AC5 each have steps and a test. The JS and Razor wiring is manual-only, which is consistent with A5.
- The README controls line at `README.md:20` exists for the CG63 update.
- The plan is about 10.8 KB, roughly the M budget.

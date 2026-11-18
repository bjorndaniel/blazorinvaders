# Plan: FEAT-001 Mute control

| | |
|---|---|
| **ID** | FEAT-001 |
| **Type** | FEAT |
| **Status** | implemented 2026-11-18 |
| **Size** | M — about 10 files (core type, scene, page, JS, CSS, tests, three docs), ~150 lines, existing patterns only |
| **Security-sensitive** | no — one new boolean `localStorage` value on the player's own device; no input crosses to the API |
| **UI-facing** | yes — new touch button, new HUD text, new key, changed attract-screen hint |
| **Opened** | 2026-11-17 |
| **Plan round** | 2 |

## Summary
The mute state lives in C# (a small core type, `SoundSetting`, unit-tested) and travels to JS in the `Scene` like `Paused` does (A12). `SceneBuilder` holds the one `SoundSetting`. The M key and a new touch button send a one-shot press bit, as P and the D button do; `Index.razor` toggles the setting, saves it with the existing `getItem`/`setItem` and re-renders the button's `aria-pressed`. In `game.js` every sound goes through one master gain node that drops to 0 while muted, so tones already playing stop at once, and the march keeps its timer silently so it resumes in tempo on unmute.
- Key: M (ignored with Ctrl, Alt or Meta held). Button: letter "M", `aria-label="Mute"`, last of the small touch buttons.
- Touch layout: the three small buttons (II, D, M) move into a 2×2 grid beside Fire, so the portrait row still fits a 375 px phone (344 of 359 px) and the landscape column gets no taller.
- On-screen state: "SOUND OFF" in `#888`, 14 px, top right under the combo line, on every screen while muted. Attract control hints gain "M toggles sound".
- Stored as `muted` = `"true"`; anything else, missing included, means sound on.
- WP1 (single package) builds all of it.
- Security: one more `localStorage` key under TB6/T14; `security-model.md` TB6 lists it.
- UX: follows UX29, UX31, UX33, UX35, UX37, UX43, UX51; updates UX29, UX31, UX33, UX51 and Known gaps.

## Context found
- `docs/ux-guidelines.md`: UX29 (the spec), UX31 (keys only on focused playfield, no modifier combos), UX33 (`aria-label` on touch buttons), UX35 (not colour alone), UX37 (44 px small buttons), UX41 (HUD labels uppercase), UX43 (hints name the input), UX51 (touch pads beside the playfield in landscape, below it in portrait), UX11 (`#888` secondary text), UX14 (14 px minimum), UX22, UX28 (device storage). Known gaps lists the mute control as not built.
- `docs/architecture.md`: A3 (JS owns audio and `localStorage`), A5 (xUnit on core only; no component or JS tests), A10 (pure core), A12 (C# describes, JS draws; "Must match" constants).
- `docs/security-model.md`: TB6 (`localStorage` to client), T14 (client tampering), AC1 (player keeps its own data in `localStorage`).
- Patterns copied: one-shot presses `PRESS_KEYS` / `data-press` in `src/wwwroot/js/game.js` and `PausePressed`/`DailyPressed` in `src/Pages/Index.razor`; `Scene.Paused` (`src/core/Rendering/Scene.cs`) and `SceneBuilder.TouchControls`; stored value read like `personalBest` in `Index.razor` `OnAfterRenderAsync`; tests like `Scene_ReportsPaused` and `AttractInstructions_MatchTheControls` in `tests/BlazorInvaders.Tests/SceneBuilderTests.cs`.
- Coding guidelines: CG1, CG5, CG6, CG10, CG11, CG13 (input bit and `muted` must-match comments both sides), CG15 (sealed), CG17 (file-scoped namespace in the new file), CG33 (`private set`), CG50, CG52, CG60 (XML summaries in core), CG61, CG63 (README feature list), CG70, CG72, CG73; `src/.editorconfig`, root `.editorconfig`.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Mute setting, scene flag and HUD, key and button, audio gain, tests, docs | M | `src/core/GameObjects/SoundSetting.cs` (new), `src/core/Rendering/Scene.cs`, `src/core/Rendering/SceneBuilder.cs`, `src/Pages/Index.razor`, `src/wwwroot/js/game.js`, `src/wwwroot/css/app.css`, `tests/BlazorInvaders.Tests/SoundSettingTests.cs` (new), `tests/BlazorInvaders.Tests/SceneBuilderTests.cs`, `docs/ux-guidelines.md`, `docs/security-model.md`, `README.md` | — |

## Steps
### WP1
1. **`SoundSetting`** — file: `src/core/GameObjects/SoundSetting.cs`. `public sealed class SoundSetting` with `public const string StorageKey = "muted"`, `bool Muted { get; private set; }` (default false), `static SoundSetting FromStored(string? stored)` (muted only for exactly `"true"`, ordinal), `void Toggle()`, `string ToStored()` (`"true"`/`"false"`). XML summaries on all (CG60).
2. **Scene flag** — `Scene.cs`: `public bool Muted { get; init; }` with a summary ("all sound off; game.js silences its master gain"). Reaches JS as `muted` (camelCase, like `paused`).
3. **SceneBuilder** — `SceneBuilder.cs`: `public SoundSetting Sound { get; set; } = new();` (summary, like `TouchControls`), the single copy of the mute state. At the end of `Build`, after the pause overlay so it is not dimmed: if `Sound.Muted`, `Text("SOUND OFF", w - 120, 45, 14, "#888")`. Set `Muted = Sound.Muted` on the returned `Scene`. In `AttractScreen`, change the hint lines to `"II to pause  ·  M toggles sound"` (touch) and `"P / Escape to pause  ·  M toggles sound"` (keyboard). Position checked against the combo text (y 22), the boss bar (x 285-585, label at x 225) and "You were hit!" (centred, y 55).
4. **Page** — `Index.razor`:
   - Add `const int MutePressed = 64;` beside the other press bits (must match `INPUT_MUTE` in game.js). No separate field: the page uses `_scenes.Sound`.
   - In `OnAfterRenderAsync`, before `start`, read `getItem(SoundSetting.StorageKey)` into `_scenes.Sound = SoundSetting.FromStored(...)`.
   - In `Tick`, after `HandlePresses`: if the mute bit is set, `await ToggleMuteAsync()`: `_scenes.Sound.Toggle()`, `setItem(StorageKey, ToStored())`, `StateHasChanged()`. Works in every game state (attract, play, pause, game over); input is still zeroed while the initials dialog is open.
   - Wrap the Pause and Daily buttons in `<div class="touch-small-group">` inside `.touch-actions`, after Fire, and add the mute button last in it: `<button type="button" class="touch-btn touch-small" data-press="64" aria-label="Mute" aria-pressed="@(_scenes.Sound.Muted ? "true" : "false")">M</button>`. `listen` binds `[data-press]` with `querySelectorAll`, so nesting needs no JS change. The string is needed: a bool would render `aria-pressed` as present/absent, not `true`/`false`.
5. **JS input** — `game.js`: `INPUT_MUTE = 64` in the input constants (must-match comment already covers Index.razor). Handle `KeyM` in `onKeyDown` as a press only when `!e.ctrlKey && !e.altKey && !e.metaKey` and `!e.repeat`, with `preventDefault` only then, so browser shortcuts keep working (UX31). The button needs no new wiring: `listen` already binds every `[data-press]`.
6. **JS audio** — `game.js` `audio`: create one master `GainNode` with the `AudioContext` in `getCtx`; `playTone` and `alienDeath` connect to it instead of `c.destination`. `getCtx` creates the master gain at the current state (0 if muted, else 1). Add `setMuted(muted)`: it stores the flag, and only when a context already exists and the value changed sets the master gain's `value` to 0 or 1. It never calls `getCtx`, so the `AudioContext` is still first created by a sound after a user gesture, as today, and no automation event is added per frame. While muted, `playTone` and `alienDeath` return before creating anything, so a muted page never creates an `AudioContext`; `march()` and `stopMarch()` keep managing the interval, so the march resumes in tempo on unmute. In `playSounds`, call `audio.setMuted(muted)` first (destructure `muted` from the scene).
7. **Button style and layout** — `app.css`: `.touch-btn[aria-pressed="true"]` with `text-decoration: line-through` and a dimmer border, so the state is not colour alone (UX35); the 44 px size comes from `.touch-small` (UX37). Add `.touch-small-group { display: grid; grid-template-columns: repeat(2, 44px); gap: 8px; }` (II and D on the first row, M below). Widths, with the portrait `.touch-actions` row-reverse putting Fire on the right: portrait row = move pad 140 + gap 12 + actions (84 + 12 + 96) = 344 px, within the 359 px content box of a 375 px phone (today 348); landscape column = 84 + 12 + 96 = 192 px tall (today 196) and 96 px wide, so move pad + actions + gaps = 260 px, inside the 280 px the `.game-canvas` width rule reserves.
8. **Docs** — `docs/ux-guidelines.md`: UX29 drops "planned, not built yet" and names the M key, the "Mute" button with `aria-pressed` and the "SOUND OFF" HUD text; UX31 lists "M toggles sound"; UX33 adds "Mute" to the `aria-label` examples and notes toggle buttons carry `aria-pressed`; UX51 adds that the small buttons (Pause, Daily, Mute) sit in a 2×2 grid beside Fire; UX37 is unchanged (sizes stay); Known gaps: remove the mute control from the "Not built yet" line; update **Last updated** and **Revisions**. `docs/security-model.md`: TB6 data adds `muted`. `README.md`: the controls line adds "mute with M".

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC4 | `SoundSettingTests.FromStored_Missing_SoundOn` — `null` and `""` give `Muted == false` | unit | WP1 |
| AC4 | `SoundSettingTests.FromStored_Value_ParsesOnlyTrue` (theory) — `"true"` → muted; `"false"`, `"TRUE"`, `"1"`, `"garbage"` → sound on | unit | WP1 |
| AC1, AC2, AC4 | `SoundSettingTests.Toggle_RoundTripsThroughStorage` — toggle from default gives muted and `ToStored() == "true"`; `FromStored` of that is muted; a second toggle stores `"false"` | unit | WP1 |
| AC3 | `SceneBuilderTests.MutedScene_ShowsSoundOffAndReportsMuted` (theory over attract, playing, paused) — `Sound = SoundSetting.FromStored("true")` gives "SOUND OFF" in texts and `Scene.Muted == true` | unit | WP1 |
| AC3 | `SceneBuilderTests.UnmutedScene_HasNoSoundOff` — default builder: no "SOUND OFF", `Scene.Muted == false` | unit | WP1 |
| AC1 | `SceneBuilderTests.AttractInstructions_NameTheMuteKey` (theory, touch and keyboard, as `AttractInstructions_MatchTheControls`) — controls card contains the line with "M toggles sound" | unit | WP1 |
| AC1, AC2 | Manual check by the Tester in a browser: M and the button toggle sound mid-march and mid-`playerDeath`, `aria-pressed` flips, Ctrl+M does nothing, state survives reload; touch layout at 375×667 portrait (move pad, Fire and the small-button grid on one row) and 667×375 landscape (whole column visible, canvas not narrowed) | manual | WP1 |

The key handling, `Index.razor` wiring and Web Audio gain are not unit-tested: A5 has no component or JS tests, and those layers are kept thin. No real dependencies are involved.

## Security considerations
- Crosses TB6 only: a new `localStorage` key `muted` read on load. Parsed by exact match, anything else falls back to sound on, so tampering (T14) can only change the player's own sound. `security-model.md` TB6 gets the new key (step 8); no actor (AC1) or threat changes.
- No API, network, authentication, secrets or personal data involved. No new dependency (Web Audio `GainNode` is a browser built-in, already used).

## UX considerations
- Changes: a new touch button (touch layouts only, UX51), a "SOUND OFF" HUD line on attract, play, pause and game-over screens, the M key, and the attract control hints.
- States: sound on (default, no HUD text, button not pressed) and muted (HUD text, button `aria-pressed="true"` and struck-through letter). No loading or error state: storage failures fall back to sound on and are already swallowed by `getItem`/`setItem`.
- Accessibility: toggle button with a fixed name and `aria-pressed` (UX33, WCAG 4.1.2); 44×44 px (UX37); state shown as text, not colour alone (UX35); `#888` on black is 5.9:1 (UX34); key without modifiers, only when the playfield has focus (UX31).
- Text: "SOUND OFF" (uppercase HUD, UX41), "M toggles sound" (UX42/UX43 style). Follows UX29; no new pattern.

## Risks and rollback
- HUD overlap on a long header or wide combo text: position checked against current layout; the Tester confirms visually at 870×500 and on a phone.
- Touch layout on phones narrower than 375 px: the portrait row already wraps today at 320 px (348 px needed); this change makes it 4 px narrower, not wider.
- Re-rendering the page on toggle must not drop the JS `active` class or listeners on the buttons: Blazor patches only `aria-pressed`, as the existing game-over `StateHasChanged` already does.
- Rollback: revert the item's commit; a stale `muted` key left in `localStorage` is ignored by older code.

## Out of scope
Volume slider, per-sound settings, a desktop mute button, modifier checks for the existing P, Escape and D keys.

## Revisions
- Round 1: first plan.
- Round 2: P1 small buttons in a 2×2 grid, with widths, UX51 update and layout checks (steps 4, 7, 8; Tests). P2 `setMuted` never creates the context and touches the gain only on change (step 6). P3 `SceneBuilder.Sound` is the single copy (steps 3, 4). P4 boss-bar figure corrected (step 3).

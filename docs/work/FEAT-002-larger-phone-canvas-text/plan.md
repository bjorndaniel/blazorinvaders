# Plan: FEAT-002 Larger phone canvas text

| | |
|---|---|
| **ID** | FEAT-002 |
| **Type** | FEAT |
| **Status** | implemented 2026-11-18 |
| **Size** | M — a size rule plus layout fitting across ~25 text calls, one JS argument, tests |
| **Security-sensitive** | no — no input, data, network or dependency change |
| **UI-facing** | yes — canvas text size and layout on touch screens |
| **Opened** | 2026-11-17 |
| **Plan round** | 3 |

## Summary
The canvas is CSS-scaled (about 0.41-0.45 on a 375 px phone), so 18 px text shows at about 7.5 CSS px. `game.js` reports the canvas's CSS-px-per-logical-px scale with each `Tick`; `SceneBuilder` (C#) raises every must-read text (design size 18 px or more, per UX14) to `ceil(12 / scale)` logical px when `TouchControls` is true, then keeps it from overlapping or leaving the canvas.
- Scale is measured live (portrait, landscape, rotation). The rule lives in C# (A12); JS only reports the scale.
- Widths come from an advance table of the bundled Orbitron font, so tests check fit and overlap. At 27-30 px every line fits: no wrapping; a fit cap shrinks text toward its design size only on narrower phones.
- Enlarged HUD: header a few px lower, combo right-aligned, "You were hit!" below the boss bar in boss waves, "PERFECT!" below "You were hit!" when both show; floating labels kept on screen.
- The 14 px labels (daily tag, BOSS, power-up timers, "SOUND OFF") and the 10 px letter stay as they are, per UX14; on touch the M button also shows the mute state (UX29). Floating score labels (`effects.js`) get the same floor through a new `Scene.MinTextSize` field.
- WP1 only. Security: none. UX: UX14 updated to the built behaviour; no new pattern.

## Context found
- `src/core/Rendering/SceneBuilder.cs` (base 840f5a4): `TouchControls` :58, header :89-90, combo :93, PERFECT :121 (y 80, 28 px), pause hint :127 ("Tap II to resume" / "Press P to resume", 20 px), "SOUND OFF" :132 (`w - 120`, 45, 14 px, only when muted), "You were hit!" :205, FRENZY :208, bar constants :264, touch attract hint :336 ("II to pause  ·  M toggles sound", 18 px), `Text`/`CenteredText` :379/:390. `src/core/Rendering/Scene.cs` (`Muted` :31). `src/Pages/Index.razor`: `Tick(float, int)` :126, `_scenes`. `src/wwwroot/js/game.js`: `viewWidth` :29, `Tick` call :106, `effects.draw` :248. `effects.js`: `update` :34, `draw(ctx)` :99.
- `src/wwwroot/css/app.css`: touch canvas width is `min(870px, calc(100vw - 280px), ...)` in landscape and `100%` in portrait, unchanged by FEAT-001's small-button grid; no layout change allowed.
- Tests: `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` (`Decode`, `Texts`, `TextCommands`, `AssertCentred`, `AttractState`, `IdleUntil`, `EngineOfWidth`; `TestPlay.ChaseAndShootBoss`). BUG-001's tests (e.g. `StartScreen_LinesAreCentred_WithUnchangedTextFontAndY`, touch included) pin fonts, y and `x = w / 2` with a default builder, so at `DisplayScale` 1: they must pass unchanged.
- Docs: `docs/architecture.md` (pure core, C# describes/JS draws), `docs/ux-guidelines.md` UX14 (:33), Known gaps (:82), open question "Phone text size" (:85), UX29 ("SOUND OFF" at top right).
- Font metrics (measured from `src/wwwroot/fonts/orbitron-latin.woff2`, 1000 upm, no `^` glyph, `·` falls back): ascender 0.77 em, descender 0.243 em; widest glyph `W` 1.179 em. At 30 px: header `Score: 99999   Hi: 99999 WWW` 551 px, arrow-buttons line 753, daily subtitle 699, `COMBO x8` 181, `COMBO x4!` 185, `BOSS DOWN +5000` 339. The new touch hint (31 characters, `·` at 1 em) is estimated at about 570 px with the margin, well below the arrow line.
- Guidelines: CG12 (named constants: 12 px, margin, width factor), CG13 ("must match" comments on both sides for the new Tick argument and Scene field), CG30, CG60 (XML summaries on new public core members), CG63 (README only if its feature list mentions text size), `.editorconfig`.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Size rule, layout fitting, scale plumbing, tests, UX14 | M | `src/core/Rendering/SceneBuilder.cs`, `src/core/Rendering/Scene.cs`, `src/Pages/Index.razor`, `src/wwwroot/js/game.js`, `src/wwwroot/js/effects.js`, `tests/BlazorInvaders.Tests/SceneBuilderTests.cs`, `docs/ux-guidelines.md` | — |

## Steps
### WP1
1. **Scale plumbing** — `game.js`: keep the canvas in a module variable; in `frame()` pass `canvas.clientWidth / viewWidth` as a third `Tick` argument. `Index.razor`: `Tick(float timeStamp, int input, float scale)` sets `_scenes.DisplayScale = scale`. "Must match" comments on both sides (CG13).
2. **Size rule** — `SceneBuilder.cs`: public `float DisplayScale { get; set; } = 1` (CSS px per logical px). Constants `MinPhoneTextPx = 12`, `MustReadSize = 18`, `MaxPhoneTextSize = 60`. Public static `int PhoneFontSize(int designSize, float scale)`: returns `designSize` if it is below `MustReadSize` or the scale is not finite and positive; else `Math.Min(MaxPhoneTextSize, Math.Max(designSize, (int)Math.Ceiling(MinPhoneTextPx / scale)))`. `Text` applies it only when `TouchControls`, so desktop output stays identical (AC3); at the default scale 1 the floor is 12, so nothing changes either (keeps the BUG-001 tests valid). `Scene.MinTextSize` (int; 0 when not touch, else the floor for 18 px text) set in `Build`. XML summaries (CG60).
3. **Widths and fit** — `SceneBuilder.cs`: a private static advance table for ASCII 32-126 in thousandths of an em, copied from the bundled woff2 (`hmtx`; the Implementer extracts it once, e.g. with fontTools in a throwaway environment, nothing committed but the table); characters not in it count 1000. `internal static float EstimateWidth(string text, int size)` = sum of advances × size / 1000 × `WidthMargin` (1.05). Comment naming the font file it must match (CG13). Fit cap: enlarged text whose width exceeds its space is reduced to `floor(size × space / width)`, never below its design size. Space: `w - 2 × EdgeMargin` (10) for centred text; for the HUD row, header plus combo share `w - 2 × EdgeMargin - HudGap` (20) with one common factor. No wrapping.
4. **Layout when enlarged** (only when the size changed, so desktop is identical) — header baseline `max(22, ceil(size × 0.77) + 4)` (28 at 30 px), giving `delta = baseline - 22`; `COMBO xN` on the same baseline at `x = w - EdgeMargin - EstimateWidth(...)`. "FRENZY!" at `45 + delta`. "You were hit!" at `55 + delta`, or in a boss wave below the bar: `max(55 + delta, BossBarBottom + 4 + ceil(size × 0.77))` (72 at 30 px), with `barY`/`barHeight` lifted to class constants so both places share them. "PERFECT!  +N" stays at 80 unless "You were hit!" shows enlarged (a bomb in flight can hit after the clear, GameEngine.cs:346, :654): then `max(80, hitBaseline + ceil(0.243 × hitSize) + 4 + ceil(0.77 × perfectSize))` (97 at 30 px, 108 in a boss wave; 91 and 102 at 27 px, where PERFECT itself stays 28 px). Put the hit and PERFECT baselines in `internal static` helpers (`HitBaseline(size, bossWave)`, `PerfectBaseline(hitBaseline, hitSize, perfectSize)`) so the combination, which play cannot reach deterministically, is tested directly. "SOUND OFF" (14 px, `w - 120`, 45) keeps its place: its box (about 34..45, x from 750) clears the enlarged combo (ends at 28), "You were hit!" and PERFECT (centred, x below 620). Centred text keeps `x = w / 2` (BUG-001). The other screens (game over, daily banner and its 90 px backdrop, paused with "Tap II to resume", attract cards with the new hint at 265) keep their y offsets: at 30 px their boxes do not touch (checked with the step 3 widths and the font metrics above); the AC2 test guards this.
5. **Floating labels** — `effects.js` `update` also destructures `minTextSize` from the scene (game.js already passes the whole scene); `addLabel` raises labels of 18 px design size and up (combo 20, boss 24) to `minTextSize` when it is above 0; the 14 px and 16 px labels stay. In `draw(ctx, viewWidth)`, when `minTextSize > 0`, clamp each label's x to `[width/2, viewWidth - width/2]` with `ctx.measureText`; `game.js` passes `viewWidth` at its `effects.draw` call. "Must match Scene.cs" comment.
6. **UX14** — `docs/ux-guidelines.md`: replace "The phone part is planned and not built yet" (:33) with the built rule: floor `ceil(12 / scale)` from the measured canvas scale, for text of 18 px and larger and floating labels of 20 px and larger; a line that does not fit the canvas is shrunk toward its design size (no wrapping); the 14 px labels, "SOUND OFF" included, stay. Remove the "Not built yet" line from Known gaps (:82), drop the resolved "Phone text size" open question (:85, 12 CSS px confirmed), and add a Revisions line for FEAT-002.
7. **Manual check** — emulate a 375 px phone with touch, portrait and landscape: every attract card, HUD with combo, a 5-digit Hi score and "SOUND OFF", a boss wave with a lost life, a combo with the player at each screen edge, banners, paused, game over. Text legible, no overlap or cut-off. Record in the implementation notes.

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC1 | `PhoneFontSize` theory: (18, 0.41) -> 30; (18, 0.445) -> 27; (18, 1.0) -> 18; (36, 0.41) -> 36 (never shrinks); (14, 0.41) -> 14; (10, 0.41) -> 10; scale 0, negative, NaN -> design size; (18, 0.01) -> 60 (cap) | unit | WP1 |
| AC1 | Built scene, `TouchControls` true, scale 0.41 and 0.445: every OP_TEXT of design size 18 or more has font >= `ceil(12 / scale)`; `Scene.MinTextSize` equals the floor | unit | WP1 |
| AC2 | `EstimateWidth` at 30 px: arrow-buttons line in 753..800, `COMBO x4!` in 185..196, `Score: 99999   Hi: 99999 WWW` in 551..580 (anchors the table to the measured font) | unit | WP1 |
| AC2 | Scale 0.41 and 0.445, `TouchControls` true: every attract state (incl. daily card and the mute hint), playing with combo x8, frenzy, muted and Hi `99999 WWW`, lost a life in a boss wave (muted), a daily game (HUD tag), PERFECT alone (via `ChaseAndShootBoss`), game over with daily, paused (muted), daily and wave banners. Each OP_TEXT box (x from `EstimateWidth` and alignment; y from `baseline - 0.77 × size` to `baseline + 0.243 × size` if the text has a descender `gjpqy,;`, else the baseline) lies within the canvas, and no enlarged box intersects another text box or the boss bar rect. The daily subtitle box stays inside its backdrop. Centred lines keep `x = w / 2` | unit | WP1 |
| AC2 | Perfect bonus + lost a life: `HitBaseline`/`PerfectBaseline` (30 px: no boss 61 and 97, boss 72 and 108; hit 27 px, PERFECT 28 px: 58 and 91, 69 and 102); the PERFECT box top is below the hit box bottom; design sizes (20, 28) give 55 and 80 | unit | WP1 |
| AC2 | Fit cap, scale 0.30 (floor 40): arrow-buttons line is shrunk to a size whose width fits `w - 20` and is never below 26; header and combo x8 fit side by side | unit | WP1 |
| AC3 | Same scenes with `TouchControls` false and scale 0.41: `Cmds` and `Strings` equal a default builder's; `MinTextSize` is 0 | unit | WP1 |
| AC4 | The tests above; UX14 text checked in review | unit | WP1 |

No external services: no containers. JS has no test setup; the JS changes are covered by step 7.

## Security considerations
No new input, secret, network access or dependency. The only new data is a float from the page's own JS into `Tick` (same trust level as `timeStamp` and `input`); a non-finite or non-positive value is ignored and the result is capped, so it cannot produce huge fonts. No trust boundary or access model changes; `docs/security-model.md` needs no update.

## UX considerations
Screens: attract cards, playing HUD, wave and daily banners, pause overlay, game over, floating score labels, on touch screens only. States unchanged. Follows UX14 (updated), UX11, UX12, UX22. Desktop unchanged. No new UI pattern.

## Risks and rollback
- Canvas kerning and the fallback font for `·` make real widths differ slightly from the table: the 5% margin and 1 em for unknown characters overestimate; step 7 checks the result.
- Rotation changes the scale: it is measured each frame, so the layout adapts on the next tick.
- Larger text crowds the top of the playfield: the header shift, the right-aligned combo, the moved hit and PERFECT lines and the fit cap are the mitigation; the AC2 overlap test guards it.
- Rollback: revert the commit; no data or API change.

## Out of scope
Page or touch-pad layout, the mute control (UX29), HTML text, the 14 px and 10 px canvas text, desktop sizes.

## Revisions
- r2 P1: single widest-glyph factor replaced by a font advance table with margin and anchor test (step 3); wrapping replaced by a fit cap tested at scale 0.30; header row, combo right-alignment and y offsets re-derived from measured widths (step 4); AC2 tests rewritten.
- r2 P2: floating labels clamped inside the canvas when enlarged (step 5); edge combo added to step 7.
- r2 P3: "You were hit!" placed below the boss bar in a boss wave (step 4); boss wave + lost life and daily HUD tag added to the AC2 test, bar as a box.
- r2 P4: game.js `update` call left as is; only `effects.draw` gets `viewWidth` (step 5).
- r3 rebase to 840f5a4: line refs updated; BUG-001 pinned tests kept valid by the default scale 1 (step 2, Context); "SOUND OFF" stays 14 px and its place is checked (Summary, step 4, AC2 test, step 7); the new touch hint and "Tap II to resume" covered by the layout check and AC2 test; UX14 doc line numbers updated (step 6).
- r3 P5: PERFECT moves below "You were hit!" when both show enlarged, via tested helpers (step 4); perfect + lost-a-life test row added.
- r3 P6: step 6 describes the fit cap, not wrapping; Risks name the header shift, combo alignment and fit cap.

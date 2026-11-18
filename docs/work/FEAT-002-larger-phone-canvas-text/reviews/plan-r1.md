VERDICT: CHANGES_REQUESTED

# Plan review: FEAT-002 Larger phone canvas text, round 1

**Reviewed:** plan
**Reviewer:** Crab (Plan Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | major | Step 3 (`OrbitronWidthFactor` "at or above the widest glyph advance"), Step 4, AC2 tests | I measured the bundled `src/wwwroot/fonts/orbitron-latin.woff2` (upm 1000). Its widest printable glyph is `W` at 1.179 em. Using that factor for every character, the header `Score: 00000   Hi: 12345 ABC` (28+ chars) comes out at about 1000-1060 px at 30 px. That is wider than the 870 px canvas. The header goes through `Text` at x=10, not `CenteredText`, so no step wraps it, and the AC2 test "estimated boxes lie within 0..870" fails as specified. With the plan as written, the implementer has to choose between breaking the plan's factor and leaving the test failing. The factor also over-wraps text that really fits. "Same game for everyone today" (y=260) would wrap into "Tap D to play" (y=300). The daily-banner subtitle would wrap below its 90 px backdrop (`Rect(0, h/2-50, w, 90)`). The steps do not handle either case. Real widths at 30 px: header with `99999 WWW` up to ~600 px, `Use the arrow buttons to move, FIRE to shoot` 753 px, daily subtitle 684 px, `COMBO x4` 178 px. So at the AC sizes (27-30 px), no centred line needs wrapping. | Replace the single widest-glyph factor with a realistic estimate. Option A: a per-character advance table for the ASCII characters used, taken from the bundled font, with a small safety margin. Option B: an average factor that a test checks against the measured worst strings above. Then re-derive step 4 with real widths. The header stays on one row. `COMBO xN` must be right-aligned from its estimated width when enlarged: at the current x = w-170 it runs to 878 > 870, even though header + combo (≈800) is below w, so the "second row" rule would not fire. Fix the AC2 test "arrow-buttons line yields two lines" to match: at 30 px it fits on one line (753 < 850). Either test wrapping with a forced narrow case, or drop wrapping if nothing at the AC sizes needs it. |
| P2 | minor | Step 5, `effects.js` `draw()` | Enlarged floating labels are centred on the event position and nothing keeps them inside the canvas. `ComboUp` is raised at the player centre (`GameEngine.cs:531`, `Raise` centres at :300), and `COMBO x4!` is 185 px wide at 30 px. A combo with the player within ~92 px of either edge cuts the label off, which goes against AC2 for text the plan treats as must-read. `BOSS DOWN +5000` (339 px) has the same problem near an edge. No test or step covers it. | In `effects.js` `draw()`, clamp each label's x to `[w/2, viewWidth - w/2]`, where `w` comes from `ctx.measureText`. Add "combo at the screen edge" to the step 7 manual check. |
| P3 | minor | Step 4, AC2 test scenarios | The AC2 scenario list leaves out lost-a-life, a boss wave and a daily game during play. Enlarged `You were hit!` at y=55 (30 px, x≈327-543) overlaps the boss health bar (`barY=36..44`, x 285-585, `SceneBuilder.BossFrame`). Step 4's "below the occupied rows" does not say that the bar counts as a row. | In step 4, treat the boss bar as an occupied row. Add the LostALife + boss wave case and the daily HUD tag to the AC2 overlap test (bar as a box). |
| P4 | nit | Step 5 | `game.js:185` already passes the whole scene to `effects.update(performance.now(), scene)`, so no change to game.js is needed. Only the destructuring in `effects.js:34` needs `minTextSize`. | Drop the game.js part of step 5. |

## Attacks the plan survived
- Scale values match the CSS. Portrait: `.cover-container` padding 0.5rem gives a 359/870 = 0.413 scale. Landscape on a 667×375 phone: `100vw - 280px` = 387 px, scale 0.445. The test scales 0.41/0.445 are realistic, and measuring live handles rotation.
- `canvas.clientWidth / viewWidth` is in CSS px per logical px whatever the DPR transform (`game.js` start). Pinch zoom (UX38) does not change it. A hidden canvas gives 0, which the plan ignores.
- `PhoneFontSize` test values hold in float arithmetic: 12/0.445f → 26.97 → 27, 12/0.41f → 30, cap 60, never shrinks.
- Desktop (AC3): changes only happen with `TouchControls` true and enlarged sizes. A touch tablet at scale ≥ 0.667 gets no enlargement either.
- `Tick` has exactly one caller (`game.js` `frame()`). Adding a third numeric argument is safe, and `frameBusy` prevents overlapping calls.
- `Scene.MinTextSize` reaches JS camelCased (`scene.minTextSize`), like `cmds`/`strings`. CG13 "must match" comments are planned.
- Treating text of 18 px design size or more as must-read follows UX14 as written. The 14 px HUD labels and the 10 px letter stay as they are, with no conflict.
- The untrusted float into `Tick` is the same origin and trust level as `timeStamp`, and it is bounded. No security-model change is needed, and Security-sensitive: no is correct.
- Classification M / UI-facing yes, one WP covering all 7 files, ~7.5 KB plan: correct and within budget.

## Checked and fine
- `SceneBuilderTests` has `Decode`, `Texts`, `NewEngine`, `Arity`, `RunMs`, and existing tests reach paused, boss wave, daily and attract cycling.
- Architecture A12 (C# describes, JS draws) holds: the rule and layout are in C#, and JS only reports the scale and applies the label floor.
- CG12 constants and CG60 XML summaries are named in step 2.
- The UX14 / Known gaps / open-question updates in step 6 cover AC4.
- Max initials are 3 (`HighScoreValidator.cs:20`), so the header length is bounded.

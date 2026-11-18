VERDICT: APPROVED

# UX review: FEAT-002 Larger phone canvas text, round 1

**Reviewed:** diff vs base_commit 840f5a4 plus untracked files (SceneBuilder.cs, Scene.cs, Index.razor, game.js, effects.js, docs/ux-guidelines.md). I reviewed against UX14, UX11, UX12 and UX22, the existing canvas layout in SceneBuilder.cs and app.css, and .devagents/ux-defaults.md. I did not see the running app: I have no Playwright tools in this session.
**Reviewer:** Pixel (ux-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| U1 | nit | docs/ux-guidelines.md:30 (UX14) | UX14 says floating labels are raised from "20 px and larger". effects.js:166 and the Scene.cs `MinTextSize` summary use 18 px and up. Today they behave the same, because the label sizes are 14, 16, 20 and 24, but the rule is worded differently in two places. | Optional: say "18 px and larger" for both in UX14. |

## Checked and fine
- AC1 sizes. Portrait is 359 CSS px, scale 0.413, so text becomes 30 logical px, which shows at 12.4 CSS px. Landscape uses the coarse-pointer width rule `100vw - 280px`, giving 387 CSS px, scale 0.445 and 27 px. The scale is read from `clientWidth` every frame, so rotating the phone and device pixel ratio are both handled.
- AC2 widths. I re-ran the advance-width table at 30 and 27 px. The widest attract line, "Use the arrow buttons to move, FIRE to shoot", is 791 of 850. The HUD header plus "COMBO x4" is 765 of 830. The daily, game-over, pause and PERFECT lines all fit. The fit cap only matters for lines longer than these.
- AC2 vertical spacing at 30 px:
  - Attract cards: 40-50 px between lines.
  - Game over: lines at 270 and 305, cap tops clear.
  - Daily banner: the second line stays inside its dark rect (200-290).
  - Pause: lines at 240 and 290.
- HUD row on phones. The header baseline moves to 27. "You were hit!" moves to 60, or below the boss bar in a boss wave. FRENZY moves to 50. "SOUND OFF" (14 px, x 750) and FRENZY (x 10) stay clear of the centred hit text and of the right-aligned combo.
- Floating labels. The combo (20) and boss (24) labels are raised only on touch screens and are clamped horizontally so they stay on the canvas. Score labels (14/16) are unchanged, as UX14 says.
- AC3 desktop. `Size()` returns the design size when `TouchControls` is false. `FitSize` leaves unenlarged text alone. The HUD keeps baseline 22 and combo at `w - 170`. `MinTextSize` is 0, so effects.js does nothing new. Large touch screens with scale of 1 or more keep the design sizes, because `ceil(12/scale)` is below 18 there.
- Colours, the Orbitron font and copy are unchanged (UX11, UX12). No new UI pattern, no page or touch-pad layout change (request constraint).
- UX14 now describes the built behaviour. Known gaps and the resolved "Phone text size" question were updated.

## Notes for the PM
- I did not see the app: no Playwright tools were available, so I did not start a preview and there is nothing to stop. My layout checks are computed from the code and the font's advance table. The Tester's headless Chrome run at 375 px (test-report.md) is the only rendered evidence. It did not drive the boss wave, daily banner, game over or pause screens on a phone, and my computed checks of those found no overlap.
- docs/ux-guidelines.md still has the open question "Not seen running". It came before this item and was not part of my review.

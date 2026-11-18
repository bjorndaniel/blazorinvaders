VERDICT: APPROVED

# Plan review: FEAT-002 Larger phone canvas text, round 2

**Reviewed:** plan
**Reviewer:** Crab (Plan Reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| P1 | resolved | Step 3 now uses a per-character advance table with a 1.05 margin and an anchor test. With the margin, the anchor ranges (753→791, 185→194, 551→579) hold. Header plus combo come to about 769 px, below the 830 px shared space, so the HUD stays on one row. Combo is right-aligned at `w - 10 - width`. Wrapping is gone, and the fit cap gets its own test at scale 0.30. |
| P2 | resolved | Step 5 clamps labels in `effects.js` `draw(ctx, viewWidth)` (now `draw(ctx)` at effects.js:99). `viewWidth` already exists in game.js:29, and the call to change is game.js:243. The edge combo is added to step 7. |
| P3 | resolved | Boss-wave "You were hit!" now sits at `max(61, 44 + 4 + 24) = 72`, so its top is about 49, below the bar bottom at 44 (barY 36 + height 8, SceneBuilder.cs:256). Both places share the bar constants, and the AC2 test treats the bar as a box. |
| P4 | resolved | game.js:185 `update` stays as it is. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P5 | minor | Step 4; AC2 test list | "PERFECT! +N" (SceneBuilder.cs:118, y=80, 28→30 px, box about 57..80) keeps its y. The enlarged "You were hit!" (box about 38..68, or 49..79 in a boss wave) overlaps it, and both are centred. The two can show together. `ShowPerfectBonus` lasts 2500 ms into the next wave (GameEngine.cs:346). `BeginWave` resets `_hitThisWave` but leaves bombs in flight (GameEngine.cs:225-247). So a bomb dropped by the last alien that hits after the clear shows both texts. On desktop the overlap is about 2 px; enlarged it is about 11 px, or full overlap in a boss wave. | In step 4, when both show and the size is enlarged, put PERFECT below "You were hit!" (for example, its baseline at hit baseline + descender + 4 + ascent). Add "perfect bonus + lost a life" to the AC2 overlap scenarios. |
| P6 | minor | Step 6; Risks bullet 3 | These are left over from r1. Step 6 tells UX14 to say "long lines wrap", but step 3 now says "No wrapping". UX14 written that way would not describe the built behaviour (AC4). Risks still names "the second HUD row" as the mitigation, and that row no longer exists. | Step 6: describe the fit cap (shrinks toward design size when a line does not fit) instead of wrapping. Risks: name the header shift, combo right-alignment and fit cap instead of the second row. |

## Attacks the plan survived
- Vertical spacing at 30 px on every screen with fixed offsets:
  - Attract: 160/220/265.
  - Daily card: 130/175/220/260/300, smallest gap about 10 px.
  - Game over: h/2+20 descender ends at +27, next top at +32.
  - Paused: h/2-10/+40.
  - Top scores: 45 px rows.
  - Score table: 50 px rows.
  - No overlap.
- Daily banner subtitle at h/2+30 (30 px): the descender ends at about h/2+37, inside the backdrop's bottom at h/2+40.
- Enlarged FRENZY at 51 (16 px, x 10..~100) does not touch "BOSS" (x≈225, y 45), the header bottom (28) or the bar.
- Score-advance labels at x=w/2-40 (395) with 30 px text end well inside 870.
- Fit cap at scale 0.30 (floor 40): the arrow line comes to about 1054 px estimated, gets capped to about 32 px, and stays at or above 26. Header plus combo get one shared factor of about 0.81. Both are reachable and testable.
- The header baseline `max(22, ceil(0.77×size)+4)` gives 28 at 30 px and stays at 22 at design size, so desktop is unchanged (AC3).
- The longest daily modifier, "ONE LIFE, DOUBLE POINTS" at 24→30, and the subtitle (699×1.05 ≈ 734 px) fit the 850 px centred space.
- JS clamp: `measureText` is used after `ctx.font` is set per label. Only x is clamped and labels drift in y only, so a clamped label stays on screen.
- Size M, UI-facing yes, Security no, one WP owning all 7 files: unchanged and still correct.

## Checked and fine
- SceneBuilder.cs line refs:
  - header :86-87
  - combo :90
  - "You were hit!" :197
  - FRENZY :200
  - bar constants :256
  - `Text` :371
- effects.js `draw(ctx)` :99 and `update` destructuring :34. game.js `Tick` call :106, `effects.draw` :243.
- UX14 lines to update: ux-guidelines.md:87 and the open question at :90.
- The brief contained only template fields and the task; nothing to ignore.

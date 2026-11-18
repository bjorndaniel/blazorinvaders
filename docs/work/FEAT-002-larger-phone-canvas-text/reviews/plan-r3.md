VERDICT: APPROVED

# Plan review: FEAT-002 Larger phone canvas text, round 3

**Reviewed:** plan
**Reviewer:** Crab (Plan Reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| P1-P4 | resolved | Resolved in round 2. Nothing in this revision undoes them. |
| P5 | resolved | Step 4 moves PERFECT to `max(80, hit + ceil(0.243×hitSize) + 4 + ceil(0.77×perfectSize))` through `internal static` helpers. I re-derived the values: 61/97 and 72/108 at 30 px, 58/91 and 69/102 at 27 px with PERFECT staying at 28. All match. The test row uses these values directly. Tests can reach `internal` members through `InternalsVisibleTo` (BlazorInvaders.Core.csproj:13). |
| P6 | resolved | Step 6 now describes the fit cap and says there is no wrapping. The Risks bullet names the header shift, the combo alignment, the moved lines and the fit cap. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P7 | minor | Step 4 helpers; AC2 "Perfect bonus + lost a life" test row | The helper formulas as written do not produce the design positions the test row expects. `HitBaseline(20, bossWave: true)` gives `max(55, 44+4+16) = 64`, not 55. `PerfectBaseline(55, 20, 28)` gives `max(80, 55+5+4+22) = 86`, not 80. Step 4 does say "only when the size changed", so desktop stays correct only if `Build` never calls the helpers at design size, or the helpers check for it themselves. As written, it is open which of the two is meant. | In step 4, say that the helpers return the design positions (55, and 80) when the hit size equals its design size (20). Add `HitBaseline(20, true) == 55` to the helper test, next to the existing (20, 28) → 55 and 80 cases. |
| P8 | minor | Step 4 header baseline; AC2 fit-cap test at 0.30 | The plan does not say whether the header baseline `max(22, ceil(size×0.77)+4)` uses the size before or after the fit cap. Example at scale 0.30 (floor 40): if the uncapped 40 is used, the baseline is 35. The combo, capped to about 32 px and right-aligned, then has a box of about 10..35. That touches the "SOUND OFF" box (about 34..45, x from 750, SceneBuilder.cs:132). This scale is outside the 375 px range the ACs cover. | Derive the baseline (and `delta`) from the final capped header size. Add "muted" to the 0.30 fit-cap scene. |

## Attacks the plan survived
- **Line references on base 840f5a4.** All match:
  - SceneBuilder.cs: header :89-90, combo :93, PERFECT :121, pause hint :127, SOUND OFF :132, hit :205, FRENZY :208, bar :264, touch hint :336, `Text`/`CenteredText` :379/:390
  - game.js :29/:106/:248
  - Index.razor `Tick` :126
  - effects.js :34/:99
  - Scene.cs `Muted` :31
  - ux-guidelines.md :33/:82/:85
  - GameEngine.cs :346/:654
- **"SOUND OFF" (14 px, x 750, baseline 45) against enlarged text at 0.41 and 0.445:**
  - Combo box ends at 28.
  - "You were hit!" box (38..68, or 49..79 in a boss wave) and PERFECT are centred and end well before x 750.
  - The boss bar ends at x 585.
  - No intersection.
- **New touch hint "II to pause · M toggles sound" at 265 (18→30, descender):** box 242..272. "Tap the screen to start" (28→30, 'p') ends at 227. No overlap. Its estimated width is far below 850.
- **"Tap II to resume" (20→30 at h/2+40):** box top about h/2+17, clear of "PAUSED" (48 px, baseline h/2-10, no descender).
- **PERFECT alone at 80 (30 px, box 57..80, x about 265..605):** clears FRENZY (x 10..~100), "BOSS" (x 225..~270, y 34..45), the bar (36..44) and the header (bottom 28).
- **BUG-001 pinned tests:** with a default builder (scale 1) the floor is 12, so no size changes and no layout branch runs. TouchControls cases stay identical.
- **27 px case (scale 0.445):** `ceil(12/0.445)=27`. Header delta 3; FRENZY at 48. PERFECT at 28 px is not enlarged but still moves below an enlarged hit line, as step 4 intends.
- **Test helpers named in Context exist:** `Decode`, `Texts`, `TextCommands`, `AssertCentred`, `EngineOfWidth`, `AttractState`, `IdleUntil` (SceneBuilderTests.cs:21-65) and `TestPlay.ChaseAndShootBoss` (TestPlay.cs:32).
- **Classification still correct:** Size M, UI-facing yes, Security no, one WP owning all 7 files. At 13.7 KB the plan is somewhat over the M budget, but not far enough to raise.

## Checked and fine
- No Testcontainers; no new dependency.
- The brief contained only template fields and the task; nothing to ignore.
- No instruction-like text found in the repository files I read.

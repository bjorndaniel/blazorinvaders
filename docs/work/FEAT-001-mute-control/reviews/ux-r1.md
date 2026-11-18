VERDICT: CHANGES_REQUESTED

# UX review: FEAT-001 Mute control, round 1

**Reviewed:** diff vs base_commit 5658734 + untracked files. Sources: `docs/ux-guidelines.md` (UX14, UX29, UX31, UX33-UX35, UX37, UX41-UX43, UX51), the existing touch controls in `app.css`/`Index.razor`, `.devagents/ux-defaults.md` and WCAG 2.2 AA. I did not see the running app: `preview` started it, but I have no Playwright tools. This is a code-only review.
**Reviewer:** Pixel (UX Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| U1 | major | src/wwwroot/css/app.css:262-265 | The pressed state changes the button border from `rgba(255,255,255,0.5)` to `rgba(255,255,255,0.25)`. On the page gradient (`linear-gradient(#111, #555)`, app.css:42) that border has a contrast of about 2.2:1 at the top and 1.9:1 at the bottom. The current border reaches 3.2-5.3:1. So when a touch player mutes, the Mute button's boundary drops below 3:1 and the button gets hard to find on the dark background. This departs from UX34 and WCAG 1.4.11, the plan does not declare it, and the colours are fixed, so the result does not depend on the device. | Remove the `border-color` line from the `[aria-pressed="true"]` rule, so the pressed state keeps the normal border and shows only the struck-through letter, as UX29 describes. If you want another pressed cue, it must keep the boundary at 3:1 or more against #111 to #555. |
| U2 | nit | src/core/Rendering/SceneBuilder.cs:336 | The touch hint "II to pause  ·  M toggles sound" does not say "Tap …" as UX43 asks for touch screens. It does follow the existing "II to pause" wording. | Optional: "Tap II to pause  ·  Tap M for sound", or leave it as it is. |

The other checks raised nothing:
- **Mute button:** the fixed name "Mute" with `aria-pressed` is correct (UX33, WCAG 4.1.2). The visible "M" is contained in the name (2.5.3). The button is 44x44 (UX37) and sits in the 2x2 grid (UX51).
- **"SOUND OFF" text:** it is uppercase (UX41), 14 px #888 on black, about 5.9:1 (UX14, UX34), and drawn after the pause overlay, so it is not dimmed. The state is shown as text, not only colour (UX35).
- **M key:** it ignores Ctrl, Alt and Meta and ignores key repeat (UX31). It is off during initials entry through `_disableKeyboard`, which is expected.
- **Saved setting:** anything other than "true" means sound on, so the default is sound on.
- **Guidelines document:** it is updated for UX29, UX31, UX33, UX51 and Known gaps.

## Notes for the PM
- I could not see the app, because no Playwright tools were available in this run. I started the preview at http://localhost:63382/ and stopped it with `--stop`. It reported no changed files. Two things were not checked visually, by me or by the tester: where the "SOUND OFF" HUD text lands next to the COMBO text and the mothership, and how the struck-through "M" looks on a phone.
- Outside UX scope, for the Code Reviewer: `git diff --stat` shows 723 changed lines in `src/wwwroot/css/app.css`. With `--ignore-cr-at-eol` it shows only 13. The rest is line-ending churn.
- The brief contained nothing beyond its template fields and the task.

VERDICT: APPROVED

# UX review: CHORE-014 Fix the five UX gaps, round 1

**Reviewed:** diff vs base_commit d2bf333 plus untracked files. I reviewed against docs/ux-guidelines.md (UX21, UX23, UX24, UX32-UX34, UX38, UX41, UX43, UX44), the existing UI and .devagents/ux-defaults.md. I did **not** see the running app: no Playwright tools were available to me, so this review is from the code only.
**Reviewer:** Pixel (UX Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| U1 | minor | src/Pages/Leaderboard.razor:32 | Basis: default rules (preserve context) and WCAG 2.4.3. A keyboard or screen-reader user who activates `Try again` loses focus. `ShowAsync` sets `_loadFailed = false` straight away, so the focused button leaves the DOM and focus drops to `<body>`. If the retry fails again, the alert is announced, but the user has to Tab from the top of the page to reach `Try again` again. | Keep focus somewhere predictable after a retry. For example, focus the selected tab or the new `Try again` button once `ShowAsync` finishes, or keep the button in the DOM (disabled) while the retry loads. |
| U2 | nit | src/Pages/Leaderboard.razor:32,66 | In the failed state, `Try again` and `Back to game` share one line. Both are `.btn` with `vertical-align: middle`, and only the link has `mt-2`, so the link sits about 0.25rem lower than the button. | Give `Try again` the same `mt-2`, or put it in its own block above the link. |

## Checked and fine
- AC1/UX24: the failed state is separate from the empty state. A non-success status or offline error (`GetFromJsonAsync` throws) shows the fixed message with `role="alert"` and no exception text (UX44). `null` still counts as empty. Switching tabs or retrying clears the failure. A stale response from another tab is ignored.
- `Try again` follows UX21: it is a `btn-secondary` with a verb label, sits next to its cause, and reloads the selected board (`_daily`).
- AC2/UX32/UX34: the playfield has a 2px white `:focus-visible` outline on its black and gradient background. The initials inputs have a 2px black `:focus` outline, offset from their 4px bottom border, on the white dialog. Both are well above 3:1 and take no layout space.
- AC3/UX33: each input is named "Initial n of 3" inside `role="group"`, which is labelled by the visible "Enter your initials" label. Nothing visible changes.
- AC4/UX38: the viewport meta allows zoom. `touch-action: none` remains only on `.game-canvas` and `.touch-btn` (app.css:218, 245). No JS blocks touchmove or gestures elsewhere.
- AC5/UX43/UX41: "Tap II to resume" matches the visible `II` button (Index.razor:44), the only touch control that resumes (`HandlePresses`), and the sentence case of "Tap D to play".
- AC6: the ux-guidelines.md Known gaps are cleared, UX24 now describes the failed state, and the revision line and Last updated are set.

## Notes for the PM
- No browser check was possible: the preview started at http://localhost:64557/, but I had no Playwright tools, and I stopped the preview (no changed files reported). The visual checks the plan left for UX (AC2's fresh-load ring, AC4's pinch zoom on a touch device) are still unobserved by anyone.
- From the code, this is likely: `Index.razor:99` focuses the playfield by script on load, so Chromium will probably show the white ring right away. On touch screens the ring may stay for the whole game, because the touch buttons `preventDefault` on pointerdown and focus never moves. AC2 asks for this ring and it is accurate, but it is a visible change on phones. Have someone look at it on a phone or in touch emulation before merge, and decide whether that is acceptable.

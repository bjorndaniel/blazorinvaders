VERDICT: APPROVED

# UX review: CHORE-014 Fix the five UX gaps, round 2

**Reviewed:** changes since round 1 (`devagents.mjs changes --since 1`: Leaderboard.razor, app.css, docs/ux-guidelines.md). I reviewed against docs/ux-guidelines.md (UX21, UX24, UX32), D2 in status.md, the existing UI and .devagents/ux-defaults.md. I did **not** see the running app: I had no Playwright tools again, so this review is from the code only.
**Reviewer:** Pixel (UX Reviewer)

## Earlier findings
| ID | Severity | Status | Why |
|----|----------|--------|-----|
| U1 | minor | resolved | `RetryAsync` (Leaderboard.razor:81-86) focuses the selected tab after the load. The tabs are always rendered, so focus lands somewhere predictable. If the retry fails again, `Try again` is one or two Tabs away. |
| U2 | nit | resolved | `Try again` now has `mt-2` (Leaderboard.razor:32), the same as `Back to game`. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| U3 | minor | src/Pages/Leaderboard.razor:84 | Basis: default rules (no raw technical errors) and UX44. Scenario: on a slow connection a user taps `Try again`, gives up and taps `Back to game` before the load finishes. When `ShowAsync` then returns, `FocusAsync` runs on a tab element that has left the DOM. In Blazor 10.0.4 the element-reference reviver returns `null` (`document.querySelector`), and `domWrapper.focus` throws "Unable to focus an invalid element." The handler does not catch it, so the game page shows `#blazor-error-ui` ("An unhandled error has occurred. Reload"). The user can dismiss the bar, and the game still works. | Only move focus while this load is still the current one and the page is still shown. For example, skip `FocusAsync` once the component is disposed, or catch `JSException` around it. |

## Checked and fine
- D2: `.game-canvas:focus-visible { outline: none }` sits inside the existing `@media (pointer: coarse)` block (app.css:292-295). That is the query that shows the touch pads and that `game.js:46` uses to detect touch. It comes after the base rule (app.css:221) and has the same specificity, so it wins only on touch devices. Keyboard users on fine-pointer devices keep the 2px white ring. The initials `:focus` ring (app.css:353) is not affected.
- UX32 is updated for the D2 departure (docs/ux-guidelines.md:54), so no undeclared guideline departure remains.
- The C1 request counter applies only the newest load. A tab switch during a retry cannot show a stale failure or a stale board, and the alert still appears only for the current request.
- If the user switches tab during a slow retry, `RetryAsync` focuses the newly selected tab (`_daily` is read after the await). That is the tab the user just activated.
- The focused selected tab (`btn-light`) keeps Bootstrap's existing focus style. No new styling was added.

## Notes for the PM
- No browser check was possible: no Playwright tools were available, so I did not start the preview. The Tester's round 2 headless Chrome checks of AC1-AC4 and D2 are the only observation of the running app. A real pinch on a phone is still untried.

VERDICT: APPROVED

# UX review: CHORE-014 Fix the five UX gaps, round 3

**Reviewed:** changes since round 2 (`devagents.mjs changes --since 2`: src/Pages/Leaderboard.razor, docs/ux-guidelines.md; TRACKER.md is not UI). I reviewed against docs/ux-guidelines.md (UX32, UX44), D2 in status.md, the existing UI (the `Index.razor` dispose guard) and .devagents/ux-defaults.md. I did **not** see the running app: I had no Playwright tools, so this review is from the code only.
**Reviewer:** Pixel (UX Reviewer)

## Earlier findings
| ID | Severity | Status | Why |
|----|----------|--------|-----|
| U3 | minor | resolved | `RetryAsync` (Leaderboard.razor:84-90) now calls `FocusAsync` only `if (!_disposed)`, and `Dispose` sets the flag. Blazor disposes the page when the user goes `Back to game`. That happens before the awaited `ShowAsync` continues, so the focus call on the removed tab is skipped and `#blazor-error-ui` no longer appears. If the user stays on the page, focus still moves to the selected tab, as it did in round 2. The guard follows the existing `Index.razor` pattern. |

## Findings
none

## Checked and fine
- UX32 now cites "CHORE-014 D2" in its text and Source column (docs/ux-guidelines.md:54). The touch-ring rule is unchanged, so the D2 departure is still recorded.
- No user-facing copy, markup or styles changed this round apart from the guard. The tab order, the alert and the `Try again` / `Back to game` buttons are as I approved them in round 2.

## Notes for the PM
- I could not check anything in a browser: no Playwright tools were available, so I did not start the preview. The Tester's round 3 headless Chrome checks (Q2a, Q2b, C2 navigate-away) are the only time anyone has watched this fix run.
- The brief contained only its template fields and the task. There was nothing to ignore.

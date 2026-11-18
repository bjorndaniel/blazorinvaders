VERDICT: APPROVED

# Code review: CHORE-014 Fix the five UX gaps, round 3

**Reviewed:** the changes since round 2 (`devagents changes --since 2`). I skipped the item folder and TRACKER.md.
**Reviewer:** Saucer (code-reviewer)

## Earlier findings
| ID | Severity | Status | Why |
|----|----------|--------|-----|
| C2 | major | resolved | `Leaderboard` now implements `IDisposable` and sets `_disposed` in `Dispose`. `RetryAsync` calls `FocusAsync` only `if (!_disposed)` (src/Pages/Leaderboard.razor:89-90). Take the case where the user navigates away while the retry request is in flight. `Dispose` has already run when the request completes, so the focus call is skipped and no `JSException` reaches `#blazor-error-ui`. WebAssembly runs on one thread, so nothing can dispose the component between the check and the interop call. The guard follows the `Index.razor:257` pattern. Plain `IDisposable` is correct here because nothing needs to be awaited. |
| C3 | nit | not changed (accepted) | The implementer's reason holds: the `Try again` button is not rendered while the retry loads. Focusing it would need an extra ref and a post-render step, which is too much for a nit. |

## Findings
None.

## Checked and fine
- If the user switches tabs during a retry, `_daily` reflects the tab they last clicked, so focus goes to that tab. That is consistent.
- `StateHasChanged` after disposal is harmless: the renderer ignores it, and `ShowAsync` only sets fields.
- UX32 in docs/ux-guidelines.md now cites "CHORE-014 D2" in both the text and the Source column, which matches D2 in status.md.

## Notes for the PM
- The brief held only its template fields and the task. There was nothing to ignore.

Files reviewed:
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\src\Pages\Leaderboard.razor
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\docs\ux-guidelines.md

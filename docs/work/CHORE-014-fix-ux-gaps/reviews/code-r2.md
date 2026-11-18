VERDICT: CHANGES_REQUESTED

# Code review: CHORE-014 Fix the five UX gaps, round 2

**Reviewed:** changes since round 1 (`devagents changes --since 1`; item folder and TRACKER skipped)
**Reviewer:** Saucer (code-reviewer)

## Earlier findings
| ID | Severity | Status | Why |
|----|----------|--------|-----|
| C1 | minor | resolved | `ShowAsync` applies a result only when `request == _request`. A tab switch, a re-click and a retry each bump the counter, so a stale success or failure is dropped. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C2 | major | src/Pages/Leaderboard.razor:81-86 | `RetryAsync` calls `FocusAsync()` after the load finishes without checking that the page still exists. To reproduce: open the Leaderboard with the API failing, click `Try again`, then click the Play nav link before the response arrives (a cold Functions start, or a request that hangs while offline). When the request completes, the component is gone and the tab buttons are out of the DOM. Blazor's element-reference reviver (`document.querySelector('[_bl_…]')`) returns null, and `focus` in blazor.webassembly.js 10.0.4 throws "Unable to focus an invalid element.". That surfaces as an unhandled `JSException` from the event handler, so the `#blazor-error-ui` bar ("An unhandled error has occurred") appears over the game. This was introduced this round. `Index.razor:256-258` already guards the same call with `if (!_disposed)`. | Follow the `Index.razor` pattern. Implement `IDisposable` on `Leaderboard` with a `_disposed` flag, and call `FocusAsync` only `if (!_disposed)`. Cancelling the request with a `CancellationToken` in `Dispose` is also acceptable. |
| C3 | nit | src/Pages/Leaderboard.razor:85 | If the retry fails again, focus goes to the selected tab rather than to the new `Try again` button, so keyboard users must Tab once to retry again. | Optional: focus the tab only when `!_loadFailed`. |

## Checked and fine
- D2: `.game-canvas:focus-visible { outline: none; }` sits inside the existing `@media (pointer: coarse)` block. It has the same specificity as the rule at app.css:221 and comes later, so it wins only on coarse pointers. That is the same query `game.js:46` uses to turn on touch controls, so the ring shows for keyboard users and hides while touch controls are active, as D2 asks.
- The UX32 text in docs/ux-guidelines.md records the D2 departure.
- U1: the tab `@ref`s always render, so focus after a successful retry (component still mounted) is valid. `FocusAsync` is the same API `Index.razor` uses (net10.0).
- U2: `mt-2` on `Try again` is fine.

## Notes for the PM
- The brief had an extra `User decision:` line. It matches D2 in status.md, so it changed nothing. I took the decision from status.md.

Files reviewed:
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\src\Pages\Leaderboard.razor
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\src\wwwroot\css\app.css
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\docs\ux-guidelines.md

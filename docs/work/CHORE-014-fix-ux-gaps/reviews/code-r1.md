VERDICT: APPROVED

# Code review: CHORE-014 Fix the five UX gaps, round 1

**Scope:** the whole diff against d2bf333 (7 files; the item folder skipped)

## Summary
All five fixes are small and follow the plan and the existing patterns. In detail:
- **Leaderboard failed state:** it is tracked separately from the empty state. The catch now logs the way `Index.razor` does (CG20, CG21, CG24, CG26), and the error text holds no exception detail (UX44).
- **Pause text:** it follows the existing `TouchControls ? "Tap …" : "Press …"` pattern. The new `[Theory]` follows the existing tests.
- **Initials inputs, focus outlines and viewport:** these edits are minimal and correct. The "II" label matches the touch Pause button (`Index.razor:44`).

I found no blocker or major issues.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | minor | src/Pages/Leaderboard.razor:90-97 | The stale-response guard compares only the tab (`_daily == daily`), not the request. Suppose two loads of the same tab overlap: the user clicks the selected tab again while it loads, or clicks it during a `Try again`. If the earlier request fails after the later one starts, the catch sets `_loadFailed = true`. The page then shows "Could not load…" even though the later request loads the scores, because `_loadFailed` takes precedence in the markup. Before this change the same race showed an empty board instead, so it is not a regression. It needs a double click during a slow, failing request. | Optional: give each call a request counter (`var request = ++_request;`) and apply the result only if `request == _request`. Otherwise justify it in the implementation notes. |

## Notes for the PM
- The brief contained only the template fields and the task. There was nothing to ignore.
- AC1-AC4 were checked by reading the code only, by both the Tester and me. That includes the risk the plan names: the white focus ring may show on a fresh load because the playfield gets focus by script. Pixel or Octopus needs to check this in a browser.

Files reviewed:
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\src\Pages\Leaderboard.razor
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\src\Components\SetHighScore.razor
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\src\wwwroot\css\app.css
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\src\wwwroot\index.html
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\src\core\Rendering\SceneBuilder.cs
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\tests\BlazorInvaders.Tests\SceneBuilderTests.cs
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps\docs\ux-guidelines.md

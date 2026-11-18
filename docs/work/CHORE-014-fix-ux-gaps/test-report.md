# Test report: CHORE-014

## Round 1
RESULT: PASS

### Suite
| Check | Result |
|---|---|
| dotnet build | 0 warnings, 0 errors |
| dotnet test | 263 passed, 0 failed |
| lint/typecheck | no separate step; build covers it |

### ACs
| AC | Verification | Result |
|---|---|---|
| AC1 | code inspection of Leaderboard.razor (no browser run): `_loadFailed` set in catch, alert text and Try again shown before the empty branch; manual browser check not run | verified by inspection |
| AC2 | app.css: outline none removed, `:focus-visible` white / `:focus` black outlines; fresh-load ring not observed (no browser) | verified by inspection |
| AC3 | SetHighScore.razor: group labelled by label id, aria-label "Initial n of 3" | verified by inspection |
| AC4 | index.html viewport has no maximum-scale/user-scalable; touch-action none kept on canvas and touch buttons | verified by inspection |
| AC5 | `PausedScene_InstructionMatchesTheControls` (2 cases) passes | pass |
| AC6 | Known gaps emptied; tests pass | pass |

Tests added: none by me (implementer's theory covers AC5).

### Breaks
| # | Break | Result |
|---|---|---|
| B1 | AC5 touch text reverted | caught |
| B2 | AC5 condition inverted | caught |

Mutation: not run (no mutation framework step for this item).

## Round 2
RESULT: PASS

Q1 answered: AC1-AC4 run in a real Chrome 157 (headless, driven over the DevTools protocol with throwaway scripts outside the worktree), against `dotnet run` on localhost. `getleaderboard` and `appsettings.json` responses were mocked in the browser. Re-verified after the round 2 changes (D2 touch ring, U1 focus, U2, C1 request counter).

### Suite
| Check | Result |
|---|---|
| dotnet build src/BlazorInvaders.sln | 0 warnings, 0 errors |
| dotnet test | 263 passed, 0 failed |
| lint/typecheck | no separate step; build covers it |

### ACs
| AC | Verification | Result |
|---|---|---|
| AC1 | Browser, `/leaderboard`, request failing: All-time and Today's tabs both show "Could not load the leaderboard. Check your connection and try again." and a `btn-secondary` `Try again`; no "No scores yet". API back with `[]` and `Try again` clicked: "No scores yet. Be the first!", no error; focus lands on the selected tab (Today's, U1). API with one score: table shows it. | pass |
| AC2 | Browser. Desktop: after Tab the playfield matches `:focus-visible` with `solid 2px rgb(255,255,255)`; after a mouse click `outline: none`. Fresh load (script focus): playfield is active but `:focus-visible` is false, no ring, in headless Chrome (a headed Chrome may differ; not observable here). Touch emulation (`pointer: coarse`, 915x412 mobile): fresh load no ring; even after Tab (`:focus-visible` true) outline is `none` (D2). Initials dialog (reached by playing `/?wave=5` idle to game over with an empty board): all 3 inputs show `solid 2px rgb(0,0,0)` on focus, desktop and touch. | pass |
| AC3 | Browser accessibility tree on the open dialog: group "Enter your initials"; textboxes "Initial 1 of 3", "Initial 2 of 3", "Initial 3 of 3". | pass |
| AC4 | Served viewport meta is `width=device-width, initial-scale=1.0`. Effective `touch-action` over all ancestors: leaderboard table `auto`; initials dialog and body `auto`; playfield canvas and touch buttons include `none`. A real pinch was not reproduced: `Input.synthesizePinchGesture` in headless Chrome gave scale 1 even on a plain page with a permissive meta (control), so it cannot tell pass from fail. A pinch on a real phone is untried. | pass by computed state; pinch gesture not run |
| AC5 | `PausedScene_InstructionMatchesTheControls` (2 cases) | pass |
| AC6 | Known gaps holds only "None open" and the UX29/UX14 line; UX32 notes no ring on touch (D2); suite passes | pass |

Tests added: none (AC1-AC4 are Razor/CSS/HTML; A5 rules out component tests).

### Breaks
Round 2 changed only Razor, CSS and docs. `SceneBuilder.cs` and its test are unchanged since round 1, so round 1 breaks (B1, B2 caught; QA B1-B3 caught) stand. No new unit-reachable logic, so no new breaks.

### Mutation
Not run: no mutation framework (`quality-tools` declined) and no core code changed since round 1.

## Round 3
RESULT: PASS

Q2 answered with a browser check (headless Chrome 157 over CDP, `dotnet run` on localhost, `Fetch` interception of `getleaderboard` with delays; throwaway script outside the worktree). The C2 scenario is checked too.

### Suite
| Check | Result |
|---|---|
| dotnet build src/BlazorInvaders.sln | 0 warnings, 0 errors |
| dotnet test | 263 passed, 0 failed |
| lint/typecheck | no separate step; build covers it |

### Browser checks (Leaderboard.razor, `_request` guard and `_disposed`)
| Check | Steps | Observed | Result |
|---|---|---|---|
| Q2a, stale failure | First `getleaderboard` held 3 s, then fails. While pending, click the selected All-time tab again (second call returns `[]`). Wait for the stale failure. | After the click: "No scores yet". After the stale failure: still "No scores yet", no "Could not load" | pass |
| Q2b, stale success | All-time held 3 s then returns ALL/999; Today's returns DAY/111 at once. Click Today's while All-time is pending. | Today's board shows DAY; after All-time resolves it still shows DAY, no ALL | pass |
| C2, leave during retry | API returns 500; click `Try again` (second call held 3 s), click `Back to game` at once, wait for the response. | Route is `/`; `#blazor-error-ui` display `none` | pass |

These checks fail if the guard in `catch` (Q2a) or in the success path (Q2b) is dropped: stale results would replace the current board. I did not run that break (production code is not changed by me; `breaks` runs only the unit suite).

### ACs
AC1 to AC4 keep their Round 2 result for everything the round 3 change did not touch; the AC1 load, retry and tab-switch behaviour is re-exercised by the checks above. AC5 and AC6: suite passes (263/263).

Tests added: none (browser checks only; A5 rules out component tests).

### Breaks
No new unit-reachable code; earlier breaks stand.

### Notes
Q3 (citation "D2" in `docs/ux-guidelines.md:54`) is a document fix, not mine to change.

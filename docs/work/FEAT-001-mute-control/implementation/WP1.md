# Implementation: FEAT-001 Mute control — WP1

## Round 1
**Plan steps covered:** WP1 steps 1-8
**Addresses findings:** n/a
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/core/GameObjects/SoundSetting.cs` | added — mute state and storage parsing |
| `src/core/Rendering/Scene.cs` | modified — `Muted` flag |
| `src/core/Rendering/SceneBuilder.cs` | modified — `Sound`, "SOUND OFF" HUD text, attract hints |
| `src/Pages/Index.razor` | modified — `MutePressed`, load/toggle/store, small-button group and M button |
| `src/wwwroot/js/game.js` | modified — `INPUT_MUTE`, KeyM, master gain, `setMuted` |
| `src/wwwroot/css/app.css` | modified — `aria-pressed` style, 2x2 small-button grid |
| `tests/BlazorInvaders.Tests/SoundSettingTests.cs` | added — parsing and round trip |
| `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` | modified — new mute tests; existing centred-lines test updated for the new hint text |
| `docs/ux-guidelines.md` | modified — UX29, UX31, UX33, UX51, Known gaps, revisions |
| `docs/security-model.md` | modified — TB6 lists `muted` |
| `README.md` | modified — controls line |

### Deviations from plan
None.

### Commands run
- `dotnet test tests/BlazorInvaders.Tests` → 309 passed
- `dotnet build src/BlazorInvaders.csproj` → 0 warnings, 0 errors
- `node --check src/wwwroot/js/game.js` → ok
- No formatter configured for JS/CSS/MD; files kept CRLF like the working copy.

## Round 2
**Plan steps covered:** WP1 step 7 (fix only)
**Addresses findings:** U1 (fixed), C1 (fixed), Q1 (fixed, same root cause as C1); C2 and U2 not changed (optional nits)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/wwwroot/css/app.css` | modified — removed the `border-color` change from `.touch-btn[aria-pressed="true"]` (U1); file rewritten with LF endings and no trailing lone CR, matching the index (C1, Q1) |

### Evidence
- U1: the pressed state now changes only `text-decoration: line-through`; the border keeps the normal `rgba(255,255,255,0.5)` (UX29, UX34).
- C1/Q1: `git ls-files --eol` shows `i/lf w/lf`; `git diff 5658734 --stat -- src/wwwroot/css/app.css` shows 10 insertions, no deletions.

### Deviations from plan
None. The plan's step 7 said the pressed state has "a dimmer border"; the change removes it, as U1 requires.

### Commands run
- `git diff 5658734 --stat -- src/wwwroot/css/app.css` → 1 file, 10 insertions
- `git ls-files --eol src/wwwroot/css/app.css` → `i/lf w/lf`
- No formatter configured for CSS; no code changed, so the test suite is unaffected.

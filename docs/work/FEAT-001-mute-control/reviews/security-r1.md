VERDICT: APPROVED

# Security review: FEAT-001 Mute control, round 1

**Reviewed:** diff vs base_commit 5658734 plus untracked files (`src/core/GameObjects/SoundSetting.cs`, `tests/BlazorInvaders.Tests/SoundSettingTests.cs`, item folder)
**Reviewer:** Bunker (Security Reviewer)

## Threat model
- Crosses only TB6 (`localStorage` to client). The change adds one key, `muted`, which is read on load in `src/Pages/Index.razor` and written when the player toggles mute. Nothing goes to the API (TB1-TB5), and no server, Function or CI code changes.
- Actor: only AC1, the anonymous player, acting on its own device. No permission changes.
- Threat: T14 (client tampering with `localStorage`). A tampered value can only change the player's own sound setting, so T14 stays mitigated.
- `docs/security-model.md` TB6 now lists `muted`. No actor, boundary or threat changed, so nothing else in the model needed updating.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|

None.

## Checked and fine
- **Input parsing:** `SoundSetting.FromStored` (`src/core/GameObjects/SoundSetting.cs`) mutes only on an exact ordinal match with `"true"`. Null, empty or junk values leave sound on. No exception path, and no deserialization of untrusted JSON.
- **Storage access:** reads and writes go through the existing `getItem`/`setItem` in `src/wwwroot/js/game.js:88-93`, which already catch errors. The key is a constant (`SoundSetting.StorageKey`) and the values are fixed literals.
- **Rendering:** `aria-pressed` in `Index.razor` is bound to a fixed `"true"`/`"false"` and Razor encodes it. "SOUND OFF" is a constant string drawn with `fillText`. No XSS surface.
- **Input handling:** `KeyM` in `game.js` skips Ctrl, Alt and Meta and is ignored on key repeat. The touch button reuses the existing `[data-press]` handler (`game.js:124-130`), and bit 64 only toggles mute in `Index.razor`. Nothing new reaches the engine or the leaderboard submission.
- **Audio:** the master `GainNode` and `setMuted` use only Web Audio built-ins. No new resource growth, since muted calls return before any oscillator is created.
- **CSS:** `src/wwwroot/css/app.css` adds 11 lines (the rest of its diff is line-ending churn). Styling only.
- **Dependencies, CI and infrastructure:** no manifest, lockfile, workflow, Dockerfile or IaC changes.
- **Secrets and prompt injection:** none in the diff, the tests or the item folder (`plan.md`, `request.md`, `status.md`, `implementation/WP1.md`, test report, `test-breaks-r1.json`).

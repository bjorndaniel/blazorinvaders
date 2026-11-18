VERDICT: APPROVED

# Code review: FEAT-001 Mute control, round 1

**Reviewed:** diff vs base_commit 5658734 + untracked files
**Reviewer:** Saucer (Code Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | minor | src/wwwroot/css/app.css:366 | The file now ends in a lone `\r` (`}\r`, no `\n`). It has 367 CRs and 366 LFs. Because of that lone CR, git classifies the working copy as non-text (`git ls-files --eol` shows `i/lf w/-text`), so `core.autocrlf=true` does not normalise it. `git diff` shows all 723 lines changed, and a commit would store the whole file with CRLF endings when the real change is 12 lines. Every other branch that touches app.css would then hit a whole-file conflict, and blame would be lost. The implementation notes ("files kept CRLF like the working copy") suggest this was not intended. | Remove the trailing `\r` so the file ends with `}` as the base does (or with `}\r\n`). Then confirm that `git -C .work/FEAT-001-mute-control diff 5658734 --stat -- src/wwwroot/css/app.css` shows about 12 lines and `git ls-files --eol` shows `w/crlf`. |
| C2 | nit | tests/BlazorInvaders.Tests/SceneBuilderTests.cs:293 | `AttractInstructions_NameTheMuteKey` repeats the 3-frame setup of `AttractInstructions_MatchTheControls`. Its expected strings are also asserted by the updated `StartScreen_LinesAreCentred_WithUnchangedTextFontAndY`. | Optional: fold the new strings into `AttractInstructions_MatchTheControls` as extra `InlineData` rows, or leave it as it is. |

## Checked and fine
- `SoundSetting` matches CG15 (sealed), CG17, CG33 (`private set`) and CG60 (summaries). Parsing is ordinal and exact, and every other stored value, missing included, means sound on.
- `Index.razor`: `MutePressed = 64` matches `INPUT_MUTE = 64` in game.js (CG13 comment covered by the existing lines on both sides). `_scenes.Sound` is loaded before `start`, so no Tick runs with a stale setting. The toggle is serialised by `frameBusy` and zeroed while the initials dialog is open, as planned. `_module!` in `ToggleMuteAsync` follows the existing Tick-path invariant (CG30), like lines 153 and 159.
- `aria-pressed` renders as a `"true"`/`"false"` string. `listen` uses `querySelectorAll('[data-press]')`, so the nested button gets wired with no JS change.
- game.js audio: every tone goes through `playTone` or `alienDeath`, and both route to `master`. Nothing still connects straight to `ctx.destination`. `setMuted` never creates the context and changes the gain only when the value changes. `playSounds` runs on every frame, paused frames included, so muting takes effect at once in every state. The march interval keeps running silently.
- KeyM: ignored with Ctrl, Alt or Meta held, `preventDefault` only when it is handled, and repeats ignored (UX31).
- SceneBuilder: "SOUND OFF" is drawn after the pause overlay, and `Scene.Muted` reaches JS as `muted` through camelCase serialisation, like `paused`.
- Docs: UX29/31/33/51, Known gaps, revisions, security-model TB6 and the README controls line are all updated. No changelog exists in the repository.

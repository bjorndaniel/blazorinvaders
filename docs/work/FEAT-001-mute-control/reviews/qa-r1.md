VERDICT: APPROVED

# QA review: FEAT-001 Mute control, round 1

**Reviewed:** diff vs base_commit 5658734 + untracked files
**Reviewer:** Octopus (qa-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | minor | `src/wwwroot/css/app.css` (whole file, last byte) | This is an unplanned whole-file line-ending rewrite. The base file is LF (`i/lf`). The working copy is CRLF throughout and now ends in `}\r`, a lone CR with no LF (`tail -c 40 … \| od -c`). Because of that, git classes the file as `w/-text`, so autocrlf no longer normalises it. `git diff 5658734 --stat` reports 723 changed lines, but `--ignore-cr-at-eol` shows the real change is only +12/-1. Committed like this, every line of the file becomes CRLF in the repo. That causes merge conflicts with any parallel branch that touches `app.css`. | Save `app.css` with LF endings as in the base (or with CRLF that autocrlf can normalise), with no trailing lone CR. After that, `git diff 5658734 --stat -- src/wwwroot/css/app.css` should show only the ~12 lines from step 7. |

## QA evidence
Assurance: standard. No `quality` section in `.devagents/config.json` (`quality-tools` is declined), so I ran no mutation framework. I ran coverage with the project's own coverlet and made the breaks by hand.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| S1 SoundSetting | `src/core/GameObjects/SoundSetting.cs:1-24` | done (sealed, `StorageKey`, `private set`, Ordinal `FromStored`, `Toggle`, `ToStored`, summaries) |
| S2 Scene flag | `src/core/Rendering/Scene.cs:30-31` | done |
| S3 SceneBuilder | `SceneBuilder.cs:60-61` (`Sound`), `:130-132` ("SOUND OFF" after the pause overlay), `:142` (`Muted`), `:336`, `:350` (hints) | done |
| S4 Page | `Index.razor:44-48` (group + button with string `aria-pressed`), `:63` (`MutePressed`), `:95` (load before `start` at :104), `:133-134`, `:184-189` (`ToggleMuteAsync`) | done |
| S5 JS input | `game.js:13` (`INPUT_MUTE`), `:153-157` (KeyM, modifiers return before `preventDefault`, `!e.repeat`) | done |
| S6 JS audio | `game.js:289-290`, `:306-307`, `:312-320` (master gain created at current state), `:325`, `:330`, `:339-343` (`setMuted` only on change, never calls `getCtx`), `:346`, `:351`; march keeps its interval (`:375-385`) | done |
| S7 CSS | `app.css:262-271` | done (see Q1 for the file's line endings) |
| S8 Docs | `docs/ux-guidelines.md` UX29, UX31, UX33, UX51, Known gaps, Last updated, Revisions; `docs/security-model.md` TB6; `README.md:20` | done |
| T `FromStored_Missing_SoundOn` | `SoundSettingTests.cs` | present, asserts null and "" give sound on |
| T `FromStored_Value_ParsesOnlyTrue` | `SoundSettingTests.cs` | present, all 5 cases |
| T `Toggle_RoundTripsThroughStorage` | `SoundSettingTests.cs` | present, matches the row |
| T `MutedScene_ShowsSoundOffAndReportsMuted` | `SceneBuilderTests.cs:309` | present, attract/playing/paused, both asserts |
| T `UnmutedScene_HasNoSoundOff` | `SceneBuilderTests.cs:327` | present |
| T `AttractInstructions_NameTheMuteKey` | `SceneBuilderTests.cs:293` | present, touch and keyboard |
| T manual browser check | `test-report.md` | done by the Tester. Audio mid-march and mid-`playerDeath` was not heard (headless); the Tester checked it by reading the code |
| `StartScreen_LinesAreCentred_…` hint strings | `SceneBuilderTests.cs:391-392` | unplanned but needed: it follows the step 3 text change and is declared in WP1.md |

Work packages: every changed file is on WP1's list (TRACKER.md is the PM's file).

### Coverage and CRAP
I ran coverlet (`coverlet.collector`, already referenced in the test project) through `scratch` with `dotnet test tests/BlazorInvaders.Tests --collect:"XPlat Code Coverage"`: 309 passed. Every changed C# production line is executed: `SoundSetting.cs` line-rate 1, branch-rate 1. `SceneBuilder.cs` lines 61, 131 (2/2 branches), 132, 142, 336 and 350 are hit, and `Scene.cs:31` is hit. No complexity tool is configured, so there is no CRAP score. Every changed C# function has complexity 2 or less and is fully covered. `Index.razor` and `game.js` are not measured, because A5 has no component or JS tests (the plan declares this).

### Mutation testing
Not run: no mutation framework is configured (`quality` is missing, `quality-tools` declined).

### Breaks
| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | AC3 HUD condition inverted | src/core/Rendering/SceneBuilder.cs:131 | SoundSetting + SceneBuilder tests | caught (4 failed) |
| B2 | AC3 SOUND OFF hidden while paused | src/core/Rendering/SceneBuilder.cs:131 | same | caught (1 failed) |
| B3 | AC3 SOUND OFF hidden on attract screen | src/core/Rendering/SceneBuilder.cs:131 | same | caught (1 failed) |
| B4 | AC4 any non-empty value other than "false" mutes | src/core/GameObjects/SoundSetting.cs:17 | same | caught (3 failed) |
| B5 | AC4 unmuted stored as "" (still reads back as sound on) | src/core/GameObjects/SoundSetting.cs:23 | same | caught (1 failed) |
| B6 | AC1 toggle only ever mutes | src/core/GameObjects/SoundSetting.cs:20 | same | caught (1 failed) |
| B7 | AC1 keyboard hint drops the M key | src/core/Rendering/SceneBuilder.cs:350 | same | caught (3 failed) |

7 breaks: 7 caught, 0 survived. I did not repeat the Tester's B1-B5. The breaks list is `C:\Projects\bjorndaniel\blazorinvaders\.work\FEAT-001-mute-control\docs\work\FEAT-001-mute-control\reviews\qa-r1-breaks.json`. I did not break `Index.razor` or `game.js`: the approved plan leaves them without unit tests (A5), covered by the Tester's browser check instead.

## Checked and fine
- No skipped or focused tests, no tautologies, no mocks. Every new test calls the real `SoundSetting` or `SceneBuilder` and asserts the observable outcome.
- The `Tick` mute bit works in every state. Input is still zeroed while `_disableKeyboard` is set (the initials dialog).
- No `c.destination` is left except the master gain's own connection. Every tone goes through `playTone` or `alienDeath`, and both return early while muted.
- The march keeps `marchStep` and its interval while muted, so it resumes in tempo.
- `playSounds` runs every frame (`game.js:110`), so `setMuted` is applied on the frame of the toggle.
- The doc edits match step 8. UX37 is unchanged.

Restored: yes

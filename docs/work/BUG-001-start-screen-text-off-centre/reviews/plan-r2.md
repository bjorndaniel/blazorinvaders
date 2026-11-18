VERDICT: APPROVED

# Plan review: BUG-001 Start screen text off-centre, round 2

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| P1 | resolved | D1 settles the build question: the screenshot is from `master`. The plan now names the `master` cause (`Game.cs:604-619`, measuring inside the batch while the font is stale), makes no JS change, and is reduced to C# pinning tests. I checked the worktree at 886d225: `SceneBuilder.cs:382-383` emits `width / 2` with the centre flag, and `game.js:210` sets `textAlign = 'center'`. |
| P2 | resolved | No JS test tool remains. The tests are xUnit in `tests/BlazorInvaders.Tests`, as architecture A5/A11 require, so no document update is needed. |
| P3 | resolved | `effects.js` is out of scope. |
| P4 | resolved | The plan is 3.5 KB, under the S budget. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P5 | minor | Tests, row AC1-AC3 | The test asserts that "every text command with centre flag 1 has `x == engine.Width / 2`". That is not true of every centred command: the power-up letters are centred text placed at the power-up's own position (`SceneBuilder.cs:231`, 10 px). A play-state test (wave banner, PERFECT, "You were hit!") fails if a power-up is on screen. | Assert on the named message lines (match by text), or skip the 10 px power-up letters. Do not assert on every centre-flag command. |

## Attacks the plan survived
- Every message AC2 lists goes through `CenteredText` with `w = engine.Width` (`SceneBuilder.cs:83,183,296`): lines 105-124, 197, 240, 280-283, 300-342.
- Odd width: `GameEngine(int width, int height, Random?)` (`GameEngine.cs:92`) can be built at 801. 801/2 = 400, which is 0.5 px from the true centre and within AC1's 2 px.
- AC4 values match the code: desktop lines at y 160/220/265, 28/28/18 px, font string `"{n}px Orbitron"` (`SceneBuilder.cs:340-342,377`). Touch line 1 is 26 px (line 326). The plan's font row names only the desktop lines, so it does not claim 26 px is 28.
- The helpers `Decode`, `Texts` and `NewEngine` (870x500) exist in `SceneBuilderTests.cs:20-36`.
- The browser draw path is not unit-tested, which is consistent with A5 (no JS tests). The plan hands that check to the QA run. On this branch the remaining ink-versus-advance error is under 1 px (font analysis in round 1).
- Classification: S, not security-sensitive, not UI-facing. This is correct for a change to one test file with no visible change.
- Risk: the deployed bug stays until the branch merges and deploys. The plan states this, and it follows from D1.

## Checked and fine
- WP1 owns its one file exclusively and has no dependencies.
- Nothing is out of scope, and there is no new dependency.

## Notes for the PM
- Besides its template fields, the brief carried a "User decision" line. I did not take it from the brief. I checked it against `status.md` D1, which says the same thing.

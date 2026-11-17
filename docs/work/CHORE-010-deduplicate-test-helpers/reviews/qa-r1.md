VERDICT: APPROVED

# QA review: CHORE-010 Deduplicate test helpers, round 1

**Reviewed:** diff vs base_commit 886d225 + untracked files
**Reviewer:** Octopus (qa-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | minor | tests/BlazorInvaders.Tests/TestPlay.cs:17-29; SceneBuilderTests.cs:113,131,169; BossWaveTests.cs:97; PowerUpEngineTests.cs:49 | Plan step 4 allowed replacing `RunMs` only "if tests still pass with identical results", and the Summary kept the StepMs loop for default `hz`. The shared double-math `Run` runs one more engine step than the old helpers did. I simulated the engine's float32 accumulator (scratchpad sim.py): SceneBuilder at 500 ms goes from 29 to 30 steps, BossWave `Boss_AttacksAfterBanner` at 1900 ms from 113 to 114, PowerUp `RapidFire_KeepsThreeShotsInFlight` at 2000 ms from 119 to 120. All other durations are unchanged. The deviation is written up in WP1.md, all 211 tests pass, and B5 shows the boss-timing test still pins the attack time. | No code change needed. The PM records the deviation as accepted under Decisions, or reverts the three SceneBuilder calls to a step loop if exact frame counts must be kept. |
| Q2 | minor | tests/BlazorInvaders.Tests/TestPlay.cs:20; GameEngineTests.cs:42-55 | B1 survived: `Run` can ignore `hz` and `GameClock_AdvancesInRealTime_AtAnyRefreshRate` still passes for 30, 144 and 240 Hz. At 60 Hz the clock also ends near 1000 ms, so the theory cannot tell a real refresh rate from a fake one. The same code existed at 886d225 in GameEngineTests.cs, so this change did not cause it. It does not serve any AC of this item. | Follow-up item, outside this chore's scope: make the theory check something that depends on the frame rate. For example, assert the number of `Update` calls, or the steps run per frame at 30 Hz (2 per frame). |
| Q3 | nit | docs/work/CHORE-010-deduplicate-test-helpers/implementation/WP1.md (Deviations) | The notes list `Scene_CarriesPendingSounds` as affected by the `RunMs` replacement, but it never calls `Run`. The affected tests are `AlienKill_IsEncodedWithScoreLabelAndRowColour`, `Events_AreSentOnlyOnce` and `Hud_ListsActivePowerUps_AndShieldRing`. | Correct the test names in the notes. |

## QA evidence
Assurance: standard. Quality tools are declined in `.devagents/config.json`: there is no mutation, coverage or complexity tool. No production code changed (`git diff 886d225 --stat -- src` is empty).

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| WP1 step 1: shared `Width`, `Height`, `NewEngine`, `Run` with summaries | TestPlay.cs:8-29 | done; `Run` follows the double math (Q1, recorded deviation) |
| WP1 step 2: BossWave, PowerUp, GameEngine copies removed | BossWaveTests.cs:1-9, PowerUpEngineTests.cs:1-8, GameEngineTests.cs:1-8 | done; `using static` in all five files (one choice, as the plan allows); BossTests `870` changed to `Width` (P4) |
| WP1 step 3: DailyChallenge `Width`/`Height` | DailyChallengeTests.cs:1-9, 147 | done; also `NewEngine()` at :147 (P4) |
| WP1 step 4: SceneBuilder `NewEngine`/`RunMs` | SceneBuilderTests.cs:113,131,169; `RunMs` removed | done; replaced without identical results (Q1) |
| WP1 step 5: verify | my breaks baseline 211/211 pass; plan `rg` empty | done |
| Tests row AC1, AC2: `rg "private (const int (Width\|Height)\|static GameEngine NewEngine\|static float Run)" tests` | run by me: no matches (rc=1) | pass; per-file count of static helper names: each declared once |
| Tests row AC3: 211 tests pass, same as base | `[Fact]/[Theory]/[InlineData]` count 226 at base and now; suite 211 total, all pass in the baseline of `breaks` (B2's run shows Total 211) | pass |
| docs/work/TRACKER.md hunk | PM bookkeeping, no work package | not a finding |

All six test files belong to WP1.

### Coverage and CRAP
quality.coverage is not set up. coverlet.collector is referenced, but no production line changed, so there are no changed lines to cover and no CRAP to score.

### Mutation testing
Did not run: quality.mutation is not set up (quality-tools declined).

### Breaks
| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | Run ignores hz (always 60 Hz frames) | tests/BlazorInvaders.Tests/TestPlay.cs:20 | GameEngineTests | **SURVIVED** (Q2) |
| B2 | Run returns fromMs instead of the last timestamp (chained calls) | tests/BlazorInvaders.Tests/TestPlay.cs:26 | full suite | caught: 1 failed of 211 |
| B3 | Shared Height 500 to 450 | tests/BlazorInvaders.Tests/TestPlay.cs:12 | full suite | **SURVIVED**: not a finding. It is a fixture constant that no assertion depends on, the same as at base, and like the Tester's seed break. |
| B4 | TakeEvents no longer clears (SceneBuilder tests now on shared Run) | src/core/GameEngine.cs:294 | SceneBuilderTests | caught: 1 failed of 18 |
| B5 | First boss attack 150 ms early (BossWave timing on shared Run) | src/core/GameEngine.cs:235 | BossWaveTests | caught: 1 failed of 11 |

My first version of B4 was a no-op (the `Clear` still ran afterwards). I fixed it and ran the file again; the table above is from that run. Breaks file: C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-010-deduplicate-test-helpers\docs\work\CHORE-010-deduplicate-test-helpers\reviews\qa-r1-breaks.json

## Checked and fine
- The test count is unchanged (226 test attributes, 211 tests). No `.skip`, focused or ignored tests were added.
- No test file still declares a copy of `Width`, `Height`, `NewEngine`, `Run` or `RunMs`. The class-specific helpers stayed in their files.
- The removed `Run` copies in BossWave and PowerUp match the shared one apart from the frame math (Q1). The GameEngineTests copy was moved verbatim.
- Line endings: all six files are LF at base and now, so the git CRLF warnings come from autocrlf only.
- Production code is untouched (constraint met).

## Notes for the PM
- test-report.md is the Tester's final response, saved by the PM. I used it only to avoid repeating the Tester's breaks.

Restored: yes

VERDICT: CHANGES_REQUESTED

# QA review: BUG-001 Start screen text off-centre, round 1

**Reviewed:** diff vs base_commit 6879e6d + untracked files
**Reviewer:** Octopus (qa-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | major | `src/core/Rendering/SceneBuilder.cs:283`; `tests/BlazorInvaders.Tests/SceneBuilderTests.cs:458` | AC2 names the game over screen. Its third centred line, "D for today's challenge" (drawn when `TodaysChallenge != null`), is never asserted, because `GameOverScreen_IsCentred` does not set `TodaysChallenge`. Break B1 moved that line to `w / 2 + 30`, and the full suite still passed. The code it changed serves AC2, which is about exactly this centring, so it matters. | Set `TodaysChallenge` in `GameOverScreen_IsCentred` (or a separate case) and add `AssertCentred(scene, width, "D for today's challenge")`. |
| Q2 | major | `src/core/Rendering/SceneBuilder.cs:312`; `tests/BlazorInvaders.Tests/SceneBuilderTests.cs:358` | AC2 names the leaderboard. Its daily title, "TODAY'S CHALLENGE" (shown when `engine.IsDaily`), is never asserted, because the leaderboard tests only reach the "TOP SCORES" title. Break B2 moved the daily title only to `w / 2 + 30`, and the full suite still passed. The code serves AC2, so it matters. | Add a leaderboard case on an engine whose last game was daily, so `IsDaily` is true, and assert `AssertCentred(scene, width, "TODAY'S CHALLENGE")`. |

## QA evidence
Assurance: standard. No production code changed. `quality` is declined in `.devagents/config.json`, so no coverage, complexity or mutation tool is set up.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| WP1 step 1: add centring tests with existing helpers | `SceneBuilderTests.cs:38-70` (helpers), `:322-496` (14 tests, 30 cases) | done |
| WP1 step 2: `dotnet test` passes, no production change | `breaks` baseline run of the full suite passed; the diff touches only the test file | done |
| Tests row AC1/AC2/AC3: start (default, touch, daily), pause, game over, wave/boss banner, PERFECT, "You were hit!", leaderboard; x == Width/2 with centre flag, widths 870 and 801 | `StartScreen_LinesAreCentred_WithUnchangedTextFontAndY` :329, `ScoreTable_TitleIsCentred` :348, `Leaderboard_LinesAreCentred` :358, `EmptyLeaderboard_SaysNoScoresYet_Centred` :373, `DailyAttractCard_LinesAreCentred` ~:388, `WaveBanner_IsCentred` :404, `BossBanner_IsCentred` :415, `DailyBanner_AndHudTag_AreCentred` :426, `PauseOverlay_IsCentred` :441, `GameOverScreen_IsCentred` :458, `YouWereHit_IsCentred` :474, `PerfectBonus_IsCentred` :487 | partial: game-over daily hint (Q1) and daily leaderboard title (Q2) not checked |
| Tests row AC4: start-screen text, font, y 160/220/265 | `StartScreen_LinesAreCentred_WithUnchangedTextFontAndY` :329 (desktop and touch) | done |

Changed files: only `tests/BlazorInvaders.Tests/SceneBuilderTests.cs`, which WP1 owns. No unplanned hunks.

### Coverage and CRAP
Not applicable: no coverage tool is set up (`quality` declined), and no production line changed.

### Mutation testing
Not run: no mutation framework is set up, and no production code changed.

### Breaks
Command: `node .devagents/devagents.mjs breaks BUG-001-start-screen-text-off-centre <item>/reviews/qa-r1-breaks.json`. B1 and B2 were run first with the SceneBuilderTests filter, then again with the full suite. Both runs gave the same result.

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | AC2: game-over daily hint off-centre | src/core/Rendering/SceneBuilder.cs:283 | `dotnet test tests/BlazorInvaders.Tests --nologo` | **SURVIVED** |
| B2 | AC2: daily leaderboard title off-centre | src/core/Rendering/SceneBuilder.cs:312 | `dotnet test tests/BlazorInvaders.Tests --nologo` | **SURVIVED** |
| B3 | AC4: touch start line font 26 to 28 | src/core/Rendering/SceneBuilder.cs:326 | `... --filter FullyQualifiedName~SceneBuilderTests` | caught: StartScreen_LinesAreCentred_WithUnchangedTextFontAndY |
| B4 | AC2: daily HUD tag x off by 1 design px | src/core/Rendering/SceneBuilder.cs:240 | `... --filter FullyQualifiedName~SceneBuilderTests` | caught: DailyBanner_AndHudTag_AreCentred(870) |
| B5 | AC2: leaderboard rows drift right per row | src/core/Rendering/SceneBuilder.cs:321 | `... --filter FullyQualifiedName~SceneBuilderTests` | caught: Leaderboard_LinesAreCentred(801) |

## Checked and fine
- All new tests call the real `SceneBuilder` and `GameEngine`, and assert on the decoded text commands (anchor x, centre flag, y, font). No mocks, no `.skip` or focused tests, and no swallowed exceptions.
- `AssertCentred` uses `Assert.Single`, so a missing line or a duplicate fails the test.
- `IdleUntil` has a cap at 180 s, and each caller guards it with `Assert.True(engine.GameOver / LostALife)`, so a test cannot pass vacuously.
- The 801 cases pin integer division at an odd width, as AC1 asks.
- The power-up letters (`SceneBuilder.cs:231`) are left out on purpose (P5). They are not messages.
- The browser side (`game.js:210`, `textAlign = 'center'`) is not covered by C# tests. The plan accepts this, so it is not a finding.

Restored: yes

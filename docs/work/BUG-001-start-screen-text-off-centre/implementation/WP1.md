# Implementation: BUG-001 Start screen text off-centre — WP1

## Round 1
**Plan steps covered:** WP1 steps 1, 2
**Addresses findings:** P5 (fixed: the centring tests name the message lines and do not assert on every centre-flag command, so the power-up letters at 10 px are not checked)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` | modified — adds helpers `TextCommands`, `AssertCentred`, `EngineOfWidth`, `AttractState`, `IdleUntil` and 14 centring tests (30 cases) for the start screen, attract cards, leaderboard, daily, wave/boss banners, PERFECT, pause, game over and "You were hit!" at widths 870 and 801 |

### Deviations from plan
None. No production code, no JS, no new dependency.

### Findings with no change, and disputed findings
- P5: fixed as the plan's first option. `AssertCentred` is called on named lines only; no assertion covers every centre-flag command.

### Minor findings not fixed
none

### Commands run
- `dotnet test tests/BlazorInvaders.Tests --filter FullyQualifiedName~SceneBuilderTests` → Passed 48/48
- `dotnet test tests/BlazorInvaders.Tests` → Passed 241/241
- `dotnet format whitespace tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj --include tests/BlazorInvaders.Tests/SceneBuilderTests.cs --verify-no-changes` → no changes needed
- `dotnet format style ... --include ... --verify-no-changes` → no changes needed

## Round 2
**Plan steps covered:** WP1 steps 1, 2 (fixes only)
**Addresses findings:** Q1 (fixed), C2 (fixed), C3 (fixed); Q2 (disputed); C1 (not fixed)
**Disputes findings:** Q2

### Files changed
| File | Change |
|------|--------|
| `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` | modified: `AssertCentred` returns the line, used by the start-screen test (C2); the `IdleUntil` doc comment wording (C3); new `GameOverScreen_DailyHint_IsCentred` asserts "D for today's challenge" (Q1) |

### Deviations from plan
None.

### Findings addressed
- Q1: fixed. `GameOverScreen_DailyHint_IsCentred` sets `TodaysChallenge` and asserts the daily hint at width/2 (870, 801).
- C2: fixed. `AssertCentred` returns the decoded line; the start-screen test asserts Y and font on it.
- C3: fixed. Doc comment now says one 60 Hz frame (about 16.7 ms).
- C1: not fixed. The fix moves `IdleUntil` to `TestPlay.cs`, which WP1 does not own. Left for the PM to decide; the duplicated loop is three lines.

### Findings disputed
- Q2: the required change cannot be made as written, because the state it asks for is unreachable. The attract screen draws the leaderboard title with `engine.IsDaily` (`SceneBuilder.cs:312`), and the attract screen is shown only when `!engine.Started && !engine.GameOver` (`SceneBuilder.cs:94-97`). `IsDaily` is set only by `GameEngine.Start` (`GameEngine.cs:182`), which sets `Started = true` (`GameEngine.cs:203`). The only way out of a started game is `EndGame` (`GameEngine.cs:401-404`), which sets `GameOver = true` and `Started = false`. So a daily game always ends on the game-over screen, and no public call leads to an attract screen with `Daily` set. I tried the test (`engine.Start(0, daily: ...)` then attract state 1) and it failed with "The collection did not contain any matching items": the scene had no "TODAY'S CHALLENGE" line. I removed it rather than set engine state by reflection. The branch is dead under the current engine, so centring cannot be tested through the public API. Options for the PM: (1) accept the gap, since the same centre formula is pinned by the other leaderboard and banner tests; (2) add a small engine seam to reach the state, which is a production change and needs a plan step; (3) remove the unreachable branch in a follow-up.

### Commands run
- `dotnet format whitespace ... --include tests/BlazorInvaders.Tests/SceneBuilderTests.cs --verify-no-changes` → no changes
- `dotnet format style ... --verify-no-changes` → no changes
- `dotnet test tests/BlazorInvaders.Tests --nologo` → Passed 243/243

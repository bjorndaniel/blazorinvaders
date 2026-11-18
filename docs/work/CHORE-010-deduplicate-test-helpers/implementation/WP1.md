# Implementation: CHORE-010 Deduplicate test helpers — WP1

## Round 1
**Plan steps covered:** WP1 steps 1, 2, 3, 4, 5
**Addresses findings:** P1 (fixed: one `Run`, see Deviations), P2 (fixed: check in Verification, pattern is literal and excludes `TestPlay.cs`), P3 (fixed: `using static` in all five files), P4 (fixed: DailyChallenge uses `NewEngine()`; BossWave `870` literals now `Width`), P5 (fixed: `TestPlay` summary widened)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `tests/BlazorInvaders.Tests/TestPlay.cs` | modified: added `Width`, `Height` (public const), `NewEngine()` and `Run(engine, fromMs, durationMs, hz = 60)` with XML summaries; widened class summary |
| `tests/BlazorInvaders.Tests/BossWaveTests.cs` | modified: removed local `Width`, `Height`, `NewEngine`, `Run`; `using static` added; `870` literals use `Width` |
| `tests/BlazorInvaders.Tests/PowerUpEngineTests.cs` | modified: removed local `Width`, `Height`, `NewEngine`, `Run`; `using static` added |
| `tests/BlazorInvaders.Tests/GameEngineTests.cs` | modified: removed local `Width`, `Height`, `NewEngine`, `Run`; `using static` added |
| `tests/BlazorInvaders.Tests/DailyChallengeTests.cs` | modified: removed local `Width`, `Height`; `using static` added; `new GameEngine(Width, Height, new Random(42))` replaced by `NewEngine()` |
| `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` | modified: removed local `NewEngine` (literals 870/500) and `RunMs`; `using static` added; `RunMs(engine, n)` calls are `Run(engine, 0, n)` |

### Deviations from plan
- **Shared `Run` uses the double frame math of GameEngineTests for every caller, not the float `StepMs` loop of BossWave/PowerUp (plan Summary, P1).** The float `StepMs` loop made `GameEngineTests.HoldingRight_MovesPlayerAtConstantSpeed` fail (609 instead of 615: 29 steps instead of 30, because float timestamps accumulate below the engine's step threshold). The double version passes all 211 tests, so one `Run` serves every caller. `hz` stays optional, default 60.
- **`RunMs` replaced by `Run(engine, 0, ms)` without a separate frame-count comparison.** The old loop starts at `StepMs` with accumulated float timestamps and `<= ms`; the new one uses `i * frameMs`. For 500 ms the frame counts may differ (29 vs 30). The three affected tests (`Scene_CarriesPendingSounds`, the combo/events test at line ~131, and the 40 ms power-up test) pass with the replacement, which is the plan's acceptance criterion. Revert these three calls if a frame-exact comparison is wanted.

### Findings with no change, and disputed findings
none

### Minor findings not fixed
none

### Commands run
- `dotnet test` in `tests/BlazorInvaders.Tests` after the change → Passed 211, Failed 0, Total 211 (base count 211).
- Before the double-math change, the float `Run` gave 210 pass, 1 fail (`HoldingRight_MovesPlayerAtConstantSpeed`), see Deviations.
- `dotnet format whitespace tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj --include <the six test files> --verify-no-changes` → exit 0.
- Verification `rg -n "const int (Width|Height)|GameEngine NewEngine\(|static float Run\(|RunMs" tests/BlazorInvaders.Tests --glob '!TestPlay.cs'` → no declarations; only call sites remain.

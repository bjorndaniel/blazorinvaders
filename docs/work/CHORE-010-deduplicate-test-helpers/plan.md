# Plan: CHORE-010 Deduplicate test helpers

| | |
|---|---|
| **ID** | CHORE-010 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-17 |
| **Size** | S — one test project, mechanical moves, no new pattern (extends `TestPlay.cs`) |
| **Security-sensitive** | no — tests only |
| **UI-facing** | no — tests only |
| **Opened** | 2026-11-17 |
| **Plan round** | 1 |

## Summary
Move the helpers copied across test classes into the existing shared `TestPlay.cs` (CG54) and delete the copies. Shared: `Width`/`Height`, `NewEngine()`, and one `Run`. Class-specific helpers stay where they are.
- `Run` uses the `GameEngine.StepMs` loop (identical in BossWave and PowerUp) with an optional `hz = 60` parameter, so GameEngineTests' superset version is kept; if the float rounding changes any result, the Implementer keeps the hz math only for non-default `hz`.
- `Width`/`Height` become `public const` on `TestPlay` (CG12). `DailyChallengeTests` and `SceneBuilderTests` also use them or literals (870, 500) and switch to the constants.
- Tests only; no production change.

## Context found
- `docs/coding-guidelines.md`: CG54 (shared helpers in `TestPlay.cs`, no copied `NewEngine()`, `Width`/`Height`, step loops), CG12 (constants), CG30 (no `!`; existing `engine.Player!` is unchanged).
- Duplicates (tests/BlazorInvaders.Tests): `Width`/`Height` in BossWave, DailyChallenge, GameEngine, PowerUpEngine; `NewEngine()` in BossWave, GameEngine, PowerUpEngine, SceneBuilder (literals 870/500); `Run` in BossWave, GameEngine (with `hz`), PowerUpEngine; `RunMs` in SceneBuilder is the same step loop (starts at StepMs, inclusive `<= ms`).
- Not duplicated, stay: `Hit`/`Miss`, `Play`, `Snapshot`, `With`, `Day`, `Collect`, `RunUntilGameOver`, `Parse`/`ParseOn`, `Texts`, `Arity`.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Shared helpers and removal of copies | S | `TestPlay.cs`, `BossWaveTests.cs`, `DailyChallengeTests.cs`, `GameEngineTests.cs`, `PowerUpEngineTests.cs`, `SceneBuilderTests.cs` | — |

## Steps
### WP1
1. **Add shared helpers** — `TestPlay.cs`: `public const int Width = 870`, `Height = 500`; `NewEngine()` => `new(Width, Height, new Random(42))`; `Run(engine, fromMs, durationMs, hz = 60)` from GameEngineTests with its XML summary. Keep CG60-style `<summary>` on each.
2. **BossWaveTests, PowerUpEngineTests, GameEngineTests** — delete local `Width`, `Height`, `NewEngine`, `Run`; call `TestPlay.NewEngine()`/`TestPlay.Run(...)`. Add `using static BlazorInvaders.Tests.TestPlay;` only if not already the file's style (existing tests call `TestPlay.ChaseAndShootBoss` qualified: stay qualified, or use the using static consistently in all five files, one choice).
3. **DailyChallengeTests** — delete local `Width`/`Height`; use `TestPlay.Width/Height`.
4. **SceneBuilderTests** — delete local `NewEngine`; use shared one. Replace `RunMs(engine, ms)` with `TestPlay.Run(engine, 0, ms)` only if tests still pass with identical results (frame count by rounding vs `<=`); otherwise leave it and say so in the notes.
5. **Verify** — `dotnet test` in the worktree: 211 tests, all pass. Grep that no test file still declares `NewEngine`, `Width`/`Height` constants or `Run(`.

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC1, AC2 | `rg "private (const int (Width\|Height)\|static GameEngine NewEngine\|static float Run)" tests` returns nothing | check | WP1 |
| AC3 | Existing suite: 211 tests pass, same count as base (`dotnet test`) | unit | WP1 |

## Security considerations
None: tests only; no input, data, secrets, dependencies or trust boundaries (`docs/security-model.md` unaffected).

## UX considerations
None: not UI-facing.

## Risks and rollback
- Rounding: the unified `Run` could shift timing in BossWave/PowerUp tests; mitigated by step 2 fallback and the full test run. Rollback: revert the commit.

## Out of scope
Production code; renaming or restructuring class-specific helpers.

## Revisions

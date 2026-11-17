VERDICT: APPROVED

# Code review: CHORE-010 Deduplicate test helpers (round 1)

**Scope:** full diff against 886d225: `TestPlay.cs`, `BossWaveTests.cs`, `DailyChallengeTests.cs`, `GameEngineTests.cs`, `PowerUpEngineTests.cs`, `SceneBuilderTests.cs`.

## Summary
The change does what the plan says. `Width`/`Height`, `NewEngine()` and a single `Run` now live in `TestPlay.cs` (CG54), each with a `<summary>`. No test file keeps its own copy of them. `using static` is used the same way in all five files.

I checked the implementer's declared deviation: the shared `Run` uses double frame math instead of the float `StepMs` loop. It is sound, because it returns the same value as the old BossWave/PowerUp `Run`, which was `fromMs + frames * step`.

Replacing `RunMs` in SceneBuilderTests with `Run(engine, 0, n)` keeps the frame counts:
- **40 ms:** 2 frames in both the old and new loop.
- **500 ms:** the new loop ends exactly at t=500, which matches the `Build(engine, 500)` the tests use.

The helpers that remain are specific to one class and are correctly left in place: `Play`, `RunUntilGameOver`, `Collect`, and the seeded `new GameEngine(Width, Height, new Random(1|2))` engines in DailyChallengeTests. No production code changed. Line endings are CRLF, the same as the untouched files.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | nit | tests/BlazorInvaders.Tests/BossWaveTests.cs:79, tests/BlazorInvaders.Tests/SceneBuilderTests.cs:217 | These files now have `using static BlazorInvaders.Tests.TestPlay;`, but they still call `TestPlay.ChaseAndShootBoss(...)` qualified. Other `TestPlay` members are called unqualified in the same files, so each file mixes two styles. | Optional: call `ChaseAndShootBoss(...)` unqualified to match. |

## Notes for the PM
- The brief contained nothing beyond its template fields and the task.
- I did not re-run `dotnet test`. The tester's run (211/211 passing) and my trace of the frame math are consistent.

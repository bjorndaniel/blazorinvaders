VERDICT: APPROVED

# Code review: BUG-001 Start screen text off-centre, round 1

**Scope:** the full diff against `6879e6d`. Only `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` changed, plus a TRACKER row. There is no production change.

## Summary
The tests work and fit the file's existing style. They reuse `Decode`, `TestPlay.Run`, `ChaseAndShootBoss` and `Height`. I checked the timestamps `AttractState` uses (1, 4002, 8003, 12004) against `SceneBuilder.AttractScreen` (`src/core/Rendering/SceneBuilder.cs:290`, where the state advances when more than 4000 ms have passed). They land on states 0, 1, 2 and 3 as intended. With `TouchControls` off and no daily challenge, state 2 falls to the `default` branch, which is the desktop start screen. The expected texts, fonts and y values match `SceneBuilder.cs:324-341`. The tests are deterministic (seeded engine, injected timestamps, as CG51 requires). I re-ran `dotnet test --filter FullyQualifiedName~SceneBuilderTests` and all 48 passed.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | minor | tests/BlazorInvaders.Tests/SceneBuilderTests.cs:64-70 | `IdleUntil` is a second copy of the idle step loop in `GameEngineTests.RunUntilGameOver` (`tests/BlazorInvaders.Tests/GameEngineTests.cs:9-14`): the same `while (!done && t < 180_000) t = Run(...)`. CG54 (should) says step loops are not copied between classes. | Move `IdleUntil` to `TestPlay.cs` and have `RunUntilGameOver` call it. If that is out of scope here, because WP1 owns only this file, say so in the implementation notes. |
| C2 | nit | tests/BlazorInvaders.Tests/SceneBuilderTests.cs:340-342 | Each start-screen line is decoded twice: once inside `AssertCentred` and again by `TextCommands(scene).Single(...)`. | Optional: have `AssertCentred` return the line and assert Y and Font on what it returns. |
| C3 | nit | tests/BlazorInvaders.Tests/SceneBuilderTests.cs:63 | The doc comment says "16 ms frames". `Run(engine, t, 16)` at the default 60 Hz actually advances one frame of about 16.7 ms. | Optional: change the wording to "one 60 Hz frame at a time". |

## Notes for the PM
- The new tests check that centred text is placed at `width / 2`, which is the same formula `CenteredText` uses. So they catch a change to the centre anchor or a lost centre flag, but not font-metric errors in the browser. The plan says so (real rendering is left to QA) and states the bug is already fixed on this branch.
- The brief held only template fields and the task. Nothing in it or in the item files told me what to find or which verdict to give.

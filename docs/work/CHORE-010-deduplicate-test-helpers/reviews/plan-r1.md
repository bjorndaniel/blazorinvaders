VERDICT: APPROVED

# Plan review: CHORE-010 Deduplicate test helpers, round 1

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | minor | Summary bullet 1 vs Step 1 | The Summary says the shared `Run` uses the float `GameEngine.StepMs` loop from BossWave/PowerUp and adds `hz` math only for non-default `hz`. Step 1 says to copy GameEngineTests' version (`frameMs = 1000.0 / hz` as a double, `(float)(fromMs + i * frameMs)`). For hz=60 the two produce timestamps that can differ by a float ulp at exact step boundaries, so BossWave/PowerUp tests may see different timestamps. The tests decide, but the two sections tell the implementer two different things. | Pick one in Step 1 (recommended: the Summary's version, a StepMs loop when `hz == 60` and the double `hz` math otherwise, which keeps every current caller's timestamps exactly as they are). |
| P2 | minor | Tests, AC1/AC2 check | Inside the markdown table the regex is written `(Width\|Height)` and `NewEngine\|static float Run`. Copied from the raw file, ripgrep reads `\|` as a literal pipe, so the pattern never matches and the check passes even if copies remain. | Write the check in Step 5 outside the table, e.g. `rg -n "const int (Width|Height)|GameEngine NewEngine\(|static float Run\(" tests/BlazorInvaders.Tests --glob '!TestPlay.cs'`. Leaving out `private` also catches a copy with a different access modifier. |
| P3 | minor | Step 2 / Size | Step 2 lets the implementer choose between qualified calls and `using static`. The five files have about 120 call-site lines (`NewEngine()`, `Run(`, `Width`, `Height`: 19+49+21+19+15). If every call is qualified, the diff is about 200 lines over 6 files, which is beyond S as defined (about 3 files and 100 lines). With `using static BlazorInvaders.Tests.TestPlay;` it is about 75 lines. | Prescribe `using static` in all five files, which keeps the item S and the call sites unchanged. Otherwise set Size to M. |
| P4 | nit | Step 3 | `DailyChallengeTests.cs:148` `new GameEngine(Width, Height, new Random(42))` is exactly `NewEngine()`. `BossWaveTests.cs:166-167` uses the literal `870` for the play-field width. | Optionally use `NewEngine()` and `Width` there. Not required by the ACs. |
| P5 | nit | Step 1 | The `TestPlay` class summary ("Scripted play shared by tests.") no longer covers constants and engine construction. | Widen the summary, e.g. "Engine setup and scripted play shared by tests." |

## Attacks the plan survived
- Missed duplicates: I listed every private helper and constant in the 8 test files myself. The plan's shared and stay-put lists match what I found; `Play`, `Collect`, `RunUntilGameOver`, `Snapshot`, `Decode`/`Texts`, `Hit`/`Miss` and `Parse`/`ParseOn` each exist in one class only.
- Run signature merge: `hz` is used only by `GameEngineTests.cs:70` (Theory 30/60/144/240). Every other call uses three arguments, so the optional parameter breaks no caller, and the return value (last timestamp, or `fromMs` when there are 0 frames) is the same in all three copies.
- SceneBuilder `RunMs` replacement: it has three callers (500, 500, 40). For 40 both loops give 2 frames. For 500, float accumulation can give 29 or 30 frames against 30 from rounding. Step 4 makes the replacement conditional on identical results, and `RunMs` exists in one class only, so keeping it does not violate AC2 or CG54.
- Name collisions under `using static`: no test class has another member named `Width`, `Height`, `NewEngine` or `Run`, and `Boss.Size.Width` is accessed through a member, so it does not collide.
- Accessibility: `public const` and `public static` members on the `internal static class TestPlay` work from the public test classes in the same assembly.
- AC3 count: 151 `[Fact]` plus 60 `[InlineData]` rows makes 211, matching the request. No `MemberData`, so the count is stable.
- Scope and classification: only files under `tests/` change, no production code, no dependency. Security-sensitive `no` and UI-facing `no` are right. One WP owns all 6 files.
- Guidelines: the plan follows CG54 (shared helpers in `TestPlay.cs`) and CG12. CG60 (XML summaries) applies to core only, and the plan keeps the summaries anyway.

## Checked and fine
- `GameEngine.StepMs = 1000f / 60f` (src/core/GameEngine.cs:12) is what all the loops use.
- No existing `using static` in the tests. The only existing call to TestPlay is qualified (`TestPlay.ChaseAndShootBoss`).
- The test project compiles all `*.cs` by default, so no csproj change is needed.

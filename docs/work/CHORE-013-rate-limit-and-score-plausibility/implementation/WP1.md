# Implementation: CHORE-013 Rate limiting and score plausibility check — WP1

## Round 1
**Plan steps covered:** WP1 steps 1-12
**Addresses findings:** P7 (minor, plan-r3): the limiter's last-prune time starts at `DateTimeOffset.MinValue` and only the times passed to `TryAcquire`/`Prune` are compared, so the inline-prune test does not depend on the real clock.
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `.github/scripts/validate-dispatch-inputs.sh` | modified: `MAX_INSTANCES`, `DAILY_QUOTA_GB_SECONDS` patterns and rules; `RESOURCE_GROUP` rule text fixed |
| `.github/scripts/validate-dispatch-inputs.test.sh` | modified: new cases, `env -u` list, `rg-` regression |
| `.github/workflows/provision.yml` | modified: two inputs, env, validation, scale limit and quota step, summary |
| `src/api/ScoreRateLimiter.cs` | added: the limiter |
| `src/api/SaveHighScore.cs` | modified: limiter check first, 429 with `Retry-After` |
| `src/api/Program.cs` | modified: registers the limiter |
| `src/api/HighScoreValidator.cs` | modified: `Wave`, multiple-of-10 and `MaxPlausibleScore` |
| `src/core/GameEngine.cs` | modified: four scoring members `internal` for the drift test |
| `src/core/GameObjects/HighScore.cs` | modified: `Wave`, `ShouldRetrySend` |
| `src/Pages/Index.razor` | modified: sends `Wave`, queues on 429/503/403 |
| `README.md` | modified: "Abuse and cost limits" |
| `docs/security-model.md` | modified: TB2, TB8, T1, T2, T4, assumptions, revision |
| `docs/architecture.md` | modified: A2, A5, A7, A11, A20, A23, decision |
| `tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj` | modified: compiles in the limiter |
| `tests/BlazorInvaders.Tests/HighScoreValidatorTests.cs` | modified: `wave` in fixtures, new rules |
| `tests/BlazorInvaders.Tests/ScoreRateLimiterTests.cs` | added |
| `tests/BlazorInvaders.Tests/ScorePlausibilityTests.cs` | added |
| `tests/BlazorInvaders.Tests/HighScoreTests.cs` | added |

### Deviations from plan
- `SaveHighScore` also takes `TimeProvider` (already registered) to pass `now` to the limiter; the plan named only the limiter.
- `MaxScore_IsAccepted` uses 999,990 at wave 30 rather than the first wave that reaches it.
- `MaxPlausibleScore_GrowsWithWave` checks waves 1-6, as the bound stops growing past `MaxScore` (early exit).

### Commands run
- `dotnet test tests/BlazorInvaders.Tests` -> 254 passed
- `dotnet build src/BlazorInvaders.sln` -> 0 warnings, 0 errors
- `dotnet format src/BlazorInvaders.sln --verify-no-changes` -> clean
- `bash .github/scripts/validate-dispatch-inputs.test.sh` -> 57 passed, 0 failed

## Round 2
**Plan steps covered:** fixes for qa-r1 Q7, Q8; code-r1 C1 (= Q7), C2, C3; security-r1 S1, S2 (= Q7)
**Addresses findings:** Q7, C1, S2 (fixed: `TryAcquire` reports the counter used, `RateBucket` client/unknown/overflow, and the 429 warning logs it, never the address or key); S1 (fixed: T2 says a full table shares one bucket and can delay everyone for up to an hour); C2 (fixed: TB2 "What crosses it" lists `wave` and the two headers); C3 (fixed: `AlienWaveMax` is a `const`, `var boss`); Q8 (fixed: `BossWaveMax` is `internal` and the test pins 7,500 and 10,500, with a comment that the alien term wins at these waves; the vacuous wave assertion is removed)
**Disputes findings:** none
**Not in this round (Tester):** Q1-Q6

### Files changed
| File | Change |
|------|--------|
| `src/api/ScoreRateLimiter.cs` | modified: `RateBucket` enum; `TryAcquire` takes `out RateBucket bucket` |
| `src/api/SaveHighScore.cs` | modified: 429 warning logs `{Bucket}` |
| `src/api/HighScoreValidator.cs` | modified: `AlienWaveMax` const; `BossWaveMax` internal; `var boss` |
| `tests/BlazorInvaders.Tests/ScoreRateLimiterTests.cs` | modified: call sites take the extra `out` |
| `tests/BlazorInvaders.Tests/ScorePlausibilityTests.cs` | modified: `MaxPlausibleScore_BossWaveAtLeastBossMax` pins `BossWaveMax` |
| `docs/security-model.md` | modified: TB2 cell (C2), T2 (S1) |

### Deviations from plan
- `TryAcquire` has a fourth `out` parameter (`RateBucket`) so the warning can name the bucket (plan Risks). It is internal to the API project's limiter and its tests; no public API outside the project changes.

### Commands run
- `dotnet format src/BlazorInvaders.sln --verify-no-changes --include <the 5 .cs files>` -> clean
- `dotnet test tests/BlazorInvaders.Tests` -> 254 passed
- `dotnet build src/api` -> 0 warnings, 0 errors

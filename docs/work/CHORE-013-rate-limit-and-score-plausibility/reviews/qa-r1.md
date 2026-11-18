VERDICT: CHANGES_REQUESTED

# QA review: CHORE-013 Rate limiting and score plausibility check, round 1

**Reviewed:** diff vs base_commit feee1ce + untracked files
**Reviewer:** Octopus (QA Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | major | src/api/ScoreRateLimiter.cs:97 | Nothing pins the IPv6 prefix at /64 (AC2, T2, a security control). Break Q-B1 (`Array.Clear(bytes, 8, 8)` changed to `(bytes, 12, 4)`, which masks /96) survived. `ClientKey_Normalizes` compares `::a`/`::b`, which differ only in the last byte, and `1:2::`/`1:3::`, which differ in byte 7. | In `ClientKey_Normalizes`, assert that two addresses in the same /64 but differing in bytes 8-11 get the same key, e.g. `2001:db8:1:2::a` = `2001:db8:1:2:ffff:ffff::a`. |
| Q2 | major | src/api/HighScoreValidator.cs:108 | The early exit `total <= MaxScore` is the only thing that stops a CPU DoS at TB2. Without it, `"wave":2147483647` loops forever: `w++` wraps around, so `w <= int.MaxValue` stays true. Break Q-B2 removed it and survived, because no test sends a huge wave. | Add a validator test: `{"score":999990,"wave":2147483647}` is accepted and returns (for example within an xUnit `Timeout`, or by asserting `MaxPlausibleScore(int.MaxValue, …)` returns a value just past `MaxScore`). |
| Q3 | major | src/api/ScoreRateLimiter.cs:56 | AC2/D1 expiry without traffic depends on the timer, and no test drives it. Break Q-B3 (timer callback made a no-op) survived. Coverage marks the line as covered, but the lambda never runs. | Add a test with a hand-written `TimeProvider` whose `CreateTimer` captures the callback (the same pattern as `ManualTime` in `LegacyTopCacheTests`). Post at T0, advance past `Window` with no `TryAcquire`, fire the callback, and assert `TrackedClients == 0`. Also assert the due time and period equal `Window`. |
| Q4 | major | src/api/ScoreRateLimiter.cs:140,154 | The inline-prune throttle (P6; the plan's Security considerations, "scanned inline at most once a minute") is untested. Break Q-B5 (`_lastPrune` never advances, so every request to a full table runs a scan) survived. | Add a test with cap 3 and 3 clients at T0. A new address at T0+59m30s goes to overflow. A new address at T0+60m is still not tracked, because no prune is due yet (`TrackedClients` stays 4). |
| Q5 | major | tests/BlazorInvaders.Tests/HighScoreValidatorTests.cs `AllTimeScore_IsBoundedByWave`, `DailyScore_AllowsDoubleTheAllTimeBound` | The AC3 bound's value is never pinned. Both tests take their expected value from `MaxPlausibleScore` itself (a tautology). Break Q-B6 (the loop sums one wave too many, `w <= wave + 1`) and break Q-B8 (`MaxMotherShipsPerWave` 200 changed to 150) both survived, so the bound can grow or shrink with no test failing. | Assert literal values derived from the plan's formula: `MaxPlausibleScore(1,false) == 64_460`, `MaxPlausibleScore(2,false) == 128_920`, `MaxPlausibleScore(1,true) == 128_920`. Accept 64,460 and reject 64,470 at wave 1. |
| Q6 | major | src/core/GameObjects/HighScore.cs:24-27 | The `"wave"` JSON contract between client and API is not executed by any test: coverage shows line 27 at 0 hits. Renaming or dropping the property would make every client score a 400 (AC3; data sent). | In `HighScoreTests`, serialize `new HighScore{Name="ABC",Score=100,Wave=3}` and assert it contains `"wave":3`, and that `wave` is omitted when it is 0. Better: feed the serialized bytes to `HighScoreValidator.TryParse` and assert it is accepted with `Wave == 3`. |
| Q7 | major | src/api/SaveHighScore.cs:19 | This deviates from the plan and is not recorded. Plan **Risks** says the 429 warning "names the bucket type, never the address" so the owner can watch for unknown-bucket rejections (the main deployment risk). The warning is a fixed "too many posts from one client" for the client, unknown and overflow buckets alike. The deviation is not in WP1.md. | Return the bucket kind from `TryAcquire` (client / unknown / overflow) and log it as a structured field, still without the address or key, or record and justify the deviation. |
| Q8 | nit | src/api/HighScoreValidator.cs:194-216; tests/BlazorInvaders.Tests/ScorePlausibilityTests.cs `MaxPlausibleScore_BossWaveAtLeastBossMax` | The boss term never wins below the early exit. `BossWaveMax(k) = 4500 + 3000k` stays under the alien max of 64,460 until k > 20. So break Q-B7 is equivalent, and `_BossWaveAtLeastBossMax` would pass with the boss code deleted. | Add a comment that the boss term matters only if the mothership allowance drops, or have the test call `BossWaveMax` directly (make it internal) and assert 7,500 for k=1. |

## QA evidence
Assurance: standard. No `quality` tools are configured (`"declined": ["quality-tools"]` in `.devagents/config.json`), so there was no mutation run and no `crap`. Without a framework, the limit for this M item is 10 breaks by hand. I ran 9.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| 1 Input validation | .github/scripts/validate-dispatch-inputs.sh:15-16,19,25-26; validate-dispatch-inputs.test.sh:17,78-87 | done |
| 2 Rate limiter | src/api/ScoreRateLimiter.cs (constant named `DefaultMaxTrackedClients`) | done |
| 3 Wire it | src/api/Program.cs:12; src/api/SaveHighScore.cs:9,15-24,49-50 (also takes `TimeProvider`, recorded in WP1.md) | done; warning wording deviates (Q7) |
| 4 Plausibility | src/api/HighScoreValidator.cs:9,19-37,93-122,184-188,206-210 | done |
| 5 Engine internals | src/core/GameEngine.cs:20-23,38-39,442-443 | done |
| 6 Client DTO | src/core/GameObjects/HighScore.cs:24-35 | done |
| 7 Client | src/Pages/Index.razor:244,269-275 | done (not compiled by tests; A5 shell) |
| 8 Provisioning | .github/workflows/provision.yml:30-37,52-53,60,123-133,217-218 | done |
| 9 README | README.md "Abuse and cost limits" | done |
| 10 Security model | docs/security-model.md TB2, TB8, T1, T2, T4, Assumptions, Revisions | done |
| 11 Architecture | docs/architecture.md A2, A5, A7, A11, A20, A23, Decisions | done |
| 12 Tests and csproj | tests/…/BlazorInvaders.Tests.csproj:33; fixtures `"wave":1`; `MaxScore_IsAccepted` uses wave 30 (recorded) | done |
| T: TryAcquire_PastLimit_RejectsWithRetryAfter | ScoreRateLimiterTests | present, checks the row |
| T: TryAcquire_WindowEnds_AllowsAgain, _OtherClient_IsIndependent | ScoreRateLimiterTests | present |
| T: TryAcquire_TableFull_UsesSharedOverflowBucket | ScoreRateLimiterTests | present |
| T: TryAcquire_TableFullOfEndedWindows_PrunesInline | ScoreRateLimiterTests | present |
| T: ClientKey_IsSaltedHash | ScoreRateLimiterTests | present |
| T: ClientKey_Normalizes | ScoreRateLimiterTests | present; does not pin /64 (Q1) |
| T: ClientAddress_PrefersXClientIp_ElseRightmostForwardedFor | ScoreRateLimiterTests | present |
| T: Prune_RemovesEndedWindowsOnly | ScoreRateLimiterTests | present |
| T: HighScoreValidatorTests wave/multiple-of-10/bound/daily | MissingOrInvalidWave_IsRejected, ScoreNotMultipleOf10_IsRejected, AllTimeScore_IsBoundedByWave, DailyScore_AllowsDoubleTheAllTimeBound | present; expected values tautological (Q5) |
| T: ValidatorScoring_MatchesEngine | ScorePlausibilityTests | present, checks the row |
| T: MaxPlausibleScore_GrowsWithWave, _BossWaveAtLeastBossMax | ScorePlausibilityTests | present (waves 1-6, recorded); boss test vacuous (Q8) |
| T: HighScoreTests.ShouldRetrySend | HighScoreTests.ShouldRetrySend_OnlyForTemporaryRefusals | present |
| T: validate-dispatch-inputs cases + `rg-` | validate-dispatch-inputs.test.sh:78-87 | present; 57 passed, 0 failed (scratch run) |
| T: az step / forged-header check / AC6 docs | manual, review | not automatable; docs checked |

All changed files belong to WP1. No unplanned hunks.

### Coverage and CRAP
I used coverlet.collector (already in the test project) through `scratch -- dotnet test tests/BlazorInvaders.Tests --collect:"XPlat Code Coverage" -- …IncludeTestAssembly=true`. Changed lines that no test executes:
- src/api/ScoreRateLimiter.cs:46-48: the public DI constructor (random salt).
- src/api/ScoreRateLimiter.cs:113: `[` without a closing `]` returns false.
- src/core/GameObjects/HighScore.cs:27: the `Wave` property (Q6).
- The timer lambda at ScoreRateLimiter.cs:56 never runs, although the line counts as covered (Q3).

There is no complexity tool, so no CRAP scores.

### Mutation testing
Not run: no mutation framework is configured (`mutate --status`: no run yet).

### Breaks
File: .work/CHORE-013-rate-limit-and-score-plausibility/docs/work/CHORE-013-rate-limit-and-score-plausibility/reviews/qa-r1-breaks.json. Tests run for every break: `dotnet test tests/BlazorInvaders.Tests --nologo`.

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| Q-B1 | AC2/T2 IPv6 mask /64 to /96 | src/api/ScoreRateLimiter.cs:97 | dotnet test | **SURVIVED** (Q1) |
| Q-B2 | TB2 CPU: early exit removed | src/api/HighScoreValidator.cs:108 | dotnet test | **SURVIVED** (Q2) |
| Q-B3 | AC2 expiry timer does nothing | src/api/ScoreRateLimiter.cs:56 | dotnet test | **SURVIVED** (Q3) |
| Q-B4 | AC1 inline prune never runs | src/api/ScoreRateLimiter.cs:154 | dotnet test | caught: TryAcquire_TableFullOfEndedWindows_PrunesInline |
| Q-B5 | P6 inline-prune throttle off | src/api/ScoreRateLimiter.cs:140 | dotnet test | **SURVIVED** (Q4) |
| Q-B6 | AC3 bound sums one wave too many | src/api/HighScoreValidator.cs:108 | dotnet test | **SURVIVED** (Q5) |
| Q-B7 | AC3 Boss Rush branch dropped | src/api/HighScoreValidator.cs:115 | dotnet test | SURVIVED, equivalent: the boss term never exceeds the alien max before the early exit (Q8) |
| Q-B8 | AC3 mothership allowance 200 to 150 | src/api/HighScoreValidator.cs:26 | dotnet test | **SURVIVED** (Q5) |
| Q-B9 | AC2 prune boundary >= to > | src/api/ScoreRateLimiter.cs:138 | dotnet test | caught: Prune_RemovesEndedWindowsOnly |

## Checked and fine
- Suite in the scratch copy: 254/254 passed. Script tests: 57 passed, 0 failed.
- Every score award is a multiple of 10 (`Award` multiplies by 1 or 2; the base values are 10-300). `Wave` at game over is at least the last wave scored.
- The limiter runs before the body is read. `Retry-After` is rounded up. The log line contains no IP or key.
- The overflow and unknown keys cannot collide with the 64-hex HMAC keys. The salt is never logged.
- `provision.yml`: the inputs are validated before login and passed only as quoted `"$VAR"` through `env:`.
- Tester breaks B1-B9 cover the limit boundary, window end, overflow, wave, multiple of 10, bound boundary, daily factor and the 429/403 retry. I did not repeat them.
- The `Index.razor` retry wiring is not compiled by any test (A5 thin shell, as planned). `ShouldRetrySend` carries the rule.
- AC6: T1, T2 and T4 are set to partly, with what remains open. TB8 lists both new inputs.

Restored: yes

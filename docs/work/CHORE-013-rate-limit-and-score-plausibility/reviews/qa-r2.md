VERDICT: APPROVED

# QA review: CHORE-013 Rate limiting and score plausibility check, round 2

**Reviewed:** changes since round 1 (`changes CHORE-013-rate-limit-and-score-plausibility --since 1`): src/api/ScoreRateLimiter.cs, src/api/SaveHighScore.cs, src/api/HighScoreValidator.cs, docs/security-model.md, and four test files
**Reviewer:** Octopus (QA Reviewer)

## Earlier findings
| ID | Status | Evidence |
|----|--------|----------|
| Q1 | resolved | `ClientKey_Normalizes` now checks that `2001:db8:1:2::a` and `2001:db8:1:2:ffff:ffff::a` get the same key. Break Q-B1 (mask /96) is now caught by it. |
| Q2 | resolved | `HighScoreValidatorTests.HugeWave_ReturnsQuickly` calls `MaxPlausibleScore(int.MaxValue,false)` with a 5 s `WaitAsync` and asserts the result is above `MaxScore`, then accepts wave 2147483647. Break Q-B2 (early exit removed) is now caught by it. |
| Q3 | resolved | `Timer_DropsEndedWindows_WithoutTraffic` uses a `TimeProvider` that captures the timer callback. It asserts the due time and period both equal `Window`, fires the callback after the window with no `TryAcquire`, and asserts `TrackedClients == 0`. Break Q-B3 (callback does nothing) and my new break Q-B11 (timer fires only once) are both caught by it. |
| Q4 | resolved | `TryAcquire_FullTableScan_IsThrottledToOncePerMinute` follows the requested scenario: a scan at T0+59m30s, then a new address at T0+60m with `TrackedClients` still 4. Break Q-B5 (throttle off) is now caught by it. |
| Q5 | resolved | `MaxPlausibleScore_PinnedValues` asserts the literals 64,460, 128,920 and 128,920, accepts a score of 64,460 at wave 1 and rejects 64,470. Breaks Q-B6 and Q-B8 are both caught by it. |
| Q6 | resolved | `HighScoreTests.Wave_IsSerializedAndAcceptedByValidator` serializes a real `HighScore`, asserts the output contains `"wave":3`, and feeds the bytes to `HighScoreValidator.TryParse` (accepted, `Wave == 3`). `Wave_IsOmittedWhenZero` covers the omitted case. |
| Q7 | resolved | `TryAcquire` now returns a `RateBucket` (Client, Unknown or Overflow) in an extra `out` parameter (ScoreRateLimiter.cs:150-164). SaveHighScore.cs:20 logs it as the structured field `{Bucket}`, without the address or key. The signature change is recorded under Deviations in WP1.md round 2. `TryAcquire_ReportsBucket` asserts all three values, and my break Q-B10 is caught by it. |
| Q8 | resolved | `BossWaveMax` is now `internal`, and the test pins 7,500 for boss 1 and 10,500 for boss 2, with a comment that the alien term wins at these waves. The vacuous assertion is gone. Q-B7 is still equivalent at the waves the bound reaches, so I did not run it again. |

## Findings
None.

## QA evidence
Assurance: standard. No `quality` tools are configured, so there was no mutation run and no `crap`. Production code changed this round, so I traced and broke only the changed code, and ran each round 1 survivor again. Round 1 covered the traceability, coverage and breaks of the unchanged code.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| 3 Wire it: the 429 warning names the bucket type (plan Risks) | src/api/SaveHighScore.cs:17-20; src/api/ScoreRateLimiter.cs:16-17, 150-164 | done (Q7). The `out RateBucket` is recorded in WP1.md. |
| 4 Plausibility: `AlienWaveMax` is now a const, `BossWaveMax` is internal, `var boss` | src/api/HighScoreValidator.cs:93-99, 112 | refactor that serves code-r1 C3 and qa-r1 Q8; no change in behaviour |
| 10 Security model: TB2 lists `wave` and the two headers (C2); T2 records the table-fill lockout (S1) | docs/security-model.md TB2, T2 | done |
| T: ClientKey_Normalizes (/64) | ScoreRateLimiterTests.ClientKey_Normalizes | checks the row (Q1) |
| T: expiry without traffic (D1) | ScoreRateLimiterTests.Timer_DropsEndedWindows_WithoutTraffic | added (Q3) |
| T: inline prune throttled (P6) | ScoreRateLimiterTests.TryAcquire_FullTableScan_IsThrottledToOncePerMinute | added (Q4) |
| T: HighScoreValidatorTests bound | MaxPlausibleScore_PinnedValues, HugeWave_ReturnsQuickly | literal values; no longer tautological (Q2, Q5) |
| T: MaxPlausibleScore_BossWaveAtLeastBossMax | ScorePlausibilityTests | pins `BossWaveMax` directly (Q8) |
| T: HighScoreTests | Wave_IsSerializedAndAcceptedByValidator, Wave_IsOmittedWhenZero | added (Q6) |

All changed files still belong to WP1. There are no unplanned hunks. No test is skipped or focused (grep for `.skip`, `Skip =` and `.only` found nothing). Every new test calls the production code; none asserts only on mocks.

### Coverage and CRAP
There is no coverage or complexity tool under `quality`, so there are no CRAP scores. I did not do a separate coverlet run this round, because breaks Q-B10 and Q-B3/Q-B11 show that tests execute and assert the changed lines in `TryAcquire` and the timer. The changed log line at src/api/SaveHighScore.cs:20 runs in no test. That is the same as in round 1: the plan has no Function-level HTTP tests (A5).

### Mutation testing
Not run: no mutation framework is configured.

### Breaks
File: .work/CHORE-013-rate-limit-and-score-plausibility/docs/work/CHORE-013-rate-limit-and-score-plausibility/reviews/qa-r2-breaks.json. Every break ran `dotnet test tests/BlazorInvaders.Tests --nologo`.

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| Q-B1 | AC2/T2 IPv6 mask /64 to /96 (r1 survivor, Q1) | src/api/ScoreRateLimiter.cs:100 | dotnet test | caught: ScoreRateLimiterTests.ClientKey_Normalizes |
| Q-B2 | TB2 CPU: early exit of MaxPlausibleScore removed (r1 survivor, Q2) | src/api/HighScoreValidator.cs:109 | dotnet test | caught: HighScoreValidatorTests.HugeWave_ReturnsQuickly |
| Q-B3 | AC2 expiry timer does nothing (r1 survivor, Q3) | src/api/ScoreRateLimiter.cs:59 | dotnet test | caught: ScoreRateLimiterTests.Timer_DropsEndedWindows_WithoutTraffic |
| Q-B5 | P6 inline-prune throttle off (r1 survivor, Q4) | src/api/ScoreRateLimiter.cs:143 | dotnet test | caught: ScoreRateLimiterTests.TryAcquire_FullTableScan_IsThrottledToOncePerMinute |
| Q-B6 | AC3 bound sums one wave too many (r1 survivor, Q5) | src/api/HighScoreValidator.cs:109 | dotnet test | caught: HighScoreValidatorTests.MaxPlausibleScore_PinnedValues |
| Q-B8 | AC3 mothership allowance 200 to 150 (r1 survivor, Q5) | src/api/HighScoreValidator.cs:26 | dotnet test | caught: HighScoreValidatorTests.MaxPlausibleScore_PinnedValues |
| Q-B10 | Q7 unknown-address posts reported as the client bucket | src/api/ScoreRateLimiter.cs:153 | dotnet test | caught: ScoreRateLimiterTests.TryAcquire_ReportsBucket |
| Q-B11 | Q3 timer period not the window (fires only once) | src/api/ScoreRateLimiter.cs:59 | dotnet test | caught: ScoreRateLimiterTests.Timer_DropsEndedWindows_WithoutTraffic |

All 8 breaks were caught, none survived, and all were tried. The baseline test run passed in the scratch copy before the breaks.

## Checked and fine
- The changes since round 1 are as described in WP1.md round 2 and in the test report.
- The docs changes for C2 and S1 match what those findings asked for.

## Notes for the PM
- The brief contained only its template fields and the task. It told me no verdict or findings.
- I wrote one file, my breaks list: .work/CHORE-013-rate-limit-and-score-plausibility/docs/work/CHORE-013-rate-limit-and-score-plausibility/reviews/qa-r2-breaks.json. The worktree's `git status --porcelain` was the same at the start and at the end.

Restored: yes

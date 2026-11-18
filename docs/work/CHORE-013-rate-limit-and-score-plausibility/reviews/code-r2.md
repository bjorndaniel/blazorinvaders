VERDICT: APPROVED

# Code review: CHORE-013 Rate limiting and score plausibility check, round 2

**Reviewed:** changes since round 1 (`devagents.mjs changes --since 1`), plus the code around them in `src/api/ScoreRateLimiter.cs`
**Reviewer:** Saucer (Code Reviewer)

## Earlier findings
Round 1 had no blocker or major findings. The minor and nit findings were all fixed:
- **C1 (minor): fixed.** `TryAcquire` now reports a `RateBucket` (`Client`, `Unknown` or `Overflow`) through an `out` parameter. The 429 warning in `src/api/SaveHighScore.cs:20` logs only that bucket kind, never the address or the key. I traced the bucket logic:
  - `Unknown` when the key is `UnknownKey`.
  - `Overflow` only when the table is still full after the inline prune.
  - `Client` otherwise.
- **C2 (nit): fixed.** The TB2 cell now lists `wave` and the two headers.
- **C3 (nit): fixed.** `AlienWaveMax` is a `const`, and the code uses `var boss`.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C4 | nit | tests/BlazorInvaders.Tests/HighScoreTests.cs:26 | `System.Text.Encoding.UTF8` is written out in full, in the same change that adds `using System.Text.Json;`. | Add `using System.Text;` and write `Encoding.UTF8`. |

## Checked and fine
- **The `RateBucket` enum** is nested in the sealed limiter and documented. It belongs to the API project, so it does not belong in the core `Enumerations.cs` (CG10). The new `out` parameter is internal to the API and its tests (declared in the implementation notes).
- **`BossWaveMax`** is now `internal` with a doc comment, as CG14 asks for test-only members. Its pinned values 7,500 and 10,500 match 50 × 4 × HP + 1000k + 500.
- **The new tests:**
  - The /64 test differs only in bytes 8-11, the bytes the mask drops.
  - The `int.MaxValue` wave test is bounded by `WaitAsync`.
  - The timer test captures the callback with a small fake `TimeProvider`, without real waits.
  - The prune-throttle test: I traced it against `TryAcquire`/`PruneLocked`. The first scan happens at T0, the second at T0+59m30s and removes nothing, and at T0+60m the scan is skipped, so 4 entries remain as asserted.
  - The bucket test covers all three kinds.
  - The wave JSON test round-trips through `HighScoreValidator.TryParse`, and the omitted-when-zero test matches `JsonIgnoreCondition.WhenWritingDefault` on `HighScore.Wave`.
- **The T2 text** for S1 is accurate: once the table is full, new clients all share the one overflow bucket.
- **Tests run:** `dotnet test --filter` on the ScoreRateLimiter, HighScore and ScorePlausibility classes: 70 passed, 0 failed.

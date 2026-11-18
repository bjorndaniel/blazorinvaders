VERDICT: APPROVED

# Security review: CHORE-013 Rate limiting and score plausibility check, round 2

**Reviewed:** the changes since round 1 (`devagents.mjs changes --since 1`: `ScoreRateLimiter.cs`, `SaveHighScore.cs`, `HighScoreValidator.cs`, `docs/security-model.md`, four test files, `TRACKER.md`), plus my round 1 findings
**Reviewer:** Bunker (Security Reviewer)

## Threat model
- The boundaries are the same as in round 1: TB2 (anonymous POST to `savehighscore`, with the `wave` field and the client-IP headers) and TB8/T12 (dispatch inputs). Neither changed in this round.
- The new code in this round only reports and logs which bucket a post was counted against (client, unknown or overflow). It is an enum value, so neither the address nor the HMAC key reaches the log (AC2/D1, CG24).
- Both security model updates in this round are text only: the TB2 "What crosses it" cell (`wave` and the two headers) and the table-fill lockout clause in T2. Neither adds or changes an actor, a boundary or a threat status.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| S1 | — | docs/security-model.md T2 | Fixed. T2 now says that a full 10,000-client table moves new clients into one shared bucket, so a large address pool can delay everyone's posts for up to an hour. | — |
| S2 | — | src/api/ScoreRateLimiter.cs:150-165; src/api/SaveHighScore.cs:17-20 | Fixed. `TryAcquire` reports a `RateBucket`, and the 429 warning logs `{Bucket}` as structured data. An unknown address is reported as `Unknown`. A full table is reported as `Overflow`, including when the address is unknown and the unknown bucket is not yet tracked. The address and the key are never logged. | — |
| S3 | nit | src/api/HighScoreValidator.cs:93-125 | [info] Still open, unchanged from round 1: the per-wave bound is loose and the wave is reported by the client. This is already stated in T1. | None. |

## Checked and fine
- **Bucket reporting:** the bucket is set before the lock and changed to `Overflow` only inside the lock, on the same path that swaps the key. It is a local `out` value, not shared state, so the lock is unchanged and there is no race. The rate-limit result is unchanged, and the existing tests still pass with the extra `out`.
- **`HighScoreValidator`:** `AlienWaveMax` became a `const` with the same value. `BossWaveMax` is now `internal` and is visible only to the test assembly. `int boss` became `var boss`. None of this changes behaviour. The early exit on huge waves is now covered by a test with a time bound (`HugeWave_ReturnsQuickly`).
- **New tests:** they cover the /64 mask (bytes 8-11), the expiry timer firing with no traffic, the throttle on the inline prune, the pinned bounds and the `wave` JSON field. They contain no real secrets: the salts are fixed test values and the addresses come from documentation ranges.
- **Item folder and `TRACKER.md`:** I searched both for credentials, connection strings and keys and found none. I also found no text that tries to instruct an AI agent.
- **Scanners and dependencies:** no dependency changed in this round and no project scanners are approved, so I ran no scanner.

## Notes for the PM
- The brief contained only its template fields and the task. Nothing in it told me what to find or which verdict to give.

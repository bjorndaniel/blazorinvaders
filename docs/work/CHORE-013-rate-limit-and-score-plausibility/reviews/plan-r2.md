VERDICT: APPROVED

# Plan review: CHORE-013 Rate limiting and score plausibility check, round 2

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| P1 | resolved | Step 2 adds a `TimeProvider.CreateTimer` timer that calls `Prune` every `Window`, whatever the POST traffic. The limiter holds the timer as a field and is a container-owned singleton, so the timer stays alive and DI disposes it on shutdown. The 2 h bound is correct: an entry ends at most one period before the next prune. It is now stated in Summary, step 10 and Security. `Prune_RemovesEndedWindowsOnly` tests it with explicit times and no POSTs. |
| P2 | resolved | When the table is still full after pruning, new clients go into a shared overflow bucket that is limited like any other key, and `TryAcquire_TableFull_UsesSharedOverflowBucket` covers it. The cost is that legitimate new clients get 429 while the table is full. Security "Memory" declares this, and those scores are queued. |
| P3 | resolved | (a) Step 9 and Risks add the forged-header check, with a manual test row. A forgeable `X-Client-IP` would give 21 different keys and so 21 responses of 400, so the check does detect it. (b) The API never returns 403 (I checked `src/api/*.cs`; it only sends 200, 400, 413 and 500), so retrying 403 is safe. The 429 is readable in the browser because the Function App's platform CORS (`provision.yml:141-155`) also adds headers to responses the function itself sends. |
| P4 | resolved | Step 8 and Security now use `^[1-9][0-9]*$`. |
| P5 | maintained (nit, non-blocking) | The plan is now 17.5 KB, still well over the ~10 KB guide for an M plan. Trimming would still help; it does not block. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P6 | minor | Step 2, `TryAcquire` ("When a new key arrives and the table holds `MaxTrackedClients`, call `Prune(now)`") | Once the table is full of live entries, every request from a new key runs a full O(10,000) scan under the global lock, before going to the overflow bucket. An attacker who keeps the table full therefore makes every one of their requests cost a full scan. Throughput still holds, but the scan does nothing now that the timer prunes. | Drop the inline `Prune` and rely on the timer. Or prune inline only when the last prune is at least some interval old. The implementer can settle this. |

## Attacks the plan survived
- Timer lifetime: an `ITimer` that only the singleton's field references stays alive with the singleton. `Dispose` stops it, so it neither leaks nor is collected early.
- Timer callback and `TryAcquire` racing: both run under the same lock, so no entry is pruned in the middle of an update.
- Prune bound: with period `Window`, an entry whose window ends just after a prune goes at the next one, so its lifetime is under 2 × `Window`. This meets D1's 24 h limit.
- Overflow test: with cap 3, 20 new addresses are allowed and the 21st is rejected. This shows the limit stays on once the table is full, and `TrackedClients` is bounded at cap + 1.
- 403 retry: if a 403 persisted, the queue (at most 10) would retry on each load, and `SendPendingScoresAsync` stops at the first false. No score is lost and nothing loops.
- 429 readable by the browser: platform CORS covers function responses, so it goes through `ShouldRetrySend`. Without CORS headers it would surface as `HttpRequestException`, and the score would be queued either way.
- Forged-header check: the limiter runs before the body is read, so 21 invalid-body POSTs save nothing. Forging both `X-Client-IP` and `X-Forwarded-For` tests both header paths.
- `ShouldRetrySend` in core uses only `System.Net.HttpStatusCode`, so it adds no dependency to the WASM-shared assembly.
- The `TimeProvider` API is available: every project targets net10.0, and `CreateTimer` has been in the BCL since .NET 8. No package is needed, because the tests pass explicit `now` values instead of a fake clock.

## Checked and fine
- The Revisions section matches the changes. The parts that did not change were checked in round 1 and need no new finding.
- Classification (M, security-sensitive yes, UI-facing no) and the single WP are unchanged. Every file is still listed once.

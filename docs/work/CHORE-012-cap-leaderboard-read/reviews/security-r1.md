VERDICT: APPROVED

# Security review: CHORE-012 Cap the leaderboard read, round 1

**Reviewed:** diff vs base_commit 1c264bf + untracked files
**Reviewer:** Bunker (Security Reviewer)

## Threat model
- Entry points: anonymous `GET /api/getleaderboard` (TB3) and `POST /api/savehighscore` (TB2), both reaching Table Storage (TB4). Actors are AC1/AC2, who are anonymous and untrusted, and AC3, the owner, who moderates directly in storage (R5).
- Data: public names and scores only (AS1). No PII, credentials or auth changes. The account-key connection string is still read from the app setting (T10 unchanged).
- Threats in scope: T3 (unbounded read, the target of this item), T7 (query and key injection, since the filters and the RowKey change), T9 (error leakage), T2/T4 (flooding, which stays open), and the R5 moderation control behind T1/T6.
- The diff updates the model for every element it changes: T3, TB4, R2, T7 and Revisions.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| S1 | minor | src/api/LegacyTopCache.cs:29-31, :71; docs/security-model.md:34 (R5) | [low] A non-empty legacy top 10 is cached for the instance's lifetime and never re-read. R5 says AC3 moderates by deleting rows in Table Storage, and that is the only mitigation for T1 (forged scores) and T6 (offensive initials). Scenario: a forged 999,999 score or an offensive name is already in the all-time legacy top 10 when this deploys. The owner deletes the row, but every warm instance keeps serving it until it recycles, which can take hours or days under steady traffic. The owner can work around it by restarting the Function App, but nothing tells them to. | Pick one. (a) Document it: in R5 (or T1/T6), state that removing a legacy (GUID-keyed) row takes effect after a Function App restart. (b) Give cached entries a TTL, for example 1 hour. That is still one bounded legacy read per board per hour, so T3 stays mitigated. |
| S2 | nit | src/api/HighScoreStore.cs:139, src/api/LegacyTopCache.cs:72-89 | [info] Suppose a legacy row has a non-string `Name`, which only AC3 could write. `GetString` then throws a non-`RequestFailedException`, the cache treats it as a failed load, and the instance-wide 5-minute back-off restarts every time that board is retried. Uncached boards are then served without their legacy rows on a recurring cycle. No path exists for AC1/AC2 to cause this. | Optional: map the name with `TryGetValue` plus `as string`, as `Score` already is, so a bad row is skipped instead of failing the whole load. |

## Checked and fine
- T7 injection. Every filter goes through `TableClient.CreateQueryFilter` with interpolated parameters. The only inputs are the constants `R`/`S` and a partition from `TryGetLeaderboardPartition`, which is `HighScore` or `Daily-` plus a validated `^[0-9]{8}$` date. The RowKey is built only by the server (`RankedRowKey` plus `Guid.NewGuid()`), and the DTO still has four fields.
- Key range correctness. Legacy GUIDs (lowercase hex `0-9`, `a-f`) and `Current` fall outside the ranked range `["R","S")`. So the ranked query cannot pick up legacy rows, and the legacy queries cannot pick up ranked rows. `D10` padding holds for every `int`, including negative scores and `int.MinValue`.
- T3 bound. The ranked read uses `maxPerPage: 10` and `TakeTopAsync` stops at 10. Legacy cost is fixed by the frozen legacy rows, and both legacy range queries run once per board per instance.
- Memory under arbitrary `day` values (about 3.6M valid dates). Empty and failed loads are removed on completion. Only boards with legacy rows stay cached, at 10 entries each. In-flight entries are bounded by request concurrency.
- Back-off and shared loads. One `Lazy` load is shared among callers and runs with `CancellationToken.None`, so a client that cancels cannot abort it. Faulted loads are caught and the entry is removed only if it is the same `Lazy`. `_retryAfter` is lock-guarded. An attacker could only trigger the back-off through storage throttling, which is already T4 (open) and only hides legacy rows for 5 minutes.
- `EnsureTableAsync`: double-checked under a `SemaphoreSlim`. The unsynchronised read of the `bool` is benign (worst case, one extra lock), and a failure gives a 500 with the next call retrying.
- T9: error texts are unchanged, and exceptions are logged on the server only. Log messages use structured placeholders with the validated partition, so there is no log injection.
- Dependencies, CI and secrets. There are no package or workflow changes; only the test csproj adds `<Compile Include>` lines. The item folder, tests and test-breaks file contain no secrets or connection strings, and no text tries to instruct an agent.
- Rollback safety: old code reads ranked rows as ordinary rows (`GetInt32` on an `Int32` `Score`).

No project scanners were approved for me, and none were run.

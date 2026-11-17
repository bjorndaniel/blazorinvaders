VERDICT: APPROVED

# Plan review: CHORE-012 Cap the leaderboard read, round 3

**Reviewed:** plan
**Reviewer:** Crab (Plan Reviewer)

## Earlier findings
| ID | Severity | Status | Why |
|----|----------|--------|-----|
| P6 | major | resolved | Step 4 has every query pass one `select` constant with `PartitionKey`, `RowKey`, `Name` and `Score`. The Tests review row checks it, and the Tester checks it on Azurite if available. |
| P7 | major | resolved | The all-partition scan is gone. `LoadLegacyAsync` uses `PartitionKey eq {p} and RowKey lt 'R'` and `... RowKey ge 'S'`. Both are in-partition range queries, so they never touch ranked rows (`R...`). Their cost is bounded by that board's frozen legacy rows. Only non-empty results are cached, so arbitrary `day` values hold no memory, and a board with no legacy rows costs a constant 3 queries. The T3, TB4 and A4 wording (Steps 9 and 10) matches. Rewording "T2 makes this worse" is justified, because ranked rows past the first page are never read. |
| P8 | minor | resolved | The plan is 13.1 KB, down from 15.1 KB, and the new cache step accounts for most of the rest. It is still a little over the M budget of about 10 KB, which is acceptable. |
| P9 | minor | resolved | Step 4 `GetTopAsync` now awaits the cache's `GetAsync(p)` before the ranked query and `Merge`. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P10 | minor | Step 3 (`LegacyTopCache`, "An empty or failed load removes its entry") and Step 4 (row mapping `GetString("Name")`) | 1. "Failed" is defined as the loader returning `null`, and the loader catches only `RequestFailedException`. Anything else thrown during mapping (for example `GetString` on a non-string `Name`, which throws today as well) faults the cached `Lazy<Task>`. If only `null` or empty results remove the entry, that board then returns 500 on every GET for the rest of the instance's lifetime, with no back-off. 2. Removing the entry with a plain `TryRemove(key)` after completion can evict a newer entry started by another caller. | State that a faulted load counts as failed: remove the entry and set `retryAfter`. Remove the entry only if it is still the same `Lazy`, for example with `TryRemove(KeyValuePair)`. A unit test with a throwing loader is optional. |

## Attacks the plan survived
- Legacy key ranges: `Guid.NewGuid().ToString()` is lowercase hex. Digits and `C` sort below `R`, and `a` to `f` sort at or above `S`. So `RowKey lt 'R'` together with `RowKey ge 'S'` covers every legacy key and no ranked key. `SaveHighScore.cs:38` is still the only writer.
- AC2 on a board created after deploy: 2 empty range queries plus 1 ranked page per GET, the same no matter how many entries are stored. It does not grow with the table, with flooding or with the number of `Daily-*` partitions.
- AC2 against arbitrary `day` values: with empty results not cached, memory stays flat. Each such request costs a constant 3 queries, and their rate is CHORE-013's concern (T2/T4).
- Concurrent first calls: `Lazy<Task>` in a `ConcurrentDictionary` shares one load. The load runs on `CancellationToken.None` and callers use `WaitAsync(ct)`, so one caller cancelling neither poisons the shared load nor leaks it.
- Back-off: one instance-wide timestamp. While it runs, boards that are not cached get ranked rows only for 5 minutes, and cached boards are still served. This is stated in Risks, and a test covers the +4:59 and +5:00 edges.
- Merge after P6: with `RowKey` now selected, the `SortKey` of both kinds of row is `R{inv}_{id}`. That gives score descending, then RowKey ascending, which matches today's stable sort (GetLeaderboard.cs:38-50). `Current` and GUIDs keep their relative order.
- `TimeProvider` and a manual subclass of it: both projects target net10.0 (BlazorInvaders.Api.csproj:3, BlazorInvaders.Tests.csproj:4), so no package is needed. CG31 limits clock use only in `src/core`.
- CG21: the store catches `RequestFailedException` by name, and the catch-all stays only in the functions' `Run`. CG20: the legacy-read failure is logged as a warning.
- Rollback: the plan writes only normal score rows, and old code reads ranked rows as ordinary rows.
- Classification: M, security-sensitive yes and UI-facing no all still fit. The new cache is a small pure class in the same area, with unit tests.

## Checked and fine
- AC1 to AC5 each map to steps and to tests or review rows. The cache's four tests cover caching, keeping empty results out of the cache, the back-off and shared loads.
- The plan's description of `QueryAsync<T>(filter, maxPerPage, select, ct)` and `CreateQueryFilter` matches Azure.Data.Tables 12.9.1, and the filters stay parameterized (T7).
- The security model updates (T3, TB4, R2, T7) and the architecture updates (A4, A5, A11) are in Steps 9 and 10.

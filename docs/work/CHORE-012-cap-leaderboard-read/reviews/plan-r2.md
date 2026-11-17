VERDICT: CHANGES_REQUESTED

# Plan review: CHORE-012 Cap the leaderboard read, round 2

**Reviewed:** plan
**Reviewer:** Crab (Plan Reviewer)

## Earlier findings
| ID | Severity | Status | Why |
|----|----------|--------|-----|
| P1 | major | resolved | The migration, marker and upserts are gone. Nothing is written except normal score rows, so it stays M. The new legacy read has its own flaws, raised as P6 and P7. |
| P2 | minor | resolved | `RequestFailedException` is caught by name, `TryReadScore` replaces `GetInt32`, the 5-minute retry is stated, and there is a test. |
| P3 | minor | resolved | The deploy window is accepted in Risks. |
| P4 | nit | resolved | Step 3 says `public sealed`. |
| P5 | nit | maintained | It is still over budget. Raised again as P8. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P6 | major | Step 3, `GetLegacyTopAsync` ("selecting `Name` and `Score`") and `GetTopAsync` (`select: Name, Score`) | `$select` returns only the properties it lists. The 12.9.1 XML docs (Azure.Data.Tables.xml:2506-2508) say it "selects which set of entity properties to return", and their example selects `PartitionKey, RowKey` explicitly. The old SDK needed `ProjectSystemProperties` for the same reason. So built as written, `PartitionKey` and `RowKey` come back null. In the legacy scan, `SelectTopPerPartition` groups on a null key, and a `Dictionary` throws `ArgumentNullException`. That exception is not a `RequestFailedException`, so it escapes the guard: every GET returns 500 and the scan reruns on every call. In `GetTopAsync`, a ranked row's `SortKey` is its null RowKey, so `Merge` puts every ranked row ahead of the legacy rows whatever the scores. AC4 fails, and no planned test can catch it, because the tests never touch storage. | In both queries, select `PartitionKey`, `RowKey`, `Name` and `Score`, or drop `select`. |
| P7 | major | Step 3 legacy filter `RowKey lt 'R' or RowKey ge 'S'` (all partitions); Security considerations ("the legacy rows are frozen at deploy, so that scan does not grow afterwards"); Step 8 (dropping "T2 makes this worse") | A filter without `PartitionKey` is a full table scan. The service reads every entity, ranked rows included, and returns continuation pages (each one a billed transaction) as it goes. What it returns is frozen, but its server work, latency and transaction count grow with every new score and every new `Daily-*` partition. The first GET on each instance therefore grows with the stored entries, which misses AC2. Under load, the consumption plan scales out and each new instance runs the full scan. Flooding (T2, still open) makes that worse, so T3 is not mitigated as claimed and the T2 sentence cannot be dropped. | Make the legacy read touch only legacy rows. For example, read the requested partition lazily with two in-partition range queries (`PartitionKey eq {p} and RowKey lt 'R'`, and `... RowKey ge 'S'`). Their cost is bounded by that partition's frozen legacy rows. Cache non-empty results only, so arbitrary `day` values cannot grow memory. An empty partition then costs a constant 3 small transactions per call. Or name another approach whose cost does not depend on ranked rows. Update the Security considerations, the T3 text and the A4 text to match. |
| P8 | minor | Whole plan | 15.1 KB against the M budget of about 10 KB. **Context found** and **Security considerations** repeat each other, and step 2 spells out signatures that the implementer can derive. | Trim. |
| P9 | minor | Step 3 `GetTopAsync` | The step does not say that `GetTopAsync` triggers the guarded `GetLegacyTopAsync`. It only says "the cached legacy top". | Say that it awaits the guarded legacy load (or the per-partition load from P7) before `Merge`. |

## Attacks the plan survived
- Merge order: `SortKey` = `R{inv}_{guid}` for both kinds of row gives score descending, then RowKey ordinal ascending. That equals today's stable `OrderByDescending` over the service's RowKey order (GetLeaderboard.cs:40-50), and `Current` sorts like a GUID.
- Exact top 10: the legacy top 10 merged with the ranked top 10 is the true top 10 of the union, ties included.
- The frozen legacy set: `SaveHighScore.cs:38` is the only writer (`AddEntityAsync`), and `GetHighScore` only does a point read. A late old-code write during a deploy is accepted (P3).
- Null `Score` maps to 0 and `Name` to "", as today. A non-Int32 score is skipped instead of failing with a 500, an accepted improvement.
- A failed legacy read: ranked rows only, retried at most every 5 minutes. A failed table creation: 500, retried on the next call. Neither runs per call.
- Arbitrary `day` values (TryGetLeaderboardPartition accepts any valid date): with the plan's global scan, nothing is cached for them. The P7 fix has to keep that true.
- Rollback: a plain revert. The old code reads ranked rows as ordinary rows.
- Size M: with the migration gone, this is one area and a routine once-per-instance guard. Security-sensitive yes and UI-facing no are correct.

## Checked and fine
- Every acceptance criterion maps to steps and tests. AC3 and AC5 are verified by review, which A5 allows.
- `QueryAsync<T>(filter, maxPerPage, select, ct)` exists in Azure.Data.Tables 12.9.1 with the semantics the plan relies on, apart from P6.
- `HighScoreValidator` constants and the partition rules are unchanged. Daily writes go only to today's or yesterday's partition.
- Steps 8 and 9 update T3, TB4, R2, T7, A4, A5 and A11. The "no migrations" wording in A4 now stays true.

# Planner response to plan review round 2: CHORE-012 Cap the leaderboard read

**Planner:** Mothership
**Review answered:** `reviews/plan-r2.md`
**Plan revised in round:** 3

| Finding | Severity | Response | What changed, or why not |
|---------|----------|----------|--------------------------|
| P6 | major | accepted | Every query passes one `select` constant with `PartitionKey`, `RowKey`, `Name` and `Score` (Step 4). The review row in Tests checks it, since unit tests cannot (AC3/AC4 row). |
| P7 | major | accepted | Your option, nearly as proposed. The new pure `LegacyTopCache` (Step 3) loads a board's legacy rows lazily with two in-partition range queries (`RowKey lt 'R'`, `RowKey ge 'S'`; Step 4). It caches non-empty results only and shares in-flight loads. Failures back off 5 minutes through one instance-wide timestamp, so arbitrary `day` values grow no memory. The cache gets four unit tests. Security, T3 (Step 9), A4 (Step 10) and Risks are updated. T3's "T2 makes this worse" is reworded rather than kept: with no full scan, ranked rows past the first 10 are never read. |
| P8 | minor | accepted | Context found, Steps, Tests and Security are trimmed, with no repetition between them. The plan is now about 13 KB (from 15), including the new cache step and its tests. |
| P9 | minor | accepted | Step 4 `GetTopAsync` awaits the cache's `GetAsync(partition)` before the ranked query and `Merge`. |

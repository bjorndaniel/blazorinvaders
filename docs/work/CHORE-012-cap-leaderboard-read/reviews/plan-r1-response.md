# Planner response to plan review round 1: CHORE-012 Cap the leaderboard read

**Planner:** Mothership
**Review answered:** `reviews/plan-r1.md`
**Plan revised in round:** 2

| Finding | Severity | Response | What changed, or why not |
|---------|----------|----------|--------------------------|
| P1 | major | accepted | Option (b): the migration, the marker and all upserts are dropped. The legacy top 10 per partition is read once per instance into memory and merged per GET, using a `SortKey` that reproduces today's tie order. Stays M (Summary, Steps 2, 3, 8 and 9, Tests, Security, Risks). |
| P2 | minor | accepted | `RequestFailedException` is caught by name. Non-`Int32` scores are skipped and counted through the pure `TryReadScore` (no `GetInt32`). A failed legacy read serves ranked rows only and is retried at most every 5 minutes (Step 3; new `TryReadScore_TypeCases` test). |
| P3 | minor | accepted | Risks: the deploy window is accepted. A late GUID row is a legacy row, and it appears after the instance's next legacy scan. |
| P4 | nit | accepted | `HighScoreStore` is `public sealed` (Step 3). |
| P5 | nit | accepted | The migration text is removed and Context found is trimmed. |

# Planner response to plan review round 1: CHORE-013 Rate limiting and score plausibility check

**Planner:** Mothership
**Review answered:** `reviews/plan-r1.md`
**Plan revised in round:** 2

| Finding | Severity | Response | What changed, or why not |
|---------|----------|----------|--------------------------|
| P1 | major | accepted | Step 2: `internal Prune(now)` driven by a `TimeProvider.CreateTimer` timer every `Window` (limiter is `IDisposable`). Bound stated as 2 h in Summary, step 10 and Security. Test `Prune_RemovesEndedWindowsOnly` with injected times, no POSTs. |
| P2 | minor | accepted | Step 2: when the table is still full after pruning, new clients count against one shared overflow bucket that is limited like any other key. Test `TryAcquire_TableFull_UsesSharedOverflowBucket`. |
| P3 | minor | accepted | (a) Step 9 README and Risks: 21 invalid-body POSTs with forged `X-Client-IP`/XFF, 429 expected on the 21st, otherwise roll back; Tests has a manual row. (b) The API never returns 403, so `ShouldRetrySend` also retries 403 (step 6, test), and the README wording now matches. |
| P4 | minor | accepted | Step 8 and Security: the guard is `^[1-9][0-9]*$`. |
| P5 | nit | accepted | Context found trimmed. The new P1-P3 content offsets it, so the plan is still about 17 KB. |

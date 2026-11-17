VERDICT: APPROVED

# QA review: CHORE-012 Cap the leaderboard read, round 1

**Reviewed:** the diff against base_commit 1c264bf plus the untracked files
**Reviewer:** Octopus (QA Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | minor | `src/api/HighScoreStore.cs:40,48,72,93` | `_table!` is used in `AddAsync`, `GetTopAsync`, `EnsureTableAsync` and `LoadLegacyAsync`. The only reason it is non-null is that the callers check `IsConfigured`, and that check is in another class. CG30 (must) allows `!` only when the reason is visible in the same method or is a documented invariant, and plan Step 4 cites CG30 for this design. | Pick one: document the invariant (for example in the XML summary: "callers check `IsConfigured` first"); or use one accessor that throws `InvalidOperationException` when `_table` is null; or use `is { } table`. `created!` in `LegacyTopCache.cs:31` is fine, because the reason is visible in the same method. |

## QA evidence
Assurance: **standard**. `quality` is not configured in `.devagents/config.json` (`declined: quality-tools`), so there was no mutation framework and no `crap` run. I did 10 breaks by hand, the limit for an M item.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| S1 FunctionResponses | `src/api/FunctionResponses.cs`; used in `GetLeaderboard.cs:24`, `SaveHighScore.cs:16-34` | done (codes and texts unchanged) |
| S2 LeaderboardRanking, StoredScore | `LeaderboardRanking.cs:12-61`, `StoredScore.cs:4` | done |
| S3 LegacyTopCache | `LegacyTopCache.cs:10-74` | done. Deviation recorded in WP1.md: an `onLoadFailed` callback was added, and a faulted load counts as failed (P10) |
| S4 HighScoreStore | `HighScoreStore.cs`: select constant `:10`, `EnsureTableAsync :62-79`, legacy ranges `:88-90`, `AddAsync :30-41`, `GetTopAsync :43-60` (ranked range, `maxPerPage: 10`, `TakeTopAsync`) | done. The legacy rows are merged one at a time, not once per range. The result is the same. |
| S5 Program.cs | `Program.cs:8-12` | done |
| S6 GetLeaderboard | `GetLeaderboard.cs:23-29` | done (validation, the 400, the 500 and the catch-all are kept) |
| S7 SaveHighScore | `SaveHighScore.cs:16-34` | done |
| S8 Tests and csproj | `BlazorInvaders.Tests.csproj:30-32`, both test files | done |
| S9 security-model | T3 `mitigated`, R2, TB4, T7, a Revisions line | done |
| S10 architecture | A4, A5, A11, a Decisions line | done |
| — GetHighScore.cs | the file was removed by CHORE-008 | n/a, recorded in WP1.md |
| T: RankedRowKey_OrdinalSort_GivesScoresDescending | LeaderboardRankingTests, same name | done; checks the row |
| T: RankedRowKey_FallsInRankedRange / LegacyKey_FallsOutsideRankedRange | same names (with an extra `S` case) | done; checks the row |
| T: TakeTopAsync_LongSource_ReturnsTenAndStopsEnumerating | same name (counts what is yielded) | done; checks the row |
| T: Merge_LegacyOnly_KeepsTopTen | same name | done |
| T: Merge_MatchesTodaysSort | same name. The expected value comes from an independent model of today's code (stable `OrderByDescending` over ordinal RowKey order), not from `SortKey`, so it is not a tautology | done |
| T: TryReadScore_TypeCases | same name | done |
| T: NonEmpty_LoadedOncePerPartition, Empty_NotCached, Failure_BacksOffFiveMinutes, ConcurrentFirstCalls_ShareOneLoad | LegacyTopCacheTests, same names (plus `FaultedLoad_CountsAsFailed_AndIsNotKept`) | done |
| T: AC3/AC4 review | `CreateIfNotExistsAsync` appears only at `HighScoreStore.cs:72`, inside the guarded `EnsureTableAsync`. All three queries pass `Select` (`:52`, `:93`) | done (checked with grep) |
| T: AC5 review | `docs/security-model.md`, row T3 | done |

Unplanned hunks: none. Every changed file is owned by WP1, except `docs/work/TRACKER.md`, which the PM owns.

### Coverage and CRAP
`quality.coverage` is not set up. I used the project's coverlet collector through `scratch`: `dotnet test tests/BlazorInvaders.Tests --collect:"XPlat Code Coverage" -- ...IncludeTestAssembly=true`. The pure files are compiled into the test assembly, so that assembly has to be included.
- `LeaderboardRanking.cs`, `LegacyTopCache.cs`, `StoredScore.cs`: 100% line coverage; branch coverage 100%, except `LoadAsync` at 87.5%. No changed line goes unexecuted.
- `HighScoreStore.cs`, `GetLeaderboard.cs`, `SaveHighScore.cs`, `Program.cs` and `FunctionResponses.cs` are not compiled into the tests. This is by design: the plan and A5 have no storage tests, and the Tester did not run Azurite. These files were checked by review only (see Checked and fine).

### Mutation testing
Not run: `quality.mutation` is not set up (`mutate --status`: no mutation run yet).

### Breaks
Command: `node .devagents/devagents.mjs breaks CHORE-012-cap-leaderboard-read .work/CHORE-012-cap-leaderboard-read/docs/work/CHORE-012-cap-leaderboard-read/reviews/qa-r1-breaks.json`. Tests run for every break: `dotnet test tests/BlazorInvaders.Tests --nologo`. None of these breaks repeats one of the Tester's B1-B8.

| # | Break | File:line | Result |
|---|---|---|---|
| Q-B1 | AC1 negative score: drop the `(long)` cast (overflow) | LeaderboardRanking.cs:17 | caught: RankedRowKey_OrdinalSort_GivesScoresDescending |
| Q-B2 | AC1 TakeTopAsync boundary `>=` to `>` | LeaderboardRanking.cs:57 | caught: TakeTopAsync_LongSource_ReturnsTenAndStopsEnumerating |
| Q-B3 | AC1 Merge returns `MaxEntries + 1` | LeaderboardRanking.cs:47 | caught: Merge_MatchesTodaysSort |
| Q-B4 | AC4 Merge sorts by score only (tie order lost) | LeaderboardRanking.cs:46 | caught: Merge_MatchesTodaysSort |
| Q-B5 | AC4 a missing Score is unreadable instead of 0 | LeaderboardRanking.cs:31 | caught: TryReadScore_TypeCases |
| Q-B6 | AC4 an Int64 Score is accepted | LeaderboardRanking.cs:34 | caught: TryReadScore_TypeCases |
| Q-B7 | AC2 a failure sets no back-off | LegacyTopCache.cs:61 | caught: Failure_BacksOffFiveMinutes |
| Q-B8 | AC2 the back-off also hides cached boards | LegacyTopCache.cs:26 | caught: Failure_BacksOffFiveMinutes |
| Q-B9 | AC2 an empty or failed load evicts other boards | LegacyTopCache.cs:67 | caught: Failure_BacksOffFiveMinutes |
| Q-B10 | error handling: a faulted load is rethrown | LegacyTopCache.cs:56 | caught: FaultedLoad_CountsAsFailed_AndIsNotKept |

10 caught, 0 survived, 0 not tried.

## Checked and fine
- The baseline run passes all 229 tests. No `.skip`, focused tests or swallowed exceptions. Every assertion is on production output, and the only test doubles are loaders and the clock.
- The legacy filters (`RowKey lt "R"`, `RowKey ge "S"`) and the ranked filter (`ge "R"` and `lt "S"`) are disjoint and cover every key. Both are built with `CreateQueryFilter` from constants and the validated partition.
- `GetTopAsync` awaits the cache before `Merge` (P9). The table is created after `EnsureTableAsync`'s double-checked `SemaphoreSlim`. A `RequestFailedException` from it reaches the catch-all (500).
- `AddAsync` writes the same entity as before, except for the ranked RowKey. The save path is unchanged otherwise (no update or upsert).
- The concurrent first load is shared through `Lazy` plus `GetOrAdd`. Eviction removes only the entry the load itself created (`ICollection.Remove` by key and value).
- The response shape (`name`, `score`) and all error texts are unchanged.

## Notes for the PM
- `docs/work/TRACKER.md` lists CHORE-012 with size `S`. The plan and `status.md` say `M`.

Restored: yes

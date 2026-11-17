# Plan: CHORE-012 Cap the leaderboard read

| | |
|---|---|
| **ID** | CHORE-012 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-17 |
| **Size** | M — one area (`src/api`), ~10 files and ~300 lines, a new RowKey scheme for new rows; no data migration, no new kind of write, no new dependency |
| **Security-sensitive** | yes — mitigates a DoS threat (T3), changes the RowKey the server writes and the read filter (R2, TB4, T7) |
| **UI-facing** | no — the API response is unchanged; no client change |
| **Opened** | 2026-11-17 |
| **Plan round** | 3 |

## Summary
Table Storage cannot sort by a property, so new scores get a **ranked RowKey**: `R` + (`int.MaxValue` - score) as 10 digits + `_` + GUID. A partition's RowKey order is then the board order, and `getleaderboard` reads only the first 10 entities of the `R` key range (one page, then stops).
- Existing (GUID-keyed) rows are **not migrated or written**. The first GET for a board on an instance reads that partition's legacy rows with two in-partition RowKey range queries that skip the ranked range, so their cost depends only on the frozen legacy rows. A non-empty legacy top 10 is cached; an empty one is not, so arbitrary `day` values add no memory. Each GET merges it with the ranked top 10 in today's order, ties included.
- A board with no legacy rows (every board created after deploy) costs a constant 3 small queries per GET.
- A legacy row whose `Score` is not an `Int32` is skipped and counted in a log line (today it causes a 500). If a legacy read fails, boards serve ranked rows only and no legacy read is retried for 5 minutes.
- New: a singleton `HighScoreStore` that creates the table once per instance; the pure `LeaderboardRanking` and `LegacyTopCache`, compiled into the tests; `FunctionResponses` per CG16.
- WP1 only. T3 becomes mitigated; R2, TB4, T7 and A4, A5, A11 are reworded.

## Context found
- `src/api/GetLeaderboard.cs` — full partition scan, in-memory sort, `Take(10)`, and table creation per call: the T3 root cause.
- `src/api/SaveHighScore.cs` — `AddEntityAsync` with RowKey `Guid.NewGuid()`, table creation per call, `TextResponse`.
- `src/api/GetHighScore.cs` — a point read of the legacy (`HighScore`, `Current`) entity, which today's all-time board includes; it stays a legacy row.
- `src/api/HighScoreValidator.cs` — `TryGetLeaderboardPartition` accepts any valid date; unchanged.
- `tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj` — compiles `HighScoreValidator.cs` in with `<Compile Include>`; reused for the new pure files.
- `Azure.Data.Tables` 12.9.1 XML docs: `CreateIfNotExistsAsync`, `QueryAsync<T>(filter, maxPerPage, select, ct)`, `CreateQueryFilter(FormattableString)`. `$select` returns only the listed properties, `PartitionKey` and `RowKey` included. `GetInt32` throws on a type mismatch.
- `docs/security-model.md`: T3, T2/T4 (stay open, CHORE-013), R2, AC6, TB3, TB4, T7. `docs/architecture.md`: A4, A5, A11, A20, A22, A23.
- Coding guidelines: CG10, CG11, CG12, CG16, CG20, CG21, CG24, CG30, CG32, CG70-CG73; `.editorconfig`. CG31 (no clock) is core-only; the API takes `TimeProvider`.
- Containers: no runtime, and A5 has no storage tests, so storage code stays thin and the logic sits in pure, unit-tested classes.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | All of it | M | new in `src/api`: `FunctionResponses.cs`, `LeaderboardRanking.cs`, `StoredScore.cs`, `LegacyTopCache.cs`, `HighScoreStore.cs`; `src/api/GetLeaderboard.cs`, `SaveHighScore.cs`, `GetHighScore.cs`, `Program.cs`; `tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj`, new `LeaderboardRankingTests.cs`, `LegacyTopCacheTests.cs`; `docs/security-model.md`, `docs/architecture.md` | - |

## Steps
### WP1
1. **`FunctionResponses.cs`** (CG16): `internal static`, with `TextAsync(req, status, text)` moved from `SaveHighScore.TextResponse` and the constants `StorageSetting = "AzureWebJobsStorage"` and `TableName = "HighScores"`. All three functions use it; codes and texts are unchanged.
2. **`LeaderboardRanking.cs`** (public static, no Azure references) and **`StoredScore.cs`**: `public sealed record StoredScore(string PartitionKey, string RowKey, string Name, int Score)`.
   - `MaxEntries = 10`, `RangeStart = "R"`, `RangeEnd = "S"`. Legacy keys (lowercase-hex GUIDs, `Current`) all fall outside `["R","S")`: digits and `C` below it, `a`-`f` above it.
   - `RankedRowKey(int score, string id)` = `R{(long)int.MaxValue - score:D10}_{id}` (invariant, any `int`). `IsRanked(rowKey)`.
   - `SortKey(row)`: a ranked row's RowKey, else `RankedRowKey(Score, RowKey)`. Ordinal order = score descending, then RowKey ascending = today's stable sort over RowKey order.
   - `TryReadScore(object? raw, out int)`: `null` gives 0 (today's `?? 0`), `int` itself, other types false.
   - `Merge(a, b)`: union by ordinal `SortKey`, at most `MaxEntries`. It also reduces a board's legacy rows to its top 10.
   - `TakeTopAsync<T>(IAsyncEnumerable<T>, ct)`: at most `MaxEntries` items, then stops enumerating (no further page).
3. **`LegacyTopCache.cs`** (public sealed, no Azure references): built from a loader `Func<string, CancellationToken, Task<IReadOnlyList<StoredScore>?>>` (`null` = failed) and a `TimeProvider`. `GetAsync(partition, ct)`:
   - A `ConcurrentDictionary<string, Lazy<Task<...>>>` shares one load among concurrent first calls. The load runs with `CancellationToken.None`; callers use `WaitAsync(ct)`.
   - A non-empty result stays for the instance's lifetime. An empty or failed load removes its entry on completion, so only boards with legacy rows (and loads in flight) are held. `CachedCount` is exposed for tests.
   - A failed load sets one instance-wide `retryAfter` = now + `RetryInterval` (5 min). Until then uncached boards get an empty list without loading; cached ones are still served.
4. **`HighScoreStore.cs`**: `public sealed` singleton (a constructor parameter of public function classes, CS0051) with `ILogger<HighScoreStore>` and `TimeProvider`. One `TableClient` from `StorageSetting`; an empty setting gives `IsConfigured == false` and no client (CG30). It owns a `LegacyTopCache` over `LoadLegacyAsync`.
   - Every query passes one `select` constant: `PartitionKey`, `RowKey`, `Name`, `Score` (`$select` returns only listed properties). Rows map with `GetString("Name") ?? ""` and `TryReadScore(TryGetValue("Score"))`; unreadable rows are skipped and logged as a count.
   - `EnsureTableAsync`: `CreateIfNotExistsAsync` once per instance (`SemaphoreSlim` + flag). A `RequestFailedException` reaches the caller's `Run` catch-all (500, as today); the next call retries.
   - `LoadLegacyAsync(p, ct)`: `PartitionKey eq {p} and RowKey lt {RangeStart}` and `PartitionKey eq {p} and RowKey ge {RangeEnd}` (`CreateQueryFilter`), each read to the end, returning `Merge` of both. Ranked rows are outside both ranges, so the cost depends only on the board's frozen legacy rows. `catch (RequestFailedException)` (CG21) logs a warning and returns `null`.
   - `AddAsync(request)`: `EnsureTableAsync`, then today's entity with `RowKey = RankedRowKey(score, Guid.NewGuid().ToString())`.
   - `GetTopAsync(p, ct)`: `EnsureTableAsync`; await the cache's `GetAsync(p)` (loads the legacy top 10 if needed); then `PartitionKey eq {p} and RowKey ge {RangeStart} and RowKey lt {RangeEnd}` with `maxPerPage: MaxEntries` through `TakeTopAsync`; return `Merge(legacyTop, rankedTop)` as `(Name, Score)`.
5. **`Program.cs`**: register `TimeProvider.System` and `HighScoreStore` as singletons.
6. **`GetLeaderboard.cs`**: inject the store; validation and 400 stay; `!IsConfigured` gives 500 "Storage not configured"; serialize `GetTopAsync` exactly as today. The catch-all stays; per-call client and table creation go.
7. **`SaveHighScore.cs`**: inject the store, save through `AddAsync`; behaviour unchanged; per-call table creation and `TextResponse` go.
8. **Tests**: `<Compile Include>` the three pure files; write both test files, with a small manual `TimeProvider` subclass (no new package).
9. **`docs/security-model.md`**: T3 `mitigated`, described as in **Security considerations**; its "T2 makes this worse" becomes "Flooding (T2) no longer slows the read". TB4: RowKey range filters. R2: ranked RowKey (inverted score + new GUID), still add-only. T7: "ranked RowKey chosen by the server". A Revisions line.
10. **`docs/architecture.md`**: A4: ranked RowKey, capped range read merged with the cached per-board legacy top 10, table created once per instance ("no migrations" stays true). A5, A11: the tests compile the three pure files. A Decisions line.

## Tests
Unit tests only, no storage (A5; no container runtime). All by WP1.

| AC | Test | Type | WP |
|----|------|------|----|
| AC1, AC4 | `RankedRowKey_OrdinalSort_GivesScoresDescending`: {9, 10, 999_999, 1_000_000, 1, 0, int.MaxValue, -5}; the 9/10 and 999_999/1_000_000 pairs catch wrong padding | unit | WP1 |
| AC1 | `RankedRowKey_FallsInRankedRange` / `LegacyKey_FallsOutsideRankedRange`: `Current`, `00000000-…`, `9f…`, `a0…`, `ffffffff-…` are outside and not `IsRanked` | unit | WP1 |
| AC1, AC2 | `TakeTopAsync_LongSource_ReturnsTenAndStopsEnumerating`: a counting 25-item iterator yields no more than 10; 3 give 3, empty gives empty | unit | WP1 |
| AC4 | `Merge_LegacyOnly_KeepsTopTen`: 12 rows give 10 without the two lowest; all fields kept | unit | WP1 |
| AC1, AC4 | `Merge_MatchesTodaysSort`: 10 legacy (GUIDs, `Current`) + 10 ranked rows with ties across sets equal today's algorithm on the same rows (ordinal RowKey order, stable `OrderByDescending`, `Take(10)`); 2 + 3 give 5 | unit | WP1 |
| AC4 | `TryReadScore_TypeCases`: `null` 0/true, `42` 42/true; `42L`, `4.2`, `"42"` false | unit | WP1 |
| AC2 | `LegacyTopCacheTests.NonEmpty_LoadedOncePerPartition`: 3 calls on `HighScore` load once; a daily board loads separately; `CachedCount` 2 | unit | WP1 |
| AC2 | `Empty_NotCached`: 100 distinct daily boards with an empty loader load 100 times; `CachedCount` 0 | unit | WP1 |
| AC2 | `Failure_BacksOffFiveMinutes`: after a `null` load, +4:59 does not load and returns empty, +5:00 loads; a board cached earlier is still served | unit | WP1 |
| AC2 | `ConcurrentFirstCalls_ShareOneLoad`: two calls during one pending load (`TaskCompletionSource`) get the same rows from one load | unit | WP1 |
| AC3, AC4 | Review: `CreateIfNotExistsAsync` only in the guarded `EnsureTableAsync`; every query passes the four-property `select`. The Tester checks both on Azurite if available | review | WP1 |
| AC5 | Review: T3 is `mitigated` | review | WP1 |

## Security considerations
- **Boundaries**: TB3 (validation unchanged); TB4 (RowKey bounds added, still parameterized `CreateQueryFilter` over constants and a validated partition, so T7 stays mitigated).
- **T3** mitigated. Per GET: one query of at most 10 entities, or three for a board without cached legacy rows; a merge of at most 20. Per board and instance: one legacy read, its cost fixed at deploy. Memory: only boards with legacy rows, 10 entries each; arbitrary `day` values cache nothing. A failing legacy read is retried after 5 minutes, not per call.
- **T2, T4** stay open (CHORE-013): flooding no longer slows reads but still grows the table and the cost.
- **Actors**: AC1, AC2, AC6, R1, R3 unchanged; R2 reworded, still no update, upsert or delete.
- **Other**: no auth or secrets change (CG24), error texts unchanged (T9), no new dependency or exposed data.

## UX considerations
None: not UI-facing.

## Risks and rollback
- **Slower first GET of a legacy board per cold instance** (one page per 1,000 legacy rows, fixed at deploy). A later item can archive legacy rows below each top 10.
- **Legacy read fails**: uncached boards show ranked scores only for 5 minutes; a warning is logged.
- **Deploy window** (accepted): a GUID row saved by old code appears on the next GET of an uncached board, else after the next cold start.
- **Paging**: a short page makes `TakeTopAsync` fetch the next one; it still stops at 10.
- **Rollback**: revert. Only normal score rows were written; old code reads ranked rows as ordinary rows.

## Out of scope
- Rate limiting and flood control (T2 and T4, CHORE-013), and moderation.
- Deleting legacy rows.
- `gethighscore`'s legacy `Current` entity.
- Any client change.

## Revisions
- Round 1: first version.
- Round 2: P1 migration dropped, legacy top 10s merged in memory, stays M. P2 named catch, `TryReadScore`, 5-minute retry. P3 deploy window accepted. P4 `public sealed`. P5 shortened.
- Round 3, P6: every query selects `PartitionKey`, `RowKey`, `Name`, `Score` (Step 4, Tests).
- Round 3, P7: the all-partition legacy scan is replaced by lazy in-partition legacy range reads through the pure `LegacyTopCache`: non-empty results cached only, instance-wide 5-minute back-off (Summary, Steps 3, 4, 9, 10, Tests, Security, Risks).
- Round 3, P8: Context found, Steps, Tests and Security trimmed.
- Round 3, P9: `GetTopAsync` awaits the cache's legacy load before `Merge` (Step 4).

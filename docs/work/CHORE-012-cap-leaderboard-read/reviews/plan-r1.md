VERDICT: CHANGES_REQUESTED

# Plan review: CHORE-012 Cap the leaderboard read, round 1

**Reviewed:** plan
**Reviewer:** Crab (Plan Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | major | Header **Size**, WP1 Size | The size is understated. The plan includes "a one-time data migration" of live production rows that runs automatically on the first anonymous GET. It also adds a concurrency guard for concurrent first calls, runs the same migration on several instances at once (it relies on idempotent upserts), adds a new DI-registered shared-service pattern, and needs a manual rollback cleanup. The size table names "data migration" and "concurrency" as L, and the plan's own text names both. | Either (a) classify the item and WP1 as L and add a **Split** section (e.g. item A: CG16 helper, `HighScoreStore`, table created once (AC3), ranked RowKey on save; item B: capped read and migration (AC1, AC2, AC4, AC5)), or (b) remove the data migration and keep M. One example for (b): a read-only legacy top 10 per partition, computed once per instance in memory and merged with the ranked range. The legacy set is frozen, so its size does not grow. There are no writes, no marker, no R2/AC6 changes, and rollback is just a revert. The cost is a scan of the legacy rows on every cold start. The Planner chooses. |
| P2 | minor | Step 3 (b), "A migration failure is logged and not retried on this instance" | Two gaps. (1) CG21 (must) allows a catch-all only at a boundary, so the store has to catch named exceptions, and the plan does not name them. (2) Legacy rows can hold a non-Int32 `Score`: the pre-77d621e JS `saveHighScore.js` upserted `Score: body.score` with no type check, and `TableEntity.GetInt32` throws `InvalidOperationException` on a type mismatch (12.9.1 XML docs). One such row fails the whole migration. That instance then serves the board without any legacy entries until it is recycled, which breaks AC4 there. If the exception escapes the store instead, the migration may rerun on every call. | Name the caught exceptions (`RequestFailedException`, `InvalidOperationException`) in step 3. Either skip and count unreadable rows rather than failing the migration, or state what the board serves after a failure and that the next init attempt does not run per call. Add the skipped-row case to `SelectLegacyTop`'s tests if rows are skipped in pure code. |
| P3 | minor | Risks and rollback | During a deploy, an old-code instance can still save a GUID-keyed row after the new code has written the marker. That score never shows on the board, and no later migration picks it up. | Add one line to Risks: the window is accepted, or the owner deletes the marker after the deploy settles so the next instance re-migrates (copies are idempotent). |
| P4 | nit | Step 3 | `HighScoreStore` must be `public`. It is a constructor parameter of the public function classes (primary constructors), so `internal` fails with CS0051. | Say `public sealed`. |
| P5 | nit | Whole plan | 13.5 KB against the M budget of about 10 KB. This is moot if P1 is answered with L. | Trim if the item stays M. |

## Attacks the plan survived
- Key ordering: `R` + (`int.MaxValue` - score) as `D10` covers every `int` (from 0000000000 to 4294967295), and an ordinal sort gives descending scores. The 9/10 and 999_999/1_000_000 test pairs catch padding bugs.
- Range collisions: `Guid.ToString()` is lowercase hex (`0-9` < `R`, `a-f` > `S`) and `Current` < `R`, so no legacy key falls in [`R`,`S`). `Meta` is unreachable from `TryGetLeaderboardPartition` and `PartitionFor`.
- Tie order (AC4): today's stable `OrderByDescending` over RowKey order breaks ties by legacy GUID. Copies use the legacy RowKey as the id, and new rows use a GUID, so ties are broken the same way.
- The legacy `HighScore`/`Current` row: it is on today's board, it is migrated like any other row, and the migration does not change `gethighscore`.
- AC2: a range filter on PartitionKey eq + RowKey with `maxPerPage: 10`, plus breaking out of the `AsyncPageable` enumeration, fetches one page. A short page with a continuation token costs one more fetch but still stops at 10.
- AC3: one creation per instance, cached; failure retries. After init, a call's work is a completed task.
- Arbitrary `day` values: the read is an empty range, and GET never writes for them (the marker is global, not per partition).
- Concurrent migrations and saves during migration: deterministic keys plus upserts, and the migration filter excludes ranked rows.
- Rollback: the duplicates that would appear are documented, and the `LegacyRowKey` property makes cleanup possible.
- Response format and order of status checks (400/413 before 500 for "Storage not configured") are kept.
- Security model: T3, TB4, R2, AC6 and T7 are all updated in step 8. The "no migrations" wording in A4 is reworded in step 9.

## Checked and fine
- `TableClient.CreateIfNotExistsAsync`, `GetEntityIfExistsAsync`, `UpsertEntityAsync`, and `QueryAsync` with `maxPerPage`/`select` exist in `Azure.Data.Tables` 12.9.1, the version in the local NuGet cache.
- The test csproj's `<Compile Include>` pattern works for `LeaderboardRanking.cs` and `StoredScore.cs`, which have no Azure references.
- `Program.cs` uses `HostBuilder().ConfigureFunctionsWorkerDefaults()`, so `ConfigureServices` + `AddSingleton` work without a new package.
- No client or other code depends on RowKey being a GUID; only the JSON `{name, score}` is consumed.
- The tests are unit-level with no storage, which is consistent with A5. No Testcontainers.
- Security-sensitive: yes and UI-facing: no are correct.

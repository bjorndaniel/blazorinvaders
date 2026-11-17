VERDICT: APPROVED

# QA review: CHORE-012 Cap the leaderboard read, round 2

**Reviewed:** the changes since round 1 (`changes CHORE-012-cap-leaderboard-read --since 1`). That covers `src/api/GetLeaderboard.cs`, `src/api/HighScoreStore.cs`, `src/api/LegacyTopCache.cs`, `docs/security-model.md` and `docs/work/TRACKER.md`. No test code changed.
**Reviewer:** Octopus (QA Reviewer)

## Earlier findings
| ID | Status | Evidence |
|----|--------|----------|
| Q1 | resolved | All four `_table!` uses are gone. `HighScoreStore.cs:40` adds `private TableClient Table => _table ?? throw new InvalidOperationException("Storage not configured");`, and `AddAsync`, `GetTopAsync`, `EnsureTableAsync` and `LoadLegacyAsync` all use it. Grep finds no `_table!` left. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q2 | minor | `src/api/HighScoreStore.cs:28` | WP1.md says the C5 fix is done, but it only partly works. The filter `when (ex is FormatException or ArgumentException)` misses the most common malformed connection strings. I probed `new TableClient(s, "HighScores")` against Azure.Data.Tables 12.9.1 with a file-based app in my scratchpad, outside the worktree. `"garbage"` and `"a=b=c;;"` throw `System.InvalidOperationException`. `"AccountName=x"` and a bad `TableEndpoint` throw `ArgumentException`, and a bad `AccountKey` throws `FormatException`. So a non-key/value setting still escapes the constructor and fails DI activation of both functions, outside their `Run` catch-alls. That is the exact case C5 describes. This is error handling, but only for a misconfigured app, and no details leak (T9). That is why it is minor, the same severity C5 had. | Add `InvalidOperationException` to the filter (`ex is FormatException or ArgumentException or InvalidOperationException`). Or record in the implementation notes that only these two types are handled. |

## QA evidence
Assurance: **standard**. `quality` is not configured, so there was no mutation run and no `crap` run, as in round 1.

### Traceability
Only the round 2 changes are traced here. Round 1 covered all the unchanged code: every plan step, every row of the Tests table and the package ownership.

| Change | Serves | Status |
|---|---|---|
| `HighScoreStore.cs:40` `Table` accessor, used at `:52`, `:60`, `:84`, `:105` | Step 4 (CG30), C1/Q1 | done |
| `HighScoreStore.cs:22-33` the constructor catches `FormatException`/`ArgumentException` and logs | Step 4, C5 | partial (Q2) |
| `HighScoreStore.cs:149-152` `Name` is read with `TryGetValue` and `as string ?? ""` | Step 4, S2 | done. A non-string `Name` now shows as `""`, which matches a missing name, and no longer faults the legacy load. |
| `GetLeaderboard.cs:18` the 400 goes through `FunctionResponses.TextAsync` | Step 1/6, C3 | done. Code and text are unchanged. |
| `GetLeaderboard.cs:34-37` an `OperationCanceledException` is answered with a 500 and not logged, but only when the request token is cancelled | Step 6, C7 | done. The status and text are unchanged. A cancellation by the SDK's own timeout does not match the filter and is still logged. |
| `GetLeaderboard.cs:41` the 500 goes through `TextAsync` | Step 6, C3 | done |
| `LegacyTopCache.cs:21-22` `CachedCount` is now `internal` with a "For tests" summary | Step 3, C4 (CG14) | done. The test project compiles this file in, and the tests pass. |
| `LegacyTopCache.cs:55` comment on the CG21 departure | C2 | done. The departure is declared in WP1.md round 2. |
| `docs/security-model.md` R5: a removed legacy row needs a restart | Step 9, S1 | done |
| `docs/work/TRACKER.md` size changed to `M` | PM-owned | done (my round 1 note) |

Unplanned hunks: none. Every change traces to a round 1 finding and is recorded in WP1.md round 2. Every changed file belongs to WP1, except TRACKER.md, which the PM owns.

### Coverage and CRAP
`quality.coverage` is not set up. Coverage did not change since round 1: the code changed this round is in `HighScoreStore.cs` and `GetLeaderboard.cs`. The test project does not compile these files, by design (plan Tests "no storage", A5). `LegacyTopCache.cs` changed only in a comment and in visibility, so its round 1 coverage still holds (100% lines). The changed lines in `HighScoreStore.cs` (22-33, 40, 149-152) and `GetLeaderboard.cs` (18, 34-37, 41) are run by no test and were checked by review only. Q2 is the result of that check.

### Mutation testing
Not run: `quality.mutation` is not set up.

### Breaks
None this round. Round 1 had no survivors to run again. The new logic sits in the constructor, `ToStoredScore` and `GetLeaderboard.Run`, and no test compiles those files. Any break there would survive trivially, and that test gap was planned (A5). In `LegacyTopCache.cs` only a comment and a visibility modifier changed, with no logic. I checked the constructor catch with a direct probe of the SDK instead (Q2).

Baseline in the scratch copy:
- `scratch -- dotnet test tests/BlazorInvaders.Tests --nologo`: 229 passed, 0 failed, 0 skipped.
- `scratch -- dotnet build src/api --nologo`: 0 warnings, 0 errors.

## Checked and fine
- `Table` throws `InvalidOperationException` when it is null. Every function that reaches the store checks `IsConfigured` first, so the throw is unreachable in normal use. If it ever is reached, the `Run` catch-alls turn it into the generic 500.
- The cancellation filter uses the same token that is passed to `GetTopAsync` (`req.FunctionContext.CancellationToken`, `GetLeaderboard.cs:26`), so it matches exactly the client-disconnect case.
- Error texts and status codes are unchanged (T9).

## Notes for the PM
- The brief held only template fields and the task. Nothing in it suggested findings or a verdict.
- Q2 comes from a runtime probe of Azure.Data.Tables 12.9.1, in a throwaway file-based app in my scratchpad (`C:\Users\elici\AppData\Local\Temp\claude\abad4d2d2f6bcd2d\41501319-ac2a-40b7-9c4d-6a53fd059269\scratchpad\tc\tc.cs`), outside the worktree.

Restored: yes

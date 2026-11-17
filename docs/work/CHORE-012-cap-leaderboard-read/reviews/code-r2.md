VERDICT: APPROVED

# Code review: CHORE-012 Cap the leaderboard read, round 2

**Reviewed:** changes since round 1 (`devagents.mjs changes --since 1`)
**Reviewer:** Saucer (Code Reviewer)

## Earlier findings
| ID | Severity | Status | Why |
|----|----------|--------|-----|
| C1 | major | resolved | The four `_table!` uses are gone. They now go through `private TableClient Table => _table ?? throw new InvalidOperationException(...)` (src/api/HighScoreStore.cs:40). No null-forgiving `!` is left in the file (CG30). |
| C2 | minor | resolved | There is a comment at src/api/LegacyTopCache.cs:55, and implementation/WP1.md round 2 declares the CG21 departure. |
| C3 | minor | resolved | The 400 and both 500 replies use `FunctionResponses.TextAsync` (GetLeaderboard.cs:18, 36, 41). |
| C4 | minor | resolved | `CachedCount` is now `internal` with "For tests." in its summary. The tests still compile, and 229 pass. |
| C5 | minor | resolved | The constructor catches `FormatException or ArgumentException`, logs it and leaves `_table` null. Version 12.9.1 of `TableConnectionString.Parse` documents exactly `ArgumentNullException`, `FormatException` and `ArgumentException`, so the filter covers them. `UriFormatException` is a `FormatException`. |
| C6 | nit | open (not changed) | The implementer did not change it. That is acceptable: it is a one-off cost per board and instance. |
| C7 | nit | resolved | A client cancellation no longer reaches the error log (GetLeaderboard.cs:34). See C8. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C8 | minor | src/api/GetLeaderboard.cs:34-37 | The new `catch (OperationCanceledException) when (...)` neither logs nor says why. CG20 (must) allows an unlogged catch only for an expected, safe-to-ignore failure, and only with a comment saying why. The implementation notes say the intent ("cancellation by the client no longer logged as an error"), so this is minor and not blocking. | Add a one-line comment, e.g. `// The client disconnected: nothing to log, the response is never read`, or log it at `Information`/`Debug`. |

## Checked and fine
- `Table` throws only in a case that cannot happen: all callers check `IsConfigured` first, and `LoadLegacyAsync` runs only after `EnsureTableAsync` has used `Table`.
- The exception filter on the cancellation catch is tied to the request's own token, so a storage timeout or another `OperationCanceledException` is still logged by the catch-all.
- The response texts and status codes are unchanged (400 query error, 500 "Storage not configured" and "Could not load leaderboard").
- `ToStoredScore` reads `Name` with `TryGetValue` + `as string ?? ""`, so a non-string `Name` no longer throws (S2). A `null` or missing `Name` still reads as `""`.
- The R5 edit in docs/security-model.md matches the per-instance legacy cache (S1).

# Implementation: CHORE-012 Cap the leaderboard read — WP1

## Round 1
**Plan steps covered:** WP1 steps 1-10
**Addresses findings:** P10 (fixed: a faulted load counts as failed and sets the back-off; the entry is removed only if still the same `Lazy`, via `ICollection<KeyValuePair>.Remove`; test `FaultedLoad_CountsAsFailed_AndIsNotKept`)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/api/FunctionResponses.cs` | added |
| `src/api/StoredScore.cs` | added |
| `src/api/LeaderboardRanking.cs` | added |
| `src/api/LegacyTopCache.cs` | added |
| `src/api/HighScoreStore.cs` | added |
| `src/api/GetLeaderboard.cs` | modified: uses the store |
| `src/api/SaveHighScore.cs` | modified: uses the store and `FunctionResponses` |
| `src/api/Program.cs` | modified: registers `TimeProvider` and `HighScoreStore` |
| `tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj` | modified: compiles in the three pure files |
| `tests/BlazorInvaders.Tests/LeaderboardRankingTests.cs` | added |
| `tests/BlazorInvaders.Tests/LegacyTopCacheTests.cs` | added |
| `docs/security-model.md` | modified: T3 mitigated, TB4, R2, T7, Revisions |
| `docs/architecture.md` | modified: A4, A5, A11, Decisions |

### Deviations from plan
- `src/api/GetHighScore.cs` no longer exists (removed by CHORE-008), so nothing was changed there.
- `LegacyTopCache` takes an optional `onLoadFailed` callback so a faulted load is logged (needed for P10; the cache has no logger).

### Commands run
- `dotnet build src/api`: 0 warnings, 0 errors
- `dotnet test tests/BlazorInvaders.Tests`: 227 passed
- `dotnet format src/BlazorInvaders.sln --verify-no-changes`: clean

## Round 2
**Addresses findings:** C1/Q1 (fixed: `Table` accessor throws `InvalidOperationException`), C2 (comment added; CG21 departure declared here: `LegacyTopCache.LoadAsync` catch-all is the boundary of a shared background load, logged via `onLoadFailed`), C3 (fixed: `TextAsync` for 400 and 500), C4 (fixed: `internal`, "For tests"), C5 (fixed: `FormatException`/`ArgumentException` logged, `_table` left null), C7 (fixed: cancellation by the client no longer logged as an error), S1 (documented in R5: legacy row removal needs a restart), S2 (fixed: `Name` read with `TryGetValue` and `as string`)
**Not changed:** C6 (nit, one-off cost per board and instance)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/api/HighScoreStore.cs` | modified |
| `src/api/LegacyTopCache.cs` | modified |
| `src/api/GetLeaderboard.cs` | modified |
| `docs/security-model.md` | modified: R5 |

### Commands run
See final response.

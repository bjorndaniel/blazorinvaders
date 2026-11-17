VERDICT: APPROVED

# Security review: CHORE-012 Cap the leaderboard read, round 2

**Reviewed:** changes since round 1 (`devagents.mjs changes CHORE-012-cap-leaderboard-read --since 1`): `src/api/GetLeaderboard.cs`, `src/api/HighScoreStore.cs`, `src/api/LegacyTopCache.cs`, `docs/security-model.md`, `docs/work/TRACKER.md`. I also re-checked the code around each change and scanned the item folder for secrets.
**Reviewer:** Bunker (Security Reviewer)

## Threat model
- Entry points are unchanged: anonymous `GET /api/getleaderboard` (TB3) and `POST /api/savehighscore` (TB2), both reaching Table Storage (TB4). Actors are AC1/AC2 (anonymous) and AC3 (moderates in storage, R5).
- This round changes error handling (T9), how stored rows are mapped (the legacy read behind T3), and the R5 moderation text behind T1/T6.
- There are no new actors, permissions, boundaries or threats. The R5 edit is the documentation fix for S1.

## Earlier findings
| ID | Status | Evidence |
|----|--------|----------|
| S1 | resolved | `docs/security-model.md:34` (R5) now says a removed legacy (GUID-keyed) row stays on warm instances until the app restarts. That was option (a) of the required change. |
| S2 | resolved | `src/api/HighScoreStore.cs:146-152` reads `Name` with `TryGetValue` plus `as string`. A row with a non-string `Name` now maps to `""` and no longer fails the whole legacy load or restarts the back-off. |

## Findings
None.

## Checked and fine
- **Logging an invalid connection string** (`HighScoreStore.cs` constructor). I read the strings in Azure.Data.Tables 12.9.1. Some of its `FormatException` messages quote part of the connection string (for example "Connection string has value '...'"), so a malformed string written by AC3 could put a fragment of it in the server log. This is not new: at base `1c264bf`, `GetLeaderboard` built the `TableClient` inside a `try` and logged the same exception with `LogError(ex, ...)`. Logs stay on the server (T9), and only AC3 can set the app setting. Clients still get the generic "Storage not configured" 500.
- **Exceptions outside the constructor filter** (`InvalidOperationException` and others). These would fail DI resolution, and the host would return a generic 500. Only AC3 can cause this, by misconfiguring the setting, and no internal details reach the client.
- **The `Table` accessor** throws `InvalidOperationException("Storage not configured")`. It cannot be reached in practice, because both functions check `IsConfigured` first. If it were reached, the existing catch would log it on the server and return the fixed 500 text.
- **The client-cancellation catch** (`GetLeaderboard.cs:34`) is filtered on the request's own token, returns the fixed 500 text and logs nothing. Nothing is exposed, and a client cannot suppress logging of any other failure through it.
- **`FunctionResponses.TextAsync`** writes the same fixed texts as before: the validator's 400 messages and the generic 500s. T9 holds.
- **`CachedCount` is now `internal`**, which removes a public surface. The comment added in `LegacyTopCache` does not change behaviour.
- **Query filters and the T3 bound** are unchanged from round 1: `CreateQueryFilter` is still parameterised, and the ranked read still has `maxPerPage: 10`.
- **The item folder and tests** contain no connection strings, account keys or SAS tokens, and no text tries to instruct an agent. The TRACKER change is only metadata.

No project scanners were approved for me, and none were run.

## Notes for the PM
- The brief held only its template fields and the task, with nothing else to ignore.
- Files read: `C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-012-cap-leaderboard-read\src\api\HighScoreStore.cs`, `C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-012-cap-leaderboard-read\src\api\GetLeaderboard.cs`, `C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-012-cap-leaderboard-read\src\api\LegacyTopCache.cs`, `C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-012-cap-leaderboard-read\docs\security-model.md`.

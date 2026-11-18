# Plan: CHORE-013 Rate limiting and score plausibility check

| | |
|---|---|
| **ID** | CHORE-013 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-18 |
| **Size** | M — one area (score submission: API, validator, one client method, provisioning), ~400 lines with tests, no new dependency |
| **Security-sensitive** | yes — input validation at TB2, client IP handling (personal data), CI/infrastructure (`provision.yml`) |
| **UI-facing** | no — a 429 is handled like being offline; nothing the player sees changes |
| **Opened** | 2026-11-17 |
| **Plan round** | 3 |

## Summary
The API counts score posts per client in memory and answers 429 past the limit, checks each score against the most a game could earn by the reported wave, and provisioning caps scale-out and sets a daily quota.
- Rate limit: 20 posts per client per fixed hour, keyed on an HMAC-SHA256 of the client IP (IPv6 by /64) with a per-process random salt. Memory only; an hourly timer drops ended counters (at most 2 h). Past 10,000 tracked clients, new ones share one limited overflow bucket. Per instance, so exact at the default cap of 1.
- Client IP: `X-Client-IP` (set by the App Service front end), else the rightmost `X-Forwarded-For` entry; checked after deployment.
- Plausibility: `wave` is required, the score must be a multiple of 10 and within a generous per-wave bound from the engine's scoring, doubled for daily games. Old clients without `wave` get 400.
- Client: 429, 503 and 403 (a quota-stopped app) are queued and retried like network failures.
- Provisioning: scale limit 1 and daily quota 10,000 GB-s (D3), checked by the shared `validate-dispatch-inputs.sh` as 1 or more. Its wrong `RESOURCE_GROUP` error text (CHORE-015 C4/Q6) is fixed too.
- WP1 only; security model T1/T2/T4/TB8 and architecture updated.

## Context found
- `src/api/SaveHighScore.cs` — the POST endpoint; takes `HighScoreStore` and answers through `FunctionResponses.TextAsync` (CG16, done by CHORE-012). `src/api/Program.cs` registers `TimeProvider.System` and `HighScoreStore` as singletons; `LegacyTopCache.cs` is the pattern for an injected `TimeProvider` and an `internal` count for tests.
- `src/api/HighScoreValidator.cs` — `TryParse(json, today, ...)`. New rules go here (A20, CG22).
- `src/Pages/Index.razor` — `SendScoreAsync` returns false (queue) only on `HttpRequestException`; `SendPendingScoresAsync` stops at the first failure.
- `src/core/GameObjects/HighScore.cs` — the client DTO, also used to read the boards.
- `src/core/GameEngine.cs` scoring: grid 990, combo x4 (aliens, boss hits), mothership max 300 (not multiplied), boss hit 50, boss HP `30+10(k-1)`, kill bonus `1000k`, perfect wave 500, Glass x2. Boss every 5 waves, or 3 with BossRush. At most ~175 motherships a wave (~19 traverses of ~18 ticks, cleared 2 ticks after a hit); the plan uses 200.
- `tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj` compiles `HighScoreValidator.cs`, `StoredScore.cs`, `LeaderboardRanking.cs` and `LegacyTopCache.cs` in. Core has `InternalsVisibleTo` for tests.
- `.github/workflows/provision.yml` passes inputs through job-level `env:` (T12) and validates them first in "Validate inputs" with `.github/scripts/validate-dispatch-inputs.sh` (allow-list `PATTERN`/`RULE` maps, values never printed; TB8). `lint-workflows.yml` runs `validate-dispatch-inputs.test.sh` (table-driven `check` cases).
- Docs: `docs/security-model.md` (TB2, TB8, T1, T2, T4, T12, R3, AS5, Q1; T3 already mitigated by CHORE-012), `docs/architecture.md` (A2, A5, A7, A11, A20, A23), `docs/ux-guidelines.md` UX3, UX25. Decision D1 in `status.md`.
- Azure docs (event-driven-scaling): `az resource update ... /config/web --set properties.functionAppScaleLimit=N`; `az functionapp update --set dailyMemoryTimeQuota=N`; a hit quota stops the app until the next UTC day. `X-Client-IP` is documented only in Microsoft Q&A, so it is verified after deployment (Risks).
- `TimeProvider` (BCL since .NET 8; the projects target net10.0) gives the prune timer through `CreateTimer`. The API never returns 403 (`src/api`), so a 403 can only come from the platform.
- Guidelines: CG12 (named constants), CG14 (internal-for-tests), CG15, CG16 (use `FunctionResponses`), CG17, CG20/21, CG22, CG24 (never log the IP or hash), CG30, CG31/CG51 (time injected), CG42, CG50, CG52–54, CG60, CG63, CG70. `.editorconfig` / `src/.editorconfig`.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Rate limiter, plausibility, client retry, provisioning, docs | M | `src/api/ScoreRateLimiter.cs` (new), `src/api/SaveHighScore.cs`, `src/api/Program.cs`, `src/api/HighScoreValidator.cs`, `src/core/GameObjects/HighScore.cs`, `src/core/GameEngine.cs`, `src/Pages/Index.razor`, `.github/workflows/provision.yml`, `.github/scripts/validate-dispatch-inputs.sh`, `.github/scripts/validate-dispatch-inputs.test.sh`, `README.md`, `docs/security-model.md`, `docs/architecture.md`, `tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj`, `tests/BlazorInvaders.Tests/HighScoreValidatorTests.cs`, `tests/BlazorInvaders.Tests/ScoreRateLimiterTests.cs` (new), `tests/BlazorInvaders.Tests/ScorePlausibilityTests.cs` (new), `tests/BlazorInvaders.Tests/HighScoreTests.cs` (new) | — |

## Steps
### WP1
1. **Input validation** — `.github/scripts/validate-dispatch-inputs.sh`:
   - Add `MAX_INSTANCES` and `DAILY_QUOTA_GB_SECONDS` to `PATTERN` as `^[1-9][0-9]{0,8}$` (fits the int both settings take; 0 would mean no limit), with a `RULE` text "a whole number from 1 to 999999999".
   - Fix the `RESOURCE_GROUP` `RULE` text to end "not ending with ." (drop "or -"): the pattern allows a trailing `-`, as Azure does (CHORE-015 C4/Q6).
   - `validate-dispatch-inputs.test.sh`: add the cases in **Tests**, and add both names to the `env -u` list in `check`.
2. **Rate limiter** — `src/api/ScoreRateLimiter.cs`, a sealed class with BCL only (it is compiled into tests):
   - Constants `MaxPostsPerWindow = 20`, `Window = 1 h` and `MaxTrackedClients = 10_000`, each with a reason comment (the burst covers the 10-score queue plus the current game).
   - `IDisposable`. A public constructor `(TimeProvider time)` for DI, with the salt `RandomNumberGenerator.GetBytes(32)`, never logged or stored; an internal one `(TimeProvider time, byte[] salt, int maxTrackedClients)` for tests (DI only sees the public one). It starts a timer with `TimeProvider.CreateTimer` that calls `Prune(time.GetUtcNow())` every `Window`, so expiry does not depend on POST traffic; `Dispose` stops it (DI disposes the singleton).
   - `static string? ClientAddress(string? xClientIp, string? xForwardedFor)`: `X-Client-IP` if present, else the rightmost XFF entry.
   - `internal string ClientKey(string? address)`: strip any port (`1.2.3.4:5678`, `[::1]:80`), map IPv4-mapped IPv6 to IPv4, and mask IPv6 to /64. Hex HMAC-SHA256. A missing or unparsable address uses one shared `unknown` bucket.
   - `internal void Prune(DateTimeOffset now)`: under the lock, removes every entry whose window has ended. With the timer, an entry lives at most 2 x `Window`.
   - `bool TryAcquire(string? address, DateTimeOffset now, out TimeSpan retryAfter)`: a fixed window under a `lock`. The window has ended when `now >= start + Window`. When a new key arrives and the table holds `MaxTrackedClients`, call `Prune(now)` if the last prune is at least `InlinePruneInterval` (1 min) old, so a table kept full costs one scan a minute, not one per request; if it is still full, count the request against one shared overflow bucket (a reserved key, limited like any other), so filling the table never switches the limit off.
   - `internal int TrackedClients` for tests (CG14).
3. **Wire it** — `src/api/Program.cs`: `services.AddSingleton<ScoreRateLimiter>()` next to `HighScoreStore`. `SaveHighScore` takes the limiter as a third primary-constructor parameter. It checks first, before reading the body, so every POST counts, with headers from `req.Headers.TryGetValues`. Over the limit it returns 429 "Too many scores, try again later" with `Retry-After` (whole seconds, rounded up; built inline, header added before the body is written, as `FunctionResponses.TextAsync` takes no headers) and logs a warning without the address or key.
4. **Plausibility** — `src/api/HighScoreValidator.cs`:
   - Add `int Wave = 0` to `HighScoreRequest`.
   - In `TryParse`, after the range check: reject `Wave < 1` ("Wave reached is required"), a score not divisible by 10, and a score over `MaxPlausibleScore(wave, daily)`. The last two say "Score is not possible for the wave reached".
   - `internal static long MaxPlausibleScore(int wave, bool daily)` is the sum over waves 1..wave of `factor * max(alien-wave max, boss-wave max if the wave could be a boss wave)`:
     - alien-wave max = `990*4 + 200*300 + 500`
     - boss-wave max for boss k = `50*4*(30+10(k-1)) + 1000k + 500`
     - all-time: factor 1, boss every 5. Daily: factor 2, boss every 3 or 5 (worst case, since the modifier is not known).
   - Write the scoring copies as internal constants with "must match `GameEngine.cs`" comments (CG13 style), and the mothership derivation in a comment. Long arithmetic, with an early exit once the bound passes `MaxScore`.
5. **Engine internals for the drift test** — `src/core/GameEngine.cs`: make `BossHitPoints`, `BossKillBonus`, `MotherShipScoreTable` and `GetAlienPoints` `internal`, each with a "for tests" doc comment. No behaviour change.
6. **Client DTO** — `src/core/GameObjects/HighScore.cs`:
   - Add `Wave` (`"wave"`, omitted when 0) with a summary.
   - Add `public static bool ShouldRetrySend(HttpStatusCode status)`: true for 429, 503 and 403 only, with a summary (CG60) saying why 403: the API never returns it, so it comes from the platform, e.g. a quota-stopped app.
7. **Client** — `src/Pages/Index.razor`:
   - `SubmitHighScoreAsync` sets `score.Wave = _engine.Wave`.
   - `SendScoreAsync`: if `ShouldRetrySend`, log "High score not sent, will retry later: <code>" with `Console.WriteLine` and return false (queued). Otherwise keep the current behaviour.
   - Update the "only network failures" comment.
8. **Provisioning** — `.github/workflows/provision.yml`:
   - Add inputs `max_instances` (default `1`) and `daily_quota_gb_seconds` (default `10000`), mapped in the job `env:` as `MAX_INSTANCES` and `DAILY_QUOTA_GB_SECONDS`, and add both names to the "Validate inputs" call (step 1), so a bad value fails before anything is created.
   - Add a step "Set scale limit and daily quota" after "Configure Function App settings": the `az resource update ... functionAppScaleLimit` command and `az functionapp update --set dailyMemoryTimeQuota`, with quoted `"$VAR"`s.
   - Add both values to the step summary.
9. **README** — a short "Abuse and cost limits" section:
   - the per-client limit
   - the two `az` commands for an app that already exists (provisioning is not re-run)
   - that hitting the quota stops the API until 00:00 UTC; scores then fail as a network error or 403 and are queued
   - a post-deployment check: send 21 POSTs with an invalid body (each a 400, so nothing is saved; the limiter runs first), each with a different forged `X-Client-IP` and `X-Forwarded-For`. The 21st must get 429; a 400 means the header is forgeable, and the build is rolled back
10. **Security model** — `docs/security-model.md`:
    - TB2: the limiter, the `wave` and multiple-of-10 checks, and trust in `X-Client-IP`. TB8: the two new numeric inputs.
    - T1 → partly. Forged scores within the bound are still possible, and a high reported wave is not checked (no server-run game).
    - T2 → partly. IP rotation, botnets and IPv6 beyond /64 bypass the limit, and shared NAT users share one.
    - T4 → partly. The cost ceiling comes from the cap and quota. A flood can exhaust the quota and take the API down for the day (availability traded for cost).
    - Assumptions: hashed IPs are held only in memory, at most 2 h (timer prune). Q1 stays open.
    - Add a Revisions line.
11. **Architecture** — `docs/architecture.md`:
    - A2: one singleton service, in-memory and per instance.
    - A5/A11: tests also compile in `ScoreRateLimiter.cs`.
    - A7: provisioning sets the scale limit and the quota (validated inputs).
    - A20: plausibility rules.
    - A23: the 429 response and its client retry.
    - Add a Decisions line.
12. **Tests** — see the table. Update the existing fixtures in `HighScoreValidatorTests` to send `"wave":1`. `MaxScore_IsAccepted` becomes 999,990 at the first wave whose bound reaches it. Add `ScoreRateLimiter.cs` to the test csproj `Compile` items.

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC1, AC5 | `ScoreRateLimiterTests.TryAcquire_PastLimit_RejectsWithRetryAfter`: 20 allowed; the 21st, 10 min into the window, returns false with retryAfter = 50 min | unit | WP1 |
| AC1, AC5 | `TryAcquire_WindowEnds_AllowsAgain`: at exactly start+1 h it is allowed, at 1 h − 1 tick still rejected. `TryAcquire_OtherClient_IsIndependent` | unit | WP1 |
| AC1, AC5 | `TryAcquire_TableFull_UsesSharedOverflowBucket`: cap 3; after 3 tracked clients, 20 posts from 20 new addresses are allowed and the 21st new-address post is rejected; `TrackedClients` does not grow past cap + overflow | unit | WP1 |
| AC1, AC5 | `TryAcquire_TableFullOfEndedWindows_PrunesInline`: cap 3, three clients at t0; a new address at t0+1 h is tracked (`TrackedClients` = 1), not put in the overflow bucket | unit | WP1 |
| AC2, AC5 | `ClientKey_IsSaltedHash`: the key contains neither the IP text nor its plain SHA-256. The same IP and salt give the same key, another salt a different one | unit | WP1 |
| AC2, AC5 | `ClientKey_Normalizes`: `1.2.3.4:5678` = `1.2.3.4`; `2001:db8:1:2::a` = `2001:db8:1:2::b`, ≠ `2001:db8:1:3::a`; `::ffff:1.2.3.4` = `1.2.3.4`; garbage or null → one shared key | unit | WP1 |
| AC2, AC5 | `ClientAddress_PrefersXClientIp_ElseRightmostForwardedFor` | unit | WP1 |
| AC2, AC5 | `Prune_RemovesEndedWindowsOnly`: clients at t0 and t0+30 min; `Prune(t0+1 h)` leaves 1, `Prune(t0+1 h 30 min)` leaves 0, with no `TryAcquire` in between | unit | WP1 |
| AC3, AC5 | `HighScoreValidatorTests`: wave missing, 0 or −1 rejected. 105 at wave 1 rejected, 110 accepted. All-time wave 1: exactly `MaxPlausibleScore(1,false)` accepted, +10 rejected. Daily wave 1 accepts 2× the all-time bound, which all-time rejects | unit | WP1 |
| AC3, AC5 | `ScorePlausibilityTests.ValidatorScoring_MatchesEngine`: the validator's copies equal the engine's (grid points from a started engine's `Aliens` via `GetAlienPoints`, `ComboTracker.MaxMultiplier`, `BossHitPoints`, `BossKillBonus`, `MotherShipScoreTable.Max()`, boss `MaxHp` at waves 5 and 10, `PerfectWaveBonus` all-time and Glass) | unit | WP1 |
| AC3, AC5 | `MaxPlausibleScore_GrowsWithWave` and `_BossWaveAtLeastBossMax` (waves 3 and 5 daily, 5 all-time) | unit | WP1 |
| AC1, AC5 | `HighScoreTests.ShouldRetrySend`: 429, 503, 403 → true; 200, 400, 413, 500 → false | unit | WP1 |
| AC4 | `validate-dispatch-inputs.test.sh`: `MAX_INSTANCES` and `DAILY_QUOTA_GB_SECONDS` accept `1`, `10000`, `999999999`; reject `0`, `01`, `-1`, `1.5`, ` 1`, `1e4`, empty and `1000000000` (10 digits). Also `RESOURCE_GROUP=rg-` exits 0 (C4/Q6 regression). Run by `lint-workflows.yml` | script | WP1 |
| AC4 | The `az` step itself: the reviewer checks it; the owner confirms in Azure after a provisioning run | manual | WP1 |
| AC1, AC2 | Forged-header check after deployment (step 9) | manual | WP1 |
| AC6 | Doc change, checked in review | — | WP1 |

There are no HTTP or integration tests for `SaveHighScore` (A5: the shells stay thin and untested). The limiter is in memory, so no container is needed.

## Security considerations
- **Boundaries and threats:** TB2 (new: limiter, wave/multiple-of-10 rules, trusted `X-Client-IP`). AC1/AC2 unchanged: still anonymous, still indistinguishable. T1, T2, T4 → partly (step 10). TB8/T12: two new dispatch inputs, allow-listed. T3 is already mitigated by CHORE-012.
- **Personal data:** the raw IP is never stored, logged or returned. The HMAC key and the counter live in memory only, the salt is random per process, and the timer removes records within 2 h whatever the traffic (D1, AC2). Q1 stays open.
- **Spoofing:** `X-Client-IP` is overwritten by the App Service front end. A caller-set XFF is used only when that header is missing, and then only its rightmost entry, which the front end appends. Verified by the forged-header check (step 9).
- **Memory and CPU:** the tracked-client cap bounds memory (about 1 MB). Past it, new clients share one limited overflow bucket, so the limit stays on; players in it may get 429 and their scores are queued. A full table is scanned inline at most once a minute.
- **CI:** new dispatch inputs reach the shell only through `env:` and pass the shared allow-list (`^[1-9][0-9]{0,8}$`) before Azure login (T12, TB8). No new secrets.
- **Dependencies:** none. HMACSHA256, `RandomNumberGenerator`, `IPAddress` and DI are in the BCL or the existing worker packages.
- **Errors:** fixed messages, and no exception text (CG24, T9).

## UX considerations
None: not UI-facing. A 429 or 503 follows UX3/UX25: silent, logged to the console, and queued.

## Risks and rollback
- **`X-Client-IP` missing on the Windows consumption front end:** every client falls into the XFF fallback, or into the `unknown` bucket, which would throttle everyone together. A forgeable `X-Client-IP` would defeat AC1. Mitigation: the forged-header check (step 9) right after deployment, and watch for "unknown"-bucket rejections (the warning names the bucket type, never the address). Rollback: redeploy the previous build.
- **Real games rejected:** possible if the engine's scoring changes. The drift test fails when a copied constant changes.
- **Clients without `wave`:** old cached PWAs and scores already in the queue get 400 and are dropped once. Accepted: the service worker updates on the next load.
- **Quota too low:** the API stops for the rest of the UTC day, and the game queues scores. The owner raises the quota with the README command.
- **Existing Function App:** its cap and quota are unset until the owner runs the README commands.
- **Rollback:** revert the merge. To remove the cap and quota in Azure, set `functionAppScaleLimit` and `dailyMemoryTimeQuota` to 0.

## Out of scope
- Retrying 500 responses.
- Using `Retry-After` on the client.
- Rate-limiting the GET endpoints (T3, T4 GET side; the quota covers cost).
- Storing the wave.
- Server-side game simulation.
- Offensive names (T6).

## Revisions
- Round 1: first draft.
- Round 2 (plan-r1): P1 timer-driven `Prune` (steps 2, 10, Security, Tests); P2 shared overflow bucket (step 2, Tests); P3 forged-header check and 403 retry (steps 6, 9, Risks, Tests); P4 guard `^[1-9][0-9]*$` (step 8); P5 Context found trimmed.
- Round 3 (rebase to feee1ce, plan-r2): `FunctionResponses` exists (CHORE-012), so old step 1 is dropped and `FunctionResponses.cs`, `GetHighScore.cs` (deleted by CHORE-008) and `GetLeaderboard.cs` leave WP1; DI and `TimeProvider` wiring follow `Program.cs`/`HighScoreStore` (steps 2, 3). New step 1 puts the new inputs into `validate-dispatch-inputs.sh` (CHORE-015) with tests, bounded to 9 digits; step 8 uses it. CHORE-015 C4/Q6: `RESOURCE_GROUP` text fixed (step 1, Tests). P6: inline prune at most once a minute (step 2, Security, Tests). T3 note updated (Security).

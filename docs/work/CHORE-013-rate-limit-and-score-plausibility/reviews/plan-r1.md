VERDICT: CHANGES_REQUESTED

# Plan review: CHORE-013 Rate limiting and score plausibility check, round 1

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | major | Step 2 (pruning in `TryAcquire`), Step 10 ("held only in memory, at most 1 h"), Security considerations ("records go within 1 h") | Expired entries are only pruned inside `TryAcquire`, so only when a score is posted. Posts are rare, because a client posts only when a player makes the top 10. GETs are frequent, since every page load calls `getleaderboard`/`gethighscore` (`Index.razor:104,185`). Those GETs keep the consumption instance warm but never prune. Scenario: client A posts at 10:00, nobody posts for two days, and leaderboard reads keep the process alive. A's hashed-IP record stays in memory for two days. That breaks AC2 and D1 ("records expire within 24 hours", "kept at most 24 hours"). The doc line "at most 1 h" is also false even in the best case: an entry whose window ends just after a prune lives until the next prune, so close to 2 h. | Make expiry independent of POST traffic. For example, a BCL `System.Threading.Timer` (or a hosted service) in the singleton that calls an `internal Prune(DateTimeOffset now)` every `Window`, or call `Prune` from the GET endpoints too. Unit-test `Prune` with injected time. State the real bound (at most 2 × `Window`) in step 10 and in Security considerations. |
| P2 | minor | Step 2 ("When the table is full ... a new client is allowed but not tracked (fail open)") | Once 10,000 keys are tracked, every key outside the table is "new" on every request. A client that is not in the table gets unlimited posts for the rest of the window, not just one. Filling the table takes 10,000 distinct keys. That is one IPv6 /50 at /64 masking, or trivial if `X-Client-IP` turns out to be spoofable (P3). After that the limiter is off for everyone. | When the table is full, put untracked clients into one shared overflow bucket that is rate-limited like any other key (or raise the cap well above what one attacker can fill). Cover it with `TryAcquire_TableFull_...`. |
| P3 | minor | Risks ("`X-Client-IP` missing ..."), Step 9 README, Security "Spoofing" | Two platform behaviours are claimed but not verified. (a) That the front end overwrites a caller-set `X-Client-IP`. The only source is a Microsoft Q&A answer, and the planned check (watch "unknown"-bucket warnings) would not detect spoofing. If the header can be spoofed, one header defeats AC1. (b) "Hitting the quota stops the API ... while the game keeps queuing scores." A stopped app answers 403. If the browser can read that 403, `ShouldRetrySend` drops the score. It is queued only if the response has no CORS headers and fetch fails as a network error. | Add a post-deployment check to Risks and the README. (a) Send 21 POSTs with an invalid body (400s, so no junk scores, because the limiter runs before the body is read), each with a different forged `X-Client-IP`. Expect 429 on the 21st, otherwise roll back. (b) Either verify the quota-stop response or soften the README wording. |
| P4 | minor | Step 8 (`^[0-9]+$` guard) | `0` passes the guard. For both `functionAppScaleLimit` and `dailyMemoryTimeQuota`, 0 means "no limit" (the plan's own Rollback says so). A typo would silently remove the spend ceiling AC4 asks for. | Require values of 1 or more (e.g. `^[1-9][0-9]*$`). |
| P5 | nit | plan.md | 15.7 KB against the ~10 KB guide for an M plan. Context found and Steps restate code details the implementer will read anyway. | Trim in the next revision. |

## Attacks the plan survived
- Mothership bound of 175 per wave: destroyed aliens stay in `_aliens` (no RemoveAll), and the edge checks at `GameEngine.cs:548-556` do not filter `Destroyed`. So the formation keeps its full width, a traverse is about 17 ticks, and there are about 18 drops from y=100 to 450. A hit mothership clears in 2 alien ticks (`:579-580`). The derivation holds, and 200 has headroom.
- Other score sources: `_points` changes only through `Award` and `PerfectWaveBonus` (`:221,345`). All awards are multiples of 10, so the multiple-of-10 rule never rejects a real game.
- Boss bound (HP `30+10(k-1)`, kill bonus `1000·k` at `:234,381`), Glass ×2 (`:190`), BossRush every 3. Worst-case doubling for daily covers every modifier, and no modifier changes the 5×11 grid (990 points).
- Huge `wave` (e.g. int.MaxValue): the early exit once the bound passes `MaxScore` prevents an O(wave) loop. A non-integer or string `wave` gives a `JsonException`, and the client gets a 400.
- 429 path: `SendScoreAsync` returning false leads to `QueueScoreAsync` (`Index.razor:246`), and `SendPendingScoresAsync` stops at the first false and keeps the rest of the queue (`:297`). A 400 still returns true and is not retried (AC3). If the 429 reached the browser without CORS headers, it would surface as `HttpRequestException` and still be queued.
- Burst size: 20 per hour is enough for the queue of at most 10 plus the current score.
- Test feasibility: `HighScoreValidator.cs` is compiled into the tests (csproj:29), so internal `MaxPlausibleScore`/`ClientKey` can be reached. Core has `InternalsVisibleTo`. `Aliens`, `Boss`, `PerfectWaveBonus` and `ComboTracker.MaxMultiplier` are public, and `Start(time, startWave)` reaches boss waves 5 and 10.
- Cost arithmetic: 10,000 GB-s × 31 days = 310,000, which is inside the 400,000 GB-s monthly free grant. The `az` commands for both settings apply to the Windows consumption plan. The new inputs go through `env:` (T12 pattern).
- Alternative considered: computing the daily modifier on the server from the date. It would tighten the daily bound, but the API shares no assembly with core (A11), so worst-case doubling is a reasonable choice.

## Checked and fine
- CG16 helper creation and moving the three functions onto it is required by the guideline, not scope creep.
- Classification M / security-sensitive yes / UI-facing no matches the change. One WP, and each file is listed once.
- AC1-AC6 each map to steps and tests. AC4 is a manual check, which is acceptable for an `az` step.
- The doc updates for T1/T2/T4 (to "partly", with what remains open) and A2/A5/A7/A11/A20/A23 are present, and Q1 stays open as the request says.
- The DI singleton through `HostBuilder.ConfigureServices` fits the isolated worker and is declared under A2.
- Old queued scores without `wave` being dropped once is declared under Risks and is acceptable.

Sources consulted for P3: [Microsoft Q&A: Documentation on x-client-ip http header](https://learn.microsoft.com/en-us/answers/questions/819540/documentation-on-x-client-ip-http-header), [Azure Front Door HTTP headers (XFF append behaviour)](https://learn.microsoft.com/uk-ua/azure/frontdoor/front-door-http-headers-protocol)

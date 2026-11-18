VERDICT: APPROVED

# Security review: CHORE-013 Rate limiting and score plausibility check, round 1

**Reviewed:** diff vs base_commit feee1ce + untracked files
**Reviewer:** Bunker (Security Reviewer)

## Threat model
- Crosses TB2 (AC1/AC2 anonymous POST to `savehighscore`: new `wave` field, client-IP headers) and TB8/T12 (two new `provision.yml` dispatch inputs that reach `az` through the shell).
- Data: client IP, which is personal data (AC2/D1). It is used only as an HMAC-SHA256 key with a 32-byte per-process RNG salt, held in memory and pruned by a timer within 2 h. It is never logged or returned.
- Threats: T1 (forgery), T2 (flooding) and T4 (denial of wallet) move to partly. New risks are header spoofing of `X-Client-IP`/XFF, memory or CPU exhaustion of the limiter table, and lockout through the overflow bucket.
- The security model is updated in the diff (TB2, TB8, T1, T2, T4, Assumptions, Revisions). AC1 to AC6 and R1 to R6 are unchanged and still respected.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| S1 | minor | src/api/ScoreRateLimiter.cs:152-161; docs/security-model.md T2 | [low] Lockout by filling the table. An attacker with many distinct keys sends 10,000 POSTs to fill the table, then 20 more to drain the overflow bucket. Distinct keys could come from a botnet, or from an IPv6 /48 (65,536 /64s) if the front end accepts IPv6. For up to 1 h, every client not already tracked then gets 429. The impact is limited: scores are queued (at most 10, retried on load), and a worse outcome, exhausting the daily quota, is already documented in T4. The plan accepted this trade-off, but T2 does not say it. | Add one clause to T2, for example: "filling the 10,000-client table puts new clients into one shared bucket, so a large address pool can delay everyone's posts for up to an hour". Optional: also count per /48 for IPv6. |
| S2 | minor | src/api/SaveHighScore.cs:19 | [low] The plan's Risks section relies on "watch for unknown-bucket rejections (the warning names the bucket type)", but the warning is a fixed string. Suppose the front end sets neither `X-Client-IP` nor XFF. Every player then shares the `unknown` bucket (20 posts/h for everyone). The README forged-header check still passes, because the 21st POST gets 429, so the owner has no signal of this failure. | Have `TryAcquire` report the bucket kind (client / unknown / overflow), and include it in the warning. Never log the address or the key (CG24). |
| S3 | nit | src/api/HighScoreValidator.cs:307-336 | [info] The per-wave bound is loose: the mothership term (60,000 per wave) allows 999,990 at a reported wave 16 (all-time) or about wave 8 (daily), and `wave` is client-reported. T1 already says this, and it matches AC6's assumption. | None. |

## Checked and fine
- Limiter: every POST is counted before the body is read. The limiter has one lock, and `ClientKey` (static `HMACSHA256.HashData`, read-only salt) runs outside it. The fixed-window boundary, `Retry-After` (ceiling of whole seconds) and `_lastPrune = MinValue` (no overflow when adding 1 min) are all correct. The inline prune runs at most once a minute. Memory is bounded at cap + 2 entries, about 2-3 MB.
- Expiry (AC2): the `TimeProvider.CreateTimer` prune runs every hour regardless of traffic, so a record lives at most 2 h. The timer is disposed with the DI singleton. The salt is never stored or logged. The raw IP appears in no log or response.
- Address parsing: the port is stripped, IPv4-mapped addresses are unmapped, IPv6 is masked to /64 (any scope ID is dropped by `GetAddressBytes`), and garbage goes to `unknown`. Multiple header values are joined and then fail to parse, so they cannot add keys. Trusting `X-Client-IP` is a declared risk, with a post-deployment forged-header check in the README.
- Plausibility: `Wave` up to `int.MaxValue` cannot cause CPU use or overflow, because the loop exits once the total passes `MaxScore` (about 16 iterations) and uses long arithmetic. The multiple-of-10 and wave rules run after the range check. `Wave` is not persisted (`HighScoreStore` writes only Name and Score), so there is no mass assignment.
- Client: 429/503/403 are queued. Retries happen only on a leaderboard load and stop at the first failure, so there is no retry loop. The queue keeps at most 10 scores. The logs hold no PII.
- CI (TB8/T12): `max_instances` and `daily_quota_gb_seconds` reach the shell only through job `env:`. Both are validated against `^[1-9][0-9]{0,8}$` before Azure login and quoted in `az`. The step summary prints only validated values. Permissions and the actions pins are unchanged. The `RESOURCE_GROUP` change edits rule text only.
- Engine members made `internal` are visible only to the test assembly (`InternalsVisibleTo`). There is no behaviour change.
- Dependencies: none added. The csproj only compiles `ScoreRateLimiter.cs` into the tests.
- Secrets and prompt injection: none found in the code, the tests (fixed test salts are not secrets), the docs or the item folder.

No project scanners were approved or found to run. No dependency changed, so `dotnet list package --vulnerable` was not needed.

VERDICT: APPROVED

# Code review: CHORE-013 Rate limiting and score plausibility check, round 1

**Reviewed:** diff vs base_commit feee1ce + untracked files
**Reviewer:** Saucer (Code Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | minor | src/api/SaveHighScore.cs:19 | The plan's Risks section says the warning names the bucket type, so the owner can spot `unknown`-bucket rejections. Those happen if `X-Client-IP` is missing on the front end, and then every client shares 20 posts an hour. The warning text is the same for every bucket, so the logs cannot show that failure. | Have `TryAcquire` report which bucket it counted against (client, `unknown` or `overflow`), for example through an out parameter or a small enum. Put that in the warning, never the address or the key. |
| C2 | nit | docs/security-model.md:41 | TB2's "What crosses it" cell still reads (`name`, `score`, `mode`, `day`). It leaves out the new `wave` field and the `X-Client-IP`/`X-Forwarded-For` headers that the boundary now trusts. | Add `wave` and the two headers to that cell. |
| C3 | nit | src/api/HighScoreValidator.cs:93,111 | `AlienWaveMax` is a computed property over constants, so it could be a `const`. In `MaxPlausibleScore`, `int boss = 0;` sits next to `var best`. | Make it `private const long AlienWaveMax`, and use `var boss = 0`. |

## Checked and fine
- Plausibility bound against `GameEngine.cs`. Every award goes through `Award`, which applies `_scoreFactor`, plus the perfect-wave bonus. Boss waves have no aliens and no mothership, boss HP is `30+10(k-1)` with one HP per hit, the kill bonus is `1000*(wave/BossEvery)`, and the combo multiplier is capped at 4. All awards are multiples of 10. The bound is a true upper bound, so a real game is never rejected, a dev start wave included. The early exit keeps the loop to about 16 iterations even for `wave = int.MaxValue`.
- `ScoreRateLimiter` concurrency and memory:
  - Hashing happens outside the lock, all table access inside it, and the timer's `Prune` takes the same lock.
  - The overflow key is added at most once, so the table never holds more than cap + 1 entries.
  - The inline prune runs at most once a minute, and a window that has ended resets its counter.
  - `Retry-After` is positive and rounded up.
- Address handling:
  - IPv4 with a port, bracketed IPv6 with a port, IPv4-mapped IPv6 and the /64 mask are all handled.
  - Several header values are joined with ",", so XFF still gives its rightmost entry. A duplicated `X-Client-IP` fails to parse and lands in the `unknown` bucket, so it cannot bypass the limit.
- DI: a singleton with a public constructor, disposed by the container. `TimeProvider` is already registered, so passing it to `SaveHighScore` is consistent with `HighScoreStore`.
- Client: `ShouldRetrySend` runs before the success check. A 400 for an old queued score without `wave` is dropped, as the plan's Risks section accepts. `Wave` round-trips through the `localStorage` queue under its `JsonPropertyName`.
- `provision.yml`: the inputs pass through `env:` and are validated before Azure login, and the variables are quoted. The app is created with `--os-type Windows`, so both settings apply to its consumption plan.
- Coding guidelines:
  - CG12/CG13: named constants with must-match comments, backed by a drift test.
  - CG14: the test-only `internal` members have doc comments.
  - CG15: the limiter class is sealed.
  - CG24: neither the IP nor the key is logged.
  - CG60: the new public members of `HighScore` have summaries.
- The architecture A2/A5/A7/A11/A20/A23 updates and the README section match the code.

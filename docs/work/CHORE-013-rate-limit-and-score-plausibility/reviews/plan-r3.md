VERDICT: APPROVED

# Plan review: CHORE-013 Rate limiting and score plausibility check, round 3

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| P5 | maintained (nit, non-blocking) | The Planner shortened the Summary and dropped the old step 1, but the plan is still about 19.5 KB, against a guide of about 10 KB for an M plan. This does not block. |
| P6 | resolved | Step 2 now runs the inline `Prune` only when the last prune is at least `InlinePruneInterval` (1 min) old. An attacker who keeps the table full therefore causes at most one scan a minute. Keeping the inline prune, instead of dropping it, means new players are not held in overflow for up to an hour, which is a sound reason. The new test `TryAcquire_TableFullOfEndedWindows_PrunesInline` covers it. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P7 | minor | Step 2, `InlinePruneInterval`; Tests, `TryAcquire_TableFullOfEndedWindows_PrunesInline` | The plan does not say what the "last prune" time starts at. If the implementer sets it to `time.GetUtcNow()` in the constructor, the test's explicit `t0+1 h` is compared with the real clock (the tests use `TimeProvider.System`). Depending on the date, `now - lastPrune` is then negative and the inline prune never runs, so the test fails or passes by accident. | Start the last-prune time at `DateTimeOffset.MinValue`, so the first full-table event always prunes, or compare only the times passed to `TryAcquire` and `Prune`. The implementer can settle this. |

## Attacks the plan survived
- Allow-list `^[1-9][0-9]{0,8}$`: it rejects `0`, `01`, `-1`, `1.5`, ` 1`, `1e4`, an empty value and 10 digits, and its upper bound 999,999,999 fits the int that both Azure settings take. A required input left empty fails, because the script treats a name without `?` as required (`validate-dispatch-inputs.sh:36-37`).
- Error names in the test harness: the script prints the lowercased variable name (`max_instances`, `daily_quota_gb_seconds`), which matches the workflow input names. The secret-leak check cannot trip on the RULE text, because "1000000000" does not appear in "1 to 999999999".
- The `RESOURCE_GROUP` text fix is correct: the last character class `[A-Za-z0-9_()-]` allows a trailing `-`, and a new `rg-` exit-0 case pins this. Adding the new names to `PATTERN` does not affect `set-custom-domain.yml`, which passes only its own names.
- `env -u` in `check` (`validate-dispatch-inputs.test.sh:17`): the plan adds both new names, so a runner environment cannot leak values into the tests.
- `provision.yml` wiring: the job-level `env:` (lines 37-43) and the single "Validate inputs" call (line 50) are where the plan says. "Configure Function App settings" exists at line 102, so the new step has a real anchor, and validation runs before Azure login.
- DI and the two constructors: Microsoft.Extensions.DependencyInjection considers only public constructors, so the internal test constructor `(TimeProvider, byte[], int)` cannot confuse `AddSingleton<ScoreRateLimiter>()`. `TimeProvider.System` is already registered (`Program.cs:10`).
- A 429 built inline with `Retry-After`: `FunctionResponses.TextAsync` (`FunctionResponses.cs:11`) takes no headers, CG16 is a `should` rule, and the plan declares the exception. `FunctionResponses.cs` stays out of WP1, so no file is shared.
- `SaveHighScore` on the new base: it uses `FunctionResponses` for every response, and the limiter check before `ReadBoundedAsync` (line 14) still makes every POST count, including oversize and invalid ones.
- The overflow test with a rate-limited inline prune: the first new key prunes (nothing has ended) and the next 19 skip the prune. All 20 go to the shared bucket and the 21st is rejected, so the test's expectations still hold.
- Old clients without `wave` get 400. `Index.razor:269` returns true (drop) on non-success when the status is not in `ShouldRetrySend`, so AC3's "not retried" holds.
- The test csproj already compiles four API files in (`BlazorInvaders.Tests.csproj:29-32`). Adding `ScoreRateLimiter.cs`, which uses only the BCL, needs no new reference.

## Checked and fine
- The Revisions section matches the changes for the rebase onto feee1ce. `GetHighScore.cs` is gone (CHORE-008), `FunctionResponses.cs` exists (CHORE-012), and the security model has TB8, T12 and the CHORE-015 revision line that steps 1 and 10 build on.
- The classification (M, security-sensitive yes, UI-facing no) and the single work package still fit. Each touched file appears once.

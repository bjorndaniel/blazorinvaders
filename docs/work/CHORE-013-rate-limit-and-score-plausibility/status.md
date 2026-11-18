---
id: CHORE-013
name: Rate limiting and score plausibility check
key: CHORE-013-rate-limit-and-score-plausibility
type: CHORE
phase: done  # intake | planning | plan-review | awaiting-approval | implementing | testing | review | done | needs-input | blocked | parked | cancelled | escalated
size: M  # S | M | L (PM estimate at intake; the plan's value once written)
security_sensitive: yes
ui_facing: no  # yes: the item gets a UX review
plan_round: 3
impl_round: 2
worktree: .work/CHORE-013-rate-limit-and-score-plausibility
branch: devagents/CHORE-013-rate-limit-and-score-plausibility
base_branch: devagents/CHORE-015-harden-workflows-further
base_commit: feee1ce2d21d863469f5a3e10e783d5a940816bd
blocked_by: none
---

# Status: CHORE-013 Rate limiting and score plausibility check

## Work packages
| WP | Depends on | State |
|----|------------|-------|
| WP1 | — | done |

## Decisions
- D1 (2026-11-17): rate-limit approach? → "Both": a per-client limit on a salted hash of the IP (kept at most 24 hours), plus a Function App scale-out cap and a daily usage quota.
- D2 (2026-11-17): approval gate → approved ("do all of them")
- D3 (2026-11-17): daily usage quota (10,000 GB-s, as planned, vs raise/drop) → no change requested: keep the plan's default 10,000 GB-s
- D4 (2026-11-18): approval gate for the plan updated to base feee1ce (plan r3) → Approve

## Accepted risks
none

## Log
- 2026-11-17 15:42 Nishikado (PM): created (intake)
- 2026-11-17 15:44 Nishikado (PM): intake confirmed by user ("OK as proposed")
- 2026-11-17 15:44 Mothership (planner) r1 balanced/opus: dispatched
- 2026-11-17 15:52 Mothership (planner) r1 balanced/opus: PLAN WRITTEN (M, security-sensitive, WP1)
- 2026-11-17 15:52 Crab (plan-reviewer) r1 balanced/opus: dispatched
- 2026-11-17 15:58 Crab (plan-reviewer) r1 balanced/opus: CHANGES_REQUESTED (P1 major: expiry tied to POSTs; P2-P4 minor; P5 nit)
- 2026-11-17 15:58 Mothership (planner) r2 balanced/opus: dispatched
- 2026-11-17 16:00 Mothership (planner) r2 balanced/opus: PLAN WRITTEN (P1-P5 accepted)
- 2026-11-17 16:00 Crab (plan-reviewer) r2 balanced/opus: dispatched
- 2026-11-17 16:02 Crab (plan-reviewer) r2 balanced/opus: APPROVED (P1-P4 resolved; P6 minor, P5 nit open)
- 2026-11-17 20:24 Nishikado (PM): approved (D1); blocked by CHORE-009 Enforce formatting (shares files); stack on its branch when done
- 2026-11-17 21:13 Nishikado (PM): stacked on devagents/CHORE-009 (886d225); waits for CHORE-008/CHORE-010 (shared docs and test files), then stacks on them
- 2026-11-17 21:45 Nishikado (PM): stacked on devagents/CHORE-012 (7c78ecd). Shares provision.yml and security-model.md with CHORE-015 (input validation script); waits for it, then stacks on it and re-plans against the new base (FunctionResponses now exists)
- 2026-11-18 07:42 Nishikado (PM): CHORE-015 done (feee1ce); stacked on devagents/CHORE-015 (includes CHORE-012); plan update for the new base
- 2026-11-18 07:42 Mothership (planner) r3 balanced/opus: dispatched
- 2026-11-18 07:46 Mothership (planner) r3 balanced/opus: PLAN WRITTEN (M, security-sensitive; rebased on feee1ce; C4/Q6 in step 1; P6, P5 accepted)
- 2026-11-18 07:46 Crab (plan-reviewer) r3 balanced/opus: dispatched
- 2026-11-18 07:47 Crab (plan-reviewer) r3 balanced/opus: APPROVED (P6 resolved; P5 nit maintained; P7 minor open)
- 2026-11-18 08:42 Nishikado (PM): approved (D4); no shared files with CHORE-014
- 2026-11-18 08:42 Cannon (implementer WP1) r1 fast/sonnet: dispatched
- 2026-11-18 08:48 Cannon (implementer WP1) r1 fast/sonnet: IMPLEMENTED (18 files; P7 fixed; small deviations: SaveHighScore takes TimeProvider, two test-shape choices; 254 tests, 57 script tests)
- 2026-11-18 08:48 Squid (tester) r1 fast/sonnet: dispatched
- 2026-11-18 08:50 Squid (tester) r1 fast/sonnet: PASS (6/6 ACs; 254 tests, 57 script tests; 9/9 breaks); test-report.md saved verbatim by PM
- 2026-11-18 08:50 Octopus (qa-reviewer) r1 balanced/opus, Saucer (code-reviewer) r1 balanced/opus, Bunker (security-reviewer) r1 deep/opus: dispatched
- 2026-11-18 08:52 Bunker (security-reviewer) r1 deep/opus: APPROVED (S1 minor: document table-fill lockout in T2; S2 minor: warning should name the bucket kind; S3 nit)
- 2026-11-18 08:52 Saucer (code-reviewer) r1 balanced/opus: APPROVED (C1 minor = S2: warning should name bucket kind; C2, C3 nit)
- 2026-11-18 08:55 Octopus (qa-reviewer) r1 balanced/opus: CHANGES_REQUESTED (Q1-Q6 major: missing tests for /64 mask, huge-wave exit, expiry timer, prune throttle, literal bound, wave JSON; Q7 major = C1/S2 bucket kind in warning; Q8 nit; 2/9 breaks caught)
- 2026-11-18 08:55 Nishikado (PM): round 2: Cannon for Q7/C1/S2, S1, C2, C3, Q8; then Squid for the test findings Q1-Q6
- 2026-11-18 08:55 Cannon (implementer WP1) r2 light/haiku: dispatched
- 2026-11-18 08:57 Cannon (implementer WP1) r2 light/haiku: IMPLEMENTED (Q7/C1/S2 RateBucket in warning; S1, C2, C3, Q8 fixed; 254 tests)
- 2026-11-18 08:57 Squid (tester) r2 fast/sonnet: dispatched (Q1-Q6)
- 2026-11-18 08:58 Squid (tester) r2 fast/sonnet: PASS (6/6 ACs; 261 tests, 9 added for Q1-Q6; 8/9 breaks, Q-B7 equivalent)
- 2026-11-18 08:58 Octopus (qa-reviewer) r2 balanced/opus, Saucer (code-reviewer) r2 balanced/opus: dispatched; Bunker later
- 2026-11-18 08:59 Saucer (code-reviewer) r2 balanced/opus: APPROVED (C1-C3 fixed; C4 nit)
- 2026-11-18 09:00 Octopus (qa-reviewer) r2 balanced/opus: APPROVED (Q1-Q8 resolved; 8/8 breaks caught)
- 2026-11-18 09:00 Bunker (security-reviewer) r2 deep/opus: dispatched (final check)
- 2026-11-18 09:01 Bunker (security-reviewer) r2 deep/opus: APPROVED (S1, S2 fixed; S3 nit info)
- 2026-11-18 09:01 Nishikado (PM): done; committed (C4 nit open)

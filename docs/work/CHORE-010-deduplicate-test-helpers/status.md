---
id: CHORE-010
name: Deduplicate test helpers
key: CHORE-010-deduplicate-test-helpers
type: CHORE
phase: done  # intake | planning | plan-review | awaiting-approval | implementing | testing | review | done | needs-input | blocked | parked | cancelled | escalated
size: S  # S | M | L (PM estimate at intake; the plan's value once written)
security_sensitive: no
ui_facing: no  # yes: the item gets a UX review
plan_round: 1
impl_round: 1
worktree: .work/CHORE-010-deduplicate-test-helpers
branch: devagents/CHORE-010-deduplicate-test-helpers
base_branch: devagents/CHORE-009-enforce-formatting
base_commit: 886d225c575b8776e298f849f9a196fd8bfd33a5
blocked_by: none  # ID of the item this one waits for
---

# Status: CHORE-010 Deduplicate test helpers

## Work packages
| WP | Depends on | State |
|----|------------|-------|
| WP1 | — | done |

## Decisions
- D1 (2026-11-17): approval gate → approved ("do all of them")
- D2 (2026-11-17, PM): QA Q1 (shared Run takes one more engine step at 500/1900/2000 ms than the old helpers) → deviation accepted: technical choice inside the plan; all 211 tests pass, boss timing still pinned (QA B5), code review traced it as sound. Q2 (refresh-rate theory cannot tell 60 Hz from others; pre-existing) → follow-up candidate, not this item.

## Accepted risks
none

## Log
- 2026-11-17 15:42 Nishikado (PM): created (intake)
- 2026-11-17 15:44 Nishikado (PM): intake confirmed by user ("OK as proposed")
- 2026-11-17 15:44 Mothership (planner) r1 fast/sonnet: dispatched
- 2026-11-17 15:45 Mothership (planner) r1 fast/sonnet: PLAN WRITTEN (S, WP1)
- 2026-11-17 15:45 Crab (plan-reviewer) r1 balanced/opus: dispatched
- 2026-11-17 15:48 Crab (plan-reviewer) r1 balanced/opus: APPROVED (P1-P3 minor, P4-P5 nit open)
- 2026-11-17 20:24 Nishikado (PM): approved (D1); blocked by CHORE-009 Enforce formatting (shares files); stack on its branch when done
- 2026-11-17 21:13 Nishikado (PM): CHORE-009 done; stacked on devagents/CHORE-009 (886d225). Runs alongside CHORE-008 (no shared files)
- 2026-11-17 21:13 Cannon (implementer WP1) r1 light/haiku: dispatched
- 2026-11-17 21:28 Cannon (implementer WP1) r1 light/haiku: IMPLEMENTED (P1-P5 addressed; DEVIATION: shared Run uses the double frame math, not the float StepMs loop of P1, which broke a GameEngineTests case; RunMs replaced by Run(engine, 0, n))
- 2026-11-17 21:28 Squid (tester) r1 fast/sonnet: dispatched
- 2026-11-17 21:29 Squid (tester) r1 fast/sonnet: PASS (3/3 ACs, 211 tests; breaks 2/3 caught, seed break survived)
- 2026-11-17 21:29 Nishikado (PM): test-report.md missing; saved Squid r1 final response verbatim
- 2026-11-17 21:29 Octopus (qa-reviewer) r1 balanced/opus, Saucer (code-reviewer) r1 balanced/opus, Bunker (security-reviewer) r1 balanced/opus: dispatched
- 2026-11-17 21:30 Bunker (security-reviewer) r1 balanced/opus: APPROVED (no findings)
- 2026-11-17 21:30 Saucer (code-reviewer) r1 balanced/opus: APPROVED (C1 nit)
- 2026-11-17 21:34 Octopus (qa-reviewer) r1 balanced/opus: APPROVED (Q1, Q2 minor, Q3 nit; 3 of 5 breaks caught, 2 survivors pre-existing/fixture)
- 2026-11-17 21:34 Nishikado (PM): D2 recorded; done; committed

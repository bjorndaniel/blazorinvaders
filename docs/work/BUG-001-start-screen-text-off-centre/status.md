---
id: BUG-001
name: Start screen text off-centre
key: BUG-001-start-screen-text-off-centre
type: BUG
phase: done  # intake | planning | plan-review | awaiting-approval | implementing | testing | review | done | needs-input | blocked | parked | cancelled | escalated
size: S  # S | M | L (PM estimate at intake; the plan's value once written)
security_sensitive: no
ui_facing: no  # yes: the item gets a UX review
plan_round: 2
impl_round: 2
worktree: .work/BUG-001-start-screen-text-off-centre
branch: devagents/BUG-001-start-screen-text-off-centre
base_branch: devagents/CHORE-010-deduplicate-test-helpers
base_commit: 6879e6d63c0b2727c9eeb6d17ccc71619cf6e6e4
blocked_by: none
---

# Status: BUG-001 Start screen text off-centre

## Work packages
| WP | Depends on | State |
|----|------------|-------|
| WP1 | — | done |

## Decisions
- D1 (2026-11-17): Crab plan-r1 QUESTION: screenshot from deployed master or local feature build? → "Deployed site (master)"
- D2 (2026-11-17): approval gate → Approve

## Accepted risks
none

## Log
- 2026-11-17 19:15 Nishikado (PM): created (intake)
- 2026-11-17 20:24 Mothership (planner) r1 fast/sonnet: dispatched
- 2026-11-17 20:26 Mothership (planner) r1 fast/sonnet: PLAN WRITTEN (S, UI-facing, WP1)
- 2026-11-17 20:26 Crab (plan-reviewer) r1 balanced/opus: dispatched
- 2026-11-17 20:30 Crab (plan-reviewer) r1 balanced/opus: CHANGES_REQUESTED (P1 blocker, P2 major, P3 minor, P4 nit); ESCALATION QUESTION: screenshot from master or feature build?
- 2026-11-17 20:30 Nishikado (PM): asked user (needs-input)
- 2026-11-17 21:13 Nishikado (PM): D1 recorded; stacked on devagents/CHORE-009 (886d225)
- 2026-11-17 21:13 Mothership (planner) r2 fast/sonnet: dispatched
- 2026-11-17 21:13 Mothership (planner) r2 fast/sonnet: PLAN WRITTEN (S, not UI-facing: tests only; P1-P4 accepted)
- 2026-11-17 21:13 Crab (plan-reviewer) r2 balanced/opus: dispatched
- 2026-11-17 21:14 Crab (plan-reviewer) r2 balanced/opus: APPROVED (P1-P4 resolved; P5 minor open)
- 2026-11-17 21:28 Nishikado (PM): approved (D2); blocked by CHORE-010 (shares SceneBuilderTests.cs); stacks on it when done
- 2026-11-17 21:34 Nishikado (PM): CHORE-010 done (6879e6d); stacked on devagents/CHORE-010; no shared files with CHORE-012
- 2026-11-17 21:34 Cannon (implementer WP1) r1 light/haiku: dispatched
- 2026-11-17 21:36 Cannon (implementer WP1) r1 light/haiku: IMPLEMENTED (P5 fixed; 241 tests)
- 2026-11-17 21:36 Squid (tester) r1 fast/sonnet: dispatched
- 2026-11-17 21:38 Squid (tester) r1 fast/sonnet: PASS (4/4 ACs, 241 tests; 5/5 breaks caught)
- 2026-11-17 21:38 Octopus (qa-reviewer) r1 balanced/opus, Saucer (code-reviewer) r1 balanced/opus, Bunker (security-reviewer) r1 balanced/opus: dispatched
- 2026-11-17 21:38 Bunker (security-reviewer) r1 balanced/opus: APPROVED (no findings)
- 2026-11-17 21:39 Saucer (code-reviewer) r1 balanced/opus: APPROVED (C1 minor: IdleUntil duplicates a step loop (CG54 should); C2, C3 nit)
- 2026-11-17 21:40 Octopus (qa-reviewer) r1 balanced/opus: CHANGES_REQUESTED (Q1, Q2 major: game-over daily hint and daily leaderboard title not asserted; 3 of 5 breaks caught)
- 2026-11-17 21:40 Nishikado (PM): round 2 (tests only) for Q1, Q2 and Saucer C1-C3, all in the WP1 test file
- 2026-11-17 21:40 Cannon (implementer WP1) r2 light/haiku: dispatched
- 2026-11-17 21:41 Cannon (implementer WP1) r2 light/haiku: IMPLEMENTED (Q1, C2, C3 fixed; Q2 DISPUTED: daily leaderboard title unreachable state; C1 left: TestPlay.cs outside WP1; 243 tests)
- 2026-11-17 21:41 Squid (tester) r2 fast/sonnet: dispatched
- 2026-11-17 21:42 Squid (tester) r2 fast/sonnet: PASS (4/4 ACs, 243 tests; 5/5 breaks caught)
- 2026-11-17 21:42 Octopus (qa-reviewer) r2 balanced/opus: dispatched (code skip: only tests changed; security later)
- 2026-11-17 21:44 Octopus (qa-reviewer) r2 balanced/opus: APPROVED (Q1 resolved; Q2 withdrawn: branch unreachable; note: SceneBuilder.cs:311-312 daily-board title is dead code, follow-up candidate)
- 2026-11-17 21:44 Saucer (code-reviewer) r2 balanced/opus: dispatched (rerun: run)
- 2026-11-17 21:45 Saucer (code-reviewer) r2 balanced/opus: APPROVED (C2, C3 resolved; C1 open by design; C4 nit)
- 2026-11-17 21:45 Bunker (security-reviewer) r2 balanced/opus: dispatched (final check)
- 2026-11-17 21:46 Bunker (security-reviewer) r2 balanced/opus: APPROVED (no findings)
- 2026-11-17 21:46 Nishikado (PM): done; committed (C1 minor, C4 nit open; follow-up candidates: share IdleUntil via TestPlay, dead daily-board branch in SceneBuilder)

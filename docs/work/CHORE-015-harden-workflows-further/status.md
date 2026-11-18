---
id: CHORE-015
name: Harden workflows further
key: CHORE-015-harden-workflows-further
type: CHORE
phase: done  # intake | planning | plan-review | awaiting-approval | implementing | testing | review | done | needs-input | blocked | parked | cancelled | escalated
size: M  # S | M | L (PM estimate at intake; the plan's value once written)
security_sensitive: yes
ui_facing: no  # yes: the item gets a UX review
plan_round: 1
impl_round: 2
worktree: .work/CHORE-015-harden-workflows-further
branch: devagents/CHORE-015-harden-workflows-further
base_branch: devagents/CHORE-012-cap-leaderboard-read
base_commit: 7c78ecd161125df39de4db7fd20930d7b3182f5c
blocked_by: none
---

# Status: CHORE-015 Harden workflows further

## Work packages
| WP | Depends on | State |
|----|------------|-------|
| WP1 | — | done |

## Decisions
- D1 (2026-11-17): switch Azure login to OIDC in this item? → "Leave out for now" (keep AZURE_CREDENTIALS; OIDC can be its own item later).
- D2 (2026-11-17): approval gate → approved ("do all of them")
- D3 (2026-11-17): Cannon r1 DEVIATION: zizmor ref-version-mismatch on Azure/static-web-apps-deploy (tag v1 = 1a947af, branch v1 = 4d27395) → "Keep tag pin, suppress": keep 1a947af and add a commented `zizmor: ignore[ref-version-mismatch]` on that line (plan amendment accepted)

## Accepted risks
none

## Log
- 2026-11-17 15:42 Nishikado (PM): created (intake)
- 2026-11-17 15:44 Nishikado (PM): intake confirmed by user ("OK as proposed")
- 2026-11-17 15:44 Mothership (planner) r1 balanced/opus: dispatched
- 2026-11-17 15:51 Mothership (planner) r1 balanced/opus: PLAN WRITTEN (M, security-sensitive, WP1)
- 2026-11-17 15:51 Crab (plan-reviewer) r1 balanced/opus: dispatched
- 2026-11-17 15:55 Crab (plan-reviewer) r1 balanced/opus: APPROVED (P1-P5 minor, P6 nit open)
- 2026-11-17 20:24 Nishikado (PM): approved (D1); blocked by CHORE-009 Enforce formatting (shares files); stack on its branch when done
- 2026-11-17 21:13 Nishikado (PM): stacked on devagents/CHORE-009 (886d225); waits for CHORE-008/CHORE-010 (shared docs and test files), then stacks on them
- 2026-11-17 21:45 Nishikado (PM): stacked on devagents/CHORE-012 (7c78ecd: includes CHORE-008, shared docs); no shared files with BUG-001
- 2026-11-17 21:45 Cannon (implementer WP1) r1 fast/sonnet: dispatched
- 2026-11-17 21:47 Cannon (implementer WP1) r1 fast/sonnet: IMPLEMENTED with ESCALATION DEVIATION (zizmor ref-version-mismatch on Azure/static-web-apps-deploy pin: tag v1 = 1a947af, branch v1 = 4d27395); P1-P4, P6 fixed
- 2026-11-17 21:47 Nishikado (PM): asked user (needs-input)
- 2026-11-17 21:49 Nishikado (PM): D3 recorded
- 2026-11-17 21:49 Cannon (implementer WP1) r1 light/haiku: re-dispatched with User decision (D3)
- 2026-11-17 21:50 Cannon (implementer WP1) r1 light/haiku: IMPLEMENTED (D3 applied: tag pin kept, ignore comment on the uses line; zizmor clean)
- 2026-11-17 21:50 Nishikado (PM): procedural DEVIATION (plan step 9 vs D3) settled: a small completed deviation with no scope, API, dependency or architecture change; D3 plus implementation/WP1.md are the record, no plan revision
- 2026-11-17 21:50 Squid (tester) r1 fast/sonnet: dispatched
- 2026-11-17 21:51 Squid (tester) r1 fast/sonnet: PASS (5/5 ACs; AC1/AC3 by config, zizmor run by implementer only; 30/30 script tests; 5/5 breaks)
- 2026-11-17 21:51 Octopus (qa-reviewer) r1 balanced/opus, Saucer (code-reviewer) r1 balanced/opus, Bunker (security-reviewer) r1 deep/opus: dispatched
- 2026-11-18 07:37 Octopus (qa-reviewer) r1 balanced/opus: CHANGES_REQUESTED (Q1-Q3 major: missing validation test cases; Q4 minor: zizmor negative check; Q5 nit; 7/10 breaks)
- 2026-11-18 07:37 Saucer (code-reviewer) r1 balanced/opus: APPROVED (C1 minor test env; C2, C3 nit)
- 2026-11-18 07:37 Bunker (security-reviewer) r1 deep/opus: APPROVED (S1 minor: T11 note on mutable SWA image; S2 nit)
- 2026-11-18 07:37 Nishikado (PM): round 2: Cannon for C2, C3, S1, S2; then Squid for the test-only findings Q1-Q5, C1; tracker size corrected to M
- 2026-11-18 07:37 Cannon (implementer WP1) r2 light/haiku: dispatched
- 2026-11-18 07:37 Cannon (implementer WP1) r2 light/haiku: IMPLEMENTED (C2, C3, S1, S2 fixed; 30/30 script tests)
- 2026-11-18 07:37 Squid (tester) r2 fast/sonnet: dispatched (Q1-Q5, C1)
- 2026-11-18 07:39 Squid (tester) r2 fast/sonnet: PASS (5/5 ACs; 34/34 script tests; Q1-Q5, C1 fixed; zizmor negative check exit 14; 4/4 breaks)
- 2026-11-18 07:39 Octopus (qa-reviewer) r2 balanced/opus, Saucer (code-reviewer) r2 balanced/opus: dispatched; Bunker later
- 2026-11-18 07:39 Saucer (code-reviewer) r2 balanced/opus: APPROVED (C1, C3 fixed; C4 minor: RESOURCE_GROUP error text wrongly forbids trailing -)
- 2026-11-18 07:41 Octopus (qa-reviewer) r2 balanced/opus: APPROVED (Q1-Q5 resolved; Q6 minor = C4 RESOURCE_GROUP text; 4/4 breaks)
- 2026-11-18 07:41 Nishikado (PM): C4/Q6 (one error string, non-blocking) carried into CHORE-013 re-plan, which adds inputs to the same validation script; no round 3
- 2026-11-18 07:41 Bunker (security-reviewer) r2 deep/opus: dispatched (final check)
- 2026-11-18 07:42 Bunker (security-reviewer) r2 deep/opus: APPROVED (S1, S2 fixed; no findings)
- 2026-11-18 07:42 Nishikado (PM): done; committed (C4/Q6 carried to CHORE-013)

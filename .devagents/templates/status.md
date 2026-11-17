---
id: <TYPE-NNN>
name: <Name>
key: <TYPE-NNN-slug>
type: FEAT | BUG | CHORE
phase: intake  # intake | planning | plan-review | awaiting-approval | implementing | testing | review | done | needs-input | blocked | parked | cancelled | escalated
size: M  # S | M | L (PM estimate at intake; the plan's value once written)
security_sensitive: no
ui_facing: no  # yes: the item gets a UX review
plan_round: 0
impl_round: 0
worktree: .work/<key>
branch: devagents/<key>
base_branch: <branch the worktree was created from>
base_commit: <full SHA the worktree was created from>
blocked_by: none  # ID of the item this one waits for
---

# Status: <ID> <Name>

## Work packages
<!-- Copied from the plan once approved: WP, depends on, state (pending | running | done). -->

## Decisions
<!-- The user's answers to escalations. Every agent reads these as requirements.
     "D1 (2026-10-13): plan finding P2 (deadlocked: one table or two?) → planner is right: keep one table"
     "D2 (2026-10-13): Cannon (WP2) DISPUTE on C3 (rename API) → drop the finding, keep the name" -->
none

## Accepted risks
<!-- Security findings the user explicitly accepted. "S2 (medium): ... — accepted by user on YYYY-MM-DD" -->
none

## Log
<!-- One line per dispatch, transition, escalation or failure, newest last, with the
     team member's name, role, round, tier and model. Tokens and cost are in usage.md. E.g.
     "- 2026-10-13 11:05 Mothership (planner) r2 balanced/opus: PLAN WRITTEN — P1 accepted, P2 disputed"
     "- 2026-10-13 11:12 Crab (plan-reviewer) r2 balanced/opus: CHANGES_REQUESTED — P1 resolved, P2 maintained"
     "- 2026-10-13 11:20 Mothership (planner) r3 balanced/opus: PLAN WRITTEN — P2 deadlocked"
     "- 2026-10-13 11:31 Nishikado (PM): tie-break P2 → planner is right (D1)"
     "- 2026-10-13 14:20 Cannon (implementer WP1) r1 balanced/opus: IMPLEMENTED"
     "- 2026-10-13 14:41 Squid (tester) r1 balanced/opus: ESCALATION BLOCKED — needs Docker for integration tests"
     "- 2026-10-13 14:43 Nishikado (PM): asked user (needs-input)" -->
- <YYYY-MM-DD HH:MM> Nishikado (PM): created (intake)

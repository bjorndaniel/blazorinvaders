---
id: CHORE-002
name: Security model
key: CHORE-002-security-model
type: CHORE
phase: done  # intake | planning | plan-review | awaiting-approval | implementing | testing | review | done | needs-input | blocked | parked | cancelled | escalated
size: S  # S | M | L (PM estimate at intake; the plan's value once written)
security_sensitive: no
ui_facing: no  # yes: the item gets a UX review
plan_round: 0
impl_round: 0
worktree: .work/CHORE-002-security-model
branch: devagents/CHORE-002-security-model
base_branch: feature/arcade-upgrades
base_commit: f2ce76802e2756d695bf0f6c0a747375e188887f
blocked_by: none  # ID of the item this one waits for
---

# Status: CHORE-002 Security model

## Work packages

## Decisions
- D1 (2026-11-17): questions asked (PM's stated assumptions in brackets): 1. Who uses the game [anonymous players, no accounts], and is anyone else an admin? 2. Is any data sensitive beyond initials and scores? 3. Anything you worry about, e.g. leaderboard cheating, spam, cost of the Functions app? 4. Compliance needs (e.g. GDPR) or past incidents? → user: "Yes to all" (taken as: the stated assumption holds; nothing else known — treat the rest as "don't know")
- D2 (2026-11-17): T1-T4 (forgery, spam, unbounded read, denial of wallet)? → "Fix in follow-ups" (stay open in the model; fixed by later items)
- D3 (2026-11-17): Q1, Q3, Q4 unknowns? → "Leave as open questions"
- D4 (2026-11-17, PM from other items): Q5 → the root `SaveHighScore.cs` is removed by CHORE-006 (commit 4828a1a, branch devagents/CHORE-006-remove-template-leftovers); Q6 / T13 → the API and provisioned runtime move to .NET 10 in CHORE-001 (commit f28870e); T12 → fixed by CHORE-007 Harden GitHub workflows (in progress, stacked on CHORE-001). The model describes the state after these merge.
- D5 (2026-11-17): approve the round-2 security model? → "approve both"

## Accepted risks
none

## Log
- 2026-11-17 08:40 Nishikado (PM): created (intake)
- 2026-11-17 11:19 Nishikado (PM): user answered questions (D1)
- 2026-11-17 11:19 Bunker (security-reviewer) r1 balanced/opus: dispatched (Draft the security model.)
- 2026-11-17 11:23 Bunker (security-reviewer) r1 balanced/opus: DRAFT WRITTEN (saved by PM to docs/security-model.md; Q1-Q6 open)
- 2026-11-17 11:32 Nishikado (PM): user answered open questions (D2, D3); D4 from other items
- 2026-11-17 11:32 Bunker (security-reviewer) r2 balanced/opus: dispatched (revise draft)
- 2026-11-17 11:34 Bunker (security-reviewer) r2 balanced/opus: REVISED (saved by PM to docs/security-model.md; Q7 added)
- 2026-11-17 14:22 Nishikado (PM): approved by user (D5); committed

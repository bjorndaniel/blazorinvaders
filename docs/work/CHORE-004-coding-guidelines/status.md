---
id: CHORE-004
name: Coding guidelines
key: CHORE-004-coding-guidelines
type: CHORE
phase: done  # intake | planning | plan-review | awaiting-approval | implementing | testing | review | done | needs-input | blocked | parked | cancelled | escalated
size: S  # S | M | L (PM estimate at intake; the plan's value once written)
security_sensitive: no
ui_facing: no  # yes: the item gets a UX review
plan_round: 0
impl_round: 0
worktree: .work/CHORE-004-coding-guidelines
branch: devagents/CHORE-004-coding-guidelines
base_branch: devagents/CHORE-003-architecture
base_commit: d2e377a0042d4f06cec32de0a0205a7a9831c451
blocked_by: none  # ID of the item this one waits for
---

# Status: CHORE-004 Coding guidelines

## Work packages

## Decisions
- D1 (2026-11-17): explicit access modifiers? → "Always write it" (CG6 should, new code)
- D2 (2026-11-17): API helpers? → "One shared helper" (CG16 should)
- D3 (2026-11-17): namespaces in new files? → "File-scoped" (CG17 should)
- D4 (2026-11-17): silent catch in Leaderboard.razor? → "Must log" (CG20 must, no offline exception)
- D5 (2026-11-17): rule levels → "CG1 naming → should, CG12 constants → must, CG30 `!` operator → must, CG60 → must in core" (every public member of src/core has an XML <summary>); all other levels as proposed
- D6 (2026-11-17): client logging? → "Keep Console"
- D7 (2026-11-17): follow-ups → "Remove dead API/package, Finish .editorconfig, Format check in CI, Dedupe test helpers" (tracked as separate items, not part of this document)
- D8 (2026-11-17): anything you insist on or dislike in reviews? → "Nothing more"
- D9 (2026-11-17): API runtime is .NET 10 (CHORE-001, user's request "this project should be updated to .net 10"); this base predates that merge, as docs/architecture.md A1 already records
- D10 (2026-11-17): approve docs/coding-guidelines.md? → "Yes to all"
- D11 (2026-11-17): CG60 backfill CHORE? → "Yes to all" (tracked as a separate follow-up)

## Accepted risks
none

## Log
- 2026-11-17 08:35 Nishikado (PM): created (intake), stacked on devagents/CHORE-003-architecture
- 2026-11-17 08:51 Saucer (code-reviewer) r1 balanced/opus: dispatched (Propose the coding guidelines.)
- 2026-11-17 08:55 Saucer (code-reviewer) r1 balanced/opus: PROPOSAL WRITTEN (saved by PM to coding-guidelines-proposal.md)
- 2026-11-17 10:21 Nishikado (PM): user answered proposal questions (D1-D8)
- 2026-11-17 10:21 Saucer (code-reviewer) r1 balanced/opus: dispatched (Write the coding guidelines.)
- 2026-11-17 10:24 Saucer (code-reviewer) r1 balanced/opus: WRITTEN (saved by PM to docs/coding-guidelines.md)
- 2026-11-17 11:19 Nishikado (PM): user approved docs/coding-guidelines.md (D10); committing

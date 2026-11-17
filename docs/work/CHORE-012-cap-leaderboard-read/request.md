# Request: CHORE-012 Cap the leaderboard read

**ID:** CHORE-012  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Fix threat T3 from the security model in a follow-up (CHORE-002 D2). Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
Every `getleaderboard` call reads and sorts the whole partition; cost and latency grow without limit.

## Acceptance criteria
- [ ] AC1: `getleaderboard` returns at most the top 10 entries for the requested board, in the same response format as today.
- [ ] AC2: The work and storage transactions per call do not grow with the number of stored entries.
- [ ] AC3: The table is created once (startup or first use), not on every GET.
- [ ] AC4: Existing leaderboard behaviour (all-time and daily boards, ordering) is unchanged; tests cover the cap.
- [ ] AC5: `docs/security-model.md` T3 is updated to mitigated.

## Project context
- `docs/security-model.md`: rules R1-R6, threats T1-T16; T1-T4 open, planned for follow-ups (D2 of CHORE-002); Q1 (compliance, IP logs) open.
- `docs/architecture.md`: pure core in `src/core`, thin Blazor/Functions shells, "C# describes, JS draws"; validation, config, logging and CI conventions (A1-A23).
- `docs/coding-guidelines.md`: rules CG1-CG73 (CG12 constants must, CG30 no `!` operator must, CG60 XML summaries must in core).

## Constraints and non-goals
No change to how scores are saved beyond what the cap needs. Rate limiting is CHORE-013.

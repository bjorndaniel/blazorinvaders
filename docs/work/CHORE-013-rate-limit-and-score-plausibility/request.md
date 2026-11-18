# Request: CHORE-013 Rate limiting and score plausibility check

**ID:** CHORE-013  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Fix threats T1, T2 and T4 from the security model in a follow-up (CHORE-002 D2). Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
Anyone can post any score, as often as they like, and a flood scales the consumption plan and storage cost.

## Acceptance criteria
- [ ] AC1: Score posts from one client beyond a set rate get HTTP 429. The game treats 429 like any send failure: the score is queued and retried, and play is never interrupted (UX3, UX25).
- [ ] AC2: The per-client limit is keyed on a salted hash of the client IP; no raw IP is stored, and the records expire within 24 hours (D1).
- [ ] AC3: The server rejects a score that is not plausible for the game the client reports (for example, more than the maximum possible for the waves reached), with a 400 that the client does not retry.
- [ ] AC4: Provisioning caps Function App scale-out and sets a daily usage quota, so spend has a ceiling (D1).
- [ ] AC5: Tests cover the limit, the plausibility rules and the client handling of 429.
- [ ] AC6: `docs/security-model.md` T1, T2 and T4 are updated, with what remains open. Assumed: the plausibility check raises the bar; without a server-run game a determined cheater can still forge scores.

## Project context
- `docs/security-model.md`: rules R1-R6, threats T1-T16; T1-T4 open, planned for follow-ups (D2 of CHORE-002); Q1 (compliance, IP logs) open.
- `docs/architecture.md`: pure core in `src/core`, thin Blazor/Functions shells, "C# describes, JS draws"; validation, config, logging and CI conventions (A1-A23).
- `docs/coding-guidelines.md`: rules CG1-CG73 (CG12 constants must, CG30 no `!` operator must, CG60 XML summaries must in core).
- `docs/ux-guidelines.md`: UX1-UX53; Known gaps section; UX29 mute control and UX14 larger phone text planned.

## Constraints and non-goals
No accounts or sign-in. No server-side game simulation. Q1 stays open; the salted hash with short retention is the user's chosen privacy approach.

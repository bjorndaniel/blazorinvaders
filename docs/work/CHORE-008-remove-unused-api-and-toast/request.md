# Request: CHORE-008 Remove unused GetHighScore function and Blazored.Toast

**ID:** CHORE-008  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Follow-up from CHORE-004 Coding guidelines ("Remove dead API/package"): remove the unused GetHighScore function and the unused Blazored.Toast package. Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
Dead code and an unused package widen the attack surface and confuse readers (UX20 says there are no toasts).

## Acceptance criteria
- [ ] AC1: The `GetHighScore` Azure Function and any code only it uses are removed from `src/api`.
- [ ] AC2: The Blazored.Toast package reference, its service registration, imports and any markup or CSS for it are removed.
- [ ] AC3: No reference to `GetHighScore` or `Blazored.Toast` remains in the code, CI or docs (docs updated where they mention them).
- [ ] AC4: The app, API and tests build, and all tests pass.

## Project context
- `docs/architecture.md`: pure core in `src/core`, thin Blazor/Functions shells, "C# describes, JS draws"; validation, config, logging and CI conventions (A1-A23).
- `docs/coding-guidelines.md`: rules CG1-CG73 (CG12 constants must, CG30 no `!` operator must, CG60 XML summaries must in core).
- `docs/ux-guidelines.md`: UX1-UX53; Known gaps section; UX29 mute control and UX14 larger phone text planned.
- `docs/security-model.md`: rules R1-R6, threats T1-T16; T1-T4 open, planned for follow-ups (D2 of CHORE-002); Q1 (compliance, IP logs) open.

## Constraints and non-goals
No behaviour change for players. Other endpoints unchanged.

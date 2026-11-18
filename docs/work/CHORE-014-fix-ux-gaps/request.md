# Request: CHORE-014 Fix the five UX gaps

**ID:** CHORE-014  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Fix the five gaps in the UX guidelines' Known gaps section in one fix-up item (CHORE-005 D2). Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
The UI departs from UX24, UX32, UX33, UX38 and UX43.

## Acceptance criteria
- [ ] AC1: UX24: when the leaderboard request fails (for example offline), the page says it could not load the board, not "No scores yet. Be the first!".
- [ ] AC2: UX32: the playfield and the initials inputs show a visible focus indicator (WCAG 2.4.7).
- [ ] AC3: UX33: each initials input has its own accessible name, for example "Initial 1 of 3".
- [ ] AC4: UX38: pinch zoom works on the leaderboard and the initials dialog, and stays blocked for the playfield and touch controls during play.
- [ ] AC5: UX43: on touch screens the pause overlay says "Tap" instead of "Press P".
- [ ] AC6: The Known gaps section of `docs/ux-guidelines.md` is emptied of the fixed gaps; tests pass.

## Project context
- `docs/ux-guidelines.md`: UX1-UX53; Known gaps section; UX29 mute control and UX14 larger phone text planned.
- `docs/architecture.md`: pure core in `src/core`, thin Blazor/Functions shells, "C# describes, JS draws"; validation, config, logging and CI conventions (A1-A23).
- `docs/coding-guidelines.md`: rules CG1-CG73 (CG12 constants must, CG30 no `!` operator must, CG60 XML summaries must in core).

## Constraints and non-goals
No other visual changes.

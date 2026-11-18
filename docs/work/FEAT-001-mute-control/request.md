# Request: FEAT-001 Mute control

**ID:** FEAT-001  **Type:** FEAT
**Requested by:** user, 2026-11-17

## What the user asked for
Add a mute control (CHORE-005 D3), as UX29 describes. Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
There is no way to turn the sound off.

## Acceptance criteria
- [ ] AC1: One key with no modifier toggles all sound, the march included, and takes effect at once. Assumed: the key is M.
- [ ] AC2: A touch button does the same, at least 44x44 CSS px, with an accessible name and `aria-pressed` showing the state. Assumed: it sits with the small touch buttons.
- [ ] AC3: The current mute state is shown on screen.
- [ ] AC4: The setting is remembered on the device (localStorage) and survives a reload; sound is on by default.
- [ ] AC5: UX29, UX31 and UX33 in `docs/ux-guidelines.md` describe the built control; tests cover the toggle and persistence.

## Project context
- `docs/ux-guidelines.md`: UX1-UX53; Known gaps section; UX29 mute control and UX14 larger phone text planned.
- `docs/architecture.md`: pure core in `src/core`, thin Blazor/Functions shells, "C# describes, JS draws"; validation, config, logging and CI conventions (A1-A23).
- `docs/coding-guidelines.md`: rules CG1-CG73 (CG12 constants must, CG30 no `!` operator must, CG60 XML summaries must in core).

## Constraints and non-goals
No volume slider or per-sound settings.

# Request: FEAT-002 Larger phone canvas text

**ID:** FEAT-002  **Type:** FEAT
**Requested by:** user, 2026-11-17

## What the user asked for
Larger canvas text on phones (CHORE-005 D3), as UX14 describes; 12 CSS px confirmed with the UX guidelines approval. Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
Canvas text scales down with the playfield and is hard to read on phones.

## Acceptance criteria
- [ ] AC1: On touch screens, text the player must read during play (score, prompts) shows at least 12 CSS px on a 375 px-wide phone, in portrait and in landscape.
- [ ] AC2: Text does not overlap or get cut off at those sizes.
- [ ] AC3: Desktop (non-touch) canvas text is unchanged.
- [ ] AC4: UX14 in `docs/ux-guidelines.md` describes the built behaviour; tests cover the size rule where it is computed.

## Project context
- `docs/ux-guidelines.md`: UX1-UX53; Known gaps section; UX29 mute control and UX14 larger phone text planned.
- `docs/architecture.md`: pure core in `src/core`, thin Blazor/Functions shells, "C# describes, JS draws"; validation, config, logging and CI conventions (A1-A23).
- `docs/coding-guidelines.md`: rules CG1-CG73 (CG12 constants must, CG30 no `!` operator must, CG60 XML summaries must in core).

## Constraints and non-goals
No layout change to the page or touch pads.

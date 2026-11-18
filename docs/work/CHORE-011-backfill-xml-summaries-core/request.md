# Request: CHORE-011 Backfill XML summaries in core

**ID:** CHORE-011  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Follow-up from CHORE-004 Coding guidelines (CG60 is a must in core). Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
Public types and members in `src/core` have no XML summaries, which CG60 requires.

## Acceptance criteria
- [ ] AC1: Every public type and public member in `src/core` has a `<summary>` that says what it is or does.
- [ ] AC2: The core project generates documentation and the build reports no missing-XML-comment warnings (CS1591) for core. Assumed: the warning check is turned on for core only.
- [ ] AC3: All tests pass; no behaviour change.

## Project context
- `docs/coding-guidelines.md`: rules CG1-CG73 (CG12 constants must, CG30 no `!` operator must, CG60 XML summaries must in core).
- `docs/architecture.md`: pure core in `src/core`, thin Blazor/Functions shells, "C# describes, JS draws"; validation, config, logging and CI conventions (A1-A23).

## Constraints and non-goals
Comments only in core. Other projects out of scope. Runs last in the UI track (touches core files the UI items change).

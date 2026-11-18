# Request: CHORE-010 Deduplicate test helpers

**ID:** CHORE-010  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Follow-up from CHORE-004 Coding guidelines ("Dedupe test helpers"; user chose "One shared helper"). Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
The same helper code is duplicated across test files.

## Acceptance criteria
- [ ] AC1: Helpers duplicated across test files live in one shared helper in the test project, per the coding guidelines.
- [ ] AC2: No test file keeps its own copy of a shared helper.
- [ ] AC3: The same tests exist as before (211) and all pass.

## Project context
- `docs/coding-guidelines.md`: rules CG1-CG73 (CG12 constants must, CG30 no `!` operator must, CG60 XML summaries must in core).

## Constraints and non-goals
Tests only; no production code change.

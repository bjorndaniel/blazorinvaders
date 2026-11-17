# Request: CHORE-009 Enforce formatting

**ID:** CHORE-009  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Follow-ups from CHORE-004 Coding guidelines ("Finish .editorconfig", "Format check in CI"). Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
Formatting rules exist in the coding guidelines but are not enforced; `dotnet format` reports 32 whitespace errors.

## Acceptance criteria
- [ ] AC1: The root `.editorconfig` has `root = true`, a charset, indentation settings and the Async-suffix naming rule from the coding guidelines.
- [ ] AC2: The existing whitespace errors are fixed: `dotnet format --verify-no-changes` passes for every project.
- [ ] AC3: CI (`deploy.yml` build job) runs `dotnet format --verify-no-changes` and fails the build on formatting drift.
- [ ] AC4: Architecture A6 and the Tools row in `docs/coding-guidelines.md` describe the enforced setup correctly.
- [ ] AC5: All tests pass. Assumed: formatting only, no behaviour change.

## Project context
- `docs/architecture.md`: pure core in `src/core`, thin Blazor/Functions shells, "C# describes, JS draws"; validation, config, logging and CI conventions (A1-A23).
- `docs/coding-guidelines.md`: rules CG1-CG73 (CG12 constants must, CG30 no `!` operator must, CG60 XML summaries must in core).

## Constraints and non-goals
No refactoring beyond what the formatter changes. Runs first: it touches files across the repo.

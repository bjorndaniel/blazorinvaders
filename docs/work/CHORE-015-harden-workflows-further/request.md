# Request: CHORE-015 Harden workflows further

**ID:** CHORE-015  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Track the Security Reviewer's follow-up suggestions from CHORE-007 as an item. Confirmed by the user at intake with the acceptance criteria below ("OK as proposed").

## Problem / motivation
Workflow security is checked by hand; actions are pinned to tags; inputs are not validated.

## Acceptance criteria
- [ ] AC1: CI runs a workflow linter (`actionlint` or `zizmor`) on `.github/workflows/` and fails on findings.
- [ ] AC2: Every third-party action is pinned to a full commit SHA, with the version in a comment.
- [ ] AC3: Dependabot updates GitHub Actions (`.github/dependabot.yml`).
- [ ] AC4: `workflow_dispatch` inputs are validated for format before use; for example, a custom domain starting with `-` is rejected.
- [ ] AC5: Existing workflow behaviour is otherwise unchanged.

## Project context
- `docs/security-model.md`: rules R1-R6, threats T1-T16; T1-T4 open, planned for follow-ups (D2 of CHORE-002); Q1 (compliance, IP logs) open.
- `docs/architecture.md`: pure core in `src/core`, thin Blazor/Functions shells, "C# describes, JS draws"; validation, config, logging and CI conventions (A1-A23).

## Constraints and non-goals
OIDC login stays out of scope (D1); AZURE_CREDENTIALS is kept.

# Request: CHORE-007 Harden GitHub workflows

**ID:** CHORE-007  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Follow-up from the CHORE-001 security review (finding S1, pre-existing, info): the user asked to track it ("Yes, track it").

S1 as raised: `provision.yml` interpolates `${{ inputs.* }}` directly into `run:` scripts (self-injection by users with write access, with Azure credentials loaded); neither workflow declares `permissions:`, so `GITHUB_TOKEN` gets the repository default scope.

## Problem / motivation
Workflow inputs are interpolated into shell scripts and the workflows run with the default token scope.

## Acceptance criteria
- [ ] AC1: No `${{ inputs.* }}` (or other user-controlled `${{ }}` expressions) is interpolated directly into a `run:` script in any workflow under `.github/workflows/`; inputs are passed through `env:` and used quoted (`"$VAR"`).
- [ ] AC2: Every workflow declares a top-level `permissions:` block with least privilege (`contents: read` by default), adding only the scopes a job actually needs.
- [ ] AC3: The workflows keep their current behaviour: same triggers, inputs, steps and deploy targets; the YAML is valid.

## Project context
- `README.md`: Blazor WebAssembly (.NET 10) Space Invaders game; Azure Functions (isolated) API with Azure Table Storage leaderboards; deployed via GitHub Actions to Azure Static Web Apps and a Function App.
- CHORE-001 security review (`docs/work/CHORE-001-upgrade-api-and-ci-to-net-10/reviews/security-r1.md`, S1): the source of this item.

## Constraints and non-goals
- Assumed: no change to secrets, Azure resources or action versions (beyond what least privilege requires).
- Stacked on devagents/CHORE-001-upgrade-api-and-ci-to-net-10, which changes the same workflow files.

# Request: CHORE-002 Security model

**ID:** CHORE-002  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Offered at session start (preflight: missing); the user chose to set it up. Draft docs/security-model.md (access model, trust boundaries, threat model) from the code and the user's answers.

## Problem / motivation
Plans and reviews have no shared project document for this area yet.

## Acceptance criteria
- [ ] AC1: docs/security-model.md exists from .devagents/templates/security-model.md with IDs for every access-model entry, trust boundary and threat
- [ ] AC2: The user has approved its actors, boundaries and open threats

## Project context
- `README.md`: Blazor WebAssembly (.NET 10) Space Invaders game on an HTML5 canvas; Azure Functions (isolated) API with Azure Table Storage for a global top-10 leaderboard and a daily-challenge leaderboard; installable PWA with offline play; deployed via GitHub Actions to Azure Static Web Apps.

## Constraints and non-goals
- Document only; no source code changes. Run as in WORKFLOW.md (no plan, tests or code review).

# Request: CHORE-004 Coding guidelines

**ID:** CHORE-004  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
Offered at session start (preflight: missing); the user chose to set it up. Write docs/coding-guidelines.md: how the code is written beyond what its tools enforce.

## Problem / motivation
Plans and code reviews have no shared coding guidelines yet.

## Acceptance criteria
- [ ] AC1: A coding-guidelines proposal per template area is shown to the user and decided, including each rule's level (must / should).
- [ ] AC2: docs/coding-guidelines.md is written from .devagents/templates/coding-guidelines.md and the user's answers, every rule with an ID, source and level, and approved by the user.

## Project context
- `README.md`: Blazor WebAssembly (.NET 10) Space Invaders game; Azure Functions (isolated) API with Azure Table Storage leaderboards; deployed via GitHub Actions to Azure Static Web Apps and a Function App.
- `docs/architecture.md` (CHORE-003, on this item's base branch devagents/CHORE-003-architecture): stack and patterns (A1-A23); coding guidelines build on it.

## Constraints and non-goals
- Document only; no source code changes. Run as in WORKFLOW.md → Coding guidelines (no plan, tests or code review).
- Stacked on devagents/CHORE-003-architecture so docs/architecture.md is present.

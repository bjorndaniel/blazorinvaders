# Request: CHORE-001 Upgrade API and CI to .NET 10

**ID:** CHORE-001  **Type:** CHORE
**Requested by:** user, 2026-11-17

## What the user asked for
"this project should be updated to .net 10"

Intake: the Blazor app (`src/BlazorInvaders.csproj`), core library and tests already target `net10.0`, and `global.json` pins SDK 10.0.200. Remaining .NET 9 references: the Azure Functions API project (`net9.0`), the deploy workflow's SDK setup (9.0.x + 10.0.x) and the provision workflow's Function App runtime (`--runtime-version 9`). User confirmed the scope below.

## Problem / motivation
The solution mixes .NET 9 and .NET 10; the whole project should be on .NET 10.

## Acceptance criteria
- [ ] AC1: `src/api/BlazorInvaders.Api.csproj` targets `net10.0`; Azure Functions Worker packages are bumped only where .NET 10 requires it.
- [ ] AC2: `.github/workflows/deploy.yml` sets up only the .NET 10 SDK (10.0.x), no 9.0.x.
- [ ] AC3: `.github/workflows/provision.yml` provisions the Function App with `--runtime-version 10`.
- [ ] AC4: The whole solution (app, API, core, tests) builds and all tests pass.
- [ ] AC5: No remaining references to .NET 9 / `net9.0` in project files or workflows.

## Project context
- `README.md`: already describes the app as "Blazor WebAssembly (.NET 10)" with an Azure Functions API and Azure Table Storage leaderboard.

## Constraints and non-goals
- Assumed: the live Azure Function App is not changed by this item; the final report gives the user the `az` command to switch its runtime to .NET 10.
- Assumed: other package upgrades (test SDK, xunit, Blazored, etc.) are out of scope.

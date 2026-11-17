# Architecture

Last updated: 2026-11-17 by CHORE-003 Architecture

## The project
BlazorInvaders is a Space Invaders arcade game for anyone with a browser, drawn on an HTML5 canvas by a Blazor WebAssembly client that installs as a PWA and plays offline. A small Azure Functions API keeps a global top-10 leaderboard and a daily-challenge leaderboard in Azure Table Storage. It is a small hobby project and is expected to stay one; it runs on Azure Static Web Apps (client) and a consumption-plan Function App (API).

## Stack
| ID | Area | Choice | Version | Why |
|----|------|--------|---------|-----|
| A1 | Language and runtime | C# on .NET 10 for client, core, API and tests (`net10.0`; Functions runtime version 10); SDK pinned in `global.json` with `rollForward: latestFeature` | SDK 10.0.200 | One stack everywhere; CI needs a single SDK |
| A2 | Web or API framework | Azure Functions v4, isolated worker, `HttpRequestData`/`HttpResponseData`, one class per endpoint, anonymous HTTP triggers | `Microsoft.Azure.Functions.Worker` 2.51.0, HTTP extension 3.3.0 | Serverless and pay-per-use; fits a small leaderboard API |
| A3 | UI framework | Blazor WebAssembly, standalone, PWA with a service worker for offline play; Bootstrap CSS; Blazored.Modal. The game is drawn on a canvas by ES modules (`src/wwwroot/js/game.js`, `effects.js`) that execute the C# `Scene` draw commands; JS owns the frame loop, input, audio and `localStorage` | Blazor WASM 10.0.4, Blazored.Modal 7.3.1 | Layout and rules stay in C#; JS does only what needs the browser |
| A4 | Data storage and access | Azure Table Storage via `Azure.Data.Tables`, table `HighScores`, one partition per board (`HighScore`, `Daily-yyyyMMdd`), created on first use; no ORM, no migrations; Azurite locally. Client side: `localStorage` for personal bests and pending scores | `Azure.Data.Tables` 12.9.1 | Cheapest store for a top-10 list; schemaless |
| A5 | Testing | xUnit unit tests in `tests/BlazorInvaders.Tests` against `src/core` and `HighScoreValidator`; deterministic via seeded `Random` and injected timestamps; `InternalsVisibleTo` for state setup; coverlet collector. No integration, component or end-to-end tests | xUnit 2.9.3 | The rules and the validation carry the risk; Razor, JS and storage code is kept thin |
| A6 | Linting and formatting | `src/.editorconfig` (underscore-prefixed private fields, expression-bodied members, `Async` suffix; severity suggestion); nullable reference types and implicit usings enabled; no analyzers, no JS linter, no format check in CI | - | What the repository has |
| A7 | Build, CI and deployment | GitHub Actions `.github/workflows/deploy.yml`: build client and API and run the tests on push and PR to `master`; on push, publish the client to Azure Static Web Apps (Free) and the API to a separate Function App (consumption plan). Infrastructure by `provision.yml` and `set-custom-domain.yml` (manual runs, `az` CLI) | - | Simple, free tier, no infrastructure-as-code tooling to maintain |

## Patterns
| ID | Pattern | Where it applies | Why |
|----|---------|------------------|-----|
| A10 | Modular monolith with a pure core: game rules (`GameEngine`) and the scene description (`SceneBuilder` -> `Scene`) have no Blazor or browser dependency; the UI and the API are thin shells around it | `src/core` | Game logic stays unit-testable and the renderer replaceable |
| A11 | Modules: `src/core` (rules, game objects, scene), `src` (Blazor WASM client: pages, components, JS interop), `src/api` (Azure Functions), `tests/BlazorInvaders.Tests` (the one test project). Dependencies: client -> core; tests -> core and `src/api/HighScoreValidator.cs` (compiled in); the API shares no assembly with the client | whole repository | Each part builds and deploys on its own |
| A12 | C# describes, JS draws: the client builds a `Scene` of draw commands in C# and hands it to the canvas module; constants that JS duplicates from C# are marked "Must match" in the JS modules | `src` client, `src/wwwroot/js` | One source of truth for layout; per-frame interop stays a single call |

## Conventions
| ID | Convention |
|----|------------|
| A20 | Validation: all untrusted input to the API is checked server side in the static, dependency-free `HighScoreValidator` (bounded body read, `System.Text.Json`, generated regexes, score and day ranges) before it reaches storage. New rules go there, with unit tests |
| A21 | Authentication: none. Players are anonymous, identified only by 1-3 initials; Functions use `AuthorizationLevel.Anonymous`; access is limited by CORS to the site's origins (set in `provision.yml` and `set-custom-domain.yml`). No secret may be relied on in the WASM client |
| A22 | Configuration and secrets: client reads `FunctionsApi` from `src/wwwroot/appsettings.json`, whose placeholder CI replaces from the `FUNCTIONS_API_URL` secret (`appsettings.Development.json`, git-ignored, locally). API reads `AzureWebJobsStorage` from the environment (Function App setting; `local.settings.json`, git-ignored, locally). Deployment credentials live only in GitHub secrets; no secret reaches the client bundle |
| A23 | Logging and errors: the API logs exceptions with an injected `ILogger<T>` and answers with plain-text status messages without exception details (400, 413, 500). The client degrades gracefully when the API fails: the game stays playable offline and scores are queued in `localStorage` |

## Decisions
- 2026-11-17 A1: API runtime moved from .NET 9 to .NET 10 (`net10.0`, Functions runtime 10), so the whole stack is on one runtime; alternative considered: keep the API on .NET 9. Decided by the user, CHORE-001 ("this project should be updated to .net 10"), recorded in CHORE-003 D1.
- 2026-11-17 A2-A7, A10-A12, A20-A23: recorded as the code already uses them; alternatives considered per area in `docs/work/CHORE-003-architecture/architecture-proposal.md` (hexagonal ports, shared contracts project, ASP.NET Core integration for Functions, SWA managed functions, Cosmos DB, FluentValidation, SWA built-in auth, Key Vault or managed identity, Application Insights, bUnit/Playwright/Testcontainers, `dotnet format` in CI, Bicep or Terraform). Accepted by the user, CHORE-003 D2.

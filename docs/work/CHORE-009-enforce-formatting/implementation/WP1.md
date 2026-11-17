# Implementation: CHORE-009 Enforce formatting — WP1

## Round 1
**Plan steps covered:** WP1 steps 1-7
**Addresses findings:** P1 (gate reworded: diff shows only BOM lines and whitespace-only lines; 10 `src/core` files changed), P2 (kept the literal `dotnet format --verify-no-changes`; A6 now says it also fails on warning-level style or analyzer diagnostics, none today)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `.editorconfig` | modified: root, charset, indents, `.sln` section, `async_methods_end_in_async` rule |
| `src/core/GameEngine.cs`, `src/core/Rendering/SceneBuilder.cs` | modified: `dotnet format` output |
| `src/core/GameObjects/{Alien,Bomb,Enumerations,GameTime,MotherShip,Player,Shot,Sprite}.cs` | modified: `dotnet format` output (whitespace, BOM) |
| `src/Shared/MainLayout.razor`, `src/_Imports.razor`, `src/wwwroot/css/app.css` | modified: BOM removed |
| `.github/workflows/deploy.yml` | modified: `Check formatting` step before Build |
| `docs/architecture.md`, `docs/coding-guidelines.md`, `README.md` | modified: A6, A7, Decisions, Tools row, CG1, CG72, CG73, Open question, Revisions, README |

### Deviations from plan
None.

### Commands run
- `dotnet format src/BlazorInvaders.sln --verify-no-changes` -> exit 0
- both `dotnet build` -> 0 errors; `dotnet test tests/BlazorInvaders.Tests` -> 211 passed

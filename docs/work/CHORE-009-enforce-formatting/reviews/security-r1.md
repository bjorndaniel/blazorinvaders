VERDICT: APPROVED

# Security review: CHORE-009 Enforce formatting, round 1

## Threat model
- Boundary crossed: TB7 (GitHub repository to Azure, CI/CD). The change adds one step (`dotnet format src/BlazorInvaders.sln --verify-no-changes`) to the `build_and_test` job in `.github/workflows/deploy.yml`.
- Threat that applies: T11 (CI/CD and supply-chain compromise). The job has no secrets, runs with workflow-level `permissions: contents: read`, and uses no new action, package or `pull_request_target`. `dotnet format` comes with the SDK that `actions/setup-dotnet@v4` already installs.
- On `pull_request` runs, untrusted PR code can run during the MSBuild evaluation and restore that `dotnet format` does. The existing Build and Test steps in the same job already run it, so this adds no exposure.
- Actors AC1-AC4, boundaries, threats and the access model are unchanged, so `docs/security-model.md` needs no update.
- No runtime inputs, authentication, data or secrets are involved. The code changes are formatting only.

## Findings
None.

## Checked and fine
- **Workflow (`.github/workflows/deploy.yml:27-28`):** the new step is a fixed command with no `${{ }}` expressions, so nothing from the event can be injected into it. It runs before Build in the secret-free job. `build_and_deploy` and its secrets (`AZURE_CREDENTIALS`, `SWA_DEPLOYMENT_TOKEN`, `FUNCTIONS_API_URL`) are unchanged and still skipped for `pull_request`. Top-level `permissions: contents: read` is unchanged.
- **Code diff (`src/**`):** a whitespace-insensitive diff (`git diff -w --ignore-blank-lines`) shows only removed UTF-8 BOMs. That covers 8 files in `src/core/GameObjects`, `src/Shared/MainLayout.razor`, `src/_Imports.razor` and `src/wwwroot/css/app.css`. `GameEngine.cs`, `Bomb.cs` and `SceneBuilder.cs` changed only in whitespace. No logic, validation or rendering changed.
- **`.editorconfig`:** contains only editor, formatter and naming settings, at severity `suggestion`. It does not switch off or weaken any analyzer or compiler warning.
- **Dependencies:** no change to any manifest or lockfile.
- **Docs (`README.md`, `docs/architecture.md`, `docs/coding-guidelines.md`, `docs/work/TRACKER.md`):** descriptive changes only, with no secrets, URLs or credentials. No text tries to instruct an AI agent.
- **Item folder (`docs/work/CHORE-009-enforce-formatting/`):** I searched it for secrets, tokens and connection strings. The only matches are prose about the workflow's secrets, with no values.

## Notes for the PM
- The brief contained only template fields and the task. Nothing in it or in the files tried to steer the verdict.
- No project scanners were approved or needed; the change adds no dependency to audit.

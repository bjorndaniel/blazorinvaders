# Plan: CHORE-001 Upgrade API and CI to .NET 10

| | |
|---|---|
| **ID** | CHORE-001 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-17 |
| **Size** | S — 4 small edits, no new code |
| **Security-sensitive** | yes — touches CI/CD and infrastructure (provision workflow) |
| **UI-facing** | no — project files and workflows only |
| **Opened** | 2026-11-17 |
| **Plan round** | 1 |

## Summary
Move the last .NET 9 pieces to .NET 10: the API's `TargetFramework`, the two SDK setups in `deploy.yml`, and the Function App runtime in `provision.yml`. The API's Worker packages (Worker 2.51.0, Http 3.3.0, Sdk 2.0.7) are already 2.x and support net10.0, so they stay unchanged unless the build proves otherwise.
- Assumption: README lines still saying ".NET 9" (prerequisites, backend table) are fixed too, so no stale .NET 9 text remains.
- The live Function App is not changed; the final report gives the `az functionapp config set --net-framework-version v10.0 ...` style command (Implementer verifies the exact flag against current `az` docs before reporting).
- WP1 only. Security: CI/infra change limited to version strings; no secrets or inputs touched.

## Context found
- `src/api/BlazorInvaders.Api.csproj:3` is the only `net9.0`; `deploy.yml:21-23` and `:43-45` list `9.0.x` + `10.0.x`; `provision.yml:83` has `--runtime-version 9`.
- `global.json` already pins SDK 10.0.200; app, core and tests are already `net10.0`.
- `README.md:41` (".NET 9 runtime" prerequisite) and `:100` ("isolated worker, .NET 9").
- No `docs/` architecture, coding-guidelines or security-model files apply. `.editorconfig` governs formatting (keep indentation as is).

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Retarget API, CI and runtime to .NET 10 | S | `src/api/BlazorInvaders.Api.csproj`, `.github/workflows/deploy.yml`, `.github/workflows/provision.yml`, `README.md` | — |

## Steps
### WP1
1. **Retarget API** — `src/api/BlazorInvaders.Api.csproj` — `net9.0` to `net10.0`. Then `dotnet build src/api/BlazorInvaders.Api.csproj`; bump a Functions package only if the build or restore fails or warns that it does not support net10.0 (AC1).
2. **deploy.yml** — both `Setup .NET` steps: remove `9.0.x`; keep `10.0.x` (single-line `dotnet-version: 10.0.x` or the one-entry block, either fine) (AC2).
3. **provision.yml** — line 83 `--runtime-version 9` to `10` (AC3).
4. **README.md** — line 41: drop ".NET 9 runtime" (the .NET 10 SDK covers it); line 100: ".NET 9" to ".NET 10".
5. **Verify** — `dotnet build` the solution and `dotnet test tests/BlazorInvaders.Tests`; `grep -rniE "net9|9\.0\.x|\.NET 9|runtime-version 9"` outside `docs/work`, `.devagents`, `bin`, `obj` returns nothing (AC4, AC5).

## Tests
No new tests: this is a retarget with no behaviour change.
| AC | Test | Type | WP |
|----|------|------|----|
| AC1 | API project builds on net10.0 with no package-support warnings | build | WP1 |
| AC2, AC3, AC5 | grep check in step 5 | check | WP1 |
| AC4 | existing `tests/BlazorInvaders.Tests` pass; full solution build | unit | WP1 |

## Security considerations
Touches CI/CD and infrastructure only as version strings. No new dependency (existing packages kept), no secrets, inputs, auth or data exposure change. No `docs/security-model.md`; nothing to update. Moving to a current supported runtime is a net security gain.

## UX considerations
None: not UI-facing.

## Risks and rollback
- Deploying the new API build before the live Function App runs .NET 10 would fail at startup: the user must run the `az` runtime switch (in the final report) before or with the first deploy. Existing apps keep `--runtime-version 9` until then; provision only affects new creation.
- Functions Worker/Sdk may need a bump for net10.0; handled in step 1.
- Rollback: revert the commit and set the Function App runtime back to 9.

## Out of scope
Other package upgrades; changing the live Azure resources.

## Revisions

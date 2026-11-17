VERDICT: APPROVED

# Security review: CHORE-001 Upgrade API and CI to .NET 10, round 1

**Reviewed:** diff vs base_commit c727069 + untracked files (only `docs/work/` item records)
**Reviewer:** Vera (Security Reviewer)

## Threat model
- There is no `docs/security-model.md`, so I worked out the boundaries myself. The diff touches two: CI/CD (GitHub Actions, which uses `AZURE_CREDENTIALS`, `SWA_DEPLOYMENT_TOKEN` and `FUNCTIONS_API_URL`) and the Azure Function App runtime (HTTP API, Table Storage connection string).
- The change is limited to version strings: TFM `net9.0` to `net10.0`, SDK setup `9.0.x` removed, Function App `--runtime-version 10`, and README text. No code paths, inputs, auth, secrets handling, permissions or packages change.
- The relevant risks are an unsupported runtime (.NET 9 STS support ended 2026-11-10, so this upgrade lowers risk), dependency compatibility, and any change to how workflows handle secrets or what they are allowed to do. None of these is made worse.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| S1 | nit | .github/workflows/provision.yml:44-95, .github/workflows/deploy.yml:1-12 | [info] Already in the code before this change, not part of the diff. `provision.yml` puts `${{ inputs.* }}` straight into `run:` scripts. Only users with write access can trigger `workflow_dispatch`, so this is a self-injection risk with Azure credentials loaded. Neither workflow declares `permissions:`, so `GITHUB_TOKEN` gets the repository's default scope. | Optional follow-up item: pass inputs through `env:` and quote them as `"$VAR"`; add `permissions: contents: read` at the top level (and any deploy-specific scopes the SWA action needs). |

## Checked and fine
- `src/api/BlazorInvaders.Api.csproj`: only the TFM changed. Package versions are unchanged (Worker 2.51.0, Http 3.3.0, Sdk 2.0.7, Azure.Data.Tables 12.9.1). No new dependency, and no manifest or lockfile changes.
- `dotnet list src/api/BlazorInvaders.Api.csproj package --vulnerable --include-transitive` reports no vulnerable packages, direct or transitive (source: nuget.org).
- `deploy.yml`: only `9.0.x` was removed from both `setup-dotnet` steps. Secret use, the `pull_request` gate on `build_and_deploy`, triggers and action versions are unchanged. No secret is exposed to PR builds.
- `provision.yml:83`: only the runtime version changed. Connection-string masking (`::add-mask::`) and the TLS 1.2 minimum are untouched.
- `global.json` (10.0.200, `latestFeature`) is satisfied by `10.0.x`. Moving off the .NET 9 runtime, now out of support, is a net security gain.
- README changes are text only. No secrets or instructions aimed at agents.
- The item folder `docs/work/CHORE-001-...` has no credentials, connection strings, tokens or injected instructions.

## Notes for the PM
- S1 is outside this item's scope. Raise it with the user as a possible separate CHORE if they want to harden the workflows.

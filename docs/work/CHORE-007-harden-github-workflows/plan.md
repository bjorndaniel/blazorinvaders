# Plan: CHORE-007 Harden GitHub workflows

| | |
|---|---|
| **ID** | CHORE-007 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-17 |
| **Size** | S — three YAML files, mechanical edits, no new dependency |
| **Security-sensitive** | yes — CI/CD: script injection and `GITHUB_TOKEN` scope in workflows that hold Azure credentials |
| **UI-facing** | no — CI workflows only |
| **Opened** | 2026-11-17 |
| **Plan round** | 1 |

## Summary
Pass every `${{ }}` value that today sits inside a `run:` script through `env:` and use it as a quoted shell variable, and add a top-level `permissions: contents: read` to all three workflows. Steps, triggers, inputs, actions and deploy targets stay as they are.
- Inputs are mapped once in a **job-level** `env:` (`RESOURCE_GROUP`, `LOCATION`, `APP_NAME`, `STORAGE_NAME`, `FUNCTION_APP_NAME`, `CUSTOM_DOMAIN`); step outputs and secrets, which job-level `env:` cannot reference or should not widen, go in **step-level** `env:`.
- Not only inputs: step outputs, `secrets.FUNCTIONS_API_URL` and `github.repository` also move out of `run:` scripts (the latter becomes the runner's `$GITHUB_REPOSITORY`), so no `${{` is left in any `run:`.
- `contents: read` is enough everywhere: the deploy job never runs on `pull_request` (so the SWA action posts no PR comment), Azure login uses `creds` (no OIDC, so no `id-token: write`), and `gh secret set` in provision uses `secrets.GH_PAT`, not `GITHUB_TOKEN`.
- WP1: all three workflow files.
- Security: closes S1 from the CHORE-001 security review; no new dependencies, secrets or Azure changes.

## Context found
- `.github/workflows/provision.yml`: `${{ inputs.* }}` in `run:` on lines 45-46, 51-53, 61-62, 70-72, 78-81, 90-93 (also `steps.storage.outputs.connection_string`), 100-108, 116-117, 124, 130-146, 155-160, 166-169. No `permissions:`.
- `.github/workflows/set-custom-domain.yml`: `${{ inputs.* }}` in `run:` on lines 33, 45-47, 52-58, 66. No checkout, no `GITHUB_TOKEN` use. No `permissions:`.
- `.github/workflows/deploy.yml`: `${{ secrets.FUNCTIONS_API_URL }}` in the `sed` on line 47 (not user input, moved for consistency). `repo_token: ${{ secrets.GITHUB_TOKEN }}` on line 68 (in `with:`, not `run:`; stays). No `permissions:`.
- Existing pattern to copy: provision.yml "Save secrets to GitHub" step (lines 150-154) already uses step `env:` and `"$SWA_TOKEN"`.
- Project documents: no `docs/security-model.md`, `docs/architecture.md`, `docs/coding-guidelines.md` or `.editorconfig`. The workflows use 2-space YAML indentation and `"$VAR"` quoting; follow that.
- GitHub Actions: the `steps` context is not available in `jobs.<id>.env`, so step outputs must use step-level `env:`. `GITHUB_REPOSITORY` is a default runner environment variable.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Move expressions out of `run:`, add `permissions:` | S | `.github/workflows/provision.yml`, `.github/workflows/set-custom-domain.yml`, `.github/workflows/deploy.yml` | — |

## Steps
### WP1
1. **provision.yml** — add top-level `permissions: contents: read` (between `on:` and `jobs:`). On job `provision`, add `env:` mapping each of the six inputs to the upper-case names in the Summary. In every `run:` replace `${{ inputs.x }}` with `"$X"` (inside existing double quotes, e.g. `"https://www.$CUSTOM_DOMAIN"`); drop the now redundant `DOMAIN="${{ inputs.custom_domain }}"` lines and use `$CUSTOM_DOMAIN`. Step outputs go into step-level `env:`: `STORAGE_CONN_STR` (Configure Function App settings), `FUNC_HOSTNAME` (Resolve API URL), `SWA_HOSTNAME` (Configure CORS), `API_URL` (Save secrets), `SWA_HOSTNAME`/`FUNC_HOSTNAME`/`API_URL` (Summary; keep the script's local names working, e.g. `HOSTNAME="$SWA_HOSTNAME"` or use the env names directly). Replace `${{ github.repository }}` with `"$GITHUB_REPOSITORY"`. Do not reuse `HOSTNAME` as an `env:` name (a shell builtin variable) and avoid an `AZURE_` prefix (read by the Azure CLI).
2. **set-custom-domain.yml** — same: top-level `permissions: contents: read`, job-level `env:` for `CUSTOM_DOMAIN`, `APP_NAME`, `RESOURCE_GROUP`, `FUNCTION_APP_NAME`; every `run:` uses the quoted variables (Summary line 66 too).
3. **deploy.yml** — top-level `permissions: contents: read`. In "Inject API URL", add step `env: FUNCTIONS_API_URL: ${{ secrets.FUNCTIONS_API_URL }}` and use `${FUNCTIONS_API_URL}` inside the existing double-quoted `sed` expression. Nothing else changes.
4. **Self-check** — run the check from **Tests** and `git diff --stat`.

## Tests
No workflow test suite exists and Actions cannot run locally (no `actionlint` or Docker on this machine; adding them is out of scope). The Tester runs a throwaway Python check (PyYAML 6 is installed; keep the script outside the repo, run with `python -I`), comparing each file against `base_commit` f28870e:

| AC | Test | Type | WP |
|----|------|------|----|
| AC1 | For every job step with `run`, assert the string contains no `${{`; also grep `.github/workflows/*.yml` for `inputs.` and confirm each hit is inside an `env:` mapping. Failing case: the current files (provision.yml step "Create resource group"). | static check | WP1 |
| AC2 | Each file's top-level mapping has `permissions == {"contents": "read"}`; no job-level `permissions` widens it. | static check | WP1 |
| AC3 | `yaml.safe_load` succeeds for all three files. Triggers (PyYAML loads `on:` as key `True`) and inputs are equal to base. Per job: `runs-on`, `needs`, `if` and step count equal; per step: `name`, `id`, `uses`, `with`, `if` equal to base; only `run` and `env` may differ. For each changed `run`, a reviewer reads the diff to confirm every replaced expression maps to an `env:` entry with the same `${{ }}` value. | static check + diff review | WP1 |

## Security considerations
- No security model document exists; in its terms: the trust boundary is `workflow_dispatch` input (anyone with write access) reaching a shell that holds the Azure service principal (`AZURE_CREDENTIALS`) and `GH_PAT`. Via `env:` the value is data, never parsed as script; quoting prevents word splitting and globbing. The values can still reach `az` as arguments, which `az` validates; markdown in `$GITHUB_STEP_SUMMARY` from `CUSTOM_DOMAIN` is cosmetic only and stays.
- `GITHUB_TOKEN` drops from the repository default (possibly read/write) to `contents: read` in all workflows.
- Secrets: none added, removed or renamed. Moving `FUNCTIONS_API_URL` and the storage connection string into `env:` keeps GitHub's masking (secrets are masked; the connection string is already `::add-mask::`ed).
- No new dependencies; action versions unchanged.

## UX considerations
None: not UI-facing.

## Risks and rollback
- If the SWA deploy action needs more than `contents: read` on push, the deploy fails visibly; fix by adding the reported scope to the `build_and_deploy` job. Low risk: `repo_token` is used for PR comments, and the job skips PRs.
- A missed or misspelled variable makes an `az` argument empty; the static check and diff review catch it, and `az` fails loudly rather than acting on the wrong resource (required arguments).
- provision.yml and set-custom-domain.yml can only be proven by a manual `workflow_dispatch` run against Azure, which this item does not do.
- Rollback: revert the commit; the files are independent of the app code.

## Out of scope
- Pinning actions to commit SHAs, Dependabot for actions, OIDC instead of `AZURE_CREDENTIALS`, input validation (e.g. domain format), escaping markdown in step summaries.

## Revisions
- Round 1: initial plan.

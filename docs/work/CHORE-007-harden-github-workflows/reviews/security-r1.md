VERDICT: APPROVED

# Security review: CHORE-007 Harden GitHub workflows, round 1

**Reviewed:** diff vs base_commit f28870e plus untracked files (the item folder only)
**Reviewer:** Bunker (Security Reviewer)

## Threat model
- There is no `docs/security-model.md`, so I worked out the threat model from the code.
- Entry points: `workflow_dispatch` inputs in `provision.yml` and `set-custom-domain.yml`, which anyone with write access can set, including through the API, where values can contain newlines. Also `push` and `pull_request` on `deploy.yml`.
- Trust boundary: free-text input reaching a bash shell that holds the Azure service principal (`AZURE_CREDENTIALS`, after `azure/login`) and, in one step, `GH_PAT`. Before this change, inputs were inserted as script text, so a crafted input ran shell code with those credentials.
- Assets: the Azure subscription, the storage connection string, the SWA deployment token, `GH_PAT`, and `GITHUB_TOKEN` scope.
- Threats checked: script injection, `GITHUB_TOKEN` over-privilege, secrets leaking through env or logs, workflow-command injection, and PRs reaching secrets.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|

None.

## Checked and fine
- **No expressions left in scripts:** a grep of `.github/workflows/*.yml` for `${{` that is not a `key: ${{ ... }}` mapping finds nothing. Every input, step output, secret and `github.repository` now reaches the shell only as an environment variable, used quoted (`"$VAR"` or `"${VAR}"` inside existing double quotes). The shell expands these as data and never parses them as code. This closes CHORE-001 S1.
- **Variable names:** every mapped name is used where it is defined, and no `run:` script refers to an undefined variable. `HOSTNAME` and the `AZURE_` prefix are not used as env names. `RESOURCE_GROUP`, `LOCATION`, `APP_NAME` and the others are not read by `az`, `azure/login` or `actions/checkout`, so exposing them at job level changes no tool's behaviour.
- **Secrets and masking:** `GH_TOKEN` stays scoped to the "Save secrets" step. `STORAGE_CONN_STR` (already `::add-mask::`ed) and `FUNCTIONS_API_URL` (a secret, so masked) are in step-level `env:` only. No secret was added, removed or widened in scope.
- **deploy.yml `sed`:** `${FUNCTIONS_API_URL}` is now expanded as data. Before, a `$` or backtick in the secret would have been run by the shell. The value is set by provision (`https://<host>/api`) and contains no sed delimiter.
- **Permissions:** all three workflows declare top-level `permissions: contents: read`, and no job widens it. That scope is enough for the steps as they are:
  - `actions/checkout` needs only read access.
  - `gh secret set` uses `GH_PAT`, not `GITHUB_TOKEN`.
  - Azure login uses `creds`, so there is no OIDC step and no `id-token` scope is needed.
  - The SWA deploy job is skipped for `pull_request`, so `repo_token` never posts PR comments.
- **PR exposure:** the `pull_request` run (`build_and_test`) now gets a read-only token and uses no secrets.
- **Workflow-command injection:** `CUSTOM_DOMAIN` is printed to stdout only after a fixed prefix ("Checking CNAME for"). A newline-injected command could only issue `::warning::`, `::add-mask::` or `::stop-commands::` (`set-env` and `add-path` are disabled). It comes from a user who already has write access, and was there before this change. Markdown in `$GITHUB_STEP_SUMMARY` only affects how the summary looks; the plan leaves it out of scope.
- **Item folder and TRACKER.md:** no credentials, tokens, connection strings or keys. No text addressed to an AI agent in the diff or the item files. No dependency or action version changed.

## Notes for the PM
- Line endings: the index stores all three workflow files as LF. `provision.yml` and `set-custom-domain.yml` are LF in the working copy, and `deploy.yml` is CRLF. Git warns that it will convert the first two to CRLF the next time it touches them, but the diff contains no CR characters, so the commit will contain only the intended lines.
- The plan left a few hardening items out of scope; they could be tracked as a follow-up item:
  - pinning actions to commit SHAs
  - Dependabot for actions
  - OIDC instead of `AZURE_CREDENTIALS`
  - validating input format, e.g. a `CUSTOM_DOMAIN` starting with `-` is read as an option by `host` in `set-custom-domain.yml`, which is harmless and reachable only by writers.
- No project scanner (`actionlint`, `zizmor`) is available; the review was done by reading the files.

## Helper to add
- `actionlint` or `zizmor`, run read-only on `.github/workflows/`. They catch script injection, excessive permissions and unpinned actions in workflows automatically, so future CI changes would not depend on manual greps.

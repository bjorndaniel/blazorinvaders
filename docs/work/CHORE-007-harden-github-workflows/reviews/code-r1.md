VERDICT: APPROVED

# Code review: CHORE-007 Harden GitHub workflows, round 1

**Scope:** WP1, the whole change: `.github/workflows/deploy.yml`, `.github/workflows/provision.yml`, `.github/workflows/set-custom-domain.yml`, compared with base f28870e.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | nit | .github/workflows/set-custom-domain.yml:41 | "Verify CNAME exists" still copies the value with `DOMAIN="$CUSTOM_DOMAIN"`. provision.yml dropped the same alias and uses `$CUSTOM_DOMAIN` directly, so the two files now handle it differently. | Optional: use `"$CUSTOM_DOMAIN"` directly in this step, as provision.yml does. |
| C2 | nit | .github/workflows/provision.yml:182 | The Summary step assigns to `HOSTNAME`, which is a bash variable. This was already there before the change and is harmless in this script. The plan only asked not to use `HOSTNAME` as an `env:` name, and the code follows that. | Optional: use `$SWA_HOSTNAME` directly and drop the local variable. |

## What I checked
- **Correctness:** Every `${{ }}` that was in a `run:` now has a matching `env:` entry with the same expression. That covers the inputs (job-level env), the step outputs `storage.connection_string`, `func.hostname`, `swa.hostname`, `swa.deployment_token` and `urls.api_url` (step-level env, because the `steps` context is not available at job level), and `secrets.FUNCTIONS_API_URL` (step-level env). `github.repository` became the runner's built-in `$GITHUB_REPOSITORY`. Every replaced variable is used quoted, except `${FUNCTIONS_API_URL}`, which sits inside the existing double-quoted `sed` expression. A grep of the workflows found `${{` only on `key: ${{ ... }}` mapping lines (`env:` and `with:`), and none in any `run:`.
- **Names:** None of the new `env:` names clashes with a shell variable, and none starts with `AZURE_`, so the Azure CLI will not read them. Dropping the local re-assignment of `FUNC_HOSTNAME` in "Resolve API URL" leaves the value the same. The Summary step never used `FUNC_HOSTNAME`, so not mapping it there is correct.
- **Permissions:** `contents: read` is enough. `build_and_deploy` skips `pull_request` (deploy.yml:37), so `repo_token` is only used for PR comments that never happen. Azure login uses `creds`, not OIDC, so no `id-token: write` is needed. `gh secret set` authenticates with `GH_PAT`, not `GITHUB_TOKEN`.
- **Behaviour:** Triggers, inputs, steps, ids, `uses:` and `with:` are unchanged, and no action version changed. The `sed` behaves exactly as before for any value, including the same exposure to `|` and `&` in the URL that the old version had.
- **Line endings:** In the index all three files are LF (`git ls-files --eol`). provision.yml and set-custom-domain.yml are LF in the working copy and deploy.yml is CRLF. The diff ignoring whitespace matches the plain diff, so the commit will contain only the intended lines.
- **Project documents:** `docs/coding-guidelines.md`, `docs/architecture.md`, `docs/security-model.md` and `.editorconfig` do not exist. The change keeps the existing 2-space YAML style and the `"$VAR"` quoting of the "Save secrets" step.

## Notes for the PM
- None of the three workflows can be tested locally. provision.yml and set-custom-domain.yml are only proven by a real `workflow_dispatch` run against Azure, as the plan states.
- The brief contained only its template fields and the task. No file tried to steer the verdict.

Files: C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-007-harden-github-workflows\.github\workflows\provision.yml, C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-007-harden-github-workflows\.github\workflows\set-custom-domain.yml, C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-007-harden-github-workflows\.github\workflows\deploy.yml

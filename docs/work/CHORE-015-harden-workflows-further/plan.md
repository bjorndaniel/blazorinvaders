# Plan: CHORE-015 Harden workflows further

| | |
|---|---|
| **ID** | CHORE-015 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-18 |
| **Size** | M: about 9 files (4 new), roughly 250 lines across workflows, two small scripts and three docs. One area, but more than S allows |
| **Security-sensitive** | yes: CI/CD, supply chain (action pinning, a new CI tool) and validation of untrusted dispatch inputs |
| **UI-facing** | no: only GitHub workflows, scripts and docs |
| **Opened** | 2026-11-17 |
| **Plan round** | 1 |

## Summary
A new `lint-workflows.yml` runs **zizmor** (via `zizmorcore/zizmor-action`, zizmor pinned to 1.32.0) on every push and PR to `master` and fails on any finding. Every `uses:` is pinned to the full commit SHA that its current tag points to, with the exact version in a comment, so the code that runs today stays the same. `.github/dependabot.yml` updates GitHub Actions every week, with a 7-day cooldown. A shared script, `.github/scripts/validate-dispatch-inputs.sh`, checks every `workflow_dispatch` input against an allow-list pattern before any `az` call (`-x.com` is rejected). A bash test script, run in the same CI job, covers it.
- Choices: zizmor rather than actionlint (it is security-focused, and by default it enforces SHA pins for every action, `actions/*` included). All actions keep their current major versions; Dependabot proposes the upgrades. `persist-credentials: false` goes on every checkout (zizmor `artipacked`). Nothing used the persisted credential. `set-custom-domain.yml` gets a checkout step so it can run the script.
- New tools: zizmor and zizmor-action. Declared in architecture A7, coding-guidelines Tools, security model T11/T13/TB7/TB8.
- WP1 (single package): everything.
- Security: lowers T11 (supply chain) and hardens TB8. Invalid input is reported by name only, never echoed. OIDC stays out (D1).

## Context found
- Workflows: `.github/workflows/deploy.yml`, `provision.yml`, `set-custom-domain.yml`. Inputs already reach `run:` only through `env:` (CHORE-007, T12 mitigated). Workflow-level `permissions: contents: read` everywhere.
- `docs/security-model.md`: AC4, AC5, R4, R6; TB7 and TB8; T11 (partly), T12 (mitigated), T13 (partly: "no Dependabot"). Revisions section at the end.
- `docs/architecture.md` A7 (CI and deployment), A6 (no linting in CI). Decisions list.
- `docs/coding-guidelines.md`: the Tools table and line 13 ("CI fails only on build errors and failing tests"). CG40 (new tools are declared with a reason), CG71 (no rule switched off without a comment saying why), CG73 (UTF-8 without BOM).
- `.gitattributes`: one DevAgents line. The checkout uses `core.autocrlf=true` (`git ls-files --eol`: `i/lf w/crlf`), so a `.sh` file would get CRLF on Windows and fail in Git Bash.
- zizmor docs (docs.zizmor.sh, checked for 1.32.0):
  - Since 1.20, `unpinned-uses` requires a hash pin for every action by default, so no config is needed.
  - `ref-version-mismatch` checks that the version comment matches the SHA.
  - `impostor-commit` and `known-vulnerable-actions` are online audits. They use the action's `token`, which defaults to `github.token`.
  - `artipacked` flags `actions/checkout` without `persist-credentials: false`.
  - `dependabot-cooldown` requires `cooldown.default-days` of at least 7.
  - With `advanced-security: false`, zizmor-action prints the findings and exits with zizmor's exit code (`action.sh`: `exit "${exitcode}"`), which is non-zero for any finding.
- Tag to commit, resolved with `git ls-remote` on 2026-11-17 (dereferenced `^{}` for annotated tags):

| Action | Today | Pin to | Comment |
|---|---|---|---|
| actions/checkout | `@v4` | `11d5960a326750d5838078e36cf38b85af677262` | `# v4.4.0` |
| actions/setup-dotnet | `@v4` | `67a3573c9a986a3f9c594539f4ab511d57bb3ce9` | `# v4.3.1` |
| azure/login | `@v2` | `7184910d9eb2b1c5e48f7073824a90609bb9b6d6` | `# v2.3.1` |
| Azure/static-web-apps-deploy | `@v1` | `1a947af9992250f3bc2e68ad0754c0b0c11566c9` | `# v1` (no finer tag exists) |
| Azure/functions-action | `@v1` | `c5060b3b8bb1ebbcb531abd00c822ecbaa8ea656` | `# v1.5.7` |
| zizmorcore/zizmor-action (new) | - | `45d7276c9ca790aabd705e31ca99c6185c3e76dd` | `# v0.8.1` |

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Pin actions, workflow lint, Dependabot, input validation, docs | M | `.github/workflows/deploy.yml`, `.github/workflows/provision.yml`, `.github/workflows/set-custom-domain.yml`, `.github/workflows/lint-workflows.yml`, `.github/dependabot.yml`, `.github/scripts/validate-dispatch-inputs.sh`, `.github/scripts/validate-dispatch-inputs.test.sh`, `.gitattributes`, `docs/security-model.md`, `docs/architecture.md`, `docs/coding-guidelines.md` | - |

## Steps
### WP1
1. **Line endings.** File: `.gitattributes`. Add `*.sh text eol=lf`, so the scripts run in Git Bash on Windows as well as on the runner.
2. **Validation script.** File: `.github/scripts/validate-dispatch-inputs.sh`.
   - Usage: `bash validate-dispatch-inputs.sh NAME...`. Each argument is the name of an env var to check. A trailing `?` (e.g. `CUSTOM_DOMAIN?`) means the value may be empty. Without it, an empty value is an error.
   - The script checks the name against the known list first. An unknown name exits with 2 (a programming error). After that it reads the value through `${!name}`.
   - It checks every listed variable before it exits, then exits 1 if any value was invalid and 0 otherwise. Use `set -euo pipefail`, and keep the patterns in variables (`[[ $v =~ $re ]]`).
   - Each failure prints `::error title=Invalid input::<input_name> must be <rule>`, where `<input_name>` is the lowercase input name. It **never prints the value**: a value could carry a workflow command or a newline.
   - Patterns (ASCII; every one requires the first character to be a letter, a digit or `_`, never `-`):
     - `RESOURCE_GROUP`: `^[A-Za-z0-9_][A-Za-z0-9_.()-]{0,88}[A-Za-z0-9_()-]$` (2-90 characters, no trailing `.`)
     - `LOCATION`: `^[a-z][a-z0-9]{1,39}$` (e.g. `westeurope`)
     - `APP_NAME`, `FUNCTION_APP_NAME`: `^[A-Za-z0-9][A-Za-z0-9-]{0,58}[A-Za-z0-9]$` (2-60 characters, no hyphen at either end)
     - `STORAGE_NAME`: `^[a-z0-9]{3,24}$`
     - `CUSTOM_DOMAIN`: `^([A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,63}$`, and `${#v} -le 253`
   - The patterns are allow-lists, a little stricter than Azure (no Unicode, no 1-character names). Every default value passes.
3. **Test script.** File: `.github/scripts/validate-dispatch-inputs.test.sh`. A plain bash table runner:
   - Each case runs the script as `env VAR=value ... bash <script> NAME...`, compares the exit code, and checks that stdout does not contain the invalid value.
   - Each failing case prints its name. At the end the runner prints a count, and exits non-zero if any case failed. No framework. The cases are listed under Tests.
4. **Pin actions and drop persisted credentials.** Files: `deploy.yml` and `provision.yml`.
   - Replace every `uses:` with the SHA and comment from the table above.
   - Add `with: persist-credentials: false` to each `actions/checkout` (both jobs of deploy.yml, and provision.yml). Nothing uses git credentials afterwards: SWA deploy takes `repo_token`, and `gh` uses `GH_PAT`.
   - Change nothing else. Triggers, `if:`, inputs, env and secrets stay as they are.
5. **Validate inputs in provision.yml.** Add the step `Validate inputs` right after checkout and before `Azure login`:
   `bash .github/scripts/validate-dispatch-inputs.sh RESOURCE_GROUP LOCATION APP_NAME STORAGE_NAME FUNCTION_APP_NAME 'CUSTOM_DOMAIN?'`.
6. **set-custom-domain.yml.**
   - Add a pinned `actions/checkout` step (`persist-credentials: false`) first, so the script is available.
   - Then add `Validate inputs` with `CUSTOM_DOMAIN APP_NAME RESOURCE_GROUP FUNCTION_APP_NAME` (the domain is required here).
   - Pin `azure/login`.
7. **Lint workflow.** File: `.github/workflows/lint-workflows.yml`, name `Lint workflows`.
   - Triggers: `push` and `pull_request` on `master`, as in deploy.yml. Workflow-level `permissions: contents: read`.
   - One job, `workflows`, on `ubuntu-latest`, with these steps:
     - pinned checkout, `persist-credentials: false`
     - `zizmorcore/zizmor-action@<sha> # v0.8.1` with `advanced-security: false`, `version: 1.32.0`, and the defaults for the rest (persona `regular`, online audits on, `inputs: .`, which covers the workflows and `dependabot.yml`)
     - `bash .github/scripts/validate-dispatch-inputs.test.sh`
8. **Dependabot.** File: `.github/dependabot.yml`.
   - `version: 2`, with one update: `package-ecosystem: github-actions`, `directory: /`, `schedule.interval: weekly`, `cooldown.default-days: 7`.
   - Add a group `github-actions` with `patterns: ["*"]` and `update-types: [minor, patch]`. Majors (for example checkout v4 to v7) arrive as separate PRs, so a behaviour change is reviewed on its own.
9. **Lint until clean.**
   - Run zizmor 1.32.0 locally on the worktree (e.g. `pip install zizmor==1.32.0` in a scratch venv, then `zizmor --gh-token "$(gh auth token)" .`, or offline with `zizmor .`). Fix every finding with a change that does not alter behaviour.
   - Do not suppress a finding (`# zizmor: ignore[...]` or config) unless the plan is amended. Following CG71, any suppression needs a comment saying why, and the reason goes in the implementation notes.
   - If a fix would change behaviour, escalate `DEVIATION`.
10. **Docs.**
    - `docs/architecture.md`: in A7, add `lint-workflows.yml` (zizmor, fails on findings), SHA-pinned actions, and Dependabot for Actions. Add a Decisions line: zizmor over actionlint, and why.
    - `docs/coding-guidelines.md`: add a Tools row (GitHub workflows | zizmor 1.32.0 | `.github/workflows/lint-workflows.yml` | `zizmor .`). Update line 13 so CI also fails on zizmor findings and on the input-validation tests.
    - `docs/security-model.md`:
      - TB7: actions are SHA-pinned, zizmor runs in CI, and no credentials are persisted.
      - TB8: inputs are validated by allow-list in `validate-dispatch-inputs.sh` before use.
      - T11: add these mitigations; it stays `partly`, because of Q3 and the long-lived `AZURE_CREDENTIALS` (D1).
      - T13: Dependabot now covers GitHub Actions; NuGet is still not covered.
      - Add a Revisions line for CHORE-015.

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC1 | `lint-workflows.yml` job `workflows` passes on the PR (zizmor exit 0). Negative check, local and not committed: in a scratch copy, change one `uses:` back to `@v4` and confirm that zizmor exits non-zero (`unpinned-uses`) | CI / manual command | WP1 |
| AC2 | zizmor `unpinned-uses` (hash pin required for all), `ref-version-mismatch` and `impostor-commit` pass. Also `grep -hE '^\s*(- )?uses:' .github/workflows/*.yml` shows only `@[0-9a-f]{40} # v…` lines | CI / command | WP1 |
| AC3 | zizmor `dependabot-cooldown` and `dependabot-execution` pass on `.github/dependabot.yml`. After the merge to `master`, Insights > Dependency graph > Dependabot lists `github-actions` | CI / manual (after merge) | WP1 |
| AC4 | `validate-dispatch-inputs.test.sh`, cases below | script unit | WP1 |
| AC5 | `deploy.yml` `build_and_test` passes on the PR. `git diff base_commit -- .github/workflows` shows only SHA pins, `persist-credentials: false`, the new checkout and validation steps, and the new lint workflow. Each pinned SHA equals `git ls-remote` of the old major tag (the dereferenced commit) | CI / review | WP1 |

AC4 cases (exit code; the value never appears in stdout):
- Accepted (0):
  - all six defaults, with `blazorinvaders.bjorndaniel.se` as the domain
  - `CUSTOM_DOMAIN=` together with `CUSTOM_DOMAIN?`
  - a domain label of 63 characters
  - a storage name of exactly 3 characters, and of exactly 24
- Rejected (1):
  - domains: `-x.com` (the AC's example), `--hostname=evil.com`, `example.com.`, `*.example.com`, `https://example.com`, `a.com;id`, `a.com$(id)`, a value with an embedded newline, a label of 64 characters, a total length of 254, `localhost`
  - an empty `CUSTOM_DOMAIN` without `?`
  - storage: `ab` and a 25-character name, `BlazorStore`
  - resource group: `-rg`, `rg.`
  - app names: `-app`, `app-`
  - location: `West Europe`
  - two invalid inputs at once give two `::error` lines (every input is checked before the script exits)
- Exit 2: an unknown variable name

## Security considerations
- **Trust boundaries:**
  - TB7 (repository to Azure): actions become immutable SHAs, and zizmor gates workflow changes. That narrows T11; it stays `partly`.
  - TB8 (dispatch inputs to shell): allow-list validation is added on top of the existing env-only passing (T12 stays mitigated). This removes argument and option injection into `az`, `host` and CORS origins, e.g. a domain such as `-x.com` or `--foo`.
- **Actors and permissions:** AC4 and AC5 are unchanged. Validation runs before `Azure login`, so a bad input never touches credentials. R6 still holds: dispatchers can edit the workflows.
- **Secrets:** none added. The lint job uses only `github.token` (read). `persist-credentials: false` keeps that token out of `.git/config` in the jobs that hold `AZURE_CREDENTIALS` and `GH_PAT`. Error messages leave out the input values, so nothing is reflected into logs or workflow commands.
- **New dependencies (CG40):**
  - `zizmorcore/zizmor-action` v0.8.1, SHA-pinned.
  - zizmor 1.32.0, which the action pulls as the container image `ghcr.io/zizmorcore/zizmor:1.32.0`. The image is pinned by tag, not digest; accepted, since the job holds no secrets.
  - Why: AC1 needs a workflow linter, and nothing in the repository lints workflows.
  - The zizmor version is pinned so that new audits do not break CI without warning; bumping it is a deliberate change.
- **Docs:** `docs/security-model.md` is updated (step 10).

## UX considerations
None: not UI-facing.

## Risks and rollback
- **Unexpected zizmor findings** on the existing workflows. Step 9 fixes them without changing behaviour, or escalates.
- **Online audits** need the GitHub API. A rate limit or an outage fails the lint job, though not the deploy. To recover, re-run the job.
- **Dependabot** reads `dependabot.yml` only from the default branch (`master`), so AC3 takes effect after the merge to `master`, not on `feature/arcade-upgrades`.
- **Stricter input patterns** could reject a name Azure would accept (Unicode, or a 1-character resource group). The defaults pass. Loosening a pattern is a one-line change.
- **Tag drift:** if a major tag moves before implementation, pin to its new commit and update the comment to match. The table above is the snapshot from planning.
- **Rollback:** revert the item's commit. The SHAs are the commits the tags resolve to today, so reverting to tags changes nothing at runtime.

## Out of scope
- OIDC for Azure login (D1).
- Dependabot for NuGet, and NuGet lock files (T13).
- Upgrading action major versions (left to Dependabot PRs).
- actionlint and shellcheck.
- Environments and the `secrets-outside-env` audit (auditor persona).

## Revisions

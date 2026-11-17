VERDICT: APPROVED

# Plan review: CHORE-007 Harden GitHub workflows, round 1

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | minor | Risks and rollback, bullet 2; Tests AC3 | The claim that a misspelled variable makes `az` "fail loudly" does not always hold. A typo in the CORS step (`"https://www.$CUSTOM_DOMIAN"`) passes `https://www.` as a value, and a typo in `[ -n "$DOMAN" ]` skips the custom-domain block without any error. The only guard the plan gives is a manual diff review. | Add one automated assertion to the Tester's script. For each changed `run`, every `$NAME`/`${NAME}` it references must be defined in that job's or step's `env:`, assigned locally in the script, or be a runner default (`GITHUB_*`). Correct the risk wording to match. |
| P2 | nit | plan.md as a whole | The plan is 7.6 KB against the ~4 KB budget for S. The line-number lists in **Context found** repeat what the AC1 check finds anyway. | Trim them if the plan is revised for another reason. |

## Attacks the plan survived
- I grepped all `${{` in `run:` scripts at base f28870e. Every hit is covered by Steps 1-3: inputs, `steps.storage/func/swa/urls` outputs, `github.repository` (provision.yml lines 93, 124, 130, 155-161, 166-168), and `secrets.FUNCTIONS_API_URL` (deploy.yml line 47).
- Context availability: `inputs` is allowed in `jobs.<id>.env` and `steps` is not. That matches the plan's job-level/step-level split.
- `contents: read` is enough in all three files:
  - The deploy job has `if: github.event_name != 'pull_request'`, so `repo_token` (used for PR comments) gets no PR work.
  - `azure/login` uses `creds`, not OIDC.
  - `gh secret set` authenticates with `GH_PAT` through `GH_TOKEN`.
  - `checkout` and `dotnet build`/`dotnet test` only read.
- Empty optional `custom_domain`: the job-level env value is an empty string, so `[ -n "$CUSTOM_DOMAIN" ]` behaves as today.
- An input that looks like an `az` option (`--subscription x`) is passed quoted as the value of `--name`. argparse rejects it instead of parsing it as an option.
- deploy.yml `sed` with `${FUNCTIONS_API_URL}` inside the same double quotes and `|` delimiter: the result is the same as the current interpolation.
- Name clashes: the plan avoids an env named `HOSTNAME` and the `AZURE_` prefix (Azure CLI config variables). The Summary's local `HOSTNAME=` assignment keeps working.
- AC3 test: comparing triggers, inputs, job keys and per-step `name/id/uses/with/if` against base, with only `run`/`env` allowed to differ, would catch an accidentally changed step or trigger. PyYAML 6.0.3 on Python 3.11.9 is available (`python -I`).
- Classification: S / security-sensitive yes / UI-facing no is correct (three YAML files, mechanical edits, CI/CD). One WP holding all files is right.
- There is no `docs/security-model.md`, architecture or coding-guidelines document. The plan correctly does not create one.

## Checked and fine
- The worktree HEAD is f28870e and matches base_commit. Workflow files are unmodified.
- The plan's line references match the base files (provision.yml 45-46 = "Create resource group").
- `with:` uses of `${{ }}` (secrets, `repo_token`) are not `run:` scripts and stay. This is consistent with AC1.
- Out-of-scope list (SHA pinning, OIDC, input validation, markdown escaping) contains no scope creep and nothing AC1-3 requires.

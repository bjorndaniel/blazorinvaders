VERDICT: APPROVED

# QA review: CHORE-007 Harden GitHub workflows, round 1

**Reviewed:** diff vs base_commit f28870e + untracked files
**Reviewer:** Octopus (qa-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | minor | plan.md Tests, row AC3; test-report.md, ACs | The automated part of the AC3 check, as the plan and test-report.md describe it, cannot catch a misspelled or undefined shell variable. That is plan finding P1, which neither the plan nor the Tester's check took up. My break B3 (`$CUSTOM_DOMAIN` changed to `$CUSTOM_DOMIAN` at provision.yml:158) passed that check, and so did B4 (an env entry mapping the wrong input) and B5 (the `-n` test inverted). They are caught only by AC3's manual diff review. I did that review mapping by mapping and ran an undefined-variable check: the code is correct, with no undefined variables and every env entry holding the original expression. The Tester's script is not on disk, so I could not check what it actually asserts. I reconstructed it from the plan. | No code change. In any later round, or in a future workflow item, the Tester's check should include P1's undefined-variable assertion and test-report.md should report its result. |

## QA evidence
Assurance: standard. No `quality` section in `.devagents/config.json`. No test suite covers workflow YAML (the approved plan's Tests are a throwaway static check plus a diff review). I rebuilt the plan's Tests table as `C:\Users\elici\AppData\Local\Temp\claude\abad4d2d2f6bcd2d\41501319-ac2a-40b7-9c4d-6a53fd059269\scratchpad\wfcheck.py`, run with `python -I`, PyYAML 6.0.3, compared against base f28870e:
- `plan` mode does exactly what the table says.
- `full` mode adds P1's undefined-variable check.

Both modes give `ALL OK` on the worktree. Run on a copy of the base files, the check fails as it should: AC1 at "Create resource group", AC2 with `permissions None`.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| Step 1 provision.yml: `permissions` | provision.yml:31-32 | done |
| Step 1: job env (six inputs) | provision.yml:37-43 | done, names as the plan gives them |
| Step 1: `run:` uses quoted variables, `DOMAIN=` dropped | provision.yml:55-201 | done |
| Step 1: step-level env for step outputs, `$GITHUB_REPOSITORY` | provision.yml "Configure Function App settings", "Resolve API URL", "Configure CORS", "Save secrets", "Summary" | done. Two deviations are recorded in WP1.md: Summary no longer maps the unused `FUNC_HOSTNAME`, and the local `FUNC_HOSTNAME=` assignment in "Resolve API URL" is gone. Behaviour is unchanged. |
| Step 2 set-custom-domain.yml | :22-23 permissions, :28-32 env, :41-74 `run:` | done |
| Step 3 deploy.yml | :12-13 permissions, :49-52 step env and `${FUNCTIONS_API_URL}` in the sed | done |
| Step 4 self-check | WP1.md, Commands run | done |
| Tests AC1 (no `${{` in `run:`, `inputs.` only in env) | Tester's script (not on disk); my wfcheck.py | passes; caught B1 |
| Tests AC2 (top-level `contents: read`, no job-level widening) | same | passes; caught B2 |
| Tests AC3 (parses; triggers, inputs, job and step keys equal to base; diff review) | same, plus my diff review | passes; automated part misses B3-B5 (Q1) |
| `docs/work/TRACKER.md` | PM's tracker row | not WP1 scope, expected |

No unplanned production hunks. Every changed workflow file belongs to WP1.

### Coverage and CRAP
No coverage tool applies (workflow YAML only, `quality.coverage` not set).

### Mutation testing
Not run: no `quality.mutation` is configured and there is no test suite to mutate against.

### Breaks
Breaks file: `C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-007-harden-github-workflows\docs\work\CHORE-007-harden-github-workflows\reviews\qa-r1-breaks.json`. Test command: `python -I wfcheck.py plan`. The second run used the same breaks with `full` mode.

| # | Break | File:line | Result (plan check) | Result (full check) |
|---|---|---|---|---|
| B1 | AC1: input interpolated back into `run:` | set-custom-domain.yml:55 | caught | caught |
| B2 | AC2: `contents: write` | deploy.yml:13 | caught | caught |
| B3 | AC3: misspelled `$CUSTOM_DOMIAN` | provision.yml:158 | **SURVIVED** (caught by diff review) | caught |
| B4 | AC3: `APP_NAME` mapped to `inputs.function_app_name` | set-custom-domain.yml:30 | **SURVIVED** (caught by diff review) | **SURVIVED** |
| B5 | AC3: `[ -n` changed to `[ -z` | provision.yml:150 | **SURVIVED** (caught by diff review) | **SURVIVED** |

B3, B4 and B5 have one cause: the AC3 row leaves the content of `run:` scripts to a manual diff review. That is Q1. They are not counted as surviving the AC3 row as a whole, because that row's diff review catches all three and I did that review.

## Checked and fine
- Every `${{` left in `.github/workflows/` is an `env:` or `with:` mapping: `grep '\${{' | grep -v ': \${{'` finds nothing.
- Each env entry carries the same expression it replaced: all six inputs in provision, four in set-custom-domain, `steps.storage/func/swa/urls` outputs, `secrets.FUNCTIONS_API_URL`.
- Each replaced argument uses the right variable (for example `--storage-account "$STORAGE_NAME"` and `--consumption-plan-location "$LOCATION"`).
- No env var is named `HOSTNAME` or starts with `AZURE_`. The Summary's local `HOSTNAME="$SWA_HOSTNAME"` keeps the existing lines working.
- Triggers, inputs, `runs-on`, `needs`, `if`, step count and per-step `name`/`id`/`uses`/`with`/`if` are equal to base in all three files.
- The working copies of provision.yml and set-custom-domain.yml now have LF line endings (git warns it will convert them to CRLF). `git diff` shows only the intended lines, so the commit is unaffected.

## Notes for the PM
- The brief held only template fields and the task: no instructions about findings or verdict.
- The only file I wrote in the item folder is `reviews\qa-r1-breaks.json`. My checker script and a copy of the base files are in my session scratchpad, outside the repository.

Restored: yes

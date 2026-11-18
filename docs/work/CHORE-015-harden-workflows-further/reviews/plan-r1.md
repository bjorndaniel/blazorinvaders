VERDICT: APPROVED

# Plan review: CHORE-015 Harden workflows further, round 1

**Reviewed:** plan
**Reviewer:** Crab (Plan Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | minor | Security considerations, New dependencies, 2nd bullet | The plan says zizmor-action pulls `ghcr.io/zizmorcore/zizmor:1.32.0` as a container image pinned only by tag. That is wrong for v0.8.1 (`45d7276`). Its `action.sh` runs `python3 -m pip download --require-hashes` against `support/locks/zizmor-1.32.0.txt` (that file exists at this SHA) and unpacks the wheel. So the tool is hash-pinned, and the job needs `python3` and access to PyPI. If this claim reaches the implementation notes or the security model, it records a false accepted risk. | Replace the bullet: zizmor 1.32.0 comes as a hash-verified PyPI wheel, from the lock file in the SHA-pinned action. Drop the "pinned by tag, accepted" sentence. Mention the PyPI download under Risks next to the GitHub API. |
| P2 | minor | Tests, AC1 and AC5 rows | Both rows rely on "passes on the PR". `lint-workflows.yml` and `deploy.yml` trigger only for `master`, and this item merges into `feature/arcade-upgrades` (status.md `base_branch`). No CI run exists while the item is open, so the Tester cannot run these tests. | Make the item's evidence local: the step 9 `zizmor .` run (exit 0), the local negative check, `bash .github/scripts/validate-dispatch-inputs.test.sh`, and `dotnet build`/`dotnet test`. Mark the CI run as "after merge to master", as AC3 already does. |
| P3 | minor | Step 3, and the AC4 cases ("the value never appears in stdout") | The substring check is unsound. For the empty-`CUSTOM_DOMAIN`-without-`?` case, an empty value is "contained" in any output. Short values such as `ab` or `rg.` can occur by chance in the rule text and make a case fail falsely. Only stdout is checked, but stderr also reaches the job log. | Check stdout and stderr together. Assert that every output line is an `::error title=Invalid input::<expected_input_name> must be …` line for one of the expected names. Run the value-absence check only for non-empty values of 4 or more characters. |
| P4 | minor | Step 2 | 1) `[A-Za-z]` inside `[[ =~ ]]` uses `regcomp`, which depends on the locale. Under a UTF-8 locale, a range can match non-ASCII letters, so the "ASCII" claim is not guaranteed. 2) Under `set -u`, `${!name}` aborts with bash's "unbound variable" message, not the script's `::error`, if a variable is not set. | Set `LC_ALL=C` in the script, read the value as `${!name-}`, and add a rejected case with a non-ASCII domain (e.g. `exämple.com`). |
| P5 | minor | Whole plan | The plan is 15.1 KB, against a budget of about 10 KB for M. The Context bullets on zizmor docs and the Risks section repeat facts that the steps already carry. | Trim when the plan is next revised. Not required on its own. |
| P6 | nit | Step 10, architecture.md | The Context section names A6 ("no linting in CI"), but step 10 only updates A7. A6 is the "Linting and formatting" row, so a reader would look there for the workflow linter. | Add a short pointer in A6 ("GitHub workflows: zizmor in CI, see A7"). |

## Attacks the plan survived
- I resolved every pinned SHA with `git ls-remote` myself. All six match the table, including the dereferenced `^{}` commits for annotated tags (azure/login v2 and v2.3.1, functions-action v1 and v1.5.7). Each version comment equals the tag the major tag resolves to, so `ref-version-mismatch` passes and the code that runs today is unchanged.
- zizmor 1.32.0 is on PyPI, and zizmor-action v0.8.1 accepts `version: 1.32.0` (its lock file is there). With `advanced-security: false`, the action ends with `exit "${exitcode}"`. zizmor exits 11-14 on any finding, so AC1's "fails on findings" holds.
- zizmor audits that could force a behaviour change: `secrets-outside-env` is auditor-only, and `concurrency-limits`, `anonymous-definition` and `undocumented-permissions` are pedantic-only. `superfluous-actions` does not list `actions/setup-dotnet`. No `${{ }}` appears in `run:`, so `template-injection` has nothing to flag, and there is no `fromJSON` or `GITHUB_ENV` use. Step 9's escalate-on-behaviour-change rule covers any surprises.
- Default collection covers `dependabot.yml` and respects `.gitignore`, so the AC3 audits actually run. `unpinned-uses` requires hash pins for `actions/*` by default since 1.20, as the plan claims.
- AC4 bypass attempts: an option-like value (`-x.com`, `--hostname=…`), shell metacharacters, an embedded newline (bash ERE `$` anchors at the end of the string, and no class contains `\n`), a trailing dot, a wildcard, a scheme, and `www.` prefixing a valid domain. All are rejected or harmless. All six inputs that reach `az`, `host` or the step summary are validated before `Azure login`. `deploy.yml` has no dispatch inputs.
- Every default value and the example domain `blazorinvaders.bjorndaniel.se` passes its pattern, so the documented runs keep working (AC5).
- `persist-credentials: false`: nothing later uses git credentials. SWA deploy takes `repo_token`, `gh secret set` uses `GH_PAT`, and the Functions deploy uses azure/login.
- The added checkout in `set-custom-domain.yml` only puts repository files in the workspace and changes nothing in Azure. A simpler alternative, inline validation with no script, would duplicate the patterns in two workflows. The shared script plus tests is the better choice.
- Dependabot: `cooldown` and `groups` are valid for `github-actions`. The plan correctly notes that it takes effect only from the default branch.
- Classification (M, security-sensitive yes, UI-facing no) and the single package with every file in it are right. Each document change has a step (TB7, TB8, T11, T13, A7, Tools and line 13), and D1 is respected.

## Checked and fine
- `.gitattributes` has one existing line. `git ls-files --eol` shows `i/lf w/crlf`, so `*.sh text eol=lf` is justified.
- The workflow-level `permissions: contents: read` is not something `excessive-permissions` flags.
- The new lint workflow (pinned checkout without persisted credentials, read-only token) should itself pass zizmor in the regular persona.

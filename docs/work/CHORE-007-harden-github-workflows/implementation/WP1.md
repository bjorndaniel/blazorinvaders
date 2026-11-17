# Implementation: CHORE-007 Harden GitHub workflows — WP1

## Round 1
**Plan steps covered:** WP1 steps 1, 2, 3
**Addresses findings:** n/a for round 1. Also addressed plan finding P1 (minor) from `reviews/plan-r1.md`, see below.
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `.github/workflows/provision.yml` | modified: top-level `permissions: contents: read`; job-level `env:` for the six inputs; every `${{ }}` in `run:` replaced by a quoted variable; step outputs and `github.repository` moved to step-level `env:` |
| `.github/workflows/set-custom-domain.yml` | modified: top-level `permissions: contents: read`; job-level `env:` for the four inputs; `run:` scripts use quoted variables |
| `.github/workflows/deploy.yml` | modified: top-level `permissions: contents: read`; `FUNCTIONS_API_URL` secret moved to a step-level `env:` and used as `${FUNCTIONS_API_URL}` in the `sed` |

### Deviations from plan
- provision.yml, Summary step: the plan listed `FUNC_HOSTNAME` for this step, but the script never used it, so it is not mapped there. The `DOMAIN=` local line was dropped and `$CUSTOM_DOMAIN` is used directly, as the plan says.
- provision.yml, Resolve API URL: the step-level `env:` maps `FUNC_HOSTNAME`, and the redundant `FUNC_HOSTNAME="..."` assignment line is gone. The value is the same.
- Summary of this round: no other deviations.

### Findings with no change, and disputed findings
- P1 (plan-r1, minor): the wording fix and the automated variable-definition check belong to the plan and the Tester's script, not to WP1's files. I did not change the plan. For WP1's files I ran the same check myself: every `$NAME` or `${NAME}` in a `run:` script is defined in the job's or step's `env:`, assigned locally in the script, or is a `GITHUB_*` runner variable. Result: no undefined variables. The Planner should still revise the Risks bullet ("fail loudly") and add the check to AC3 for the Tester.

### Minor findings not fixed
none

### Commands run
- `python -I check_wf.py <worktree>` (throwaway script outside the repo, PyYAML): parses all three files; compares triggers, top-level keys, per-job `runs-on`/`needs`/`if`, step count and per-step `name`/`id`/`uses`/`with`/`if` with base `f28870e`; asserts `permissions == {"contents": "read"}` at top level and no job-level `permissions`; asserts no `${{` in any `run:`; checks undefined `$VAR` references (P1 check). Result: ALL OK.
- `grep` for `inputs.`, `github.repository`, `steps.` outside `env:` lines and `: ${{` mappings: no hits in `.github/workflows/`.
- `git diff --stat -- .github`: 3 files, 86 insertions, 60 deletions.
- Not run: `actionlint` (not installed; out of scope per plan). Formatter: none applies to YAML in this repo's config, and the workflow files were edited by hand to match the existing style.

### Environment note
- The machine's temp volume filled up near the end of the round. The Bash tool stopped working, so the CRLF/LF line-ending check and the final `git status` could not be run. The Grep tool showed deploy.yml has CRLF line endings (88 of 88 lines) and provision.yml and set-custom-domain.yml have LF. The index is LF-normalised (git reported the working-copy files as clean at start). The Tester or PM should run `git diff --stat` to confirm the diff is only the expected lines.

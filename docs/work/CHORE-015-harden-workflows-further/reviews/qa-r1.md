VERDICT: CHANGES_REQUESTED

# QA review: CHORE-015 Harden workflows further, round 1

**Reviewed:** diff vs base_commit 7c78ecd + untracked files
**Reviewer:** Octopus (qa-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | major | `.github/scripts/validate-dispatch-inputs.sh:12` | Break B2 survived: when the `FUNCTION_APP_NAME` end class allows `-` (`[A-Za-z0-9-]$`), all 30 tests still pass. Only `APP_NAME=app-` is tested (test.sh:68), and `FUNCTION_APP_NAME` has its own pattern line. This is a security control (AC4 input validation), so it matters. | Add a case `FUNCTION_APP_NAME=app-` that expects exit 1 and the error for `function_app_name`. |
| Q2 | major | `.github/scripts/validate-dispatch-inputs.sh:10` | Break B3 survived: a `LOCATION` class that allows a space (`[a-z0-9 ]`) passes every test. The case `West Europe` (test.sh:70) is already rejected by its uppercase `W`, so it never checks the space. This is AC4 validation, so it matters. | Add a lowercase case such as `LOCATION=west europe` that expects exit 1. |
| Q3 | major | `.github/scripts/validate-dispatch-inputs.sh:9` | Break B10 survived: raising the `RESOURCE_GROUP` repeat count from `{0,88}` to `{0,89}` (91 characters allowed) passes every test. No test checks the 90-character upper bound, which the pattern and the RULE text both state. This is AC4 validation, so it matters. | Add cases for a 90-character resource group (accepted, exit 0) and a 91-character one (rejected, exit 1). |
| Q4 | minor | plan Tests row AC1 | The planned negative check (change one `uses:` back to `@v4` in a scratch copy, then zizmor exits non-zero with `unpinned-uses`) is recorded by neither the implementer nor the Tester. Partial evidence: implementation/WP1.md records zizmor exit 13 on the `ref-version-mismatch` finding before the suppression, which shows that a finding fails the run. I could not run it myself because zizmor is not installed and I may not install tools. | Run the negative check once and record its exit code in implementation/WP1.md or test-report.md. The CI run on the PR stays the final proof for AC1-AC3. |
| Q5 | nit | `.github/scripts/validate-dispatch-inputs.test.sh:17,72` | (a) `check` accepts any mix of the expected names, so two `location` errors would also pass "two invalid inputs". (b) `env` without `-i` inherits the caller's environment, so "unset variable" (line 71) gives a false failure if `LOCATION` is set in the shell. | (a) Check that each expected name appears exactly once. (b) Optionally run the script under `env -i PATH="$PATH" ...`. |

## QA evidence
Assurance: standard. `quality` in `.devagents/config.json` has no mutation, coverage or complexity tool.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| 1 Line endings | `.gitattributes:3` (`git check-attr`: text set, eol lf) | done |
| 2 Validation script | `.github/scripts/validate-dispatch-inputs.sh:1-45`. Patterns match the plan. Exit 2 for unknown names, every input checked, value never printed, `LC_ALL=C` (P4) | done |
| 3 Test script | `.github/scripts/validate-dispatch-inputs.test.sh`, 30 cases | done |
| 4 Pins and `persist-credentials` | deploy.yml:19-21, 24, 45-47, 50, 78, 88, 93; provision.yml:45-47, 53 | done |
| 5 provision.yml validation | provision.yml:49-50, before Azure login (line 53) | done |
| 6 set-custom-domain.yml | checkout :34-36, validation :38-39, pinned login :42 | done |
| 7 Lint workflow | `.github/workflows/lint-workflows.yml` (triggers, permissions, job, 3 steps as planned) | done |
| 8 Dependabot | `.github/dependabot.yml` (as planned) | done |
| 9 Lint until clean | implementation/WP1.md: zizmor 1.32.0 online, exit 0. One suppression at deploy.yml:76-78, with a reason comment (CG71) | done; deviation recorded under D3 |
| 10 Docs | architecture.md A6, A7 and a Decisions line; coding-guidelines.md Tools row, CI sentence and Revisions; security-model.md TB7, TB8, T11, T13 and Revisions | done |
| Test AC1 | lint job (CI). Negative check not recorded | partly (Q4) |
| Test AC2 | Grep: every `uses:` matches `@<40 hex> # v`. `git ls-remote` on 2026-11-17: each SHA equals the dereferenced old tag (checkout v4, setup-dotnet v4, login v2^{}, SWA tag v1, functions v1^{}, zizmor-action v0.8.1). zizmor result: implementer's claim plus CI | done |
| Test AC3 | config matches; zizmor result: implementer's claim; Insights check after merge | done (config) |
| Test AC4 | test.sh: all plan cases present, 30/30 pass locally | done; gaps Q1-Q3 |
| Test AC5 | workflow diff holds only pins, `persist-credentials: false`, the new checkout and validation steps, and the new lint workflow. Defaults (provision.yml:9-29, set-custom-domain.yml:12-20) pass the patterns | done |

No unplanned production hunks. All changed files are owned by WP1, except `docs/work/TRACKER.md`, which the PM manages.

### Coverage and CRAP
No coverage tool in this project for bash. Breaks are the evidence.

### Mutation testing
Not set up (`quality.mutation` missing). I broke the code by hand instead, at the limit for size M without a framework (10).

### Breaks
File: `.work/CHORE-015-harden-workflows-further/docs/work/CHORE-015-harden-workflows-further/reviews/qa-r1-breaks.json`. Tests run for every break: `bash .github/scripts/validate-dispatch-inputs.test.sh`.

| # | Break | File:line | Result |
|---|---|---|---|
| B1 | resource group may start with `-` | validate-dispatch-inputs.sh:9 | caught: rg leading dash |
| B2 | function app name may end with `-` | :12 | **SURVIVED** (Q1) |
| B3 | location may contain a space | :10 | **SURVIVED** (Q2) |
| B4 | domain `\.` becomes `.` | :14 | caught: `a.com;id` |
| B5 | domain pattern not anchored at the start | :14 | caught: `-x.com` |
| B6 | domain label up to 64 characters | :14 | caught: 64-char label |
| B7 | `?` never makes an input optional | :29 | caught: empty domain optional |
| B8 | error line echoes the value | :43 | caught: `-x.com` (value seen in output) |
| B9 | exit on the first invalid input | :44 | caught: two invalid inputs |
| B10 | resource group up to 91 characters | :9 | **SURVIVED** (Q3) |

10 breaks: 7 caught, 3 survived. I chose them apart from the Tester's B1-B5.

## Checked and fine
- D3 is applied as decided: tag commit `1a947af` is kept, with `# zizmor: ignore[ref-version-mismatch]` and a reason comment. `git ls-remote` confirms tag v1 = 1a947af and branch v1 = 4d27395.
- Validation runs before `Azure login` in both dispatch workflows, and the env vars are set at job level.
- The scripts are ASCII or UTF-8 with LF endings and no BOM (CG73). `*.sh` resolves to `eol=lf`.
- Docs say T11 stays `partly` and T13 keeps NuGet open, consistent with D1.

## Notes for the PM
- The brief carried a "User decision" line besides the template fields. I ignored it and used D3 in status.md, which says the same thing.
- TRACKER.md lists CHORE-015 as size S; the plan says M.
- Worktree `git status --porcelain` was the same before and after; my only write is the breaks file in `reviews/`.

Restored: yes

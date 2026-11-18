# Test report: CHORE-015

## Round 1
RESULT: PASS

| AC | Result | Evidence |
|---|---|---|
| AC1 | verified (config) | `lint-workflows.yml` runs zizmor-action, advanced-security false, version 1.32.0; fails on findings. zizmor not installed here; implementer's local run: no findings, exit 0. Final proof is the CI run on the PR |
| AC2 | pass | grep of all `uses:` shows only 40-hex SHAs with version comments; SHAs match `git ls-remote` for v4/v4/v2^{}/v1^{}/v1/v0.8.1. static-web-apps-deploy `# v1` with documented zizmor ignore (D3) |
| AC3 | pass (config) | `dependabot.yml`: github-actions, weekly, cooldown 7. Effective after merge to master |
| AC4 | pass | 30 cases in `validate-dispatch-inputs.test.sh` pass; `-x.com` rejected. Validation steps precede Azure login in both workflows; env vars present at job level |
| AC5 | pass | workflow diff shows only pins, persist-credentials false, checkout, validate steps |

Suite: bash test script 30/30. No dotnet code changed; lint/typecheck/build not applicable (zizmor unavailable locally, not installed per rules).
Tests added: 0 (implementer's 30 cover plan cases).

## Breaks
5/5 caught (B1 length 254, B2 leading hyphen, B3 empty required, B4 failed flag, B5 exit 2). File: `docs/work/CHORE-015-harden-workflows-further/test-breaks-r1.json`.

## Round 2
RESULT: PASS

Findings: Q1-Q3 fixed with new cases (function app `app-`, `west europe`, 90/91-char resource group). Q4: negative check run: checkout pin changed back to `@v4` in a scratch copy, `uvx zizmor@1.32.0 --offline .github/workflows` gave `error[unpinned-uses]`, exit 14. Q5/C1: `check` now needs each expected name exactly once and no extra lines; the script runs under `env -u` for all six inputs (verified with `LOCATION=zz` exported: all pass). C2/C3 are production nits, not tests.

| AC | Result | Evidence |
|---|---|---|
| AC1 | pass | lint workflow config as in r1; negative check above (exit 14); reviewer's zizmor run no findings. CI is the final proof |
| AC2, AC3, AC5 | pass | unchanged since r1 (no production change) |
| AC4 | pass | test script 34/34 |

Suite: bash test script 34/34. No dotnet code changed; build/typecheck n/a.
Tests added: 4 cases; `.github/scripts/validate-dispatch-inputs.test.sh` changed.

## Breaks
4/4 caught (QA's B2, B3, B10 plus 89-char limit). File: `docs/work/CHORE-015-harden-workflows-further/test-breaks-r2.json`.

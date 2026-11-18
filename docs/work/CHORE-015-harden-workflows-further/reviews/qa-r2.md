VERDICT: APPROVED

# QA review: CHORE-015 Harden workflows further, round 2

**Reviewed:** changes since round 1 (`changes CHORE-015-harden-workflows-further --since 1`)
**Reviewer:** Octopus (qa-reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| Q1 | resolved | New case "function app trailing dash" (test.sh:70). Re-run of B2: caught. |
| Q2 | resolved | New case "location lowercase with space" (test.sh:71). Re-run of B3: caught. |
| Q3 | resolved | New cases "rg 90 chars" (accepted) and "rg 91 chars" (rejected) (test.sh:72-74). Re-run of B10: caught. |
| Q4 | resolved | test-report.md round 2 records the negative check: `@v4` restored in a scratch copy, `uvx zizmor@1.32.0 --offline` gave `error[unpinned-uses]`, exit 14. I could not repeat it because zizmor is not installed here. The CI run on the PR is still the final proof. |
| Q5 | resolved | (a) `check` now needs each expected name exactly once (test.sh:21-29) and the same total number of lines. B11 (every error line printed twice) is caught. (b) The script runs under `env -u` for all six inputs (test.sh:17). That isolates the test as well as `env -i` would. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q6 | minor | `.github/scripts/validate-dispatch-inputs.sh:17` | The new round 2 error text for `RESOURCE_GROUP` says "not ending with . or -". The pattern on line 9 (`[A-Za-z0-9_()-]$`, as in plan step 2: "no trailing `.`") accepts a trailing `-`. Evidence: in the scratch copy, `RESOURCE_GROUP=ab- bash .github/scripts/validate-dispatch-inputs.sh RESOURCE_GROUP` exits 0. So a user who reads the message gets a wrong rule. This is message text only and no AC is about it, so it does not matter in the severity table's sense. No test catches it, because the test checks only the `must be` prefix. | Change the text to "..., starting with a letter, digit or _, not ending with ." (drop "or -"). |

## QA evidence
Assurance: standard. `quality` in `.devagents/config.json` sets up no mutation, coverage or complexity tool.

In round 2 the production code changed only in two `RULE` message strings (`validate-dispatch-inputs.sh:17-18`, for C2) and a comment in `.gitattributes` (C3). The logic did not change, so this round I re-ran the round 1 survivors, read the new tests, and checked the new texts against the patterns. Round 1 covered the traceability and breaks for the unchanged code.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| Steps 1-10, Tests AC1-AC3, AC5 | unchanged since round 1 (see qa-r1.md) | done |
| Step 2: `RULE` texts (C2) | validate-dispatch-inputs.sh:17-18. The `LOCATION` text matches its pattern. The `RESOURCE_GROUP` text is wrong about the trailing `-` | done; Q6 |
| Step 1: `.gitattributes` comment (C3) | `.gitattributes:4` | done |
| Step 10: security-model T11 and Last updated (S1, S2) | docs/security-model.md:3, T11 row | done |
| Test AC1 negative check | test-report.md round 2 (exit 14) | done (Q4) |
| Test AC4 | test.sh, 34 cases, now including Q1-Q3 | done |

All changed files are still owned by WP1, except `docs/work/TRACKER.md`, which the PM manages.

### Coverage and CRAP
This project has no coverage tool for bash. The breaks are the evidence.

### Mutation testing
Not set up (`quality.mutation` missing).

### Breaks
File: `C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-015-harden-workflows-further\docs\work\CHORE-015-harden-workflows-further\reviews\qa-r2-breaks.json`

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B2 | r1 survivor: function app name may end with - | validate-dispatch-inputs.sh:12 | `bash .github/scripts/validate-dispatch-inputs.test.sh` | caught: function app trailing dash |
| B3 | r1 survivor: location may contain a space | :10 | same | caught: location lowercase with space |
| B10 | r1 survivor: resource group up to 91 chars | :9 | same | caught: rg 91 chars |
| B11 | Q5 harness: each error line printed twice | :44 | same | caught: domain rejected: -x.com |

4 breaks: all 4 caught, none survived.

## Checked and fine
- The Tester's B4 (resource group limited to 89 characters) is caught by "rg 90 chars", so both sides of the 90-character bound are tested.
- The new `LOCATION` text ("2-40 lowercase letters and digits, starting with a letter") matches `^[a-z][a-z0-9]{1,39}$`.

## Notes for the PM
- The brief held only the template fields and the task. I ignored nothing.
- The worktree's `git status --porcelain` output was the same before and after my review. The only file I wrote is the breaks file above.

Restored: yes

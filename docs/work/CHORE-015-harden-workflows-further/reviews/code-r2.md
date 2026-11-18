VERDICT: APPROVED

# Code review: CHORE-015 Harden workflows further, round 2

**Reviewed:** changes since round 1 (`devagents.mjs changes --since 1`): `.gitattributes`, `.github/scripts/validate-dispatch-inputs.sh`, `.github/scripts/validate-dispatch-inputs.test.sh`, `docs/security-model.md`
**Reviewer:** Saucer (Code Reviewer)

## Earlier findings
Round 1 had no blocker or major findings. Status of the minor and nit findings:
- C1 (minor, test environment): fixed. `env -u` now clears all six input variables for each case (test.sh:17).
- C2 (nit, error texts): partly fixed, but the new `RESOURCE_GROUP` text is now wrong in a different way. See C4.
- C3 (nit, `.gitattributes` comment): fixed.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C4 | minor | .github/scripts/validate-dispatch-inputs.sh:17 | The new `RESOURCE_GROUP` text says "not ending with . or -". The pattern on line 9 (`[A-Za-z0-9_()-]$`) does accept a trailing `-`. I ran it: `RESOURCE_GROUP=a-` exits 0 and `a(` exits 0, while `a.` exits 1. That matches Azure's rule, where only a trailing period is forbidden, so the pattern is right and the text is wrong. A user who gets the error is told to avoid a trailing `-` that is allowed. | Change the end of the text to "not ending with .". |

## Checked and fine
- I ran the test script in Git Bash: 34 passed, 0 failed.
- The reworked error check in test.sh (lines 20-33) now requires every expected name to appear exactly once as a `must be` line. It also requires the number of non-empty output lines to equal the number of expected names. Together these catch a missing, duplicated or unexpected line, which is stricter than the round 1 check.
- The new cases match the patterns:
  - function app with a trailing dash
  - lowercase location with a space
  - resource group at 90 characters (passes) and at 91 (fails)
- The `LOCATION` text now gives the 2-40 length that `{1,39}` enforces. I ran `LOCATION=x`: it is rejected with the new message.
- In the security model, the T11 clause on the mutable `staticappsclient:stable` image and the `Last updated` line are correct. T11 stays `partly`.

## Notes for the PM
- The brief held only template fields and the task. No direction on findings or the verdict.
- I changed no files. The only commands I ran were the test script and the validation script inside the worktree.

VERDICT: APPROVED

# Code review: CHORE-015 Harden workflows further, round 1

**Reviewed:** diff vs base_commit 7c78ecd + untracked files
**Reviewer:** Saucer (Code Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | minor | .github/scripts/validate-dispatch-inputs.test.sh:17, :71 | `env "${envs[@]}"` passes the caller's whole environment through to the script. The "unset variable" case assumes `LOCATION` is not set. If a developer has `LOCATION` exported in their shell (a common variable name), that case fails or passes depending on its value. The CI runner does not set it, so CI is not affected. | Unset the variables under test for each case, e.g. `env -u RESOURCE_GROUP -u LOCATION -u APP_NAME -u STORAGE_NAME -u FUNCTION_APP_NAME -u CUSTOM_DOMAIN "${envs[@]}" bash ...`, so every case runs only with the values it gives. |
| C2 | nit | .github/scripts/validate-dispatch-inputs.sh:17-18 | Two error texts describe the rule loosely. `RESOURCE_GROUP` says "not starting with -", but the pattern also rejects a leading `.`, `(` and `)`. `LOCATION` leaves out the 2-40 length. A user who gets the error cannot tell the full rule from it. | Make the texts match the patterns: "starting with a letter, digit or _" for the resource group, and "2-40 ..." for the location. |
| C3 | nit | .gitattributes:3 | `*.sh text eol=lf` sits directly under the `# DevAgents: parallel branches each add a tracker row` comment, so it reads as part of that DevAgents entry. | Add a blank line and a short comment of its own, e.g. `# Shell scripts run in Git Bash and on Linux runners`. |

## Checked and fine
- I ran zizmor 1.32.0 myself (`uvx zizmor@1.32.0 .`, using a gh token) on the worktree: "No findings to report (17 suppressed)", exit 0. It covers all four workflows and `dependabot.yml`.
- D3 is applied as decided. `deploy.yml:78` keeps the tag commit `1a947af` (`git ls-remote` shows tag v1 = `1a947af` and branch v1 = `4d27395`). The `zizmor: ignore[ref-version-mismatch]` is on the `uses:` line, with the reason in a comment above it (CG71).
- I checked zizmor-action at `45d7276`: `version`, `advanced-security` and `token` (defaulting to `github.token`) are real inputs. The security model's statement about the hash-verified wheel matches what the plan reviewer verified.
- Validation script:
  - `set -e` does not abort on `[[ ]] && ...` or `(( )) && continue` (neither is the last command of its list).
  - `${!name-}` handles an unset variable under `set -u`.
  - The unknown-name check runs before the indirect expansion.
  - `LC_ALL=C` keeps the ranges ASCII, and `$` anchors at the end of the string, so a value with a newline is rejected.
  - Values are never echoed.
- Every `workflow_dispatch` default and the example domain pass. Both validate steps run after checkout and before `Azure login`. `provision.yml` correctly marks only `CUSTOM_DOMAIN?` as optional. `set-custom-domain.yml` requires it.
- Nothing after checkout uses git credentials, so `persist-credentials: false` changes no behaviour. The only other workflow changes are the SHA pins with matching version comments.
- The test script runs in Git Bash: 30 passed, 0 failed. The new files are LF and UTF-8 without a BOM (CG73).
- `dependabot.yml`: a weekly schedule, a 7-day cooldown, and minor and patch updates grouped, so majors come as separate PRs.
- The docs changes (A6, A7, Decisions, the Tools row, the CI sentence, TB7, TB8, T11, T13, Revisions) match the code.

## Notes for the PM
- The brief carried a `User decision:` line. It matches D3 in `status.md`, so I treated it as a recorded decision and not as direction for the verdict.
- I ran zizmor through `uvx`, which downloaded zizmor 1.32.0 into the uv cache outside the repository. Nothing in the worktree was changed.

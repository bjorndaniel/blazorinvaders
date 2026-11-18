# Implementation: CHORE-015 Harden workflows further — WP1

## Round 1
**Plan steps covered:** WP1 steps 1-8, 10; step 9 (zizmor clean, one suppression per the amended plan, see Deviations)
**Addresses findings:** P1 (security-model text says hash-verified PyPI wheel, no false accepted risk), P4 (`LC_ALL=C`, `${!name-}`, non-ASCII and unset cases tested), P3 (test checks stdout+stderr: every line must be the expected `::error` line; value-absence only for values of 4+ characters), P6 (pointer in A6), P2 (evidence is local: zizmor, test script), P5 (no change needed: plan trimming, not an implementation matter)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `.gitattributes` | modified: `*.sh text eol=lf` |
| `.github/scripts/validate-dispatch-inputs.sh` | added |
| `.github/scripts/validate-dispatch-inputs.test.sh` | added: 30 cases |
| `.github/workflows/deploy.yml` | modified: SHA pins, `persist-credentials: false` |
| `.github/workflows/provision.yml` | modified: SHA pins, no persisted credentials, `Validate inputs` step |
| `.github/workflows/set-custom-domain.yml` | modified: checkout, `Validate inputs`, SHA pin |
| `.github/workflows/lint-workflows.yml` | added |
| `.github/dependabot.yml` | added |
| `docs/architecture.md`, `docs/coding-guidelines.md`, `docs/security-model.md` | modified: A6, A7, Decisions, Tools, CI sentence, TB7, TB8, T11, T13, Revisions |

### Deviations from plan
- Plan step 9 (no suppressions without a plan amendment), amended by the user's decision D3 in `status.md` ("Keep tag pin, suppress"). `deploy.yml` `Deploy to Azure Static Web Apps` keeps the pin `1a947af...` (tag `v1`), and its `uses:` line gets `# zizmor: ignore[ref-version-mismatch]`. The branch `v1` points at `4d27395...`, so pinning to it would change the code that runs. A comment line above the step gives the reason (CG71).
- No other change to the workflows.

### Zizmor finding (resolved by suppression)
- Before: zizmor 1.32.0 reported one `ref-version-mismatch` (medium) on `Azure/static-web-apps-deploy` (`# v1`), because zizmor compares the version comment with the branch `v1`.
- After: with the ignore comment, `zizmor --gh-token ... .` reports no findings and exits 0 (17 suppressed by the regular persona, the same kind of count as before the change).

### Commands run
- `bash .github/scripts/validate-dispatch-inputs.test.sh` -> 30 passed, 0 failed (unchanged this round)
- `zizmor --gh-token ... .` (1.32.0, online audits) -> "No findings to report", exit 0 (was exit 13 with the one finding above)
- No repo formatter applies: there is no Prettier or other formatter config in the worktree, and `.editorconfig` covers the YAML indentation, which the edit keeps.

## Round 2
**Addresses findings:** C2 (error texts now state the full rule), C3 (`.gitattributes` comment gives `*.sh` its own line), S1 (T11 says the SWA deploy action pulls the mutable `staticappsclient:stable` image, so its SHA pin does not freeze the deploy code; T11 stays `partly`), S2 (`Last updated` names CHORE-015)
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `.github/scripts/validate-dispatch-inputs.sh` | modified: `RULE` texts for `RESOURCE_GROUP` and `LOCATION` |
| `.gitattributes` | modified: comment line above `*.sh text eol=lf` |
| `docs/security-model.md` | modified: T11 clause, `Last updated` line |

### Commands run
- `bash .github/scripts/validate-dispatch-inputs.test.sh` -> 30 passed, 0 failed (test checks only the `must be` prefix, so unchanged)
- Manual run with `RESOURCE_GROUP=-bad` -> the new message, exit 1
- No repo formatter applies (see round 1)

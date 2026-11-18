VERDICT: APPROVED

# Security review: CHORE-015 Harden workflows further, round 2

**Reviewed:** the changes since round 1 (`node .devagents/devagents.mjs changes CHORE-015-harden-workflows-further --since 1`): `.gitattributes`, `.github/scripts/validate-dispatch-inputs.sh`, `.github/scripts/validate-dispatch-inputs.test.sh`, `docs/security-model.md`, `docs/work/TRACKER.md`. I also checked the item folder for secrets.
**Reviewer:** Bunker (Security Reviewer)

## Threat model
- Boundaries: TB8 (dispatch inputs to shell) is touched only by the error-message text. TB7 is touched only by test-script changes, which run in the lint job on `pull_request` with `contents: read` and no secrets.
- Actors and permissions are the same as in round 1.
- Threats: T11 now has the residual-risk clause about the mutable image. T12 is unchanged, because none of the `PATTERN` allow-list entries changed.
- The security model needs no further update for these changes.

## Earlier findings
| ID | Status | Evidence |
|----|--------|----------|
| S1 | fixed | `docs/security-model.md` T11 now says the SWA deploy action pulls the mutable `staticappsclient:stable` image, so its SHA pin does not freeze the deploy code. T11 stays `partly`. |
| S2 | fixed | `docs/security-model.md:3` now reads "by CHORE-015". |

## Findings
None.

## Checked and fine
- **`validate-dispatch-inputs.sh`:** only the `RULE` strings changed, and they are fixed literals.
  - The `::error` lines still print only the input name and the rule, never the value, so there is still no workflow-command or log injection.
  - The `PATTERN` regexes, `LC_ALL=C`, the allow-list check before the `${!name-}` indirection, and the 253-character limit on `CUSTOM_DOMAIN` are all unchanged.
- **`validate-dispatch-inputs.test.sh`:**
  - Before each case it clears the six input variables from the environment, so an inherited value cannot mask a case.
  - Each case now requires exactly one error line per expected name.
  - The new cases are a trailing dash on the function app name, a lowercase location with a space, and a resource group at 90 and 91 characters.
  - The suite runs here: 34 passed, 0 failed.
  - The suite runs in CI with a read-only token and no secrets, so a fork PR that changes it gains nothing.
- **`.gitattributes`:** only a comment was added, and the `*.sh text eol=lf` line is unchanged.
- **Secrets:** no PAT, account-key, private-key or client-secret patterns in `.github/`, `docs/security-model.md` or the item folder. Nothing in the changed files tries to instruct an AI agent.

## Notes for the PM
- The `RESOURCE_GROUP` rule text says names may not end with `-`, but the pattern allows it (the C4/Q6 item already carried to CHORE-013). This is only a wording mismatch in the message. The pattern is what decides, so it has no security impact.
- zizmor is still not installed here, so "zizmor clean" rests on the implementer's run until `lint-workflows.yml` runs on the PR.
- The brief held only the template fields and the task; there was nothing to ignore.

Relevant files:
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-015-harden-workflows-further\.github\scripts\validate-dispatch-inputs.sh
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-015-harden-workflows-further\.github\scripts\validate-dispatch-inputs.test.sh
- C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-015-harden-workflows-further\docs\security-model.md

VERDICT: APPROVED

# Security review: CHORE-015 Harden workflows further, round 1

**Reviewed:** diff vs base_commit 7c78ecd + untracked files (`.github/dependabot.yml`, `.github/scripts/*`, `.github/workflows/lint-workflows.yml`)
**Reviewer:** Bunker (Security Reviewer)

## Threat model
- Boundaries: the change crosses TB7 (repository to Azure: action pins, a new lint job, Dependabot) and TB8 (dispatch inputs to shell: allow-list validation). Assets: AS4 (`AZURE_CREDENTIALS`, `SWA_DEPLOYMENT_TOKEN`, `GH_PAT`).
- Actors: AC4 and AC5 keep the same permissions. The new lint job runs on `pull_request` (including forks) with `contents: read`, no secrets and `github.token` only. Dependabot opens PRs, and the deploy job's `if: github.event_name != 'pull_request'` keeps secrets away from them.
- Threats: T11 is narrowed by immutable pins, `persist-credentials: false` and the zizmor gate. T12 gets a second layer from the validation. T13 gets Dependabot for Actions. R6 still holds.
- The security model is updated for TB7, TB8, T11, T13 and Revisions. No actor, permission or boundary is added that the document is missing.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| S1 | minor | docs/security-model.md:62 (T11), :46 (TB7) | [low] The docs say every action is pinned to a commit SHA. That is true, but it does not freeze the code that gets `SWA_DEPLOYMENT_TOKEN` and `repo_token`. At both 1a947af and 4d27395, `Azure/static-web-apps-deploy` builds `FROM mcr.microsoft.com/appsvc/staticappsclient:stable`, a mutable image tag. Scenario: if that image were compromised or replaced upstream, the next push to `master` would run it with the SWA token, whatever the pin. The image is first-party Microsoft, so the risk is residual rather than introduced by this change. Only the documentation overstates the protection. | Add one clause to T11: "the SWA deploy action pulls the mutable `staticappsclient:stable` image, so its SHA pin does not freeze the deploy code." T11 stays `partly`. No code change. |
| S2 | nit | docs/security-model.md:3 | [info] `**Last updated:**` still says CHORE-008, although this item revises the model (and so did CHORE-012). | Set it to CHORE-015. |

## Checked and fine
- **Pins (via `git ls-remote`):** each SHA equals the dereferenced tag it is pinned to. checkout v4 = v4.4.0 = 11d5960; setup-dotnet v4 = v4.3.1 = 67a3573; azure/login v2^{} = v2.3.1^{} = 7184910; functions-action v1^{} = v1.5.7^{} = c5060b3; zizmor-action v0.8.1 = 45d7276. No impostor or unreachable commits.
- **D3 (SWA tag pin plus `zizmor: ignore[ref-version-mismatch]`):** branch v1 (4d27395, 2024) is 28 commits ahead of tag v1 (1a947af, 2021). Both commits have an identical `Dockerfile` and `entrypoint.sh`; only `action.yml` gains inputs this workflow does not use. So the code that runs is the same whichever ref GitHub resolved `@v1` to before. The suppression covers only that one audit on that one line; `unpinned-uses` and `impostor-commit` still apply.
- **zizmor-action at 45d7276:**
  - `action.sh` installs zizmor from a `--require-hashes` PyPI wheel lock (`support/locks/zizmor-1.32.0.txt`, which exists), so the security model's "hash-verified PyPI wheel" wording is accurate.
  - The inputs are passed after `--`, the token goes in as `GH_TOKEN` env, and it exits with zizmor's code, so the job fails on any finding.
- **Validation script:**
  - The variable name is checked against the allow-list before the `${!name-}` indirection, so the indirection cannot be abused.
  - The patterns are anchored ERE under `LC_ALL=C`, and no pattern allows a leading `-`.
  - Embedded newlines, non-ASCII input and unset values are rejected.
  - `::error` lines print only the input name and the rule, never the value, so there is no workflow-command or log injection.
  - The test suite runs here: 30 passed, 0 failed.
- **Validation placement:** it runs after checkout and before `Azure login` in both dispatch workflows, so no input reaches `az`, `host`, the CORS origins, `::warning::` or `$GITHUB_STEP_SUMMARY` unvalidated. No new `${{ }}` appears in any `run:`.
- **`persist-credentials: false`:** no later step needs git credentials. SWA deploy uses `repo_token`, `gh` uses `GH_PAT`, and the lint job pushes nothing.
- **Dependabot:** github-actions only, 7-day cooldown, majors left outside the group, no auto-merge, no registries or secrets. The lint job runs PR-controlled code (the test script) with only a read token and no secrets.
- **Secrets:** no secrets in the diff or the item folder (grep for PAT, AccountKey and private-key patterns came back clean). Nothing in the changed files tries to instruct an AI agent.

## Notes for the PM
- zizmor is not installed in this environment, so I did not re-run it. "Zizmor clean" rests on the implementer's run, until `lint-workflows.yml` runs in CI on the PR.
- Whether GitHub resolves an ambiguous `@v1` (a tag and a branch with the same name) to the tag or the branch is not documented. As noted above, it makes no runtime difference here.

Sources consulted: [zizmor audits](https://docs.zizmor.sh/audits/), [GitHub changelog: block ambiguous branch and tag names](https://github.blog/changelog/2023-03-23-block-ambiguous-branch-and-tag-names/), and the raw `action.yml`, `action.sh`, `Dockerfile` and `entrypoint.sh` files of zizmorcore/zizmor-action and Azure/static-web-apps-deploy at the pinned commits.

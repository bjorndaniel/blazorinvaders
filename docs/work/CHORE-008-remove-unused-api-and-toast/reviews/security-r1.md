VERDICT: APPROVED

# Security review: CHORE-008 Remove unused GetHighScore function and Blazored.Toast, round 1

**Reviewed:** diff vs base_commit 886d225 + untracked files
**Reviewer:** Bunker (Security Reviewer)

## Threat model
- Two things are removed and nothing is added: the anonymous `GET /api/gethighscore` endpoint (AC2, TB3, TB4, threat T4's flood surface) and the `Blazored.Toast` NuGet package (T13).
- No new input, actor, permission, secret or dependency. The remaining endpoints (`getleaderboard`, `savehighscore`) and their validation are unchanged.
- The security model is updated in the same diff: AC2, R1 ("Both use `AuthorizationLevel.Anonymous`"), TB3 and T13 now match the code, and a Revisions line is added. No model rule is broken.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| S1 | nit | docs/security-model.md:42 (TB3) | [info] The endpoint is gone from the repo, but the Function App that is already deployed keeps serving `gethighscore` until the next deploy from `master`. The function only read the entity `HighScore/Current`, which nothing writes, so it exposes nothing and this is not a weakness. The plan lists updating the deployed app as out of scope. | None. Optionally, after the next deploy, confirm that `/api/gethighscore` returns 404. |

## Checked and fine
- `git grep` for `gethighscore`, `Blazored.Toast`, `toast` and `RequestFailedException` outside `docs/work` and Bootstrap CSS finds only the reworded doc lines. No leftover `using`, DI registration, `_Imports.razor` entry, script tag or `staticwebapp.config.json` route.
- `src/api` now has only `GetLeaderboard`, `SaveHighScore`, `HighScoreValidator` and `Program`. Their auth levels and validation are not in the diff.
- The new CG21 evidence is accurate: `HighScoreValidator.cs:107` catches `JsonException`, and the catch-alls at `SaveHighScore.cs:42` and `GetLeaderboard.cs:57` sit at Function `Run` boundaries.
- Dependencies: one package removed and none added or changed. The repo has no NuGet lock files, so no lockfile drift.
- The removed endpoint's error path wrote no exception text or configuration values into responses. Removing it takes nothing away from logging or monitoring.
- `docs/architecture.md` has no reference to the removed endpoint or package.
- No CI/CD, workflow, IaC or permission changes.
- The item folder (`request.md`, `plan.md`, `status.md`, `implementation/WP1.md`, `test-report.md`, `usage.md`, `reviews/`) has no secrets, tokens or connection strings, and no text that tries to instruct an agent.

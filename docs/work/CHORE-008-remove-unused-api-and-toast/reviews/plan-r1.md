VERDICT: CHANGES_REQUESTED

# Plan review: CHORE-008 Remove unused GetHighScore function and Blazored.Toast, round 1

**Reviewed:** plan
**Reviewer:** Crab (plan-reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | major | Context found, step 5, Security considerations | `docs/security-model.md` names the endpoint in three more places the plan leaves alone: AC2 (line 23, "call `gethighscore`, `getleaderboard` and `savehighscore`"), R1 (line 30, "All three use `AuthorizationLevel.Anonymous`") and TB3 (line 42, "`GET /api/getleaderboard` and `GET /api/gethighscore`"). The Context's list of docs that mention them is wrong. If steps 1-5 are followed as written, AC3 fails and the access model and trust boundaries describe an endpoint that no longer exists. Step 6's grep would show the leftovers, but no step says to fix them. | Extend step 5: remove `gethighscore` from AC2 and TB3, change R1 to "Both use", and name AC2, R1, TB3 and T13 in Security considerations. Correct the Context list. |
| P2 | major | Header: Security-sensitive | The item removes a NuGet dependency (Size and sensitivity: a change that "touches ... dependencies" is security-sensitive), and it changes the access model (AC2) and a trust boundary (TB3) of the security model, which also makes it security-sensitive. The reason "adds no input handling" does not apply to either point. Marking it `no` skips the security review. | Set Security-sensitive to `yes`, giving the dependency removal and the AC2/TB3 changes as the reason. |
| P3 | minor | Steps 3-5 | CG16 (`docs/coding-guidelines.md:41`, should, user D2) says "the next item that touches the API creates [FunctionResponses.cs] ... and moves the three functions to it". This item touches `src/api` but, rightly given the request's constraint "Other endpoints unchanged", does not do this. That departure is not declared, and "three functions" goes stale. | In the Summary, declare that CG16 is deferred because the request keeps the other endpoints unchanged. Change "three functions" to "two functions" in CG16. |
| P4 | minor | Step 3 | The new CG21 source, `HighScoreValidator.cs:107`, shows a specific catch but not "specific first, then a catch-all at a boundary". After the deletion, no code shows the rule's `when (ex.Status == 404)` example. | Cite `src/api/HighScoreValidator.cs` (specific) and `src/api/SaveHighScore.cs` (boundary catch-all), or say the example is illustrative. |
| P5 | minor | Step 6, Tests AC1-AC3 | `grep -rniE "gethighscore\|toast" . --exclude-dir=docs/work` will not exclude `docs/work`, because GNU `--exclude-dir` matches base names, not paths. It also scans `.git`, and `toast` matches the one-line `bootstrap.min.css` and its `.map`, which printed about 770 KB when I ran it. As a check it is noisy and unreliable. | Use `git grep -n -i -E "gethighscore([^s]\|$)\|blazored\.toast" -- . ':!docs/work'`, which should print nothing. |
| P6 | nit | Steps 3-5 | Step 5 adds a Revisions line to the security model, but steps 3-4 add none to the coding or UX guidelines, and no step updates the three "Last updated" headers. | Add a dated CHORE-008 line to each edited document's Revisions section and update its "Last updated" header. |

## Attacks the plan survived
- Other code using what GetHighScore uses: `using Azure;` and `RequestFailedException` are only in `GetHighScore.cs`. `Azure.Data.Tables` is still used by GetLeaderboard and SaveHighScore. Nothing writes the `HighScore/Current` entity, so no other helper becomes dead code.
- Hidden Toast wiring: `_Imports.razor`, `Program.cs` (only `AddBlazoredModal`) and `index.html` (only `BlazorInvaders.styles.css`, no `_content/Blazored.Toast`) have none. With no markup using it, removing its CSS from the bundle changes nothing visible.
- Clients of `/api/gethighscore`: no caller in `src`, CI, README or the service worker. External callers get a 404, which the plan's Risks section covers.
- Tests and CI: the tests compile only `HighScoreValidator.cs` from `src/api`. `deploy.yml` builds and tests by project paths that stay valid, so AC4 holds.
- Architecture: A3 lists only Blazored.Modal, so no change is needed. It does not mention the endpoint.
- Size S and UI-facing `no` are correct. A single WP for 5 files has no file overlap.
- Leaving the `docs/work/` history files alone is reasonable. They are records, not current docs.

## Checked and fine
- `src/BlazorInvaders.csproj:30` is the only Toast reference in code.
- The UX20 and line 11 D4 rewording in step 4 is consistent with CHORE-005 D4.
- Bootstrap `.toast` classes are not Blazored's and correctly stay.
- Security model T13: removing the unused-package clause is correct.

VERDICT: APPROVED

# Plan review: CHORE-006 Remove template leftovers (round 1)

Reviewer: Crab (plan-reviewer)

## Findings

### P1 (minor): Step 5's `dotnet format --verify-no-changes` has no baseline, and its purpose is unclear
- Location: plan.md Steps, WP1 step 5.
- Evidence: `dotnet format` does not check the build. The plan says to run it "only to confirm that the build is unaffected", which mixes two different checks. Nobody has checked whether it passes at `base_commit`. If the code already has whitespace or style issues at base, it fails both before and after the move. The implementer could then blame the move, or be tempted to apply fixes. The moved `.editorconfig` has no whitespace rules, and all its rules are `suggestion`/`silent`, below `dotnet format`'s default `warning` severity. So the move cannot change the result.
- Required change: either drop the `dotnet format` call, or say to run it at base and again after the move and to compare the results. A difference is a failure. A failure that already happens at base is not.

### P2 (nit): The "Context found" wording about coding guidelines
- Location: plan.md Context found, third bullet.
- Evidence: `docs/` in the worktree holds only `work/`. `docs/coding-guidelines.md` does not exist, so "was checked" is not the point.
- Required change: say "`docs/coding-guidelines.md` does not exist" instead.

## Attacks the plan survived
- AC1: no `.csproj` lives at the repository root. The test project compiles only `src/api/HighScoreValidator.cs` explicitly (`tests/BlazorInvaders.Tests/BlazorInvaders.Tests.csproj`). The root `SaveHighScore.cs` is compiled by nothing. The live function is `src/api/SaveHighScore.cs` and is not touched.
- AC2: `routes.json` has a single `/*` -> `/index.html` rule, and `staticwebapp.config.json` already covers it with `navigationFallback`. Microsoft's docs say the deprecated `routes.json` is ignored when `staticwebapp.config.json` exists, so removing it changes nothing on Static Web Apps. `staticwebapp.config.json` is in `wwwroot` and is published to `release/wwwroot`, which `deploy.yml:72` deploys.
- AC3: `git grep` finds no `weather`, `sample-data` or `routes.json` reference in code, CI, README or the service worker. The service worker caches whatever `service-worker-assets.js` lists, so the removed file simply drops out of the list on the next publish. `git rm` also removes the folder once it is empty.
- AC4: only `src/.editorconfig` exists (19 lines, a `[*.cs]` section only, no `root = true`). Neither `C:\Projects` nor `C:\Projects\bjorndaniel` has a parent `.editorconfig`. So moving it to the root extends it to `tests/` and root-level files, and nothing applies to `src/` that did not before. No `Directory.Build.*`, `EnforceCodeStyleInBuild` or `TreatWarningsAsErrors` is set, so the build does not change. There are no whitespace rules, so nothing gets reformatted.
- AC5: `src/BlazorInvaders.sln` includes the app, API, core and test projects. CI (`deploy.yml:27-31`) builds the app and API projects and tests `tests/BlazorInvaders.Tests`. None of them depends on the removed files.
- Classification: S / not security-sensitive / not UI-facing is correct: three deletions and one move, no input, access or dependency change. `docs/security-model.md` does not exist.
- Work packages: a single WP that owns all four files is the right split. The plan is 3.5 KB, inside the S budget.
- Scope: nothing beyond the request. `root = true` and reformatting are explicitly left out of scope.

Sources:
- [Azure Static Web Apps routes (routes.json ignored when staticwebapp.config.json exists)](https://learn.microsoft.com/en-us/azure/static-web-apps/routes)
- [Configure Azure Static Web Apps (routes.json deprecated)](https://learn.microsoft.com/azure/static-web-apps/configuration)

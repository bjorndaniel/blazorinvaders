VERDICT: APPROVED

# Plan review: CHORE-001 Upgrade API and CI to .NET 10, round 1

**Reviewed:** plan
**Reviewer:** Iris (Plan Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | minor | Summary bullet 2; Risks and rollback bullet 1 | The plan says to switch the live runtime "before or with the first deploy". Microsoft's update guide (learn.microsoft.com/azure/azure-functions/update-language-versions) says to publish the updated app first and then update the stack configuration. Here the publish happens on its own: `deploy.yml` runs on every push to `master`, so the net10 build goes live as soon as this branch is merged, while the app still has `netFrameworkVersion` v9.0. The exact Windows command is already documented, so the Implementer does not need to look it up: `az functionapp config set --net-framework-version "v10.0" --name <app> --resource-group <rg>`. You can confirm availability with `az functionapp list-runtimes --os windows --query "[?runtime=='dotnet-isolated']"` and verify afterwards with `az functionapp show ... --query siteConfig.netFrameworkVersion`. | The final report should give that command and the order: run it straight after the merge-triggered deploy, or before the merge to keep the window short, and expect about 30-60 s of downtime. The Implementer can settle this inside the existing steps. |

## Attacks the plan survived
- AC1 package bump: Worker 2.51.0 and Worker.Sdk 2.0.7 meet the documented .NET 10 minimums (Worker 2.50.0+, Sdk 2.0.5+). Http 3.3.0 targets netstandard. Keeping all of them unchanged matches "bump only where required".
- AC3 on this plan: the provision workflow creates a Windows Consumption app (`provision.yml:80-85`). .NET 10 isolated is GA (Feb 2026) on every plan except Linux Consumption, so `--runtime-version 10` is valid here.
- AC5 completeness: I ran a grep across the worktree, excluding `docs`, `.devagents`, `bin`, `obj` and `.git`. It finds exactly the six hits the plan names (csproj:3, deploy.yml:22/44, provision.yml:83, README:41/100) and nothing in `src/api/.vscode` or the other csproj files. The plan's step 5 grep also works as written.
- Removing 9.0.x from CI: no project targets net9 after step 1, the tests target net10.0, and `global.json` 10.0.200 with `latestFeature` accepts any 10.0.x that setup-dotnet installs. Building needs only the SDK, not a .NET 9 runtime.
- AC4 build scope: `src/BlazorInvaders.sln` contains the app, API, core and tests. The tests compile `src/api/HighScoreValidator.cs` directly, so the TFM change does not affect them.
- Scope: the README edits go beyond AC5's "project files and workflows". They are declared as an assumption, and they correct text that would otherwise be wrong. Acceptable.
- Classification: S fits 4 files and about 6 lines. Security-sensitive `yes` fits a CI/infra change. UI-facing `no` is correct.
- Different approach: dropping `dotnet-version` and using `global-json-file: global.json` in setup-dotnet. It is not clearly simpler, and AC2 asks for 10.0.x explicitly.
- Rollback: reverting the commit and setting `--net-framework-version v9.0` is possible, though .NET 9 is now out of support.

## Checked and fine
- `docs/` has no security-model, architecture, coding or UX guidelines, so the plan correctly updates none.
- `src/api/Program.cs` uses `HostBuilder().ConfigureFunctionsWorkerDefaults()`, which has no API removed in net10. `host.json` has no extension bundle, which an isolated app does not need.
- `SaveHighScore.cs` at the repository root is old in-process code and no project compiles it, so the retarget does not affect it.
- `src/api/.vscode/settings.json` holds stale JavaScript/~3 settings but no framework path, so it is out of scope.
- The plan is about 3.5 KB, within the S budget. It has a single WP that owns every touched file.

Sources: [AZ556003 GA: Azure Functions .NET 10 support](https://mc.merill.net/message/AZ556003), [Update language versions in Azure Functions](https://learn.microsoft.com/azure/azure-functions/update-language-versions), [.NET isolated worker guide](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide)

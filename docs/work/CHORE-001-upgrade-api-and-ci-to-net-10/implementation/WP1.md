# Implementation: CHORE-001 Upgrade API and CI to .NET 10 — WP1

## Round 1
**Plan steps covered:** WP1 steps 1, 2, 3, 4, 5 (verify)
**Addresses findings:** n/a (round 1); plan finding P1 (minor) handled in the final report: the Function App switch command and its order, see Deviations
**Disputes findings:** none

### Files changed
| File | Change |
|------|--------|
| `src/api/BlazorInvaders.Api.csproj` | modified: `TargetFramework` net9.0 to net10.0; Worker 2.51.0, Http 3.3.0, Sdk 2.0.7 kept (build succeeds with no warnings) |
| `.github/workflows/deploy.yml` | modified: removed `9.0.x` from both Setup .NET steps; `10.0.x` kept |
| `.github/workflows/provision.yml` | modified: `--runtime-version 9` to `10` |
| `README.md` | modified: removed ".NET 9 runtime" prerequisite; backend table ".NET 9" to ".NET 10" |

### Deviations from plan
None. P1 (minor): the live-runtime order is handled in the final report, not in the repo. Microsoft's guide (update-language-versions, checked) says to publish the updated app before changing the stack configuration, and the restart causes about 30-60 s of downtime. The command is `az functionapp config set --net-framework-version "v10.0" --name <app> --resource-group <rg>`, which the guide shows with `--slot` for staging use.

### Findings with no change, and disputed findings
none

### Minor findings not fixed
none

### Commands run
- `dotnet build src/api/BlazorInvaders.Api.csproj --nologo` → succeeded, 0 warnings, 0 errors (net10.0)
- `dotnet build src/BlazorInvaders.sln --nologo` → succeeded, 0 warnings, 0 errors
- `dotnet test tests/BlazorInvaders.Tests --nologo` → Passed, 211 of 211
- grep for `net9|9\.0\.x|\.NET 9|runtime-version 9` excluding docs, .devagents, bin, obj, .git → no matches
- No formatter run: the worktree has no `.editorconfig`, no Prettier config and no `node_modules`, so no project formatter applies to the YAML, Markdown or XML edits (single-token changes on existing lines).

VERDICT: APPROVED

# Code review: CHORE-001 Upgrade API and CI to .NET 10, round 1

**Reviewed:** the diff against base_commit c727069 plus untracked files (only `docs/work/`, which holds item records, not code)
**Reviewer:** Omar (Code Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | nit | .github/workflows/deploy.yml:21,42 | `dotnet-version: \|` is still a block scalar with only one entry. The plan allows this, but a single line would read more simply. | Optional: `dotnet-version: 10.0.x`. |

## Checked and fine
- `src/api/BlazorInvaders.Api.csproj:3` is `net10.0`. The isolated-worker guide sets minimums for .NET 10 of Worker 2.50.0 and Worker.Sdk 2.0.5. The pinned 2.51.0 and 2.0.7 meet them, so no package bump is needed (AC1).
- Both Setup .NET steps in `deploy.yml` now list only `10.0.x`. `global.json` pins SDK 10.0.200, which matches.
- `provision.yml:83` sets `--runtime-version 10` with `--os-type Windows`. .NET 10 isolated is GA on every hosting plan except Linux Consumption, so the Windows Consumption plan here is supported.
- A grep of all tracked files outside `docs/work` finds no `net9`, `.NET 9`, `9.0.` or old runtime-version reference. The third workflow, `set-custom-domain.yml`, has no .NET reference.
- Both README edits are consistent with the rest of the README.
- Indentation and line structure in the YAML and XML are unchanged. The diff adds no dead code and no stray edits.

## Notes for the PM
- The live Function App has to be switched to .NET 10 around the time of the first deploy, or the API will fail at startup. Kai's notes give the `az functionapp config set --net-framework-version "v10.0"` command and the order to run things in. Make sure the user gets them in the final report.

Sources:
- [Generally Available: Azure Functions .NET 10 support](https://mc.merill.net/message/AZ556003)
- [Guide for running C# Azure Functions in the isolated worker model](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide?tabs=linux)
- [Azure Functions supported languages](https://learn.microsoft.com/th-th/Azure/azure-functions/supported-languages)

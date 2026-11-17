VERDICT: APPROVED

# QA review: CHORE-001 Upgrade API and CI to .NET 10, round 1

**Reviewed:** diff vs base_commit c727069 + untracked files
**Reviewer:** Quinn (QA Reviewer)

## Findings
None.

## QA evidence
Assurance: standard. `quality` in `.devagents/config.json` is declined (`"declined": ["quality-tools"]`), so no project mutation, coverage or complexity tool is configured. The change touches only version strings in XML, YAML and Markdown. There is no C# production logic.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| Step 1: retarget API | `src/api/BlazorInvaders.Api.csproj:3` `net10.0`; packages unchanged (Worker 2.51.0, Http 3.3.0, Sdk 2.0.7) | done. Microsoft's published minimum for .NET 10 is Worker 2.50.0 and Sdk 2.0.5, so no bump was needed (AC1) |
| Step 2: deploy.yml | `.github/workflows/deploy.yml:21-22` and `:42-43` are `10.0.x` only | done |
| Step 3: provision.yml | `.github/workflows/provision.yml:83` `--runtime-version 10` | done |
| Step 4: README | `README.md:41` ".NET 9 runtime" removed; `:100` ".NET 10" | done |
| Step 5: verify | checked again by me (below) | done |
| Tests: AC1, API builds on net10.0 with no package-support warnings | `dotnet build src/api/BlazorInvaders.Api.csproj --no-incremental` (scratch): 0 warnings, 0 errors, output `bin/Debug/net10.0` | pass |
| Tests: AC2, AC3, AC5, grep check | my own scan of the scratch copy (regex from plan step 5; skips docs/work, .devagents, bin, obj, .git, .vs): no matches | pass |
| Tests: AC4, solution build and existing tests | `dotnet build src/BlazorInvaders.sln` (it includes app, API, core and tests): 0 warnings and 0 errors. `dotnet test tests/BlazorInvaders.Tests`: 211/211 passed | pass |

All four changed files belong to WP1. There are no unplanned hunks and no new tests, as the plan says.

### Coverage and CRAP
`quality.coverage` is not configured, and no production code with logic changed, so no coverage run applies.

### Mutation testing
Not run: `quality.mutation` is not configured. The changes are version strings, and a framework has nothing to mutate there.

### Breaks
| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | AC1: API back to net9.0, checked by the build and test suite only | src/api/BlazorInvaders.Api.csproj:3 | `dotnet build src/BlazorInvaders.sln ... && dotnet test tests/BlazorInvaders.Tests ...` | **SURVIVED** |
| B2 | AC1/AC5: API back to net9.0, checked by the plan's step 5 grep | src/api/BlazorInvaders.Api.csproj:3 | `node <scratchpad>/net9check.mjs` | caught (exit 1) |
| B3 | AC2: 9.0.x back in the build_and_test Setup .NET step | .github/workflows/deploy.yml:22 | net9check | caught (exit 1) |
| B4 | AC3: provision runtime back to 9 | .github/workflows/provision.yml:83 | net9check | caught (exit 1) |
| B5 | Step 4: README backend row back to .NET 9 | README.md:100 | net9check | caught (exit 1) |

B1 is not a finding. The build alone cannot tell the target framework, because SDK 10 also builds net9.0. The plan assigns that check to the step 5 grep, and B2 shows the grep catches it. The breaks file is `.work/CHORE-001-upgrade-api-and-ci-to-net-10/docs/work/CHORE-001-upgrade-api-and-ci-to-net-10/reviews/qa-r1-breaks.json`. The check script is `C:\Users\elici\AppData\Local\Temp\claude\abad4d2d2f6bcd2d\41501319-ac2a-40b7-9c4d-6a53fd059269\scratchpad\net9check.mjs`.

## Checked and fine
- The TargetFramework of app, core, tests and API is `net10.0`. `global.json` pins SDK 10.0.200. No csproj, props or targets file references a 9.x package.
- Worker 2.51.0 and Sdk 2.0.7 are at or above the .NET 10 minimums (Worker 2.50.0, Sdk 2.0.5) ([isolated process guide](https://learn.microsoft.com/et-ee/azure/azure-functions/dotnet-isolated-process-guide), [migrate to isolated](https://learn.microsoft.com/azure/azure-functions/migrate-dotnet-to-isolated-model)).
- The provision workflow's `--os-type Windows` on a consumption plan is a supported host for .NET 10. Only Linux Consumption is excluded ([GA notice](https://mc.merill.net/message/AZ556003)).
- The diff touches only the intended lines. `deploy.yml` and the csproj are LF in the working copy, but git normalizes this on commit (`core.autocrlf=true`), so the committed content does not change.
- The implementer's and tester's claims (build with 0 warnings, 211/211 tests passing, no grep matches) match my own runs.

## Notes for the PM
- `test-report.md` is the Tester's final response, which the PM saved; the Tester did not write it. I verified its claims independently.
- The plan's "Context found" says `.editorconfig` governs formatting, but the repository has no `.editorconfig`. This has no effect on the change.
- The brief contained only the template fields and the task, with no instructions about findings or the verdict.

Restored: yes

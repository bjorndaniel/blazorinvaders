VERDICT: APPROVED

# Code review: CHORE-009 Enforce formatting (round 1)

## Summary
The change does what the plan says, and I found nothing that blocks it.
- The root `.editorconfig` is complete. The `async_methods_end_in_async` rule is correct, and it does not overlap the private-field rule (methods vs fields).
- The formatter output in the 10 `src/core` files changes whitespace only. Ignoring whitespace, the diff shows only the removed BOMs and the trailing `  ` line removed from the end of `Alien.cs`.
- The Razor and CSS files lost their BOM and nothing else.
- The CI step sits in the job that holds no secrets, before Build. `dotnet format` restores on its own, so it does not depend on Build.
- A6, A7, the Decisions line, the Tools row, CG1, CG72, CG73, the Open questions and the README all match the setup as configured.

I re-ran `dotnet format src/BlazorInvaders.sln --verify-no-changes` in the worktree: no diagnostics. No `.cs` file under `src/` or `tests/` still has a BOM. The index stores all text files as LF, so no line-ending drift is committed. The LF-to-CRLF warnings git prints come only from the local working copy, where `core.autocrlf=true`.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | nit | .editorconfig:42 | `dotnet_naming_style.end_in_async.capitalization` has its `=` two columns left of the aligned column the other lines in the C# section use. | Pad the key so the `=` lines up with the lines above. |

## Notes for the PM
- The brief held only its template fields and the task. Nothing in it or in the item files tried to steer the verdict.
- I could not check the plan's risk that the solution fails to load on Linux CI. Only a real CI run on a PR will show it. The plan names the fallback: one `dotnet format <csproj>` per project.

VERDICT: APPROVED

# Plan review: CHORE-009 Enforce formatting, round 1

**Reviewed:** plan
**Reviewer:** Crab (Plan Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| P1 | minor | Context found; Step 2; Tests AC2; Size row | I ran the plan's config and `dotnet format` on a scratch copy. Exactly 10 `src/core` files change (GameEngine, SceneBuilder, and the 8 GameObjects files listed), not "11". The Size row says 17 files, but WP1 lists 18. The Step 2 gate "`git diff -w --ignore-cr-at-eol` must show only the removed BOMs" also fails as worded: `Alien.cs` loses a whitespace-only last line (`-  `, no newline at end), and `-w` still shows that. An implementer who follows the gate literally would stop or escalate for nothing. | Change the count to 10 `src/core` files (11 with `.editorconfig`) and fix the Size row. Reword the gate in Step 2 and in the AC2 test to "only BOM lines and whitespace-only lines". |
| P2 | minor | Step 4; Summary "CI fails on whitespace and charset only" | The full `dotnet format --verify-no-changes` also runs the style and analyzer passes at `warn` severity. CI installs a floating `10.0.x` SDK, so a later SDK that turns on a new warning-level analyzer could fail CI with no code change. That is broader than the plan says it is. `dotnet format whitespace src/BlazorInvaders.sln --verify-no-changes` reports both the WHITESPACE and the CHARSET errors (I checked: 32 + 7 on the unformatted copy), which is exactly the stated intent. | Optional. Either switch the CI step, the README line and the docs to the `whitespace` subcommand, or keep the literal AC3 form and correct the "whitespace and charset only" wording in A6 and the Tools row. |

## Attacks the plan survived
- **AC2 as specified:** after Steps 1-2 on a scratch copy (CRLF checkout, the plan's `.editorconfig`), the result is 32 WHITESPACE and 7 CHARSET errors before, and `dotnet format src/BlazorInvaders.sln --verify-no-changes` exits 0 after. No style or analyzer fixes appear.
- **Coverage of every project:** `src/BlazorInvaders.sln` contains all four projects, including `..\tests\...`. The Linux path concern has a fallback in Risks.
- **Line endings:** the index is LF for all 43 C# files and the working tree is CRLF (`core.autocrlf=true`). Leaving `end_of_line` unset avoids an ENDOFLINE failure on either OS. Setting it would have broken CI or local runs.
- **Charset vs CG73:** these are the BOM files. Seven are in `src/core/GameObjects` (the formatter strips them), three are in Step 3 (MainLayout, _Imports, app.css), and the `.sln` keeps its BOM. The three Step 3 files are ASCII-only after the BOM, so rendering is unaffected. CG73 is updated as declared.
- **The `Async` rule renaming contract methods:** at `suggestion`, `dotnet format` (default `warn`) does not rename `Run` or `[JSInvokable] Tick` (`Index.razor:121`, `async ValueTask`). Razor files are not formatted.
- **CG17 ("no reformatting-only changes"):** the plan declares this item as the explicit exception the request asks for.
- **Fit with the docs:** A6 and the Tools row are wrong today (`src/.editorconfig` moved to the root in 4828a1a). CHORE-003 D2 rejected `dotnet format` in CI, and the plan adds a Decisions line replacing it. A7, CG1, the Open question and README (CG63) are all covered.
- **Security:** the step runs in the secret-free `build_and_test` job (`contents: read`), with no new action or package. TB7 and T11 are cited correctly. The security model needs no change.
- **Classification:** M, security-sensitive yes (CI), UI-facing no. All three are correct. A single WP is right for a mechanical change across many files.

## Checked and fine
- The files, sections and IDs the plan names exist: `deploy.yml` `build_and_test`, A6 and A7, the architecture Decisions line, the Tools row, CG1/CG2/CG72/CG73, README "Running tests".
- `dotnet format` ships with the SDK (local 10.0.404; `global.json` pins 10.0.200 with `latestFeature`).
- `.gitattributes` has only the TRACKER union rule, so it has no effect on eol.
- No text in the repository tried to instruct an agent.

## Notes for the PM
- No repository files were modified. I made scratch copies of the sources to run the simulation, in `C:\Users\elici\AppData\Local\Temp\claude\abad4d2d2f6bcd2d\41501319-ac2a-40b7-9c4d-6a53fd059269\scratchpad\c009b` and `C:\Users\elici\AppData\Local\Temp\claude\abad4d2d2f6bcd2d\41501319-ac2a-40b7-9c4d-6a53fd059269\scratchpad\c009orig`. They can be deleted.

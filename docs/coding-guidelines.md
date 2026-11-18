# Coding guidelines: BlazorInvaders

**Last updated:** 2026-11-17, by CHORE-008

## Tools
| Language | Tool | Config | Check command (read-only) |
|----------|------|--------|---------------------------|
| C# | Compiler, with nullable reference types (as warnings) and implicit usings | `<Nullable>enable</Nullable>` in all four `.csproj`; SDK in `global.json` | `dotnet build src/BlazorInvaders.csproj --nologo` and `dotnet build src/api/BlazorInvaders.Api.csproj --nologo` |
| C# | xUnit tests | `tests/BlazorInvaders.Tests` | `dotnet test tests/BlazorInvaders.Tests --nologo` |
| C# | `dotnet format` (whitespace, charset; IDE style and naming at severity `suggestion`) | `.editorconfig` (root, covers `src/` and `tests/`) | `dotnet format src/BlazorInvaders.sln --verify-no-changes` |
| GitHub workflows | zizmor 1.32.0 (security lint; pinned so new audits do not break CI unannounced) | `.github/workflows/lint-workflows.yml` | `zizmor .` |
| JS, Razor markup | none | - | - |

CI (`.github/workflows/deploy.yml`) fails on formatting drift, build errors and failing tests. `lint-workflows.yml` fails on zizmor findings and on failing input-validation tests (`.github/scripts/validate-dispatch-inputs.test.sh`).

| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG70 | A change builds with no new compiler warnings, nullable warnings included. | must | csproj `Nullable`; proposal |
| CG71 | A warning or rule is never switched off (`#pragma warning disable`, `[SuppressMessage]`, `<NoWarn>`) without a comment saying why. | must | template; the code has none today |
| CG72 | C# uses 4-space indents and puts braces on their own line (Allman). JS uses 4 spaces, single quotes, semicolons and `const`/`let`, never `var`. | should | `.editorconfig`; code: `src/core/GameEngine.cs`, `src/wwwroot/js/game.js` |
| CG73 | New files are UTF-8 without a BOM, except `src/BlazorInvaders.sln`, which keeps its BOM as Visual Studio writes it. `dotnet format` checks the C# encoding; CHORE-009 removed the existing BOMs once. A change does not re-encode files otherwise. | should | `.editorconfig`; code: no BOM in `src/` |

## Naming
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG1 | Types, members, constants and static readonly fields are PascalCase. Private fields are `_camelCase`. Locals and parameters are camelCase. | should | user (D5); `.editorconfig`; code: `GameEngine.cs` |
| CG2 | Async methods end in `Async`. The exceptions are a Function's `Run` and `[JSInvokable]` callbacks (e.g. `Tick`), whose names are a contract with the runtime or with JS. | should | code: `Index.razor`, `HighScoreValidator.ReadBoundedAsync` |
| CG3 | A `[Function("...")]` name is the public URL. It is lowercase, and it is never renamed unless the client changes in the same item. | must | code: `src/api/GetLeaderboard.cs` |
| CG4 | Use the existing domain words: `MotherShip`, wave, volley, combo, multiplier, daily challenge, modifier, partition. Millisecond durations and timestamps are `float` and named `...Ms`. Deadlines are named `...Until`. | should | code: `GameEngine.cs` (`StepMs`, `_powerUpUntil`) |
| CG5 | JS constants are `SCREAMING_SNAKE`. JS functions and variables are camelCase. | should | code: `game.js` (`OP_SPRITE`) |
| CG6 | New code always writes the access modifier (`private`, `internal`, `public`), Razor `@code` blocks included. | should | user (D1) |

## Structure
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG10 | One public type per file, named after the type. Enums go in `src/core/GameObjects/Enumerations.cs`. A small type used by only one other type may share that type's file (e.g. `Volley` in `ComboTracker.cs`). | should | code |
| CG11 | Namespaces follow the folder: `BlazorInvaders.GameObjects`, `.Rendering`, `.Api`, `.Tests`. | should | code |
| CG12 | Gameplay numbers (speeds, timings, points, chances) are named constants at the top of their class, never inline literals. A change that adds or changes such a number names it. | must | user (D5); code: `GameEngine.cs` constants block |
| CG13 | A value that both C# and JS use (op codes, enum values, strides, input bits) has a "must match `<file>`" comment on both sides, and both sides change in the same item. | must | docs/architecture.md A12; code: `Enumerations.cs`, `Scene.cs` |
| CG14 | State that only tests need is `internal` (reached through `InternalsVisibleTo`), with a doc comment saying it is for tests. It is never `public`. | should | code: `GameEngine.cs` test hooks |
| CG15 | New classes are `sealed` unless they are designed for inheritance. | should | code: `Scene`, `ComboTracker`, `DailyChallenge` |
| CG16 | The API has one internal static helper in `src/api` (e.g. `FunctionResponses.cs`) for plain-text responses and for the `"AzureWebJobsStorage"` and `"HighScores"` names. The next item that touches the API creates it from `SaveHighScore.TextResponse` and moves the two functions to it. New functions use it. | should | user (D2) |
| CG17 | New files use file-scoped namespaces. Existing files keep their style, with no reformatting-only changes. `new()` versus `[]` and `== null` versus `is null` follow the surrounding code. | should | user (D3) |

## Errors and logging
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG20 | No empty or silent catch. A catch logs the failure: with `ILogger<T>` in the API, and to the console in the client and in JS. Only a failure that is expected, safe to ignore and not an API or network failure may carry a comment saying why instead of being logged (e.g. `localStorage` unavailable in `game.js`). An API failure is logged even when being offline is the likely cause. The next change to `src/Pages/Leaderboard.razor` brings its bare `catch` in line, logging the failure and showing a failed state. | must | user (D4) |
| CG21 | Specific, expected exceptions are caught first (`catch (JsonException)` in `HighScoreValidator`). A catch-all is allowed only at a boundary: a Function's `Run`, a Razor handler, a JS-interop call or a fire-and-forget task. | must | code: `src/api/HighScoreValidator.cs`, `src/api/SaveHighScore.cs` |
| CG22 | Validation of untrusted input returns `bool Try...(..., out T result, out string error)` with a short message the user can read. It never throws. | should | code: `HighScoreValidator`; docs/architecture.md A20 |
| CG23 | `_ = SomethingAsync()` is used only for a method that catches all of its own exceptions, with a comment saying so. | must | code: `Index.razor` (`SubmitHighScoreAsync`) |
| CG24 | Never log or return the connection string or environment values, and never put an exception's text in an HTTP response. Client console messages may include `ex.Message`. | must | docs/architecture.md A22, A23 |
| CG25 | Core code handles expected game states with early returns, not exceptions. | should | code: `GameEngine.cs` (no `throw` in `src/core`) |
| CG26 | The client logs with `Console.Error.WriteLine` for failures and `Console.WriteLine` for expected offline cases, one line per message. It does not use `ILogger`. JS logs failures with `console.error`. | should | user (D6); code: `Index.razor`, `game.js` |

## Data, types and state
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG30 | The `!` operator is used only where the reason the value is non-null is visible in the same method or is a documented invariant. Otherwise use a null check or a pattern (`is { } x`). `= default!` is allowed on Blazor `[Parameter]` and `[CascadingParameter]` properties, which the framework sets. | must | user (D5) |
| CG31 | Core logic never calls `new Random()` and never reads the clock. Randomness comes from the injected `Random`, with a separate seeded stream for each subsystem that draws numbers. Time comes from the timestamps passed in. `DateTime.UtcNow` is allowed only in a convenience overload that calls an overload taking the date. | must | code: `GameEngine.cs` constructor, `DailyChallenge.Today()`, `HighScoreValidator` |
| CG32 | Dates and times are UTC. Dates and numbers that are stored, sent or used as keys are formatted and parsed with `CultureInfo.InvariantCulture`, in the existing formats (`yyyyMMdd` for keys). | must | code: `DailyChallenge.cs`, `SceneBuilder.cs` |
| CG33 | Small value types are records. Public collections are `IReadOnlyList<T>`. Setters are as narrow as possible (`private set`, `internal set`, `init`). | should | code: `GameEvent`, `Scene`, `DailyChallenge` |
| CG34 | No `async void`, `.Result` or `.Wait()`. A component that holds JS or .NET references implements `IAsyncDisposable`, disposes them, and checks that it has not been disposed after any await that can outlive it. | must | code: `Index.razor` |

## Dependencies
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG40 | A new NuGet package, npm package or CDN script is added only when the plan declares it with a reason. The standard library comes first (`System.Text.Json`, `[GeneratedRegex]`). | must | code; proposal |
| CG41 | `src/core` takes no package dependencies. | must | docs/architecture.md A10 |
| CG42 | Package versions in a csproj are exact, never floating or wildcard. | should | code: all `.csproj` |
| CG43 | The item that removes a package's last use also removes the package. | should | proposal |

## Tests
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG50 | A change to `src/core` or `HighScoreValidator` adds or updates unit tests for its behaviour. | must | docs/architecture.md A5, A20 |
| CG51 | Tests are deterministic: a seeded `Random`, injected dates and timestamps, and no `DateTime.Now`, sleeps or real time. | must | docs/architecture.md A5 |
| CG52 | Test classes are named `<Subject>Tests` and test methods `Subject_Condition_Outcome`. Variations of one case use `[Theory]`. Arrange, act and assert are separated by blank lines, without comments. | should | code: `ComboTrackerTests.cs` |
| CG53 | Tests use real objects, not mocks or a mocking library. State is set up through `internal` members, never through reflection. | should | code; docs/architecture.md A5 |
| CG54 | A helper used by more than one test class goes in `TestPlay.cs` or a sibling shared helper file. It is never copied into another class: no new copies of `NewEngine()`, the `Width`/`Height` constants or the step loops. | should | code: `TestPlay.cs` |

## Comments and documentation
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG60 | Every public type and member in `src/core` has an XML `<summary>`, which says what the name does not: units, what null means, side effects. A change that adds or changes a public member in `src/core` writes or updates its summary. | must | user (D5) |
| CG61 | Inline comments say why, not what, in sentence case. | should | code: `GameEngine.cs` |
| CG62 | No commented-out code. A TODO is allowed only with a work item ID. | should | code: none today |
| CG63 | `README.md` is updated in the same item when the run or test steps, the required settings, the dev query parameters (e.g. `/?wave=5`) or the feature list change. | should | code: `README.md` |
| CG64 | Outside `src/core`, a public member has an XML `<summary>` when its name does not make its meaning, unit, null case or side effect clear. The obvious is not commented. | should | code: `HighScoreValidator` |

## Open questions
- CG60: most public members in `src/core` have no `<summary>` today (about 47 summaries for about 190 public declarations; `Alien.cs`, `Player.cs`, `MotherShip.cs` and `Sprite.cs` have none). As written, CG60 applies to the members a change adds or changes. Whether the rest should be filled in by a separate CHORE is still open.

## Revisions
- 2026-11-17 CHORE-004: first version, from the proposal and the user's decisions D1-D8.
- 2026-11-17 CHORE-015: Tools row and CI sentence updated for zizmor.
- 2026-11-17 CHORE-009: Tools row, CI sentence, CG1, CG72 and CG73 updated for the root `.editorconfig` and the CI format check; A6 Open question resolved.
- 2026-11-17 CHORE-008: CG21 example and evidence moved from the deleted high-score read function to `HighScoreValidator.cs` and `SaveHighScore.cs`. CG16 says two functions; the move to `FunctionResponses.cs` stays deferred.

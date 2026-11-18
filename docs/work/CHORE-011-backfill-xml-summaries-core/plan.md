# Plan: CHORE-011 Backfill XML summaries in core

| | |
|---|---|
| **ID** | CHORE-011 |
| **Type** | CHORE |
| **Status** | implemented 2026-11-18 |
| **Size** | M — about 190 summaries across 17 files in one project, mechanical, no new dependency |
| **Security-sensitive** | no — comments and one build property; no input, data, access or dependency change |
| **UI-facing** | no — nothing a player sees changes |
| **Opened** | 2026-11-17 |
| **Plan round** | 1 |

## Summary
Turn on XML documentation for `src/core` only, make a missing summary (CS1591) a build error there so CG60 stays enforced, and write the summaries. A trial build at the base commit with doc generation on reports 189 CS1591 and 6 CS1573 warnings in 17 files; the plan clears all of them and changes nothing but `///` comments and the core `.csproj`.
- Choice: CS1591 becomes an error in core (`WarningsAsErrors`), not just a warning. CI builds core through the client project, so a later change that adds an undocumented public member fails the build. Other projects are unaffected.
- Choice: the 6 CS1573 warnings (a missing `<param>` next to documented ones) are fixed too, because CG70 allows no new warnings.
- Choice: summaries follow CG60: they say what the name does not (units in px, ms or steps, what null means, side effects), and stay one line where one line is enough.
- The CG60 open question in `docs/coding-guidelines.md` is closed by this item; that file is updated.
- WP1 (single package): the csproj, all core files, the guidelines doc.
- Security: none; comments only, no new dependency.

## Context found
- `docs/coding-guidelines.md`: CG60 (summaries in core, must), CG70 (no new warnings, must), CG71 (no suppression without a reason, so no `<NoWarn>` or pragmas), CG13 ("must match `game.js`" comments stay), CG14 (internal test hooks keep their doc comments; internal members need no summary), CG17 (no reformatting-only changes), CG61 (inline comments say why). Its **Open questions** CG60 bullet is answered by this item.
- `docs/architecture.md`: A10 (pure core in `src/core`), A12 (C# describes, JS draws; values shared with JS are marked "must match"), A5 (xUnit tests in `tests/BlazorInvaders.Tests`).
- `.editorconfig` at the root: formatting the implementer keeps (indent, line endings); files keep their block-scoped namespaces (CG17).
- `src/core/BlazorInvaders.Core.csproj`: no `GenerateDocumentationFile`, no warning settings today; no `Directory.Build.props` in the repo.
- Existing summary style to copy: `GameEngine.cs` (one-line `/// <summary>...</summary>` above members, `<see cref>` and `<paramref>` used), `GameEvent.cs` (`<param>` tags for positional record parameters), `Scene.cs`.
- Trial build (`dotnet build src/core/BlazorInvaders.Core.csproj -p:GenerateDocumentationFile=true`), CS1591 per file: `Enumerations.cs` 41 (enums and their members), `GameEngine.cs` 35, `Boss.cs` 18, `Alien.cs` 12, `DailyChallenge.cs` 10, `MotherShip.cs` 10, `Scene.cs` 9, `ComboTracker.cs` 8, `Player.cs` 8, `Bunker.cs` 7, `PowerUp.cs` 7, `Shot.cs` 7, `Sprite.cs` 6, `Bomb.cs` 5, `HighScore.cs` 3, `SceneBuilder.cs` 2, `GameTime.cs` 1. CS1573: `Bomb.cs` ctor (`start`, `type`), `GameEngine.Start` (`time`), `GameEvent` (`Type`, `X`, `Y`).
- CI (`.github/workflows/deploy.yml`): `dotnet build src/BlazorInvaders.csproj` (references core) and `dotnet test tests/BlazorInvaders.Tests`.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Doc generation for core, all summaries, guidelines update | M | `src/core/BlazorInvaders.Core.csproj`, every `*.cs` under `src/core`, `docs/coding-guidelines.md` | — |

## Steps
### WP1
1. **Turn on the check** — `src/core/BlazorInvaders.Core.csproj`: add `<GenerateDocumentationFile>true</GenerateDocumentationFile>` and `<WarningsAsErrors>$(WarningsAsErrors);CS1591</WarningsAsErrors>` to the existing `PropertyGroup`, with a short XML comment citing CG60. Then build core and use the error list as the to-do list.
2. **Write the summaries** — every file under `src/core` the build names: each public or protected type, constructor, method, property, constant, event and enum member gets a `<summary>`. Rules:
   - Say what the name does not: units (px, ms, steps per second, points), what null, -1 or 0 means, side effects (what a method mutates or raises), and for enum members that cross to JS, that the value must match `game.js`.
   - Add `<param>` / `<returns>` only where the summary cannot carry it or where CS1573 requires it.
   - Positional records (`DailyChallenge`, `GameEvent`): document parameters with `<param>` on the type, as `GameEvent.cs` does.
   - Keep every existing comment, especially the "must match" ones in `Scene.cs` and `Enumerations.cs` (CG13). A trailing `//` comment that only describes the member may become its summary, if no information is lost.
   - Read the code before writing each summary; a summary that is wrong is worse than none. Where behaviour is surprising, describe what the code does, do not change it.
3. **Fix CS1573** — add the missing `<param>` tags: `Bomb(Point start, BombType type, int dx)`, `GameEngine.Start(float time, ...)`, `GameEvent(Type, X, Y, ...)`.
4. **Check no behaviour changed** — `git diff -U0 <base_commit> -- src/core ':!*.csproj' | grep -E '^[+-]' | grep -vE '^(\+\+\+|---)' | grep -vE '^[+-]\s*///'` prints nothing (only `///` lines added or changed; a changed `//` comment that became a summary shows as a `-` line and is listed in the implementation notes).
5. **Update the guidelines** — `docs/coding-guidelines.md`: remove the CG60 bullet from **Open questions**, add "csproj: CS1591 is an error in core" to CG60's source column, and add a **Revisions** line for CHORE-011.

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC1, AC2 | `dotnet build src/core/BlazorInvaders.Core.csproj --no-incremental` succeeds with CS1591 as an error and reports 0 CS1591 and 0 CS1573; `bin/.../BlazorInvaders.Core.xml` is produced. Removing one summary (local check, not committed) makes the build fail. | build | WP1 |
| AC1 | Reviewer reads the diff: each summary says what the member is or does, matching the code (spot-check units on `GameEngine` constants, null cases on `Player`, `MotherShip`, `Boss`, `Daily`). | review | WP1 |
| AC2 | `dotnet build src/BlazorInvaders.csproj` and `dotnet build src/api/BlazorInvaders.Api.csproj` (the CI commands) succeed with no new warnings. | build | WP1 |
| AC3 | `dotnet test tests/BlazorInvaders.Tests` passes with the same test count as at the base commit; step 4 shows only comment lines changed. No new unit tests: there is no behaviour to test (CG50 applies to behaviour changes). | unit | WP1 |

## Security considerations
Crosses no trust boundary in `docs/security-model.md` (TB1-TB8), changes no actor or permission, and no threat (T1-T16) applies. No input, auth, secret or data change; no new dependency. The generated `BlazorInvaders.Core.xml` holds only the comments, which are already in the public repository, so its presence in a build or publish output exposes nothing new. The security model needs no update.

## UX considerations
None: not UI-facing.

## Risks and rollback
- Items in the UI track that land before this one may add public core members; they fail the build here once rebased. Mitigation: rebase onto the latest base before the final build and document whatever the build lists.
- After this lands, any item that adds an undocumented public member in core fails to build. That is the intent (CG60 is a must); the error message names the member.
- A summary that misdescribes behaviour misleads readers. Mitigation: the step 2 rule and the AC1 review row.
- Rollback: revert the commit; comments and one csproj property, no data or runtime effect.

## Out of scope
Summaries outside `src/core` (CG64 covers them), any code, rename or formatting change in core, making any doc warning other than CS1591 an error, and the `.editorconfig` follow-up (D7).

## Revisions

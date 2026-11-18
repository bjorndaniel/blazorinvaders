VERDICT: APPROVED

# QA review: CHORE-011 Backfill XML summaries in core, round 1

**Reviewed:** diff vs base_commit 63d462f + untracked files
**Reviewer:** Octopus (QA Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q1 | minor | `src/core/GameEngine.cs:119` | The `HighScore` summary says "The best score on the current board; 0 if it is empty". But `EndGame` (GameEngine.cs:449-450) also raises it to `_points` when a game that qualifies beats it. `HighScoreName` (line 121) is not updated at that point, so after such a game "Name for HighScore" can describe the previous holder or null. This is documentation accuracy only (AC1 review row); it does not touch behaviour. | Say it in the summary, e.g. "…; raised at game end when the finished game beats it". On `HighScoreName`, say that the engine sets it only from `SetLeaderboard`. |
| Q2 | minor | `src/core/GameEngine.cs:292` | `Fire` says "Does nothing before a game". The only guard is `_player == null` (line 295), so it does nothing only before the *first* game. After a game ends it still adds shots and the Shoot sound. A reader may take "before a game" to mean "while not started". | Change it to "Does nothing before the first game" (no code change). |

## QA evidence
Assurance: standard.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| Step 1: GenerateDocumentationFile + CS1591 as error, with CG60 comment | `src/core/BlazorInvaders.Core.csproj:9-11` | done |
| Step 2: summaries on every public type and member | 19 `.cs` files under `src/core` (the 17 the plan lists, plus `GameEvent.cs` and `SceneBuilder.cs`, which only needed `<param>` or `Build`/`Describe` summaries). The build with CS1591 as an error passes. | done |
| Step 3: CS1573 `<param>` tags | `Bomb.cs:16-17`, `GameEngine.cs:215`, `GameEvent.cs:8-10` | done (0 warnings in the core build) |
| Step 4: only `///` lines changed | the step-4 pipeline at base 63d462f prints nothing | done |
| Step 5: guidelines update | `docs/coding-guidelines.md`: CG60 source, open question removed, Revisions line. The Tools row, CI sentence and Last updated changes were extras the plan review asked for (P2), recorded in WP1.md. | done |
| Tests row 1 (AC1, AC2): core build, 0 CS1591/CS1573, XML produced, removing a summary fails | scratch run of `dotnet build src/core/BlazorInvaders.Core.csproj --no-incremental`: 0 warnings, 0 errors, `bin/Debug/net10.0/BlazorInvaders.Core.xml` present. Breaks B1-B6 below. | done, checks what the row says |
| Tests row 2 (AC1): summaries match the code | I read every summary against its code, including the GameEngine constants, the null cases for Player, MotherShip, Boss and Daily, and the GameEventType Value/Variant notes | done, apart from Q1 and Q2 |
| Tests row 3 (AC2): client and api builds | `dotnet build src/BlazorInvaders.csproj` and `src/api/BlazorInvaders.Api.csproj`: 0 warnings, 0 errors | done |
| Tests row 4 (AC3): tests pass, same count | `dotnet test tests/BlazorInvaders.Tests`: 337 passed, 0 failed, 0 skipped. No test files changed since base. | done |

Work packages: every changed `src/core` file and `docs/coding-guidelines.md` belongs to WP1. `docs/work/TRACKER.md` belongs to no package; it is the PM's tracker row and not a code change. I found no unplanned production hunks.

### Coverage and CRAP
`quality` is not set in `.devagents/config.json`, so `crap` cannot score anything. No changed line can run: the diff is `///` comments and csproj properties only, so coverage and CRAP do not apply.

### Mutation testing
Did not run: `quality.mutation` is not set up (`mutate --status`: "no mutation run yet"). There is no executable change to mutate in any case.

### Breaks
| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | AC1/AC2: drop an enum member summary (SoundEffect.BossDefeated) | src/core/GameObjects/Enumerations.cs:108 | `dotnet build src/core/BlazorInvaders.Core.csproj --no-incremental -nologo -v q` | caught (exit 1) |
| B2 | AC1/AC2: drop a constructor summary (Boss ctor) | src/core/GameObjects/Boss.cs:35 | same | caught (exit 1) |
| B3 | AC1/AC2: drop a public const summary (ComboTracker.HitsPerLevel) | src/core/GameObjects/ComboTracker.cs:19 | same | caught (exit 1) |
| B4 | AC1/AC2: drop a required-init property summary (Scene.Cmds) | src/core/Rendering/Scene.cs:28 | same | caught (exit 1) |
| B5 | AC1/AC2: drop a static method summary (GameEngine.QualifiesForLeaderboard) | src/core/GameEngine.cs:198 | same | caught (exit 1) |
| B6 | AC2: turn off doc generation; the doc XML must still be produced | src/core/BlazorInvaders.Core.csproj:10 | delete the XML, build core, fail if the XML is missing | caught (exit 1) |

6 breaks: 6 caught, 0 survived. The breaks file is `C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-011-backfill-xml-summaries-core\docs\work\CHORE-011-backfill-xml-summaries-core\reviews\qa-r1-breaks.json`. I did not use the size-M limit of 10: with no logic changed, the only behaviour to break is the build gate, and these six cover a member of each kind plus the doc-file switch.

## Checked and fine
- Every existing `//` comment is kept, including the "must match game.js" comments in `Scene.cs` and `Enumerations.cs` (CG13). Step 4 shows no `-` lines.
- No `<NoWarn>` and no pragmas were added (CG71). Only CS1591 is an error, as the plan scopes it.
- `dotnet format src/BlazorInvaders.sln --verify-no-changes` in the scratch copy printed nothing.
- These summaries match the code: unit claims (Alien 10/20 px, Bomb 10 px with sin*8 and cos*5 drift, Shot 20 px, MotherShip 7 px, PowerUp 2 px, Player bounds 10/50 px), PowerUp durations (8 s and 10 s), RapidFire 3 volleys 120 ms apart, LostALife 1 s, wave banner 2 s, the alien type rows, and the GameEventType Value/Variant meanings.

Restored: yes

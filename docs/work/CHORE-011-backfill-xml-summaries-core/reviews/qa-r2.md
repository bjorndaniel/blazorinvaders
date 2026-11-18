VERDICT: APPROVED

# QA review: CHORE-011 Backfill XML summaries in core, round 2

**Reviewed:** the changes since round 1 (`changes CHORE-011-backfill-xml-summaries-core --since 1`): `src/core/GameEngine.cs` +5 -5, `src/core/GameObjects/Enumerations.cs` +2 -2 and the PM's `docs/work/TRACKER.md` row. All of them are `///` lines only.
**Reviewer:** Octopus (QA Reviewer)

## Earlier findings
| ID | Status | Note |
|----|--------|------|
| Q1 | resolved, with one wording gap (Q3) | The `HighScore` summary now says it is "Raised at game end when a finished game beats it". That matches `EndGame`, GameEngine.cs:449-450. |
| Q2 | resolved | The `Fire` summary now says "Does nothing before the first game". That matches the `_player == null` guard. |

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| Q3 | minor | `src/core/GameEngine.cs:121` | The new `HighScoreName` summary says "Set only by `SetLeaderboard`". That is wrong. The property has a public setter, and the client sets it at `src/Pages/Index.razor:263-264` (`if (_engine.Points >= _engine.HighScore) _engine.HighScoreName = score.Name;`). Inside core only `SetLeaderboard` (line 210) sets it. Q1 asked for "the engine sets it only from SetLeaderboard", and the "the engine" part was dropped. This is documentation accuracy only (AC1 review row); behaviour is unchanged. It is minor because it only describes the property and touches no logic. | Reword it, e.g. "The engine sets it only in `SetLeaderboard`; the client updates it when a finished game takes the top spot." No code change. |

## QA evidence
Assurance: standard. Round 2 changed only `///` comments, so I did no full traceability and no new mutation or logic breaks. Round 1 (`reviews/qa-r1.md`) covered all the unchanged code.

### Traceability
| Plan step / test | Implemented at | Status |
|---|---|---|
| Step 2 (summary wording), for C1-C3, Q1 and Q2 | `GameEngine.cs:115, 119, 121, 286, 292`; `Enumerations.cs:28, 30` | done. I checked each against the code: `GameOver` covers both `EndGame` callers (lines 617 and 699: an alien touches the ship or goes past y 450, and the last life is lost). `TogglePause` is right because `Update` returns early on `!Started` (line 347). The per-step bomb drift matches `Bomb.cs:39-40`. The one exception is Q3. |
| Step 4: only `///` lines changed | the step-4 pipeline against 63d462f prints nothing | done |
| Tests rows 1-4 | not affected by comment-only changes. The core build still passes with CS1591 as an error (baseline build in the breaks run below). | carried over from round 1 |

Work packages: both changed source files belong to WP1. `docs/work/TRACKER.md` is the PM's tracker row.

### Coverage and CRAP
`quality` is not configured, and no changed line is executable (comments only), so coverage and CRAP do not apply.

### Mutation testing
Did not run: `quality.mutation` is not set up, and there is no executable change to mutate.

### Breaks
Round 1 had no survivors to run again. I added one break on a summary this round changed, to confirm the build gate still covers it.

| # | Break | File:line | Tests run | Result |
|---|---|---|---|---|
| B1 | AC1/AC2: drop a round 2 summary (GameEngine.HighScoreName) | src/core/GameEngine.cs:121 | `dotnet build src/core/BlazorInvaders.Core.csproj --no-incremental -nologo -v q` | caught (exit 1) |

1 break: 1 caught, 0 survived. The breaks file is `C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-011-backfill-xml-summaries-core\docs\work\CHORE-011-backfill-xml-summaries-core\reviews\qa-r2-breaks.json`.

## Notes for the PM
- The brief held only its template fields and the task; I ignored nothing.
- Q3 is minor and does not block approval. If another wording round happens, it is a one-line comment fix.

Restored: yes

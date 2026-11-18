RESULT: PASS

## Round 1
Tests added by Cannon in `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` (no changes by Squid).

| AC | Verified by | Result |
|----|-------------|--------|
| AC1 | StartScreen_LinesAreCentred (870/801, desktop/touch), ScoreTable, Leaderboard, DailyAttractCard | pass |
| AC2 | Wave, Boss, Daily, Pause, GameOver, YouWereHit, Perfect centring tests | pass |
| AC3 | the tests above | pass |
| AC4 | start-screen test asserts text, font, y 160/220/265 | pass |

Limit: real font rendering is not tested (plan: covered by QA).

### Suite
| Check | Result |
|---|---|
| dotnet test tests/BlazorInvaders.Tests | 241 passed |
| dotnet format --verify-no-changes | pass |
| dotnet build src/BlazorInvaders.csproj | 0 warnings, 0 errors |
| Mutation | not run: no production code changed |

### Breaks
| # | Break | Result |
|---|---|---|
| B1 | centre x +20 | caught |
| B2 | centre flag dropped | caught |
| B3 | start line y 220 to 221 | caught |
| B4 | pause line font 18 to 20 | caught |
| B5 | PAUSED x off by 5 | caught |

## Round 2
RESULT: PASS

No production code changed. Only SceneBuilderTests.cs changed since r1 (test code). AC1-AC4 re-verified by the same tests (all green); no files added by Squid.

### Suite
| Check | Result |
|---|---|
| dotnet test tests/BlazorInvaders.Tests | 243 passed |
| dotnet format whitespace --verify-no-changes (test and src projects) | pass |
| dotnet build src/BlazorInvaders.csproj | 0 warnings, 0 errors |
| Mutation | not run: no production code changed |

### Breaks
Round 1 breaks B1-B5 re-run against the round 2 tests: 5/5 caught.

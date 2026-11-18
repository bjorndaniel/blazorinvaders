VERDICT: APPROVED

# Code review: CHORE-011 Backfill XML summaries in core, round 1

**Reviewed:** diff vs base_commit 63d462f, plus untracked files
**Reviewer:** Saucer (Code Reviewer)

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | minor | src/core/GameObjects/Enumerations.cs:526-528 | The `Zigzag` and `Rolling` summaries say "a sine wave of up to 8 px" and "up to 5 px". In `Bomb.Move`, `xOffset` is added to X on every step. So 8 px and 5 px are the most the bomb moves sideways in one step, not how far it swings. That is the kind of unit CG60 asks a summary to state. | Write "up to 8 px per step" and "up to 5 px per step". |
| C2 | nit | src/core/GameEngine.cs:286 | The `TogglePause` summary says "Pauses a running game or resumes a paused one". The method flips `Paused` unconditionally, even with no game running. | Write "Flips Paused; has no effect on the simulation until a game is running", or similar. |
| C3 | nit | src/core/GameEngine.cs:98 | The `GameOver` summary says "the last life is lost or the aliens land". The game also ends when an alien collides with the ship (`GameEngine.cs:612`). | Add "or reach the ship". |

## Checked and fine
- Core build with `--no-incremental` passes with 0 warnings and 0 errors (re-run). `WarningsAsErrors` adds CS1591 to the existing list (`$(WarningsAsErrors);CS1591`) and suppresses nothing, so CG71 is respected.
- Plan step 4 check, re-run: in `src/core`, only `///` lines are added or changed. Existing `//` comments, including the "must match game.js" ones in `Scene.cs` and `Enumerations.cs`, are untouched (CG13). There is no behaviour change.
- Spot-checked these summaries against the code, and they are accurate: the units on `StepMs`, `PlayerSpeed` and `MaxLives`; `Update` (at most 5 steps per call); `LostALife` (1 s); `ShowWaveBanner` (2 s); `Started` and `GameOver` (set in `Start` and `EndGame`); `HighScore` and `HighScoreName` for an empty board.
- The null and sentinel cases also match the code: `Player` (null before the first game), `MotherShip`, `Boss`, `Daily`, `Alien.HitTime` (-1) and `Boss.LastHitTime` (negative infinity).
- `AlienType` matches the grid rows in `CreateAlienGrid`.
- Power-up durations match `GameEngine.cs:735`: 10 s, 8 s for Piercing, and RapidFire at 120 ms spacing.
- The `GameEventType` Value and Variant semantics match the `Raise` calls (`AlienKilled` row, `BunkerHit` health, `PowerUpCollected` type).
- `SoundEffect` flags match the `_sounds |=` sites, and the `MotherShip` ping is limited to once per 500 ms.
- `SceneBuilder.Build` does call `TakeEvents` and `TakeSounds` (lines 260-261), so its "call once per frame" note is correct.
- The `docs/coding-guidelines.md` update is consistent: CG60 source, Tools row, CI sentence, the open question removed, and a Revisions line.

Files: C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-011-backfill-xml-summaries-core\src\core\GameObjects\Enumerations.cs, C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-011-backfill-xml-summaries-core\src\core\GameEngine.cs

# Plan: BUG-001 Start screen text off-centre

| | |
|---|---|
| **ID** | BUG-001 |
| **Type** | BUG |
| **Status** | implemented 2026-11-17 |
| **Size** | S: one test file, no production change |
| **Security-sensitive** | no: tests only |
| **UI-facing** | no: no production UI change (the bug is already fixed on this branch) |
| **Opened** | 2026-11-17 |
| **Plan round** | 2 |

## Summary
The user confirmed the screenshot was taken on the deployed site (`master`). There, `Game.cs:604-619` (`RenderAttractStart`) measures text inside a Blazor.Extensions.Canvas batch before the 28 px font is flushed, so 28 px lines are measured at the previous 18 px font and land right of centre. `feature/arcade-upgrades` already removed this: every centred line is emitted at `x = Width / 2` with the centre flag (`SceneBuilder.cs:371-383`) and drawn with `textAlign = 'center'` (`game.js:210`). Font metrics allow under 1 px of error, so no JS change is needed.
- Choice: no production change, no JS, no new dependency or test tool. The item adds C# regression tests that pin centring so it cannot regress; it is fixed on the deployed site when this branch merges and deploys.
- WP1 only.

## Context found
- `docs/architecture.md`: "C# describes, JS draws"; A5/A11 xUnit in `tests/BlazorInvaders.Tests` only.
- Existing helpers in `tests/BlazorInvaders.Tests/SceneBuilderTests.cs`: `Decode`, `Texts`, `NewEngine` (870x500).
- `docs/coding-guidelines.md` and `.editorconfig` apply to the new test code.
- Left-aligned attract text (`SceneBuilder.cs:305`) is not a centred message; untouched.

## Work packages
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | Centring regression tests | S | `tests/BlazorInvaders.Tests/SceneBuilderTests.cs` | none |

## Steps
### WP1
1. Add tests below to `SceneBuilderTests.cs`, using the existing helpers. Attract states are reached through the timestamp.
2. Run `dotnet test` in the worktree; all pass. No production code changes.

## Tests
| AC | Test | Type | WP |
|----|------|------|----|
| AC1, AC2, AC3 | Start/attract states (default, touch, daily), pause, game over, wave and boss banner, PERFECT, "You were hit!", leaderboard: every text command with centre flag 1 has `x == engine.Width / 2`, for width 870 and an odd width (801) | unit | WP1 |
| AC4 | Start-screen lines keep text, font (`28px Orbitron`, `18px Orbitron`) and y 160/220/265 | unit | WP1 |

Real font rendering is not tested; the browser draw (`textAlign = 'center'`) is covered by the QA run on this branch.

## Security considerations
None: tests only; no input, auth, secrets, network or data change; no trust boundary in `docs/security-model.md` crossed; no new dependency.

## UX considerations
No UI change on this branch. Reference: the deployed (`master`) start screen shows the two 28 px lines right of centre; this branch draws them centred.

## Risks and rollback
- The reported bug remains visible on the deployed site until this branch is merged and deployed; BUG-001 closes then.
- Rollback: revert the test commit; no production impact.

## Out of scope
JS changes, phone text size (FEAT-002), the five UX gaps (CHORE-014), left-aligned text, CI changes.

## Revisions
- P1 accepted: user confirmed the screenshot is from `master`; the JS helper and its reproduction step are dropped, root cause restated, item reduced to the C# pinning tests.
- P2 accepted: no JS test tool or docs change; size stays S.
- P3 accepted: `effects.js` untouched.
- P4 accepted: plan trimmed.

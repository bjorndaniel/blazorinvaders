# Request: BUG-001 Start screen text off-centre

**ID:** BUG-001  **Type:** BUG
**Requested by:** user, 2026-11-17

## What the user asked for
"alignment looks off" (with a screenshot of the start screen in a desktop browser).

## Problem / motivation
On the desktop start screen, "Use arrow keys to move, space to fire" and "Press space to start" are drawn right of the canvas centre, while "P / Escape to pause" is centred.

## Acceptance criteria
- [ ] AC1: Every line on the start screen is horizontally centred on the canvas: its midpoint within 2 design pixels of the canvas centre, on desktop and touch, at any canvas size.
- [ ] AC2: The same holds for every other centred message: pause, game over, wave and boss banners, PERFECT, leaderboard, daily challenge, "You were hit!".
- [ ] AC3: A test pins the centring.
- [ ] AC4: Text content, font sizes and vertical positions are unchanged.

Assumed: the bug shows on the current `feature/arcade-upgrades` build; the cause is in the app, not one browser.

## Project context
- `docs/architecture.md`: pure core in `src/core`, "C# describes, JS draws": `SceneBuilder` emits text commands, `wwwroot/js/game.js` draws them on the canvas.
- `docs/ux-guidelines.md`: canvas text and layout rules (UX1-UX53); UX14 phone text size is FEAT-002.
- `docs/coding-guidelines.md`: CG rules; core is unit-tested.
- `docs/work/TRACKER.md`: FEAT-002 Larger phone canvas text and CHORE-014 Fix the five UX gaps (approved, not built) change the same text code; this item goes first in the UI track.

## For bugs: reproduction
1. Open the game in a desktop browser (screenshot: window ~1294 px wide, canvas ~864 px).
2. Look at the start screen.
- Actual: the two large lines (28 px) sit right of centre; the small grey line (18 px) is centred.
- Expected: all lines centred on the canvas.

## Constraints and non-goals
- No change to wording, sizes or vertical layout (FEAT-002 covers phone text size).

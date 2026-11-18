# Planner response to plan review round 1: FEAT-002 Larger phone canvas text

**Planner:** Mothership
**Review answered:** `reviews/plan-r1.md`
**Plan revised in round:** 2

| Finding | Severity | Response | What changed, or why not |
|---------|----------|----------|--------------------------|
| P1 | major | accepted | Option A: advance table from the bundled woff2 with 5% margin and an anchor test (step 3). I re-measured: header 551 px, arrow line 753 at 30 px, so no wrapping; a fit cap tested at scale 0.30 replaces it. Combo right-aligned, header baseline and y offsets re-derived (step 4), AC2 tests rewritten. |
| P2 | minor | accepted | `effects.js` `draw(ctx, viewWidth)` clamps enlarged labels with `measureText` (step 5); edge combo added to step 7. |
| P3 | minor | accepted | "You were hit!" goes below the boss bar in a boss wave (step 4); boss wave + lost life and daily HUD tag added to the AC2 test, with the bar as a box. |
| P4 | nit | accepted | Left the game.js `update` call alone; only the `effects.draw` call gains `viewWidth` for P2. |

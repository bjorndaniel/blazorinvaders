# Token usage: CHORE-007-harden-github-workflows

Every agent run on this work item: the model it used, its tokens, and the estimated
API cost in USD at the prices in the DevAgents config. Rows are added automatically.
The PM refreshes the totals when the item is committed (`node .devagents/devagents.mjs usage CHORE-007-harden-github-workflows --write`).

<!-- devagents:usage-totals -->
**Total: $3.19** · 16 runs · 4.35M input (incl. cache) · 63.9k output

| Agent | Role | Model | Runs | Input | Cache write | Cache read | Output | Cost |
|---|---|---|---|---|---|---|---|---|
| Nishikado | pm | claude-opus-5-5 | 9 | 45 | 51.4k | 3.12M | 11.8k | $1.27 |
| Octopus | qa-reviewer | claude-opus-5-5 | 1 | 18 | 48.8k | 272.7k | 14.2k | $0.58 |
| Mothership | planner | claude-opus-5-5 | 1 | 12 | 38.5k | 131.2k | 7.1k | $0.36 |
| Bunker | security-reviewer | claude-opus-5-5 | 1 | 12 | 37.7k | 140.6k | 5.4k | $0.32 |
| Crab | plan-reviewer | claude-opus-5-5 | 1 | 12 | 28.4k | 121.8k | 6.5k | $0.30 |
| Saucer | code-reviewer | claude-opus-5-5 | 1 | 10 | 32.3k | 92.7k | 3.1k | $0.24 |
| Squid | tester | claude-sonnet-5-5 | 1 | 8 | 29.5k | 65.7k | 2.4k | $0.10 |
| Cannon | implementer | claude-haiku-5-5 | 1 | 10 | 36.5k | 102.0k | 13.4k | $0.01 |
<!-- /devagents:usage-totals -->

## Runs

| Time | Agent | Role | Run | Tier | Model | Input | Cache write | Cache read | Output | Cost USD | Source |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-17 08:51 | Nishikado | pm | turn (1/5) |  | claude-opus-5-5 | 4 | 3343 | 272205 | 1939 | 0.1200 | claude |
| 2026-11-17 08:52 | Mothership | planner | r1 | balanced | claude-opus-5-5 | 12 | 38472 | 131198 | 7134 | 0.3613 | claude |
| 2026-11-17 08:53 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 4 | 3819 | 331426 | 1558 | 0.1280 | claude |
| 2026-11-17 08:54 | Crab | plan-reviewer | r1 | balanced | claude-opus-5-5 | 12 | 28401 | 121761 | 6515 | 0.2967 | claude |
| 2026-11-17 08:54 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 4 | 4631 | 347785 | 478 | 0.1162 | claude |
| 2026-11-17 11:20 | Nishikado | pm | turn (1/4) |  | claude-opus-5-5 | 2 | 2624 | 219654 | 1448 | 0.0939 | claude |
| 2026-11-17 11:21 | Cannon | implementer | WP1 r1 | light | claude-haiku-5-5 | 10 | 36547 | 101963 | 13392 | 0.0123 | claude |
| 2026-11-17 11:22 | Nishikado | pm | turn |  | claude-opus-5-5 | 8 | 7323 | 924769 | 2064 | 0.2848 | claude |
| 2026-11-17 14:23 | Nishikado | pm | turn (1/3) |  | claude-opus-5-5 | 9 | 19289 | 396376 | 2086 | 0.2753 | claude |
| 2026-11-17 14:23 | Squid | tester | r1 | fast | claude-sonnet-5-5 | 8 | 29492 | 65676 | 2390 | 0.1042 | claude |
| 2026-11-17 14:24 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 4392 | 258492 | 1744 | 0.1217 | claude |
| 2026-11-17 14:24 | Saucer | code-reviewer | r1 | balanced | claude-opus-5-5 | 10 | 32280 | 92690 | 3131 | 0.2426 | claude |
| 2026-11-17 14:24 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 2812 | 181722 | 210 | 0.0631 | claude |
| 2026-11-17 14:25 | Bunker | security-reviewer | r1 | deep | claude-opus-5-5 | 12 | 37697 | 140593 | 5359 | 0.3238 | claude |
| 2026-11-17 14:25 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 3122 | 187788 | 265 | 0.0678 | claude |
| 2026-11-17 14:26 | Octopus | qa-reviewer | r1 | balanced | claude-opus-5-5 | 18 | 48822 | 272701 | 14154 | 0.5818 | claude |

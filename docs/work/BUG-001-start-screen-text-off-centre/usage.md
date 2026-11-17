# Token usage: BUG-001-start-screen-text-off-centre

Every agent run on this work item: the model it used, its tokens, and the estimated
API cost in USD at the prices in the DevAgents config. Rows are added automatically.
The PM refreshes the totals when the item is committed (`node .devagents/devagents.mjs usage BUG-001-start-screen-text-off-centre --write`).

<!-- devagents:usage-totals -->
**Total: $4.76** · 29 runs · 9.95M input (incl. cache) · 111.4k output

| Agent | Role | Model | Runs | Input | Cache write | Cache read | Output | Cost |
|---|---|---|---|---|---|---|---|---|
| Nishikado | pm | claude-opus-5-5 | 15 | 67 | 63.3k | 6.13M | 17.5k | $2.08 |
| Crab | plan-reviewer | claude-opus-5-5 | 2 | 50 | 87.0k | 800.5k | 21.5k | $1.03 |
| Octopus | qa-reviewer | claude-opus-5-5 | 2 | 34 | 44.6k | 446.0k | 12.6k | $0.56 |
| Saucer | code-reviewer | claude-opus-5-5 | 2 | 28 | 40.2k | 342.6k | 6.2k | $0.39 |
| Mothership | planner | claude-sonnet-5-5 | 2 | 26 | 65.1k | 279.9k | 10.8k | $0.30 |
| Bunker | security-reviewer | claude-opus-5-5 | 2 | 14 | 20.6k | 113.6k | 3.1k | $0.19 |
| Squid | tester | claude-sonnet-5-5 | 2 | 30 | 33.3k | 322.3k | 5.1k | $0.17 |
| Cannon | implementer | claude-haiku-5-5 | 2 | 64 | 90.8k | 1.06M | 34.7k | $0.04 |
<!-- /devagents:usage-totals -->

## Runs

| Time | Agent | Role | Run | Tier | Model | Input | Cache write | Cache read | Output | Cost USD | Source |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-17 20:25 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 3 | 12549 | 138322 | 1163 | 0.1513 | claude |
| 2026-11-17 20:26 | Mothership | planner | r1 | fast | claude-sonnet-5-5 | 20 | 40133 | 240892 | 7986 | 0.2043 | claude |
| 2026-11-17 20:26 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 3451 | 282983 | 865 | 0.1015 | claude |
| 2026-11-17 20:30 | Crab | plan-reviewer | r1 | balanced | claude-opus-5-5 | 40 | 60083 | 714331 | 18167 | 0.8068 | claude |
| 2026-11-17 21:13 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 2 | 2205 | 131530 | 1115 | 0.0662 | claude |
| 2026-11-17 21:13 | Mothership | planner | r2 | fast | claude-sonnet-5-5 | 6 | 24986 | 38973 | 2862 | 0.0950 | claude |
| 2026-11-17 21:13 | Nishikado | pm | turn |  | claude-opus-5-5 | 8 | 3774 | 537562 | 1106 | 0.1599 | claude |
| 2026-11-17 21:14 | Crab | plan-reviewer | r2 | balanced | claude-opus-5-5 | 10 | 26943 | 86158 | 3322 | 0.2184 | claude |
| 2026-11-17 21:28 | Nishikado | pm | turn (1/3) |  | claude-opus-5-5 | 5 | 4374 | 333614 | 1971 | 0.1412 | claude |
| 2026-11-17 21:34 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 4 | 3481 | 363889 | 1548 | 0.1316 | claude |
| 2026-11-17 21:36 | Cannon | implementer | WP1 r1 | light | claude-haiku-5-5 | 32 | 57913 | 556931 | 22481 | 0.0241 | claude |
| 2026-11-17 21:36 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 3580 | 379515 | 708 | 0.1187 | claude |
| 2026-11-17 21:37 | Squid | tester | r1 | fast | claude-sonnet-5-5 | 18 | 17273 | 198575 | 3160 | 0.0947 | claude |
| 2026-11-17 21:38 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 5330 | 603067 | 1831 | 0.1999 | claude |
| 2026-11-17 21:38 | Bunker | security-reviewer | r1 | balanced | claude-opus-5-5 | 8 | 11574 | 66927 | 1713 | 0.1055 | claude |
| 2026-11-17 21:38 | Nishikado | pm | turn |  | claude-opus-5-5 | 2 | 1382 | 205392 | 176 | 0.0557 | claude |
| 2026-11-17 21:39 | Saucer | code-reviewer | r1 | balanced | claude-opus-5-5 | 18 | 26563 | 245205 | 4148 | 0.2649 | claude |
| 2026-11-17 21:39 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 4 | 4882 | 420640 | 907 | 0.1413 | claude |
| 2026-11-17 21:39 | Octopus | qa-reviewer | r1 | balanced | claude-opus-5-5 | 16 | 23110 | 209866 | 7505 | 0.3077 | claude |
| 2026-11-17 21:40 | Nishikado | pm | turn |  | claude-opus-5-5 | 2 | 3058 | 219316 | 1169 | 0.0917 | claude |
| 2026-11-17 21:41 | Cannon | implementer | WP1 r2 | light | claude-haiku-5-5 | 32 | 32901 | 506705 | 12224 | 0.0153 | claude |
| 2026-11-17 21:41 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 5168 | 687769 | 1100 | 0.2009 | claude |
| 2026-11-17 21:42 | Squid | tester | r2 | fast | claude-sonnet-5-5 | 12 | 16043 | 123698 | 1932 | 0.0718 | claude |
| 2026-11-17 21:42 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 1936 | 471647 | 868 | 0.1272 | claude |
| 2026-11-17 21:43 | Octopus | qa-reviewer | r2 | balanced | claude-opus-5-5 | 18 | 21478 | 236097 | 5075 | 0.2562 | claude |
| 2026-11-17 21:44 | Nishikado | pm | turn |  | claude-opus-5-5 | 8 | 5345 | 980044 | 1671 | 0.2722 | claude |
| 2026-11-17 21:44 | Saucer | code-reviewer | r2 | balanced | claude-opus-5-5 | 10 | 13615 | 97357 | 2054 | 0.1287 | claude |
| 2026-11-17 21:45 | Nishikado | pm | turn (1/4) |  | claude-opus-5-5 | 3 | 2759 | 378689 | 1255 | 0.1229 | claude |
| 2026-11-17 21:45 | Bunker | security-reviewer | r2 | balanced | claude-opus-5-5 | 6 | 9008 | 46686 | 1362 | 0.0816 | claude |

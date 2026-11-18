# Token usage: FEAT-001-mute-control

Every agent run on this work item: the model it used, its tokens, and the estimated
API cost in USD at the prices in the DevAgents config. Rows are added automatically.
The PM refreshes the totals when the item is committed (`node .devagents/devagents.mjs usage FEAT-001-mute-control --write`).

<!-- devagents:usage-totals -->
**Total: $9.22** · 35 runs · 21.30M input (incl. cache) · 131.8k output

| Agent | Role | Model | Runs | Input | Cache write | Cache read | Output | Cost |
|---|---|---|---|---|---|---|---|---|
| Nishikado | pm | claude-opus-5-5 | 18 | 85 | 80.8k | 15.96M | 22.9k | $4.30 |
| Mothership | planner | claude-opus-5-5 | 2 | 42 | 96.8k | 815.5k | 28.4k | $1.22 |
| Crab | plan-reviewer | claude-opus-5-5 | 2 | 40 | 75.7k | 769.1k | 18.1k | $0.89 |
| Octopus | qa-reviewer | claude-opus-5-5 | 2 | 38 | 71.6k | 623.8k | 14.5k | $0.77 |
| Saucer | code-reviewer | claude-opus-5-5 | 2 | 32 | 62.0k | 477.6k | 9.8k | $0.60 |
| Pixel | ux-reviewer | claude-opus-5-5 | 2 | 28 | 52.9k | 367.5k | 7.8k | $0.49 |
| Bunker | security-reviewer | claude-opus-5-5 | 2 | 18 | 43.4k | 162.0k | 3.8k | $0.33 |
| Cannon | implementer | claude-sonnet-5-5 | 1 | 46 | 47.2k | 856.6k | 12.1k | $0.32 |
| Squid | tester | claude-sonnet-5-5 | 3 | 40 | 48.6k | 558.8k | 11.2k | $0.29 |
| Cannon | implementer | claude-haiku-5-5 | 1 | 10 | 32.3k | 103.4k | 3.2k | $0.01 |
<!-- /devagents:usage-totals -->

## Runs

| Time | Agent | Role | Run | Tier | Model | Input | Cache write | Cache read | Output | Cost USD | Source |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-17 15:44 | Nishikado | pm | turn (1/8) |  | claude-opus-5-5 | 3 | 2339 | 176646 | 1915 | 0.0924 | claude |
| 2026-11-17 15:48 | Mothership | planner | r1 | balanced | claude-opus-5-5 | 26 | 70311 | 585275 | 16914 | 0.8070 | claude |
| 2026-11-17 15:48 | Nishikado | pm | turn (1/5) |  | claude-opus-5-5 | 2 | 3289 | 177516 | 932 | 0.0805 | claude |
| 2026-11-17 15:51 | Crab | plan-reviewer | r1 | balanced | claude-opus-5-5 | 30 | 58627 | 666227 | 15181 | 0.7301 | claude |
| 2026-11-17 15:51 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 6854 | 555549 | 1172 | 0.1894 | claude |
| 2026-11-17 15:53 | Mothership | planner | r2 | balanced | claude-opus-5-5 | 16 | 26518 | 230237 | 11510 | 0.4089 | claude |
| 2026-11-17 15:53 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 3106 | 409338 | 759 | 0.1219 | claude |
| 2026-11-17 15:54 | Crab | plan-reviewer | r2 | balanced | claude-opus-5-5 | 10 | 17040 | 102894 | 2915 | 0.1641 | claude |
| 2026-11-17 15:54 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 3368 | 423009 | 1044 | 0.1324 | claude |
| 2026-11-17 20:25 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 3 | 12549 | 138322 | 1163 | 0.1513 | claude |
| 2026-11-17 21:13 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 2 | 2205 | 131530 | 1115 | 0.0662 | claude |
| 2026-11-18 09:01 | Nishikado | pm | turn (1/3) |  | claude-opus-5-5 | 3 | 2045 | 581108 | 1021 | 0.1530 | claude |
| 2026-11-18 09:03 | Cannon | implementer | WP1 r1 | fast | claude-sonnet-5-5 | 46 | 47221 | 856576 | 12060 | 0.3244 | claude |
| 2026-11-18 09:03 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 3440 | 1321008 | 960 | 0.3109 | claude |
| 2026-11-18 09:08 | Squid | tester | r1 | fast | claude-sonnet-5-5 | 30 | 37082 | 468839 | 10184 | 0.2415 | claude |
| 2026-11-18 09:09 | Squid | tester | r1 | fast | claude-sonnet-5-5 | 4 | 2339 | 42692 | 79 | 0.0109 | claude |
| 2026-11-18 09:09 | Nishikado | pm | turn |  | claude-opus-5-5 | 8 | 4672 | 1782020 | 4269 | 0.4792 | claude |
| 2026-11-18 09:10 | Bunker | security-reviewer | r1 | balanced | claude-opus-5-5 | 10 | 33866 | 94089 | 2560 | 0.2394 | claude |
| 2026-11-18 09:11 | Nishikado | pm | turn |  | claude-opus-5-5 | 12 | 10785 | 2725685 | 2966 | 0.6908 | claude |
| 2026-11-18 09:11 | Pixel | ux-reviewer | r1 | balanced | claude-opus-5-5 | 20 | 41050 | 292498 | 6339 | 0.3906 | claude |
| 2026-11-18 09:11 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 2817 | 918846 | 400 | 0.2143 | claude |
| 2026-11-18 09:11 | Saucer | code-reviewer | r1 | balanced | claude-opus-5-5 | 24 | 50934 | 407000 | 8305 | 0.5023 | claude |
| 2026-11-18 09:11 | Nishikado | pm | turn |  | claude-opus-5-5 | 2 | 2000 | 461214 | 197 | 0.1122 | claude |
| 2026-11-18 09:12 | Octopus | qa-reviewer | r1 | balanced | claude-opus-5-5 | 26 | 55578 | 496909 | 11891 | 0.6152 | claude |
| 2026-11-18 09:12 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 4254 | 927173 | 798 | 0.2354 | claude |
| 2026-11-18 09:13 | Cannon | implementer | WP1 r2 | light | claude-haiku-5-5 | 10 | 32335 | 103361 | 3205 | 0.0067 | claude |
| 2026-11-18 09:13 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 2887 | 936657 | 744 | 0.2253 | claude |
| 2026-11-18 09:13 | Squid | tester | r2 | fast | claude-sonnet-5-5 | 6 | 9212 | 47236 | 918 | 0.0369 | claude |
| 2026-11-18 09:13 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 4211 | 1415273 | 2150 | 0.3598 | claude |
| 2026-11-18 09:14 | Saucer | code-reviewer | r2 | balanced | claude-opus-5-5 | 8 | 11038 | 70573 | 1505 | 0.0994 | claude |
| 2026-11-18 09:14 | Pixel | ux-reviewer | r2 | balanced | claude-opus-5-5 | 8 | 11891 | 74954 | 1438 | 0.1032 | claude |
| 2026-11-18 09:14 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 6407 | 1429976 | 472 | 0.3467 | claude |
| 2026-11-18 09:14 | Octopus | qa-reviewer | r2 | balanced | claude-opus-5-5 | 12 | 15977 | 126910 | 2635 | 0.1580 | claude |
| 2026-11-18 09:14 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 3612 | 1445113 | 805 | 0.3340 | claude |
| 2026-11-18 09:15 | Bunker | security-reviewer | r2 | balanced | claude-opus-5-5 | 8 | 9527 | 67916 | 1263 | 0.0865 | claude |

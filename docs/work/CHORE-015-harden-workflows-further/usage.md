# Token usage: CHORE-015-harden-workflows-further

Every agent run on this work item: the model it used, its tokens, and the estimated
API cost in USD at the prices in the DevAgents config. Rows are added automatically.
The PM refreshes the totals when the item is committed (`node .devagents/devagents.mjs usage CHORE-015-harden-workflows-further --write`).

<!-- devagents:usage-totals -->
**Total: $8.21** · 26 runs · 12.05M input (incl. cache) · 137.5k output

| Agent | Role | Model | Runs | Input | Cache write | Cache read | Output | Cost |
|---|---|---|---|---|---|---|---|---|
| Nishikado | pm | claude-opus-5-5 | 13 | 53 | 221.4k | 6.85M | 17.5k | $3.49 |
| Mothership | planner | claude-opus-5-5 | 1 | 40 | 71.0k | 939.8k | 27.5k | $1.09 |
| Octopus | qa-reviewer | claude-opus-5-5 | 2 | 40 | 87.1k | 631.7k | 19.3k | $0.95 |
| Bunker | security-reviewer | claude-opus-5-5 | 2 | 38 | 78.3k | 780.0k | 14.5k | $0.84 |
| Saucer | code-reviewer | claude-opus-5-5 | 2 | 28 | 75.1k | 430.2k | 10.1k | $0.66 |
| Crab | plan-reviewer | claude-opus-5-5 | 1 | 24 | 48.1k | 448.6k | 15.2k | $0.63 |
| Squid | tester | claude-sonnet-5-5 | 2 | 26 | 62.6k | 327.9k | 7.2k | $0.26 |
| Cannon | implementer | claude-sonnet-5-5 | 1 | 24 | 40.2k | 379.5k | 11.7k | $0.26 |
| Cannon | implementer | claude-haiku-5-5 | 2 | 42 | 54.0k | 516.0k | 14.4k | $0.02 |
<!-- /devagents:usage-totals -->

## Runs

| Time | Agent | Role | Run | Tier | Model | Input | Cache write | Cache read | Output | Cost USD | Source |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-17 15:44 | Nishikado | pm | turn (1/8) |  | claude-opus-5-5 | 3 | 2339 | 176646 | 1915 | 0.0924 | claude |
| 2026-11-17 15:50 | Mothership | planner | r1 | balanced | claude-opus-5-5 | 40 | 71004 | 939833 | 27534 | 1.0938 | claude |
| 2026-11-17 15:51 | Nishikado | pm | turn (1/4) |  | claude-opus-5-5 | 2 | 2902 | 173908 | 685 | 0.0717 | claude |
| 2026-11-17 15:54 | Crab | plan-reviewer | r1 | balanced | claude-opus-5-5 | 24 | 48079 | 448571 | 15182 | 0.6338 | claude |
| 2026-11-17 15:55 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 4852 | 431176 | 1275 | 0.1506 | claude |
| 2026-11-17 20:25 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 3 | 12549 | 138322 | 1163 | 0.1513 | claude |
| 2026-11-17 21:13 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 2 | 2205 | 131530 | 1115 | 0.0662 | claude |
| 2026-11-17 21:45 | Nishikado | pm | turn (1/4) |  | claude-opus-5-5 | 3 | 2759 | 378689 | 1255 | 0.1229 | claude |
| 2026-11-17 21:47 | Cannon | implementer | WP1 r1 | fast | claude-sonnet-5-5 | 24 | 40223 | 379526 | 11667 | 0.2552 | claude |
| 2026-11-17 21:49 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 5 | 4719 | 679871 | 1855 | 0.2108 | claude |
| 2026-11-17 21:50 | Cannon | implementer | WP1 r1 | light | claude-haiku-5-5 | 26 | 26341 | 365094 | 9723 | 0.0118 | claude |
| 2026-11-17 21:50 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 4 | 6287 | 567822 | 774 | 0.1793 | claude |
| 2026-11-17 21:51 | Squid | tester | r1 | fast | claude-sonnet-5-5 | 12 | 25138 | 157188 | 3108 | 0.1097 | claude |
| 2026-11-17 21:53 | Saucer | code-reviewer | r1 | balanced | claude-opus-5-5 | 20 | 47913 | 367900 | 7995 | 0.4731 | claude |
| 2026-11-17 21:54 | Bunker | security-reviewer | r1 | deep | claude-opus-5-5 | 30 | 55463 | 711781 | 12198 | 0.6638 | claude |
| 2026-11-17 21:54 | Octopus | qa-reviewer | r1 | balanced | claude-opus-5-5 | 22 | 46898 | 404619 | 13737 | 0.5902 | claude |
| 2026-11-18 07:37 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 5 | 163496 | 588360 | 3085 | 1.4873 | claude |
| 2026-11-18 07:37 | Cannon | implementer | WP1 r2 | light | claude-haiku-5-5 | 16 | 27617 | 150874 | 4715 | 0.0073 | claude |
| 2026-11-18 07:38 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 6681 | 955408 | 1254 | 0.2696 | claude |
| 2026-11-18 07:39 | Squid | tester | r2 | fast | claude-sonnet-5-5 | 14 | 37499 | 170731 | 4136 | 0.1522 | claude |
| 2026-11-18 07:39 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 4357 | 976126 | 1505 | 0.2602 | claude |
| 2026-11-18 07:39 | Saucer | code-reviewer | r2 | balanced | claude-opus-5-5 | 8 | 27212 | 62273 | 2070 | 0.1899 | claude |
| 2026-11-18 07:39 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 3468 | 657962 | 389 | 0.1671 | claude |
| 2026-11-18 07:40 | Octopus | qa-reviewer | r2 | balanced | claude-opus-5-5 | 18 | 40225 | 227124 | 5597 | 0.3586 | claude |
| 2026-11-18 07:41 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 4801 | 998907 | 1215 | 0.2625 | claude |
| 2026-11-18 07:41 | Bunker | security-reviewer | r2 | deep | claude-opus-5-5 | 8 | 22887 | 68169 | 2331 | 0.1747 | claude |

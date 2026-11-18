# Token usage: CHORE-013-rate-limit-and-score-plausibility

Every agent run on this work item: the model it used, its tokens, and the estimated
API cost in USD at the prices in the DevAgents config. Rows are added automatically.
The PM refreshes the totals when the item is committed (`node .devagents/devagents.mjs usage CHORE-013-rate-limit-and-score-plausibility --write`).

<!-- devagents:usage-totals -->
**Total: $12.44** · 36 runs · 23.09M input (incl. cache) · 241.3k output

| Agent | Role | Model | Runs | Input | Cache write | Cache read | Output | Cost |
|---|---|---|---|---|---|---|---|---|
| Nishikado | pm | claude-opus-5-5 | 20 | 84 | 89.7k | 13.84M | 24.7k | $3.98 |
| Mothership | planner | claude-opus-5-5 | 3 | 86 | 194.5k | 1.99M | 63.9k | $2.65 |
| Crab | plan-reviewer | claude-opus-5-5 | 3 | 62 | 129.4k | 1.24M | 39.3k | $1.68 |
| Octopus | qa-reviewer | claude-opus-5-5 | 2 | 56 | 106.2k | 1.34M | 33.0k | $1.46 |
| Saucer | code-reviewer | claude-opus-5-5 | 2 | 38 | 105.0k | 742.6k | 15.4k | $0.98 |
| Bunker | security-reviewer | claude-opus-5-5 | 2 | 24 | 78.4k | 372.1k | 11.3k | $0.69 |
| Cannon | implementer | claude-sonnet-5-5 | 1 | 64 | 82.9k | 1.76M | 29.3k | $0.68 |
| Squid | tester | claude-sonnet-5-5 | 2 | 32 | 63.0k | 419.7k | 10.0k | $0.30 |
| Cannon | implementer | claude-haiku-5-5 | 1 | 20 | 67.5k | 464.5k | 14.5k | $0.02 |
<!-- /devagents:usage-totals -->

## Runs

| Time | Agent | Role | Run | Tier | Model | Input | Cache write | Cache read | Output | Cost USD | Source |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-17 15:44 | Nishikado | pm | turn (1/8) |  | claude-opus-5-5 | 3 | 2339 | 176646 | 1915 | 0.0924 | claude |
| 2026-11-17 15:52 | Mothership | planner | r1 | balanced | claude-opus-5-5 | 32 | 104037 | 910804 | 35989 | 1.4223 | claude |
| 2026-11-17 15:52 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 4699 | 586393 | 1964 | 0.1942 | claude |
| 2026-11-17 15:58 | Crab | plan-reviewer | r1 | balanced | claude-opus-5-5 | 40 | 73204 | 975663 | 28524 | 1.1318 | claude |
| 2026-11-17 15:58 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 5321 | 471628 | 935 | 0.1556 | claude |
| 2026-11-17 16:00 | Mothership | planner | r2 | balanced | claude-opus-5-5 | 18 | 28419 | 265247 | 12679 | 0.4488 | claude |
| 2026-11-17 16:00 | Nishikado | pm | turn |  | claude-opus-5-5 | 2 | 1404 | 245323 | 638 | 0.0731 | claude |
| 2026-11-17 16:01 | Crab | plan-reviewer | r2 | balanced | claude-opus-5-5 | 8 | 20284 | 85769 | 4522 | 0.2090 | claude |
| 2026-11-17 16:02 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 5156 | 745951 | 1769 | 0.2258 | claude |
| 2026-11-17 20:25 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 3 | 12549 | 138322 | 1163 | 0.1513 | claude |
| 2026-11-17 21:13 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 2 | 2205 | 131530 | 1115 | 0.0662 | claude |
| 2026-11-17 21:45 | Nishikado | pm | turn (1/4) |  | claude-opus-5-5 | 3 | 2759 | 378689 | 1255 | 0.1229 | claude |
| 2026-11-18 07:42 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 4 | 2579 | 676608 | 1381 | 0.1836 | claude |
| 2026-11-18 07:45 | Mothership | planner | r3 | balanced | claude-opus-5-5 | 36 | 62004 | 814617 | 15213 | 0.7773 | claude |
| 2026-11-18 07:46 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 4572 | 695514 | 1147 | 0.1986 | claude |
| 2026-11-18 07:47 | Crab | plan-reviewer | r3 | balanced | claude-opus-5-5 | 14 | 35936 | 181666 | 6276 | 0.3416 | claude |
| 2026-11-18 08:42 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 6 | 6108 | 1088070 | 2008 | 0.3067 | claude |
| 2026-11-18 08:48 | Cannon | implementer | WP1 r1 | fast | claude-sonnet-5-5 | 64 | 82861 | 1760818 | 29250 | 0.6759 | claude |
| 2026-11-18 08:48 | Nishikado | pm | turn (1/3) |  | claude-opus-5-5 | 3 | 3112 | 651802 | 973 | 0.1747 | claude |
| 2026-11-18 08:49 | Squid | tester | r1 | fast | claude-sonnet-5-5 | 16 | 21943 | 203384 | 3576 | 0.1110 | claude |
| 2026-11-18 08:50 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 4045 | 1194724 | 2722 | 0.3258 | claude |
| 2026-11-18 08:52 | Bunker | security-reviewer | r1 | deep | claude-opus-5-5 | 16 | 49045 | 297311 | 9189 | 0.4885 | claude |
| 2026-11-18 08:52 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 5904 | 1208881 | 585 | 0.3007 | claude |
| 2026-11-18 08:52 | Saucer | code-reviewer | r1 | balanced | claude-opus-5-5 | 22 | 69403 | 536790 | 11988 | 0.6942 | claude |
| 2026-11-18 08:52 | Nishikado | pm | turn |  | claude-opus-5-5 | 2 | 2169 | 406370 | 205 | 0.1027 | claude |
| 2026-11-18 08:55 | Octopus | qa-reviewer | r1 | balanced | claude-opus-5-5 | 40 | 70337 | 1096906 | 24823 | 1.0677 | claude |
| 2026-11-18 08:55 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 6566 | 817847 | 1236 | 0.2408 | claude |
| 2026-11-18 08:56 | Cannon | implementer | WP1 r2 | light | claude-haiku-5-5 | 20 | 67519 | 464548 | 14457 | 0.0203 | claude |
| 2026-11-18 08:57 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 3534 | 832324 | 998 | 0.2147 | claude |
| 2026-11-18 08:58 | Squid | tester | r2 | fast | claude-sonnet-5-5 | 16 | 41098 | 216279 | 6450 | 0.1889 | claude |
| 2026-11-18 08:58 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 4436 | 1260857 | 1478 | 0.3172 | claude |
| 2026-11-18 08:59 | Saucer | code-reviewer | r2 | balanced | claude-opus-5-5 | 16 | 35647 | 205843 | 3391 | 0.2873 | claude |
| 2026-11-18 08:59 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 4459 | 1274781 | 462 | 0.2999 | claude |
| 2026-11-18 09:00 | Octopus | qa-reviewer | r2 | balanced | claude-opus-5-5 | 16 | 35814 | 239766 | 8155 | 0.3902 | claude |
| 2026-11-18 09:00 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 5831 | 859180 | 771 | 0.2339 | claude |
| 2026-11-18 09:01 | Bunker | security-reviewer | r2 | deep | claude-opus-5-5 | 8 | 29306 | 74803 | 2076 | 0.2030 | claude |

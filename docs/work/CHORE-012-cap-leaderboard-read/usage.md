# Token usage: CHORE-012-cap-leaderboard-read

Every agent run on this work item: the model it used, its tokens, and the estimated
API cost in USD at the prices in the DevAgents config. Rows are added automatically.
The PM refreshes the totals when the item is committed (`node .devagents/devagents.mjs usage CHORE-012-cap-leaderboard-read --write`).

<!-- devagents:usage-totals -->
**Total: $9.40** · 35 runs · 12.61M input (incl. cache) · 207.8k output

| Agent | Role | Model | Runs | Input | Cache write | Cache read | Output | Cost |
|---|---|---|---|---|---|---|---|---|
| Mothership | planner | claude-opus-5-5 | 3 | 76 | 150.9k | 1.43M | 76.3k | $2.57 |
| Nishikado | pm | claude-opus-5-5 | 19 | 76 | 78.0k | 7.56M | 18.6k | $2.51 |
| Crab | plan-reviewer | claude-opus-5-5 | 3 | 44 | 86.1k | 584.1k | 31.1k | $1.17 |
| Octopus | qa-reviewer | claude-opus-5-5 | 2 | 44 | 82.2k | 871.0k | 23.9k | $1.06 |
| Saucer | code-reviewer | claude-opus-5-5 | 2 | 26 | 81.6k | 362.5k | 13.8k | $0.76 |
| Bunker | security-reviewer | claude-opus-5-5 | 2 | 32 | 67.8k | 407.7k | 11.8k | $0.66 |
| Cannon | implementer | claude-sonnet-5-5 | 2 | 32 | 74.4k | 489.4k | 26.2k | $0.50 |
| Squid | tester | claude-sonnet-5-5 | 2 | 22 | 38.2k | 240.6k | 6.2k | $0.18 |
<!-- /devagents:usage-totals -->

## Runs

| Time | Agent | Role | Run | Tier | Model | Input | Cache write | Cache read | Output | Cost USD | Source |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-17 15:44 | Nishikado | pm | turn (1/8) |  | claude-opus-5-5 | 3 | 2339 | 176646 | 1915 | 0.0924 | claude |
| 2026-11-17 15:49 | Mothership | planner | r1 | balanced | claude-opus-5-5 | 26 | 58344 | 434975 | 25560 | 0.8900 | claude |
| 2026-11-17 15:49 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 3206 | 322588 | 1013 | 0.1104 | claude |
| 2026-11-17 15:52 | Crab | plan-reviewer | r1 | balanced | claude-opus-5-5 | 22 | 35463 | 299484 | 15512 | 0.5475 | claude |
| 2026-11-17 15:52 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 4891 | 401250 | 833 | 0.1361 | claude |
| 2026-11-17 15:55 | Mothership | planner | r2 | balanced | claude-opus-5-5 | 20 | 40497 | 352596 | 17959 | 0.6323 | claude |
| 2026-11-17 15:56 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 2 | 3295 | 220228 | 652 | 0.0834 | claude |
| 2026-11-17 15:58 | Crab | plan-reviewer | r2 | balanced | claude-opus-5-5 | 16 | 31448 | 230797 | 10641 | 0.4163 | claude |
| 2026-11-17 15:58 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 4798 | 462046 | 986 | 0.1505 | claude |
| 2026-11-17 16:03 | Mothership | planner | r3 | deep | claude-opus-5-5 | 30 | 52042 | 645978 | 32767 | 1.0449 | claude |
| 2026-11-17 16:03 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 4034 | 515530 | 894 | 0.1533 | claude |
| 2026-11-17 16:04 | Crab | plan-reviewer | r3 | balanced | claude-opus-5-5 | 6 | 19148 | 53861 | 4936 | 0.2053 | claude |
| 2026-11-17 16:04 | Nishikado | pm | turn |  | claude-opus-5-5 | 2 | 2766 | 260598 | 291 | 0.0801 | claude |
| 2026-11-17 20:25 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 3 | 12549 | 138322 | 1163 | 0.1513 | claude |
| 2026-11-17 21:13 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 2 | 2205 | 131530 | 1115 | 0.0662 | claude |
| 2026-11-17 21:30 | Nishikado | pm | turn (1/3) |  | claude-opus-5-5 | 4 | 2926 | 340253 | 1095 | 0.1134 | claude |
| 2026-11-17 21:33 | Cannon | implementer | WP1 r1 | fast | claude-sonnet-5-5 | 22 | 49665 | 390313 | 22978 | 0.3930 | claude |
| 2026-11-17 21:33 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 2791 | 351990 | 799 | 0.1087 | claude |
| 2026-11-17 21:35 | Squid | tester | r1 | fast | claude-sonnet-5-5 | 12 | 22662 | 138397 | 4410 | 0.1146 | claude |
| 2026-11-17 21:35 | Nishikado | pm | turn |  | claude-opus-5-5 | 8 | 3415 | 746635 | 1973 | 0.2161 | claude |
| 2026-11-17 21:37 | Bunker | security-reviewer | r1 | deep | claude-opus-5-5 | 16 | 50377 | 225774 | 7551 | 0.4481 | claude |
| 2026-11-17 21:37 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 4983 | 582295 | 558 | 0.1675 | claude |
| 2026-11-17 21:37 | Saucer | code-reviewer | r1 | balanced | claude-opus-5-5 | 14 | 61138 | 223925 | 9918 | 0.5489 | claude |
| 2026-11-17 21:37 | Nishikado | pm | turn |  | claude-opus-5-5 | 2 | 2892 | 197170 | 338 | 0.0693 | claude |
| 2026-11-17 21:38 | Octopus | qa-reviewer | r1 | balanced | claude-opus-5-5 | 26 | 51028 | 592610 | 14752 | 0.6688 | claude |
| 2026-11-17 21:39 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 4 | 4882 | 420640 | 907 | 0.1413 | claude |
| 2026-11-17 21:39 | Cannon | implementer | WP1 r2 | fast | claude-sonnet-5-5 | 10 | 24709 | 99110 | 3182 | 0.1035 | claude |
| 2026-11-17 21:39 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 2779 | 434239 | 717 | 0.1234 | claude |
| 2026-11-17 21:40 | Squid | tester | r2 | fast | claude-sonnet-5-5 | 10 | 15573 | 102168 | 1750 | 0.0667 | claude |
| 2026-11-17 21:41 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 5109 | 672756 | 2045 | 0.2163 | claude |
| 2026-11-17 21:41 | Saucer | code-reviewer | r2 | balanced | claude-opus-5-5 | 12 | 20506 | 138588 | 3895 | 0.2082 | claude |
| 2026-11-17 21:42 | Nishikado | pm | turn |  | claude-opus-5-5 | 4 | 2724 | 467232 | 340 | 0.1221 | claude |
| 2026-11-17 21:43 | Octopus | qa-reviewer | r2 | balanced | claude-opus-5-5 | 18 | 31135 | 278363 | 9172 | 0.3949 | claude |
| 2026-11-17 21:43 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 5407 | 717383 | 960 | 0.2060 | claude |
| 2026-11-17 21:44 | Bunker | security-reviewer | r2 | deep | claude-opus-5-5 | 16 | 17437 | 181972 | 4214 | 0.2079 | claude |

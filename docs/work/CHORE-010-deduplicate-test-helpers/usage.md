# Token usage: CHORE-010-deduplicate-test-helpers

Every agent run on this work item: the model it used, its tokens, and the estimated
API cost in USD at the prices in the DevAgents config. Rows are added automatically.
The PM refreshes the totals when the item is committed (`node .devagents/devagents.mjs usage CHORE-010-deduplicate-test-helpers --write`).

<!-- devagents:usage-totals -->
**Total: $2.59** · 16 runs · 4.59M input (incl. cache) · 65.3k output

| Agent | Role | Model | Runs | Input | Cache write | Cache read | Output | Cost |
|---|---|---|---|---|---|---|---|---|
| Nishikado | pm | claude-opus-5-5 | 9 | 33 | 42.9k | 2.37M | 11.1k | $1.04 |
| Octopus | qa-reviewer | claude-opus-5-5 | 1 | 32 | 39.3k | 593.9k | 17.4k | $0.66 |
| Crab | plan-reviewer | claude-opus-5-5 | 1 | 22 | 24.5k | 254.3k | 8.9k | $0.35 |
| Saucer | code-reviewer | claude-opus-5-5 | 1 | 14 | 20.1k | 164.4k | 3.3k | $0.20 |
| Bunker | security-reviewer | claude-opus-5-5 | 1 | 8 | 13.3k | 67.7k | 1.8k | $0.12 |
| Mothership | planner | claude-sonnet-5-5 | 1 | 14 | 26.8k | 126.8k | 3.5k | $0.12 |
| Squid | tester | claude-sonnet-5-5 | 1 | 8 | 23.1k | 57.5k | 1.7k | $0.08 |
| Cannon | implementer | claude-haiku-5-5 | 1 | 32 | 67.8k | 700.4k | 17.6k | $0.02 |
<!-- /devagents:usage-totals -->

## Runs

| Time | Agent | Role | Run | Tier | Model | Input | Cache write | Cache read | Output | Cost USD | Source |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-17 15:44 | Nishikado | pm | turn (1/8) |  | claude-opus-5-5 | 3 | 2339 | 176646 | 1915 | 0.0924 | claude |
| 2026-11-17 15:45 | Mothership | planner | r1 | fast | claude-sonnet-5-5 | 14 | 26774 | 126849 | 3535 | 0.1150 | claude |
| 2026-11-17 15:45 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 4 | 5800 | 269342 | 987 | 0.1200 | claude |
| 2026-11-17 15:47 | Crab | plan-reviewer | r1 | balanced | claude-opus-5-5 | 22 | 24467 | 254326 | 8875 | 0.3508 | claude |
| 2026-11-17 15:48 | Nishikado | pm | turn (1/5) |  | claude-opus-5-5 | 2 | 3289 | 177516 | 932 | 0.0805 | claude |
| 2026-11-17 20:25 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 3 | 12549 | 138322 | 1163 | 0.1513 | claude |
| 2026-11-17 21:13 | Nishikado | pm | turn (1/11) |  | claude-opus-5-5 | 2 | 2205 | 131530 | 1115 | 0.0662 | claude |
| 2026-11-17 21:15 | Cannon | implementer | WP1 r1 | light | claude-haiku-5-5 | 32 | 67815 | 700396 | 17625 | 0.0243 | claude |
| 2026-11-17 21:28 | Nishikado | pm | turn (1/3) |  | claude-opus-5-5 | 5 | 4374 | 333614 | 1971 | 0.1412 | claude |
| 2026-11-17 21:29 | Squid | tester | r1 | fast | claude-sonnet-5-5 | 8 | 23088 | 57547 | 1686 | 0.0804 | claude |
| 2026-11-17 21:29 | Nishikado | pm | turn (1/2) |  | claude-opus-5-5 | 4 | 6960 | 309989 | 1499 | 0.1477 | claude |
| 2026-11-17 21:30 | Bunker | security-reviewer | r1 | balanced | claude-opus-5-5 | 8 | 13335 | 67659 | 1767 | 0.1156 | claude |
| 2026-11-17 21:30 | Nishikado | pm | turn |  | claude-opus-5-5 | 6 | 2463 | 494515 | 419 | 0.1270 | claude |
| 2026-11-17 21:30 | Saucer | code-reviewer | r1 | balanced | claude-opus-5-5 | 14 | 20080 | 164391 | 3288 | 0.1991 | claude |
| 2026-11-17 21:30 | Nishikado | pm | turn (1/3) |  | claude-opus-5-5 | 4 | 2926 | 340253 | 1095 | 0.1134 | claude |
| 2026-11-17 21:33 | Octopus | qa-reviewer | r1 | balanced | claude-opus-5-5 | 32 | 39265 | 593869 | 17410 | 0.6634 | claude |

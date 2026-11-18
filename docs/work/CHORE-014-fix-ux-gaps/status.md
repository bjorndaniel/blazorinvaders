---
id: CHORE-014
name: Fix the five UX gaps
key: CHORE-014-fix-ux-gaps
type: CHORE
phase: done  # intake | planning | plan-review | awaiting-approval | implementing | testing | review | done | needs-input | blocked | parked | cancelled | escalated
size: S  # S | M | L (PM estimate at intake; the plan's value once written)
security_sensitive: no
ui_facing: yes  # yes: the item gets a UX review
plan_round: 2
impl_round: 3
worktree: .work/CHORE-014-fix-ux-gaps
branch: devagents/CHORE-014-fix-ux-gaps
base_branch: devagents/BUG-001-start-screen-text-off-centre
base_commit: d2bf3333e5df737ca162196661cd535e26adb532
blocked_by: none
---

# Status: CHORE-014 Fix the five UX gaps

## Work packages
| WP | Depends on | State |
|----|------------|-------|
| WP1 | — | done |

## Decisions
- D1 (2026-11-17): approval gate → approved ("do all of them")
- D2 (2026-11-18): Pixel r1 note: the 2px white focus ring on the playfield likely shows on load (script focus) and stays during touch play on phones; OK? → "Hide on touch play": show the ring for keyboard users, but not when touch controls are active

## Accepted risks
none

## Log
- 2026-11-17 15:42 Nishikado (PM): created (intake)
- 2026-11-17 15:44 Nishikado (PM): intake confirmed by user ("OK as proposed")
- 2026-11-17 15:44 Mothership (planner) r1 balanced/opus: dispatched
- 2026-11-17 15:47 Mothership (planner) r1 balanced/opus: PLAN WRITTEN (S, UI-facing, WP1)
- 2026-11-17 15:47 Crab (plan-reviewer) r1 balanced/opus: dispatched
- 2026-11-17 15:50 Crab (plan-reviewer) r1 balanced/opus: CHANGES_REQUESTED (P1 major; P2, P3 minor)
- 2026-11-17 15:50 Mothership (planner) r2 balanced/opus: dispatched
- 2026-11-17 15:51 Mothership (planner) r2 balanced/opus: PLAN WRITTEN (P1-P3 accepted)
- 2026-11-17 15:51 Crab (plan-reviewer) r2 balanced/opus: dispatched
- 2026-11-17 15:51 Crab (plan-reviewer) r2 balanced/opus: APPROVED (P1-P3 resolved)
- 2026-11-17 20:24 Nishikado (PM): approved (D1); blocked by CHORE-009 Enforce formatting (shares files); stack on its branch when done
- 2026-11-17 21:13 Nishikado (PM): stacked on devagents/CHORE-009 (886d225); waits for CHORE-008/CHORE-010 (shared docs and test files), then stacks on them
- 2026-11-17 21:46 Nishikado (PM): stacked on devagents/BUG-001 (36abd93) + merged devagents/CHORE-012 (CHORE-008 ux-guidelines); base d2bf333; no shared files with CHORE-015
- 2026-11-17 21:46 Cannon (implementer WP1) r1 light/haiku: dispatched
- 2026-11-17 21:49 Cannon (implementer WP1) r1 light/haiku: IMPLEMENTED (7 files, 263 tests)
- 2026-11-17 21:49 Squid (tester) r1 fast/sonnet: dispatched
- 2026-11-17 21:49 Squid (tester) r1 fast/sonnet: PASS (6/6 ACs; AC1-AC4 by reading, browser check left to QA/UX; 263 tests; 2/2 breaks)
- 2026-11-17 21:49 Octopus (qa-reviewer), Saucer (code-reviewer), Bunker (security-reviewer), Pixel (ux-reviewer) r1 balanced/opus: dispatched
- 2026-11-17 21:50 Bunker (security-reviewer) r1 balanced/opus: APPROVED (no findings)
- 2026-11-17 21:50 Saucer (code-reviewer) r1 balanced/opus: APPROVED (C1 minor: overlapping same-tab loads can show the failed state; not a regression)
- 2026-11-17 21:51 Pixel (ux-reviewer) r1 balanced/opus: APPROVED (U1 minor: focus lost after Try again; U2 nit; no browser: Playwright tools unavailable; flags white focus ring likely visible on load and on phones during play)
- 2026-11-17 21:51 Octopus (qa-reviewer) r1 balanced/opus: CHANGES_REQUESTED (Q1 major: manual browser checks AC1-AC4 not run; 3/3 breaks caught); tracker size corrected to S
- 2026-11-17 21:51 Nishikado (PM): asked user about the focus ring on phones (Pixel note, AC2)
- 2026-11-18 07:37 Nishikado (PM): D2 recorded; round 2: Cannon for D2, U1, C1, U2; then Squid runs the manual browser checks (Q1)
- 2026-11-18 07:37 Cannon (implementer WP1) r2 light/haiku: dispatched
- 2026-11-18 07:38 Cannon (implementer WP1) r2 light/haiku: IMPLEMENTED (D2 via pointer: coarse; U1 focus to selected tab; U2; C1 request counter; 263 tests)
- 2026-11-18 07:38 Squid (tester) r2 fast/sonnet: dispatched (Q1 browser checks AC1-AC4 incl. D2 touch emulation)
- 2026-11-18 07:44 Squid (tester) r2 fast/sonnet: PASS (6/6 ACs; headless Chrome checks AC1-AC4 incl. D2 touch; real pinch not reproducible; 263 tests)
- 2026-11-18 07:45 Octopus (qa-reviewer), Saucer (code-reviewer), Pixel (ux-reviewer) r2 balanced/opus: dispatched; Bunker later
- 2026-11-18 07:46 Pixel (ux-reviewer) r2 balanced/opus: APPROVED (U1, U2 resolved; U3 minor: FocusAsync after navigating away can show blazor-error-ui; no browser: Playwright tools unavailable)
- 2026-11-18 07:46 Saucer (code-reviewer) r2 balanced/opus: CHANGES_REQUESTED (C1 resolved; C2 major = U3: FocusAsync after dispose shows blazor-error-ui; C3 nit)
- 2026-11-18 08:42 Octopus (qa-reviewer) r2 balanced/opus: CHANGES_REQUESTED (Q1 resolved; Q2 major: no check for the request-counter race; Q3 minor: wrong D2 citation)
- 2026-11-18 08:42 Nishikado (PM): round 3 (last): Cannon for C2/U3, C3, Q3; then Squid adds the race browser check (Q2)
- 2026-11-18 08:42 Cannon (implementer WP1) r3 light/haiku: dispatched
- 2026-11-18 08:43 Cannon (implementer WP1) r3 light/haiku: IMPLEMENTED (C2/U3 dispose guard; Q3 citation; C3 nit not changed; 263 tests)
- 2026-11-18 08:43 Squid (tester) r3 fast/sonnet: dispatched (Q2 race browser check; C2 navigate-away check)
- 2026-11-18 08:46 Squid (tester) r3 fast/sonnet: PASS (6/6 ACs; browser checks Q2a, Q2b, C2 pass; 263 tests; note: an attempted broad taskkill of chrome.exe was blocked by permissions)
- 2026-11-18 08:46 Octopus (qa-reviewer), Saucer (code-reviewer), Pixel (ux-reviewer) r3 balanced/opus: dispatched; Bunker later
- 2026-11-18 08:47 Pixel (ux-reviewer) r3 balanced/opus: APPROVED (U3 resolved; no findings)
- 2026-11-18 08:47 Saucer (code-reviewer) r3 balanced/opus: APPROVED (C2 resolved; C3 nit accepted; no findings)
- 2026-11-18 08:47 Octopus (qa-reviewer) r3 balanced/opus: APPROVED (Q2, Q3 resolved; no findings)
- 2026-11-18 08:47 Bunker (security-reviewer) r3 balanced/opus: dispatched (final check)
- 2026-11-18 08:48 Bunker (security-reviewer) r3 balanced/opus: APPROVED (no findings)
- 2026-11-18 08:48 Nishikado (PM): done; committed (C3 nit accepted; real-phone pinch untried)

---
id: CHORE-005
name: UX guidelines
key: CHORE-005-ux-guidelines
type: CHORE
phase: done  # intake | planning | plan-review | awaiting-approval | implementing | testing | review | done | needs-input | blocked | parked | cancelled | escalated
size: S  # S | M | L (PM estimate at intake; the plan's value once written)
security_sensitive: no
ui_facing: no  # yes: the item gets a UX review
plan_round: 0
impl_round: 0
worktree: .work/CHORE-005-ux-guidelines
branch: devagents/CHORE-005-ux-guidelines
base_branch: feature/arcade-upgrades
base_commit: f2ce76802e2756d695bf0f6c0a747375e188887f
blocked_by: none  # ID of the item this one waits for
---

# Status: CHORE-005 UX guidelines

## Work packages

## Decisions
- D1 (2026-11-17): questions asked (PM's stated assumptions in brackets): 5. Design docs or a style guide [none; the current UI is the reference]? 6. Players, devices, top tasks [desktop keyboard plus phone touch; play, enter initials, view leaderboards]? 7. Look [current retro neon on black]; fonts or colours you insist on? 8. Accessibility [WCAG 2.2 AA where a canvas game allows], languages [English only]? 9. Conventions you like or dislike (menus, dialogs, toasts)? → user: "Yes to all" (taken as: every stated assumption holds; nothing else to add)
- D2 (2026-11-17): five gaps (failed leaderboard load looks empty, pinch zoom blocked, focus outlines removed, initials inputs unnamed, "Press P" on touch)? → "One fix-up CHORE" (rules stay as written; pinch zoom stays blocked during play only; fixed in a later item)
- D3 (2026-11-17): mute control and larger phone canvas text? → "Add mute control, Larger phone text" (both wanted; built in later items; the guidelines should say so)
- D4 (2026-11-17, PM from other items): Blazored.Toast is unused and is removed by a planned follow-up (CHORE-008); it is not the notification pattern.
- D5 (2026-11-17): approve the round-2 UX guidelines (12 CSS px phone text in UX14 included)? → "approve both"

## Accepted risks
none

## Log
- 2026-11-17 08:40 Nishikado (PM): created (intake)
- 2026-11-17 11:19 Nishikado (PM): user answered questions (D1)
- 2026-11-17 11:19 Pixel (ux-reviewer) r1 balanced/opus: dispatched (Draft the UX guidelines.)
- 2026-11-17 11:23 Pixel (ux-reviewer) r1 balanced/opus: DRAFT WRITTEN (saved by PM to docs/ux-guidelines.md; preview failed: disk full)
- 2026-11-17 11:32 Nishikado (PM): user answered open questions (D2, D3); D4 from other items
- 2026-11-17 11:32 Pixel (ux-reviewer) r2 balanced/opus: dispatched (revise draft)
- 2026-11-17 11:34 Pixel (ux-reviewer) r2 balanced/opus: REVISED (saved by PM to docs/ux-guidelines.md; UX29, UX38, Known gaps added)
- 2026-11-17 14:22 Nishikado (PM): approved by user (D5); committed

<!-- devagents:start -->
## DevAgents team

This repository uses the DevAgents multi-agent workflow: a PM takes requests from
the user and runs them through planning, plan review, implementation, testing,
code review, security review and, for a user interface, UX review. The process is defined in `.devagents/WORKFLOW.md`.

- The team: Saucer (Code Reviewer), Cannon (Implementer), Crab (Plan Reviewer), Mothership (Planner), Nishikado (PM), Octopus (QA Reviewer), Bunker (Security Reviewer), Squid (Tester), Pixel (UX Reviewer).
- Start the PM: Claude Code: `claude --agent pm --autocompact 300k`
- Agent definitions (generated, do not edit here): `.claude/agents/`
- Every request is tracked by ID and name, e.g. `FEAT-003 Account lockout after failed logins`.
  Refer to work items by ID.
- `docs/work/` is the single source of truth for all work: `docs/work/TRACKER.md`
  lists every item, and `docs/work/<key>/` (e.g. `docs/work/FEAT-003-account-lockout/`)
  holds its plan, status log, token usage, implementation notes, test reports and reviews.
- Token usage and cost across all items: `node .devagents/devagents.mjs report`.
- How to start the team, change its settings and upgrade it: `.devagents/README.md`.
- Work in progress lives in git worktrees under `.work/` (git-ignored), one per item.
  Do not modify work items or worktrees unless you are acting as a DevAgents role.
<!-- devagents:end -->

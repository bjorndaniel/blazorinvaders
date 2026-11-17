# DevAgents in this repository

This repository has a team of AI agents led by a PM, Nishikado. You describe a
feature or a bug, and Nishikado plans it with the team, shows you the plan for
approval, has it built, tested and reviewed, and commits it on a branch. Nothing
is pushed.

This file is generated. Do not edit it, or anything else DevAgents generated: an
install overwrites them. Change `.devagents/config.json` instead (see below).

## Start the PM

| Platform | How |
|---|---|
| Claude Code | `claude --agent pm --autocompact 300k` (compacts the PM's conversation at 300k tokens, which saves a lot; an older Claude Code without the option: update it, or leave it out) |
| GitHub Copilot (VS Code) | pick **pm** in the agent dropdown |
| Copilot CLI | `copilot --agent pm`, or `copilot --agent pm --model auto --auto-tier balance` if the Copilot models are `auto` |
| OpenAI Codex | `$devagents <what you want>`, e.g. `$devagents add rate limiting to the login endpoint` |

Then describe the work: one item or several. You can ask Nishikado *"what's in
progress?"*, *"status of FEAT-003?"* or *"what has the team cost so far?"* at any
time.

The process every agent follows is in [`WORKFLOW.md`](WORKFLOW.md); each agent's
own instructions already carry its part, and
`node .devagents/devagents.mjs workflow "<section>"` prints one section. Work records
live in `docs/work/`: one folder per item, plus `TRACKER.md`. While an item is
in flight, the team works in its own git worktree, `.work/<key>`: a
full checkout on the item's branch, git-ignored. `.devagents/` holds only the
team itself.

## The security model

The team works from `docs/security-model.md`: the project's access model (who may do
what), trust boundaries (where untrusted data comes in) and threat model (what
could go wrong, and what stops it). Plans and security reviews refer to its
entries by ID, and a change that alters the model updates the document in the
same item.

If the file is missing, Nishikado offers to create it at the start of a session:
you answer a few questions about users, data and systems, Bunker
drafts it from the code and your answers, and you approve it. It is committed on
its own branch for you to merge. If the team already keeps a threat model
elsewhere, set `securityModel` to its path instead (see below).

Skip any of these setup offers (the security model, architecture, coding or UX
guidelines, the app preview, quality tools) and Nishikado records it
(`node .devagents/devagents.mjs decline <setup>`, saved as `declined` in
`.devagents/config.json`), so no later session offers it again.
`decline --undo <setup>` brings an offer back.

## Architecture

`docs/architecture.md` records the architecture patterns, frameworks and conventions
the code follows, each choice with an ID. Plans name the choices they build on,
and a plan that needs a new framework or a different pattern says so, for you to
approve; the reviewers check plans and code against it.

If the file is missing, and first of all in a new project, Nishikado offers to
settle it: Mothership proposes a choice per area, based on what the code
already uses or, for a new project, a mainstream fit for what it is. Nishikado
asks you one area at a time, the suggestion first, and you change any of them.
Mothership then writes the document, committed on its own branch for you to
merge. If the team already keeps one, set `architecture` to its path instead.

## Coding guidelines

`docs/coding-guidelines.md` records how the code is written, beyond what the
formatter, linter and type checker enforce: naming, structure, errors and
logging, data and state, dependencies, tests and comments, each rule with an ID
(`CG1`, ...) and a level. Plans name the rules that apply, and
Saucer checks every diff against them, so the code stays
consistent from item to item: breaking a `must` rule without the plan saying so
sends the work back for a fix, a `should` slip is noted and fixed when the work
goes back anyway, and a new convention updates the document.

If the file is missing (in a new project, right after the architecture),
Nishikado offers to create it. Saucer proposes rules from
what the project already has, in whatever language: its formatter, linter,
type-checker and analyzer configs and the code itself, with where it is
inconsistent. Nishikado questions you on it, the inconsistencies first and
each rule's level, and Saucer writes the document from your
answers, committed on its own branch for you to merge. If the team already
keeps a style or contribution guide, set `codingGuidelines` to its path instead.

## UX guidelines and seeing the app

Every plan says whether the item is UI-facing: whether it changes anything a
person sees or operates. A UI-facing item gets a UX review by
Pixel, against, in this order: `docs/ux-guidelines.md` and the design
documents it names; the project's other design documents (a design system, a
style or brand guide); the UI as it is; and the team's default UI rules,
[`ux-defaults.md`](ux-defaults.md) (from
[praeclarum/ui.md](https://github.com/praeclarum/ui.md)).

If the guidelines are missing and the project has a UI, Nishikado offers to
create them: you point to the design documents you have, answer a few questions
(your users and their tasks, the look, the component library, accessibility,
language), Pixel drafts the document, and you approve it. If the
team already keeps such a guide, set `uxGuidelines` to its path instead.

Pixel looks at the running app, not only the code, through the
[Playwright MCP server](https://github.com/microsoft/playwright-mcp). Set it up
once, and commit what it changes:

```sh
node .devagents/devagents.mjs preview --set "npm run dev -- --port {port}"   # how the app starts; {port} is where the port goes (also in PORT)
node .devagents/devagents.mjs preview --playwright   # adds the server to the MCP configs, and gives the UX Reviewer its tools
node .devagents/devagents.mjs preview                # whether it is set up
```

Add `--url <url>` to `--set` if the page to open is not `http://localhost:{port}/`.
The UX Reviewer starts the item's app from its worktree on a free port, uses it
with the browser tools (never the ones that run code on your machine or read
its files), and stops it. Playwright uses Google Chrome; on a machine without
it, run `npx playwright install chrome` once. The platforms load a project's MCP
servers once you trust the project (Claude Code also asks about each server in
`.mcp.json`).

## Coding standards and library docs

The agents follow this repository's `.editorconfig` and coding guidelines
(`CONTRIBUTING.md`, style guides, lint and formatter configs, `AGENTS.md`,
`CLAUDE.md`, `.github/copilot-instructions.md`), so keep those current.

They check library and framework APIs in the documentation for the version this
repository pins: first the installed package itself (its type definitions, docs,
README and changelog), otherwise the library's official documentation on the web.
So install the dependencies before the team works.

## Helpers

The team can use what this repository already has: its own agents (for example a
security or database specialist in `.claude/agents/` or `.github/agents/`), skills,
prompt files, MCP servers (`.mcp.json`, `.github/mcp.json`, `.vscode/mcp.json`,
`.codex/config.toml`) and scanners such as semgrep or gitleaks.
`node .devagents/devagents.mjs helpers` lists them.

- **You decide who uses each one.** The first time Nishikado starts, it asks with
  the team's names, suggesting the team members each helper would help; later,
  whenever new ones turn up. Your answer is saved under `helpers` in
  `.devagents/config.json`, and each team member's prompt lists its own.
- **Skills, prompts, MCP servers and tools** a team member uses itself. **Agents**
  it asks through Nishikado, and gets the answer as a file in the item folder.
- **MCP servers that can write** (issues, comments, data): give the reviewers
  only their reading tools, one by one (`mcp:<server>/<tool>`); Nishikado
  proposes which. A helper never lifts a team member's limits, but for a whole
  server only the reviewers' instructions hold its writing tools back.
- A team member that finds another useful one says so, and Nishikado asks you.

## Notifications

Nishikado can tell you on Slack, Teams or any webhook when an item needs your
input, a plan is ready for approval, something is blocked, or an item is done.
You answer in the session as usual, or from the Claude app through Claude Code's
Remote Control. Each developer sets up their own channels; the webhook URLs stay
on your machine, in `.devagents/local.json` (git-ignored).

1. **Get a webhook URL:**
   - **Slack:** create an app at api.slack.com/apps, turn on **Incoming
     Webhooks**, add one to a channel (or to a DM with yourself), and copy its URL.
   - **Microsoft Teams:** in the **Workflows** app, use the template **Post to a
     channel when a webhook request is received**, pick the channel, and copy the URL.
   - **Anything else** (Discord, ntfy, Zapier, Make, n8n, your own WhatsApp
     bridge): a URL that takes a JSON POST with `text` (also `content`, `event`,
     `item`, `repo` and `at`).
2. **Save it yourself**, so the URL never goes through the chat. In Claude Code,
   type `!` first to run it in the session:
   ```sh
   node .devagents/devagents.mjs notify --set slack=https://hooks.slack.com/services/...
   node .devagents/devagents.mjs notify --set teams=https://...   # and/or webhook=https://...
   node .devagents/devagents.mjs notify --test                    # send a test message
   node .devagents/devagents.mjs notify                           # what is set up (URLs masked)
   ```
   An empty URL (`--set slack=`) removes a channel. The environment variables
   `DEVAGENTS_NOTIFY_SLACK`, `DEVAGENTS_NOTIFY_TEAMS` and `DEVAGENTS_NOTIFY_WEBHOOK`
   work too, and win over the file.
3. To get fewer messages, list the events you want in `.devagents/local.json`:
   `{ "notify": { "events": ["input", "approval"] } }` (the events are `input`,
   `approval`, `blocked` and `done`).

Messages carry the repository, the item's ID and name, and one line about what
is needed, never code or secrets.

## Change the settings

1. Create or edit `.devagents/config.json`. Put in only the keys you want to
   change; everything else keeps its default.
2. Run `node .devagents/devagents.mjs install` to regenerate the agent files.
   It updates the platforms already installed here.
3. Commit `.devagents/config.json` and the regenerated files.

To see every setting with its current value:

```sh
node .devagents/devagents.mjs config
```

Example `.devagents/config.json`:

```json
{
  "aliases": { "pm": "Sam", "implementer": "Robin" },
  "models": {
    "claude": { "deep": "opus" },
    "copilot": { "balanced": { "model": "Claude Sonnet 5.5 (copilot)", "effort": "medium" } },
    "codex": { "fast": { "model": "gpt-6-luna", "effort": "low" } }
  },
  "pricing": { "my-model": { "input": 1, "output": 4 } },
  "maxParallelAgents": 2
}
```

| Key | What it sets |
|---|---|
| `aliases` | The human name of each role: `pm`, `planner`, `plan-reviewer`, `implementer`, `tester`, `qa-reviewer`, `code-reviewer`, `security-reviewer`, `ux-reviewer`. Names must be unique, up to 30 characters, starting with a letter. The PM asks for a theme on first start and sets them with `node .devagents/devagents.mjs names --theme "<theme>" <role>=<Name> ...`, which also re-renders the agent files; `names --keep` keeps the defaults. |
| `models` | The model for each tier (`light`, `fast`, `balanced`, `deep`) on each platform. The PM picks a tier per task (see `WORKFLOW.md`, Models). Model names differ per account and change often: check yours against the model picker (Claude Code, Copilot) or the Codex model list. Every tier can also set the reasoning `effort`, e.g. `low`, `medium` or `high` (`{ "model": "...", "effort": "..." }`; Codex tiers always do). Claude and Copilot tiers can also be a plain model name. Claude tiers are aliases (`haiku`, `sonnet`, `opus`, `fable`), the only models Claude Code's Agent tool takes; on Bedrock, Vertex AI or Foundry, pin them with `ANTHROPIC_DEFAULT_HAIKU_MODEL` and the like (`preflight` warns). |
| `reviewModels` | Models for the reviewers (plan, QA, code, security and UX review), in the same shape as `models`, so a reviewer checks the work with a different model from the one that wrote it. A platform or tier left out uses `models`. Example: `{ "reviewModels": { "copilot": { "balanced": { "model": "GPT-6 Sol (copilot)", "effort": "medium" } } } }`. |
| `pmModel` | The PM's own model on Claude Code and Copilot, outside the tiers. By default `null`: the PM's tier, `balanced` (Opus). A small model at high effort makes the session cheaper: `{ "pmModel": { "claude": { "model": "haiku", "effort": "high" }, "copilot": { "model": "GPT-6 Luna (copilot)", "effort": "high" } } }`. On Codex the PM is your Codex session: pick its model when you start it. |
| `quality` | The project's own mutation, coverage and complexity tools for the QA Reviewer, each a `command` (see `WORKFLOW.md`, Quality tools and assurance). The PM offers to set them up as an item. |
| `assurance` | `low`, `standard` (the default) or `high`: how strict QA and security review are. Security-sensitive items are never below `standard`. |
| `testcontainers` | `use` or `never`: whether tests that need a real database, broker or other service use Testcontainers. Not set: the Planner asks you the first time it matters (see `WORKFLOW.md`, Testcontainers). |
| `declined` | The setup offers you skipped, so the PM does not make them again: any of `security-model`, `architecture`, `coding-guidelines`, `ux-guidelines`, `ux-preview`, `quality-tools`. Set with `node .devagents/devagents.mjs decline <setup>`; `--undo` removes one. |
| `pricing` | USD per million tokens, used only to estimate costs: `input`, `output`, and optionally `cacheWrite`, `cacheWrite1h`, `cacheRead`. A model id matches the longest key it contains. Add any model that shows `n/a` as its cost. |
| `maxParallelAgents` | How many agents may run at once. Currently 8. |
| `workDir` | Where the work records live. Currently `docs/work`. |
| `worktreeDir` | Where each work item's git worktree (a full checkout on its branch) goes while it is in flight. Currently `.work`. |
| `securityModel` | The security model document. Currently `docs/security-model.md`. |
| `architecture` | The architecture document. Currently `docs/architecture.md`. |
| `codingGuidelines` | The coding guidelines. Currently `docs/coding-guidelines.md`. |
| `uxGuidelines` | The UX guidelines. Currently `docs/ux-guidelines.md`. |
| `uxPreview` | How the UX Reviewer starts the app: `command` (with `{port}`), `url` (default `http://localhost:{port}/`), `waitSeconds` (120), `stopMinutes` (30). Set with `preview --set`. |
| `estimates` | The figures behind the cost estimate: tokens per agent run by item size, the cached share of input, the usual planning and build rounds, what a later round costs as a share of the first (`laterRounds`), the PM's share. |

### Copilot accounts that only allow Auto

If Copilot says `Model '…' is not available`, your account may only allow Auto.
The PM then retries with `auto` and tells you. To stop the failed attempts, set
every Copilot tier to `auto`:

```json
{ "models": { "copilot": { "light": "auto", "fast": "auto", "balanced": "auto", "deep": "auto" } } }
```

Copilot then picks the model for each request. Choose how it routes for the
whole session with `--auto-tier efficiency|balance|intelligence`, or in the
model picker. Copilot cannot route each agent differently, so the tiers no
longer make a difference there.

## Token usage and cost

Every agent run is recorded with its model, tokens and cost in
`docs/work/<item>/usage.md`: on Claude Code and Codex as estimated USD, on
Copilot CLI in the AI credits it billed (read from its session logs; Copilot Chat
in VS Code keeps none).

```sh
node .devagents/devagents.mjs report                 # everything: by item, agent and model
node .devagents/devagents.mjs usage FEAT-003-account-lockout
node .devagents/devagents.mjs estimate FEAT-003-account-lockout   # ballpark for an item, from its plan
```

With every plan, Nishikado shows an estimate: the expected tokens and cost, and a
range from one round to the round limit. It is a ballpark from the `estimates`
settings; compare it with what `report` shows and tune them.

Costs are estimates at API list prices from `pricing`. On a subscription you pay
differently, but the numbers still show which roles, models and items cost the
most. Claude Code records usage only in folders you have trusted, and only with
`node` on the PATH.

## Other commands

```sh
node .devagents/devagents.mjs install --dry-run      # what an install would change
node .devagents/devagents.mjs install --platforms claude,copilot,codex   # add a platform: list every one you want
node .devagents/devagents.mjs uninstall              # keeps work records, config and worktrees
node .devagents/devagents.mjs migrate                # move worktrees left in .devagents/worktrees/ by versions before 0.9
node .devagents/devagents.mjs preflight              # before work: commits, branch, and what is uncommitted
node .devagents/devagents.mjs changes <key> --since 1   # what changed in an item since round 1 was reviewed
node .devagents/devagents.mjs docs                   # the project's documents, with their titles
node .devagents/devagents.mjs crap <key>             # CRAP scores of the functions an item changed
node .devagents/devagents.mjs containers             # container runtime, Testcontainers use and setting
node .devagents/devagents.mjs preview                # whether the UX Reviewer can see the app (see UX guidelines above)
node .devagents/devagents.mjs names                  # the team's names; --theme "<theme>" <role>=<Name> ... renames it
node .devagents/devagents.mjs notify                 # your notification channels; --set, --test (see Notifications)
node .devagents/devagents.mjs decline                # setup offers you skipped; decline <setup> [--undo] (see The security model)
node .devagents/devagents.mjs workflow "Models"      # one section of WORKFLOW.md; no name lists the headings
node .devagents/devagents.mjs help
```

## Upgrade

Run a newer `devagents-<version>.mjs` from the repository root:

```sh
node path/to/devagents-<new version>.mjs install
```

It updates the platforms already installed here, replaces
`.devagents/devagents.mjs`, and keeps `.devagents/config.json` and the work
records. Commit the result.

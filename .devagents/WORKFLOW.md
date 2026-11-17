# DevAgents Workflow

The protocol every DevAgents role follows, on Claude Code, GitHub Copilot and OpenAI
Codex. Role prompts are installed as agents in `.claude/agents/` and
`.github/agents/`, and as prompt files in `.devagents/roles/` for Codex (whose PM is
the `devagents` skill in `.agents/skills/`).

**Reading this file.** Each role prompt carries what its role needs; nobody reads
this whole file. For anything else, print one section:
`node .devagents/devagents.mjs workflow "<section>"` (several names allowed; none
lists the headings). The PM reads a section when its prompt points to one, such as
**Models** before its first dispatch in a session.

## Roles

| Name | Role | Writes | Never does |
|---|---|---|---|
| Nishikado | **PM** | the tracker, `request.md`, `status.md` (with the log), the plan's header fields, review files (verbatim), worktrees, branch + commit | Write plans or code, review work, skip a gate |
| Mothership | **Planner** | the plan, and its response to each plan review | Touch source code |
| Crab | **Plan Reviewer** | nothing (returns its review); the plan's adversary | Edit the plan or code |
| Cannon | **Implementer** | its work package's code and unit tests, `implementation/<WP>.md` | Touch files outside its package, change scope, commit |
| Squid | **Tester** | test files, `test-report.md` | Change production code |
| Octopus | **QA Reviewer** | its breaks list (returns its review); breaks code on purpose, in the scratch copy, to test the tests | Change the worktree, fix code or tests |
| Saucer | **Code Reviewer** | nothing (returns its review); keeps the code consistent with the coding guidelines, and proposes and drafts them | Edit code |
| Bunker | **Security Reviewer** | nothing (returns its review) | Edit code |
| Pixel | **UX Reviewer** | nothing (returns its review); reviews UI-facing items in the running app | Edit code |

Logs and messages use the names (set in the config), with role and package where it
helps: "Cannon (implementer WP2)". Only the PM talks to the user;
everyone else talks only to the PM, through its final response and the item files.
The PM may run several agents of one role at once, for different items or packages.

## Naming

| | Format | Example | Used in |
|---|---|---|---|
| **Name** | Short title, ≤ 60 chars, sentence case | `Account lockout after failed logins` | Tracker, plan, headings, commit subject, messages to the user |
| **ID** | `<TYPE>-<NNN>`: `FEAT`, `BUG` or `CHORE`, zero-padded, sequential per type | `FEAT-003` | Every reference: findings, logs, commit trailer |
| **Key** | `<ID>-<slug>`, the name in kebab-case, ≤ 40 chars | `FEAT-003-account-lockout` | Item folder, worktree, branch |

The PM proposes name and type at intake; the user confirms or renames (the name
only: the ID never changes). **Next number** for a type = 1 + the highest for that
type among the folders in `docs/work/` and `.work/` and the branches
from `git branch -a --list "*devagents/*"`, so an unmerged branch's ID is not reused.

## Where things live

**`docs/work/` is the single source of truth**: every item's records are
committed there with the code they describe.

| Path (from the repository root) | What | Git |
|---|---|---|
| `docs/work/<key>/` | The **item folder**: everything about one work item | committed |
| `docs/work/TRACKER.md` | One row per work item, newest first | committed |
| `.work/<key>/` | The item's **git worktree**, branch `devagents/<key>`: its code, item folder and every git/build/test command, until merged | ignored |
| `.devagents/` | The team: the tool `devagents.mjs` (install, usage reports, Codex runner, Claude Code usage hook), this workflow, templates, Codex role prompts. No work items | committed |
| `.devagents/runs/` | Dispatch scratch files, each round's snapshot of the reviewed code, the app preview's state and log, usage-tracking state | ignored |
| `.devagents/config.json` | The team's settings (names, models, prices, limits); see `.devagents/README.md` | committed |
| `.devagents/ux-defaults.md` | Default UI rules, where the project's own say nothing | committed |
| `docs/security-model.md`, `docs/architecture.md`, `docs/coding-guidelines.md`, `docs/ux-guidelines.md` | The project's security model, architecture, coding and UX guidelines (sections below) | committed |

The item folder lives in the worktree, on the item's branch, and reaches the main
branch on merge; the user's working tree is never touched. A worktree starts from
the last commit, so before any work the PM runs `node .devagents/devagents.mjs
preflight`; for uncommitted changes, or no commit at all, it offers to commit them
on the current branch, the DevAgents setup apart from the user's own changes. That
commit, with the user's OK, is all the PM ever does in the user's checkout.

Shorthands: `<wt>` = the worktree, `.work/<key>` (run commands there:
`cd <wt> && <command>`, `git -C <wt> ...`); `<item>` = `<wt>/docs/work/<key>`.

```
docs/work/<key>/
  plan.md                Planner  the plan; PM maintains its header fields
  status.md              PM       phase, size, rounds, worktree, base commit, decisions, log
  usage.md               (auto)   tokens, model and cost of every agent run
  request.md             PM       the request, acceptance criteria, constraints
  implementation/<WP>.md Impl.    what changed per round, one file per work package
  test-report.md         Tester   results per round (the final check, with several packages)
  test-report-<WP>.md    Tester   one package's results per round, with several packages
  reviews/
    plan-r<N>.md           PM saves the plan review of round N
    plan-r<N>-response.md  Planner  its answer to each finding of plan-r<N>.md
    code-r<N>.md           PM saves the code review (code-r<N>-<WP>.md when split)
    qa-r<N>.md, security-r<N>.md, ux-r<N>.md (UI-facing)   PM saves the other reviews
  help/<role>-r<N>-<helper>.md  PM saves a helper's answer (see Helpers)
```

Each file has a template in `.devagents/templates/`. **Keep them short:** later
agents read them every round. Delete the templates' comments, leave out optional
sections that do not apply, and never restate other files (request, plan, diff,
another review).

### The plan

The Planner writes it from `.devagents/templates/plan.md` and revises it in place,
logging each round under **Revisions**. The PM maintains the header fields
**Status** (`draft` → `approved <date>` → `implemented <date>`, or
`cancelled <date>`), **Size**, **Security-sensitive** and **UI-facing**.

### Plan consensus

A plan goes to the user only when the Planner and the Plan Reviewer agree on it. The
Plan Reviewer is its adversary: it assumes the plan is wrong until it has tried to
break it and could not. They argue only through files; the user breaks ties.

1. The Plan Reviewer writes `reviews/plan-r<N>.md`.
2. The Planner answers every open `blocker`, `major` and `minor` finding in
   `reviews/plan-r<N>-response.md`: `accepted` (plan changed) or `disputed` (wrong,
   with evidence).
3. The next review marks each `resolved` (fix verified), `withdrawn` (the dispute
   convinced it), `maintained` (answering the argument) or `decided` (by the user).
4. A `maintained` finding gets one more response: `accepted` or `deadlocked`.
5. **Deadlocked findings go to the user** (tie-break, phase `needs-input`): per
   finding its ID, severity and issue in one line; both positions in their own
   terms, at similar length; the paths of review and response; no side recommended
   unless asked. Options: **the reviewer is right** (the plan changes as required),
   **the planner is right** (dropped), or the user's own instruction. The answer goes
   under **Decisions** (`D<n> (<date>): plan finding P3 (deadlocked) → reviewer is
   right: <what the plan must do>`) and binds both sides for good: the Planner
   applies it, the Plan Reviewer marks it `decided`. A decision that changes the plan
   sends the Planner back in the same round, with it on `User decision:`, before the
   next review. If the user dropped every deadlocked finding and nothing else is
   open, that is consensus.
6. **Consensus** is `VERDICT: APPROVED`: every `blocker` and `major` so far is
   resolved, withdrawn or decided.

**Planning converges.** Round 1 is the one full review: every finding is raised
then. Later rounds check the fixes and attack only what changed; on unchanged parts
only a `blocker` may be raised, anything less is `minor`. A plan finding is `major`
only if the plan, built as written, would miss an acceptance criterion, break
existing behaviour, be insecure or not be buildable. `minor` never blocks: the
Planner folds in those it agrees with; the PM shows open ones at the approval gate.
After 3 review rounds without consensus, the user decides every open finding or
allows more rounds.

### Tracker

The PM creates `docs/work/TRACKER.md` from `.devagents/templates/tracker.md` if
missing, adds the item's row at intake (in the worktree) and updates it on every
phase change; it is committed with the item. `.gitattributes` marks it
`merge=union`; if a merge tool still shows a conflict, keep both sides' rows. An
in-flight item's row exists only on its branch, so the live picture is each
worktree's `status.md`.

## Size and sensitivity

The PM estimates these at intake; the Planner sets them in the plan; the Plan
Reviewer checks them. They drive model choice and parallelism.

| Size | Meaning |
|---|---|
| **S** | ≤ ~3 files and ~100 changed lines, follows an existing pattern, no new dependency |
| **M** | A typical feature or bug fix within one area of the codebase |
| **L** | Cross-cutting, a new pattern or architecture, data migration, concurrency, performance-critical, or unfamiliar territory |

**Prefer S and M items.** An L item costs several times an M one, mostly in extra
review rounds, so the PM proposes splitting an L request into a sequence of S/M
items at intake, and an L plan carries a **Split** section the user may choose at
the approval gate. The user decides; an item that cannot be split stays L.

**Security-sensitive** = `yes` if the change touches authentication, authorization,
sessions, cryptography, secrets, parsing or rendering of untrusted input, file or
network access driven by input, payment processing or payment data (charging,
refunds, card or bank details), personal data, dependencies, or CI/CD and
infrastructure. Price, discount or tax arithmetic alone is not payment processing.
A change that crosses a trust boundary or changes the access model in the security
model is security-sensitive too.

**UI-facing** = `yes` if the change adds or changes anything a person sees or
operates in the product's user interface: screens, pages, components, styles and
layout, navigation, forms, dialogs, the user-facing text and messages in them,
keyboard and focus behaviour, or accessibility. Command-line output, APIs, logs and
background jobs are not. A UI-facing item gets a UX review.

## Security model

`docs/security-model.md`, at the repository root, says how the project is meant to be
secure (template `.devagents/templates/security-model.md`), every entry with an ID:
- **Access model** (`AC1`, ...): who and what acts on the system (people, services,
  jobs), how each is authenticated, what each may and may not do
- **Trust boundaries** (`TB1`, ...): where data or control crosses from less to more
  trusted (user input, HTTP, files, queues, third-party APIs, the database), and
  where it is checked
- **Threat model** (`T1`, ...): what could go wrong at each boundary, its impact,
  what stops it, its status

**Missing:** at the start of a session the PM offers item `CHORE-<NNN> Security
model` (ID, worktree, item folder as at intake), or the user points `securityModel`
at their own document.
1. The PM asks in one message: who uses the system, with which roles and
   permissions; how they sign in; which data is sensitive; where input comes from
   (UIs, APIs, files, queues, services); which outside systems it trusts; how it is
   deployed; known threats, incidents or compliance needs. "Don't know" is fine.
2. The Security Reviewer drafts it from the code and the answers (Task `Draft the
   security model.`, answers verbatim on `User decision:`); the PM saves it to
   `<wt>/docs/security-model.md`.
3. The PM shows its actors, boundaries and open threats; changes go back to the
   Security Reviewer, naming the draft, until the user approves.
4. The PM commits it on the item's branch for the user to merge. No plan, tests or
   code review.

**Skipped:** `decline security-model` (see **Problems and escalation**): not offered
again. Plans cover security in their own words; no agent creates the document, asks
for it or reports its absence.

**In use:** every plan names under **Security considerations** the boundaries,
actors and threats it touches, by ID. A change that alters the model (new actor or
permission, new or changed boundary, new threat) updates it in the same item, in a
plan step the reviewers check. Every security review checks the change against the
access model and at every boundary it crosses.

## Architecture

`docs/architecture.md`, at the repository root, records the architecture patterns,
frameworks and conventions the project uses, and why, so every plan builds on the
same choices. Each choice has an ID (`A1`, ...). Template:
`.devagents/templates/architecture.md`.

**Missing:** at the start of a session the PM offers item `CHORE-<NNN>
Architecture` (ID, worktree, item folder as at intake), or the user points
`architecture` at their own document. A new project settles it first.
1. New project: the PM first asks what it does, for whom, how large it may grow,
   where it runs, and the team's languages, into `request.md`.
2. The Planner proposes (Task `Propose the architecture.`) in
   `<item>/architecture-proposal.md`: per area that applies, a suggestion, why in
   one line, one or two alternatives. Areas: language and runtime version;
   architecture pattern (layered, hexagonal, vertical slices, modular monolith,
   services); modules; web or API framework; UI framework; data storage and access
   (database, ORM or query builder, migrations); validation; authentication;
   configuration and secrets; logging and errors; testing (unit, integration, end
   to end); linting and formatting; build, CI and deployment. Existing code: what it
   uses, unless the user asked for a change. New project: a mainstream,
   well-supported choice for its purpose and languages; small projects stay simple.
3. The PM shows it one area at a time (AskUserQuestion where available, up to four
   at once, the suggestion first as the recommendation; the user may type their
   own), or as one table to accept or change. Answers go under **Decisions**.
4. The Planner writes `<wt>/docs/architecture.md` from the template and the answers
   (Task `Write the architecture document.`, answers verbatim on `User decision:`).
5. The PM shows a summary; changes go back to the Planner until the user approves.
   The PM commits it on the item's branch. No plan review, tests or code review.

**Skipped:** `decline architecture`: not offered again. Plans follow the code as it
is; no agent creates the document, asks for it or reports its absence.

**In use:** every plan names under **Context found** the choices it builds on, by ID.
A choice the document does not cover (a new framework, library, pattern, or an
unnamed dependency), or a different one, is declared in the plan's **Summary** with
a step that updates the document. The Plan Reviewer and Code Reviewer check against
it: an undeclared departure is `major`.

## Coding guidelines

`docs/coding-guidelines.md`, at the repository root, records how the project's code
is written beyond what its tools enforce: naming, structure, errors and logging,
data and state, dependencies, tests, comments. Each rule has an ID (`CG1`, ...), a
source and a level:
- `must`: an undeclared breach is `major`. For rules whose breach causes bugs,
  security or data problems, or spreads through the code.
- `should`: a slip is `minor`, fixed when the change goes back anyway. Style, taste.

A rule without a level (in an older guide) is `must` if it says must, never or
always, else `should`. Template: `.devagents/templates/coding-guidelines.md`. Stack
and patterns are in `docs/architecture.md`; what a formatter, linter, type checker or
analyzer enforces is listed with the tool, not repeated as a rule. Rules come from
the project's own tools, code and people, for any language.

**Missing:** at the start of a session the PM offers item `CHORE-<NNN> Coding
guidelines`; for a new project right after the architecture, before the first
request.
1. If preflight names documents that may be them (a contribution or style guide,
   conventions), the PM asks whether one is; if so, the user points
   `codingGuidelines` at it and runs `install`, and the item ends.
2. The Code Reviewer proposes (Task `Propose the coding guidelines.`); the PM saves
   it as `<item>/coding-guidelines-proposal.md`. It reads, per language, the
   formatter, linter, type-checker and analyzer configs and the build's compiler
   warnings (e.g. `.editorconfig`, Prettier, ESLint, Biome, Ruff, Black, mypy,
   Pyright, rustfmt, Clippy, gofmt, golangci-lint, clang-format, clang-tidy,
   RuboCop, SwiftLint, ktlint, detekt, Checkstyle, Spotless, PMD, Roslyn analyzers,
   PHP_CodeSniffer, Dart's `analysis_options.yaml`, Credo, ShellCheck),
   `docs/architecture.md`, the instruction files, and code samples per language and
   layer. Per template area: what the code does now (files, how consistently), a
   suggestion with a level per rule, alternatives; inconsistent or silent → a
   question. New project: the language's official or most used conventions and
   tools for the stack in `docs/architecture.md`.
3. The PM grills the user: the questions, then the other areas one at a time
   (AskUserQuestion where available, up to four at once, suggestion first; the user
   may type their own), then each rule's level (`must` blocks when broken, `should`
   does not), last what they insist on or dislike in reviews. Answers go under
   **Decisions**.
4. The Code Reviewer writes it (Task `Write the coding guidelines.`, answers
   verbatim on `User decision:`); the PM saves it as `<wt>/docs/coding-guidelines.md`.
   The user's answers win over the code, the code over the language's conventions.
5. The PM shows a summary; changes go back to the Code Reviewer, naming the earlier
   draft, until the user approves. The PM commits it on the item's branch. No plan
   review, tests or code review.

**Skipped:** `decline coding-guidelines`: not offered again. Everyone follows the
code as it is, then the language's mainstream conventions; no agent creates the
document, asks for it or reports its absence.

**In use:** every plan names the rules that apply under **Context found**, by ID; a
new convention or a departure is declared in its **Summary** with a step updating
the document. The Code Reviewer checks every diff against the document, the
surrounding code and the tools' settings (the Tester runs the tools): an undeclared
departure is `major` from a `must` rule, `minor` from a `should` rule; a new
convention added without updating the document is `minor`. The Plan Reviewer checks
plans the same way.

## UX guidelines

`docs/ux-guidelines.md`, at the repository root, records how the project's UI looks,
behaves and reads: its users and their tasks, the design documents it follows, its
look, components and patterns, its accessibility target and language. Each
guideline has an ID (`UX1`, ...). Template: `.devagents/templates/ux-guidelines.md`.
It matters only with a UI; `preflight` says whether it found one.

UI work is built and reviewed against these, an earlier one winning:
1. the guidelines, and the design documents they name
2. the project's other design documents (design system, style or brand guide,
   accessibility requirements), whatever their names
3. the UI as it is
4. the team's default rules, `.devagents/ux-defaults.md`, with the platform's
   conventions and WCAG 2.2 AA

**Missing**, with a UI or the first UI-facing item: the PM offers item
`CHORE-<NNN> UX guidelines` (ID, worktree, item folder as at intake).
1. If `preflight` names documents that may be one, the PM asks whether one is; the
   user can point `uxGuidelines` at it and run `install`.
2. Otherwise the PM asks a few questions at a time (AskUserQuestion where
   available): design documents (paths or links); users, devices, platforms, top
   tasks; the look (brand colours, typography, light and dark mode); the component
   library; the accessibility target (WCAG 2.2 AA by default); languages and tone;
   conventions they care about or dislike (forms, errors, confirmations,
   navigation). "Don't know" is fine: the default rules fill gaps.
3. The UX Reviewer drafts it from the code (and the running app, if the preview is
   set up), the documents and the answers (Task `Draft the UX guidelines.`, answers
   verbatim on `User decision:`); the PM saves it to `<wt>/docs/ux-guidelines.md`.
4. The PM shows its principles and open questions; changes go back to the UX
   Reviewer, naming the draft, until the user approves.
5. The PM commits it on the item's branch. No plan, tests or code review.

**Skipped:** `decline ux-guidelines`: not offered again. The UX Reviewer uses the
other sources; no agent creates the document, asks for it or reports its absence.

**In use:** every UI-facing plan names under **UX considerations** the screens and
states it adds or changes and the guidelines it follows by ID (or the default
rules). A new UI pattern or a departure is declared in its **Summary** with a step
that updates the document.

**Seeing the app** (Playwright):
- `node .devagents/devagents.mjs preview <key>` starts the app from the worktree on
  a free port with the `uxPreview` command (`{port}` where the port goes; also in
  `PORT`) and prints its URL; `preview <key> --stop` stops it, as do 30 minutes,
  `snapshot save` and `snapshot check`. The command must write nothing outside
  git-ignored folders (the snapshot check would report it). On Codex, whose
  reviewers cannot start processes, the runner starts the app around the UX run.
- The browser runs through the Playwright MCP server (`@playwright/mcp`, headless,
  started by `node .devagents/devagents.mjs playwright-mcp`), with the tools for
  looking at and using the app as a person would; never those that run code or read
  files on the machine.
- **Setup**, once, with the guidelines or alone while `preflight` says `UX preview:
  not set up`: the PM proposes the start command (from the README and package
  scripts), and on the user's OK runs `node .devagents/devagents.mjs preview --set
  "<command>" [--url <url>]` and `preview --playwright` (adds the server to the
  platforms' MCP configs, grants its tools to the UX Reviewer), then offers to
  commit, as at the start of a session. Skipped: `decline ux-preview`; the UX
  Reviewer reviews the code alone.

## Quality tools and assurance

QA proves the tests catch broken code: with a **mutation framework** (many small
changes, each of which a test must catch) and **CRAP scores** (complexity² ×
(1 − coverage)³ + complexity per changed function: complex, barely tested code
scores high), or by breaking code by hand without them. The tools go under
`quality` in `.devagents/config.json`:

```json
"quality": {
  "mutation":   { "command": "npx stryker run --mutate {files,} --reporters clear-text,json", "report": "reports/mutation/mutation.json", "results": "survivors in the clear-text summary" },
  "coverage":   { "command": "npx c8 --reporter=lcov --reports-dir={out} npm test", "report": "{out}/lcov.info" },
  "complexity": { "command": "uvx lizard --csv {files}" }
}
```

- Commands run in the item's **scratch copy**, `.devagents/runs/<key>/scratch` (a
  git worktree of `base_commit` with the item's changes, kept in step by the kit),
  never the worktree, so tools that write and QA's breaks never touch what other
  reviewers read. The PM installs its dependencies (**Worktree setup**). `{files}` =
  the changed production files (`{files,}` comma-separated, for Stryker's
  `--mutate`); `{out}` = an output folder outside the worktree;
  `mutation.timeoutMinutes` (90) stops a hung run.
- `coverage.report`: where coverage writes LCOV, Cobertura, JaCoCo XML or a Go
  coverprofile. `complexity` prints `lizard --csv`; leave it out when the report has
  per-method complexity (Cobertura from coverlet or PHPUnit, JaCoCo).
- `node .devagents/devagents.mjs crap <key>` scores the changed functions from
  coverage and complexity; `preflight` says whether the tools are set up.
- `node .devagents/devagents.mjs mutate <key>` mutates the changed production files.
  **The Tester** runs it once its tests pass, and writes tests for survivors in the
  same round. **QA** reuses the run while production code is unchanged
  (`mutate <key> --status`), re-checks survivors, and judges those the Tester calls
  equivalent. With `mutation.report` naming a mutation-testing JSON report
  (Stryker's `json` reporter: `reports/mutation/mutation.json`), `mutate` writes the
  survivors on changed lines as a `breaks` file: `breaks <key> <file> --tests
  "<command>"` re-checks them in seconds.

**Setup** is item `CHORE-<NNN> Quality tools`, offered at session start while they
are missing; it also settles `assurance`. The plan picks the stack's tools below,
adds them as dev dependencies and configuration, and fills `quality` (and
`assurance`); the Tester runs each command once; security review checks the new
dependencies. Skipped: `decline quality-tools`; QA breaks code by hand.

| Stack | Mutation | Coverage report | Complexity |
|---|---|---|---|
| JavaScript / TypeScript | StrykerJS (`@stryker-mutator/core` with its jest, vitest or mocha runner, or the command runner) | LCOV: c8, nyc, jest, vitest, or `node --test --experimental-test-coverage --test-reporter=lcov` | lizard |
| .NET | Stryker.NET (`dotnet-stryker` as a local tool) | Cobertura from coverlet (has complexity) | from the report |
| Java / Kotlin | PIT (`pitest-maven`, Gradle `info.solidsoft.pitest`) | JaCoCo XML (has complexity) | from the report |
| Python | mutmut | `coverage xml` (Cobertura) | lizard |
| PHP | Infection | PHPUnit `--coverage-cobertura` (has complexity) | from the report |
| Go | Gremlins | `go test -coverprofile` | lizard |
| Rust | cargo-mutants | cargo-llvm-cov `--lcov` | lizard |

lizard covers all of these, without installing (`uvx lizard`, `pipx run lizard`).

**Assurance** (`low`, `standard` or `high` in the config; default `standard`) sets
how strict QA and security review are; security-sensitive items are never below
`standard`. The PM puts the item's level on the `Assurance:` line of QA and security
briefs.

| | low | standard | high |
|---|---|---|---|
| Mutation framework | not run | run on the changed code | run on the changed code |
| Breaks by hand, S/M/L | 3/5/8 | 5/10/15, or 3/5/8 after a framework run | 5/10/15 |
| CRAP of a changed function | `major` above 60, `minor` above 30 | `major` above 30 | `major` above 20 |
| A mutant or break that survived | `major` where it matters, else `minor` | `major` where it matters, else `minor` | `major` |
| Security Reviewer | at most `balanced`; only an issue shown to be exploitable in this project blocks; hardening ideas are `minor` | tier as in **Models**; a weakness blocks when it has a plausible exploit path in this project or breaks a rule of the security model; hardening or defence in depth without either is `minor` | always `deep`; as `standard`, and a missing second defence at a trust boundary is `major` |

It **matters** when the changed code serves an acceptance criterion, a planned test,
a security control, error handling, or data that is stored or sent. Code that only
presents (CSS classes, copy text, log wording) does not, unless an acceptance
criterion is about exactly that; a gap there is `minor` (below `high`), so it never
costs a round alone. CRAP limits: `quality.crapLimits`.

## Testcontainers

Tests needing a real database, broker, cache or other service can run it in a
throwaway container with [Testcontainers](https://testcontainers.com/) (Java, .NET,
Go, Node, Python, Rust, Ruby): fresh containers on random ports, so parallel Testers
do not clash. It needs Docker or Podman wherever those tests run (CI too) and is
slower than unit tests: integration tests only. The user decides, the first time it
matters:
- `node .devagents/devagents.mjs containers [<key>]` (and `preflight`) shows the
  runtime, whether the project uses Testcontainers, and the `testcontainers`
  setting (`use` or `never`; unset = undecided). A project that uses it keeps using
  it, as its tests do.
- Undecided, with a runtime running, and a plan needing a real dependency: the
  Planner escalates a `QUESTION` (Testcontainers, with library, images and the
  runtime it needs, or the existing approach); the plan then sets `testcontainers`
  in `.devagents/config.json`, committed for later items.
- `never`, or no runtime: the existing approach (test database, in-memory fake,
  mocks at the boundary).
- Images are pinned to the version run in production, never `latest`.
- Tests whose runtime is not running: the Tester escalates `BLOCKED`, never skips
  them. The Codex sandbox cannot reach a container runtime.

## Models

Every dispatch uses one of four tiers; the PM's prompt maps them to models.
- **The PM's own model** (`pmModel`): it reads more tokens than the rest of the team
  together. Default: its tier, `balanced` (Opus). A small model at high effort
  (Claude Haiku 5.5, GPT-6 Luna) is cheaper; the kit's commands (`preflight`,
  `rerun`, `snapshot`, `log`) carry the bookkeeping. On Codex: your session's model.
- **Claude Code's tiers are aliases** (`haiku`, `sonnet`, `opus`, `fable`), the only
  models its Agent tool takes; the newest on Anthropic's API. On Bedrock, Vertex AI,
  Foundry and Claude Platform on AWS they can mean older ones (`haiku` is Haiku 4.5
  there): pin them with `ANTHROPIC_DEFAULT_HAIKU_MODEL`, `_SONNET_`, `_OPUS_` (the
  provider's model IDs); `preflight` warns. A member whose model is unavailable runs
  once more on the next larger one, and the user is told.
- **Reviewers may have their own models** per tier (`reviewModels`), different from
  the writer's, so they do not share its blind spots.

| Tier | For |
|---|---|
| `light` | Well-specified work with a clear check: building a small plan's steps, running the checks |
| `fast` | Building and testing larger changes, work with some judgment |
| `balanced` | Most planning and review |
| `deep` | Hard reasoning, high stakes |

Builders start cheap and move up only when their work fails; checkers start strong
and stay there: the Tester and strong reviewers make a cheap builder safe. The
Tester starts on `fast`: on `light` it needs about twice the steps, and every round
waits for it.

**Starting tier by role and size:**

| Role | S | M | L |
|---|---|---|---|
| Planner | fast | balanced | deep |
| Plan Reviewer | balanced | balanced | deep |
| Implementer | light | fast | fast |
| Tester | fast | fast | fast |
| Code Reviewer | balanced | balanced | deep |
| QA Reviewer | balanced | balanced | balanced |
| Security Reviewer | balanced | balanced | deep |
| UX Reviewer | balanced | balanced | balanced |

The size is the item's, except for the Implementer and a package's Tester
(`Scope: <WP>`): the package's **Size** from the plan's Work packages table, so a
small package in a larger item starts on `light`. The final check and a
single-package Tester use the item's size.

**Adjustments:**
- Security-sensitive: the Security Reviewer is always `deep`, and the Planner is at least `balanced`.
- Assurance (see **Quality tools and assurance**): at `low`, the Security Reviewer is at most `balanced`; at `high`, always `deep`.
- The Planner is never `light`. The Plan, QA, Code, Security and UX Reviewers are
  never below `balanced`: a weak adversary raises false findings, each costing a round.
- A reviewer keeps its round-1 tier in later rounds, so the bar does not move.
- Each `status.md` log line names tier and model, and `usage.md` each run's cost,
  for tuning.

**Escalation.** A role moves one step up the ladder `light` → `fast` →
`balanced` → `deep` only when its own work failed, and moves down only for a
mechanical fix round. Its current tier is the one in its last `status.md` log line.
- **Implementer**, per package: up only when the package failed on substance (a
  failing acceptance criterion, wrong behaviour, a design or security flaw); never
  for test-only findings, docs or small mechanical fixes, however many rounds. A
  package without findings is not re-dispatched.
- **Mechanical fix rounds** run on `light`, whatever the package's tier: all of the
  package's findings small and mechanical (a rename, a missing null check, a test
  name). If it fails, the package returns to its earlier tier and escalates from there.
- **Tester**: `fast` every round; QA findings on its tests come back to it on the
  same tier. `RESULT: FAIL` is the code failing, not the Tester; only **Any role**
  moves it up.
- **Planner**: up when the review's blockers show a misunderstanding of the problem
  or design, not slips.
- **Any role**: a crashed or empty run is retried once on the same tier; a confused
  or malformed one, one step up. A `BLOCKED` saying the task is beyond the agent is
  dispatched once more one step up before it goes to the user.

## Coding standards and documentation

- Whoever writes or reviews code follows `.editorconfig`, `docs/coding-guidelines.md`
  and the project's other conventions (`CONTRIBUTING.md`, style guides, lint and
  formatter configs, `AGENTS.md`, `CLAUDE.md`, `.github/copilot-instructions.md`).
  The Planner lists those that apply; the Code Reviewer checks them.
- A library API the code does not already use the same way, or whose behaviour
  depends on the version, is checked in its documentation for the pinned version:
  the installed package first (type definitions, docs, README, changelog), else the
  official docs on the web. Reviewers look one up only when in doubt. An agent that
  cannot reach it says so.

## Names

Every member has a name (`aliases` in the config). The first time the PM starts in a
repo (`preflight`: `Not chosen yet`), before any work, it asks for a theme (Greek
myth, jazz musicians, famous ships, ...) or to keep the defaults; it renames the
team whenever asked.
- **Choosing:** one name per role in the theme, all different, easy to say, ≤ 30
  letters (spaces, `.`, `'`, `-` fine), fitting the role where the theme allows (a
  watchful figure for security, a maker for the Implementer); fictional or real,
  living people included. One message: a table of role, name, a few words why; the
  user may change any.
- **Applying**, on the user's OK:
  `node .devagents/devagents.mjs names --theme "<theme>" pm=<Name> planner=<Name> plan-reviewer=<Name> implementer=<Name> tester=<Name> qa-reviewer=<Name> code-reviewer=<Name> security-reviewer=<Name> ux-reviewer=<Name>`
  saves them and re-renders the agent files; the PM offers to commit, as at the
  start of a session. `names --keep` keeps the defaults, so nobody asks again.
- New helpers in `preflight` are asked about in the same message (**Helpers**).
- New names apply at once: the running session keeps its old prompts, so the PM uses
  the new names itself and gives every agent its name on the brief's `Name:` line,
  which wins over its prompt. The next session loads the new files. Earlier records
  keep the names they were written with.

## Notifications

The PM can tell the user on Slack, Microsoft Teams or any webhook when an item needs
them. One-way: the user answers in the session (or from the Claude app through
Claude Code's Remote Control).

| Event | When |
|---|---|
| `input` | An item is in `needs-input`: a question, a tie-break, a dispute, an escalation, the round limit |
| `approval` | A plan is at the approval gate |
| `blocked` | A failure the PM brings to the user: an agent failed twice, or one of its own steps failed |
| `done` | An item is committed on its branch |

- `node .devagents/devagents.mjs notify --event <event> --item <key> "<one line>"`
  sends `<repo> · <ID> <Name>: <event> — <line>`: never code, secrets or personal
  data, since it leaves the machine.
- Channels are per developer, never committed: `.devagents/local.json`
  (git-ignored) via `notify --set slack=<url>` (or `teams=`, `webhook=`), or
  `DEVAGENTS_NOTIFY_SLACK`, `_TEAMS`, `_WEBHOOK`. A webhook URL is a secret: the user
  sets it, and the tool never prints it in full. `"events"` there narrows what is sent.
- No channel: `notify` does nothing; a failed send never stops the work.
  `notify --test` sends a test; `notify` and `preflight` show the setup.

## Helpers

The project's own agents, skills, prompt files, MCP servers and tools can help.
`node .devagents/devagents.mjs helpers` lists them, with the user's decisions and,
for new ones, suggested members. Only what the repo has counts (tracked or new, not
git-ignored), so every clone renders the same team.

| Kind | Where it is found | How a team member uses it |
|---|---|---|
| agent | `.claude/agents/`, `.github/agents/` (not the team's own) | asks it through the PM (below); Claude Code and Copilot only |
| skill | `.claude/skills/`, `.agents/skills/`, `.github/skills/`, `.claude/commands/` | reads its file and follows it |
| prompt | `.github/prompts/*.prompt.md` | reads its file and follows it |
| MCP server | `.mcp.json` under `mcpServers` (Claude Code, Copilot CLI; Copilot CLI also takes server names at the top), `.github/mcp.json` (Copilot CLI), `.vscode/mcp.json` under `servers` (VS Code), `.codex/config.toml` (Codex) | calls its tools |
| tool | scanner configs (semgrep, gitleaks, trivy, osv-scanner, bandit, checkov, hadolint), and `package.json` scripts or make targets named like audit, security, scan, secret, vuln or licence (none that fix, update or generate) | runs it |

- **The user decides once per helper**, with the names question at first start,
  later when `preflight` shows new ones: each with what it does and suggested
  members, suggestion first; the user may change any. Then
  `node .devagents/devagents.mjs helpers <helper>=<role>,<role> ...` (`<helper>=`
  declines) saves `helpers` and re-renders the agent files: each member's prompt
  lists its helpers; an approved MCP server's tools join its tool list.
- **MCP servers that can write** (issues, comments, data): only instructions would
  hold reviewers back, so the PM proposes such a server's reading tools one by one,
  as `mcp:<server>/<tool>=<role>`. Names come from the PM's tool list (Claude Code:
  `mcp__<server>__<tool>`, once the user approved the server), else the server's
  docs or the user. Grant by the server's own name: Claude Code writes characters
  other than letters, digits, `_` and `-` as `_` (`get.file` →
  `mcp__<server>__get_file`), and `helpers` writes that form. A misspelt name grants
  nothing. A whole server (`mcp:<server>=<role>`) only when all its tools just read
  (a scanner, a docs server); `helpers` warns when a reviewer gets one.
- **Platforms.** Claude Code and Copilot give each member only its MCP tools. On
  Codex the runner switches off each server in `.codex/config.toml` a member was not
  given, and limits a given one to its granted tools (`~/.codex/config.toml` is not
  limited). VS Code reads `.vscode/mcp.json` (`.mcp.json` only behind an experimental
  setting); Copilot CLI reads `.mcp.json` and `.github/mcp.json`.
- **In the session that changed them**, agents keep their old prompts: `helpers`
  prints a `Helpers:` line per member (`none` if it lost all), which the PM puts in
  each brief for the rest of the session. MCP tools arrive next session. Platforms
  load a project's MCP servers only once the user trusts the project; Claude Code
  also asks about each server in `.mcp.json`.
- **A helper never overrides a role.** The role's limits hold for everything done
  through it (a reviewer changes no files, whatever the helper could do). Through a
  helper nobody commits, pushes, or posts, sends or changes anything outside the
  worktree (issues, comments, messages, shared data; a tool's own caches are fine).
  Members use no other project skills, prompts or MCP servers, check what a helper
  says, and stay responsible for their work; decisions stay with the user.
- **Helper agents run through the PM.** A member ends its final response with
  `## Help request` (the agent, a self-contained question, why), stopping there if
  it needs the answer to finish. The PM dispatches the helper fresh with only the
  question and "answer without modifying files", saves the answer to `<item>/help/`,
  and dispatches the member again with that file. Helper runs are logged in
  `status.md`, not `usage.md` (no usage hook in their files). Codex cannot run them.
- **Something the scan missed:** a member ends with `## Helper to add`; the PM tells
  the user. It counts once the repo has it where `helpers` looks (agent, skill or
  prompt file, MCP config, scanner config, or a check script in `package.json` or the
  Makefile). `helpers <helper>=` forgets a gone one, so it is new if it returns.

## Cost estimate

At the approval gate: `node .devagents/devagents.mjs estimate <key> --platform
<platform>`. It counts the runs the plan implies (planning, implementers per
package, tester, QA, code, security, and UX if UI-facing), with each run's tier from
**Models**, priced at the configured models. Later rounds count at a share of the
first (`laterRounds`): the fixing Implementers, one Tester, QA, code and UX, and
Security once at the end. Output: an expected figure, a range from one round to the
round limit, and what earlier items of that size cost. Tune `estimates` in the config
with `report`. On Copilot: **AI credits** (1 credit = $0.01) at API rates; with Auto,
tokens only.

## Token usage and cost

Every run is recorded in `<item>/usage.md`: name, role, run (package and round),
tier, model, input, cache and output tokens, and estimated USD at the configured
prices.

| Platform | Recorded by | What is recorded |
|---|---|---|
| Claude Code | a hook in the agent files | every team member's run, and the PM's own turns, split across the items each turn worked on |
| OpenAI Codex | the runner (`node .devagents/devagents.mjs codex`) | every team member's run; not the PM's own session |
| GitHub Copilot CLI | the kit, from Copilot CLI's session logs (`~/.copilot/session-state/`), whenever `usage`, `report` or `estimate` runs | every run and the PM's turns, in the AI credits Copilot billed; Copilot Chat in VS Code keeps no such log: only the model, in the `status.md` log |

- Rows are appended automatically; nobody edits them. The hook and runner read the
  brief's `Role`, `Tier`, `Key`, `Round` and `Work package` lines: always write them.
- At commit, `node .devagents/devagents.mjs usage <key> --write` puts totals atop
  `usage.md`; the PM copies the total to the tracker's **Cost** column. `report`
  shows usage and cost by item, agent and model.
- Claude Code and Codex costs are estimates at API list prices (subscriptions bill
  differently). Copilot rows are what Copilot billed; included credits go first.

## Long PM sessions

Every PM step re-reads its whole conversation, so cost grows with the session (one
two-day session averaged 400k tokens a step). The item files are the record, so the
conversation can be cut short without loss:
- **Claude Code compacts it at 300k tokens** when the PM is started with
  `claude --agent pm --autocompact 300k` (not near its 1M window). After a compaction a hook adds
  `node .devagents/devagents.mjs preflight --items` (each in-flight item's phase,
  rounds, packages, last log lines). Without it (other platforms, older installs)
  the PM runs it before acting, and trusts it over the summary.
- **Everything is written down as it happens:** decisions under **Decisions**,
  dispatches and results in the log.
- **Fresh session between items:** once an item is committed and no member runs, the
  PM writes a handoff line (the next step) in each in-flight item's log and offers a
  fresh session (`/clear` on Claude Code, free; a new chat on Copilot; a new session
  on Codex), which starts from `preflight`. Never while an agent runs: results reach
  only the session that started it.

## Parallel work

At most **8 agents at the same time**, across all items: a ceiling, not
a target. Only what these rules allow runs in parallel; nothing is split to fill slots.

**Across items** (each in its own worktree):
- Planning and plan review never touch code: always parallel.
- Two items build in parallel only if their **Work packages** share no files and
  neither depends on the other; otherwise the later one waits in `blocked` (reason in
  `status.md`), and the user chooses: wait for the earlier merge, or stack it on the
  earlier item's branch.
- Each item has its own approval gate: plans may be shown together, but each is
  approved explicitly.

**Within an item**, **work packages** (`WP1`, ...) own disjoint files, with
dependencies:
- Independent packages build in parallel, one Implementer each, in one worktree.
  Shared files (manifests, lockfiles, central registries and route tables,
  migrations, generated code) belong to exactly one package.
- Implementers touch only their package's files and run only scoped commands: no
  repo-wide formatters or `--fix` runs.
- Each agent re-reads context, so several packages only when each is substantial
  (roughly 150+ changed lines, or a separate subsystem); size S: always one.
- Testing follows packages (see **Testing**); Testers that clash over a shared
  resource (port in use, database locked) are re-run one at a time.
- Reviews in parallel: one QA (breaks only in the scratch copy; the PM snapshots the
  changes before and checks them after), one Security, always on the whole diff, in
  round 1 and at the end (**Later rounds**), one UX if UI-facing, one Code (or, for
  an L item with several packages, one per package).

## Phases

```
                                consensus
intake ─► planning ─► plan-review ─┬─► awaiting-approval ─► implementing ─► testing ─► review ─┬─► done
              ▲                    │            │                  ▲                           │
              └── CHANGES_REQUESTED┘            │ user changes     └─ any FAIL / CHANGES ──────┘
              │  (deadlock ─► user decides)     │
              └─────────────────────────────────┘
                         (past the round limit ─► escalated)
```

1. **intake** (PM): read the project's documents on the area
   (`node .devagents/devagents.mjs docs`); settle with the user only what they and the
   code leave open (**Problems and escalation**); agree name and type, estimate size,
   assign the ID; `git worktree add -b devagents/<key> .work/<key> HEAD`,
   record `base_commit`; in the worktree, create `request.md` (relevant documents
   under **Project context**), `status.md` and the tracker row.
2. **planning** (Planner): `<item>/plan.md`.
3. **plan-review** (Plan Reviewer, **Plan consensus**): `CHANGES_REQUESTED` →
   planning, `plan_round += 1`; deadlock → `needs-input`.
4. **awaiting-approval** (PM ↔ user): **mandatory gate**, only with consensus: the
   agreed plan's summary, then explicit approval. Changes the user asks for go back
   to planning and consensus, not counted toward the round limit.
5. **implementing** (Implementers): after dependency setup (**Worktree setup**), in
   dependency order, independent packages in parallel.
6. **testing** (Tester, **Testing**): `FAIL` → implementing, `impl_round += 1`.
7. **review** (QA, Code **and** Security, plus UX if UI-facing, in parallel, after
   the PM's snapshot, which updates the scratch copy; items before 0.21 may show
   `qa`): QA traces plan and tests and proves them in the scratch copy; Code and
   Security review the diff (Security again at the end of later rounds); UX the UI,
   in the running app. Any `CHANGES_REQUESTED` → implementing with **all** the
   round's findings, each routed to the package owning its file (test-only findings
   to the Tester), `impl_round += 1`; testing and reviews then rerun on what changed
   (**Later rounds**).
8. **done** (PM): commit in the worktree, report, remove the worktree.

**Round limit:** if `plan_round` would exceed **3**, or a build round would exceed
**3 counted rounds**, the PM sets `escalated`, stops and asks the user how to go on
(plan loop: the user decides each open finding or allows more rounds). A build round
changing only tests and documents (implementation notes, a README, the security
model's text) does not count: a test gap is closed, not argued. Past the limit up to
**2** such rounds go ahead; one that would change production code, configuration or
build files stops. `snapshot save <key> --round <N>` prints the count; a round
without a snapshot (failed at testing) counts.

**Other phases:** `needs-input` (awaiting the user's answer), `blocked` (awaiting
another item), `parked` (paused by the user; resumable), `cancelled` (dropped, never
resumed or reused; its folder is committed on its branch, keeping what was tried and
why it stopped).

## Problems and escalation

Only the PM talks to the user, so every problem must reach the PM explicitly.

**Agents escalate.** An agent that cannot finish, or needs a decision only the user
can make, stops and ends its final response with `## Escalation`: type, what
happened, what is needed, options (recommendation first), the state left behind.

| Type | Meaning |
|---|---|
| `QUESTION` | Ambiguous requirement, or the request, plan and code disagree |
| `BLOCKED` | Cannot continue: missing tool, dependency, credential or service; broken environment; a change needed outside the agent's scope; a plan step that cannot work |
| `DISPUTE` | A code or security finding the agent must fix is wrong, with evidence (plan findings are disputed in the Planner's response file) |
| `DEVIATION` | Departing from the plan in a way that changes scope, public API, dependencies or architecture |

**A question for the user** is one whose answer changes what the user gets
(behaviour, scope, an acceptance criterion), or acceptance criteria that contradict
each other or a documented rule. Technical choices within the request, the
architecture and the code's conventions belong to the agent: the Planner makes them
and names the notable ones in the plan's **Summary**, which the user sees at the
approval gate.

Two problems of normal work are a line in the final response instead:
- **`Findings disputed:`** (Implementer): findings it believes wrong, with evidence
  (**Disputes** below).
- **`Pre-existing failures:`** (Tester): checks that failed before this change, with
  evidence; they do not make the result FAIL. The PM asks the user once per failure
  (accept as a known issue, or fix in this item) and records the answer under
  **Decisions**; later the Tester lists it as known (`known: D<n>`), and the PM does
  not ask again.

No agent resolves a problem by guessing, weakening tests, adding unplanned
dependencies, or editing outside its scope.

**The PM resolves or asks.**
- **It decides itself** whenever the request, the plan, this workflow, the project's
  documents or an earlier **Decision** settles the question, and logs what and why.
  It checks them before asking anything, and never asks what the user has answered.
- **At intake** it asks only what changes behaviour, scope or the acceptance criteria
  and that the request, documents and code do not answer. Obvious defaults go into
  the proposed acceptance criteria as stated assumptions, confirmed in the same
  reply as the name and criteria.
- **Otherwise it asks**: phase `needs-input` (other items keep running), the answer
  under **Decisions** in `status.md`, the agent re-dispatched with it. Not a new round.
- **Disputes** in the build loop do not go to the user when they come in. A finding
  the Implementer disputes, or marks `no change needed`, goes back to the reviewer
  who raised it at its next review: it runs again anyway (it asked for changes),
  reads the evidence in `implementation/<WP>.md`, and marks the finding `withdrawn`
  or `maintained`, answering the evidence. A `DISPUTE` escalation goes the same way.
  Only a finding `maintained` after a dispute goes to the user (`needs-input`), like a
  plan tie-break: both positions neutrally, at similar length, with the paths of the
  review and the implementation notes, no side recommended. Options: **fix it** (the
  reviewer is right), **drop it** (the implementer is right; under **Decisions**,
  never raised again), or the user's own instruction. A round whose only blocking
  findings the user dropped counts as approved.
- **Deadlocked plan findings** go to the user (**Plan consensus**); **scope-changing
  deviations** go through the user, then a plan revision.

**Declined setup items.** When the user skips the security model, architecture,
coding guidelines, UX guidelines, UX preview or quality tools, the PM records it:
`node .devagents/devagents.mjs decline <setup>` (`security-model`, `architecture`,
`coding-guidelines`, `ux-guidelines`, `ux-preview`, `quality-tools`; stored as
`declined` in `.devagents/config.json`). `preflight` then shows it as declined, and
the PM does not offer it again unless the user asks (`decline --undo <setup>`). The
work goes on without it, as its section says.

**Failed runs** (crash, timeout, empty or malformed output, a non-zero exit) are
retried once, one tier higher if the output was confused; a second failure goes to
the user with the error. A failed step of the PM's own (worktree, dependency setup,
commit): it shows the error and asks, never forces.

**Decisions are requirements.** Every agent reads **Decisions** in `status.md`
before starting. Nobody re-raises what the user has decided.

## Testing

One work package: one Tester writes the missing tests, verifies every acceptance
criterion, and runs the full suite, lint, typecheck and build. Several packages, in
round 1:
1. **One Tester per package** (`Scope: <WP>`), once it and its dependencies are
   implemented: that package's **Tests** rows, its test files only, its tests only
   (no full suite, so parallel Testers do not clash); writes `test-report-<WP>.md`.
2. **One final check** (`Scope: final`) once all pass: the full suite, lint,
   typecheck and build, and the acceptance criteria spanning packages (tests for
   them may be added); writes `test-report.md`.

`FAIL` → the next round: findings on a package's code to its Implementer, on tests
to the Tester.

**Later rounds:** one Tester for the whole item, no `Scope:`, once the fixes are in.
It re-verifies the acceptance criteria that failed or whose code or tests changed,
and runs what the change since the last round needs
(`node .devagents/devagents.mjs changes <key> --since <N-1> --summary`):
- production code, or test, build, lint or CI configuration: everything, as round 1
- only test code: the whole unit suite (helpers and fixtures reach other tests),
  lint, typecheck; of the end-to-end tests the changed specs, or all if a shared
  end-to-end helper or fixture changed
- only documents: their formatter or linter, if any

An item is committed only after a full run, end-to-end included, after its last
production or configuration change, plus a run of every end-to-end spec changed since.

**The Tester breaks its own tests** before reporting (single or final scope): one
deliberate break per acceptance criterion and per logic row of the **Tests** table
(at most 3/5/8 for S/M/L; later rounds the changed code only), with `breaks` in the
scratch copy, and a test for each survivor in the same round, so QA's breaks rarely
cost a round. QA still breaks the code itself.

## Later rounds

A reviewer's first round is its full review; later rounds look only at what changed:
- Before each round's reviews the PM runs `node .devagents/devagents.mjs snapshot
  save <key> --round <N>`; QA, code, security and UX all see that state.
- A reviewer runs `node .devagents/devagents.mjs changes <key> --since <M>` (`M` = its
  own last review's round): the diff from what it reviewed to the worktree now. It
  verifies its earlier findings and reviews that diff, reading unchanged code only
  where a change affects it; without a round-`M` snapshot, the whole change.
- `node .devagents/devagents.mjs rerun <key> --round <N>` says who runs again: a
  reviewer that asked for changes, always; one that approved, when anything changed
  since, except Code and UX when only test code changed (QA proves tests every
  round). UX only if UI-facing.
- **Security reviews in round 1 and at the end.** Later it waits (`rerun`: `later`),
  even after its own `CHANGES_REQUESTED`: once the Tester passes and QA, Code and UX
  approve, the PM runs `rerun` again and Security checks everything changed since its
  last review, tests and fixtures included (secrets, say), in the same round. If it
  asks for changes, the next round follows and it checks again at its end.
- QA after a test-only round re-runs its survivors and reads the new tests, nothing
  more; after a production change it breaks only changed code, at most once per
  changed function with logic.
- Implementers work only on packages with findings; the Tester as in **Testing**.
- Plan rounds likewise (**Planning converges**): the Planner changes only what the
  findings concern.

## Worktree setup

Before the first build round the PM installs dependencies in the worktree with the
project's usual command, and the same in the scratch copy (where QA's tools run):
`node .devagents/devagents.mjs scratch <key> --install -- <command>`. When a manifest
or lockfile changes, `snapshot save` says so and the PM installs in the scratch copy
again. README and CI first, then the lockfiles:

| Found | Command |
|---|---|
| `package-lock.json` | `npm ci` |
| `pnpm-lock.yaml` | `pnpm install --frozen-lockfile` |
| `yarn.lock` | `yarn install --immutable` (Yarn 2+) or `yarn install --frozen-lockfile` |
| `uv.lock` | `uv sync` |
| `poetry.lock` | `poetry install` |
| `*.sln` / `*.csproj` | `dotnet restore` |
| `go.mod`, `Cargo.toml` | nothing; the build fetches dependencies |

Untracked local files the tests need (`.env.test`, local certificates): ask before
copying them in. Never copy secrets without permission.

## Verdict format (all reviewers and Tester)

The **first line** of a review or test report section is exactly one of:

```
VERDICT: APPROVED
VERDICT: CHANGES_REQUESTED
RESULT: PASS
RESULT: FAIL
```

Findings use this table, so the PM and Implementer can act on them mechanically:

```
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C1 | blocker  | src/auth.ts:42 | ... | ... |
```

Locations are paths inside the worktree, relative to it.

Severity: `blocker` and `major` must be fixed; `minor` is fixed or justified in the
implementation notes; `nit` is optional. A reviewer returns `CHANGES_REQUESTED` if
and only if there is at least one `blocker` or `major` finding.

**The blocking bar.** A finding is `blocker` or `major` only if leaving it costs
something concrete: an acceptance criterion not met; a wrong or unsafe result for
an input you can name; existing behaviour, a public API or stored data broken; a
security weakness with a plausible path to exploitation; an accessibility
failure, or a state the user cannot recover from, on a changed screen; or an
undeclared departure from a `must` rule, the architecture or a UX guideline.
Where your role's instructions list what is `major`, that list holds. Everything
else is `minor` or `nit` and never blocks: naming, structure, duplication,
readability, comments and docs, an approach you would prefer, an extra test for
an unlikely case. A blocking finding costs the item a whole round of fixes,
tests and reviews, so:
- one finding per cause: merge findings that one change fixes
- between two severities, choose the lower, except for a security weakness that
  could plausibly be exploited
- no finding when the required change is what the code already does

The Security Reviewer maps its ratings: critical/high → `blocker`, medium →
`major`, low → `minor`, info → `nit`. Only the user, via the PM, may accept a
medium finding without a fix, recorded in `status.md` as an accepted risk.

## Isolation and independence

No context passes between agents: every judgment is one agent's own, from evidence.
- **Fresh agents:** every dispatch starts empty. Nobody continues, resumes or
  messages a finished agent, or runs a member inside another's context (no forks). A
  retry is a new dispatch.
- **Briefs carry facts only:** the template fields and a short task naming finding
  IDs; never another agent's reasoning, verdict or summary, the PM's opinion, or
  another item's content. The only free text: the user's words in `User decision:`.
- **Files are the only channel:** plan, reviews, the Planner's responses,
  implementation notes, test reports, **Decisions**. Each review is saved verbatim
  to its brief's `Save as:` file (by the hook on Claude Code, the runner on Codex,
  else the PM); nobody edits, summarizes or annotates them.
- **Agents stay in their lane:** their own item only; never `.devagents/runs/`,
  other agents' transcripts or logs, or other items; they never start agents.
- **Nobody reviews their own output.** Reviewers judge files on disk and the diff,
  not the author's summary, and form their own view first.
- **Reviewers modify no files**; a one-line fix is still a finding. Only QA's
  deliberate breaks change code, in the scratch copy, undone at once, verified by the PM.
- **The PM takes no sides:** disagreements are argued in files and settled by the user.

## Git rules

- The PM creates the worktree and branch at intake; nobody else creates, switches or
  removes branches or worktrees.
- Nobody pushes, force-pushes, rebases, resets, or rewrites history.
- Diffs are against `base_commit` in `status.md`, not a branch (the user's may move):
  `git -C <wt> diff <base_commit>` plus untracked files from
  `git -C <wt> status --porcelain`.
- The PM commits, in the worktree, only after all gates pass:

  ```
  <type>(<scope>): <Name> [<ID>]

  <2-4 lines: what changed and why>

  Work-Item: <ID> <Name>
  Reviews: plan r<N> APPROVED, qa r<N> APPROVED, code r<N> APPROVED, security r<N> APPROVED, ux r<N> APPROVED, tests PASS
  ```

  `ux r<N>` only if UI-facing. `<type>`: `FEAT` → `feat`, `BUG` → `fix`, `CHORE` →
  `chore`.
- Stage only (never `git add -A`) the item folder `docs/work/<key>/`,
  `docs/work/TRACKER.md`, and the files listed in `implementation/*.md` and
  `test-report.md`: code and record in **one commit**, so `git log` and
  `docs/work/<key>/` agree.
- Then `git worktree remove .work/<key>`; branch and commit remain. If git
  refuses (untracked files), tell the user; never force.
- **Cancelled items** keep their record:
  1. The PM sets the plan's **Status** to `cancelled <YYYY-MM-DD>`, and the phase in
     `status.md` and the tracker row to `cancelled`.
  2. It runs `node .devagents/devagents.mjs usage <key> --write` and puts the total
     in the tracker row's **Cost** column.
  3. It commits the item folder and tracker on the branch as
     `chore: cancel <Name> [<ID>]`, and removes the scratch copy:
     `node .devagents/devagents.mjs scratch <key> --remove`.
  4. It asks the user whether to merge that record, keep the branch, or remove the
     worktree and delete the branch.
- Never put secrets in the item folder: it is committed.

# Coding guidelines: <project>

<!-- How this project's code is written, beyond what its tools enforce. Every DevAgents
     plan, implementation and code review works from this file, so keep it current: a
     change that adds a convention or departs from a rule updates this file in the same
     work item. Every rule has an ID, so plans and reviews can refer to it, and a source:
     the user, a document, a tool's config, the code (with an example file), or the
     language's conventions. Every rule has a level: `must` (a change that breaks it
     without the plan saying so goes back for a fix) or `should` (a slip is noted, and
     fixed when the change goes back anyway). Keep `must` for rules whose breach causes
     bugs, security or data problems, or spreads through the code. Write rules a
     reviewer can check in a diff, not an essay.
     Leave out what a tool already enforces (list the tool instead), the stack and the
     patterns (they are in docs/architecture.md), and the parts that do not apply. Where this
     file is silent, the team follows the code as it is, then the language's mainstream
     conventions. -->

**Last updated:** <date>, by <work item ID or person>

## Tools
<!-- What the tools enforce, so the rules below need not: formatter, linter, type checker,
     analyzers and compiler warnings, per language. New code passes them; a rule is never
     switched off in code without a comment saying why. -->
| Language | Tool | Config | Check command (read-only) |
|----------|------|--------|---------------------------|
| | | | |

## Naming
<!-- Files, modules, types, functions, variables, constants, tests; domain terms and the
     words the code uses for them; abbreviations. -->
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG1 | | must \| should | |

## Structure
<!-- Where new code goes, size of files and functions, one responsibility per module,
     public versus internal API, shared helpers to reuse instead of copying (with paths). -->
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG10 | | must \| should | |

## Errors and logging
<!-- Exceptions or result values, what is caught where, error types and messages, retries,
     what is logged at which level, and what is never logged (secrets, personal data). -->
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG20 | | must \| should | |

## Data, types and state
<!-- Null and missing values, typing strictness, immutability, shared and global state,
     async and concurrency, resource cleanup, units (money, time zones). -->
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG30 | | must \| should | |

## Dependencies
<!-- When a new dependency is acceptable, how versions are pinned, wrappers around
     third-party APIs, the standard library first. -->
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG40 | | must \| should | |

## Tests
<!-- Framework, where tests live and how they are named, their structure, fakes and mocks,
     test data, what every change tests (and at which level). -->
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG50 | | must \| should | |

## Comments and documentation
<!-- When to comment and how, doc comments on public API, README and changelog updates. -->
| ID | Rule | Level | Source |
|----|------|-------|--------|
| CG60 | | must \| should | |

## Open questions
<!-- What the user, the documents and the code left open, or where they disagree. -->

## Revisions
<!-- <date> <work item ID>: what changed -->

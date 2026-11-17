# Plan: <ID> <Name>

<!-- Delete every comment in this template. Every agent reads the plan in every round:
     no restating the request, no code the implementer can write. A section that does
     not apply is one line ("None: <why>"); delete Split unless the item is size L. -->

| | |
|---|---|
| **ID** | <ID> |
| **Type** | FEAT \| BUG \| CHORE |
| **Status** | draft |
| **Size** | S \| M \| L — <one line why> |
| **Security-sensitive** | yes \| no — <one line why> |
| **UI-facing** | yes \| no — <one line why> |
| **Opened** | <YYYY-MM-DD> |
| **Plan round** | <N> |

## Summary
<!-- What the PM shows the user at the approval gate, and the only part it reads: at most
     about 1 KB. The approach and why in 2-4 sentences; the notable technical choices and
     assumptions you made, one line each; one line per work package (what it builds, and
     which run in parallel); the security considerations in one line; and for a UI-facing
     item the UX considerations in one line. -->

## Context found
<!-- Relevant files, existing patterns, and conventions you will follow (with paths).
     The business rules and decisions from the project's documents that the plan relies on. -->

## Work packages
<!-- Each file belongs to exactly one package. Shared files (manifests, lockfiles,
     registries, migrations) go to one package. Use a single WP1 unless each package
     is substantial (~150+ changed lines or a separate subsystem). Size S: WP1 only.
     Size: the package's own size, by the same rules as the item's. It sets the model for
     the package's Implementer and Tester; the item's size sets it for everyone else. -->
| WP | Scope | Size | Files (exclusive) | Depends on |
|----|-------|------|-------------------|------------|
| WP1 | <what> | S \| M \| L | `path/a`, `path/b` | — |

## Steps
<!-- Ordered within each package, each small enough to verify. Name files and functions. -->
### WP1
1. **<step>** — files: `path/to/file` — what changes and why.

## Tests
<!-- Which unit/integration tests the Implementer writes, mapped to acceptance criteria. -->
| AC | Test | Type | WP |
|----|------|------|----|
| AC1 | `tests/...` — case | unit | WP1 |

## Security considerations
<!-- By ID from docs/security-model.md: the trust boundaries crossed (TB), the actors and
     permissions affected (AC), the threats that apply (T), and whether the
     document needs updating. Then inputs, auth, secrets, new dependencies, data
     exposure. "None" must be justified. -->

## UX considerations
<!-- UI-facing items only; "None: not UI-facing" otherwise. The screens and states the change
     adds or changes (loading, empty, error, success, disabled), the guidelines it follows by
     ID from docs/ux-guidelines.md (or .devagents/ux-defaults.md where there are none), keyboard
     use and accessibility, and the user-facing text. A new UI pattern or a departure from a
     guideline is declared in the Summary, with a step that updates the document. -->

## Risks and rollback

## Split
<!-- Size L only; delete the section otherwise. The same request as a sequence of S or M
     items, each complete and testable on its own: for each, a name, its acceptance
     criteria (from request.md), and what it builds. The user may approve the plan as one
     item, or split it: this plan is then trimmed to the first item. -->

## Out of scope

## Revisions
<!-- Round N: what changed in response to which review finding IDs. -->

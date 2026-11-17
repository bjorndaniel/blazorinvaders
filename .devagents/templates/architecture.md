# Architecture

<!-- The architecture patterns, frameworks and conventions this project uses, and why.
     Written by the Planner from the user's decisions (item CHORE-<NNN> Architecture),
     changed only by an item whose plan says so. Every choice has an ID; plans and
     reviews refer to them. Leave out the areas that do not apply. -->

Last updated: <YYYY-MM-DD> by <ID> <Name>

## The project
<!-- What it does, for whom, how large it is expected to grow, where it runs. Two to four sentences. -->

## Stack
| ID | Area | Choice | Version | Why |
|----|------|--------|---------|-----|
| A1 | Language and runtime | <e.g. TypeScript on Node.js> | <pinned> | <one line> |
| A2 | Web or API framework | | | |
| A3 | UI framework | | | |
| A4 | Data storage and access | <database, ORM or query builder, migrations> | | |
| A5 | Testing | <unit, integration, end to end> | | |
| A6 | Linting and formatting | | | |
| A7 | Build, CI and deployment | | | |

## Patterns
<!-- How the code is organised, each with an ID and the reason. E.g.
     "A10: Hexagonal: domain code in src/domain has no framework imports; adapters in
     src/adapters. Why: the domain outlives the web framework." -->
| ID | Pattern | Where it applies | Why |
|----|---------|------------------|-----|
| A10 | <architecture pattern> | | |
| A11 | <module structure> | | |

## Conventions
<!-- Validation, authentication, configuration and secrets, logging and errors, and
     anything else every change must follow. One line each, with an ID. -->
| ID | Convention |
|----|------------|
| A20 | |

## Decisions
<!-- One line per choice made or changed, newest last: date, ID, what, the alternatives
     considered, and who decided (the user, by item ID). -->

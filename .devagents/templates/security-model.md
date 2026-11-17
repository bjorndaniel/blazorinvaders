# Security model: <project>

<!-- The project's access model, trust boundaries and threat model, in one place.
     Every DevAgents plan and security review works from this file, so keep it
     current: a change that adds or alters a role, permission, boundary or threat
     updates this file in the same work item. Give everything an ID, so plans and
     reviews can refer to it. -->

**Last updated:** <date>, by <work item ID or person>

## Scope and assets
<!-- What the system is, how it is deployed, and what is worth protecting:
     data (personal, financial, credentials), money, availability, reputation. -->
| ID | Asset | Why it matters |
|----|-------|----------------|
| AS1 | | |

## Access model
<!-- Who and what can act on the system, how they are authenticated, and what each
     may do. Include services and jobs, not only people. -->
| ID | Actor | Authenticated by | May | May not |
|----|-------|------------------|-----|---------|
| AC1 | | | | |

**Rules:** <!-- e.g. "a customer sees only their own orders"; where checks are enforced -->

## Trust boundaries
<!-- Every place where data or control crosses from less trusted to more trusted:
     user input, HTTP requests, files, queues, third-party APIs, the database. -->
| ID | Boundary | What crosses it | Checked where, and how |
|----|----------|-----------------|------------------------|
| TB1 | | | |

## Threat model
<!-- What could go wrong at each boundary (spoofing, tampering, repudiation,
     information disclosure, denial of service, elevation of privilege), and what
     stops it. Status: mitigated | partly | open | accepted. -->
| ID | Threat | Boundary | Impact | Mitigation | Status |
|----|--------|----------|--------|------------|--------|
| T1 | | TB1 | | | |

## Assumptions and out of scope
<!-- What this model takes for granted (e.g. "TLS is terminated by the load
     balancer") and what it does not cover. -->

## Open questions
<!-- Unknowns to settle with the team. -->

## Revisions
<!-- <date> <work item ID>: what changed -->

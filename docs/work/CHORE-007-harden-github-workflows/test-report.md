## Round 1
RESULT: PASS

### Suite
| Check | Result |
|---|---|
| Static check (throwaway PyYAML script outside repo, vs base f28870e) | ALL OK |
| grep `inputs.` outside `env:` mappings | no hits |
| lint/typecheck/build/unit tests | not applicable (workflow YAML only; actionlint not installed) |

### ACs
- AC1 verified: no `${{` in any `run:`; every `inputs.` hit is an `env:` mapping; diff reviewed, each replaced expression maps to an env entry with the same value; variables quoted.
- AC2 verified: top-level `permissions == {contents: read}` in all three files; no job-level permissions.
- AC3 verified: YAML parses; triggers, inputs, runs-on/needs/if, step count, per-step name/id/uses/with/if equal to base; only `run`/`env` differ. Noted: Summary step in provision.yml no longer defines unused FUNC_HOSTNAME; `HOSTNAME="$SWA_HOSTNAME"` kept (fine).
- Not provable locally: real workflow_dispatch run against Azure.

### Tests added
0 (static check script kept outside the repo, per plan).

### Mutation / Breaks
Not run: no production code or unit test suite; change is workflow YAML.

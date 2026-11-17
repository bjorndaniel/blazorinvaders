# Test report: <ID> <Name>

<!-- Delete these comments. A table with no rows is one line: "None". In later rounds,
     list only what you re-checked; the rest keeps its earlier result. -->

## Round <N>
RESULT: PASS | FAIL

### Acceptance criteria
| AC | Verified by | Result |
|----|-------------|--------|
| AC1 | `tests/...` | pass / fail — evidence |

### Suite
| Check | Command | Result |
|-------|---------|--------|
| unit tests | `...` | pass (N tests) |
| lint | `...` | |
| typecheck / build | `...` | |

### Mutation
<!-- `mutate` on the changed production code (single or final scope), or why it did not run.
     Every survivor on a changed line that is left, with why it is equivalent. -->
| Mutant | File:line | Why it is equivalent |
|--------|-----------|----------------------|

### Breaks
<!-- Your own deliberate breaks (single or final scope): the table `breaks` printed for
     `<item>/test-breaks-r<N>.json`, after the tests you wrote for the ones that survived.
     A break still surviving: why no test can tell, or the finding it makes. -->
| # | Break | File:line | Result |
|---|-------|-----------|--------|

### Tests added
| File | Covers |
|------|--------|

### Failures
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|

### Pre-existing failures
<!-- Checks that already failed before this change, with the evidence. They do not
     make the result FAIL; the PM asks the user about them once. One the user already
     accepted under Decisions: "known: D<n>" as its evidence. -->
| Check | Evidence it failed before this change |
|-------|---------------------------------------|

VERDICT: APPROVED

# Code review: FEAT-002 Larger phone canvas text, round 2

**Reviewed:** changes since round 1 (`devagents.mjs changes FEAT-002-larger-phone-canvas-text --since 1`). The item folder was left out.
**Reviewer:** Saucer (code-reviewer)

## Earlier findings
Round 1 had no `blocker` or `major` findings. The round 1 minors and nits are all resolved:
- C1 resolved: `MUST_READ_SIZE` in effects.js and `MustReadSize` in SceneBuilder.cs now carry "must match" comments that point at each other.
- C2 resolved: `FitSize` and `HudSize` now share `ShrinkToFit`. The arithmetic is the same as before for both callers.
- C3 resolved: `gameCanvas` now sits after `viewHeight`, and `stop()` sets it to null.
- C4 resolved: the `CrowdedHudEngine()` helper replaces the two copies.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|
| C5 | minor | tests/BlazorInvaders.Tests/SceneBuilderTests.cs:784, :839 | The new tests use reflection in two places, and these are the first uses in the test project. One sets `GameEngine.LostALife` through its private setter. The other calls the private `SceneBuilder.ShrinkToFit`. CG53 says state is set up through `internal` members, never through reflection. CG14 says such test-only state is `internal`, with a doc comment. A rename would break both at run time, not at compile time. | Make `LostALife`'s setter `internal set` (with a CG14 "for tests" doc comment, or reach the state through real play). Make `ShrinkToFit` `internal static` and call it directly. Or justify the reflection in the implementation notes. |
| C6 | nit | tests/BlazorInvaders.Tests/SceneBuilderTests.cs:796-842 | The new tests do not separate arrange, act and assert with blank lines (CG52). | Add the blank lines. |

## Checked and fine
- `ShrinkToFit` gives the same results as the old code:
  - `FitSize` still calls it only when the size was enlarged.
  - `HudSize` still floors at `MustReadSize`.
  - The width check is unchanged.
- In effects.js, `MUST_READ_SIZE` is declared at module level before `addLabel` uses it. Behaviour is the same as the old literal 18.
- The UX14 text now says 18 px for floating labels, which matches `MUST_READ_SIZE`.
- The new tests cover these points, and each would fail if the code broke:
  - PERFECT sits below the enlarged hit line.
  - `_hitSize` does not carry over into the next `Build`.
  - FRENZY clears the lowered header.
  - `ShrinkToFit` never goes below the design size; the expected values (18, 26, 30) are correct by hand.
- `dotnet test tests/BlazorInvaders.Tests --nologo`: 337 passed.

## Notes for the PM
- The brief held only its template fields and the task. Nothing in it or in the files suggested a verdict.
- C5 comes from a `should` rule, so it does not block. The two reflection calls came with the tests the Tester added in round 2.

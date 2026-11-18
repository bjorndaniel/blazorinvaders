VERDICT: APPROVED

# Security review: FEAT-002 Larger phone canvas text, round 1

**Reviewed:** diff vs base_commit 840f5a4 + untracked files
**Reviewer:** Bunker (security-reviewer)

## Threat model
- The change stays inside the client. The new data is one float (`scale` = `canvas.clientWidth / viewWidth`) that the page's own JS (`src/wwwroot/js/game.js:109-110`) passes into the `[JSInvokable] Tick` (`src/Pages/Index.razor:129`). It has the same trust as the existing `timeStamp` and `input`: AC1 controls it, and it affects only AC1's own screen.
- `Scene.MinTextSize` (int, 0-60) goes back from .NET to `effects.js`. It is used only to build a canvas font string.
- No TB2/TB3 API, storage, network, dependency or CI change. TB5 (names drawn on the canvas) is unchanged: the text is still drawn with `fillText`. T5 still holds.
- No actor, permission, boundary or threat changes, so `docs/security-model.md` needs no update.

## Findings
| ID | Severity | Location | Problem | Required change |
|----|----------|----------|---------|-----------------|

None.

## Checked and fine
- **`scale` is bounded.** `PhoneFontSize` (`src/core/Rendering/SceneBuilder.cs`) ignores NaN, Infinity and values of 0 or below, and caps sizes at 60. A tampered `Tick` call from devtools can only change the player's own text size. `game.js` guards `viewWidth == 0`, so it never sends a non-finite value.
- **Width estimate is bounded.** `EstimateWidth` indexes `Advances` only for chars 32-126 and counts anything else as 1000, so there is no out-of-range index. Its cost is linear in the length of short, internally built strings. The high-score name comes from the server and is limited to `^[A-Z0-9]{1,3}$`.
- **Fit arithmetic is safe.** `FitSize`, `HudSize` and `Text` get the same or smaller sizes when the available space is zero or negative (`Math.Max(designSize, …)`). An empty string gives width 0, so there is no division by zero and no unbounded size.
- **No injection in `effects.js`.** The font string is built from `parseInt` of an internal constant font and the integer `textFloor`. The label clamp uses `measureText` only. No HTML sink is introduced.
- **The interop signature change is consistent.** `Tick` has a single caller (`game.js:110`), which passes three arguments to match the new signature.
- **No dependencies.** No package, manifest, lockfile, workflow or infrastructure file changed.
- **No secrets or prompt injection.** The item folder (`plan.md`, `request.md`, `status.md`, `test-report.md`, `test-breaks-r1.json`, `usage.md`, `implementation/WP1.md`, `reviews/*`) and the `docs/ux-guidelines.md` and `docs/work/TRACKER.md` edits contain no secrets and no text aimed at AI agents.
- **No sensitive data.** No logging and no PII handling were added.

No read-only scanners were run: the change adds no dependency, secret or server-side code for one to check.

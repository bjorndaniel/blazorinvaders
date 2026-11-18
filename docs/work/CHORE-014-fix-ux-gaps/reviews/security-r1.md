VERDICT: APPROVED

# Security review: CHORE-014 Fix the five UX gaps, round 1

## Threat model
- Entry points touched: the leaderboard page's call to TB3 (`GET /api/getleaderboard`). The URL and parameters are the same as before; only the failure branch changed. TB1 is touched because `index.html` changes (the viewport meta).
- Actors: anonymous players only. No access-model rule (AC) applies, since the change involves no accounts, permissions or writes.
- Threats that apply: T9 (internal details leaking through errors). The new error text is fixed and `ex.Message` goes only to the browser console, which matches the existing pattern at `src/Pages/Index.razor:218,275`. T16 (a stale or tampered offline bundle): `index.html` is a manifest asset, and its integrity hash is regenerated at publish, so nothing changes there.
- Data: no PII, credentials or money involved. The diff adds, removes or changes no actor, boundary or threat, so `docs/security-model.md` does not need an update.

## Findings
None.

## Checked and fine
- `src/Pages/Leaderboard.razor:29-33,93-98`: the error message is a static string, so no exception text reaches the DOM (T9, CG24, UX44). The `Try again` button sends the same fixed request as the tab buttons, one per user click. That is no new load path beyond what the tabs already allow. The `_daily == daily` guard still stops a stale response from overwriting the selected board. Leaderboard names are still rendered through Razor's default encoding, which this diff leaves unchanged.
- Console logging of `ex.Message`: an `HttpClient` exception message can hold at most the public Functions API URL and a status code, which are already visible in DevTools. No secrets or PII.
- `src/Components/SetHighScore.razor`: only static `id`, `role` and `aria-label` attributes were added. Input binding, validation and `maxlength` are unchanged.
- `src/wwwroot/index.html`: removing `maximum-scale` and `user-scalable=no` has no security effect.
- `src/wwwroot/css/app.css`: only focus outline rules changed.
- `src/core/Rendering/SceneBuilder.cs:124` and `tests/BlazorInvaders.Tests/SceneBuilderTests.cs`: a constant string chosen by a bool, plus a test. Nothing security-relevant.
- Dependencies, CI/CD, configuration and secrets: no manifest, lockfile, workflow or config file changed. No new packages.
- AI-agent content: the diff and the item folder contain no text that tries to instruct an agent.
- Item folder `docs/work/CHORE-014-fix-ux-gaps/`: I searched for credentials, keys and connection strings and found none.

## Notes for the PM
- No scanners were run: the project has no security helpers approved for this role, and the diff touches no dependencies.
- The brief held only the template fields and the task, so there was nothing to ignore.
- Files reviewed are in C:\Projects\bjorndaniel\blazorinvaders\.work\CHORE-014-fix-ux-gaps (diff against d2bf333).

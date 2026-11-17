VERDICT: APPROVED

# Security review: CHORE-006 Remove template leftovers, round 1

## Threat model
- `docs/security-model.md` does not exist, so I worked out the threat model from the code.
- The change deletes three files and moves one. It adds no entry point, input handling, dependency, permission or secret, and changes no CI file.
- The only trust boundary near it is the routing edge of Azure Static Web Apps (SWA), between the browser and the static files and `/api/*`. A change there could expose or reroute paths.
- No PII, credentials or privileges are handled by the changed files.

## Findings
None.

## Checked and fine
- **Root `SaveHighScore.cs` (deleted):** This was an old Functions template that echoed a name back. No project compiled it, and `deploy.yml:74` sets `api_location: ""`. The real function is `src/api/SaveHighScore.cs`, which is unchanged. The only remaining `SaveHighScore` references point at that file or at `src/Components/SetHighScore.razor`. Deleting dead code removes nothing that was reachable.
- **`src/wwwroot/routes.json` (deleted):** This is the legacy SWA routing format. SWA ignores it when `staticwebapp.config.json` is present. Its single rule (`/*` serves `/index.html` with status 200) is covered by `navigationFallback` in `src/wwwroot/staticwebapp.config.json`. That fallback still excludes `/api/*` and `/_framework/*`, and keeps the `no-cache` headers on the service worker. Routing and headers are unchanged. The file is also no longer published as a public static file.
- **`src/wwwroot/sample-data/weather.json` (deleted):** Sample data. Nothing outside `docs/work` references it.
- **`src/.editorconfig` moved to `.editorconfig`:** A 100% rename with identical content. It holds style and naming rules only, with no build or analyzer severities that could hide warnings. No prompt-injection text.
- **Dependencies, CI/CD, secrets:** No package manifest, lockfile or workflow changed. I searched the item folder for secret-like strings and found only "Token usage" headings in `usage.md`.
- **Scanners:** None needed. There is no code or dependency change to scan.

## Notes for the PM
The brief contained only its template fields and the task, nothing to ignore. No project helpers were used.

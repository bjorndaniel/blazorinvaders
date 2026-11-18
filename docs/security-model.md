# Security model: BlazorInvaders

**Last updated:** 2026-11-17, by CHORE-015

This model describes the code as it will be once CHORE-001 (.NET 10), CHORE-006 (template leftovers removed) and CHORE-007 (GitHub workflows hardened) have merged (D4).

## Scope and assets
BlazorInvaders is a Blazor WebAssembly (.NET 10) game and installable PWA, hosted on Azure Static Web Apps (Free). Its API is an Azure Functions app (isolated worker, `net10.0`, Functions v4, Windows consumption plan). The API keeps an all-time top 10 and a daily-challenge top 10 in Azure Table Storage: table `HighScores`, partitions `HighScore` and `Daily-yyyyMMdd`. The site and the API run on separate origins. GitHub Actions builds and deploys both, and provisions the Azure resources.

| ID | Asset | Why it matters |
|----|-------|----------------|
| AS1 | Leaderboard integrity (all-time and daily boards) | It is the game's only shared state. Forged or junk entries ruin it for every player. |
| AS2 | Player initials and scores | Each entry is 1-3 characters `[A-Z0-9]` plus a score, shown publicly. The app stores no other player data. Low sensitivity. |
| AS3 | Storage account key (`AzureWebJobsStorage` connection string) | It gives full control of the storage account, including the Functions runtime's own state, not only the `HighScores` table. |
| AS4 | Deployment credentials: `AZURE_CREDENTIALS` (service principal), `SWA_DEPLOYMENT_TOKEN`, `GH_PAT` | They control the Azure resources, the content of the live site and the repository's secrets. |
| AS5 | Availability and cost of the Azure resources | Anonymous traffic scales the consumption plan and the storage transactions, and the owner pays for both. |
| AS6 | Integrity of the served app bundle | The service worker caches the bundle for offline play, so a tampered bundle stays on players' devices until the next version replaces it. |

## Access model
| ID | Actor | Authenticated by | May | May not |
|----|-------|------------------|-----|---------|
| AC1 | Player (anonymous, in a browser) | nothing (no accounts) | play, read both boards, submit initials and a score through the game UI, and keep a personal best and queued scores in its own `localStorage` | change or delete any stored score, or read anything other than the boards |
| AC2 | Direct API caller (any internet client or script) | nothing. CORS limits browsers only, not other clients | call `getleaderboard` and `savehighscore` exactly as AC1 can. The API cannot tell AC1 and AC2 apart | the same limits as AC1 |
| AC3 | Owner / maintainer | GitHub account, Azure account | everything: code, workflows, secrets and Azure resources. Moderates scores directly in Table Storage, because the app has no admin function | none in the app |
| AC4 | Deploy workflow (`deploy.yml`, on push to `master` or manual dispatch) | GitHub secrets `SWA_DEPLOYMENT_TOKEN`, `AZURE_CREDENTIALS` | publish the static site and deploy the Function App | run on pull requests (the deploy job is skipped for `pull_request`) |
| AC5 | Provisioning workflows (`provision.yml`, `set-custom-domain.yml`, manual dispatch only) | `AZURE_CREDENTIALS`, `GH_PAT` | create Azure resources, set app settings and CORS, and write repository secrets | run unless someone with write access dispatches them |
| AC6 | Function App (service identity) | storage account key in its `AzureWebJobsStorage` app setting | create the `HighScores` table, add entities, and read partitions | (not enforced: the account key allows any operation on the account) |

**Rules:**
- R1: No endpoint requires authentication. Both use `AuthorizationLevel.Anonymous`. Everything the API stores is public.
- R2: The API only adds scores. No endpoint updates or deletes them. The server picks the partition (`HighScoreValidator.PartitionFor`) and the RowKey (a ranked key: inverted score plus a new GUID). Still add-only: no update, upsert or delete.
- R3: The server validates every submission in `src/api/HighScoreValidator.cs`. The client's filtering in `SetHighScore.razor` is there for the user experience only and is not a control.
- R4: Secrets live only in GitHub secrets and Function App settings, never in the repository. `FUNCTIONS_API_URL` is public, because it ships in the client's `appsettings.json`.
- R5: Only AC3 does moderation (removing bad entries), through Azure. A removed legacy (GUID-keyed) row stays on the boards of warm Function App instances until the app restarts, because the legacy top 10 is cached per instance.
- R6: Anyone with write access to the repository can dispatch AC5, and so in effect holds AC3's power over Azure and the repository's secrets.

## Trust boundaries
| ID | Boundary | What crosses it | Checked where, and how |
|----|----------|-----------------|------------------------|
| TB1 | Browser to Static Web App | static assets, service worker, offline cache | SWA serves them over TLS. `service-worker.published.js` caches only manifest assets, fetches them with `integrity` hashes, and does not cache API calls. `staticwebapp.config.json` sets no security headers. |
| TB2 | Internet to `POST /api/savehighscore` | JSON body (`name`, `score`, `mode`, `day`, `wave`), and the `X-Client-IP` and `X-Forwarded-For` headers | `HighScoreValidator`: the body is at most 1 KiB (`ReadBoundedAsync`), and a JSON parse error returns 400. The name is trimmed, upper-cased and matched against `^[A-Z0-9]{1,3}$`. The score must be between 1 and 999,999 and a multiple of 10, `wave` is required, and the score must not exceed the most a game could earn by that wave (doubled for daily games). `mode` must be `daily` or empty, and `day` must be today or yesterday (UTC). `ScoreRateLimiter` allows 20 posts per client per hour (429 past that), keyed on a salted HMAC of the client IP from `X-Client-IP` (set by the App Service front end; trusted, and checked after deployment), else the rightmost `X-Forwarded-For` entry. Nothing proves the score came from a real game. |
| TB3 | Internet to `GET /api/getleaderboard` | query parameters `mode`, `day` | `TryGetLeaderboardPartition`: `mode` must be `daily` or empty, and `day` must match `^[0-9]{8}$` and be a valid date |
| TB4 | Function App to Table Storage | entities and the partition filter | an account-key connection string. The filters (partition, and RowKey range bounds) are built with `TableClient.CreateQueryFilter` (parameterized) from constants and a partition that has already been validated. |
| TB5 | API response to other players' screens | stored names and scores | names were restricted at TB2, Razor encodes the output (`Leaderboard.razor`), and the canvas draws text with `fillText` (`game.js`) |
| TB6 | `localStorage` to client | `personalBest`, daily best, `pendingScores` queue, `muted` | the JSON read is wrapped in try/catch, and queued scores pass through TB2 again when they are sent |
| TB7 | GitHub repository to Azure (CI/CD) | code, workflow files, secrets | secrets are used only in jobs that do not run for pull requests. Every action is pinned to a full commit SHA, zizmor runs in CI (`lint-workflows.yml`), and checkouts do not persist credentials. Further workflow hardening comes from CHORE-007 (see T11). |
| TB8 | Workflow-dispatch inputs to shell | `resource_group`, `app_name`, `custom_domain`, `max_instances`, `daily_quota_gb_seconds`, ... | handled by CHORE-007, which fixes the direct `${{ inputs.* }}` interpolation into `run:` scripts (T12). Inputs are also validated by allow-list in `.github/scripts/validate-dispatch-inputs.sh` before use (CHORE-015) |

## Threat model
| ID | Threat | Boundary | Impact | Mitigation | Status |
|----|--------|----------|--------|------------|--------|
| T1 | Score forgery: AC2 posts any score up to 999,999 to the all-time board, or to today's or yesterday's daily board, without playing | TB2 | AS1: fake scores take over the top 10 | Range, format and day-window checks, a multiple-of-10 rule, and a per-wave upper bound (CHORE-013). Forged scores within the bound are still possible, and the reported wave is not checked: nothing ties a score to a game that was played (no server-run game). The owner can remove entries by hand (R5). | partly |
| T2 | Spam / flooding: unlimited anonymous POSTs fill the boards with junk entries and make the table grow without limit | TB2 | AS1, AS5 | A per-client limit of 20 posts per hour (CHORE-013), counted in memory per instance. IP rotation, botnets and IPv6 beyond a /64 bypass it, and users behind one NAT share a limit. Filling the 10,000-client table puts new clients into one shared bucket, so a large address pool can delay everyone's posts for up to an hour. | partly |
| T3 | Unbounded leaderboard read: every `getleaderboard` call read the whole partition and sorted it in memory, so latency and cost grew with every entry. Flooding (T2) no longer slows the read. | TB3, TB4 | AS5; the leaderboard stops working | New scores get a ranked RowKey, and a GET reads one page of at most 10 entities from the ranked key range, merged in memory with the cached legacy top 10 of that board (one legacy read per board and instance, empty results not cached, 5-minute back-off after a failure). The table is created once per instance. | mitigated |
| T4 | Denial of wallet / availability: a flood of requests to the anonymous endpoints scales the consumption plan and the storage transactions | TB2, TB3 | AS5 | The Function App is capped at 1 instance and has a daily quota of 10,000 GB-seconds (CHORE-013), so cost has a ceiling. A flood can use up the quota and take the API down until 00:00 UTC: availability is traded for cost. GET endpoints are not rate-limited. CORS does not stop non-browser clients. | partly |
| T5 | Stored XSS or markup injection through names shown to other players | TB2, TB5 | AS6, other players' sessions | server-side `^[A-Z0-9]{1,3}$`, Razor output encoding, canvas `fillText` | mitigated |
| T6 | Offensive initials on the public boards | TB2 | AS1, reputation | Nothing beyond the 3-character limit and manual removal (R5). | open |
| T7 | Table query or key injection | TB3, TB4 | AS1, AS3 | parameterized `CreateQueryFilter`, partition built from validated values, ranked RowKey chosen by the server, a DTO with only four fields (no mass assignment) | mitigated |
| T8 | Oversized or malformed request bodies | TB2 | AS5 | a 1 KiB read limit (returns 413), and `JsonException` is handled (returns 400) | mitigated |
| T9 | Internal details leaking in error responses | TB2, TB3 | AS3 | fixed generic messages. Exceptions are logged on the server only. | mitigated |
| T10 | Storage credential compromise: the account key in the Function App settings grants full account access (no managed identity, no Key Vault, no table-scoped SAS) | TB4 | AS3, AS1 | The key is not in the repository, and the git history was checked and is clean. `local.settings.json` is git-ignored, and the provisioning workflow masks the key with `::add-mask::`. Whether the key is ever rotated is unknown (Q4). | partly |
| T11 | CI/CD and supply-chain compromise: a compromised action tag or a workflow change runs with `AZURE_CREDENTIALS`, `GH_PAT` or the SWA token | TB7 | AS4, AS6 | Secrets are not exposed to `pull_request` runs, and no workflow uses `pull_request_target`. CHORE-007 hardens the workflows. CHORE-015 pins every action to a commit SHA, lints the workflows with zizmor (hash-verified PyPI wheel, from the lock file of the SHA-pinned action), and stops persisting checkout credentials. The SWA deploy action pulls the mutable `staticappsclient:stable` image, so its SHA pin does not freeze the deploy code. Long-lived `AZURE_CREDENTIALS` remain (D1). The scopes of the service principal and the PAT are unknown (Q3). | partly |
| T12 | Script injection through workflow-dispatch inputs that are put into `run:` steps | TB8 | AS4 | Fixed by CHORE-007 (D4). Only users with write access can dispatch the workflows, and they can already edit them (R6). | mitigated |
| T13 | Out-of-support runtime and dependency drift | TB2, TB7 | the API's exposure to unpatched vulnerabilities | CHORE-001 moves the API to `net10.0` and provisions new Function Apps with `--runtime-version 10` (D4). Dependabot now covers GitHub Actions (CHORE-015). Still open: no Dependabot for NuGet, no NuGet lock files. The provisioning change affects only newly created apps, so the deployed app's runtime setting must be updated separately (Q7). | partly |
| T14 | Client-side tampering with `localStorage` (personal best, queued scores) | TB6 | only the player's own client | Parse errors are handled. Queued scores are validated again at TB2, so tampering gains nothing beyond what T1 already allows. | mitigated |
| T15 | Platform hardening gaps: the site sends no CSP, `X-Content-Type-Options` or `frame-ancestors` headers. The provisioning workflow does not enforce HTTPS-only on the Function App. The storage account keeps its default public network access and shared-key access. | TB1, TB2, TB4 | low: the client holds no secrets or sessions | none | open |
| T16 | A tampered or stale bundle served from the offline cache | TB1 | AS6 | assets fetched from the same origin with SRI `integrity` hashes, old caches deleted on activate, `no-cache` on the service-worker files | mitigated |

## Assumptions and out of scope
- Players are anonymous and have no accounts. The only administrator is the owner (AC3), who works through GitHub and Azure (D1).
- The app stores no data beyond initials and scores (D1). Azure platform logs for the Function App and SWA may record client IP addresses, but they are outside the app's code.
- Azure terminates TLS for both the SWA and the Function App hostnames.
- The security of the Azure and GitHub accounts themselves (MFA, who has access) is the owner's responsibility and is not modelled here.
- T1, T2 and T4 are partly mitigated by CHORE-013; T3 is mitigated by CHORE-012. They are not accepted risks (D2).
- Hashed client IPs (keyed with a per-process random salt) are held only in memory, for at most 2 hours (a timer drops ended counters). Q1 stays open.
- Out of scope: cheating inside the client (memory or WASM tampering). Its effect is the same as T1.

## Open questions
- Q1: Compliance needs (for example GDPR) and past incidents are unknown (D1, D3). Do platform logs that keep IP addresses matter to you, and should a retention period be set?
- Q3: What scopes do the `AZURE_CREDENTIALS` service principal (subscription or resource group, and which role) and `GH_PAT` have, and are they rotated? (D3)
- Q4: Is the Function App set to HTTPS-only, and is the storage account key ever rotated? The provisioning workflow sets neither. (D3)
- Q7: Has the deployed Function App's runtime setting been moved to .NET 10? CHORE-001 changes only the `--runtime-version` used when a new app is created, so an existing app keeps its old setting until it is updated in Azure or provisioned again (T13).

## Revisions
- 2026-11-17 CHORE-002: first draft, from the code and the user's answers (D1).
- 2026-11-17 CHORE-002: revised with D2-D4. T1-T4 stay open, to be fixed in follow-up items. Q1, Q3 and Q4 stay open. Q2 is answered. Q5 is closed: CHORE-006 removes the root `SaveHighScore.cs`. Q6 is closed: CHORE-001 moves the API to .NET 10, and T13 becomes partly mitigated. T12 is mitigated by CHORE-007, and TB7, TB8 and T11 were updated to match. Q7 is added.
- 2026-11-17 CHORE-015: actions SHA-pinned, zizmor in CI, Dependabot for GitHub Actions, dispatch inputs validated by allow-list. TB7, TB8, T11 and T13 updated; T11 stays partly.
- 2026-11-17 CHORE-008: removed the high-score read endpoint and the unused toast package. AC2, R1, TB3 and T13 updated.
- 2026-11-17 CHORE-012: capped the leaderboard read with a ranked RowKey. T3 is mitigated; R2, TB4 and T7 reworded. T2 and T4 stay open.
- 2026-11-18 CHORE-013: per-client rate limit, `wave` and plausibility checks, scale cap and daily quota. TB2, TB8, T1, T2 and T4 updated; all three stay partly. Hashed IPs held in memory only (Assumptions).

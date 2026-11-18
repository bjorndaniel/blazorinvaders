![Deploy to Azure Static Web Apps](https://github.com/bjorndaniel/blazorinvaders/workflows/Deploy%20to%20Azure%20Static%20Web%20Apps/badge.svg)

# Blazor Invaders

A Space Invaders clone built with Blazor WebAssembly (.NET 10) and an HTML5 canvas. Features a global top-10 leaderboard backed by Azure Table Storage and an Azure Functions API.

## Features

- Classic Space Invaders gameplay — move, shoot, dodge
- 5 rows of aliens with row-based scoring (30 / 20 / 10 pts) and colour-coded rows
- Mystery ship with deterministic scoring (authentic to the original)
- Destructible bunkers that degrade visually as they take hits
- Power-ups dropped by destroyed aliens: double shot, rapid fire, piercing shots, shield and extra life
- Combo multiplier: every 5 hits in a row raise alien points up to x4; a missed volley resets it
- Boss fight every 5th wave — a giant mothership with a health bar and rotating attack patterns
- Daily challenge (press D): the same seed and modifier for everyone each day, with its own leaderboard
- Last-alien frenzy mode — the final alien moves and fires at maximum speed
- Perfect wave bonus (+500 pts for clearing a wave without being hit)
- Explosions, floating scores, screen shake and hit flash; starfield and Web Audio API sound effects
- Hold arrow keys to move smoothly, hold space to keep firing; pause with P or Escape
- Touch controls on phones and tablets; installable as an app that plays offline
- Personal best stored in localStorage
- Global top-10 leaderboard (enter initials on game over if you qualify)
- Attract screen cycling through the score table, leaderboard, controls and the daily challenge

## Running locally

**Prerequisites:** .NET 10 SDK

```bash
# Clone and run the Blazor WASM frontend
git clone https://github.com/bjorndaniel/blazorinvaders.git
cd blazorinvaders/src
dotnet run
```

The game will be available at `https://localhost:5001`. The leaderboard API calls will fail without a running Functions backend, but the game itself is fully playable offline.

### Running the API locally

**Prerequisites:** Azure Functions Core Tools v4, .NET 10 SDK (builds the API too), [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) (`npm install -g azurite`, or the copy bundled with Visual Studio)

Both config files below are gitignored, so create them once:

`src/api/local.settings.json`
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*"
  }
}
```

`src/wwwroot/appsettings.Development.json`
```json
{
    "FunctionsApi": "http://localhost:7071/api"
}
```

Then run each in its own terminal:

```bash
# 1. Storage emulator (Table service on port 10002)
azurite --silent --disableTelemetry --skipApiVersionCheck --location ~/.azurite

# 2. API
cd src/api
func start

# 3. Frontend (the launch profile sets the Development environment)
cd src
dotnet run
```

In Development, `/?wave=5` starts games on that wave (e.g. to try the boss). The service worker only caches in published builds, so test offline play with `dotnet publish`.

The leaderboard is stored in the `HighScores` table, which is created on first use: all-time scores in the `HighScore` partition, daily challenges in `Daily-yyyyMMdd`. To browse or clear it, use Azure Storage Explorer (Emulator & Attached → Local).

## Running tests

```bash
dotnet test tests/BlazorInvaders.Tests
```

CI also checks formatting. Run this to check, and the same command without `--verify-no-changes` to fix:

```bash
dotnet format src/BlazorInvaders.sln --verify-no-changes
dotnet format src/BlazorInvaders.sln
```

## Abuse and cost limits

The API allows 20 score posts per client per hour (counted in memory per instance, on a salted hash of the client IP that is never stored). Past that it answers 429 and the game queues the score. Scores also have to be possible for the wave reached.

Provisioning caps the Function App at 1 instance and sets a daily quota of 10,000 GB-seconds (inputs `max_instances` and `daily_quota_gb_seconds`). Provisioning is not re-run for an app that exists, so set them by hand:

```bash
az resource update --resource-group <rg> --name <function-app>/config/web \
  --resource-type Microsoft.Web/sites/config --set properties.functionAppScaleLimit=1
az functionapp update --name <function-app> --resource-group <rg> --set dailyMemoryTimeQuota=10000
```

When the quota is hit the API stops until 00:00 UTC; scores then fail as a network error or a 403 and are queued and sent later. A value of 0 removes a limit.

After deploying, check that the client IP header cannot be forged: send 21 POSTs to `/api/savehighscore` with an invalid body (each is a 400, so nothing is saved), each with a different forged `X-Client-IP` and `X-Forwarded-For`. The 21st must get 429. If it gets 400, the header is forgeable and the build must be rolled back.

## Tech stack

| Layer | Technology |
|---|---|
| Frontend | Blazor WebAssembly (.NET 10) |
| Game logic | `src/core` class library (engine + scene description, unit-tested) |
| Canvas rendering | JS module (`wwwroot/js/game.js`), one interop call per frame |
| Audio | Web Audio API (JS interop) |
| Backend | Azure Functions (isolated worker, .NET 10) |
| Storage | Azure Table Storage |
| Hosting | Azure Static Web Apps (Free tier) |
| Font | Orbitron (Google Fonts) |

## Credits

Originally inspired by:
- https://www.davideguida.com/blazor-and-2d-game-development-part-1-intro/
- https://github.com/sausheong/invaders

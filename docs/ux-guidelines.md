# UX guidelines: Blazor Invaders

**Last updated:** 2026-11-17, by CHORE-014

## Product and users
A Space Invaders game in the browser, installable as a PWA that also plays offline. Players use a desktop with a keyboard or a phone with touch (D1). Their top tasks are playing a game (standard or today's challenge), entering initials after a qualifying score, and viewing the leaderboards (all-time and today's challenge). There are no accounts. Personal bests are kept on the device.

## Sources
| Source | Where | Decides |
|--------|-------|---------|
| User decisions | `docs/work/CHORE-005-ux-guidelines/status.md` D1-D4 | D1: there are no design documents, so the current UI is the reference; desktop keyboard plus phone touch; retro neon on black; WCAG 2.2 AA where a canvas game allows; English only. D2: the five known gaps get fixed in one fix-up item, and pinch zoom stays blocked only during play. D3: a mute control and larger phone text are wanted. D4: there is no toast or notification library; messages go on the canvas or in page text. |
| Page styles | `src/wwwroot/css/app.css`, Bootstrap in `src/wwwroot/css/bootstrap/` | Page colours, layout, breakpoints, touch controls, dialog styling |
| Canvas look and copy | `src/core/Rendering/SceneBuilder.cs`, `src/wwwroot/js/game.js`, `src/wwwroot/js/effects.js` | Playfield palette, text sizes, on-canvas messages, input keys, effects |
| Dialogs | Blazored.Modal 7.3.1 (`src/BlazorInvaders.csproj`) | Modal behaviour |
| Default rules | `.devagents/ux-defaults.md`, WCAG 2.2 AA | Everything the sources above leave open |

## Principles
| ID | Guideline | Source |
|----|-----------|--------|
| UX1 | The game is the home page. It gets focus on load, and one key or tap starts a game. Nothing comes before play. | code (`Pages/Index.razor`), D1 |
| UX2 | Every player action works with the keyboard on desktop and with touch on phones. Neither device is an afterthought. | D1, code |
| UX3 | Play never waits on the network. The game runs offline. Scores that cannot be sent are queued and sent later. Network failures do not interrupt a game. | code (`Index.razor` `QueueScoreAsync`, `LoadLeaderboardAsync`) |
| UX4 | The look is retro arcade: neon colours on black with Orbitron type. New UI matches it rather than adding a new style. | D1 |
| UX5 | Interruptions are rare. Only a qualifying score opens a dialog. It opens after the game is over and can always be cancelled. | code |

## Look
| ID | Guideline | Source |
|----|-----------|--------|
| UX10 | Only a dark appearance exists. The playfield is `#000`, the page background is the `#111`→`#555` gradient and text is white. The app does not follow `prefers-color-scheme`. This is a deliberate departure from the default rules. | D1, `app.css` |
| UX11 | Each canvas colour has one meaning. Cyan `#00E5FF` means the daily challenge. Gold `#FFD700` means rewards, scores and headings. Red `#ff4040` means danger, the boss and the mothership. `#888` is secondary text. The alien row colours, power-up colours and boss tints stay as defined. A new canvas colour is a named constant in `SceneBuilder.cs` (effects go in `effects.js`), never a literal elsewhere. | code |
| UX12 | Canvas text and the touch buttons use Orbitron, self-hosted from `fonts/orbitron-latin.woff2`. HTML page text uses Bootstrap's font stack. Nothing loads fonts or assets from a CDN, because the app must work offline. | code, UX3 |
| UX13 | Page styles live in `app.css` classes that use the colours above. New inline `style=` attributes are not added. The existing `color:#aaa` ones in `Index.razor` and `Leaderboard.razor` could become one class. | default rules |
| UX14 | Canvas text is at least 14 px at the 870×500 logical size, except the 10 px power-up letter, which the HUD also names. Text the player must read during play (score, prompts) is at least 18 px. On touch screens, that text is drawn larger so that it shows at least 12 CSS px on a 375 px-wide phone, in portrait and landscape. The phone part is planned and not built yet. | code, D3, default rules |

## Components and patterns
| ID | Guideline | Source |
|----|-----------|--------|
| UX20 | Page UI uses Bootstrap classes (`btn`, `btn-group`, `table-dark`, grid) and `app.css`. Dialogs use Blazored.Modal, with black text on white as styled in `app.css` (`.blazored-modal`, `.bm-content`). There are no toasts. A message goes on the canvas (UX22) or into page text next to its cause. No other component library is added. | code, D4 |
| UX21 | The primary action is `btn-primary` and the secondary one is `btn-secondary` (white with dark text). Labels are verbs or destinations, such as `Submit`, `Cancel` and `Back to game`. | code, default rules |
| UX22 | Game states are drawn on the canvas, not in HTML: the cycling attract screens, wave banners, the pause overlay and game over. Every overlay that waits for the player names the key or tap that continues it. | code (`SceneBuilder.cs`) |
| UX23 | The initials dialog stops game input while it is open and returns focus to the playfield when it closes. Validation errors appear under the inputs and are plain ("Use letters A-Z or digits"). The typed initials stay in the inputs. | code (`SetHighScore.razor`, `Index.razor`) |
| UX24 | On the leaderboard, peer boards are tabs (`btn-group`, `role="tablist"`, with the selected tab filled `btn-light`). It shows "Loading..." while fetching, "No scores yet. Be the first!" when a board is empty, and a striped dark table of #, Name and Score otherwise. If a board cannot be loaded, it shows "Could not load the leaderboard. Check your connection and try again." and a `Try again` button (`btn-secondary`) that reloads the selected board; it never shows the empty text. | code, default rules, D2 |
| UX25 | During play, a network failure is silent. It is logged to the console, the game continues and the score is queued (at most 10). The player never sees a raw error. | code |
| UX26 | There are two routes: `/` (the game) and `/leaderboard`. Every new screen has a visible way back to the game. | code |
| UX27 | There are no confirmation dialogs. Starting, pausing and leaving happen at once, and new features keep it that way unless an action loses data. | default rules, code |
| UX28 | Progress lives on the device (`localStorage`: personal best, today's best, pending scores) and is shown above the game. There is no sign-in. | code |
| UX29 | The game has one mute control (planned, not built yet). It toggles all sound, the march included, and takes effect at once. It works from the keyboard with one key that has no modifier (added to UX31) and from a touch button (UX33, UX37). Its current state shows on screen, and on the button through `aria-pressed`. The setting is remembered on the device (UX28). Sound is on by default. | D3, default rules |

## Accessibility
| ID | Guideline | Source |
|----|-----------|--------|
| UX30 | The target is WCAG 2.2 AA for all HTML UI: pages, dialogs, touch controls and page text. The real-time canvas playfield is exempt where a game cannot comply, but it follows UX31, UX34, UX35 and UX36. | D1 |
| UX31 | The whole game can be played with the keyboard: the arrow keys move, Space fires and starts, P or Escape pauses, and D starts today's challenge. Keys are handled only when the playfield has focus (`tabindex="0"`), so Tab still moves focus on and browser shortcuts still work. New keys avoid Ctrl, Alt, Meta and Tab combinations. | code (`game.js`) |
| UX32 | Every focusable HTML element, the playfield included, shows a visible focus indicator (WCAG 2.4.7). On touch screens the playfield shows none, because the on-screen controls replace it (CHORE-014 D2). | WCAG 2.2 AA, CHORE-014 D2 |
| UX33 | Every input and every icon-only button has an accessible name. The touch buttons use `aria-label`, such as "Move left" and "Fire". Each initials input has its own name, for example "Initial 1 of 3". | code, WCAG 4.1.2, D2 |
| UX34 | Text contrast is at least 4.5:1, or 3:1 for large text. Control boundaries are at least 3:1 against their actual background, including the gradient. | WCAG 1.4.3, 1.4.11 |
| UX35 | Colour is never the only signal. Power-ups carry a letter and a HUD name, and the daily mode shows a "DAILY" label. A new state gets text or shape as well as colour. | code, default rules |
| UX36 | Screen shake and other motion that is not part of gameplay are skipped when `prefers-reduced-motion: reduce` is set (`effects.js`). Nothing flashes more than 3 times a second. | code, WCAG 2.3.1 |
| UX37 | Touch targets are at least 44×44 CSS px. Today the small buttons are 44 px, the move buttons 64 px and Fire 84 px. | code, default rules |
| UX38 | Pinch zoom is blocked only for the playfield and touch controls during play. The leaderboard and the initials dialog can be zoomed (WCAG 1.4.4). | D2, WCAG 1.4.4 |

## Content and language
| ID | Guideline | Source |
|----|-----------|--------|
| UX40 | English only, with no translation mechanism. UI text lives in the Razor markup and in `SceneBuilder.cs`. | D1, code |
| UX41 | Canvas banners and titles are uppercase (GAME OVER, PAUSED, WAVE 3, DAILY CHALLENGE). Instructions, page headings and buttons are sentence case ("Press space to start", "Today's challenge", "Back to game"). | code |
| UX42 | Terms stay the same everywhere: score, Hi, leaderboard, today's challenge (daily challenge on the canvas), initials, wave, boss, and the power-up names DOUBLE SHOT, RAPID FIRE, SHIELD, PIERCING and +1 LIFE. | code |
| UX43 | Instructions name the input the player has: "Tap …" on touch screens and "Press <key> …" with a keyboard (`SceneBuilder.TouchControls`). | code, D2 |
| UX44 | Copy is short and arcade-toned. Errors do not blame the player and say what to do. Exception text goes to the console, never the UI. Canvas scores are zero-padded to 5 digits. | code, default rules |

## Layout and platforms
| ID | Guideline | Source |
|----|-----------|--------|
| UX50 | The playfield keeps its 870×500 logical size (87:50) and scales to fit. It never reflows. The page column steps at 30em, 35em and 60em (`.cover-container`). | code (`app.css`) |
| UX51 | The touch layout is chosen by `(pointer: coarse)`, not by width. The touch pads sit beside the playfield in landscape and below it in portrait. The title and footer are hidden when the height is 600 px or less. | code |
| UX52 | The PWA runs fullscreen, landscape is preferred, the theme colour is black, and it works offline. | `manifest.webmanifest`, code |
| UX53 | Supported browsers are the current evergreen ones on desktop, plus Safari and Chrome on mobile. Newer CSS gets a fallback first, as `100vh` comes before `100dvh`. | default rules, code |

## Known gaps
None open. CHORE-014 fixed the five gaps D2 listed.

Not built yet, each in its own later item (D3): the mute control (UX29) and the larger phone text (UX14).

## Open questions
- **Phone text size:** D3 asks for larger phone text but gives no size. The 12 CSS px in UX14 is my figure, taken from the platforms' minimum legible sizes (for example, Apple's 11 pt). Please confirm it or give another size when that item is planned.
- **Not seen running:** this revision also comes from the code alone. No Playwright tools were available in this session, and the earlier preview failed because the disk was full.

## Revisions
- 2026-11-17 CHORE-005: first draft from the code and D1.
- 2026-11-17 CHORE-005: revised with D2-D4.
  - Added UX29 (the mute control, planned), UX38 (pinch zoom allowed outside play) and the Known gaps section.
  - UX14 now covers larger phone text and the power-up letter exception.
  - UX20 states there are no toasts.
  - Named the initials inputs in UX33.
  - Removed the resolved open questions.
- 2026-11-17 CHORE-014: fixed the five known gaps (UX24, UX32, UX33, UX38, UX43).
- 2026-11-17 CHORE-008: removed the toast package reference from D4 and UX20; no toast package is used.

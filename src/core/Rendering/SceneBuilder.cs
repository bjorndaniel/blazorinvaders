using System.Globalization;
using BlazorInvaders.GameObjects;

namespace BlazorInvaders.Rendering
{
    /// <summary>
    /// Turns engine state into a <see cref="Scene"/>. Layout and styling decisions stay in C#;
    /// the JS renderer only executes the resulting draw commands.
    /// </summary>
    public class SceneBuilder
    {
        private const string White = "white";
        private const string Gold = "#FFD700";
        private const string MotherShipRed = "#ff4040";

        // Row tint colours, applied via pre-tinted copies of the sprite sheet in JS
        private static readonly string[] RowTints =
        {
            "#ffffff",   // row 0 – Squid    (white)
            "#00dcff",   // row 1 – Crab     (cyan)
            "#00dcff",   // row 2 – Crab     (cyan)
            "#78ff3c",   // row 3 – Octopus  (green)
            "#78ff3c",   // row 4 – Octopus  (green)
        };

        // Indexed by PowerUpType
        private static readonly (string Color, string Letter, string Name, string Hud)[] PowerUpStyles =
        {
            ("#FFD700", "D", "DOUBLE SHOT", "2X SHOT"),
            ("#FF8C00", "R", "RAPID FIRE",  "RAPID"),
            ("#00E5FF", "S", "SHIELD",      "SHIELD"),
            ("#FF40FF", "P", "PIERCING",    "PIERCE"),
            ("#40FF40", "+", "+1 LIFE",     ""),
        };

        // Indexed by Boss.Phase; null keeps the sprite's own red
        private static readonly string?[] BossPhaseTints = { null, "#ff9900", "#ff00ff" };

        private static readonly Sprite LifeIcon = new(20, 47, 38, 59);

        private static readonly (Sprite sprite, string label)[] ScoreTable =
        {
            (new Sprite(12, 73, 24, 80), "= ???  PTS"),
            (new Sprite(0, 27, 20, 41),  "= 30  PTS"),
            (new Sprite(0, 0, 20, 12),   "= 20  PTS"),
            (new Sprite(20, 14, 40, 26), "= 10  PTS"),
        };

        private readonly List<int> _cmds = new(1024);
        private readonly List<string> _strings = new();
        private readonly Dictionary<string, int> _stringIndex = new();

        // Attract screen
        private int _attractState;
        private float _attractStateStart;

        /// <summary>Word on-screen instructions for the touch buttons instead of the keyboard.</summary>
        public bool TouchControls { get; set; }

        /// <summary>Shown on the attract screen; null leaves the daily card out.</summary>
        public DailyChallenge? TodaysChallenge { get; set; }

        private const string DailyColor = "#00E5FF";

        public static string Describe(DailyModifier modifier) => modifier switch
        {
            DailyModifier.NoBunkers => "NO BUNKERS",
            DailyModifier.Speedrun => "FAST START",
            DailyModifier.ShieldStart => "SHIELD START",
            DailyModifier.BossRush => "BOSS EVERY 3RD WAVE",
            DailyModifier.Glass => "ONE LIFE, DOUBLE POINTS",
            _ => "NO MODIFIER",
        };

        private static string FormatDate(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public Scene Build(GameEngine engine, float timeStamp)
        {
            _cmds.Clear();
            _strings.Clear();
            _stringIndex.Clear();

            var w = engine.Width;
            var h = engine.Height;

            var header = $"Score: {engine.Points:D5}   Hi: {engine.HighScore} {engine.HighScoreName}";
            Text(header, 10, 22, 18, White);

            if (engine.Started && engine.Multiplier > 1)
                Text($"COMBO x{engine.Multiplier}", w - 170, 22, 18, Gold);

            if (engine.GameOver)
                GameOverScreen(w, h);
            else if (engine.Started)
                GameFrame(engine);
            else
                AttractScreen(engine, timeStamp);

            // Particles sit above the playfield but below the banners and pause overlay
            _cmds.Add(Scene.OpEffects);

            if (engine.ShowWaveBanner && engine.Daily is { } daily && engine.Wave == 1)
            {
                Rect(0, h / 2 - 50, w, 90, "rgba(0,0,0,0.55)");
                CenteredText("DAILY CHALLENGE", w, h / 2, 36, DailyColor);
                CenteredText($"{FormatDate(daily.Date)}  ·  {Describe(daily.Modifier)}", w, h / 2 + 30, 18, White);
            }
            else if (engine.ShowWaveBanner)
            {
                Rect(0, h / 2 - 45, w, 65, "rgba(0,0,0,0.55)");
                if (engine.IsBossWave)
                    CenteredText("BOSS WAVE", w, h / 2 + 5, 40, MotherShipRed);
                else
                    CenteredText($"WAVE {engine.Wave}", w, h / 2 + 5, 40, "#00FF88");
            }

            if (engine.ShowPerfectBonus)
                CenteredText($"PERFECT!  +{engine.PerfectWaveBonus}", w, 80, 28, Gold);

            if (engine.Paused)
            {
                Rect(0, 0, w, h, "rgba(0,0,0,0.6)");
                CenteredText("PAUSED", w, h / 2 - 10, 48, White);
                CenteredText(TouchControls ? "Tap II to resume" : "Press P to resume", w, h / 2 + 40, 20, White);
            }

            var events = EncodeEvents(engine.TakeEvents());
            var sounds = engine.TakeSounds(out var march);
            return new Scene
            {
                Cmds = _cmds.ToArray(),
                Strings = _strings.ToArray(),
                Events = events,
                Paused = engine.Paused,
                Sounds = (int)sounds,
                March = march,
            };
        }

        private int[] EncodeEvents(GameEvent[] events)
        {
            if (events.Length == 0)
                return [];

            var encoded = new int[events.Length * Scene.EventStride];
            for (int i = 0; i < events.Length; i++)
            {
                var e = events[i];
                var (label, color) = e.Type switch
                {
                    GameEventType.AlienKilled => ($"+{e.Value}", RowTints[e.Variant < RowTints.Length ? e.Variant : 0]),
                    GameEventType.MotherShipKilled => ($"+{e.Value}", MotherShipRed),
                    GameEventType.PlayerHit => (null, White),
                    GameEventType.BunkerHit => (null, BunkerColor(e.Variant)),
                    GameEventType.ComboUp => ($"COMBO x{e.Value}!", Gold),
                    GameEventType.PowerUpCollected => (PowerUpStyles[e.Variant].Name, PowerUpStyles[e.Variant].Color),
                    GameEventType.ShieldBlocked => (null, PowerUpStyles[(int)PowerUpType.Shield].Color),
                    GameEventType.BossHit => (null, White),
                    GameEventType.BossKilled => ($"BOSS DOWN +{e.Value}", Gold),
                    _ => ((string?)null, (string?)null),
                };

                var o = i * Scene.EventStride;
                encoded[o] = (int)e.Type;
                encoded[o + 1] = e.X;
                encoded[o + 2] = e.Y;
                encoded[o + 3] = label == null ? -1 : Str(label);
                encoded[o + 4] = color == null ? -1 : Str(color);
            }
            return encoded;
        }

        private static string BunkerColor(int health) => health switch
        {
            4 => "rgba(0,230,0,1.0)",
            3 => "rgba(0,200,0,0.85)",
            2 => "rgba(180,200,0,0.7)",
            _ => "rgba(200,80,0,0.6)"
        };

        private void GameFrame(GameEngine engine)
        {
            var w = engine.Width;
            var h = engine.Height;

            foreach (var bunker in engine.Bunkers.Where(b => !b.Destroyed))
            {
                var color = BunkerColor(bunker.Health);
                var (x, y) = (bunker.Position.X, bunker.Position.Y);
                var (bw, bh) = (Bunker.Size.Width, Bunker.Size.Height);
                Rect(x + 10, y, bw - 20, bh - 8, color);
                Rect(x, y + 10, bw, bh - 10, color);
                Rect(x + 18, y + 18, 24, bh - 18, "black");
            }

            if (engine.LostALife)
                CenteredText("You were hit!", w, 55, 20, "red");

            if (engine.IsFrenzy)
                Text("FRENZY!", 10, 45, 16, "#FF4444");

            if (engine.Player is { } player)
            {
                Sprite(player.Sprite, player.CurrentPosition.X, player.CurrentPosition.Y);
                if (engine.HasShield)
                {
                    var size = player.Sprite.RenderSize;
                    Circle(player.CurrentPosition.X + size.Width / 2, player.CurrentPosition.Y + size.Height / 2, 30,
                        PowerUpStyles[(int)PowerUpType.Shield].Color, 2);
                }
            }

            foreach (var s in engine.Shots)
                Sprite(s.Sprite, s.CurrentPosition.X, s.CurrentPosition.Y,
                    tint: s.Piercing ? PowerUpStyles[(int)PowerUpType.Piercing].Color : null);

            foreach (var b in engine.Bombs)
                Sprite(b.Sprite, b.CurrentPosition.X, b.CurrentPosition.Y);

            // Its score floats up from the MotherShipKilled event
            if (engine.MotherShip is { } ship)
                Sprite(ship.Sprite, ship.CurrentPosition.X, ship.CurrentPosition.Y);

            if (engine.Boss is { Defeated: false } boss)
                BossFrame(boss, engine.Time, w);

            foreach (var p in engine.PowerUps)
            {
                var style = PowerUpStyles[(int)p.Type];
                Diamond(p.Position.X, p.Position.Y, PowerUp.Size.Width, style.Color);
                Text(style.Letter, p.Position.X + PowerUp.Size.Width / 2, p.Position.Y + 12, 10, "black", centered: true);
            }

            for (int i = 0; i < engine.Lives - 1; i++)
                Sprite(LifeIcon, w - (i + 1) * 28 - 10, h - 24, LifeIcon.Size.Width, LifeIcon.Size.Height);

            PowerUpStatus(engine, h - 10);

            if (engine.Daily is { } daily)
                CenteredText($"DAILY  ·  {Describe(daily.Modifier)}", w, h - 10, 14, DailyColor);

            // Aliens last so they sit above bunkers and the player but below the overlays
            foreach (var a in engine.Aliens.Where(a => !a.Destroyed))
            {
                var tint = a.HasBeenHit ? null : RowTints[a.Row < RowTints.Length ? a.Row : 0];
                Sprite(a.Sprite, a.CurrentPosition.X, a.CurrentPosition.Y, tint: tint);
            }
        }

        private void BossFrame(Boss boss, float time, int w)
        {
            // White for a moment after each hit, otherwise angrier colours as health drops
            var tint = time - boss.LastHitTime < 80 ? "#ffffff" : BossPhaseTints[boss.Phase];
            Sprite(Boss.ShipSprite, boss.Position.X, boss.Position.Y, Boss.Size.Width, Boss.Size.Height, tint);

            const int barWidth = 300, barHeight = 8, barY = 36;
            var barX = w / 2 - barWidth / 2;
            var fill = boss.Hp * barWidth / boss.MaxHp;
            Text("BOSS", barX - 60, barY + 9, 14, MotherShipRed);
            Rect(barX, barY, barWidth, barHeight, "rgba(255,255,255,0.2)");
            Rect(barX, barY, fill, barHeight, boss.Phase == 2 ? "#ff9900" : MotherShipRed);
        }

        // Active power-ups along the bottom left, e.g. "RAPID 7  SHIELD"
        private void PowerUpStatus(GameEngine engine, int y)
        {
            var x = 10;
            foreach (var type in Enum.GetValues<PowerUpType>())
            {
                if (!engine.IsActive(type)) continue;
                var style = PowerUpStyles[(int)type];
                var seconds = (int)Math.Ceiling(engine.RemainingMs(type) / 1000);
                Text(seconds > 0 ? $"{style.Hud} {seconds}" : style.Hud, x, y, 14, style.Color);
                x += 120;
            }
        }

        private void GameOverScreen(int w, int h)
        {
            CenteredText("GAME OVER", w, h / 2 - 30, 36, White);
            CenteredText(TouchControls ? "Tap to start a new game" : "Press space to start a new game", w, h / 2 + 20, 22, White);
            if (TodaysChallenge != null)
                CenteredText("D for today's challenge", w, h / 2 + 55, 18, DailyColor);
        }

        private void AttractScreen(GameEngine engine, float timeStamp)
        {
            // Cycle attract states every 4 seconds; the daily card is the fourth
            var states = TodaysChallenge == null ? 3 : 4;
            if (timeStamp > 0 && timeStamp - _attractStateStart > 4000)
            {
                _attractState = (_attractState + 1) % states;
                _attractStateStart = timeStamp;
            }

            var w = engine.Width;
            switch (_attractState)
            {
                case 0:
                    CenteredText("SCORE ADVANCE TABLE", w, 120, 22, Gold);
                    var y = 170;
                    foreach (var (sprite, label) in ScoreTable)
                    {
                        Sprite(sprite, w / 2 - 80, y - sprite.RenderSize.Height);
                        Text(label, w / 2 - 40, y, 20, White);
                        y += 50;
                    }
                    break;

                case 1:
                    // The board of the mode last played
                    CenteredText(engine.IsDaily ? "TODAY'S CHALLENGE" : "TOP SCORES", w, 130, 26, Gold);
                    var top = engine.TopScores;
                    if (top.Count == 0)
                    {
                        CenteredText("NO SCORES YET", w, 200, 22, White);
                    }
                    else
                    {
                        for (int i = 0; i < Math.Min(top.Count, 3); i++)
                            CenteredText($"{i + 1}.  {top[i].Name,-5} {top[i].Score:D5}", w, 180 + i * 45, 22, White);
                    }
                    break;

                case 2 when TouchControls:
                    CenteredText("Use the arrow buttons to move, FIRE to shoot", w, 160, 26, White);
                    CenteredText("Tap the screen to start", w, 220, 28, White);
                    CenteredText("II to pause", w, 265, 18, "#888");
                    break;

                case 3 when TodaysChallenge is { } daily:
                    CenteredText("DAILY CHALLENGE", w, 130, 26, DailyColor);
                    CenteredText(FormatDate(daily.Date), w, 175, 20, White);
                    CenteredText(Describe(daily.Modifier), w, 220, 24, Gold);
                    CenteredText("Same game for everyone today", w, 260, 18, "#888");
                    CenteredText(TouchControls ? "Tap D to play" : "Press D to play", w, 300, 22, White);
                    break;

                default:
                    CenteredText("Use arrow keys to move, space to fire", w, 160, 28, White);
                    CenteredText("Press space to start", w, 220, 28, White);
                    CenteredText("P / Escape to pause", w, 265, 18, "#888");
                    break;
            }
        }

        private void Sprite(Sprite sprite, int x, int y, int? width = null, int? height = null, string? tint = null)
        {
            _cmds.Add(Scene.OpSprite);
            _cmds.Add(sprite.TopLeft.X);
            _cmds.Add(sprite.TopLeft.Y);
            _cmds.Add(sprite.Size.Width);
            _cmds.Add(sprite.Size.Height);
            _cmds.Add(x);
            _cmds.Add(y);
            _cmds.Add(width ?? sprite.RenderSize.Width);
            _cmds.Add(height ?? sprite.RenderSize.Height);
            _cmds.Add(tint == null ? -1 : Str(tint));
        }

        private void Rect(int x, int y, int w, int h, string color)
        {
            _cmds.Add(Scene.OpRect);
            _cmds.Add(x);
            _cmds.Add(y);
            _cmds.Add(w);
            _cmds.Add(h);
            _cmds.Add(Str(color));
        }

        private void Text(string text, int x, int y, int fontSize, string color, bool centered = false)
        {
            _cmds.Add(Scene.OpText);
            _cmds.Add(Str(text));
            _cmds.Add(x);
            _cmds.Add(y);
            _cmds.Add(Str($"{fontSize}px Orbitron"));
            _cmds.Add(Str(color));
            _cmds.Add(centered ? 1 : 0);
        }

        private void CenteredText(string text, int width, int y, int fontSize, string color) =>
            Text(text, width / 2, y, fontSize, color, centered: true);

        private void Circle(int x, int y, int radius, string color, int lineWidth = 0)
        {
            _cmds.Add(Scene.OpCircle);
            _cmds.Add(x);
            _cmds.Add(y);
            _cmds.Add(radius);
            _cmds.Add(Str(color));
            _cmds.Add(lineWidth);
        }

        private void Diamond(int x, int y, int size, string color)
        {
            _cmds.Add(Scene.OpDiamond);
            _cmds.Add(x);
            _cmds.Add(y);
            _cmds.Add(size);
            _cmds.Add(Str(color));
        }

        private int Str(string value)
        {
            if (!_stringIndex.TryGetValue(value, out var index))
            {
                index = _strings.Count;
                _strings.Add(value);
                _stringIndex[value] = index;
            }
            return index;
        }
    }
}

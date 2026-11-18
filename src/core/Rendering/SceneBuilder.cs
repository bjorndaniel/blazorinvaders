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

        // Canvas width and HUD layout of the frame being built; "You were hit!" and "PERFECT!" share them
        private int _canvasWidth;
        private int _hudShift;
        private int _hitSize;
        private int _hitY;

        // Attract screen
        private int _attractState;
        private float _attractStateStart;

        /// <summary>Word on-screen instructions for the touch buttons instead of the keyboard.</summary>
        public bool TouchControls { get; set; }

        /// <summary>
        /// CSS pixels per logical canvas pixel, as measured by game.js. Must match the scale game.js
        /// passes to Tick (canvas.clientWidth / viewWidth). Only used when <see cref="TouchControls"/> is true.
        /// </summary>
        public float DisplayScale { get; set; } = 1;

        /// <summary>The one copy of the mute state; the page toggles it and stores it.</summary>
        public SoundSetting Sound { get; set; } = new();

        /// <summary>Shown on the attract screen; null leaves the daily card out.</summary>
        public DailyChallenge? TodaysChallenge { get; set; }

        private const string DailyColor = "#00E5FF";

        // Phone text sizing (UX14): text of MustReadSize and up shows at least MinPhoneTextPx CSS px
        private const int MinPhoneTextPx = 12;
        private const int MustReadSize = 18;  // Must match MUST_READ_SIZE in effects.js
        private const int MaxPhoneTextSize = 60;
        private const int EdgeMargin = 10;
        private const int HudGap = 20;
        private const float WidthMargin = 1.05f;
        private const float Ascent = 0.77f;
        private const float Descent = 0.243f;
        private const int HeaderBaseline = 22;
        private const int HitBaselineDesign = 55;
        private const int HitDesignSize = 20;
        private const int PerfectBaselineDesign = 80;
        private const int BossBarY = 36;
        private const int BossBarHeight = 8;
        private const int BossBarBottom = BossBarY + BossBarHeight;

        // Advance widths in thousandths of an em for ASCII 32-126, from hmtx of wwwroot/fonts/orbitron-latin.woff2.
        // Must match that font file; other characters count as 1000.
        private static readonly short[] Advances =
        {
            272, 220, 372, 797, 788, 966, 938, 224, 277, 278, 491, 433, 193, 517, 214, 521,
            834, 391, 830, 826, 730, 830, 820, 660, 834, 828, 214, 193, 473, 638, 475, 678,
            832, 836, 832, 822, 834, 766, 723, 830, 851, 214, 780, 797, 779, 928, 832, 828,
            791, 884, 825, 822, 759, 828, 1003, 1179, 812, 806, 821, 275, 520, 276, 1000, 828,
            315, 694, 667, 695, 667, 692, 407, 683, 668, 208, 239, 646, 302, 978, 696, 692,
            664, 664, 512, 686, 410, 695, 790, 1071, 692, 685, 698, 289, 214, 289, 404,
        };

        /// <summary>
        /// The font size for text of <paramref name="designSize"/> at <paramref name="scale"/> (CSS px per logical px)
        /// on a touch screen: text of 18 px and up is raised so it shows at 12 CSS px or more. Smaller text and
        /// an unusable scale keep the design size.
        /// </summary>
        public static int PhoneFontSize(int designSize, float scale)
        {
            if (designSize < MustReadSize || !float.IsFinite(scale) || scale <= 0)
                return designSize;
            return Math.Min(MaxPhoneTextSize, Math.Max(designSize, (int)Math.Ceiling(MinPhoneTextPx / scale)));
        }

        /// <summary>Estimated width in logical px of <paramref name="text"/> in Orbitron at <paramref name="size"/>, with a safety margin.</summary>
        internal static float EstimateWidth(string text, int size)
        {
            var total = 0;
            foreach (var c in text)
                total += c >= 32 && c <= 126 ? Advances[c - 32] : 1000;
            return total * size / 1000f * WidthMargin;
        }

        /// <summary>Baseline of "You were hit!" at <paramref name="size"/>: the design position at the design size, lower when enlarged.</summary>
        internal static int HitBaseline(int size, bool bossWave)
        {
            if (size <= HitDesignSize)
                return HitBaselineDesign;
            var baseline = HitBaselineDesign + RaisedBaseline(size) - HeaderBaseline;
            return bossWave
                ? Math.Max(baseline, BossBarBottom + 4 + (int)Math.Ceiling(Ascent * size))
                : baseline;
        }

        /// <summary>Baseline of "PERFECT!" when "You were hit!" shows too: below it, or the design position when it is not enlarged.</summary>
        internal static int PerfectBaseline(int hitBaseline, int hitSize, int perfectSize)
        {
            if (hitSize <= HitDesignSize)
                return PerfectBaselineDesign;
            return Math.Max(PerfectBaselineDesign,
                hitBaseline + (int)Math.Ceiling(Descent * hitSize) + 4 + (int)Math.Ceiling(Ascent * perfectSize));
        }

        private static int RaisedBaseline(int size) => Math.Max(HeaderBaseline, (int)Math.Ceiling(Ascent * size) + 4);

        private int Size(int designSize) => TouchControls ? PhoneFontSize(designSize, DisplayScale) : designSize;

        // An enlarged size of the given width, shrunk toward the design size when it would not fit in space
        private static int ShrinkToFit(int size, int designSize, float width, float space) =>
            width > space ? Math.Max(designSize, (int)Math.Floor(size * space / width)) : size;

        // The enlarged size, shrunk toward the design size when the line would not fit in space
        private int FitSize(string text, int designSize, float space)
        {
            var size = Size(designSize);
            return size > designSize ? ShrinkToFit(size, designSize, EstimateWidth(text, size), space) : size;
        }

        // Header and combo share one row: one common size so both fit side by side
        private int HudSize(string header, string? combo, int w)
        {
            var size = Size(MustReadSize);
            if (size > MustReadSize)
            {
                var width = EstimateWidth(header, size) + (combo == null ? 0 : EstimateWidth(combo, size));
                var space = w - 2 * EdgeMargin - (combo == null ? 0 : HudGap);
                size = ShrinkToFit(size, MustReadSize, width, space);
            }
            return size;
        }

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
            _canvasWidth = w;

            var header = $"Score: {engine.Points:D5}   Hi: {engine.HighScore} {engine.HighScoreName}";
            var combo = engine.Started && engine.Multiplier > 1 ? $"COMBO x{engine.Multiplier}" : null;
            var hudSize = HudSize(header, combo, w);
            var enlarged = hudSize > MustReadSize;
            var baseline = enlarged ? RaisedBaseline(hudSize) : HeaderBaseline;
            _hudShift = baseline - HeaderBaseline;
            TextAt(header, 10, baseline, hudSize, White);
            if (combo != null)
            {
                var comboX = enlarged ? w - EdgeMargin - (int)Math.Ceiling(EstimateWidth(combo, hudSize)) : w - 170;
                TextAt(combo, comboX, baseline, hudSize, Gold);
            }
            _hitSize = 0;

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
            {
                var perfect = $"PERFECT!  +{engine.PerfectWaveBonus}";
                var perfectSize = FitSize(perfect, 28, w - 2 * EdgeMargin);
                var perfectY = _hitSize > HitDesignSize ? PerfectBaseline(_hitY, _hitSize, perfectSize) : PerfectBaselineDesign;
                TextAt(perfect, w / 2, perfectY, perfectSize, Gold, centered: true);
            }

            if (engine.Paused)
            {
                Rect(0, 0, w, h, "rgba(0,0,0,0.6)");
                CenteredText("PAUSED", w, h / 2 - 10, 48, White);
                CenteredText(TouchControls ? "Tap II to resume" : "Press P to resume", w, h / 2 + 40, 20, White);
            }

            // After the pause overlay so it is not dimmed
            if (Sound.Muted)
                Text("SOUND OFF", w - 120, 45, 14, "#888");

            var events = EncodeEvents(engine.TakeEvents());
            var sounds = engine.TakeSounds(out var march);
            return new Scene
            {
                Cmds = _cmds.ToArray(),
                Strings = _strings.ToArray(),
                Events = events,
                Paused = engine.Paused,
                Muted = Sound.Muted,
                MinTextSize = TouchControls ? PhoneFontSize(MustReadSize, DisplayScale) : 0,
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
            {
                _hitSize = FitSize("You were hit!", HitDesignSize, w - 2 * EdgeMargin);
                _hitY = HitBaseline(_hitSize, engine.Boss is { Defeated: false });
                TextAt("You were hit!", w / 2, _hitY, _hitSize, "red", centered: true);
            }

            if (engine.IsFrenzy)
                Text("FRENZY!", 10, 45 + _hudShift, 16, "#FF4444");

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

            const int barWidth = 300, barHeight = BossBarHeight, barY = BossBarY;
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
                    CenteredText("II to pause  ·  M toggles sound", w, 265, 18, "#888");
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
                    CenteredText("P / Escape to pause  ·  M toggles sound", w, 265, 18, "#888");
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
            var space = centered ? _canvasWidth - 2 * EdgeMargin : _canvasWidth - x - EdgeMargin;
            TextAt(text, x, y, FitSize(text, fontSize, space), color, centered);
        }

        private void TextAt(string text, int x, int y, int fontSize, string color, bool centered = false)
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

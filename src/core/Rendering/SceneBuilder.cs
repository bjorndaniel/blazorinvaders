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

        public Scene Build(GameEngine engine, float timeStamp)
        {
            _cmds.Clear();
            _strings.Clear();
            _stringIndex.Clear();

            var w = engine.Width;
            var h = engine.Height;

            var header = $"Score: {engine.Points:D5}   Hi: {engine.HighScore} {engine.HighScoreName}";
            if (engine.DoubleShot) header += "  * 2x SHOT";
            Text(header, 10, 22, 18, White);

            if (engine.GameOver)
                GameOverScreen(w, h);
            else if (engine.Started)
                GameFrame(engine);
            else
                AttractScreen(engine, timeStamp);

            // Particles sit above the playfield but below the banners and pause overlay
            _cmds.Add(Scene.OpEffects);

            if (engine.ShowWaveBanner)
            {
                Rect(0, h / 2 - 45, w, 65, "rgba(0,0,0,0.55)");
                CenteredText($"WAVE {engine.Wave}", w, h / 2 + 5, 40, "#00FF88");
            }

            if (engine.ShowPerfectBonus)
                CenteredText("PERFECT!  +500", w, 80, 28, Gold);

            if (engine.Paused)
            {
                Rect(0, 0, w, h, "rgba(0,0,0,0.6)");
                CenteredText("PAUSED", w, h / 2 - 10, 48, White);
                CenteredText("Press P to resume", w, h / 2 + 40, 20, White);
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
                Sprite(player.Sprite, player.CurrentPosition.X, player.CurrentPosition.Y);

            foreach (var s in engine.Shots)
                Sprite(s.Sprite, s.CurrentPosition.X, s.CurrentPosition.Y);

            foreach (var b in engine.Bombs)
                Sprite(b.Sprite, b.CurrentPosition.X, b.CurrentPosition.Y);

            // Its score floats up from the MotherShipKilled event
            if (engine.MotherShip is { } ship)
                Sprite(ship.Sprite, ship.CurrentPosition.X, ship.CurrentPosition.Y);

            foreach (var p in engine.PowerUps)
                Diamond(p.Position.X, p.Position.Y, PowerUp.Size.Width, Gold);

            for (int i = 0; i < engine.Lives - 1; i++)
                Sprite(LifeIcon, w - (i + 1) * 28 - 10, h - 24, LifeIcon.Size.Width, LifeIcon.Size.Height);

            // Aliens last so they sit above bunkers and the player but below the overlays
            foreach (var a in engine.Aliens.Where(a => !a.Destroyed))
            {
                var tint = a.HasBeenHit ? null : RowTints[a.Row < RowTints.Length ? a.Row : 0];
                Sprite(a.Sprite, a.CurrentPosition.X, a.CurrentPosition.Y, tint: tint);
            }
        }

        private void GameOverScreen(int w, int h)
        {
            CenteredText("GAME OVER", w, h / 2 - 30, 36, White);
            CenteredText("Press space to start a new game", w, h / 2 + 20, 22, White);
        }

        private void AttractScreen(GameEngine engine, float timeStamp)
        {
            // Cycle attract states every 4 seconds
            if (timeStamp > 0 && timeStamp - _attractStateStart > 4000)
            {
                _attractState = (_attractState + 1) % 3;
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
                    CenteredText("TOP SCORES", w, 130, 26, Gold);
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

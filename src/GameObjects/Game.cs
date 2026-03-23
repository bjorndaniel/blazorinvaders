using System.Drawing;
using System.Text.Json;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace BlazorInvaders.GameObjects
{
    public class Game
    {
        private List<Alien> _aliens = new();
        private List<Shot> _shots = new();
        private List<Bomb> _bombs = new();
        private List<Bunker> _bunkers = new();
        private List<PowerUp> _powerUps = new();
        private readonly List<(float x, float y, float brightness, int size)> _stars = new();

        private float _lastUpdate;
        private float _removeLifeBanner;
        private Direction _currentMovement;
        private Player? _player;
        private Canvas2DContext? _context;
        private readonly GameTime _gameTime = new();
        private ElementReference _spriteSheet;
        private readonly int _width;
        private readonly int _height;
        private int _points;
        private int _lives;
        private readonly Random _random = new();
        private bool _lostALife;
        private double _gameSpeed;
        private MotherShip? _motherShip;
        private readonly HttpClient _client;
        private readonly string? _apiUrl;
        private readonly IJSRuntime _js;

        private int _wave;
        private float _waveDisplayUntil;
        private int _livesAtWaveStart;
        private float _perfectBonusUntil;

        private bool _doubleShot;
        private float _doubleShotUntil;

        private int _totalShotsFired;

        private bool _playShoot;
        private bool _playAlienDeath;
        private bool _playPlayerDeath;
        private bool _playMotherShip;
        private bool _updateMarch;
        private bool _stopMarch;
        private int _lastAliveCount;
        private float _lastMotherShipPing;
        private bool _newHighScoreFired;

        // Attract screen
        private int _attractState;
        private float _attractStateStart;
        private List<HighScore> _topScores = new();

        // Deterministic mothership score table (authentic to original)
        private static readonly int[] MotherShipScoreTable =
            { 100, 50, 50, 100, 150, 100, 100, 50, 300, 100, 100, 100, 50, 150, 100 };

        // Row tint colours (rgba backgrounds drawn behind each alien)
        private static readonly string[] RowTints =
        {
            "rgba(255,255,255,0.18)",   // row 0 – Squid      (white)
            "rgba(0,220,255,0.22)",     // row 1 – Crab       (cyan)
            "rgba(0,220,255,0.22)",     // row 2 – Crab       (cyan)
            "rgba(120,255,60,0.22)",    // row 3 – Octopus    (green)
            "rgba(120,255,60,0.22)",    // row 4 – Octopus    (green)
        };

        public event Func<object, EventArgs, Task>? NewHighScore;

        public Game(int gameWidth, int gameHeight, IConfiguration config, HttpClient client, IJSRuntime js)
        {
            _width = gameWidth;
            _height = gameHeight;
            _apiUrl = config["FunctionsApi"];
            _client = client;
            _js = js;
            InitStars();
        }

        private void InitStars()
        {
            for (int i = 0; i < 80; i++)
            {
                _stars.Add((
                    x: (float)(_random.NextDouble() * _width),
                    y: (float)(_random.NextDouble() * _height),
                    brightness: (float)(0.3 + _random.NextDouble() * 0.7),
                    size: _random.Next(1, 3)
                ));
            }
        }

        public async ValueTask Init(BECanvasComponent canvas, ElementReference spriteSheet)
        {
            _context = await canvas.CreateCanvas2DAsync();
            _gameSpeed = 0;
            _spriteSheet = spriteSheet;
            try
            {
                var result = await _client.GetAsync($"{_apiUrl}/getleaderboard");
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var scores = JsonSerializer.Deserialize<List<HighScore>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (scores?.Count > 0)
                    {
                        _topScores = scores.Take(3).ToList();
                        HighScore = scores[0].Score;
                        HighScoreName = scores[0].Name;
                    }
                }
            }
            catch { }
        }

        public async ValueTask Start(float time)
        {
            _gameSpeed = 0;
            _wave = 1;
            _waveDisplayUntil = time + 2000;
            _aliens = CreateAlienGrid();
            _shots = new List<Shot>();
            _bombs = new List<Bomb>();
            _powerUps = new List<PowerUp>();
            _bunkers = CreateBunkers();
            _doubleShot = false;
            _doubleShotUntil = 0;
            _totalShotsFired = 0;
            _newHighScoreFired = false;
            Started = true;
            Won = false;
            GameOver = false;
            _lostALife = false;
            _gameTime.TotalTime = time;
            _lastUpdate = time;
            _player = new Player(_width, _height);
            _points = 0;
            _lives = 3;
            _livesAtWaveStart = 3;
            _lastAliveCount = 55;
            _updateMarch = true;
            await Task.CompletedTask;
        }

        public bool Paused { get; private set; }
        public void TogglePause() => Paused = !Paused;

        private List<Bunker> CreateBunkers()
        {
            var spacing = _width / 5;
            return Enumerable.Range(0, 4)
                .Select(i => new Bunker(new Point(spacing * (i + 1) - Bunker.Size.Width / 2, _height - 120)))
                .ToList();
        }

        private List<Alien> CreateAlienGrid() =>
            Enumerable.Range(0, 11).SelectMany(i => new[]
            {
                new Alien(AlienType.Squid,   new Point(i * 60 + 120, 100), i, 0),
                new Alien(AlienType.Crab,    new Point(i * 60 + 120, 145), i, 1),
                new Alien(AlienType.Crab,    new Point(i * 60 + 120, 190), i, 2),
                new Alien(AlienType.Octopus, new Point(i * 60 + 120, 235), i, 3),
                new Alien(AlienType.Octopus, new Point(i * 60 + 120, 270), i, 4),
            }).ToList();

        private int GetMotherShipScore() =>
            MotherShipScoreTable[_totalShotsFired % MotherShipScoreTable.Length];

        private int GetAlienPoints(int row) => row switch
        {
            0      => 30,
            1 or 2 => 20,
            _      => 10,
        };

        public void Update(float timeStamp)
        {
            if (!Started || Paused)
                return;

            _gameTime.TotalTime = timeStamp;

            if (_doubleShot && timeStamp > _doubleShotUntil)
                _doubleShot = false;

            UpdateBombs();
            UpdateAliens();
            UpdateShots();
            UpdatePowerUps();

            if (_aliens.All(_ => _.Destroyed))
            {
                // Perfect wave bonus
                if (_lives == _livesAtWaveStart)
                {
                    _points += 500;
                    _perfectBonusUntil = timeStamp + 2500;
                }

                _gameSpeed += 0.75;
                _wave++;
                _waveDisplayUntil = timeStamp + 2000;
                _livesAtWaveStart = _lives;
                _aliens = CreateAlienGrid();
                _bunkers = CreateBunkers();
                _lastAliveCount = 55;
                _updateMarch = true;
            }

            if (_motherShip == null && _random.Next(100) > 90)
                _motherShip = new MotherShip(new Point(_width, 80));
        }

        public async ValueTask Render(float timeStamp = 0)
        {
            if (_context == null) return;

            await _context.ClearRectAsync(0, 0, _width, _height);
            await _context.BeginBatchAsync();

            await RenderStars();

            await _context.SetFillStyleAsync("white");
            await _context.SetFontAsync("18px consolas");
            var header = $"Score: {_points:D5}   Hi: {HighScore} {HighScoreName}";
            if (_doubleShot) header += "  * 2x SHOT";
            await _context.FillTextAsync(header, 10, 22);

            if (GameOver)
                await RenderGameOver();
            else if (Started)
                await RenderGameFrame(timeStamp);
            else
                await RenderAttractScreen(timeStamp);

            if (Started && !GameOver && _gameTime.TotalTime <= _waveDisplayUntil)
                await RenderWaveIndicator();

            if (Started && !GameOver && timeStamp > 0 && timeStamp <= _perfectBonusUntil)
                await RenderPerfectBonus();

            if (Paused)
                await RenderPauseOverlay();

            await _context.EndBatchAsync();
            await ProcessAudio();
        }

        private async Task ProcessAudio()
        {
            if (_stopMarch)       { await _js.InvokeVoidAsync("gameAudio.stopMarch"); _stopMarch = false; }
            if (_updateMarch)     { await _js.InvokeVoidAsync("gameAudio.march", _lastAliveCount); _updateMarch = false; }
            if (_playShoot)       { await _js.InvokeVoidAsync("gameAudio.shoot"); _playShoot = false; }
            if (_playAlienDeath)  { await _js.InvokeVoidAsync("gameAudio.alienDeath"); _playAlienDeath = false; }
            if (_playPlayerDeath) { await _js.InvokeVoidAsync("gameAudio.playerDeath"); _playPlayerDeath = false; }
            if (_playMotherShip)  { await _js.InvokeVoidAsync("gameAudio.mothership"); _playMotherShip = false; }
        }

        public bool Started { get; private set; }
        public bool Won { get; private set; }
        public bool GameOver { get; private set; }
        public int HighScore { get; private set; }
        public string? HighScoreName { get; set; }
        public IEnumerable<Alien> Aliens => _aliens;
        public Player? Player => _player;
        public int Points => _points;

        public void MovePlayer(Direction direction) => Player?.Move(20, direction);

        public void Fire()
        {
            if (_player == null) return;
            var maxActive = _doubleShot ? 2 : 1;
            if (_shots.Count(s => !s.Remove) >= maxActive) return;

            _shots.Add(new Shot(new Point(_player.CurrentPosition.X + 13, _player.CurrentPosition.Y)));
            if (_doubleShot)
                _shots.Add(new Shot(new Point(_player.CurrentPosition.X + 25, _player.CurrentPosition.Y)));

            _totalShotsFired++;
            _playShoot = true;
        }

        private void UpdateShots()
        {
            foreach (var s in _shots.Where(_ => !_.Remove))
            {
                s.Move();

                foreach (var bunker in _bunkers.Where(b => !b.Destroyed))
                {
                    if (new Rectangle(bunker.Position, Bunker.Size).IntersectsWith(
                            new Rectangle(s.CurrentPosition, s.Sprite.RenderSize)))
                    {
                        bunker.Hit();
                        s.Remove = true;
                        break;
                    }
                }
                if (s.Remove) continue;

                var hit = _aliens
                    .Where(_ => !_.Destroyed && !_.HasBeenHit)
                    .OrderByDescending(_ => _.CurrentPosition.Y)
                    .FirstOrDefault(_ => _.Collision(s));

                if (hit != null)
                {
                    s.Remove = true;
                    hit.HitTime = _gameTime.TotalTime;
                    _points += GetAlienPoints(hit.Row);
                    _playAlienDeath = true;

                    if (_random.Next(100) < 5)
                        _powerUps.Add(new PowerUp(hit.CurrentPosition));

                    var alive = _aliens.Count(a => !a.Destroyed && !a.HasBeenHit);
                    if (alive != _lastAliveCount)
                    {
                        _lastAliveCount = alive;
                        _updateMarch = true;
                    }
                }

                if (_motherShip != null && !_motherShip.HasBeenHit && _motherShip.Collision(s))
                {
                    _motherShip.Score = GetMotherShipScore();
                    _points += _motherShip.Score;
                    s.Remove = true;
                    _playAlienDeath = true;
                }
            }
        }

        private void UpdateAliens()
        {
            var alive = _aliens.Count(a => !a.Destroyed && !a.HasBeenHit);
            var isFrenzy = alive == 1;

            // In frenzy mode the last alien moves every ~50ms; otherwise use normal speed
            var threshold = isFrenzy ? 50 : (250 - 100 * _gameSpeed);
            if (_gameTime.TotalTime - _lastUpdate <= threshold)
                return;

            _lastUpdate = _gameTime.TotalTime;

            if (_aliens.Any(_ => (_.CurrentPosition.X + 80) >= (_width - 10)) && _currentMovement == Direction.Right)
            {
                _aliens.ForEach(_ => _.MoveDown());
                _currentMovement = Direction.Left;
            }
            else if (_aliens.Any(_ => _.CurrentPosition.X <= 15) && _currentMovement == Direction.Left)
            {
                _aliens.ForEach(_ => _.MoveDown());
                _currentMovement = Direction.Right;
            }
            else
            {
                _aliens.ForEach(_ => _.MoveHorizontal(_currentMovement));
            }

            _aliens
                .Where(_ => _.HasBeenHit && _.HitTime >= 0 && _gameTime.TotalTime - _.HitTime > 300)
                .ToList()
                .ForEach(_ => _.Destroyed = true);

            if (_player != null && (_aliens.Any(_ => !_.Destroyed && _player.Collision(_)) ||
                    _aliens.Any(_ => !_.Destroyed && _.CurrentPosition.Y > 450)))
            {
                _lives = 0;
                GameOver = true;
                _stopMarch = true;
            }

            if (_motherShip?.Destroyed ?? false) _motherShip = null;
            if (_motherShip?.HasBeenHit ?? false) _motherShip.Destroyed = true;
        }

        private void UpdateBombs()
        {
            var alive = _aliens.Count(a => !a.Destroyed && !a.HasBeenHit);
            var isFrenzy = alive == 1;

            // In frenzy the last alien always fires; otherwise use normal probability
            var shouldSpawn = isFrenzy
                ? !_bombs.Any(_ => !_.Remove)
                : (!_bombs.Any() || _bombs.All(_ => _.Remove)) &&
                  _random.Next(0, 100) > (85 + (_aliens.Count(_ => _.Destroyed) / 5));

            if (shouldSpawn)
            {
                var xColumn = _random.Next(0, 11);
                var alien = _aliens.OrderByDescending(_ => _.CurrentPosition.Y)
                    .FirstOrDefault(_ => _.Column == xColumn && !_.Destroyed);
                var counter = 0;
                while (alien == null && counter < 60)
                {
                    xColumn = _random.Next(0, 11);
                    alien = _aliens.OrderByDescending(_ => _.CurrentPosition.Y)
                        .FirstOrDefault(_ => _.Column == xColumn && !_.Destroyed);
                    counter++;
                }
                if (alien != null)
                {
                    var type = (BombType)_random.Next(3);
                    _bombs.Add(new Bomb(
                        new Point(alien.CurrentPosition.X + alien.Sprite.RenderSize.Width / 2, alien.CurrentPosition.Y),
                        type));
                }
            }

            foreach (var b in _bombs.Where(_ => !_.Remove))
            {
                b.Move();

                foreach (var bunker in _bunkers.Where(bu => !bu.Destroyed))
                {
                    if (new Rectangle(bunker.Position, Bunker.Size).IntersectsWith(
                            new Rectangle(b.CurrentPosition, b.Sprite.RenderSize)))
                    {
                        bunker.Hit();
                        b.Remove = true;
                        break;
                    }
                }
                if (b.Remove) continue;

                if (_player != null && _player.Collision(b))
                {
                    _lives--;
                    GameOver = _lives < 1;
                    if (GameOver) _stopMarch = true;
                    _lostALife = true;
                    b.Remove = true;
                    _removeLifeBanner = _gameTime.TotalTime;
                    _playPlayerDeath = true;
                }
            }

            if (_lostALife && _gameTime.TotalTime - _removeLifeBanner > 1000)
                _lostALife = false;
        }

        private void UpdatePowerUps()
        {
            foreach (var p in _powerUps.Where(_ => !_.Collected))
            {
                p.Move();
                if (p.Position.Y > _height) { p.Collected = true; continue; }
                if (_player != null &&
                    new Rectangle(_player.CurrentPosition, _player.Sprite.RenderSize)
                        .IntersectsWith(new Rectangle(p.Position, PowerUp.Size)))
                {
                    p.Collected = true;
                    _doubleShot = true;
                    _doubleShotUntil = _gameTime.TotalTime + 10000;
                }
            }
        }

        private async Task RenderStars()
        {
            foreach (var (x, y, brightness, size) in _stars)
            {
                await _context!.SetFillStyleAsync($"rgba(255,255,255,{brightness:F2})");
                await _context.FillRectAsync(x, y, size, size);
            }
        }

        private async Task RenderAttractScreen(float timeStamp)
        {
            if (_context == null) return;

            // Cycle attract states every 4 seconds
            if (timeStamp > 0 && timeStamp - _attractStateStart > 4000)
            {
                _attractState = (_attractState + 1) % 3;
                _attractStateStart = timeStamp;
            }

            await _context.SetFillStyleAsync("white");

            switch (_attractState)
            {
                case 0:
                    await RenderAttractPointChart();
                    break;
                case 1:
                    await RenderAttractLeaderboard();
                    break;
                default:
                    await RenderAttractStart();
                    break;
            }
        }

        private async Task RenderAttractPointChart()
        {
            if (_context == null) return;
            await _context.SetFontAsync("22px consolas");
            await _context.SetFillStyleAsync("#FFD700");
            var title = "SCORE ADVANCE TABLE";
            var tl = await _context.MeasureTextAsync(title);
            await _context.FillTextAsync(title, _width / 2 - (tl.Width / 2), 120);

            var rows = new[]
            {
                (sprite: new Sprite(12, 73, 24, 80), label: "= ???  PTS", tint: "rgba(255,0,0,0.22)"),
                (sprite: new Sprite(0, 27, 20, 41),  label: "= 30  PTS", tint: RowTints[0]),
                (sprite: new Sprite(0, 0, 20, 12),   label: "= 20  PTS", tint: RowTints[1]),
                (sprite: new Sprite(20, 14, 40, 26),  label: "= 10  PTS", tint: RowTints[3]),
            };

            var y = 170;
            foreach (var (sprite, label, tint) in rows)
            {
                var sx = _width / 2 - 80;
                var sy = y - sprite.RenderSize.Height;
                await _context.SetFillStyleAsync(tint);
                await _context.FillRectAsync(sx, sy, sprite.RenderSize.Width, sprite.RenderSize.Height);
                await _context.DrawImageAsync(_spriteSheet,
                    sprite.TopLeft.X, sprite.TopLeft.Y, sprite.Size.Width, sprite.Size.Height,
                    sx, sy, sprite.RenderSize.Width, sprite.RenderSize.Height);
                await _context.SetFillStyleAsync("white");
                await _context.SetFontAsync("20px consolas");
                await _context.FillTextAsync(label, _width / 2 - 40, y);
                y += 50;
            }
        }

        private async Task RenderAttractLeaderboard()
        {
            if (_context == null) return;
            await _context.SetFontAsync("26px consolas");
            await _context.SetFillStyleAsync("#FFD700");
            var title = "TOP SCORES";
            var tl = await _context.MeasureTextAsync(title);
            await _context.FillTextAsync(title, _width / 2 - (tl.Width / 2), 130);

            await _context.SetFontAsync("22px consolas");
            await _context.SetFillStyleAsync("white");

            if (_topScores.Count == 0)
            {
                var none = "NO SCORES YET";
                var nl = await _context.MeasureTextAsync(none);
                await _context.FillTextAsync(none, _width / 2 - (nl.Width / 2), 200);
            }
            else
            {
                var y = 180;
                for (int i = 0; i < _topScores.Count; i++)
                {
                    var line = $"{i + 1}.  {_topScores[i].Name,-5} {_topScores[i].Score:D5}";
                    var ll = await _context.MeasureTextAsync(line);
                    await _context.FillTextAsync(line, _width / 2 - (ll.Width / 2), y);
                    y += 45;
                }
            }
        }

        private async Task RenderAttractStart()
        {
            if (_context == null) return;
            await _context.SetFontAsync("28px consolas");
            await _context.SetFillStyleAsync("white");
            var t = "Use arrow keys to move, space to fire";
            var l = await _context.MeasureTextAsync(t);
            await _context.FillTextAsync(t, _width / 2 - (l.Width / 2), 160);
            t = "Press space to start";
            l = await _context.MeasureTextAsync(t);
            await _context.FillTextAsync(t, _width / 2 - (l.Width / 2), 220);
            await _context.SetFillStyleAsync("#888");
            await _context.SetFontAsync("18px consolas");
            t = "P / Escape to pause";
            l = await _context.MeasureTextAsync(t);
            await _context.FillTextAsync(t, _width / 2 - (l.Width / 2), 265);
        }

        private async Task RenderGameOver()
        {
            if (_context == null) return;
            Started = false;

            if (_points > HighScore && !_newHighScoreFired)
            {
                _newHighScoreFired = true;
                HighScore = _points;
                NewHighScore?.Invoke(this, EventArgs.Empty);
            }

            await _context.SetFontAsync("36px consolas");
            var text = "GAME OVER";
            var length = await _context.MeasureTextAsync(text);
            await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), _height / 2 - 30);
            await _context.SetFontAsync("22px consolas");
            text = "Press space to start a new game";
            length = await _context.MeasureTextAsync(text);
            await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), _height / 2 + 20);
        }

        private async Task RenderGameFrame(float timeStamp)
        {
            if (_context == null) return;

            await RenderBunkers();

            if (_lostALife)
            {
                await _context.SetFillStyleAsync("red");
                await _context.SetFontAsync("20px consolas");
                var msg = "You were hit!";
                var ml = await _context.MeasureTextAsync(msg);
                await _context.FillTextAsync(msg, _width / 2 - (ml.Width / 2), 55);
                await _context.SetFillStyleAsync("white");
            }

            // Check last-alien frenzy
            var aliveCount = _aliens.Count(a => !a.Destroyed && !a.HasBeenHit);
            if (aliveCount == 1)
            {
                await _context.SetFillStyleAsync("#FF4444");
                await _context.SetFontAsync("16px consolas");
                await _context.FillTextAsync("FRENZY!", 10, 45);
                await _context.SetFillStyleAsync("white");
            }

            foreach (var a in Aliens.Where(a => !a.Destroyed))
            {
                var tint = a.Row < RowTints.Length ? RowTints[a.Row] : "rgba(255,255,255,0.18)";
                await _context.SetFillStyleAsync(tint);
                await _context.FillRectAsync(a.CurrentPosition.X, a.CurrentPosition.Y,
                    a.Sprite.RenderSize.Width, a.Sprite.RenderSize.Height);
                await _context.DrawImageAsync(_spriteSheet,
                    a.Sprite.TopLeft.X, a.Sprite.TopLeft.Y, a.Sprite.Size.Width, a.Sprite.Size.Height,
                    a.CurrentPosition.X, a.CurrentPosition.Y, a.Sprite.RenderSize.Width, a.Sprite.RenderSize.Height);
            }

            if (_player != null)
            {
                var ship = _player.Sprite;
                await _context.DrawImageAsync(_spriteSheet,
                    ship.TopLeft.X, ship.TopLeft.Y, ship.Size.Width, ship.Size.Height,
                    _player.CurrentPosition.X, _player.CurrentPosition.Y, ship.RenderSize.Width, ship.RenderSize.Height);
            }

            foreach (var s in _shots.Where(_ => !_.Remove))
            {
                if (s.CurrentPosition.Y <= 0) { s.Remove = true; continue; }
                await _context.DrawImageAsync(_spriteSheet,
                    s.Sprite.TopLeft.X, s.Sprite.TopLeft.Y, s.Sprite.Size.Width, s.Sprite.Size.Height,
                    s.CurrentPosition.X, s.CurrentPosition.Y, s.Sprite.RenderSize.Width, s.Sprite.RenderSize.Height);
            }

            foreach (var b in _bombs.Where(_ => !_.Remove))
            {
                if (b.CurrentPosition.Y >= _height) { b.Remove = true; continue; }
                await _context.DrawImageAsync(_spriteSheet,
                    b.Sprite.TopLeft.X, b.Sprite.TopLeft.Y, b.Sprite.Size.Width, b.Sprite.Size.Height,
                    b.CurrentPosition.X, b.CurrentPosition.Y, b.Sprite.RenderSize.Width, b.Sprite.RenderSize.Height);
            }

            if (_motherShip != null)
            {
                if (!_motherShip.HasBeenHit) _motherShip.Move();
                if (_motherShip.CurrentPosition.X < 0)
                {
                    _motherShip = null;
                }
                else
                {
                    await _context.DrawImageAsync(_spriteSheet,
                        _motherShip.Sprite.TopLeft.X, _motherShip.Sprite.TopLeft.Y,
                        _motherShip.Sprite.Size.Width, _motherShip.Sprite.Size.Height,
                        _motherShip.CurrentPosition.X, _motherShip.CurrentPosition.Y,
                        _motherShip.Sprite.RenderSize.Width, _motherShip.Sprite.RenderSize.Height);

                    if (_motherShip.HasBeenHit)
                    {
                        await _context.SetFillStyleAsync("#FFD700");
                        await _context.SetFontAsync("14px consolas");
                        await _context.FillTextAsync($"+{_motherShip.Score}",
                            _motherShip.CurrentPosition.X, _motherShip.CurrentPosition.Y - 5);
                        await _context.SetFillStyleAsync("white");
                    }
                    else if (_gameTime.TotalTime - _lastMotherShipPing > 500)
                    {
                        _lastMotherShipPing = _gameTime.TotalTime;
                        _playMotherShip = true;
                    }
                }
            }

            await RenderPowerUps();
            await RenderLives();
        }

        private async Task RenderBunkers()
        {
            if (_context == null) return;
            foreach (var bunker in _bunkers.Where(b => !b.Destroyed))
            {
                var color = bunker.Health switch
                {
                    4 => "rgba(0,230,0,1.0)",
                    3 => "rgba(0,200,0,0.85)",
                    2 => "rgba(180,200,0,0.7)",
                    _ => "rgba(200,80,0,0.6)"
                };
                var x = bunker.Position.X;
                var y = bunker.Position.Y;
                var w = Bunker.Size.Width;
                var h = Bunker.Size.Height;
                await _context.SetFillStyleAsync(color);
                await _context.FillRectAsync(x + 10, y, w - 20, h - 8);
                await _context.FillRectAsync(x, y + 10, w, h - 10);
                await _context.SetFillStyleAsync("black");
                await _context.FillRectAsync(x + 18, y + 18, 24, h - 18);
            }
        }

        private async Task RenderPowerUps()
        {
            if (_context == null) return;
            foreach (var p in _powerUps.Where(_ => !_.Collected))
            {
                await _context.SetFillStyleAsync("#FFD700");
                await _context.BeginPathAsync();
                await _context.MoveToAsync(p.Position.X + 8, p.Position.Y);
                await _context.LineToAsync(p.Position.X + 16, p.Position.Y + 8);
                await _context.LineToAsync(p.Position.X + 8, p.Position.Y + 16);
                await _context.LineToAsync(p.Position.X, p.Position.Y + 8);
                await _context.ClosePathAsync();
                await _context.FillAsync();
            }
        }

        private async Task RenderLives()
        {
            if (_context == null) return;
            var shipSprite = new Sprite(20, 47, 38, 59);
            for (int i = 0; i < _lives - 1; i++)
            {
                var x = _width - (i + 1) * 28 - 10;
                await _context.DrawImageAsync(_spriteSheet,
                    shipSprite.TopLeft.X, shipSprite.TopLeft.Y,
                    shipSprite.Size.Width, shipSprite.Size.Height,
                    x, _height - 24, shipSprite.Size.Width, shipSprite.Size.Height);
            }
        }

        private async Task RenderWaveIndicator()
        {
            if (_context == null) return;
            await _context.SetFillStyleAsync("rgba(0,0,0,0.55)");
            await _context.FillRectAsync(0, _height / 2 - 45, _width, 65);
            await _context.SetFillStyleAsync("#00FF88");
            await _context.SetFontAsync("40px consolas");
            var text = $"WAVE {_wave}";
            var length = await _context.MeasureTextAsync(text);
            await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), _height / 2 + 5);
        }

        private async Task RenderPerfectBonus()
        {
            if (_context == null) return;
            await _context.SetFillStyleAsync("#FFD700");
            await _context.SetFontAsync("28px consolas");
            var text = "PERFECT!  +500";
            var length = await _context.MeasureTextAsync(text);
            await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), 80);
        }

        private async Task RenderPauseOverlay()
        {
            if (_context == null) return;
            await _context.SetFillStyleAsync("rgba(0,0,0,0.6)");
            await _context.FillRectAsync(0, 0, _width, _height);
            await _context.SetFillStyleAsync("white");
            await _context.SetFontAsync("48px consolas");
            var text = "PAUSED";
            var length = await _context.MeasureTextAsync(text);
            await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), _height / 2 - 10);
            await _context.SetFontAsync("20px consolas");
            text = "Press P to resume";
            length = await _context.MeasureTextAsync(text);
            await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), _height / 2 + 40);
        }
    }
}

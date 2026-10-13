using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    /// <summary>
    /// Game rules and state, independent of rendering and the browser.
    /// Feed it frame timestamps via <see cref="Update"/> and read its state to draw it.
    /// </summary>
    public class GameEngine
    {
        // Simulation runs in fixed 60 Hz steps regardless of display refresh rate
        public const float StepMs = 1000f / 60f;
        private const int MaxStepsPerFrame = 5;

        // Pixels per step while a direction is held (360 px/s)
        public const int PlayerSpeed = 6;

        // Deterministic mothership score table (authentic to original)
        private static readonly int[] MotherShipScoreTable =
            { 100, 50, 50, 100, 150, 100, 100, 50, 300, 100, 100, 100, 50, 150, 100 };

        private readonly int _width;
        private readonly int _height;
        private readonly Random _random;
        private readonly GameTime _gameTime = new();

        private List<Alien> _aliens = new();
        private List<Shot> _shots = new();
        private List<Bomb> _bombs = new();
        private List<Bunker> _bunkers = new();
        private List<PowerUp> _powerUps = new();
        private List<HighScore> _topScores = new();
        private Player? _player;
        private MotherShip? _motherShip;

        private float _lastTimeStamp;
        private float _stepAccumulator;
        private float _lastUpdate;
        private float _removeLifeBanner;
        private Direction _currentMovement;
        private GameInput _input;
        private int _points;
        private int _lives;
        private double _gameSpeed;

        private int _wave;
        private float _waveDisplayUntil;
        private int _livesAtWaveStart;
        private float _perfectBonusUntil;

        private bool _doubleShot;
        private float _doubleShotUntil;

        private int _totalShotsFired;
        private readonly ComboTracker _combo = new();
        private int _lastAliveCount;
        private float _lastMotherShipPing;

        private SoundEffect _sounds;
        private int _marchCount = -1;
        private readonly List<GameEvent> _events = new();

        public GameEngine(int width, int height, Random? random = null)
        {
            _width = width;
            _height = height;
            _random = random ?? new Random();
        }

        /// <summary>Raised once when a game ends with a score good enough for the top 10.</summary>
        public event Action? QualifiedForLeaderboard;

        public int Width => _width;
        public int Height => _height;
        public bool Started { get; private set; }
        public bool GameOver { get; private set; }
        public bool Paused { get; private set; }
        public int HighScore { get; private set; }
        public string? HighScoreName { get; set; }
        public int Points => _points;
        public int Lives => _lives;
        public int Wave => _wave;
        public bool DoubleShot => _doubleShot;
        public bool LostALife { get; private set; }

        /// <summary>Consecutive hits; see <see cref="ComboTracker"/>.</summary>
        public int Combo => _combo.Count;
        /// <summary>Applied to alien points: x2 after 5 hits in a row, up to x4.</summary>
        public int Multiplier => _combo.Multiplier;
        internal ComboTracker ComboTracker => _combo;

        /// <summary>Game clock in ms. Only advances while a game is running and unpaused.</summary>
        public float Time => _gameTime.TotalTime;

        public int AliveCount => _aliens.Count(a => !a.Destroyed && !a.HasBeenHit);
        public bool IsFrenzy => AliveCount == 1;
        public bool ShowWaveBanner => Started && !GameOver && Time <= _waveDisplayUntil;
        public bool ShowPerfectBonus => Started && !GameOver && Time <= _perfectBonusUntil;

        public IReadOnlyList<Alien> Aliens => _aliens;
        public IReadOnlyList<Shot> Shots => _shots;
        public IReadOnlyList<Bomb> Bombs => _bombs;
        public IReadOnlyList<Bunker> Bunkers => _bunkers;
        public IReadOnlyList<PowerUp> PowerUps => _powerUps;
        public IReadOnlyList<HighScore> TopScores => _topScores;
        public Player? Player => _player;
        public MotherShip? MotherShip => _motherShip;

        public static bool QualifiesForLeaderboard(int points, IReadOnlyList<HighScore> topScores) =>
            points > 0 && (topScores.Count < 10 || points > topScores[^1].Score);

        public void SetLeaderboard(IEnumerable<HighScore> scores)
        {
            _topScores = scores.Take(10).ToList();
            if (_topScores.Count > 0 && _topScores[0].Score >= HighScore)
            {
                HighScore = _topScores[0].Score;
                HighScoreName = _topScores[0].Name;
            }
        }

        public void Start(float time)
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
            _combo.Break();
            Started = true;
            GameOver = false;
            LostALife = false;
            _gameTime.TotalTime = time;
            _lastTimeStamp = time;
            _stepAccumulator = 0;
            _lastUpdate = time;
            _player = new Player(_width, _height);
            _points = 0;
            _lives = 3;
            _livesAtWaveStart = 3;
            _lastAliveCount = 55;
            _marchCount = _lastAliveCount;
            _events.Clear();
        }

        public void TogglePause() => Paused = !Paused;

        /// <summary>Sets the controls currently held; applied on every step until changed.</summary>
        public void SetInput(GameInput input) => _input = input;

        public void Fire()
        {
            if (_player == null) return;
            var maxActive = _doubleShot ? 2 : 1;
            if (_shots.Count(s => !s.Remove) >= maxActive) return;

            var volley = _combo.StartVolley(_doubleShot ? 2 : 1);
            _shots.Add(new Shot(new Point(_player.CurrentPosition.X + 13, _player.CurrentPosition.Y)) { Volley = volley });
            if (_doubleShot)
                _shots.Add(new Shot(new Point(_player.CurrentPosition.X + 25, _player.CurrentPosition.Y)) { Volley = volley });

            _totalShotsFired++;
            _sounds |= SoundEffect.Shoot;
        }

        /// <summary>
        /// Returns the sounds triggered since the last call and clears them.
        /// <paramref name="marchCount"/> is the new alien count for the march tempo, or -1 if unchanged.
        /// </summary>
        public SoundEffect TakeSounds(out int marchCount)
        {
            var sounds = _sounds;
            marchCount = _marchCount;
            _sounds = SoundEffect.None;
            _marchCount = -1;
            return sounds;
        }

        /// <summary>Returns the events raised since the last call and clears them.</summary>
        public GameEvent[] TakeEvents()
        {
            if (_events.Count == 0)
                return Array.Empty<GameEvent>();
            var events = _events.ToArray();
            _events.Clear();
            return events;
        }

        private void Raise(GameEventType type, Point position, Size size, int value = 0, int variant = 0) =>
            _events.Add(new GameEvent(type, position.X + size.Width / 2, position.Y + size.Height / 2, value, variant));

        public void Update(float timeStamp)
        {
            var elapsed = timeStamp - _lastTimeStamp;
            _lastTimeStamp = timeStamp;

            if (!Started || Paused)
                return;

            // Cap catch-up so a background tab doesn't fast-forward the game on return
            _stepAccumulator = Math.Min(_stepAccumulator + Math.Max(elapsed, 0), StepMs * MaxStepsPerFrame);
            while (_stepAccumulator >= StepMs && !GameOver)
            {
                _stepAccumulator -= StepMs;
                Step();
            }
        }

        private void Step()
        {
            // Game clock only advances while playing, so timers freeze during pause
            _gameTime.TotalTime += StepMs;
            var now = _gameTime.TotalTime;

            if (_doubleShot && now > _doubleShotUntil)
                _doubleShot = false;

            ApplyInput();
            UpdateBombs();
            UpdateAliens();
            UpdateShots();
            UpdatePowerUps();
            UpdateMotherShip();

            // Drop spent entities so the lists don't grow for the whole game
            _shots.RemoveAll(s => s.Remove);
            _bombs.RemoveAll(b => b.Remove);
            _powerUps.RemoveAll(p => p.Collected);

            if (_aliens.All(_ => _.Destroyed))
            {
                // Perfect wave bonus
                if (_lives == _livesAtWaveStart)
                {
                    _points += 500;
                    _perfectBonusUntil = now + 2500;
                }

                _gameSpeed += 0.75;
                _wave++;
                _waveDisplayUntil = now + 2000;
                _livesAtWaveStart = _lives;
                _aliens = CreateAlienGrid();
                _bunkers = CreateBunkers();
                _lastAliveCount = 55;
                _marchCount = _lastAliveCount;
            }
        }

        private void ApplyInput()
        {
            if (_player == null) return;
            if (_input.HasFlag(GameInput.Left))
                _player.Move(PlayerSpeed, Direction.Left);
            if (_input.HasFlag(GameInput.Right))
                _player.Move(PlayerSpeed, Direction.Right);
            // Holding fire shoots again as soon as Fire allows another shot
            if (_input.HasFlag(GameInput.Fire))
                Fire();
        }

        private void EndGame()
        {
            if (GameOver) return;

            GameOver = true;
            Started = false;
            _sounds |= SoundEffect.StopMarch;

            if (QualifiesForLeaderboard(_points, _topScores))
            {
                if (_points > HighScore)
                    HighScore = _points;
                QualifiedForLeaderboard?.Invoke();
            }
        }

        private List<Bunker> CreateBunkers()
        {
            var spacing = _width / 5;
            return Enumerable.Range(0, 4)
                .Select(i => new Bunker(new Point(spacing * (i + 1) - Bunker.Size.Width / 2, _height - 120)))
                .ToList();
        }

        private static List<Alien> CreateAlienGrid() =>
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

        private static int GetAlienPoints(int row) => row switch
        {
            0      => 30,
            1 or 2 => 20,
            _      => 10,
        };

        private void UpdateShots()
        {
            foreach (var s in _shots.Where(_ => !_.Remove))
            {
                s.Move();
                if (s.CurrentPosition.Y <= 0) { EndShot(s); continue; }

                foreach (var bunker in _bunkers.Where(b => !b.Destroyed))
                {
                    if (new Rectangle(bunker.Position, Bunker.Size).IntersectsWith(
                            new Rectangle(s.CurrentPosition, s.Sprite.RenderSize)))
                    {
                        bunker.Hit();
                        EndShot(s);
                        Raise(GameEventType.BunkerHit, s.CurrentPosition, s.Sprite.RenderSize, variant: bunker.Health);
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
                    hit.HitTime = _gameTime.TotalTime;
                    var points = GetAlienPoints(hit.Row) * _combo.Multiplier;
                    _points += points;
                    _sounds |= SoundEffect.AlienDeath;
                    Raise(GameEventType.AlienKilled, hit.CurrentPosition, hit.Sprite.RenderSize, points, hit.Row);
                    RegisterHit(s);
                    EndShot(s);

                    if (_random.Next(100) < 5)
                        _powerUps.Add(new PowerUp(hit.CurrentPosition));

                    var alive = AliveCount;
                    if (alive != _lastAliveCount)
                    {
                        _lastAliveCount = alive;
                        _marchCount = alive;
                    }
                }

                if (_motherShip != null && !_motherShip.HasBeenHit && _motherShip.Collision(s))
                {
                    // Mothership keeps its authentic score table; the hit still counts for the combo
                    _motherShip.Score = GetMotherShipScore();
                    _points += _motherShip.Score;
                    _sounds |= SoundEffect.AlienDeath;
                    Raise(GameEventType.MotherShipKilled, _motherShip.CurrentPosition, _motherShip.Sprite.RenderSize, _motherShip.Score);
                    RegisterHit(s);
                    EndShot(s);
                }
            }
        }

        private void RegisterHit(Shot s)
        {
            if (_combo.AddHit(s.Volley) && _player != null)
                Raise(GameEventType.ComboUp, _player.CurrentPosition, _player.Sprite.RenderSize, _combo.Multiplier);
        }

        /// <summary>Removes a shot; safe to call twice when one shot hits two targets in a step.</summary>
        private void EndShot(Shot s)
        {
            if (s.Remove) return;
            s.Remove = true;
            _combo.ShotEnded(s.Volley);
        }

        private void UpdateAliens()
        {
            // In frenzy mode the last alien moves every ~50ms; otherwise use normal speed
            var threshold = IsFrenzy ? 50 : (250 - 100 * _gameSpeed);
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
                Raise(GameEventType.PlayerHit, _player.CurrentPosition, _player.Sprite.RenderSize);
                _lives = 0;
                EndGame();
            }

            if (_motherShip?.Destroyed ?? false) _motherShip = null;
            if (_motherShip?.HasBeenHit ?? false) _motherShip.Destroyed = true;
        }

        private void UpdateBombs()
        {
            // In frenzy the last alien always fires; otherwise use normal probability
            var shouldSpawn = IsFrenzy
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
                if (b.CurrentPosition.Y >= _height) { b.Remove = true; continue; }

                foreach (var bunker in _bunkers.Where(bu => !bu.Destroyed))
                {
                    if (new Rectangle(bunker.Position, Bunker.Size).IntersectsWith(
                            new Rectangle(b.CurrentPosition, b.Sprite.RenderSize)))
                    {
                        bunker.Hit();
                        b.Remove = true;
                        Raise(GameEventType.BunkerHit, b.CurrentPosition, b.Sprite.RenderSize, variant: bunker.Health);
                        break;
                    }
                }
                if (b.Remove) continue;

                if (_player != null && _player.Collision(b))
                {
                    _lives--;
                    LostALife = true;
                    b.Remove = true;
                    _removeLifeBanner = _gameTime.TotalTime;
                    _sounds |= SoundEffect.PlayerDeath;
                    Raise(GameEventType.PlayerHit, _player.CurrentPosition, _player.Sprite.RenderSize);
                    _combo.Break();
                    if (_lives < 1)
                        EndGame();
                }
            }

            if (LostALife && _gameTime.TotalTime - _removeLifeBanner > 1000)
                LostALife = false;
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

        private void UpdateMotherShip()
        {
            if (_motherShip == null)
            {
                if (_random.Next(100) > 90)
                    _motherShip = new MotherShip(new Point(_width, 80));
                return;
            }

            if (!_motherShip.HasBeenHit)
                _motherShip.Move();

            if (_motherShip.CurrentPosition.X < 0)
            {
                _motherShip = null;
                return;
            }

            if (!_motherShip.HasBeenHit && _gameTime.TotalTime - _lastMotherShipPing > 500)
            {
                _lastMotherShipPing = _gameTime.TotalTime;
                _sounds |= SoundEffect.MotherShip;
            }
        }
    }
}

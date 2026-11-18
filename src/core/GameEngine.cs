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

        public const int MaxLives = 5;
        private const int DefaultBossEvery = 5;
        /// <summary>Points per boss hit before the combo multiplier. For tests.</summary>
        internal const int BossHitPoints = 50;
        /// <summary>Kill bonus per boss defeated so far. For tests.</summary>
        internal const int BossKillBonus = 1000;
        private const int PowerUpDropPercent = 5;
        private const float RapidFireIntervalMs = 120;

        // Relative drop chance of each power-up type
        private static readonly (PowerUpType Type, int Weight)[] PowerUpWeights =
        {
            (PowerUpType.DoubleShot, 30),
            (PowerUpType.RapidFire,  25),
            (PowerUpType.Shield,     20),
            (PowerUpType.Piercing,   20),
            (PowerUpType.ExtraLife,   5),
        };

        // Deterministic mothership score table (authentic to original)
        /// <summary>For tests.</summary>
        internal static readonly int[] MotherShipScoreTable =
            { 100, 50, 50, 100, 150, 100, 100, 50, 300, 100, 100, 100, 50, 150, 100 };

        private readonly int _width;
        private readonly int _height;
        // Seeds each game's random streams, unless it is a daily challenge
        private readonly Random _random;
        private readonly GameTime _gameTime = new();

        // One random stream per subsystem, so the daily seed gives every player the same
        // sequence: bomb and mothership rolls are drawn every step, drop rolls once per kill
        private Random _bombRng = new(0);
        private Random _shipRng = new(0);
        private Random _dropRng = new(0);
        private int _scoreFactor = 1;

        private List<Alien> _aliens = new();
        private List<Shot> _shots = new();
        private List<Bomb> _bombs = new();
        private List<Bunker> _bunkers = new();
        private List<PowerUp> _powerUps = new();
        private List<HighScore> _topScores = new();
        private Player? _player;
        private MotherShip? _motherShip;
        private Boss? _boss;
        private float _bossNextAttack;

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
        private bool _hitThisWave;
        private float _perfectBonusUntil;

        // Game time at which each timed power-up runs out, indexed by PowerUpType
        private readonly float[] _powerUpUntil = new float[Enum.GetValues<PowerUpType>().Length];
        private bool _shield;
        private float _lastFireTime;

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
        /// <summary>Waves between bosses: 5, or 3 with the BossRush daily modifier.</summary>
        public int BossEvery { get; private set; } = DefaultBossEvery;
        public bool IsBossWave => _wave % BossEvery == 0;
        /// <summary>Today's challenge when this is a daily game, otherwise null.</summary>
        public DailyChallenge? Daily { get; private set; }
        public bool IsDaily => Daily != null;
        /// <summary>Points for clearing a wave without being hit (doubled in a Glass daily).</summary>
        public int PerfectWaveBonus => 500 * _scoreFactor;
        /// <summary>The boss on a boss wave, otherwise null.</summary>
        public Boss? Boss => _boss;
        public bool DoubleShot => IsActive(PowerUpType.DoubleShot);
        public bool HasShield => _shield;
        public bool LostALife { get; private set; }

        /// <summary>Whether a power-up is in effect. Extra life is instant, so never active.</summary>
        public bool IsActive(PowerUpType type) => type switch
        {
            PowerUpType.Shield => _shield,
            PowerUpType.ExtraLife => false,
            _ => Time < _powerUpUntil[(int)type],
        };

        /// <summary>Time left on a timed power-up in ms, or 0.</summary>
        public float RemainingMs(PowerUpType type) =>
            IsActive(type) && type is not PowerUpType.Shield ? _powerUpUntil[(int)type] - Time : 0;

        /// <summary>Consecutive hits; see <see cref="ComboTracker"/>.</summary>
        public int Combo => _combo.Count;
        /// <summary>Applied to alien points: x2 after 5 hits in a row, up to x4.</summary>
        public int Multiplier => _combo.Multiplier;
        internal ComboTracker ComboTracker => _combo;

        /// <summary>Stops aliens dropping bombs, for tests about other things (the rolls are still drawn).</summary>
        internal bool AliensHoldFire { get; set; }

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

        /// <summary>
        /// Sets the board that qualification and the "Hi" score are measured against: the all-time
        /// board, or the day's board for a daily challenge. An empty list clears it.
        /// </summary>
        public void SetLeaderboard(IEnumerable<HighScore> scores)
        {
            _topScores = scores.Take(10).ToList();
            HighScore = _topScores.Count > 0 ? _topScores[0].Score : 0;
            HighScoreName = _topScores.Count > 0 ? _topScores[0].Name : null;
        }

        /// <param name="startWave">Wave to begin on; later waves start at the speed they would have reached.</param>
        /// <param name="daily">Play this day's challenge: its seed and modifier.</param>
        public void Start(float time, int startWave = 1, DailyChallenge? daily = null)
        {
            Daily = daily;
            var modifier = daily?.Modifier ?? DailyModifier.None;
            // A daily game seeds from the date so everyone gets the same sequence
            var seeds = daily is null ? _random : new Random(daily.Seed);
            _bombRng = new Random(seeds.Next());
            _shipRng = new Random(seeds.Next());
            _dropRng = new Random(seeds.Next());
            BossEvery = modifier == DailyModifier.BossRush ? 3 : DefaultBossEvery;
            _scoreFactor = modifier == DailyModifier.Glass ? 2 : 1;

            _wave = Math.Max(1, startWave);
            _gameSpeed = 0.75 * (_wave - 1) + (modifier == DailyModifier.Speedrun ? 0.75 : 0);
            _shots = new List<Shot>();
            _bombs = new List<Bomb>();
            _powerUps = new List<PowerUp>();
            _motherShip = null;
            Array.Clear(_powerUpUntil);
            _shield = modifier == DailyModifier.ShieldStart;
            _lastFireTime = float.NegativeInfinity;
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
            _lives = modifier == DailyModifier.Glass ? 1 : 3;
            _events.Clear();
            BeginWave(time);
        }

        /// <summary>Adds points, doubled in a Glass daily. Returns the points actually awarded.</summary>
        private int Award(int points)
        {
            var awarded = points * _scoreFactor;
            _points += awarded;
            return awarded;
        }

        private void BeginWave(float now)
        {
            _waveDisplayUntil = now + 2000;
            _hitThisWave = false;
            _bunkers = CreateBunkers();

            if (IsBossWave)
            {
                _aliens = new List<Alien>();
                _boss = new Boss(30 + 10 * (_wave / BossEvery - 1), _width / 2 - Boss.Size.Width / 2);
                _bossNextAttack = now + 2000;   // first attack once the banner is gone
                _motherShip = null;
                _lastAliveCount = 0;
                _sounds |= SoundEffect.StopMarch;
            }
            else
            {
                _boss = null;
                _aliens = CreateAlienGrid();
                _lastAliveCount = 55;
                _marchCount = _lastAliveCount;
            }
        }

        public void TogglePause() => Paused = !Paused;

        /// <summary>Sets the controls currently held; applied on every step until changed.</summary>
        public void SetInput(GameInput input) => _input = input;

        public void Fire()
        {
            if (_player == null) return;
            var perVolley = DoubleShot ? 2 : 1;
            var rapid = IsActive(PowerUpType.RapidFire);
            var maxActive = (rapid ? 3 : 1) * perVolley;
            if (_shots.Count(s => !s.Remove) + perVolley > maxActive) return;
            // Space rapid-fire volleys out so they don't leave as one clump
            if (rapid && Time - _lastFireTime < RapidFireIntervalMs) return;

            var volley = _combo.StartVolley(perVolley);
            var piercing = IsActive(PowerUpType.Piercing);
            var y = _player.CurrentPosition.Y;
            _shots.Add(new Shot(new Point(_player.CurrentPosition.X + 13, y)) { Volley = volley, Piercing = piercing });
            if (perVolley == 2)
                _shots.Add(new Shot(new Point(_player.CurrentPosition.X + 25, y)) { Volley = volley, Piercing = piercing });

            _lastFireTime = Time;
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

            ApplyInput();
            UpdateBombs();
            UpdateAliens();
            UpdateBoss(now);
            UpdateShots();
            UpdatePowerUps();
            UpdateMotherShip();

            // Drop spent entities so the lists don't grow for the whole game
            _shots.RemoveAll(s => s.Remove);
            _bombs.RemoveAll(b => b.Remove);
            _powerUps.RemoveAll(p => p.Collected);

            // A boss wave has no aliens, so it is only cleared by defeating the boss
            var cleared = IsBossWave ? _boss is { Defeated: true } : _aliens.All(_ => _.Destroyed);
            if (cleared && !GameOver)
            {
                // Perfect wave bonus
                if (!_hitThisWave)
                {
                    _points += PerfectWaveBonus;
                    _perfectBonusUntil = now + 2500;
                }

                _gameSpeed += 0.75;
                _wave++;
                BeginWave(now);
            }
        }

        private void UpdateBoss(float now)
        {
            if (_boss == null || _boss.Defeated) return;

            _boss.Move(_width);
            if (now >= _bossNextAttack && _player != null)
            {
                var size = _player.Sprite.RenderSize;
                _bombs.AddRange(_boss.Attack(_player.CurrentPosition.X + size.Width / 2, _player.CurrentPosition.Y));
                _bossNextAttack = now + _boss.AttackIntervalMs;
            }
        }

        private void HitBoss(Shot s, float now)
        {
            var boss = _boss!;
            boss.Hit(now);
            Award(BossHitPoints * _combo.Multiplier);
            _sounds |= SoundEffect.BossHit;
            Raise(GameEventType.BossHit, s.CurrentPosition, s.Sprite.RenderSize);
            RegisterHit(s);
            // Piercing shots stop here too, or they would hit on every step they overlap
            EndShot(s);

            if (boss.Defeated)
            {
                var bonus = Award(BossKillBonus * (_wave / BossEvery));
                _sounds |= SoundEffect.BossDefeated;
                Raise(GameEventType.BossKilled, boss.Position, Boss.Size, bonus);
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
            if (Daily?.Modifier == DailyModifier.NoBunkers)
                return new List<Bunker>();

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

        /// <summary>Points for an alien in <paramref name="row"/>, before the combo multiplier. For tests.</summary>
        internal static int GetAlienPoints(int row) => row switch
        {
            0 => 30,
            1 or 2 => 20,
            _ => 10,
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
                    var points = Award(GetAlienPoints(hit.Row) * _combo.Multiplier);
                    _sounds |= SoundEffect.AlienDeath;
                    Raise(GameEventType.AlienKilled, hit.CurrentPosition, hit.Sprite.RenderSize, points, hit.Row);
                    RegisterHit(s);
                    if (!s.Piercing)
                        EndShot(s);

                    if (_dropRng.Next(100) < PowerUpDropPercent)
                        _powerUps.Add(new PowerUp(hit.CurrentPosition, RollPowerUpType()));

                    var alive = AliveCount;
                    if (alive != _lastAliveCount)
                    {
                        _lastAliveCount = alive;
                        _marchCount = alive;
                    }
                }

                if (!s.Remove && _boss != null && _boss.Collision(s))
                    HitBoss(s, _gameTime.TotalTime);

                if (_motherShip != null && !_motherShip.HasBeenHit && _motherShip.Collision(s))
                {
                    // Mothership keeps its authentic score table; the hit still counts for the combo
                    _motherShip.Score = GetMotherShipScore();
                    var awarded = Award(_motherShip.Score);
                    _sounds |= SoundEffect.AlienDeath;
                    Raise(GameEventType.MotherShipKilled, _motherShip.CurrentPosition, _motherShip.Sprite.RenderSize, awarded);
                    RegisterHit(s);
                    if (!s.Piercing)
                        EndShot(s);
                }
            }
        }

        internal PowerUpType RollPowerUpType()
        {
            // No extra lives beyond the cap
            var candidates = PowerUpWeights
                .Where(w => w.Type != PowerUpType.ExtraLife || _lives < MaxLives)
                .ToArray();
            var roll = _dropRng.Next(candidates.Sum(w => w.Weight));
            foreach (var (type, weight) in candidates)
            {
                if (roll < weight) return type;
                roll -= weight;
            }
            return candidates[^1].Type;
        }

        /// <summary>Drops a power-up at a position (for tests).</summary>
        internal void SpawnPowerUp(PowerUpType type, Point position) =>
            _powerUps.Add(new PowerUp(position, type));

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

        /// <summary>The bottom alien of the nearest column to <paramref name="column"/> that still has one.</summary>
        private Alien? LowestAlienNear(int column)
        {
            for (int distance = 0; distance <= 10; distance++)
            {
                foreach (var c in distance == 0 ? new[] { column } : new[] { column + distance, column - distance })
                {
                    var alien = _aliens.Where(a => a.Column == c && !a.Destroyed).MaxBy(a => a.CurrentPosition.Y);
                    if (alien != null) return alien;
                }
            }
            return null;
        }

        private void UpdateBombs()
        {
            // Drawn every step whether or not a bomb spawns, so the stream stays in step
            // across players of the same daily challenge
            var roll = _bombRng.Next(100);
            var column = _bombRng.Next(11);
            var type = (BombType)_bombRng.Next(3);

            // In frenzy the last alien always fires; otherwise use normal probability.
            // On a boss wave only the boss fires (see UpdateBoss).
            var shouldSpawn = !IsBossWave && !AliensHoldFire && (IsFrenzy
                ? !_bombs.Any(_ => !_.Remove)
                : (!_bombs.Any() || _bombs.All(_ => _.Remove)) &&
                  roll > (85 + (_aliens.Count(_ => _.Destroyed) / 5)));

            if (shouldSpawn && LowestAlienNear(column) is { } alien)
            {
                _bombs.Add(new Bomb(
                    new Point(alien.CurrentPosition.X + alien.Sprite.RenderSize.Width / 2, alien.CurrentPosition.Y),
                    type));
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
                    b.Remove = true;
                    if (_shield)
                    {
                        // The shield takes this hit instead of the ship
                        _shield = false;
                        _player.HasBeenHit = false;
                        _sounds |= SoundEffect.ShieldBlock;
                        Raise(GameEventType.ShieldBlocked, _player.CurrentPosition, _player.Sprite.RenderSize);
                        continue;
                    }

                    _hitThisWave = true;
                    _lives--;
                    LostALife = true;
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
                    Collect(p);
                }
            }
        }

        private void Collect(PowerUp p)
        {
            switch (p.Type)
            {
                case PowerUpType.Shield:
                    _shield = true;
                    break;
                case PowerUpType.ExtraLife:
                    _lives = Math.Min(_lives + 1, MaxLives);
                    break;
                default:
                    // Collecting again while active restarts the timer
                    _powerUpUntil[(int)p.Type] = Time + (p.Type == PowerUpType.Piercing ? 8000 : 10000);
                    break;
            }
            _sounds |= SoundEffect.PowerUp;
            Raise(GameEventType.PowerUpCollected, p.Position, PowerUp.Size, variant: (int)p.Type);
        }

        private void UpdateMotherShip()
        {
            // Drawn every step, like the bomb rolls
            var roll = _shipRng.Next(100);
            if (_motherShip == null)
            {
                if (!IsBossWave && roll > 90)
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;

namespace BlazorInvaders.GameObjects
{
    public class Game
    {
        List<Alien> _aliens;
        List<Shot> _shots;
        List<Bomb> _bombs;
        float _lastUpdate = 0.0F;
        Direction _currentMovement;
        Player _player;
        Canvas2DContext _context;
        readonly GameTime _gameTime = new GameTime();
        ElementReference _spriteSheet;
        int _width;
        int _height;
        int _points;
        int _lives;
        Random _random = new Random();

        public Game(int gameWidth, int gameHeight)
        {
            _width = gameWidth;
            _height = gameHeight;
        }

        public async ValueTask Init(BECanvasComponent canvas, ElementReference spriteSheet)
        {
            _context = await canvas.CreateCanvas2DAsync();
            _spriteSheet = spriteSheet;
        }

        public void Start(ElementReference spriteSheet, float time)
        {
            _aliens = new List<Alien>();
            _shots = new List<Shot>();
            _bombs = new List<Bomb>();
            for (int i = 0; i < 11; i++)
            {
                _aliens.Add(new Alien(AlienType.Squid, new Point((i * 60 + 120), 40), i, 0));
                _aliens.Add(new Alien(AlienType.Crab, new Point((i * 60 + 120), 85), i, 1));
                _aliens.Add(new Alien(AlienType.Crab, new Point((i * 60 + 120), 130), i, 2));
                _aliens.Add(new Alien(AlienType.Octopus, new Point((i * 60 + 120), 175), i, 3));
                _aliens.Add(new Alien(AlienType.Octopus, new Point((i * 60 + 120), 220), i, 4));
            }
            Started = true;
            Won = false;
            GameOver = false;
            _gameTime.TotalTime = time;
            _lastUpdate = time;
            _player = new Player(_width, _height);
            _points = 0;
            _lives = 3;
        }

        public void Update(float timeStamp)
        {
            if (!Started)
            {
                return;
            }
            _gameTime.TotalTime = timeStamp;
            if (_gameTime.TotalTime - _lastUpdate > 250)
            {
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
                _aliens.Where(_ => _.HasBeenHit).ToList().ForEach(_ => _.Destroyed = true);
            }

            foreach (var s in _shots.Where(_ => !_.Remove))
            {
                s.Move();
                var destroyed = _aliens.Where(_ => !_.Destroyed && !_.HasBeenHit).OrderByDescending(_ => _.CurrentPosition.Y).FirstOrDefault(_ => _.Collision(s));
                if (destroyed != null)
                {
                    s.Remove = true;
                    _points += 100;
                }
            }
            if (_aliens.All(_ => _.Destroyed))
            {
                Won = true;
            }
            if ((!_bombs.Any() || _bombs.All(_ => _.Remove)) && _random.Next(100) > 60)
            {
                var xColumn = _random.Next(0, 10);
                var alien = _aliens.OrderByDescending(_ => _.CurrentPosition.Y).FirstOrDefault(_ => _.Column == xColumn && !_.Destroyed);
                while (alien == null)
                {
                    xColumn = _random.Next(0, 10);
                    alien = _aliens.FirstOrDefault(_ => _.Column == xColumn && !_.Destroyed);
                }
                _bombs.Add(new Bomb(alien.CurrentPosition));
            }
            foreach (var b in _bombs)
            {
                b.Move();
                if (_player.Collision(b))
                {
                    _lives--;
                    GameOver = _lives < 1;
                }
            }
        }

        public async ValueTask Render()
        {
            await _context.ClearRectAsync(0, 0, _width, _height);
            await _context.BeginBatchAsync();
            await _context.SetFontAsync("32px consolas");
            if (GameOver)
            {
                var text = $"Game over, you lost!";
                var length = await _context.MeasureTextAsync(text);
                await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), 150);
                text = $"Press space to start a new game";
                length = await _context.MeasureTextAsync(text);
                await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), 200);
                Started = false;
            }
            else if (Won)
            {
                var text = $"Congratulations you won!";
                var length = await _context.MeasureTextAsync(text);
                await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), 150);
                text = $"Press space to start a new game";
                length = await _context.MeasureTextAsync(text);
                await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), 200);
                Started = false;
            }
            else if (Started)
            {
                await _context.SetFontAsync("32px consolas");
                await _context.SetFillStyleAsync("white");
                var text = $"Points: {_points.ToString("D3")}  Lives: {_lives.ToString("D3")}";
                var length = await _context.MeasureTextAsync(text);
                await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), 25);
                foreach (var a in Aliens)
                {
                    if (a.Destroyed)
                    {
                        continue;
                    }
                    await _context.DrawImageAsync(_spriteSheet,
                        a.Sprite.TopLeft.X, a.Sprite.TopLeft.Y, a.Sprite.Size.Width, a.Sprite.Size.Height, a.CurrentPosition.X, a.CurrentPosition.Y, a.Sprite.RenderSize.Width, a.Sprite.RenderSize.Height);
                }
                var ship = Player.Sprite;
                await _context.DrawImageAsync(_spriteSheet,
                    ship.TopLeft.X, ship.TopLeft.Y, 18, 12, Player.CurrentPosition.X, Player.CurrentPosition.Y, 36, 24); //TODO: Remove hardcoded sizes
                foreach (var s in _shots.Where(_ => !_.Remove))
                {
                    if (s.CurrentPosition.Y <= 0)
                    {
                        s.Remove = true;
                        continue;
                    }
                    await _context.DrawImageAsync(_spriteSheet,
                        s.Sprite.TopLeft.X, s.Sprite.TopLeft.Y, 20, 14, s.CurrentPosition.X, s.CurrentPosition.Y, 40, 28); //TODO: Remove hardcoded sizes
                }
                foreach (var b in _bombs.Where(_ => !_.Remove))
                {
                    if (b.CurrentPosition.Y >= _height)
                    {
                        b.Remove = true;
                        continue;
                    }
                    await _context.DrawImageAsync(_spriteSheet,
                        b.Sprite.TopLeft.X, b.Sprite.TopLeft.Y, 20, 14, b.CurrentPosition.X, b.CurrentPosition.Y, 40, 28); //TODO: Remove hardcoded sizes
                }
            }
            else if (!Started)
            {
                var text = $"Use arrow keys to move, space to fire";
                var length = await _context.MeasureTextAsync(text);
                await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), 150);
                text = $"Press space to start";
                length = await _context.MeasureTextAsync(text);
                await _context.FillTextAsync(text, _width / 2 - (length.Width / 2), 200);
            }

            await _context.EndBatchAsync();
        }

        public bool Started { get; private set; }

        public bool Won { get; private set; }
        public bool GameOver { get; private set; }

        public IEnumerable<Alien> Aliens => _aliens;

        public Player Player => _player;

        public void MovePlayer(Direction direction) => Player.Move(10, direction);

        public void Fire() =>
            _shots.Add(new Shot(new Point(_player.CurrentPosition.X + 13, Player.CurrentPosition.Y)));
    }
}
using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Player
    {
        Point _currentPosition;
        Sprite _ship;
        Sprite _explosion;
        int _gameWidth;

        public Player(int gameWidth, int gameHeight)
        {
            _gameWidth = gameWidth;
            _currentPosition = new Point(gameWidth / 2, gameHeight - 50);
            _explosion = new Sprite(0, 47, 16, 57);
            _ship = new Sprite(20, 47, 38, 59);
        }

        public bool HasBeenHit { get; set; }

        public Sprite Sprite => HasBeenHit ? _explosion : _ship;

        public Point CurrentPosition => _currentPosition;

        public void Move(int offsetX, Direction direction)
        {
            var x = direction == Direction.Left ? _currentPosition.X - offsetX : _currentPosition.X + offsetX;
            if (x <= 10 && direction == Direction.Left)
            {
                return;
            }
            if (x >= _gameWidth - 50 && direction == Direction.Right)
            {
                return;
            }
            _currentPosition = new Point(x, _currentPosition.Y);
        }

        public bool Collision(Bomb s)
        {
            HasBeenHit = new Rectangle(CurrentPosition, Sprite.RenderSize).IntersectsWith(new Rectangle(s.CurrentPosition, s.Sprite.RenderSize));
            return HasBeenHit;
        }
    }
}
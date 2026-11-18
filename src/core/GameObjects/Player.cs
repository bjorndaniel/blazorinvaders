using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    /// <summary>The player's ship, kept inside the game width.</summary>
    public class Player
    {
        Point _currentPosition;
        Sprite _ship;
        Sprite _explosion;
        int _gameWidth;

        /// <summary>Creates the ship at the horizontal centre, 50 px above the bottom of a game of the given size (px).</summary>
        public Player(int gameWidth, int gameHeight)
        {
            _gameWidth = gameWidth;
            _currentPosition = new Point(gameWidth / 2, gameHeight - 50);
            _explosion = new Sprite(0, 47, 16, 57);
            _ship = new Sprite(20, 47, 38, 59);
        }

        /// <summary>True after a collision, so the explosion is drawn.</summary>
        public bool HasBeenHit { get; set; }

        /// <summary>The frame to draw: the explosion once hit, otherwise the ship.</summary>
        public Sprite Sprite => HasBeenHit ? _explosion : _ship;

        /// <summary>Top-left corner in game px.</summary>
        public Point CurrentPosition => _currentPosition;

        /// <summary>Moves <paramref name="offsetX"/> px in <paramref name="direction"/>; does nothing if that would pass 10 px from the left edge or 50 px from the right.</summary>
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

        /// <summary>Tests the ship against a bomb, sets <see cref="HasBeenHit"/> to the result and returns it.</summary>
        public bool Collision(Bomb s)
        {
            HasBeenHit = new Rectangle(CurrentPosition, Sprite.RenderSize).IntersectsWith(new Rectangle(s.CurrentPosition, s.Sprite.RenderSize));
            return HasBeenHit;
        }

        /// <summary>Tests the ship against an alien, sets <see cref="HasBeenHit"/> to the result and returns it.</summary>
        public bool Collision(Alien a)
        {
            HasBeenHit = new Rectangle(CurrentPosition, Sprite.RenderSize).IntersectsWith(new Rectangle(a.CurrentPosition, a.Sprite.RenderSize));
            return HasBeenHit;
        }
    }
}
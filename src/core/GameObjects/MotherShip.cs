using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    /// <summary>The bonus ship that crosses the top of the screen from right to left.</summary>
    public class MotherShip
    {
        Sprite _ship;
        Point _currentPosition;
        Sprite _explosion;

        /// <summary>Creates a mothership with its top-left corner at <paramref name="start"/> (px).</summary>
        public MotherShip(Point start)
        {
            _ship = new Sprite(12, 73, 24, 80);
            _currentPosition = start;
            _explosion = new Sprite(0, 59, 17, 69);

        }

        /// <summary>The frame to draw: the explosion once hit, otherwise the ship.</summary>
        public Sprite Sprite => HasBeenHit ? _explosion : _ship;

        /// <summary>Set when the ship should be dropped from the game.</summary>
        public bool Remove { get; set; }

        /// <summary>Top-left corner in game px.</summary>
        public Point CurrentPosition => _currentPosition;

        /// <summary>Steps 7 px to the left.</summary>
        public void Move() =>
            _currentPosition = new Point(_currentPosition.X - 7, _currentPosition.Y);

        /// <summary>True once a shot has hit it; it then shows the explosion.</summary>
        public bool HasBeenHit { get; set; }

        /// <summary>Points it was worth when hit; 0 until then.</summary>
        public int Score { get; set; }

        /// <summary>True once the explosion has been shown and the ship is gone for good.</summary>
        public bool Destroyed { get; set; }

        /// <summary>Tests the ship against a shot, sets <see cref="HasBeenHit"/> to the result and returns it.</summary>
        public bool Collision(Shot s)
        {
            HasBeenHit = new Rectangle(CurrentPosition, Sprite.RenderSize).IntersectsWith(new Rectangle(s.CurrentPosition, s.Sprite.RenderSize));
            return HasBeenHit;
        }
    }
}

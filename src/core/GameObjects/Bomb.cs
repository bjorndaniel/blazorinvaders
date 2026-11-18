using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    /// <summary>A bomb dropped by an alien or the boss; falls 10 px per step.</summary>
    public class Bomb
    {
        private readonly Sprite _bomb;
        private Point _currentPosition;
        private float _phase;
        private readonly BombType _type;
        private readonly int _dx;

        /// <summary>Creates a bomb with its top-left corner at <paramref name="start"/> (px).</summary>
        /// <param name="start">Where the bomb appears.</param>
        /// <param name="type">How the bomb drifts sideways as it falls.</param>
        /// <param name="dx">Constant sideways speed in px per step, for angled shots.</param>
        public Bomb(Point start, BombType type = BombType.Straight, int dx = 0)
        {
            _bomb = new Sprite(0, 69, 5, 80);
            _currentPosition = start;
            _type = type;
            _dx = dx;
        }

        /// <summary>The sprite to draw.</summary>
        public Sprite Sprite => _bomb;
        /// <summary>Set when the bomb should be dropped from the game.</summary>
        public bool Remove { get; set; }
        /// <summary>Top-left corner in game px.</summary>
        public Point CurrentPosition => _currentPosition;

        /// <summary>Advances one step: 10 px down, plus the sideways drift of its type and the constant dx.</summary>
        public void Move()
        {
            _phase += 0.4f;
            var xOffset = _type switch
            {
                BombType.Zigzag => (int)(Math.Sin(_phase) * 8),
                BombType.Rolling => (int)(Math.Cos(_phase * 0.7f) * 5),
                _ => 0
            };
            _currentPosition = new Point(_currentPosition.X + xOffset + _dx, _currentPosition.Y + 10);
        }
    }
}

using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Bomb
    {
        private readonly Sprite _bomb;
        private Point _currentPosition;
        private float _phase;
        private readonly BombType _type;
        private readonly int _dx;

        /// <param name="dx">Constant sideways speed in px per step, for angled shots.</param>
        public Bomb(Point start, BombType type = BombType.Straight, int dx = 0)
        {
            _bomb = new Sprite(0, 69, 5, 80);
            _currentPosition = start;
            _type = type;
            _dx = dx;
        }

        public Sprite Sprite => _bomb;
        public bool Remove { get; set; }
        public Point CurrentPosition => _currentPosition;

        public void Move()
        {
            _phase += 0.4f;
            var xOffset = _type switch
            {
                BombType.Zigzag  => (int)(Math.Sin(_phase) * 8),
                BombType.Rolling => (int)(Math.Cos(_phase * 0.7f) * 5),
                _                => 0
            };
            _currentPosition = new Point(_currentPosition.X + xOffset + _dx, _currentPosition.Y + 10);
        }
    }
}

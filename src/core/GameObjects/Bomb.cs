using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Bomb
    {
        private readonly Sprite _bomb;
        private Point _currentPosition;
        private float _phase;
        private readonly BombType _type;

        public Bomb(Point start, BombType type = BombType.Straight)
        {
            _bomb = new Sprite(0, 69, 5, 80);
            _currentPosition = start;
            _type = type;
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
            _currentPosition = new Point(_currentPosition.X + xOffset, _currentPosition.Y + 10);
        }
    }
}

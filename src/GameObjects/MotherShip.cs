using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class MotherShip
    {
        Sprite _ship; //TODO: Get movement sprite
        Point _currentPosition;
        Sprite _explosion;

        public MotherShip(Point start)
        {
            _ship = new Sprite(12, 73, 24, 80);
            _currentPosition = start;
            _explosion = new Sprite(0, 59, 17, 69);

        }

        public Sprite Sprite => HasBeenHit ? _explosion : _ship;

        public bool Remove { get; set; }

        public Point CurrentPosition => _currentPosition;

        public void Move() =>
            _currentPosition = new Point(_currentPosition.X - 7, _currentPosition.Y);

        public bool HasBeenHit { get; set; }

        public bool Destroyed { get; set; }

        public bool Collision(Shot s)
        {
            HasBeenHit = new Rectangle(CurrentPosition, Sprite.RenderSize).IntersectsWith(new Rectangle(s.CurrentPosition, s.Sprite.RenderSize));
            return HasBeenHit;
        }
    }
}

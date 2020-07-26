using System.Drawing;

namespace BlazorInvaders.GameObjects
{
    public class Shot
    {
        Sprite _beam;//TODO: Get movement sprite
        Point _currentPosition;
        public Shot(Point start)
        {
            _beam = new Sprite(20, 60, 21, 61);
            _currentPosition = start;
        }
        public Sprite Sprite => _beam;
        public bool Remove { get; set; }
        public Point CurrentPosition => _currentPosition;
        public void Move() =>
            _currentPosition = new Point(_currentPosition.X, _currentPosition.Y - 20);
    }
}
